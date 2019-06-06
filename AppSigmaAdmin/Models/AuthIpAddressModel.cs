using AppSigmaAdmin.Library;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthIpAddressModel
    {

        /// <summary>
        /// 認証ネットワークアドレス
        /// </summary>
        public class AuthIpAddressEntity
        {
            /// <summary>
            /// IPアドレス
            /// </summary>
            public string IPAddress { get; set; }

            /// <summary>
            /// サブネットアドレス
            /// </summary>
            public string SubnetAddress { get; set; }
        }

        /// <summary>
        /// 認証ネットワークアドレスをデータベースから取得
        /// </summary>
        /// <returns></returns>
        public List<AuthIpAddressEntity> GetAuthIpAddress()
        {
            List<AuthIpAddressEntity> resultEntitys = new List<AuthIpAddressEntity>();

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand dbCommand = new SqlCommand())
            {
                // クエリ生成
                StringBuilder query = new StringBuilder();
                query.AppendLine("SELECT NetAddress FROM AuthIpAddress");
                var env = ApplicationConfig.DeployEnv;
                if (env == Library.ApplicationConfig.ENV_DEBUG)
                {
                    query.AppendLine("WHERE IsEnvDevelop = 1");
                }
                else if (env == Library.ApplicationConfig.ENV_PREPROD)
                {
                    query.AppendLine("WHERE IsEnvPreProd = 1");
                }
                else if (env == Library.ApplicationConfig.ENV_PROD)
                {
                    query.AppendLine("WHERE IsEnvProd = 1");
                }

                // クエリ発行
                dbCommand.CommandText = query.ToString();
                DataTable dataTable = dbInterface.ExecuteReader(dbCommand);

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    string[] netInfos = dataRow["NetAddress"].ToString().Split('/');

                    AuthIpAddressEntity authIpAddressEntity = new AuthIpAddressEntity
                    {
                        IPAddress = netInfos[0],
                        SubnetAddress = netInfos[1]
                    };
                    resultEntitys.Add(authIpAddressEntity);
                }

                return resultEntitys;
            }
        }
        
    }
    public class AuthIpAddressEntityList : AuthIpAddressModel
    {
        /// <summary>
        /// ネットアドレス名
        /// </summary>
        public string NetAddress { get; set; }

        /// <summary>
        /// IPアドレス名(Memo)
        /// </summary>
        public string IPAddressName { get; set; }

        /// <summary>
        /// 開発環境閲覧可否
        /// </summary>
        public string EnvDev { get; set; }

        /// <summary>
        /// 検証環境閲覧可否
        /// </summary>
        public string EnvPre { get; set; }

        /// <summary>
        /// 号口環境閲覧可否
        /// </summary>
        public string EnvProd { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public string CreateDate { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public string UpdateDate { get; set; }

        ///<summary>
        ///IPアドレス表示・編集用リスト
        ///</summary>
        public List<AuthIpAddressEntityList> IPAddressList { get; set; }

        public List<AuthIpAddressEntityList> GetIpList()
        {
            List<AuthIpAddressEntityList> resultIpList = new List<AuthIpAddressEntityList>();
            using (SqlDbInterface dblist = new SqlDbInterface())
            using (SqlCommand dbCommand = new SqlCommand())
            {
                // クエリ生成
                StringBuilder query = new StringBuilder();
                query.AppendLine("SELECT * FROM AuthIpAddress");
                // クエリ発行
                dbCommand.CommandText = query.ToString();
                DataTable dataTable = dblist.ExecuteReader(dbCommand);

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    DataTable dt = dblist.ExecuteReader(dbCommand);

                    string Devst = dataRow["IsEnvDevelop"].ToString();
                    if (Devst == "True")
                    {
                        Devst = "○";
                    }
                    else
                    {
                        Devst = "-";
                    }
                    string Prest = dataRow["IsEnvPreProd"].ToString();
                    if (Prest == "True")
                    {
                        Prest = "○";
                    }
                    else
                    {
                        Prest = "-";
                    }
                    string Predst = dataRow["IsEnvProd"].ToString();
                    if (Predst== "True")
                    {
                        Predst = "○";
                    }
                    else
                    {
                        Predst = "-";
                    }
                    AuthIpAddressEntityList IpAddressList = new AuthIpAddressEntityList
                    {
                        NetAddress = dataRow["NetAddress"].ToString(),
                        IPAddressName = dataRow["Memo"].ToString(),
                        EnvDev = Devst,
                        EnvPre = Prest,
                        EnvProd = Predst,
                        CreateDate = dataRow["CreateDatetime"].ToString(),
                        UpdateDate = dataRow["UpdateDatetime"].ToString()
                    };
                    resultIpList.Add(IpAddressList);
                }

                return resultIpList;
            }
        }
        /// <summary>
        /// IPアドレス新規追加処理
        /// </summary>
        /// <param name="NetAddress"></param>
        /// <param name="Memo"></param>
        /// <param name="EnvDev"></param>
        /// <param name="EnvPre"></param>
        /// <param name="EnvProd"></param>
        public void UpdateIPList(string NetAddress, string Memo, string EnvDev, string EnvPre, string EnvProd)
        {
            using (SqlDbInterface dblist = new SqlDbInterface())
            using (SqlCommand dbCommand = new SqlCommand())
            {

                    StringBuilder query = new StringBuilder();
                    query.AppendLine("insert into AuthIpAddress");
                    query.AppendLine(" (NetAddress, Memo, IsEnvDevelop, IsEnvPreProd, IsEnvProd)");
                    query.AppendLine(" values(@NetAddress, @Memo, @EnvDev, @EnvPre, @EnvProd);");
                    // クエリ発行
                    dbCommand.CommandText = query.ToString();

                    dbCommand.Parameters.Add("@NetAddress", SqlDbType.NVarChar).Value = NetAddress;
                    dbCommand.Parameters.Add("@Memo", SqlDbType.NVarChar).Value = Memo;
                    dbCommand.Parameters.Add("@EnvDev", SqlDbType.NVarChar).Value = EnvDev;
                    dbCommand.Parameters.Add("@EnvPre", SqlDbType.NVarChar).Value = EnvPre;
                    dbCommand.Parameters.Add("@EnvProd", SqlDbType.NVarChar).Value = EnvProd;

                    dblist.ExecuteReader(dbCommand);

            }
        }
        /// <summary>
        /// IPアドレスリスト更新処理
        /// </summary>
        /// <param name="NetAddress"></param>
        /// <param name="Memo"></param>
        /// <param name="EnvDev"></param>
        /// <param name="EnvPre"></param>
        /// <param name="EnvProd"></param>
        public void EditIPList(string NetAddress, string Memo, string EnvDev, string EnvPre, string EnvProd)
        {
            using (SqlDbInterface dblist = new SqlDbInterface())
            using (SqlCommand dbCommand = new SqlCommand())
            {

                StringBuilder query = new StringBuilder();
                query.AppendLine("update AuthIpAddress");
                query.AppendLine(" set Memo=@Memo");
                query.AppendLine(", IsEnvDevelop=@EnvDev");
                query.AppendLine(", IsEnvPreProd=@EnvPre");
                query.AppendLine(", IsEnvProd=@EnvProd");
                query.AppendLine(" where NetAddress=@NetAddress;");
                // クエリ発行
                dbCommand.CommandText = query.ToString();

                dbCommand.Parameters.Add("@NetAddress", SqlDbType.NVarChar).Value = NetAddress;
                dbCommand.Parameters.Add("@Memo", SqlDbType.NVarChar).Value = Memo;
                dbCommand.Parameters.Add("@EnvDev", SqlDbType.NVarChar).Value = EnvDev;
                dbCommand.Parameters.Add("@EnvPre", SqlDbType.NVarChar).Value = EnvPre;
                dbCommand.Parameters.Add("@EnvProd", SqlDbType.NVarChar).Value = EnvProd;

                dblist.ExecuteReader(dbCommand);

            }
        }

        /// <summary>
        /// IPアドレス削除処理
        /// </summary>
        public void DeleteIPAdd(string NetAddress)
        {
            using (SqlDbInterface dblist = new SqlDbInterface())
            using (SqlCommand dbCommand = new SqlCommand())
            {
                StringBuilder query = new StringBuilder();
                query.AppendLine("delete from AuthIpAddress");
                query.AppendLine(" where NetAddress=@NetAddress");
                // クエリ発行
                dbCommand.CommandText = query.ToString();

                dbCommand.Parameters.Add("@NetAddress", SqlDbType.NVarChar).Value = NetAddress;
                dblist.ExecuteReader(dbCommand);
            }
        }
    }
    
}