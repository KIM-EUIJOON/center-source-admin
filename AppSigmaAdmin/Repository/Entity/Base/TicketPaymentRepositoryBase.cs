using AppSigmaAdmin.Contstants;
using AppSigmaAdmin.Contstants.Extensions;
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
    /// チケット決済情報基底クラス
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class TicketPaymentRepositoryBase<TEntity> : BizCompanyRepositoryBase
    {

        protected TicketPaymentRepositoryBase(params Company[] companies) : base(companies) { }

        /// <summary>
        /// DBデータ変換
        /// </summary>
        /// <param name="raw">決済情報クエリデータ</param>
        /// <returns></returns>
        protected abstract TEntity Parse(TicketPaymentQueryRaw raw);

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
        public IEnumerable<TEntity> GetPayments(DateTime stDate, DateTime edDate, string userId, string paymentType, string ticketNumType, string transportType, string ticketId, string aplType,
            int? pageNo, int? listNoEnd)
        {

            using (var dbInterface = SqlDbInterfaceWrapper.Create())
            using (var cmd = new SqlCommand())
            {
                var sb = new StringBuilder();

                sb.AppendLine("select MA.RecNo");
                sb.AppendLine("      ,MA.UserId");
                sb.AppendLine("      ,MA.TranDate");
                sb.AppendLine("      ,MA.BizCompanyCd");
                sb.AppendLine("      ,MA.TicketType");
                sb.AppendLine("      ,MA.TicketId");
                sb.AppendLine("      ,MA.TicketGroup");
                sb.AppendLine("      ,MA.SetNo");
                sb.AppendLine("      ,MA.Value");
                sb.AppendLine("      ,MA.AdultNum");
                sb.AppendLine("      ,MA.ChildNum");
                sb.AppendLine("      ,MA.PaymentId");
                sb.AppendLine("      ,MA.AplType");
                sb.AppendLine("      ,MA.PaymentType");
                sb.AppendLine("      ,MA.Amount");
                sb.AppendLine("      ,MA.ForwardCode");
                sb.AppendLine("      ,MA.ReceiptNo");
                sb.AppendLine("      ,MA.PaymentMeansCode");
                sb.AppendLine("      ,MA.PaymentDetailCode");
                sb.AppendLine("      ,MA.PaymentName");
                sb.AppendLine("      ,MA.PaymentDetailName");
                sb.AppendLine("      ,MA.InquiryId");

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
                sb.AppendLine("     , case");
                foreach (var p in PaymentType.Defines)
                    sb.AppendLine($"       when tbl.PaymentType = '{p.Value}' then N'{p.Name}'");
                sb.AppendLine($"       else N'{PaymentType.Unknown.Name}' end as PaymentType");
                sb.AppendLine("     , tbl.Amount");
                sb.AppendLine("     , tbl.ForwardCode");
                sb.AppendLine("     , tbl.ReceiptNo");
                sb.AppendLine("     , tbl.PaymentMeansCode");
                sb.AppendLine("     , tbl.PaymentDetailCode");
                sb.AppendLine("     , tbl.PaymentName");
                sb.AppendLine("     , tbl.PaymentDetailName");
                sb.AppendLine("     , tbl.InquiryId");

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
                sb.AppendLine("        	,ftm.InquiryId");                   /*問い合わせID*/
                sb.AppendLine("        	,pm.PaymentId");
                sb.AppendLine("        	,pm.PaymentType");
                sb.AppendLine("        	,pm.Amount");
                sb.AppendLine("        	,pm.ForwardCode");
                sb.AppendLine("        	,pm.ReceiptNo");
                sb.AppendLine("        	,pm.PaymentMeansCode");
                sb.AppendLine("        	,pm.PaymentDetailCode");

                sb.AppendLine("        	,paym.PaymentName");
                sb.AppendLine("        	,paym.PaymentDetailName");

                sb.AppendLine("        	,fsm.BizCompanyCd");
                sb.AppendLine("         ,fsm.TicketId");                      /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,fsm.TicketGroup");
                sb.AppendLine("        	,ftm.SetNo");
                sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                sb.AppendLine("        	from FreeTicketManage ftm");
                sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                sb.AppendLine($"         and fsm.BizCompanyCd in ({BizCompanyCodes})");
                sb.AppendLine("        	inner join PaymentManage pm");
                sb.AppendLine("        	on ftm.UserId = pm.UserId");
                sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                sb.AppendLine($"         and pm.ServiceId in ({ServiceIds})");
                sb.AppendLine($"         and pm.PaymentType = '{PaymentType.Settle.Value}'");
                sb.AppendLine("        	and pm.GmoStatus = '1'");
                sb.AppendLine("        	and pm.GmoProcType = '2'");

                sb.AppendLine("         left join");
                sb.AppendLine("         (");
                sb.AppendLine("             select pmm.PaymentMeansCode");
                sb.AppendLine("                  , pmdm.PaymentDetailCode");
                sb.AppendLine("                  , crm.Value as PaymentName");
                sb.AppendLine("                  , crd.Value as PaymentDetailName");
                sb.AppendLine("               from PaymentMeansMaster pmm");
                sb.AppendLine("               left join CharacterResource crm");
                sb.AppendLine("               on pmm.PaymentTitle = crm.ResourceId");
                sb.AppendLine("               and crm.Language = 'ja'");
                sb.AppendLine("               left join PaymentMeansDetailMaster pmdm");
                sb.AppendLine("               on pmm.PaymentMeansCode = pmdm.PaymentMeansCode");
                sb.AppendLine("               left join CharacterResource crd");
                sb.AppendLine("               on pmdm.PaymentDetailName = crd.ResourceId");
                sb.AppendLine("               and crd.Language = 'ja'");
                sb.AppendLine("         ) paym");
                sb.AppendLine("         on paym.PaymentMeansCode = pm.PaymentMeansCode");
                sb.AppendLine("         and (pm.PaymentDetailCode is null or paym.PaymentDetailCode = pm.PaymentDetailCode)");

                sb.AppendLine("        	left join CharacterResource cr");
                sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                sb.AppendLine("        	and Language ='ja'");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftm.UserId=uio.UserId");

                sb.AppendLine("        	union all");
                /*決済取消データ取得*/
                sb.AppendLine("        	select pm.TranDate");
                sb.AppendLine("        	,ftm.UserId");
                sb.AppendLine("        	,fsm.TrsType");
                sb.AppendLine("        	, N'売上' as Summary");
                sb.AppendLine("        	,fsm.TicketType");
                sb.AppendLine("        	,cr.Value");                        /*チケット名称(日本語)*/
                sb.AppendLine("        	,ftm.AdultNum");                    /*大人枚数*/
                sb.AppendLine("        	,ftm.ChildNum");                    /*子供枚数*/
                sb.AppendLine("        	,ftm.InquiryId");                   /*問い合わせID*/
                sb.AppendLine("        	,pm.PaymentId");
                sb.AppendLine("        	,pm.PaymentType");
                sb.AppendLine("        	,pm.Amount* -1 as Amount");
                sb.AppendLine("        	,pm.ForwardCode");
                sb.AppendLine("        	,pm.ReceiptNo");
                sb.AppendLine("        	,pm.PaymentMeansCode");
                sb.AppendLine("        	,pm.PaymentDetailCode");

                sb.AppendLine("        	,paym.PaymentName");
                sb.AppendLine("        	,paym.PaymentDetailName");

                sb.AppendLine("        	,fsm.BizCompanyCd");
                sb.AppendLine("         ,fsm.TicketId");                      /*チケットID*/
                sb.AppendLine("         ,fsm.TicketGroup");
                sb.AppendLine("        	,ftm.SetNo");
                sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                sb.AppendLine("        	from FreeTicketManage ftm");
                sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                sb.AppendLine($"         and fsm.BizCompanyCd in ({BizCompanyCodes})");
                sb.AppendLine("        	inner join PaymentManage pm");
                sb.AppendLine("        	on ftm.UserId = pm.UserId");
                sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                sb.AppendLine($"         and pm.ServiceId in ({ServiceIds})");
                sb.AppendLine($"         and pm.PaymentType = '{PaymentType.Cancel.Value}'");
                sb.AppendLine("        	and pm.GmoStatus = '1'");
                sb.AppendLine("        	and pm.GmoProcType = '3'");

                sb.AppendLine("         left join");
                sb.AppendLine("         (");
                sb.AppendLine("             select pmm.PaymentMeansCode");
                sb.AppendLine("                  , pmdm.PaymentDetailCode");
                sb.AppendLine("                  , crm.Value as PaymentName");
                sb.AppendLine("                  , crd.Value as PaymentDetailName");
                sb.AppendLine("               from PaymentMeansMaster pmm");
                sb.AppendLine("               left join CharacterResource crm");
                sb.AppendLine("               on pmm.PaymentTitle = crm.ResourceId");
                sb.AppendLine("               and crm.Language = 'ja'");
                sb.AppendLine("               left join PaymentMeansDetailMaster pmdm");
                sb.AppendLine("               on pmm.PaymentMeansCode = pmdm.PaymentMeansCode");
                sb.AppendLine("               left join CharacterResource crd");
                sb.AppendLine("               on pmdm.PaymentDetailName = crd.ResourceId");
                sb.AppendLine("               and crd.Language = 'ja'");
                sb.AppendLine("         ) paym");
                sb.AppendLine("         on paym.PaymentMeansCode = pm.PaymentMeansCode");
                sb.AppendLine("         and (pm.PaymentDetailCode is null or paym.PaymentDetailCode = pm.PaymentDetailCode)");

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
                sb.AppendLine("        	,ftm.InquiryId");                   /*問い合わせID*/
                sb.AppendLine("        	,pm.PaymentId");
                sb.AppendLine("        	,pm.PaymentType");
                sb.AppendLine("        	,pm.Amount");
                sb.AppendLine("        	,pm.ForwardCode");
                sb.AppendLine("        	,pm.ReceiptNo");
                sb.AppendLine("        	,pm.PaymentMeansCode");
                sb.AppendLine("        	,pm.PaymentDetailCode");

                sb.AppendLine("        	,paym.PaymentName");
                sb.AppendLine("        	,paym.PaymentDetailName");

                sb.AppendLine("        	,fsm.BizCompanyCd");
                sb.AppendLine("         ,fsm.TicketId");                      /*チケットID*/
                sb.AppendLine("         ,fsm.TicketGroup");
                sb.AppendLine("        	,ftm.SetNo");
                sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                sb.AppendLine("        	from FreeTicketManage ftm");
                sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                sb.AppendLine($"         and fsm.BizCompanyCd in ({BizCompanyCodes})");
                sb.AppendLine("        	inner join PaymentManage pm");
                sb.AppendLine("        	on ftm.UserId = pm.UserId");
                sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                sb.AppendLine($"         and pm.ServiceId in ({ServiceIds})");
                sb.AppendLine($"         and pm.PaymentType = '{PaymentType.Refund.Value}'");
                sb.AppendLine("        	and pm.GmoStatus = '1'");
                sb.AppendLine("        	and pm.GmoProcType = '2'");

                sb.AppendLine("         left join");
                sb.AppendLine("         (");
                sb.AppendLine("             select pmm.PaymentMeansCode");
                sb.AppendLine("                  , pmdm.PaymentDetailCode");
                sb.AppendLine("                  , crm.Value as PaymentName");
                sb.AppendLine("                  , crd.Value as PaymentDetailName");
                sb.AppendLine("               from PaymentMeansMaster pmm");
                sb.AppendLine("               left join CharacterResource crm");
                sb.AppendLine("               on pmm.PaymentTitle = crm.ResourceId");
                sb.AppendLine("               and crm.Language = 'ja'");
                sb.AppendLine("               left join PaymentMeansDetailMaster pmdm");
                sb.AppendLine("               on pmm.PaymentMeansCode = pmdm.PaymentMeansCode");
                sb.AppendLine("               left join CharacterResource crd");
                sb.AppendLine("               on pmdm.PaymentDetailName = crd.ResourceId");
                sb.AppendLine("               and crd.Language = 'ja'");
                sb.AppendLine("         ) paym");
                sb.AppendLine("         on paym.PaymentMeansCode = pm.PaymentMeansCode");
                sb.AppendLine("         and (pm.PaymentDetailCode is null or paym.PaymentDetailCode = pm.PaymentDetailCode)");

                sb.AppendLine("        	left join CharacterResource cr");
                sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                sb.AppendLine("        	and Language ='ja'");
                sb.AppendLine("          left join UserInfoOid uio");
                sb.AppendLine("          on ftm.UserId=uio.UserId");
                sb.AppendLine("       ) tbl");

                var wheres = new List<string>();

                // 必須条件 - 決済エラー分は含めない
                wheres.Add(new StringBuilder()
                    .AppendLine("not exists(")
                    .AppendLine("select 1")
                    .AppendLine("  from PaymentError pe")
                    .AppendLine(" where pe.UserId = tbl.UserId")
                    .AppendLine("   and pe.PaymentId = tbl.PaymentId")
                    .AppendLine("   and pe.PaymentType = tbl.PaymentType")
                    .AppendLine($"   and pe.ServiceId in ({ServiceIds})")
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
                if (paymentType == PaymentType.Unknown.Value)
                {
                    //検索条件に決済種別：決済種別不明指定(定義された決済種別以外)
                    wheres.Add($"tbl.PaymentType not in ({PaymentType.Defines.JoinDefines(",", c => $"'{c.Value}'")})");
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
                        yield return Parse(new TicketPaymentQueryRaw()
                        {
                            UserId = Convert.ToInt32(row["UserId"].ToString()),
                            TranDate = Option<DateTime>(row["TranDate"]),
                            BizCompanyCd = OptionString(row["BizCompanyCd"]),
                            TicketType = OptionString(row["TicketType"]),
                            TicketId = OptionString(row["TicketId"]),
                            TicketGroup = OptionString(row["TicketGroup"]),
                            SetNo = Convert.ToInt32(row["SetNo"].ToString()),
                            Value = OptionString(row["Value"]),
                            AdultNum = Convert.ToInt32(row["AdultNum"].ToString()),
                            ChildNum = Convert.ToInt32(row["ChildNum"].ToString()),
                            PaymentId = Convert.ToInt32(row["PaymentId"]),
                            AplType = OptionString(row["AplType"]),
                            PaymentType = OptionString(row["PaymentType"]),
                            Amount = Option<int>(row["Amount"]),
                            ForwardCode = OptionString(row["ForwardCode"]),
                            ReceiptNo = OptionString(row["ReceiptNo"]),
                            PaymentMeansCode = OptionString(row["PaymentMeansCode"]),
                            PaymentDetailCode = OptionString(row["PaymentDetailCode"]),
                            PaymentName = OptionString(row["PaymentName"]),
                            PaymentDetailName = OptionString(row["PaymentDetailName"]),
                            InquiryId = OptionString(row["InquiryId"]),
                        });

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