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
        public ICartonService cartonService;

        public bool OnUploading { get; private set; }

        public void StartUpload() { this.OnUploading = true; }
        public void StopUpload() { this.OnUploading = false; }

        public DataServerController()
        {
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
                            HttpResponseMessage httpResponseMessage = HttpOAuth.TsaBarcodeUpdate(tsaBarcode);
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
    }






    #region HttpOAuth

    public static class HttpOAuth
    {
        static HttpClient client = new HttpClient();

        public static HttpResponseMessage TsaBarcodeRead(TsaBarcode tsaBarcode)
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

        public static HttpResponseMessage TsaBarcodeUpdate(TsaBarcode tsaBarcode)
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


        private static async Task<HttpResponseMessage> GetAsync(TsaBarcode tsaBarcode)
        {
            InitClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, GetRequestURL("GET", tsaBarcode));


            HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var responseData = JObject.Parse(await httpResponseMessage.Content.ReadAsStringAsync());

                string SKU_code = responseData["labels"][0]["translations"]["SKU_code"][1]["msg"].ToString();
                string batch_number = responseData["labels"][0]["translations"]["batch_number"][1]["msg"].ToString();
                string production_line = responseData["labels"][0]["translations"]["production_line"][1]["msg"].ToString();
                string production_date = responseData["labels"][0]["translations"]["production_date"][1]["msg"].ToString();
                string production_serial_number = responseData["labels"][0]["translations"]["production_serial_number"][1]["msg"].ToString();


                var a = await httpResponseMessage.Content.ReadAsStringAsync();
                Console.WriteLine("\r\n" + "\r\n" + "\r\n" + "TSA: " + a);
            }

            return httpResponseMessage;
        }


        private static async Task<HttpResponseMessage> PutAsync(TsaBarcode tsaBarcode)
        {
            InitClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, GetRequestURL("PUT", tsaBarcode));
            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(tsaBarcode.TsaLabel), Encoding.UTF8, "application/json");


            HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var a = await httpResponseMessage.Content.ReadAsStringAsync();
                Console.WriteLine("\r\n" + "\r\n" + "\r\n" + "TSA: " + a);
            }

            return httpResponseMessage;
        }



        private static void InitClient()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static string GetRequestURL(string HTTP_Method, TsaBarcode tsaBarcode)
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







//#region HttpOAuth

//public static class HttpOAuth
//{
//    static HttpClient client = new HttpClient();

//    public static HttpResponseMessage TsaBarcodeRead(TsaBarcode tsaBarcode)
//    {
//        try
//        {
//            Thread.Sleep(168);
//            return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Fail!" };


//            //return RunAsync(tsaBarcode).GetAwaiter().GetResult();
//        }
//        catch (Exception e)
//        {
//            return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = e.Message };
//        }
//    }

//    public static HttpResponseMessage TsaBarcodeUpdate(TsaBarcode tsaBarcode)
//    {
//        try
//        {
//            Thread.Sleep(168);
//            return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Fail!" };


//            return RunAsync(tsaBarcode).GetAwaiter().GetResult();
//        }
//        catch (Exception e)
//        {
//            return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = e.Message };
//        }
//    }

//    private static async Task<HttpResponseMessage> RunAsync(TsaBarcode tsaBarcode)
//    {
//        try
//        {
//            client.DefaultRequestHeaders.Accept.Clear();
//            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

//            return await PutAsync(tsaBarcode);
//        }
//        catch (Exception e)
//        {
//            return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = e.Message };
//        }
//    }


//    private static async Task<HttpResponseMessage> PutAsync(TsaBarcode tsaBarcode)
//    {
//        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, GetRequestURL("PUT", tsaBarcode));
//        httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(tsaBarcode.TsaLabel), Encoding.UTF8, "application/json");


//        HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
//        httpResponseMessage.EnsureSuccessStatusCode();
//        if (httpResponseMessage.IsSuccessStatusCode)
//        {
//            var a = await httpResponseMessage.Content.ReadAsStringAsync();
//            Console.WriteLine("\r\n" + "\r\n" + "\r\n" + "TSA: " + a);
//        }

//        return httpResponseMessage;
//    }

//    private static string GetRequestURL(string HTTP_Method, TsaBarcode tsaBarcode)
//    {
//        List<string> parameters = new List<string>() { "q_id1=" + tsaBarcode.Q_id1 };
//        OAuth_CSharp oauth_CSharp = new OAuth_CSharp(tsaBarcode.ConsumerKey, tsaBarcode.ConsumerSecret);
//        return oauth_CSharp.GenerateRequestURL(tsaBarcode.BaseUri, HTTP_Method, parameters);
//    }
//}
//#endregion