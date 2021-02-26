using AppSigmaAdmin.Models;
using AppSigmaAdmin.Repository.Database.Entity.Base;
using AppSigmaAdmin.Repository.Entity.Base.Models;
using AppSigmaAdmin.Repository.Entity.Model;
using AppSigmaAdmin.Repository.Toyamachitetsu.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Toyamachitetsu
{
    /// <summary>
    /// 昭和バス決済情報データ
    /// </summary>
    public class ToyamachitetsuTicketPaymentRepository : TicketPaymentRepositoryBase<ToyamachitetsuPaymentInfo>
    {
        public ToyamachitetsuTicketPaymentRepository()
            : base(CompanyDefine.ToyamachitetsuCompanies)
        {

        }

        /// <summary>
        /// DBデータ変換
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        protected override ToyamachitetsuPaymentInfo Parse(TicketPaymentQueryRaw raw)
        {
            var infoN = new ToyamachitetsuPaymentInfo
            {
                UserId = raw.UserId.ToString(),
                TranDatetime = Option(raw.TranDate, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                PaymentId = raw.PaymentId.ToString(),
                TicketName = raw.Value,
                TicketId = raw.TicketId,
                TransportType = raw.BizCompanyCd,
                AdultNum = raw.AdultNum.ToString(),
                ChildNum = raw.ChildNum.ToString(),
                PaymentType = raw.PaymentType,
                Amount = raw.Amount.Value,
                ForwardCode = raw.ForwardCode,
                ReceiptNo = raw.ReceiptNo,
                InquiryId = raw.InquiryId,
                PaymentMeansCode = raw.PaymentMeansCode,
                PaymentDetailCode = raw.PaymentDetailCode,
                PaymentMeansName = raw.PaymentName,
                PaymentDetailName = raw.PaymentDetailName,
                Apltype = raw.AplType,
            };
            if (raw.TicketGroup == "1")
                infoN.TicketName += "[au]";

            if (raw.AplType == "1")
                infoN.Apltype = "au";
            else
                infoN.Apltype = "-";

            return infoN;
        }
    }
}