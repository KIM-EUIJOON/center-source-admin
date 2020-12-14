using AppSigmaAdmin.Repository.Database.Connection;
using AppSigmaAdmin.Repository.Entity.Base.Models;
using AppSigmaAdmin.Repository.Entity.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace AppSigmaAdmin.Repository.Database.Entity.Base
{
    /// <summary>
    /// 販売チケット基底クラス
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class TicketSaleRespositoryBase<TEntity> : BizCompanyRepositoryBase
    {

        protected TicketSaleRespositoryBase(params Company[] companies) : base(companies) { }

        /// <summary>
        /// DBデータ変換
        /// </summary>
        /// <param name="raw">販売チケットクエリデータ</param>
        /// <returns></returns>
        protected abstract TEntity Parse(TicketSaleQueryRaw raw);

        /// <summary>
        /// 販売チケットリスト項目取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TEntity> GetTicketList()
        {
            using (var dbInterface = SqlDbInterfaceWrapper.Create())
            using (var cmd = new SqlCommand())
            {
                var sb = new StringBuilder();

                sb.AppendLine("select fsm.TicketType");
                sb.AppendLine("     , cr.Value");
                sb.AppendLine("     , fsm.BizCompanyCd");
                sb.AppendLine("     , fsm.TicketId");
                sb.AppendLine("     , fsm.TicketGroup");

                sb.AppendLine("  from FreeTicketSalesMaster fsm");
                sb.AppendLine("  left join CharacterResource cr");
                sb.AppendLine("  on fsm.TicketName = cr.ResourceId");
                sb.AppendLine("  and Language = 'ja'");
                sb.AppendLine($" WHERE fsm.BizCompanyCd IN ({BizCompanyCodes})");
                sb.AppendLine(" ORDER BY fsm.TicketGroup, fsm.TicketId");

                cmd.CommandText = sb.ToString();

                var dt = dbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows)
                    yield return Parse(new TicketSaleQueryRaw()
                    {
                        TicketId = OptionString(row["TicketId"]),
                        BizCompanyCd = OptionString(row["BizCompanyCd"]),
                        TicketType = OptionString(row["TicketType"]),
                        TicketGroup = OptionString(row["TicketGroup"]),
                        TicketName = OptionString(row["Value"]),
                    });
            }
        }
    }
}