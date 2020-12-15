using AppSigmaAdmin.ResponseData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AppSigmaAdmin.Models
{
    public class MPA1301Context
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

        ///<summary>
        ///アプリ種別
        ///</summary>
        public string Apltype { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public string TicketInfo { get; set; }

        /// <summary>
        /// 決済種別
        /// </summary>
        public string PaymentType { get; set; }

        /// <summary>
        /// 大人・子供指定
        /// </summary>
        public string TicketNumType { get; set; }

        /// <summary>
        /// 決済情報リスト
        /// </summary>
        public MPA1301ReportData PaymentReportData { get; set; }






        /// <summary>
        /// チケットID
        /// </summary>
        public string TicketId { get; set; }

        /// <summary>
        /// チケット種別
        /// </summary>
        public string TransportType { get; set; }

        ///<summary>
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int ListPageNo { get; set; }
    }

    public class MPA1301ReportData
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
        public List<MPA1301ReportDataRow> ReportList { get; set; }
    }

    public class MPA1301ReportDataRow
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 決済日
        /// </summary>
        public string TranDatetime { get; set; }

        /// <summary>
        /// 決済ID
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// 商品種別名
        /// </summary>
        public string TicketName { get; set; }

        /// <summary>
        /// 大人枚数
        /// </summary>
        public string AdultNum { get; set; }

        /// <summary>
        /// 子供枚数
        /// </summary>
        public string ChildNum { get; set; }

        /// <summary>
        /// 決済種別
        /// </summary>
        public string PaymentType { get; set; }

        /// <summary>
        /// 利用金額
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 決済手段名
        /// </summary>
        public string PaymentName { get; set; }

        /// <summary>
        /// 仕向先
        /// </summary>
        public string Forward { get; set; }

        /// <summary>
        /// 領収書番号
        /// </summary>
        public string ReceiptNo { get; set; }

        ///<summary>
        ///アプリ種別
        ///</summary>
        public string Apltype { get; set; }

        /// <summary>
        /// 問い合わせID
        /// </summary>
        public string InquiryId { get; set; }

    }


    public class MPA1401Context
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
        public MPA1401ReportData UsageReportData { get; set; }



        ///<summary>
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int ListPageNo { get; set; }

    }

    public class MPA1401ReportData
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
        public List<MPA1401ReportDataRow> ReportList { get; set; }
    }

    public class MPA1401ReportDataRow
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// ユーザID
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
    /// 昭和自動車独自情報<para>
    /// ※西鉄独自情報{NishitetsuPaymentInfo}をコピー</para>
    /// </summary>
    public class ShowabusPaymentInfo : PaymentInfo
    {
        /// <summary>
        /// チケット種別
        /// </summary>
        public string TransportType { get; set; }

        /// <summary>
        /// 券種
        /// </summary>
        public string TicketType { get; set; }

        /// <summary>
        /// チケットID
        /// </summary>
        public string TicketId { get; set; }

        /// <summary>
        /// 商品種別名
        /// </summary>
        public string TicketName { get; set; }

        /// <summary>
        /// プルダウン用チケット種別
        /// </summary>
        public string TicketInfo { get; set; }

        /// <summary>
        /// 大人枚数
        /// </summary>
        public string AdultNum { get; set; }

        /// <summary>
        /// 子供枚数
        /// </summary>
        public string ChildNum { get; set; }

        /// <summary>
        /// 領収書番号
        /// </summary>
        public string ReceiptNo { get; set; }

        /// <summary>
        /// 問い合わせID
        /// </summary>
        public string InquiryId { get; set; }
    }

    /// <summary>
    /// 昭和自動車保険付き乗車券利用情報
    /// </summary>
    public class ShowabusUsageInsuranceInfo
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