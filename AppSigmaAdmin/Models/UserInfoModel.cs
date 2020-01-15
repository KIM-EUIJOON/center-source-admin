using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AppSigmaAdmin.Models
{
    public class UserInfoModel
    {
        /// <summary>
        /// メールアドレス
        /// </summary>
        [Required]
        [Display(Name = "メールアドレス")]
        [DataMember(Name = "eMail")]
        public string MailAddress { get; set; }
    }

    /// <summary>
    /// 内部ID検索結果クラス
    /// </summary>
    public class UserIdInfoRespons
    {
        /// <summary>
        /// UserID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// アプリ種別
        /// </summary>
        public string AplType { get; set; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public string DeleteFlag { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public string CreateDatetime { get; set; }

        /// <summary>
        /// 削除日時
        /// </summary>
        public string DeleteDate { get; set; }


    }
    /// <summary>
    /// 内部ID検索結果受け渡しクラス
    /// </summary>
    public class ResonsID: UserInfoModel
    {
        public List<UserIdInfoRespons> UserIdInfoList { get; set; }

     }
}