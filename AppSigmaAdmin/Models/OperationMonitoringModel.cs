using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// 運用レポート出力　リクエストクラス
    /// </summary>
    public class OperationMonitoringModel
    {
        /// <summary>
        /// 運用レポート出力年
        /// </summary>
        [Required]
        [Display(Name = "年")]
        [DataMember(Name = "year")]
        public string Year { get; set; }

        /// <summary>
        /// 運用レポート出力月
        /// </summary>
        [Required]
        [Display(Name = "月")]
        [DataMember(Name = "month")]
        public string Month { get; set; }
    }
}