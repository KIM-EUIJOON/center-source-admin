using AppSigmaAdmin.Models;
using AppSigmaAdmin.Repository.Database.Connection;
using AppSigmaAdmin.Repository.Database.Query.AbstractLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace AppSigmaAdmin.Repository.Database.Query.Showabus
{
    /// <summary>
    /// 保険付き乗車券データ
    /// </summary>
    public class InsuranceTicketRepository : AbstractShowabusBase
    {
        /// <summary>
        /// 券種ドロップダウンリスト項目取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ShowabusPaymentInfo> GetTicketList()
        {
            using (var dbInterface = SqlDbInterfaceWrapper.Create())
            using (var cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("select fsm.TicketType");
                sb.AppendLine("   ,cr.Value");
                sb.AppendLine("   ,fsm.BizCompanyCd");
                sb.AppendLine("   ,fsm.TicketId");
                sb.AppendLine("   ,fsm.TicketGroup");
                sb.AppendLine("   from FreeTicketSalesMaster fsm");
                sb.AppendLine("   left join CharacterResource cr");
                sb.AppendLine("   on fsm.TicketName = cr.ResourceId");
                sb.AppendLine("   and Language = 'ja'");
                sb.AppendLine($"   WHERE fsm.BizCompanyCd IN ({string.Join(", ", _Companies.Select(c => $"'{c.Code}'").ToArray())})");
                sb.AppendLine("   ORDER BY fsm.TicketGroup, fsm.TicketId");

                cmd.CommandText = sb.ToString();

                var dt = dbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows)
                {
                    var info = new ShowabusPaymentInfo()
                    {
                        TicketType = row["TicketType"].ToString(),
                        TransportType = row["BizCompanyCd"].ToString(),
                        TicketName = row["Value"].ToString(),
                        TicketId = row["TicketId"].ToString(),
                    };

                    if (row["TicketGroup"].ToString() == "1")
                        info.TicketName = info.TicketName + "[au]";
                    
                    yield return info;
                }
            }
        }

        /// <summary>
        /// 利用実績リスト取得(ページ指定)
        /// </summary>
        /// <param name="stDate"></param>
        /// <param name="edDate"></param>
        /// <param name="userId"></param>
        /// <param name="pageNo"></param>
        /// <param name="listNoEnd"></param>
        /// <returns></returns>
        public IEnumerable<ShowabusUsageInsuranceInfo> GetUsages(DateTime stDate, DateTime edDate, string userId, int? pageNo, int? listNoEnd)
        {
            using (var dbInterface = SqlDbInterfaceWrapper.Create())
            using (var cmd = new SqlCommand())
            {
                var sql = new StringBuilder();

                sql.AppendLine("select tbl.RecNo");
                sql.AppendLine("     , tbl.UserId");
                sql.AppendLine("     , tbl.TicketId");
                sql.AppendLine("     , tbl.SetNo");
                sql.AppendLine("     , tbl.UsageStartDatetime");
                sql.AppendLine("     , tbl.UsageEndDatetime");
                sql.AppendLine("     , tbl.InquiryId");

                sql.AppendLine("  from (");
                sql.AppendLine("       select ROW_NUMBER() OVER(ORDER BY ticket.UsageStartDatetime) as RecNo");
                sql.AppendLine("            , ticket.UserId");
                sql.AppendLine("            , ticket.TicketId");
                sql.AppendLine("            , ticket.SetNo");
                sql.AppendLine("            , ticket.UsageStartDatetime");
                sql.AppendLine("            , ticket.UsageEndDatetime");
                sql.AppendLine("            , insurance.InquiryId");

                sql.AppendLine("         from (");
                sql.AppendLine("              select ftm.UserId");
                sql.AppendLine("                   , ftm.TicketId");
                sql.AppendLine("                   , ftm.SetNo");
                sql.AppendLine("                   , ftm.UsageStartDatetime");
                sql.AppendLine("                   , ftm.UsageEndDatetime");
                sql.AppendLine("                   , ftsm.BizCompanyCd");
                sql.AppendLine("                from FreeTicketManage ftm");
                sql.AppendLine("                inner join FreeTicketSalesMaster ftsm");
                sql.AppendLine("                on ftm.TicketId = ftsm.TicketId");
                sql.AppendLine("                inner join BizUnitMaster bum");
                sql.AppendLine("                on bum.BizCompanyCd = ftsm.BizCompanyCd");
                sql.AppendLine("               where ftm.UsageStartDatetime is not null");
                sql.AppendLine($"                 and ftsm.BizCompanyCd in ({string.Join(", ", _Companies.Select(c => $"'{c.Code}'"))})");
                sql.AppendLine($"                 and bum.ServiceId in ({string.Join(", ", _Companies.Where(c => !string.IsNullOrEmpty(c.ServiceId)).Select(c => $"'{c.ServiceId}'"))})");
                sql.AppendLine("                 and (bum.DeleteFlag is null or bum.DeleteFlag = 0)");
                sql.AppendLine("              ) ticket");

                sql.AppendLine("              left join");

                sql.AppendLine("              (");
                sql.AppendLine("              select om.UserId");
                sql.AppendLine("                   , om.TicketId");
                sql.AppendLine("                   , om.TicketSetNo");
                sql.AppendLine("                   , oi.InquiryId");
                sql.AppendLine("                from OptionManage om");
                sql.AppendLine("                inner join OptionMaster oms");
                sql.AppendLine("                on oms.OptionId = om.OptionId");
                sql.AppendLine("                left join OptionInquiry oi");
                sql.AppendLine("                on oi.UserId = om.UserId");
                sql.AppendLine("                and oi.OptionId = om.OptionId");
                sql.AppendLine("                and oi.OptionSetNo = om.OptionSetNo");
                sql.AppendLine("                where (oms.DeleteFlag is null or oms.DeleteFlag = 0)");
                sql.AppendLine("              ) insurance");

                sql.AppendLine("              on insurance.UserId = ticket.UserId");
                sql.AppendLine("              and insurance.TicketId = ticket.TicketId");
                sql.AppendLine("              and insurance.TicketSetNo = ticket.SetNo");

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
                        yield return new ShowabusUsageInsuranceInfo
                        {
                            UserId = row["UserId"].ToString(),
                            UsageStartDatetime = ((DateTime)row["UsageStartDatetime"]).ToString("yyyy/MM/dd HH:mm:ss"),
                            UsageEndDatetime = ((DateTime)row["UsageEndDatetime"]).ToString("yyyy/MM/dd HH:mm:ss"),
                            InquiryId = OptionString(row["InquiryId"]),
                        };
            }

        }

        /// <summary>
        /// 利用実績リスト総数取得
        /// </summary>
        /// <param name="stDate"></param>
        /// <param name="edDate"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetUsagesMaxCount(DateTime stDate, DateTime edDate, string userId)
            => GetUsages(stDate, edDate, userId, null, null).Count();
    }
}