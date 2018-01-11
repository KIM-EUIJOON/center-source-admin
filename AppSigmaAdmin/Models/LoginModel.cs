using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// ユーザ認証　レスポンスクラス
    /// </summary>
    [DataContract]
    public class LoginModel
    {
        /// <summary>
        /// メールアドレス
        /// </summary>
        [Required]
        [Display(Name = "アカウント")]
        [DataMember(Name = "eMail")]
        public string MailAddress { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}