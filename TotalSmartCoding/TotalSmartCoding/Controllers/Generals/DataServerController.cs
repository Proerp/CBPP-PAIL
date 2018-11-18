using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using TotalBase;


namespace TotalSmartCoding.Controllers.Generals
{
    public class DataServerController
    {
        public void Upload()
        {
            TsaBarcode tsaBarcode = new TsaBarcode();

            tsaBarcode.ConsumerKey = "ST27FPpHyqCK942bcfMY8aRB8uS7MpVAaBGj5nZTXefT32557cmb";
            tsaBarcode.ConsumerSecret = "GvmFcdt7bfQSqRPdTCytcUN2bfmrHZSK";
            tsaBarcode.Q_id1 = "C4STR0L001";

            tsaBarcode.TsaLabel = new TsaLabel();

            HttpStatusCode httpStatusCode = HttpOAuth.TsaUpdate(tsaBarcode);

            //if (httpStatusCode == HttpStatusCode.OK)
            //{
            //    var a = await httpStatusCode.Content.ReadAsStringAsync();
            //    Console.WriteLine("\r\n" + "\r\n" + "\r\n" + "TSA: " + a);
            //}

        }

        //public void UpLoadDataMaster()
        //{
        //    try
        //    {
        //        this.UpLoadLogEventChange = false;

        //        int rowEffected = 0; string exceptionMessage = ""; int fileTimes = 0;

        //        #region UpLoadData

        //        DataDetail.UpLoadLogEventDataTable notIsSuccessfullSentDataTabe = this.UpLoadLogEventTableAdapter.GetDataByIsSuccessful(false);
        //        if (notIsSuccessfullSentDataTabe.Rows.Count > 0)
        //        {
        //            foreach (DataDetail.UpLoadLogEventRow notIsSuccessfullSentRow in notIsSuccessfullSentDataTabe)
        //            {
        //                FtpStatusCode ftpStatusCode = this.UpLoadTextFile(notIsSuccessfullSentRow.FileName, out exceptionMessage);
        //                notIsSuccessfullSentRow.IsSuccessful = ftpStatusCode == FtpStatusCode.ClosingData && exceptionMessage == "226 Transfer complete.\r\n" ? true : false;
        //                notIsSuccessfullSentRow.Remarks = notIsSuccessfullSentRow.Description;
        //                notIsSuccessfullSentRow.Description = exceptionMessage;
        //            }
        //            this.UpLoadLogEventTableAdapter.Update(notIsSuccessfullSentDataTabe);
        //            this.UpLoadLogEventChange = true;
        //        }
        //        #endregion UpLoadData

        //    }
        //    catch (System.Exception exception)
        //    {
        //        throw exception;
        //    }
        //}
















    }






    #region HttpOAuth

    public static class HttpOAuth
    {
        static HttpClient client = new HttpClient();


        public static HttpStatusCode TsaUpdate(TsaBarcode tsaBarcode)
        {
            try
            {
                return RunAsync(tsaBarcode).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private static async Task<HttpStatusCode> RunAsync(TsaBarcode tsaBarcode)
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return await PutAsync(tsaBarcode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        private static async Task<HttpStatusCode> PutAsync(TsaBarcode tsaBarcode)
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

            return httpResponseMessage.StatusCode;
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

            this.attributes.production_serial_number = new List<ProductionSerialNumber>() { new ProductionSerialNumber() { value = "CBPP02-001-8888" } };
            this.attributes.production_line = new List<ProductionLine>() { new ProductionLine() { value = "PAIL181118" } };
            this.attributes.production_date = new List<ProductionDate>() { new ProductionDate() { value = "18112018" } };
            this.attributes.batch_number = new List<BatchNumber>() { new BatchNumber() { value = "FT03" } };
            this.attributes.SKU_code = new List<SKUCode>() { new SKUCode() { value = "ITEM03" } };
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
    }

    #endregion

}
