using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Entity.Base.Models
{
    /// <summary>
    /// チケット利用実績クエリデータ
    /// </summary>
    public class TicketUsageQueryRaw
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// チケットID
        /// </summary>
        public string TicketId { get; set; }
        /// <summary>
        /// セットNo
        /// </summary>
        public int SetNo { get; set; }
        /// <summary>
        /// 利用開始日時
        /// </summary>
        public DateTime? UsageStartDatetime { get; set; }
        /// <summary>
        /// 利用終了日時
        /// </summary>
        public DateTime? UsageEndDatetime { get; set; }
        /// <summary>
        /// 問い合わID
        /// </summary>
        public string InquiryId { get; set; }
    }
}