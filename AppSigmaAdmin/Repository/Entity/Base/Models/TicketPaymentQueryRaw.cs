using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Entity.Base.Models
{
    /// <summary>
    /// チケット決済情報クエリデータ
    /// </summary>
    public class TicketPaymentQueryRaw
    {
        public string RecNo { get; set; }
        public int UserId { get; set; }
        public DateTime? TranDate { get; set; }
        public string BizCompanyCd { get; set; }
        public string TicketType { get; set; }
        public string TicketId { get; set; }
        public string TicketGroup { get; set; }
        public int SetNo { get; set; }
        public string Value { get; set; }
        public int AdultNum { get; set; }
        public int ChildNum { get; set; }
        public int? DiscountNum { get; set; }
        public int PaymentId { get; set; }
        public string AplType { get; set; }
        public string PaymentType { get; set; }
        public int? Amount { get; set; }
        public string ForwardCode { get; set; }
        public string ReceiptNo { get; set; }
        public string PaymentMeansCode { get; set; }
        public string PaymentDetailCode { get; set; }
        public string PaymentName { get; set; }
        public string PaymentDetailName { get; set; }
        public string InquiryId { get; set; }
    }
}