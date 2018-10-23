using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
}