using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Library
{
    public class SystemConst
    {
        /// <summary>
        /// ロールID（管理者）
        /// </summary>
        public const string ROLE_ID_ADMIN = "9";

        /// <summary>
        /// セッション名（トークン）
        /// </summary>
        public const string SESSION_SIGMA_TOKEN = "SigmaToken";

        /// <summary>
        /// セッション名（ユーザ情報）
        /// </summary>
        public const string SESSION_USER_INFO = "UserInfo";

    }
}