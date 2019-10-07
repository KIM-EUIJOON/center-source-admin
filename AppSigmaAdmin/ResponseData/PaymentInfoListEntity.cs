using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;

namespace AppSigmaAdmin.ResponseData
{

    /// <summary>
    /// 決済情報クラス
    /// </summary>
    public class PaymentInfo
    {
        ///<summary>
        ///リクエスト番号
        /// </summary>
        public string ReqNo { get; set; }

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
        /// 決済種別
        /// </summary>
        public string PaymentType { get; set; }

        /// <summary>
        /// 利用金額
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// リスト表示件数
        /// </summary>
        public int ListNum { get; set; }

    }

    /// <summary>
    /// Jtx独自情報
    /// </summary>
    public class JtxPaymentInfo : PaymentInfo
    {
        /// <summary>
        /// 加盟店屋号
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// JTXオーダーID
        /// </summary>
        public string OrderId { get; set; }

    }
    /// <summary>
    /// Jtx決済情報作成用リスト
    /// </summary>
    public class JtxPaymentInfoListEntity : PaymentInfo
    {

        ///<summary>
        ///抽出開始指定日(YYYY-MM-DD)
        ///</summary>
        [DataMember(Name = "targetDateBegin")]
        public string TargetDateBegin { get; set; }

        ///<summary>
        ///抽出終了指定日(YYYY-MM-DD)
        ///</summary>
        [DataMember(Name = "targetDateEnd")]
        public string TargetDateEnd { get; set; }

        ///<summary>
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int ListPageNo { get; set; }

        ///<summary>
        ///総リスト件数
        ///</summary>
        public int ListMaxCount { get; set; }

        /// <summary>
        /// Jtx決済情報リスト
        /// </summary>
        public List<JtxPaymentInfo> JtxPaymentInfoList { get; set; }

    }
    /// <summary>
    /// ナッセ独自情報
    /// </summary>
    public class NassePaymentInfo : PaymentInfo
    {
        /// <summary>
        /// パスポートID
        /// </summary>
        public string PassportID { get; set; }
        /// <summary>
        /// パスポート名
        /// </summary>
        public string PassportName { get; set; }

    }
    /// <summary>
    /// ナッセ決済情報リスト作成用情報
    /// </summary>
    public class NassePaymentInfoListEntity : NassePaymentInfo
    {
        ///<summary>
        ///抽出開始指定日(YYYY-MM-DD)
        ///</summary>
        [DataMember(Name = "targetDateBegin")]
        public string TargetDateBegin { get; set; }

        ///<summary>
        ///抽出終了指定日(YYYY-MM-DD)
        ///</summary>
        [DataMember(Name = "targetDateEnd")]
        public string TargetDateEnd { get; set; }

        ///<summary>
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int ListPageNo { get; set; }

        ///<summary>
        ///総リスト件数
        ///</summary>
        public int ListMaxCount { get; set; }

        ///<summary>
        ///ナッセ決済情報リスト
        ///</summary>
        public List<NassePaymentInfo> NassePaymentInfoList { get; set; }

        ///<summary>
        ///ナッセプルダウンリスト
        ///</summary>
        public List<NassePaymentInfo> NassePulldownList { get; set; }
    }

    /// <summary>
    /// 西鉄独自情報
    /// </summary>
    public class NishitetsuPaymentInfo : PaymentInfo
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
    }
    /// <summary>
    /// 西鉄決済情報リスト作成用情報
    /// </summary>
    public class NishitetsuPaymentInfoListEntity : NishitetsuPaymentInfo
    {
        ///<summary>
        ///抽出開始指定日(YYYY-MM-DD)
        ///</summary>
        [DataMember(Name = "targetDateBegin")]
        public string TargetDateBegin { get; set; }

        ///<summary>
        ///抽出終了指定日(YYYY-MM-DD)
        ///</summary>
        [DataMember(Name = "targetDateEnd")]
        public string TargetDateEnd { get; set; }

        ///<summary>
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int ListPageNo { get; set; }

        ///<summary>
        ///総リスト件数
        ///</summary>
        public int ListMaxCount { get; set; }

        /// <summary>
        /// 大人・子供指定
        /// </summary>
        public string TicketNumType { get; set; }

        /// <summary>
        /// 西鉄決済情報リスト
        /// </summary>
        public List<NishitetsuPaymentInfo> NishitetsuPaymentInfoList { get; set; }

        /// <summary>
        /// 西鉄券種プルダウン用リスト
        /// </summary>
        public List<NishitetsuPaymentInfo> NishitetsuPullDownList { get; set; }
    }
    /// <summary>
    /// 問い合わせ番号
    /// </summary>
    public class InquiryInfo : PaymentInfo
    {
        public string InquiryNo { get; set; }
    }
    }