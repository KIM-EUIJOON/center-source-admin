using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
                query.AppendLine("WHERE IsEnvDevelop = 1");

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
}