using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// ユーザ認証リクエストクラス
    /// </summary>
    [DataContract]
    public class LoginModel
    {
        /// <summary>
        /// メールアドレス
        /// </summary>
        [Required]
        [Display(Name = "アカウント")]
        [DataMember(Name = "eMail")]
        public string MailAddress { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        [DataMember(Name = "password")]
        public string Password { get; set; }

        /// <summary>
        /// 運用管理機能ID取得関数
        /// </summary>
        /// <param name="Role"></param>
        /// <returns></returns>
        public List<RoleFunction> GetRoleFunctions(string Role)
        {
            List<RoleFunction> result = new List<RoleFunction>();

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand()) {

                StringBuilder rolesb = new StringBuilder();

                rolesb.Append("select sf.FunctionId");              //運用管理機能ID
                rolesb.Append("     , sf.FunctionName");            //機能名
                rolesb.Append("     , sf.RoleId");                  //RoleID
                rolesb.Append("     from SystemFunction sf");
                rolesb.Append("     where sf.RoleId =@RoleID");     //入力されたアドレスに紐づくRoleID
                rolesb.Append("     ORDER BY sf.DispOrder");        //表示順

                cmd.CommandText = rolesb.ToString();
                cmd.Parameters.Add("@RoleID", SqlDbType.NVarChar).Value = Role;

                DataTable dt = dbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows) {
                    RoleFunction RowInfo = new RoleFunction {
                        FuncId = row["FunctionId"].ToString(),
                        FuncName = row["FunctionName"].ToString(),
                        RoleId = row["RoleId"].ToString(),
                    };
                    result.Add(RowInfo);
                }
                return result;

            }
        }
    }
}