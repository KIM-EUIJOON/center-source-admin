using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using AppSigmaAdmin.Models;
using System.Data;
using System.Data.SqlClient;

namespace AppSigmaAdmin.Utility
{
    /// <summary>
    /// ユーザ情報取得クラス（データベース）
    /// </summary>
    public class DbAccessUserInfo
    {
        /// <summary>
        /// 管理ユーザ情報取得処理
        /// </summary>
        /// <param name="EMailAddress">メールドレス</param>
        /// <returns>管理ユーザ情報</returns>
        public UserInfoAdminEntity GetUserInfoAdmin(string EMailAddress)
        {
            UserInfoAdminEntity result = null;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT [AdminId], [EMailAddress], [Password], [Name], [FailureCount], [FailureDatetime]");
            sb.AppendLine("FROM [UserInfoAdmin]");
            sb.AppendLine("WHERE [EMailAddress] = @EMailAddress");

            using (SqlDbInterface dbInterFace = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = sb.ToString();
                cmd.Parameters.Add("@EMailAddress", SqlDbType.NVarChar).Value = EMailAddress;

                DataTable dt = dbInterFace.ExecuteReader(cmd);
                if (dt != null && dt.Rows.Count > 0)
                {
                    result = new UserInfoAdminEntity()
                    {
                        AdminId = dt.Rows[0]["AdminId"].ToString(),
                        EMailAddress = dt.Rows[0]["EMailAddress"].ToString(),
                        Password = dt.Rows[0]["Password"].ToString(),
                        Name = dt.Rows[0]["Name"] == DBNull.Value ? null : dt.Rows[0]["Name"].ToString(),
                        FailureCount = dt.Rows[0]["FailureCount"] == DBNull.Value ? null : (int?)int.Parse(dt.Rows[0]["FailureCount"].ToString()),
                        FailureDatetime = dt.Rows[0]["FailureDatetime"] == DBNull.Value ? null : (DateTime?)dt.Rows[0]["FailureDatetime"]
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// ユーザ情報取得処理
        /// </summary>
        /// <param name="userId">ユーザID</param>
        /// <returns>ユーザ情報</returns>
        public UserInfoEntity GetUserInfoModel(string userId)
        {
            UserInfoEntity userInfoEntity = null;
            StringBuilder sb = new StringBuilder();
            sb.Append("select ui.UserId");
            sb.Append("     , ui.RoleId");
            sb.Append("     , ui.EMailAddress");
            sb.Append("     , ud.KanaLastName");
            sb.Append("     , ud.KanaFirstName");
            sb.Append("  from UserInfo ui");
            sb.Append("  left join UserDetail ud");
            sb.Append("    on ui.UserId = ud.UserId");
            sb.Append(" where ui.UserId = @UserId");

            using (SqlDbInterface dbInterFace = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = sb.ToString();
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = int.Parse(userId);

                DataTable dt = dbInterFace.ExecuteReader(cmd);
                if (dt != null || dt.Rows.Count > 0)
                {
                    userInfoEntity = new UserInfoEntity()
                    {
                        UserId = dt.Rows[0]["UserId"].ToString(),
                        RoleId = dt.Rows[0]["RoleId"].ToString(),
                        EMailAddress = dt.Rows[0]["EMailAddress"].ToString(),
                        KanaLastName = dt.Rows[0]["KanaLastName"] == DBNull.Value ? null : dt.Rows[0]["KanaLastName"].ToString(),
                        KanaFirstName = dt.Rows[0]["KanaFirstName"] == DBNull.Value ? null : dt.Rows[0]["KanaFirstName"].ToString()
                    };
                }
            }

            return userInfoEntity;
        }
    }
}