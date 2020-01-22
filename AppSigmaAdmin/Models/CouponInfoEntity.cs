using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// クーポン運用出力クラス
    /// </summary>
    public class CouponInfoEntity
    {
        /// <summary>
        /// 利用日
        /// </summary>
        public DateTime UsageDateTime { get; set; }

        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 施設ID
        /// </summary>
        public string FacilityId { get; set; }

        /// <summary>
        /// 施設名
        /// </summary>
        public string FacilityName { get; set; }

        /// <summary>
        /// テナントコード
        /// </summary>
        public string ShopCode { get; set; }

        /// <summary>
        /// テナント名
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 利用件数
        /// </summary>
        public int UseCount { get; set; }

        /// <summary>
        /// 業種
        /// </summary>
        public string IndustryName { get; set; }

        /// <summary>
        /// アプリ種別
        /// </summary>
        public string AplType { get; set; }
    }
    /// <summary>
    /// 西鉄決済情報リスト作成用情報
    /// </summary>
    public class CouponInfoEntityList : CouponInfoEntity
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
        ///総リスト件数
        ///</summary>
        public int ListMaxCount { get; set; }

        ///<summary>
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int PageNo { get; set; }

        ///<summary>
        ///ページング数
        ///</summary>
        public int rowsPerPage { get; set; } = 10;

        ///<summary>
        ///総ページ数
        ///</summary>
        public int PageCount { get; set; }

        /// <summary>
        /// クーポン一覧
        /// </summary>
        public List<CouponInfoEntity> CouponInfoList { get; set; }

        /// <summary>
        /// クーポン一覧(全件)
        /// </summary>
        public List<CouponInfoEntity> CouponInfoListAll { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CouponInfoEntityList()
        {
            CouponInfoList = new List<CouponInfoEntity>();
            CouponInfoListAll = new List<CouponInfoEntity>();
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="source">コピー元</param>
        public CouponInfoEntityList(CouponInfoEntityList source)
        {
            this.TargetDateBegin = source.TargetDateBegin;
            this.TargetDateEnd = source.TargetDateEnd;
            this.ListMaxCount = source.ListMaxCount;
            this.PageNo = source.PageNo;
            this.rowsPerPage = source.rowsPerPage;
            this.PageCount = source.PageCount;
            this.UserId = source.UserId;
            this.FacilityId = source.FacilityId;
            this.FacilityName = source.FacilityName;
            this.ShopCode = source.ShopCode;
            this.ShopName = source.ShopName;
            this.AplType = source.AplType;
            this.IndustryName = source.IndustryName;
            CouponInfoList = new List<CouponInfoEntity>();
            CouponInfoListAll = new List<CouponInfoEntity>(source.CouponInfoListAll);
        }
    }
}