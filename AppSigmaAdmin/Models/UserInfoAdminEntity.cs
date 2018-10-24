using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// 管理ユーザ情報エンティティクラス
    /// </summary>
    public class UserInfoAdminEntity
    {
        /// <summary>管理者ID</summary>
        public string AdminId { get; set; }

        /// <summary>メールアドレス</summary>
        public string EMailAddress { get; set; }
        
        /// <summary>氏名</summary>
        public string Name { get; set; }

        /// <summary>権限</summary>
        public string Role { get; set; }
    }
}