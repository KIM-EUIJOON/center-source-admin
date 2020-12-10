using AppSigmaAdmin.Models;
using AppSigmaAdmin.Repository.Database.Connection;
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
    /// 保険付き乗車券決済データ
    /// </summary>
    public class TicketPaymentRepository : AbstractShowabusBase
    {
        /// <summary>
        /// 決済情報リスト取得
        /// </summary>
        /// <param name="stDate">抽出範囲開始日</param>
        /// <param name="edDate">抽出範囲終了日</param>
        /// <param name="userId"></param>
        /// <param name="paymentType"></param>
        /// <param name="ticketNumType"></param>
        /// <param name="transportType"></param>
        /// <param name="ticketId"></param>
        /// <param name="aplType"></param>
        /// <param name="pageNo"></param>
        /// <param name="listNoEnd"></param>
        /// <returns>西鉄決済情報</returns>
        public IEnumerable<ShowabusPaymentInfo> GetPayments(DateTime stDate, DateTime edDate, string userId, string paymentType, string ticketNumType, string transportType, string ticketId, string aplType,
            int? pageNo, int? listNoEnd)
        {
            using (var dbInterface = SqlDbInterfaceWrapper.Create())
            using (var cmd = new SqlCommand())
            {
                var sb = new StringBuilder();

                sb.AppendLine("select *");
                sb.AppendLine("  from (");

                sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY tbl.PaymentId, tbl.PaymentType) as RecNo");    //決済IDと決済種別でソートする
                sb.AppendLine("     , tbl.UserId");
                sb.AppendLine("     , tbl.TranDate");
                sb.AppendLine("     , case");
                foreach (var c in _Companies)
                    sb.AppendLine($"     when tbl.BizCompanyCd =N'{c.Code}' then N'{c.TransportName}'");
                sb.AppendLine("     else N'チケット種別不明' end as BizCompanyCd");                                  /*チケット種別(交通手段)*/
                sb.AppendLine("     , tbl.TicketType");                                                              /*チケット種別(au,au以外)*/
                sb.AppendLine("     , tbl.TicketId");
                sb.AppendLine("     , tbl.TicketGroup");
                sb.AppendLine("     , tbl.SetNo");
                sb.AppendLine("     , tbl.Value");                                                                   /*チケット名称*/
                sb.AppendLine("     , tbl.AdultNum");
                sb.AppendLine("     , tbl.ChildNum");
                sb.AppendLine("     , tbl.PaymentId");
                sb.AppendLine("     , tbl.AplType");                                                                 /*アプリ種別*/
                sb.AppendLine("     , case when tbl.PaymentType = '3' then N'即時決済'");
                sb.AppendLine("            when tbl.PaymentType = '4' then N'払戻し'");
                sb.AppendLine("            when tbl.PaymentType = '5' then N'取消'");
                sb.AppendLine("       else N'決済種別不明' end as PaymentType");
                sb.AppendLine("     , tbl.Amount");
                sb.AppendLine("     , tbl.ReceiptNo");
                sb.AppendLine("     , finance.InquiryId");

                sb.AppendLine("  from (");
                // 決済データ
                sb.AppendLine("        	select pm.TranDate");
                sb.AppendLine("        	,ftm.UserId");
                sb.AppendLine("        	,fsm.TrsType");
                sb.AppendLine("        	,N'売上' as Summary");
                sb.AppendLine("        	,fsm.TicketType");
                sb.AppendLine("        	,cr.Value");                        /*チケット名称(日本語)*/
                sb.AppendLine("        	,ftm.AdultNum");                    /*大人枚数*/
                sb.AppendLine("        	,ftm.ChildNum");                    /*子供枚数*/
                sb.AppendLine("        	,pm.PaymentId");
                sb.AppendLine("        	,pm.PaymentType");
                sb.AppendLine("        	,pm.Amount");
                sb.AppendLine("        	,pm.ReceiptNo");
                sb.AppendLine("        	,fsm.BizCompanyCd");
                sb.AppendLine("         ,fsm.TicketId");                      /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,fsm.TicketGroup");
                sb.AppendLine("        	,ftm.SetNo");
                sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                sb.AppendLine("        	from FreeTicketManage ftm");
                sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                sb.AppendLine($"         and fsm.BizCompanyCd in ({string.Join(", ", _Companies.Select(c => $"'{c.Code}'"))})");
                sb.AppendLine("        	inner join PaymentManage pm");
                sb.AppendLine("        	on ftm.UserId = pm.UserId");
                sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                sb.AppendLine($"         and pm.ServiceId in ({string.Join(", ", _Companies.Where(c => !string.IsNullOrEmpty(c.ServiceId)).Select(c => $"'{c.ServiceId}'"))})");
                sb.AppendLine("        	and pm.PaymentType = '3'");
                sb.AppendLine("        	and pm.GmoStatus = '1'");
                sb.AppendLine("        	and pm.GmoProcType = '2'");
                sb.AppendLine("        	left join CharacterResource cr");
                sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                sb.AppendLine("        	and Language ='ja'");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftm.UserId=uio.UserId");

                sb.AppendLine("        	union all");
                /*払戻し返金データ取得*/
                sb.AppendLine("        	select pm.TranDate");
                sb.AppendLine("        	,ftm.UserId");
                sb.AppendLine("        	,fsm.TrsType");
                sb.AppendLine("        	, N'売上' as Summary");
                sb.AppendLine("        	,fsm.TicketType");
                sb.AppendLine("        	,cr.Value");                        /*チケット名称(日本語)*/
                sb.AppendLine("        	,ftm.AdultNum");                    /*大人枚数*/
                sb.AppendLine("        	,ftm.ChildNum");                    /*子供枚数*/
                sb.AppendLine("        	,pm.PaymentId");
                sb.AppendLine("        	,pm.PaymentType");
                sb.AppendLine("        	,pm.Amount* -1 as Amount");
                sb.AppendLine("        	,pm.ReceiptNo");
                sb.AppendLine("        	,fsm.BizCompanyCd");
                sb.AppendLine("         ,fsm.TicketId");                      /*チケットID*/
                sb.AppendLine("         ,fsm.TicketGroup");
                sb.AppendLine("        	,ftm.SetNo");
                sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                sb.AppendLine("        	from FreeTicketManage ftm");
                sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                sb.AppendLine($"         and fsm.BizCompanyCd in ({string.Join(", ", _Companies.Select(c => $"'{c.Code}'"))})");
                sb.AppendLine("        	inner join PaymentManage pm");
                sb.AppendLine("        	on ftm.UserId = pm.UserId");
                sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                sb.AppendLine($"         and pm.ServiceId in ({string.Join(", ", _Companies.Where(c => !string.IsNullOrEmpty(c.ServiceId)).Select(c => $"'{c.ServiceId}'"))})");
                sb.AppendLine("        	and pm.PaymentType = '5'");
                sb.AppendLine("        	and pm.GmoStatus = '1'");
                sb.AppendLine("        	and pm.GmoProcType = '3'");
                sb.AppendLine("        	left join CharacterResource cr");
                sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                sb.AppendLine("        	and Language ='ja'");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftm.UserId=uio.UserId");

                sb.AppendLine("        	union all");
                /*払戻し手数料取得*/
                sb.AppendLine("        	select pm.TranDate");
                sb.AppendLine("        	,ftm.UserId");
                sb.AppendLine("        	,fsm.TrsType");
                sb.AppendLine("        	,N'払戻し' as Summary");
                sb.AppendLine("        	,fsm.TicketType");
                sb.AppendLine("        	,cr.Value");                        /*チケット名称*/
                sb.AppendLine("        	,ftm.AdultNum ");                   /*大人枚数*/
                sb.AppendLine("        	,ftm.ChildNum ");                   /*子供枚数*/
                sb.AppendLine("        	,pm.PaymentId");
                sb.AppendLine("        	,pm.PaymentType");
                sb.AppendLine("        	,pm.Amount");
                sb.AppendLine("        	,pm.ReceiptNo");
                sb.AppendLine("        	,fsm.BizCompanyCd");
                sb.AppendLine("         ,fsm.TicketId");                      /*チケットID*/
                sb.AppendLine("         ,fsm.TicketGroup");
                sb.AppendLine("        	,ftm.SetNo");
                sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                sb.AppendLine("        	from FreeTicketManage ftm");
                sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                sb.AppendLine($"         and fsm.BizCompanyCd in ({string.Join(", ", _Companies.Select(c => $"'{c.Code}'"))})");
                sb.AppendLine("        	inner join PaymentManage pm");
                sb.AppendLine("        	on ftm.UserId = pm.UserId");
                sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                sb.AppendLine($"         and pm.ServiceId in ({string.Join(", ", _Companies.Where(c => !string.IsNullOrEmpty(c.ServiceId)).Select(c => $"'{c.ServiceId}'"))})");
                sb.AppendLine("        	and pm.PaymentType = '4'");
                sb.AppendLine("        	and pm.GmoStatus = '1'");
                sb.AppendLine("        	and pm.GmoProcType = '2'");
                sb.AppendLine("        	left join CharacterResource cr");
                sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                sb.AppendLine("        	and Language ='ja'");
                sb.AppendLine("          left join UserInfoOid uio");
                sb.AppendLine("          on ftm.UserId=uio.UserId");
                sb.AppendLine("       ) tbl");

                // 乗車券オプション(決済データに外部結合)
                sb.AppendLine("       left join");

                sb.AppendLine("       (");
                sb.AppendLine("         select om.UserId");
                sb.AppendLine("              , om.TicketId");
                sb.AppendLine("              , om.TicketSetNo");
                sb.AppendLine("              , oi.InquiryId");
                sb.AppendLine("           from OptionManage om");
                sb.AppendLine("           left join OptionInquiry oi");
                sb.AppendLine("           on om.UserId = oi.UserId");
                sb.AppendLine("           and om.OptionId = oi.OptionId");
                sb.AppendLine("           and om.OptionSetNo = oi.OptionSetNo");
                sb.AppendLine("       ) finance");
                sb.AppendLine("       on finance.UserId = tbl.UserId");
                sb.AppendLine("       and finance.TicketId = tbl.TicketId");
                sb.AppendLine("       and finance.TicketSetNo = tbl.SetNo");

                var wheres = new List<string>();

                // 必須条件 - 決済エラー分は含めない
                wheres.Add(new StringBuilder()
                    .AppendLine("not exists(")
                    .AppendLine("select 1")
                    .AppendLine("  from PaymentError pe")
                    .AppendLine(" where pe.UserId = tbl.UserId")
                    .AppendLine("   and pe.PaymentId = tbl.PaymentId")
                    .AppendLine("   and pe.PaymentType = tbl.PaymentType")
                    .AppendLine($"   and pe.ServiceId in ({string.Join(", ", _Companies.Where(c => !string.IsNullOrEmpty(c.ServiceId)).Select(c => $"'{c.ServiceId}'"))})")
                    .AppendLine("   and pe.IsTreat = 0") // 運用未処置
                    .AppendLine(")")
                    .ToString()
                );

                // 必須条件 - 決済日範囲
                wheres.Add("tbl.TranDate between @StartDatatTime and @EndDatatTime");
                setParam4NulDateTime(cmd, "@StartDatatTime", stDate);
                setParam4NulDateTime(cmd, "@EndDatatTime", edDate);

                // 任意条件 - ユーザID
                if (!string.IsNullOrEmpty(userId))
                {
                    wheres.Add("tbl.UserId = @UserId");
                    cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = userId;
                }

                // 任意条件 - 事業体コード
                if (transportType != "-")
                {
                    wheres.Add("tbl.BizCompanyCd = @TransportType");
                    cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = transportType;
                }

                // 任意条件 - 決済種別
                if (paymentType == "決済種別不明")
                {
                    //検索条件に決済種別：決済種別不明指定
                    wheres.Add("tbl.PaymentType not in ('3','4','5')");
                }
                else if (paymentType != "-")
                {
                    //検索条件に決済種別指定
                    wheres.Add("tbl.PaymentType = @PaymentType");
                    cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = paymentType;
                }

                // 任意条件 - 枚数種別
                if (ticketNumType == "大人")
                {
                    wheres.Add("tbl.ChildNum = '0'"); // 大人を指定: 子供が0枚
                }
                else if (ticketNumType == "子供")
                {
                    wheres.Add("tbl.AdultNum = '0'"); // 子供を指定: 大人が0枚
                }

                // 任意条件 - チケットID
                if (ticketId != "-")
                {
                    wheres.Add("tbl.TicketId = @TicketId");
                    cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = ticketId;
                }

                // 任意条件 - アプリ種別
                if (aplType != "-")
                {
                    wheres.Add("tbl.AplType = @AplType");
                    cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = aplType;
                }

                if (wheres.Any())
                    sb.AppendLine($"where {string.Join(" and ", wheres.ToArray())}");

                sb.AppendLine("       ) MA");

                // ページ指定あり
                if (pageNo.HasValue && listNoEnd.HasValue)
                {
                    sb.AppendLine("where MA.RecNo between @PageNum and @ListEnd");
                    cmd.Parameters.Add("@PageNum", SqlDbType.Int).Value = pageNo.Value;
                    cmd.Parameters.Add("@ListEnd", SqlDbType.Int).Value = listNoEnd.Value;
                }


                cmd.CommandText = sb.ToString();

                using (var dt = dbInterface.ExecuteReader(cmd))
                    foreach (DataRow row in dt.Rows)
                    {
                        var infoN = new ShowabusPaymentInfo
                        {
                            UserId = row["UserId"].ToString(),
                            TranDatetime = ((DateTime)row["TranDate"]).ToString("yyyy/MM/dd HH:mm:ss"),
                            PaymentId = row["PaymentId"].ToString(),
                            TicketName = row["Value"].ToString(),
                            TicketId = row["TicketId"].ToString(),
                            TransportType = row["BizCompanyCd"].ToString(),
                            AdultNum = row["AdultNum"].ToString(),
                            ChildNum = row["ChildNum"].ToString(),
                            PaymentType = row["PaymentType"].ToString(),
                            Amount = (int)row["Amount"],
                            ReceiptNo = row["ReceiptNo"].ToString(),
                            InquiryId = row["InquiryId"] == DBNull.Value ? null : row["InquiryId"].ToString(),
                            Apltype = row["AplType"].ToString()
                        };
                        if (row["TicketGroup"].ToString() == "1")
                        {
                            infoN.TicketName = infoN.TicketName + "[au]";
                        }

                        if (row["AplType"].ToString() == "1")
                        {
                            infoN.Apltype = "au";
                        }
                        else
                        {
                            infoN.Apltype = "-";
                        }

                        yield return infoN;
                    }

            }

        }

        /// <summary>
        /// 表示用決済情報リスト総数取得
        /// </summary>
        /// <param name="stDate"></param>
        /// <param name="edDate"></param>
        /// <param name="userId"></param>
        /// <param name="paymentType"></param>
        /// <param name="ticketNumType"></param>
        /// <param name="transportType"></param>
        /// <param name="ticketId"></param>
        /// <param name="aplType"></param>
        /// <returns></returns>
        public int GetPaymentsMaxCount(DateTime stDate, DateTime edDate, string userId, string paymentType, string ticketNumType, string transportType, string ticketId, string aplType)
            => GetPayments(stDate, edDate, userId, paymentType, ticketNumType, transportType, ticketId, aplType, null, null).Count();
    }
}