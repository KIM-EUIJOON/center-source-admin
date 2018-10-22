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

        /// <summary>パスワード</summary>
        public string Password { get; set; }
        
        /// <summary>氏名</summary>
        public string Name { get; set; }

        /// <summary>ログイン失敗回数</summary>
        public int? FailureCount { get; set; }

        /// <summary>ログイン失敗日時</summary>
        public DateTime? FailureDatetime { get; set; }
    }
}