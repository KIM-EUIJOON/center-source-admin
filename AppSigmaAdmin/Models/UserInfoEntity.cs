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
        /// 内部ID取得（完全一致）
        /// </summary>
        /// <param name="EncryptedEMail">暗号化されたメールアドレス</param>
        /// <returns>内部ID</returns>
        public List<UserIdInfoRespons> GetUserInternalId(string EncryptedEMail)
        {
            List<UserIdInfoRespons> result = new List<UserIdInfoRespons>();

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select UserId, UpdateDatetime, DeleteFlag, CreateDatetime");
                sb.AppendLine(" from UserDetailOid");
                sb.AppendLine(" where EMailAddress = @EMailAddress");
                sb.AppendLine(" order by CreateDatetime DESC");

                cmd.CommandText = sb.ToString();
                cmd.Parameters.Add("@EMailAddress", SqlDbType.NVarChar).Value = EncryptedEMail;

                DataTable dt = dbInterface.ExecuteReader(cmd);

                foreach (DataRow IdDataRow in dt.Rows)
                {
                    UserIdInfoRespons userInfo = new UserIdInfoRespons();
                    //UserID
                    userInfo.UserId = IdDataRow["UserId"].ToString();
                    //作成日時
                    userInfo.CreateDatetime = IdDataRow["CreateDatetime"].ToString();

                    Boolean DeleteId = (Boolean)IdDataRow["DeleteFlag"];
                    //削除フラグ判定
                    if (DeleteId== true)
                    {
                        //更新日時(退会日時)
                        userInfo.DeleteDate = IdDataRow["UpdateDatetime"].ToString();
                    }
                    else 
                    {
                        //削除されていないため"―"を設定する
                        userInfo.DeleteDate = "―";
                    }
                    result.Add(userInfo);
                }
            }

            return result;
        }

    }
}