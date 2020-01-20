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

                sb.AppendLine("SELECT UserId, UpdateDatetime, DeleteFlag, CreateDatetime");
                sb.AppendLine("FROM [dbo].[UserInfoOid]");
                sb.AppendLine("WHERE [UserId] IN (");
                sb.AppendLine("    SELECT UserId FROM [dbo].[UserDetailOid]");
                sb.AppendLine("    WHERE EMailAddress = @EMailAddress");
                sb.AppendLine(")");
                sb.AppendLine("ORDER BY CreateDatetime DESC");

                cmd.CommandText = sb.ToString();
                cmd.Parameters.Add("@EMailAddress", SqlDbType.NVarChar).Value = EncryptedEMail;

                DataTable dt = dbInterface.ExecuteReader(cmd);

                foreach (DataRow IdDataRow in dt.Rows)
                {
                    UserIdInfoRespons userInfo = new UserIdInfoRespons();
                    //UserID
                    userInfo.UserId = IdDataRow["UserId"].ToString();

                    //作成日時
                    userInfo.CreateDatetime = String.Format("{0:yyyy/MM/dd HH:mm:ss}", Common.Utc2JstTime((DateTime)IdDataRow["CreateDatetime"]));
                    Boolean DeleteId = (Boolean)IdDataRow["DeleteFlag"];

                    //削除フラグ判定
                    if (DeleteId== true)
                    {
                        //更新日時(退会日時)
                        userInfo.DeleteDate = String.Format("{0:yyyy/MM/dd HH:mm:ss}", Common.Utc2JstTime((DateTime)IdDataRow["UpdateDatetime"]));
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

        /// <summary>
        /// myrouteID有無検索
        /// </summary>
        /// <param name="MyrouteId">入力されたmyrouteId</param>
        /// <returns>Idの有無</returns>
        public Dictionary<string,string> UserIdSearch(string keyvalue,string MyrouteId)
        {
            var result = new Dictionary<string,string>();

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("SELECT UserId, DeleteFlag,AplType");
                sb.AppendLine("FROM [dbo].[UserInfoOid]");
                sb.AppendLine("    WHERE UserId = @MyrouteId");

                cmd.CommandText = sb.ToString();
                cmd.Parameters.Add("@MyrouteId", SqlDbType.NVarChar).Value = MyrouteId;

                DataTable dt = dbInterface.ExecuteReader(cmd);
                int searchresult = 0;

                foreach (DataRow IdDataRow in dt.Rows)
                {
                    UserIdInfoRespons userInfo = new UserIdInfoRespons();
                    //UserID
                    string Useridkey = "UserId";
                    string UserId = IdDataRow["UserId"].ToString();
                    result.Add(Useridkey, UserId);
                    //AplType
                    string AplTypekey = "AplTypeNo";
                    string AplType = IdDataRow["AplType"].ToString();
                    result.Add(AplTypekey, AplType);
                    //削除フラグ
                    Boolean DeleteId = (Boolean)IdDataRow["DeleteFlag"];

                    //削除フラグ判定
                    if (DeleteId == true)
                    {
                        /*削除済アカウント*/
                        searchresult =-1;
                    }
                    else
                    {
                        /*存在しているアカウント*/
                        searchresult= searchresult +1;
                    }
                    string Deleteflugkey = "Deleteflugkey";
                    string SearchResult = searchresult.ToString();
                    result.Add(Deleteflugkey, SearchResult);
                }

                return result;
            }
        }

    }
}
 