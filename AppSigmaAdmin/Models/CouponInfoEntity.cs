using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

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
        ///ページ番号
        ///</summary>
        [DataMember(Name = "PageNo")]
        public int ListPageNo { get; set; }

        ///<summary>
        ///総リスト件数
        ///</summary>
        public int ListMaxCount { get; set; }

        /// <summary>
        /// クーポン一覧
        /// </summary>
        public List<CouponInfoEntity> CouponInfoList { get; set; }
    }
    /// <summary>
    /// 運用レポートデータCSVマッピングクラス
    /// </summary>
    public sealed class CouponInfoEntityClassMap : ClassMap<CouponInfoEntity>
    {
        /// <summary>
        /// 運用レポートデータCSVマッピング
        /// </summary>
        public CouponInfoEntityClassMap()
        {
            Map(m => m.UsageDateTime).Index(0).Name("獲得日");
        //    Map(m => m.RoleInstance).Index(1).Name("インスタンス名");
        //    Map(m => m.CounterName).Index(2).Name("カウンター名");
        //    Map(m => m.CounterValue).Index(3).Name("カウンター値");
        //    Map(m => m.CounterName).Index(4).Name("カウンター名");
        //    Map(m => m.CounterValue).Index(5).Name("カウンター値");
        }
    }

}