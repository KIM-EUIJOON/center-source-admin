using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using AppSigmaAdmin.Utility;
using System.Data;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// ユーザ情報クラス
    /// </summary>
    public class UserInfoModel
    {
        public UserInfoEntity GetUserInfoModel(string userId)
        {
            StringBuilder sb = new StringBuilder();
            UserInfoEntity userInfoEntity = new UserInfoEntity();

            sb.Append("select ui.UserId");
            sb.Append("     , ui.RoleId");
            sb.Append("     , ui.EMailAddress");
            sb.Append("     , ud.KanaLastName");
            sb.Append("     , ud.KanaFirstName");
            sb.Append("  from UserInfo ui");
            sb.Append("  left join UserDetail ud");
            sb.Append("    on ui.UserId = ud.UserId");
            sb.Append(" where ui.UserId = '" + userId + "'");

            using (SqlDbInterface dbInterFace = new SqlDbInterface())
            {
                DataTable dt = dbInterFace.ExecuteReader(sb.ToString());
                if(dt != null || dt.Rows.Count > 0)
                {
                    userInfoEntity.UserId = dt.Rows[0]["UserId"].ToString();
                    userInfoEntity.RoleId = dt.Rows[0]["RoleId"].ToString();
                    userInfoEntity.EMailAddress = dt.Rows[0]["EMailAddress"].ToString();
                    userInfoEntity.KanaLastName = dt.Rows[0]["KanaLastName"].ToString();
                    userInfoEntity.KanaFirstName = dt.Rows[0]["KanaFirstName"].ToString();
                }
            }

            return userInfoEntity;
        }
    }
}