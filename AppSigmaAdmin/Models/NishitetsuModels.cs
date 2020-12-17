using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AppSigmaAdmin.Models
{
    public class MPA1402Context
    {
        ///<summary>
        ///抽出開始指定日(YYYY-MM-DD)
        ///</summary>
        public string TargetDateBegin { get; set; }

        ///<summary>
        ///抽出終了指定日(YYYY-MM-DD)
        ///</summary>
        public string TargetDateEnd { get; set; }

        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// チケット種別
        /// </summary>
        public string TransportType { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public string TicketInfo { get; set; }

        /// <summary>
        /// 利用情報リスト
        /// </summary>
        public MPA1402ReportData UsageReportData { get; set; }



        ///<summary>
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int ListPageNo { get; set; }

    }

    public class MPA1402ReportData
    {
        ///<summary>
        ///総リスト件数
        ///</summary>
        public int ListMaxCount { get; set; }

        /// <summary>
        /// リスト表示件数
        /// </summary>
        public int ListNum { get; set; }

        /// <summary>
        /// 検索データ
        /// </summary>
        public List<MPA1402ReportDataRow> ReportList { get; set; }
    }

    public class MPA1402ReportDataRow
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 商品種別名
        /// </summary>
        public string TicketName { get; set; }

        /// <summary>
        /// 利用開始日時
        /// </summary>
        public string UsageStartDatetime { get; set; }

        /// <summary>
        /// 利用終了日時
        /// </summary>
        public string UsageEndDatetime { get; set; }

        /// <summary>
        /// 問い合わせID
        /// </summary>
        public string InquiryId { get; set; }
    }

    /// <summary>
    /// 西鉄乗車券利用情報
    /// </summary>
    public class NishitetsuUsageInfo
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 商品種別名
        /// </summary>
        public string TicketName { get; set; }

        /// <summary>
        /// 利用開始日時
        /// </summary>
        public string UsageStartDatetime { get; set; }

        /// <summary>
        /// 利用終了日時
        /// </summary>
        public string UsageEndDatetime { get; set; }

        /// <summary>
        /// 問い合わせID
        /// </summary>
        public string InquiryId { get; set; }
    }
}