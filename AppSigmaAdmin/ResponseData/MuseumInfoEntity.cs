using AppSigmaAdmin.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace AppSigmaAdmin.ResponseData
{

    ///<summary>
    ///美術館・博物館履歴検索共通情報
    ///</summary>
    public class MuseumInfo
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
        /// リスト表示件数
        /// </summary>
        public int ListNum { get; set; }

        /// <summary>
        /// プルダウン用チケット種別
        /// </summary>
        public string TicketInfo { get; set; }

        ///<summary>
        ///アプリ種別
        ///</summary>
        public string Apltype { get; set; }

        ///<summary>
        ///業種
        /// </summary>
        public string Denomination { get; set; }

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

    }

    ///<summary>
    ///美術館・博物館利用履歴
    ///</summary>
    public class MuseumUseInfo : MuseumInfo
    {
        /// <summary>
        /// 利用日時
        /// </summary>
        public string UseDatetime { get; set; }
        /// <summary>
        /// 施設利用リスト
        /// </summary>
        public List<MuseumUseInfo> MuseumUseInfoList { get; set; }

        /// <summary>
        /// 施設利用リスト一覧
        /// </summary>
        public List<MuseumUseInfo> MuseumUseInfoListAll { get; set; }

        /// <summary>
        /// 施設名
        /// </summary>
        public string FacilityName { get; set; }

        /// <summary>
        /// 施設ID
        /// </summary>
        public string FacilityId { get; set; }

        /// <summary>
        /// テナント名
        /// </summary>
        public string TenantID { get; set; }

        /// <summary>
        /// テナント名
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// テナントタイプ
        /// </summary>
        public string ShopType { get; set; }

        /// <summary>
        /// 利用件数
        /// </summary>
        public int UseCount { get; set; }

        ///<summary>
        ///施設利用一覧表示用リスト
        ///</summary>
        public MuseumUseInfo()
        {
            MuseumUseInfoList = new List<MuseumUseInfo>();
            MuseumUseInfoListAll = new List<MuseumUseInfo>();
        }
        /// <summary>
        /// 横浜表示用リスト内容
        /// </summary>
        /// <param name="source">コピー元</param>
        public MuseumUseInfo(MuseumUseInfo source)
        {
            this.TargetDateBegin = source.TargetDateBegin;
            this.TargetDateEnd = source.TargetDateEnd;
            this.ListMaxCount = source.ListMaxCount;
            this.ListPageNo = source.ListPageNo;
            this.rowsPerPage = source.rowsPerPage;
            this.ListMaxCount = source.ListMaxCount;
            this.UserId = source.UserId;
            this.Apltype = source.Apltype;
            this.FacilityName = source.FacilityName;
            this.TenantName = source.TenantName;
            MuseumUseInfoList = new List<MuseumUseInfo>();
            MuseumUseInfoListAll = new List<MuseumUseInfo>(source.MuseumUseInfoListAll);
            this.Language = source.Language;
        }

    }

    ///<summary>
    ///美術館・博物館決済履歴
    ///</summary>
    public class MuseumPaymentInfo : MuseumInfo
    {
        /// <summary>
        /// 施設決済リスト
        /// </summary>
        public List<MuseumPaymentInfo> MuseumPaymentInfoList { get; set; }

        /// <summary>
        /// 施設決済リスト一覧
        /// </summary>
        public List<MuseumPaymentInfo> MuseumPaymentInfoListAll { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        public string TicketID { get; set; }

        /// <summary>
        /// 商品種別名
        /// </summary>
        public string TicketName { get; set; }

        /// <summary>
        /// 決済日
        /// </summary>
        public string TranDatetime { get; set; }

        /// <summary>
        /// 決済種別
        /// </summary>
        public string PaymentType { get; set; }

        /// <summary>
        /// 利用金額
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// 決済ID
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// 領収書番号
        /// </summary>
        public string ReceiptNo { get; set; }

        /// <summary>
        /// 大人枚数
        /// </summary>
        public string AdultNum { get; set; }

        /// <summary>
        /// 高大生枚数
        /// </summary>
        public string ChildNum { get; set; }

        /// <summary>
        /// 小児枚数
        /// </summary>
        public string infantNum { get; set; }

        /// <summary>
        /// 事業者種別
        /// </summary>
        public string BizCompanyCd { get; set; }

        public string TicketNumType { get; internal set; }

        ///<summary>
        ///横浜表示用リスト
        ///</summary>
        public MuseumPaymentInfo()
        {
            MuseumPaymentInfoList = new List<MuseumPaymentInfo>();
            MuseumPaymentInfoListAll = new List<MuseumPaymentInfo>();
        }
        /// <summary>
        /// 横浜表示用リスト内容
        /// </summary>
        /// <param name="source">コピー元</param>
        public MuseumPaymentInfo(MuseumPaymentInfo source)
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
            this.Denomination = source.Denomination;
            this.AdultNum = source.AdultNum;
            this.ChildNum = source.ChildNum;
            this.infantNum = source.infantNum;
            this.Amount = source.Amount;
            this.Apltype = source.Apltype;
            this.TicketID = source.TicketID;
            MuseumPaymentInfoList = new List<MuseumPaymentInfo>();
            MuseumPaymentInfoListAll = new List<MuseumPaymentInfo>(source.MuseumPaymentInfoListAll);
            this.Language = source.Language;
        }

    }
}