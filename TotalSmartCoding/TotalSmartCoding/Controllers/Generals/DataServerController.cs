﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using Ninject;

using TotalBase;
using TotalModel.Helpers;
using TotalModel.Models;
using TotalCore.Services.Productions;
using TotalSmartCoding.Libraries;
using TotalSmartCoding.Controllers.Productions;


namespace TotalSmartCoding.Controllers.Generals
{
    public class DataServerController : CodingController
    {
        private string Q_id1;

        public ICartonService cartonService;

        public bool OnUploading { get; private set; }

        public void StartUpload() { this.OnUploading = true; }
        public void StopUpload() { this.OnUploading = false; }

        public DataServerController()
            : this(null)
        { }
        public DataServerController(string q_id1)
        {
            this.Q_id1 = q_id1;
            this.cartonService = CommonNinject.Kernel.Get<ICartonService>();
        }


        public void ThreadRoutine()
        {
            this.LoopRoutine = true; this.StartUpload();

            try
            {
                TsaBarcode tsaBarcode = new TsaBarcode();

                while (this.LoopRoutine)
                {
                    if (this.OnUploading)
                    {
                        HttpOAuth httpOAuth = new HttpOAuth();

                        this.MainStatus = "Starting ..."; Thread.Sleep(500);

                        IList<CartonAttribute> cartonAttributes = this.cartonService.GetCartonAttributes(GlobalVariables.FillingLineID, (int)GlobalVariables.SubmitStatus.Freshnew + "," + (int)GlobalVariables.SubmitStatus.Failed, null);

                        foreach (CartonAttribute cartonAttribute in cartonAttributes)
                        {
                            //Random random = new Random();
                            //tsaBarcode.Q_id1 = "C4STR0L" + random.Next(1, 10).ToString("000");
                            tsaBarcode.Q_id1 = cartonAttribute.Label;


                            //tsaBarcode.TsaLabel.attributes.label = new List<Label>() { new Label() { value = cartonAttribute.Label } };

                            tsaBarcode.TsaLabel.attributes.SKU_code = new List<SKUCode>() { new SKUCode() { value = cartonAttribute.OfficialCode } };
                            tsaBarcode.TsaLabel.attributes.batch_number = new List<BatchNumber>() { new BatchNumber() { value = cartonAttribute.BatchCode } };
                            tsaBarcode.TsaLabel.attributes.production_line = new List<ProductionLine>() { new ProductionLine() { value = cartonAttribute.FillingLineName } };
                            tsaBarcode.TsaLabel.attributes.production_date = new List<ProductionDate>() { new ProductionDate() { value = cartonAttribute.BatchEntryDate.ToString("yyyy-MM-dd") } };
                            tsaBarcode.TsaLabel.attributes.production_serial_number = new List<ProductionSerialNumber>() { new ProductionSerialNumber() { value = cartonAttribute.Code.Substring(0, cartonAttribute.Code.Length - 6).Trim() } };

                            //tsaBarcode.TsaLabel.attributes.batch_serial = new List<BatchSerial>() { new BatchSerial() { value = cartonAttribute.Code.Substring(cartonAttribute.Code.Length - 6, 6).Trim() } };
                            //tsaBarcode.TsaLabel.attributes.valid = new List<Valid>() { new Valid() { value = "1" } };


                            this.MainStatus = "Sending: " + tsaBarcode.TsaLabel.attributes.production_serial_number[0].value;
                            HttpResponseMessage httpResponseMessage = httpOAuth.TsaBarcodeUpdate(tsaBarcode);
                            this.MainStatus = httpResponseMessage.StatusCode.ToString() + " " + httpResponseMessage.ReasonPhrase + ": " + tsaBarcode.TsaLabel.attributes.production_serial_number[0].value;

                            this.cartonService.UpdateSubmitStatus("" + cartonAttribute.CartonID, httpResponseMessage.IsSuccessStatusCode ? GlobalVariables.SubmitStatus.Created : GlobalVariables.SubmitStatus.Failed, "[" + (int)httpResponseMessage.StatusCode + "] " + httpResponseMessage.StatusCode.ToString() + " " + httpResponseMessage.ReasonPhrase);

                            if (!this.OnUploading) break;
                        }
                    }
                    this.MainStatus = "Idling";
                    this.StopUpload();
                    Thread.Sleep(100);
                }
            }
            catch (Exception exception)
            {
                this.LoopRoutine = false;
                this.MainStatus = exception.Message;

                this.setLED(this.LedGreenOn, this.LedAmberOn, true);
            }
            finally
            {

            }
        }

