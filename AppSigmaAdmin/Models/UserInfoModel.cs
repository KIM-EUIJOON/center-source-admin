using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using AppSigmaAdmin.Utility;
using System.Data;
using System.Data.SqlClient;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// ユーザ情報取得クラス
    /// </summary>
    public class UserInfoModel
    {
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