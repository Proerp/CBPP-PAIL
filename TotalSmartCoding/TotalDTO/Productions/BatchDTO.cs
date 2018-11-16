﻿using System;

using TotalBase;
using TotalModel;
using TotalBase.Enums;
using System.ComponentModel;
using System.Collections.Generic;
using TotalModel.Helpers;
using System.ComponentModel.DataAnnotations;

namespace TotalDTO.Productions
{
    public class BatchPrimitiveDTO : BaseDTO, IPrimitiveEntity, IPrimitiveDTO
    {
        public override GlobalEnums.NmvnTaskID NMVNTaskID { get { return GlobalEnums.NmvnTaskID.Batch; } }
        public override bool NoVoidable { get { return false; } }

        public override int GetID() { return this.BatchID; }
        public virtual void SetID(int id) { this.BatchID = id; }


        private int batchID;
        [DefaultValue(0)]
        public int BatchID
        {
            get { return this.batchID; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, int>(ref this.batchID, o => o.BatchID, value); }
        }

        private string code;
        [DefaultValue(null)]
        public string Code
        {
            get { return this.code; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, string>(ref this.code, o => o.Code, value); }
        }


        public int FillingLineID { get; set; }


        private int commodityID;
        [DefaultValue(null)]
        public int CommodityID
        {
            get { return this.commodityID; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, int>(ref this.commodityID, o => o.CommodityID, value); }
        }


        private int entryMonthID;
        public int EntryMonthID
        {
            get { return this.entryMonthID; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, int>(ref this.entryMonthID, o => o.EntryMonthID, value); }
        }


        private string nextPackNo;
        [DefaultValue("000001")]
        public string NextPackNo
        {
            get { return this.nextPackNo; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, string>(ref this.nextPackNo, o => o.NextPackNo, value); }
        }

        private string batchPackNo;
        [DefaultValue("000001")]
        public string BatchPackNo
        {
            get { return this.batchPackNo; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, string>(ref this.batchPackNo, o => o.BatchPackNo, value); }
        }

        private string nextCartonNo;
        [DefaultValue("000001")]
        public string NextCartonNo
        {
            get { return this.nextCartonNo; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, string>(ref this.nextCartonNo, o => o.NextCartonNo, value); }
        }

        private string batchCartonNo;
        [DefaultValue("000001")]
        public string BatchCartonNo
        {
            get { return this.batchCartonNo; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, string>(ref this.batchCartonNo, o => o.BatchCartonNo, value); }
        }

        private string nextPalletNo;
        [DefaultValue("000001")]
        public string NextPalletNo
        {
            get { return this.nextPalletNo; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, string>(ref this.nextPalletNo, o => o.NextPalletNo, value); }
        }

        private string batchPalletNo;
        [DefaultValue("000001")]
        public string BatchPalletNo
        {
            get { return this.batchPalletNo; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, string>(ref this.batchPalletNo, o => o.BatchPalletNo, value); }
        }

        private string finalCartonNo;
        [DefaultValue("000001")]
        public string FinalCartonNo
        {
            get { return this.finalCartonNo; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, string>(ref this.finalCartonNo, o => o.FinalCartonNo, value); }
        }

        private bool autoBarcode;
        [DefaultValue(false)]
        public bool AutoBarcode
        {
            get { return this.autoBarcode; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, bool>(ref this.autoBarcode, o => o.AutoBarcode, value); }
        }

        private bool autoCarton;
        [DefaultValue(false)]
        public bool AutoCarton
        {
            get { return this.autoCarton; }
            set { ApplyPropertyChange<BatchPrimitiveDTO, bool>(ref this.autoCarton, o => o.AutoCarton, value); }
        }

        [DefaultValue(false)]
        public bool IsDefault { get; set; }
    }

    public class BatchDTO : BatchPrimitiveDTO
    {
        public BatchDTO()
        {
            this.FillingLineID = (int)GlobalVariables.FillingLineID;
            this.LocationID = GlobalVariables.LocationID;
        }

        private string commodityName;
        [DefaultValue(null)]
        public string CommodityName
        {
            get { return this.commodityName; }
            set { ApplyPropertyChange<BatchDTO, string>(ref this.commodityName, o => o.CommodityName, value, false); }
        }

        private string commodityAPICode;
        [DefaultValue(null)]
        public string CommodityAPICode
        {
            get { return this.commodityAPICode; }
            set { ApplyPropertyChange<BatchDTO, string>(ref this.commodityAPICode, o => o.CommodityAPICode, value, false); }
        }

        protected override List<ValidationRule> CreateRules()
        {
            List<ValidationRule> validationRules = base.CreateRules(); int value;
            validationRules.Add(new SimpleValidationRule("CommodityID", "Vui lòng chọn mã sản phẩm.", delegate { return this.CommodityID > 0; }));
            validationRules.Add(new SimpleValidationRule("Code", "Số batch quy định là 9 ký tự, gồm 7 ký tự chính thức và 2 ký tự phụ.", delegate { return this.Code != null && this.Code.Length == 9; }));

            validationRules.Add(new SimpleValidationRule("NextPackNo", "Số thứ tự chai quy định là 6 chữ số.", delegate { return this.NextPackNo.Length == 6 && int.TryParse(this.NextPackNo, out value); }));
            validationRules.Add(new SimpleValidationRule("NextCartonNo", "Số thứ tự carton quy định là 6 chữ số.", delegate { return this.NextCartonNo.Length == 6 && int.TryParse(this.NextCartonNo, out value); }));
            validationRules.Add(new SimpleValidationRule("NextPalletNo", "Số thứ tự pallet quy định là 6 chữ số.", delegate { return this.NextPalletNo.Length == 6 && int.TryParse(this.NextPalletNo, out value); }));

            validationRules.Add(new SimpleValidationRule("BatchPackNo", "Số thứ tự chai quy định là 6 chữ số.", delegate { return this.BatchPackNo.Length == 6 && int.TryParse(this.BatchPackNo, out value); }));
            validationRules.Add(new SimpleValidationRule("BatchCartonNo", "Số thứ tự carton quy định là 6 chữ số.", delegate { return this.BatchCartonNo.Length == 6 && int.TryParse(this.BatchCartonNo, out value); }));
            validationRules.Add(new SimpleValidationRule("BatchPalletNo", "Số thứ tự pallet quy định là 6 chữ số.", delegate { return this.BatchPalletNo.Length == 6 && int.TryParse(this.BatchPalletNo, out value); }));

            validationRules.Add(new SimpleValidationRule("FinalCartonNo", "Số carton cuối cùng quy định là 6 chữ số và lớn hơn hoặc bằng số carton.", delegate { return this.FinalCartonNo.Length == 6 && int.TryParse(this.FinalCartonNo, out value) && (!this.AutoBarcode || int.Parse(this.NextCartonNo) <= int.Parse(this.FinalCartonNo)); }));

            return validationRules;

        }
    }
}
