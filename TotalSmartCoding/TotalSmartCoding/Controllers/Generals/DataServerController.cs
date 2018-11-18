﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using TotalBase;
using TotalCore.Services.Productions;
using TotalModel.Helpers;
using TotalModel.Models;


namespace TotalSmartCoding.Controllers.Generals
{
    public class DataServerController : NotifyPropertyChangeObject
    {
        public ICartonService cartonService;

        public DataServerController(ICartonService cartonService)
        {
            this.cartonService = cartonService;
        }


        public void Upload()
        {
            try
            {
                TsaBarcode tsaBarcode = new TsaBarcode();

                tsaBarcode.ConsumerKey = "ST27FPpHyqCK942bcfMY8aRB8uS7MpVAaBGj5nZTXefT32557cmb";
                tsaBarcode.ConsumerSecret = "GvmFcdt7bfQSqRPdTCytcUN2bfmrHZSK";

                IList<CartonAttribute> cartonAttributes = this.cartonService.GetCartonAttributes(GlobalVariables.FillingLineID, (int)GlobalVariables.SubmitStatus.Freshnew + "," + (int)GlobalVariables.SubmitStatus.Failed, null);

                int i = 1;
                foreach (CartonAttribute cartonAttribute in cartonAttributes)
                {

                    tsaBarcode.Q_id1 = "C4STR0L" + (i++).ToString("000"); if (i >= 10) i = 0;
                    //tsaBarcode.Q_id1 = cartonAttribute.Label.Substring(cartonAttribute.Label.Length - 10, 10);// "C4STR0L001";                    

                    tsaBarcode.TsaLabel.attributes.SKU_code = new List<SKUCode>() { new SKUCode() { value = cartonAttribute.OfficialCode } };
                    tsaBarcode.TsaLabel.attributes.batch_number = new List<BatchNumber>() { new BatchNumber() { value = cartonAttribute.BatchCode } };
                    tsaBarcode.TsaLabel.attributes.production_line = new List<ProductionLine>() { new ProductionLine() { value = cartonAttribute.FillingLineName } };
                    tsaBarcode.TsaLabel.attributes.production_date = new List<ProductionDate>() { new ProductionDate() { value = cartonAttribute.BatchEntryDate.ToString("yyyyMMdd") } };
                    tsaBarcode.TsaLabel.attributes.production_serial_number = new List<ProductionSerialNumber>() { new ProductionSerialNumber() { value = cartonAttribute.Code } };

                    HttpResponseMessage httpResponseMessage = HttpOAuth.TsaUpdate(tsaBarcode);

                    this.cartonService.UpdateSubmitStatus("" + cartonAttribute.CartonID, httpResponseMessage.IsSuccessStatusCode ? GlobalVariables.SubmitStatus.Created : GlobalVariables.SubmitStatus.Failed, "[" + (int)httpResponseMessage.StatusCode + "] " + httpResponseMessage.StatusCode.ToString() + " " + httpResponseMessage.ReasonPhrase);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }






    #region HttpOAuth

    public static class HttpOAuth
    {
        static HttpClient client = new HttpClient();


        public static HttpResponseMessage TsaUpdate(TsaBarcode tsaBarcode)
        {
            try
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Error!" };


                return RunAsync(tsaBarcode).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = e.Message };
            }
        }

        private static async Task<HttpResponseMessage> RunAsync(TsaBarcode tsaBarcode)
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return await PutAsync(tsaBarcode);
            }
            catch (Exception e)
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = e.Message };
            }
        }


        private static async Task<HttpResponseMessage> PutAsync(TsaBarcode tsaBarcode)
        {
            List<string> parameters = new List<string>() { "q_id1=" + tsaBarcode.Q_id1 };
            OAuth_CSharp oauth_CSharp = new OAuth_CSharp(tsaBarcode.ConsumerKey, tsaBarcode.ConsumerSecret);
            string requestURL = oauth_CSharp.GenerateRequestURL(tsaBarcode.Url, "PUT", parameters);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, requestURL);
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

    public class Attributes
    {
        public List<ProductionSerialNumber> production_serial_number { get; set; }
        public List<ProductionLine> production_line { get; set; }
        public List<ProductionDate> production_date { get; set; }
        public List<BatchNumber> batch_number { get; set; }
        public List<SKUCode> SKU_code { get; set; }
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
        public string Url { get { return "https://tcnc.eu/3PQ6/api/v3/label/"; } set { } }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }


        public int TsaBarcodeID { get; set; }

        public string Q_id1 { get; set; }

        public TsaLabel TsaLabel { get; set; }

        public TsaBarcode()
        {
            this.TsaLabel = new TsaLabel();
        }
    }

    #endregion

}
