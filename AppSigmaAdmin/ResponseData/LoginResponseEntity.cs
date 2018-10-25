using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AppSigmaAdmin.ResponseData
{
    /// <summary>
    /// ユーザ認証　レスポンスクラス
    /// </summary>
    [DataContract]
    public class LoginResponseEntity
    {
        /// <summary>
        /// 処理結果コード
        /// </summary>
        [DataMember(Name = "procCode")]
        public string ProcCode { get; set; }

        /// <summary>
        /// 処理結果メッセージ
        /// </summary>
        [DataMember(Name = "procMsg")]
        public string ProcMessage { get; set; }

        /// <summary>
        /// 認証トークン
        /// </summary>
        [DataMember(Name = "token")]
        public string Token { get; set; }

        /// <summary>
        /// ユーザID
        /// </summary>
        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        /// <summary>
        /// 名前
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// 権限
        /// </summary>
        [DataMember(Name = "role")]
        public string Role { get; set; }
    }
}
