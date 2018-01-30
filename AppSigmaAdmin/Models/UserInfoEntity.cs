using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// ユーザ情報エンティティクラス
    /// </summary>
    public class UserInfoEntity
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// ロールID
        /// </summary>
        public string RoleId { get; set; }

        /// <summary>
        /// メールアドレス
        /// </summary>
        public string EMailAddress { get; set; }

        /// <summary>
        /// カナ名字
        /// </summary>
        public string KanaLastName { get; set; }

        /// <summary>
        /// カナ名前
        /// </summary>
        public string KanaFirstName { get; set; }
    }
}