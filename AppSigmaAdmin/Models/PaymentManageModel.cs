using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// 決済管理　リクエストクラス
    /// </summary>
    [DataContract]
    public class PaymentManageModel
    {
        /// <summary>
        /// 検索開始_年
        /// </summary>
        [Required]
        [Display(Name = "年")]
        [DataMember(Name = "fromYear")]
        [RegularExpression("^[0-9]{4}", ErrorMessage = "検索開始日_年は半角数値4桁で入力してください")]
        public string FromYear { get; set; }

        /// <summary>
        /// 検索開始_月
        /// </summary>
        [Required]
        [Display(Name = "月")]
        [DataMember(Name = "fromMonth")]
        [RegularExpression("^[0-9]{1,2}", ErrorMessage = "検索開始日_月は半角数値で入力してください")]
        public string FromMonth { get; set; }

        /// <summary>
        /// 検索開始_日
        /// </summary>
        [Required]
        [Display(Name = "日")]
        [DataMember(Name = "fromDay")]
        [RegularExpression("^[0-9]{1,2}", ErrorMessage = "検索開始日_日は半角数値で入力してください")]
        public string FromDay { get; set; }

        /// <summary>
        /// 検索終了_年
        /// </summary>
        [Required]
        [Display(Name = "年")]
        [DataMember(Name = "toYear")]
        [RegularExpression("^[0-9]{4}", ErrorMessage = "検索終了日_年は半角数値4桁で入力してください")]
        public string ToYear { get; set; }

        /// <summary>
        /// 検索終了_月
        /// </summary>
        [Required]
        [Display(Name = "月")]
        [DataMember(Name = "toMonth")]
        [RegularExpression("^[0-9]{1,2}", ErrorMessage = "検索終了日_月は半角数値で入力してください")]
        public string ToMonth { get; set; }

        /// <summary>
        /// 検索終了_日
        /// </summary>
        [Required]
        [Display(Name = "日")]
        [DataMember(Name = "toDay")]
        [RegularExpression("^[0-9]{1,2}", ErrorMessage = "検索終了日_日は半角数値で入力してください")]
        public string ToDay { get; set; }

        /// <summary>
        /// 出力種別
        /// </summary>
        [DataMember(Name = "paymentOver")]
        public string PaymentOver { get; set; }

    }
}