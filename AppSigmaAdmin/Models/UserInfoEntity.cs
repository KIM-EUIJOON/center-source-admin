using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

using AppSigmaAdmin.Library;
using AppSigmaAdmin.Utility;

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

    /// <summary>
    /// ユーザ情報クラス
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 内部ID取得取得（完全一致）
        /// </summary>
        /// <param name="EncryptedEMail">暗号化されたメールアドレス</param>
        /// <returns>内部ID</returns>
        public string GetUserInternalId(string EncryptedEMail)
        {
            string result = null;

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select top 1 UserId");
                sb.AppendLine(" from UserDetailOid");
                sb.AppendLine(" where EMailAddress = @EMailAddress");
                sb.AppendLine(" and DeleteFlag = 0");
                sb.AppendLine(" order by No DESC");

                cmd.CommandText = sb.ToString();
                cmd.Parameters.Add("@EMailAddress", SqlDbType.NVarChar).Value = EncryptedEMail;

                DataTable dt = dbInterface.ExecuteReader(cmd);

                if (dt != null && dt.Rows.Count > 0)
                {
                    result = dt.Rows[0]["UserId"].ToString();
                }
            }

            return result;
        }
    }
}