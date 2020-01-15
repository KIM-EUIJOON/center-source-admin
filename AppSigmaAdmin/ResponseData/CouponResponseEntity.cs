using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AppSigmaAdmin.ResponseData
{
    /// <summary>
    ///クーポン管理　レスポンスクラス
    /// </summary>
    public class CouponResponseEntity
    {
        /// <summary>
        /// 利用日
        /// </summary>
        public string UsageDateTime { get; set; }

        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 施設
        /// </summary>
        public string FacilityId { get; set; }

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
    }
}