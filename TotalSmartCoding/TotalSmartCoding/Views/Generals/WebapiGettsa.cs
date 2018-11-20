using System;
using System.Net.Http;
using System.Windows.Forms;

using TotalSmartCoding.Libraries;
using TotalSmartCoding.Controllers.Generals;


namespace TotalSmartCoding.Views.Generals
{
    public partial class WebapiGettsa : Form
    {
        private string Q_id1 { get; set; }
        public WebapiGettsa(string q_id1)
        {
            InitializeComponent();

            this.Q_id1 = q_id1;
        }

        private void Webapi_Load(object sender, EventArgs e)
        {
            TsaBarcode tsaBarcode = new TsaBarcode();
            tsaBarcode.Q_id1 = this.Q_id1;

            HttpResponseMessage httpResponseMessage = HttpOAuth.TsaBarcodeRead(tsaBarcode);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                //this.textexLabel.Text = tsaBarcode.TsaLabel.attributes.label[0].value;
                this.textexProduction_date.Text = tsaBarcode.TsaLabel.attributes.production_date[0].value;
                this.textexProduction_line.Text = tsaBarcode.TsaLabel.attributes.production_line[0].value;
                this.textexProduct_id.Text = tsaBarcode.TsaLabel.attributes.SKU_code[0].value;
                this.textexBatch_number.Text = tsaBarcode.TsaLabel.attributes.batch_number[0].value;
                //this.textexBatch_serial.Text = tsaBarcode.TsaLabel.attributes.batch_serial[0].value;
                this.textexDomino_code.Text = tsaBarcode.TsaLabel.attributes.production_serial_number[0].value;
                //this.textexValid.Text = tsaBarcode.TsaLabel.attributes.valid[0].value;
            }
            else
                this.labelApplicationRoleName.Text = "Fail to read data from tesa server." + "\r\n" + "\r\n" + httpResponseMessage.StatusCode.ToString() + " " + httpResponseMessage.ReasonPhrase;
        }

        private void button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