        public void ThreadGet()
        {
            try
            {
                TsaBarcode tsaBarcode = new TsaBarcode();


                this.MainStatus = "Connecting ..."; Thread.Sleep(500);


                tsaBarcode.Q_id1 = this.Q_id1;

                HttpOAuth httpOAuth = new HttpOAuth();
                HttpResponseMessage httpResponseMessage = httpOAuth.TsaBarcodeRead(tsaBarcode);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    //this.MainStatus = "label@" + tsaBarcode.TsaLabel.attributes.label[0].value;
                    this.MainStatus = "production_date@" + tsaBarcode.TsaLabel.attributes.production_date[0].value;
                    this.MainStatus = "production_line@" + tsaBarcode.TsaLabel.attributes.production_line[0].value;
                    this.MainStatus = "SKU_code@" + tsaBarcode.TsaLabel.attributes.SKU_code[0].value;
                    this.MainStatus = "batch_number@" + tsaBarcode.TsaLabel.attributes.batch_number[0].value;
                    //this.MainStatus = "batch_serial@" + tsaBarcode.TsaLabel.attributes.batch_serial[0].value;
                    this.MainStatus = "production_serial_number@" + tsaBarcode.TsaLabel.attributes.production_serial_number[0].value;
                    //this.MainStatus = "valid@" + tsaBarcode.TsaLabel.attributes.valid[0].value;
                }
                else
                    this.MainStatus = "Fail to read data from tesa server." + "\r\n" + "\r\n" + httpResponseMessage.StatusCode.ToString() + " " + httpResponseMessage.ReasonPhrase;

            }
            catch (Exception exception)
            {
                this.MainStatus = exception.Message;
            }
        }

    }






    #region HttpOAuth

    public class HttpOAuth
    {
        private HttpClient client = new HttpClient();

        public HttpResponseMessage TsaBarcodeRead(TsaBarcode tsaBarcode)
        {
            try
            {
                //Thread.Sleep(168);
                //return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Fail!" };

                return GetAsync(tsaBarcode).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = e.Message };
            }
        }

        public HttpResponseMessage TsaBarcodeUpdate(TsaBarcode tsaBarcode)
        {
            try
            {
                //Thread.Sleep(168);
                //return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Fail!" };


                return PutAsync(tsaBarcode).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = e.Message };
            }
        }


        private async Task<HttpResponseMessage> GetAsync(TsaBarcode tsaBarcode)
        {
            InitClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, GetRequestURL("GET", tsaBarcode));


            HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var responseData = JObject.Parse(await httpResponseMessage.Content.ReadAsStringAsync());

                //tsaBarcode.TsaLabel.attributes.label = new List<Label>() { new Label() { value = cartonAttribute.Label } };

                tsaBarcode.TsaLabel.attributes.SKU_code = new List<SKUCode>() { new SKUCode() { value = responseData["labels"][0]["translations"]["SKU_code"][1]["msg"].ToString() } };
                tsaBarcode.TsaLabel.attributes.batch_number = new List<BatchNumber>() { new BatchNumber() { value = responseData["labels"][0]["translations"]["batch_number"][1]["msg"].ToString() } };
                tsaBarcode.TsaLabel.attributes.production_line = new List<ProductionLine>() { new ProductionLine() { value = responseData["labels"][0]["translations"]["production_line"][1]["msg"].ToString() } };
                tsaBarcode.TsaLabel.attributes.production_date = new List<ProductionDate>() { new ProductionDate() { value =  responseData["labels"][0]["translations"]["production_date"][1]["msg"].ToString() } };
                tsaBarcode.TsaLabel.attributes.production_serial_number = new List<ProductionSerialNumber>() { new ProductionSerialNumber() { value = responseData["labels"][0]["translations"]["production_serial_number"][1]["msg"].ToString() } };

                //tsaBarcode.TsaLabel.attributes.batch_serial = new List<BatchSerial>() { new BatchSerial() { value = cartonAttribute.Code.Substring(cartonAttribute.Code.Length - 6, 6).Trim() } };
                //tsaBarcode.TsaLabel.attributes.valid = new List<Valid>() { new Valid() { value = "1" } };
            }

            return httpResponseMessage;
        }


        private async Task<HttpResponseMessage> PutAsync(TsaBarcode tsaBarcode)
        {
            InitClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, GetRequestURL("PUT", tsaBarcode));
            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(tsaBarcode.TsaLabel), Encoding.UTF8, "application/json");


            HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                //var a = await httpResponseMessage.Content.ReadAsStringAsync();
                //Console.WriteLine("\r\n" + "\r\n" + "\r\n" + "TSA: " + a);
            }

            return httpResponseMessage;
        }



        private void InitClient()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string GetRequestURL(string HTTP_Method, TsaBarcode tsaBarcode)
        {
            List<string> parameters = new List<string>() { "q_id1=" + tsaBarcode.Q_id1 };
            OAuth_CSharp oauth_CSharp = new OAuth_CSharp(tsaBarcode.ConsumerKey, tsaBarcode.ConsumerSecret);
            return oauth_CSharp.GenerateRequestURL(tsaBarcode.BaseUri, HTTP_Method, parameters);
        }
    }
    #endregion



    #region TSA LABEL MODEL
    public class ProductionSerialNumber
    {
        public string value { get; set; }
    }

    public class ProductionLine
    {
        public string value { get; set; }
    }

    public class ProductionDate
    {
        public string value { get; set; }
    }

    public class BatchNumber
    {
        public string value { get; set; }
    }

    public class SKUCode
    {
        public string value { get; set; }
    }

    public class Label
    {
        public string value { get; set; }
    }

    public class BatchSerial
    {
        public string value { get; set; }
    }

    public class Valid
    {
        public string value { get; set; }
    }

    public class Attributes
    {
        //public List<Label> label { get; set; }

        public List<SKUCode> SKU_code { get; set; } //WILL BE RENAME: product_id        
        public List<BatchNumber> batch_number { get; set; }
        public List<ProductionLine> production_line { get; set; }
        public List<ProductionDate> production_date { get; set; }
        public List<ProductionSerialNumber> production_serial_number { get; set; } //WILL BE RENAME: domino_code

        //public List<BatchSerial> batch_serial { get; set; }
        //public List<Valid> valid { get; set; }
    }



    public class TsaLabel
    {
        public Attributes attributes { get; set; }

        public TsaLabel()
        {
            this.attributes = new Attributes();


        }
    }

    public class TsaBarcode
    {
        public string BaseUri { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }


        public int TsaBarcodeID { get; set; }

        public string Q_id1 { get; set; }

        public TsaLabel TsaLabel { get; set; }

        public TsaBarcode()
        {
            this.TsaLabel = new TsaLabel();

            this.BaseUri = Webapis.BaseUri; //"https://tcnc.eu/3PQ6/api/v3/label/";
            this.ConsumerKey = Webapis.ConsumerKey; //"ST27FPpHyqCK942bcfMY8aRB8uS7MpVAaBGj5nZTXefT32557cmb";
            this.ConsumerSecret = Webapis.ConsumerSecret; //"GvmFcdt7bfQSqRPdTCytcUN2bfmrHZSK";
        }
    }

    #endregion

}