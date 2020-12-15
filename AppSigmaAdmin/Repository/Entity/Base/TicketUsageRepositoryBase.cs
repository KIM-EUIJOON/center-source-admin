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
    /// チケット利用状況基底クラス
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class TicketUsageRepositoryBase<TEntity> : BizCompanyRepositoryBase
    {

        protected TicketUsageRepositoryBase(params Company[] companies) : base(companies) { }

        /// <summary>
        /// DBデータ変換
        /// </summary>
        /// <param name="raw">利用実績クエリデータ</param>
        /// <returns></returns>
        protected abstract TEntity Parse(TicketUsageQueryRaw raw);

        /// <summary>
        /// 利用実績リスト取得(ページ指定)
        /// </summary>
        /// <param name="stDate"></param>
        /// <param name="edDate"></param>
        /// <param name="userId"></param>
        /// <param name="ticketId"></param>
        /// <param name="pageNo"></param>
        /// <param name="listNoEnd"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetUsages(DateTime stDate, DateTime edDate, string userId, string ticketId, int? pageNo, int? listNoEnd)
        {
            using (var dbInterface = SqlDbInterfaceWrapper.Create())
            using (var cmd = new SqlCommand())
            {
                var sql = new StringBuilder();

                sql.AppendLine("select tbl.RecNo");
                sql.AppendLine("     , tbl.UserId");
                sql.AppendLine("     , tbl.TicketId");
                sql.AppendLine("     , tbl.SetNo");
                sql.AppendLine("     , tbl.TicketName");
                sql.AppendLine("     , tbl.UsageStartDatetime");
                sql.AppendLine("     , tbl.UsageEndDatetime");
                sql.AppendLine("     , tbl.InquiryId");

                sql.AppendLine("  from (");
                sql.AppendLine("       select ROW_NUMBER() OVER(ORDER BY ticket.UsageStartDatetime) as RecNo");
                sql.AppendLine("            , ticket.UserId");
                sql.AppendLine("            , ticket.TicketId");
                sql.AppendLine("            , ticket.SetNo");
                sql.AppendLine("            , ticket.TicketName");
                sql.AppendLine("            , ticket.UsageStartDatetime");
                sql.AppendLine("            , ticket.UsageEndDatetime");
                sql.AppendLine("            , ticket.InquiryId");

                sql.AppendLine("         from (");
                sql.AppendLine("              select ftm.UserId");
                sql.AppendLine("                   , ftm.TicketId");
                sql.AppendLine("                   , ftm.SetNo");
                sql.AppendLine("                   , cr.Value as TicketName");
                sql.AppendLine("                   , ftm.UsageStartDatetime");
                sql.AppendLine("                   , ftm.UsageEndDatetime");
                sql.AppendLine("                   , ftm.InquiryId");
                sql.AppendLine("                   , ftsm.BizCompanyCd");
                sql.AppendLine("                from FreeTicketManage ftm");
                sql.AppendLine("                inner join FreeTicketSalesMaster ftsm");
                sql.AppendLine("                on ftm.TicketId = ftsm.TicketId");
                sql.AppendLine("                inner join BizUnitMaster bum");
                sql.AppendLine("                on bum.BizCompanyCd = ftsm.BizCompanyCd");
                sql.AppendLine("        	    left join CharacterResource cr");
                sql.AppendLine("        	    on ftsm.TicketName = cr.ResourceId");
                sql.AppendLine("        	    and cr.Language ='ja'");
                sql.AppendLine("               where ftm.UsageStartDatetime is not null");
                sql.AppendLine($"                 and ftsm.BizCompanyCd in ({BizCompanyCodes})");
                sql.AppendLine($"                 and bum.ServiceId in ({ServiceIds})");
                sql.AppendLine("                 and (bum.DeleteFlag is null or bum.DeleteFlag = 0)");
                sql.AppendLine("              ) ticket");

                var wheres = new List<string>();

                // 必須条件 - 利用開始日時範囲
                wheres.Add($"ticket.UsageStartDatetime between @StartDatatTime and @EndDatatTime");
                setParam4NulDateTime(cmd, "@StartDatatTime", stDate);
                setParam4NulDateTime(cmd, "@EndDatatTime", edDate);

                // 任意条件 - ユーザID
                if (userId != null)
                {
                    wheres.Add($"ticket.UserId = @UserId");
                    setParam(cmd, "@UserId", SqlDbType.Int, userId);
                }

                // 任意条件 - チケットID
                if (ticketId != "-")
                {
                    wheres.Add("ticket.TicketId = @TicketId");
                    setParam(cmd, "@TicketId", SqlDbType.NVarChar, ticketId);
                }

                if (wheres.Any())
                    sql.AppendLine($"where {string.Join(" and ", wheres.ToArray())}");

                sql.AppendLine("       ) tbl");

                // ページ指定あり
                if (pageNo.HasValue && listNoEnd.HasValue)
                {
                    sql.AppendLine("where tbl.RecNo between @PageNum and @ListEnd");
                    cmd.Parameters.Add("@PageNum", SqlDbType.Int).Value = pageNo.Value;
                    cmd.Parameters.Add("@ListEnd", SqlDbType.Int).Value = listNoEnd.Value;
                }

                cmd.CommandText = sql.ToString();

                using (var dt = dbInterface.ExecuteReader(cmd))
                    foreach (DataRow row in dt.Rows)
                        yield return Parse(new TicketUsageQueryRaw()
                        {
                            UserId = Convert.ToInt32(row["UserId"].ToString()),
                            TicketId = row["TicketId"].ToString(),
                            SetNo = Convert.ToInt32(row["SetNo"].ToString()),
                            TicketName = OptionString(row["TicketName"]),
                            UsageStartDatetime = Option<DateTime>(row["UsageStartDatetime"]),
                            UsageEndDatetime = Option<DateTime>(row["UsageEndDatetime"]),
                            InquiryId = OptionString(row["InquiryId"])
                        });
            }

        }

        /// <summary>
        /// 利用実績リスト総数取得
        /// </summary>
        /// <param name="stDate"></param>
        /// <param name="edDate"></param>
        /// <param name="userId"></param>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        public int GetUsagesMaxCount(DateTime stDate, DateTime edDate, string userId, string ticketId)
            => GetUsages(stDate, edDate, userId, ticketId, null, null).Count();
    }
}