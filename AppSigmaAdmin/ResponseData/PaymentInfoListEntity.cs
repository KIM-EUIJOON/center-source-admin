using AppSigmaAdmin.Library;
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

        ///<summary>
        ///アプリ種別
        ///</summary>
        public string Apltype { get; set; }

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
    public class DocomoPaymentInfoListEntity : PaymentInfo
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
        ///ページング数
        ///</summary>
        public int rowsPerPage { get; set; } = SystemConst.ROWS_PER_PAGE;

        ///<summary>
        ///総ページ数
        ///</summary>
        public int PageCount { get; set; }

        ///<summary>
        ///総リスト件数
        ///</summary>
        public int ListMaxCount { get; set; }

        /// <summary>
        /// 言語設定
        /// </summary>
        public string Language { get; set; } = SystemConst.LANGUAGE_JA;

        ///<summary>
        ///予約ID
        ///</summary>
        public string ReserveId { get; set; }

        ///<summary>
        ///自転車事業者名
        ///</summary>
        public string CycleBizName { get; set; }

        /// <summary>
        /// 決済一覧
        /// </summary>
        public List<DocomoPaymentInfoListEntity> DocomoPaymentList { get; set; }

        /// <summary>
        /// 決済一覧(全件)
        /// </summary>
        public List<DocomoPaymentInfoListEntity> DocomoPaymentListAll { get; set; }
        ///<summary>
        ///ドコモ表示用リスト
        ///</summary>
        public DocomoPaymentInfoListEntity()
        {
            DocomoPaymentList = new List<DocomoPaymentInfoListEntity>();
            DocomoPaymentListAll = new List<DocomoPaymentInfoListEntity>();
        }
        /// <summary>
        /// ドコモ表示用リスト内容
        /// </summary>
        /// <param name="source">コピー元</param>
        public DocomoPaymentInfoListEntity(DocomoPaymentInfoListEntity source)
        {
            this.TargetDateBegin = source.TargetDateBegin;
            this.TargetDateEnd = source.TargetDateEnd;
            this.ListMaxCount = source.ListMaxCount;
            this.ListPageNo = source.ListPageNo;
            this.rowsPerPage = source.rowsPerPage;
            this.ListMaxCount = source.ListMaxCount;
            this.UserId = source.UserId;
            this.Apltype = source.Apltype;
            this.CycleBizName = source.CycleBizName;
            this.ReserveId = source.ReserveId;
            DocomoPaymentList = new List<DocomoPaymentInfoListEntity>();
            DocomoPaymentListAll = new List<DocomoPaymentInfoListEntity>(source.DocomoPaymentListAll);
            this.Language = source.Language;
        }
    }

    /// <summary>
    /// 横浜決済情報取得用リスト
    /// </summary>
    public class  YokohamaPaymentInfo: NishitetsuPaymentInfoListEntity
    {
        ///<summary>
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int PageNo { get; set; }

        ///<summary>
        ///総ページ数
        ///</summary>
        public int PageCount { get; set; }

        /// <summary>
        /// 言語設定
        /// </summary>
        public string Language { get; set; } = SystemConst.LANGUAGE_JA;

        ///<summary>
        ///ページング数
        ///</summary>
        public int rowsPerPage { get; set; } = SystemConst.ROWS_PER_PAGE;

        /// <summary>
        /// 横浜決済リスト
        /// </summary>
        public List<YokohamaPaymentInfo> YokohamaPaymentInfoList { get; set; }

        /// <summary>
        /// 横浜決済リスト一覧
        /// </summary>
        public List<YokohamaPaymentInfo> YokohamaPaymentInfoListAll { get; set; }

        ///<summary>
        ///横浜表示用リスト
        ///</summary>
        public YokohamaPaymentInfo()
        {
            YokohamaPaymentInfoList = new List<YokohamaPaymentInfo>();
            YokohamaPaymentInfoListAll = new List<YokohamaPaymentInfo>();
        }
        /// <summary>
        /// 横浜表示用リスト内容
        /// </summary>
        /// <param name="source">コピー元</param>
        public YokohamaPaymentInfo(YokohamaPaymentInfo source)
        {
            this.TargetDateBegin = source.TargetDateBegin;
            this.TargetDateEnd = source.TargetDateEnd;
            this.ListMaxCount = source.ListMaxCount;
            this.ListPageNo = source.ListPageNo;
            this.rowsPerPage = source.rowsPerPage;
            this.ListMaxCount = source.ListMaxCount;
            this.UserId = source.UserId;
            this.Apltype = source.Apltype;
            this.TicketName = source.TicketName;
            this.PaymentType = source.PaymentType;
            this.AdultNum = source.AdultNum;
            this.ChildNum = source.ChildNum;
            this.Apltype = source.Apltype;
            YokohamaPaymentInfoList = new List<YokohamaPaymentInfo>();
            YokohamaPaymentInfoListAll = new List<YokohamaPaymentInfo>(source.YokohamaPaymentInfoListAll);
            this.Language = source.Language;
        }
    }
    public class MiyakohPaymentInfoListEntity : NishitetsuPaymentInfoListEntity
    {
        /// <summary>
        /// 学割枚数
        /// </summary>
        public string discountNum { get; set; }

        /// <summary>
        /// 西鉄決済情報リスト
        /// </summary>
        public List<MiyakohPaymentInfoListEntity> MiyakohPaymentInfoList { get; set; }
    }
}
