using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;
using AppSigmaAdmin.ResponseData;

namespace AppSigmaAdmin.Models
{
    public class MiyakohInfoModels
    {
        /// <summary>
        /// 宮交決済情報取得
        /// </summary>
        public class MiyakohPaymentModel
        {
            /// <summary>
            /// JR九州販売チケット一覧取得
            /// </summary>
            /// <returns></returns>
            public List<MiyakohPaymentInfoListEntity> MiyakohPassportList(string UserId)
            {
                List<MiyakohPaymentInfoListEntity> result = new List<MiyakohPaymentInfoListEntity>();
                using (SqlDbInterface NTdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("select fsm.TicketType");
                    sb.AppendLine("   ,cr.Value");
                    sb.AppendLine("   ,fsm.BizCompanyCd");
                    sb.AppendLine("   ,fsm.TicketId");
                    sb.AppendLine("   ,fsm.TicketGroup");
                    sb.AppendLine("   ,fsm.TrsType");
                    sb.AppendLine("   from FreeTicketSalesMaster fsm");
                    sb.AppendLine("   left join CharacterResource cr");
                    sb.AppendLine("   on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("   and Language = 'ja'");
                    sb.AppendLine("   where fsm.BizCompanyCd='MYZ'");//宮交のみに制限する

                    cmd.CommandText = sb.ToString();

                    DataTable dt = NTdbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        MiyakohPaymentInfoListEntity info = new MiyakohPaymentInfoListEntity
                        {
                            TicketType = row["TicketType"].ToString(),
                            TransportType = row["TrsType"].ToString(),
                            TicketName = row["Value"].ToString(),
                            TicketId = row["TicketId"].ToString(),
                        };
                        if (row["TicketGroup"].ToString() == "1")
                        {
                            info.TicketName = info.TicketName + "[au]";
                        }
                        result.Add(info);
                    }
                    return result;
                }
            }
            /// <summary>
            /// JR九州の決済情報リスト取得
            /// </summary>
            /// <param name="stDate">抽出範囲開始日</param>
            /// <param name="edDate">抽出範囲終了日</param>
            /// <returns>JR九州決済情報</returns>
            public List<MiyakohPaymentInfoListEntity> GetMiyakohPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string MyrouteNo, string PaymentType, string TicketNumType, string TransportType, string TicketId, string AplType)
            {
                List<MiyakohPaymentInfoListEntity> result = new List<MiyakohPaymentInfoListEntity>();

                using (SqlDbInterface MiyakohdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder Miyakohsb = new StringBuilder();
                    string MiyakohInfo = GetALLGetMiyakohPaymentDateQuery(stDate, edDate);
                    Miyakohsb.AppendLine("select * from (" + MiyakohInfo.ToString() + "");

                    if (MyrouteNo != "")
                    {
                        //検索条件にMyrouteID指定
                        Miyakohsb.AppendLine("   and tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                    }
                    if (TransportType != "-")
                    {
                        //検索条件に券種指定
                        Miyakohsb.AppendLine("   and tbl.TrsType = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                    }
                    if (PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        Miyakohsb.AppendLine("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    else if (PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        Miyakohsb.AppendLine("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        Miyakohsb.AppendLine("   and tbl.ChildNum = '0' ");
                        Miyakohsb.AppendLine("   and tbl.discountNum is null or tbl.discountNum in ('0') ");
                    }
                    else if (TicketNumType == "子供")
                    {
                        //検索条件に枚数種別：子供
                        Miyakohsb.AppendLine("   and tbl.AdultNum = '0' ");
                        Miyakohsb.AppendLine("   and tbl.discountNum is null or tbl.discountNum in ('0') ");
                    }
                    else if (TicketNumType == "学割")
                    {
                        //検索条件に枚数種別：子供
                        Miyakohsb.AppendLine("   and tbl.AdultNum = '0' ");
                        Miyakohsb.AppendLine("   and tbl.ChildNum = '0' ");
                    }
                    if (TicketId != "-")
                    {
                        //検索条件に券種指定
                        Miyakohsb.AppendLine("   and tbl.TicketId = @TicketId ");
                        cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                    }
                    if (AplType != "-")//au用Role番号判定
                    {
                        Miyakohsb.AppendLine("   and tbl.AplType = @AplType");
                        cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = '1';
                    }

                    Miyakohsb.AppendLine("  ) as MA  where MA.RecNo between @PageNum and @ListEnd");

                    cmd.CommandText = Miyakohsb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");
                    cmd.Parameters.Add("@PageNum", SqlDbType.NVarChar).Value = pageNo;
                    cmd.Parameters.Add("@ListEnd", SqlDbType.NVarChar).Value = ListNoEnd;

                    DataTable dt = MiyakohdbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        MiyakohPaymentInfoListEntity infoN = new MiyakohPaymentInfoListEntity
                        {
                            UserId = row["UserId"].ToString(),
                            TranDatetime = ((DateTime)row["TranDate"]).ToString("yyyy/MM/dd HH:mm:ss"),
                            PaymentId = row["PaymentId"].ToString(),
                            TicketName = row["Value"].ToString(),
                            TicketId = row["TicketId"].ToString(),
                            TransportType = row["TrsType"].ToString(),
                            AdultNum = row["AdultNum"].ToString(),
                            ChildNum = row["ChildNum"].ToString(),
                            PaymentType = row["PaymentType"].ToString(),
                            Amount = (int)row["Amount"],
                            ReceiptNo = row["ReceiptNo"].ToString(),
                            Apltype = row["AplType"].ToString()
                        };
                        if (false == string.IsNullOrEmpty((row["discountNum"].ToString())))
                        {
                            infoN.discountNum = row["discountNum"].ToString();
                        }
                        else
                        {
                            infoN.discountNum = "-";
                        }
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

                        result.Add(infoN);
                    }
                    return result;
                }
            }

            /// <summary>
            /// JR九州取得情報一覧
            /// </summary>
            /// <param name="stDate"></param>
            /// <param name="edDate"></param>
            /// <returns></returns>
            private string GetALLGetMiyakohPaymentDateQuery(DateTime stDate, DateTime edDate)
            {
                using (SqlDbInterface MiyakohdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    //string PaymentList = GetNishitetsuPaymentList();
                    //string BusPayment = NishitetsuBusPayment();

                    sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY tbl.PaymentId, tbl.PaymentType) as RecNo");    //決済IDと決済種別でソートする
                    sb.AppendLine("     , tbl.UserId");
                    sb.AppendLine("     , tbl.TranDate");
                    sb.AppendLine("     , case when tbl.TrsType =N'11' then N'バス'");
                    sb.AppendLine("     else N'チケット種別不明' end as TrsType");                                  /*チケット種別(交通手段)*/
                    sb.AppendLine("     , tbl.TicketType");                                                              /*チケット種別(au,au以外)*/
                    sb.AppendLine("     , tbl.TicketId");
                    sb.AppendLine("     , tbl.TicketGroup");
                    sb.AppendLine("     , tbl.Value");                                                                   /*チケット名称*/
                    sb.AppendLine("     , tbl.AdultNum");
                    sb.AppendLine("     , tbl.discountNum");                                                            /*現在未反映項目(学割)のため仮置き(2020/9/28)*/
                    sb.AppendLine("     , tbl.ChildNum");
                    sb.AppendLine("     , tbl.PaymentId");
                    sb.AppendLine("     , tbl.AplType");                                                                /*アプリ種別*/
                    sb.AppendLine("     , case when tbl.PaymentType = '3' then N'即時決済'");
                    sb.AppendLine("           when tbl.PaymentType = '4' then N'払戻し'");
                    sb.AppendLine("           when tbl.PaymentType = '5' then N'取消'");
                    sb.AppendLine("           else N'決済種別不明' end as PaymentType");
                    sb.AppendLine("     , tbl.Amount");
                    sb.AppendLine("     , tbl.ReceiptNo");
                    sb.AppendLine("  from (");
                    /* 即時決済データ取得*/
                    sb.AppendLine("	 select pm.TranDate");
                    sb.AppendLine("	,ftm.UserId");
                    sb.AppendLine("	,fsm.TrsType");
                    sb.AppendLine("	, N'売上' as Summary");
                    sb.AppendLine("	,fsm.TicketType");
                    sb.AppendLine("	,cr.Value");                        /*チケット名称(日本語)*/
                    sb.AppendLine("	, ftm.AdultNum");                   /*大人枚数*/
                    sb.AppendLine("	, ftm.discountNum");                   /*学割枚数(仮置き)*/
                    sb.AppendLine("	, ftm.ChildNum");                   /*子供枚数*/
                    sb.AppendLine("	, pm.PaymentId");
                    sb.AppendLine("	, pm.PaymentType");
                    sb.AppendLine("	, pm.Amount");
                    sb.AppendLine("	, pm.ReceiptNo");
                    sb.AppendLine("	,fsm.BizCompanyCd");
                    sb.AppendLine("	 ,fsm.TicketId");                      /*チケット種別(au,au以外)*/
                    sb.AppendLine("	 ,fsm.TicketGroup");
                    sb.AppendLine("	 ,uio.AplType");                       /*アプリ種別*/
                    sb.AppendLine("	from FreeTicketManage ftm");
                    sb.AppendLine("	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("	and (fsm.BizCompanyCd='MYZ')");
                    sb.AppendLine("	inner join PaymentManage pm");
                    sb.AppendLine("	on ftm.UserId = pm.UserId");
                    sb.AppendLine("	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("	and pm.ServiceId = '17'");/*サービスID(西鉄バス(福岡):2,鉄道:4,西鉄バス(北九州):5,にしてつグループ:6,宮交:17)*/
                    sb.AppendLine("	and pm.PaymentType = '3'");
                    sb.AppendLine("	and pm.GmoStatus = '1'");
                    sb.AppendLine("	and pm.GmoProcType = '2'");
                    sb.AppendLine("	left join CharacterResource cr");
                    sb.AppendLine("	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("	and Language ='ja'");
                    sb.AppendLine("	  left join UserInfoOid uio");
                    sb.AppendLine("	  on ftm.UserId=uio.UserId");
                    sb.AppendLine("	union all");
                    /*払戻し返金データ取得*/
                    sb.AppendLine("	select pm.TranDate");
                    sb.AppendLine("	,ftm.UserId");
                    sb.AppendLine("	,fsm.TrsType");
                    sb.AppendLine("	, N'売上' as Summary");
                    sb.AppendLine("	,fsm.TicketType");
                    sb.AppendLine("	,cr.Value");        /*チケット名称(日本語)*/
                    sb.AppendLine("	, ftm.AdultNum");                           /*大人枚数*/
                    sb.AppendLine("	, ftm.discountNum");                   /*学割枚数(仮置き)*/
                    sb.AppendLine("	, ftm.ChildNum");                           /*子供枚数*/
                    sb.AppendLine("	, pm.PaymentId");
                    sb.AppendLine("	, pm.PaymentType");
                    sb.AppendLine("	, pm.Amount* -1 as Amount");
                    sb.AppendLine("	, pm.ReceiptNo");
                    sb.AppendLine("	,fsm.BizCompanyCd");
                    sb.AppendLine("	 ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("	 ,fsm.TicketGroup");
                    sb.AppendLine("	 ,uio.AplType");                       /*アプリ種別*/
                    sb.AppendLine("	from FreeTicketManage ftm");
                    sb.AppendLine("	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("	and fsm.BizCompanyCd='MYZ'");
                    sb.AppendLine("	inner join PaymentManage pm");
                    sb.AppendLine("	on ftm.UserId = pm.UserId");
                    sb.AppendLine("	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("	and pm.ServiceId = '17'");/*サービスID(西鉄バス(福岡):2,鉄道:4,西鉄バス(北九州):5,にしてつグループ:6,宮交:17)*/
                    sb.AppendLine("	and pm.PaymentType = '5'");
                    sb.AppendLine("	and pm.GmoStatus = '1'");
                    sb.AppendLine("	and pm.GmoProcType = '3'");
                    sb.AppendLine("	left join CharacterResource cr");
                    sb.AppendLine("	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("	and Language ='ja'");
                    sb.AppendLine("	  left join UserInfoOid uio");
                    sb.AppendLine("	  on ftm.UserId=uio.UserId");
                    sb.AppendLine("	union all");
                    /*払戻し手数料取得*/
                    sb.AppendLine("	select pm.TranDate");
                    sb.AppendLine("	,ftm.UserId");
                    sb.AppendLine("	,fsm.TrsType");
                    sb.AppendLine("	, N'払戻し' as Summary");
                    sb.AppendLine("	,fsm.TicketType");
                    sb.AppendLine("	,cr.Value");                        /*チケット名称*/
                    sb.AppendLine("	, ftm.AdultNum");                   /*大人枚数*/
                    sb.AppendLine("	, ftm.discountNum");                   /*学割枚数(仮置き)*/
                    sb.AppendLine("	, ftm.ChildNum");                   /*子供枚数*/
                    sb.AppendLine("	, pm.PaymentId");
                    sb.AppendLine("	, pm.PaymentType");
                    sb.AppendLine("	, pm.Amount");
                    sb.AppendLine("	, pm.ReceiptNo");
                    sb.AppendLine("	,fsm.BizCompanyCd");
                    sb.AppendLine("	 ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("	 ,fsm.TicketGroup");
                    sb.AppendLine("	 ,uio.AplType");                       /*アプリ種別*/
                    sb.AppendLine("	from FreeTicketManage ftm");
                    sb.AppendLine("	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("	and (fsm.BizCompanyCd='MYZ')");
                    sb.AppendLine("	inner join PaymentManage pm");
                    sb.AppendLine("	on ftm.UserId = pm.UserId");
                    sb.AppendLine("	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("	and pm.ServiceId = '17'");/*サービスID(西鉄バス(福岡):2,鉄道:4,西鉄バス(北九州):5,にしてつグループ:6,宮交:17)*/
                    sb.AppendLine("	and pm.PaymentType = '4'");
                    sb.AppendLine("	and pm.GmoStatus = '1'");
                    sb.AppendLine("	and pm.GmoProcType = '2'");
                    sb.AppendLine("	left join CharacterResource cr");
                    sb.AppendLine("	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("	and Language ='ja'");
                    sb.AppendLine("	  left join UserInfoOid uio");
                    sb.AppendLine("	  on ftm.UserId=uio.UserId");
                    sb.AppendLine("      ) tbl");
                    // 決済エラー分は含めない
                    sb.AppendLine("  where not exists(");
                    sb.AppendLine("        select 1");
                    sb.AppendLine("          from PaymentError pe");
                    sb.AppendLine("         where pe.UserId = tbl.UserId");
                    sb.AppendLine("           and pe.PaymentId = tbl.PaymentId");
                    sb.AppendLine("           and pe.PaymentType = tbl.PaymentType");
                    sb.AppendLine("        	and(pe.ServiceId = '17')");/*サービスID(西鉄バス(福岡):2,鉄道:4,西鉄バス(北九州):5,にしてつグループ:6)*/
                    sb.AppendLine("           and pe.IsTreat = 0");         // 運用未処置
                    sb.AppendLine("     )");
                    sb.AppendLine("   and tbl.TranDate between @StartDatatTime and @EndDatatTime ");

                    return sb.ToString();
                }
            }
            /// <summary>
            /// 宮交表示用決済情報リスト総数取得
            /// </summary>
            /// <param name="stDate"></param>
            /// <param name="edDate"></param>
            /// <param name="MyrouteNo"></param>
            /// <param name="TicketType"></param>
            /// <param name="PaymentType"></param>
            /// <param name="TicketNumType"></param>
            /// <param name="TransportType"></param>
            /// <returns></returns>
            public List<MiyakohPaymentInfoListEntity> MiyakohPaymentDateListMaxCount(DateTime stDate, DateTime edDate, string MyrouteNo, string PaymentType, string TicketNumType, string TransportType, string TicketId, string AplType)
            {
                List<MiyakohPaymentInfoListEntity> result = new List<MiyakohPaymentInfoListEntity>();
                //現在表示されているリストの通し番号

                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder MiyakohSb = new StringBuilder();

                    string Miyakohinfo = GetALLGetMiyakohPaymentDateQuery(stDate, edDate);
                    MiyakohSb.AppendLine(Miyakohinfo.ToString());

                    if (MyrouteNo != "")
                    {
                        //検索条件にMyrouteID指定
                        MiyakohSb.AppendLine("   and tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                    }
                    if (TransportType != "-")
                    {
                        //検索条件に券種指定
                        MiyakohSb.AppendLine("   and tbl.BizCompanyCd = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                    }
                    if (PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        MiyakohSb.AppendLine("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    else if (PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        MiyakohSb.AppendLine("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        MiyakohSb.AppendLine("   and tbl.ChildNum = '0' ");
                        MiyakohSb.AppendLine("   and tbl.discountNum = '0' ");
                    }
                    else if (TicketNumType == "子供")
                    {
                        //検索条件に枚数種別：子供
                        MiyakohSb.AppendLine("   and tbl.AdultNum = '0' ");
                        MiyakohSb.AppendLine("   and tbl.discountNum = '0' ");
                    }
                    else if (TicketNumType == "学割")
                    {
                        //検索条件に枚数種別：子供
                        MiyakohSb.AppendLine("   and tbl.AdultNum = '0' ");
                        MiyakohSb.AppendLine("   and tbl.ChildNum = '0' ");
                    }
                    if (TicketId != "-")
                    {
                        //検索条件に券種指定
                        MiyakohSb.AppendLine("   and tbl.TicketId = @TicketId ");
                        cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                    }
                    if (AplType != "-")//au用Role番号判定
                    {
                        MiyakohSb.AppendLine("   and tbl.AplType = @AplType");
                        cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = AplType;
                    }


                    cmd.CommandText = MiyakohSb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");

                    DataTable dt = dbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        MiyakohPaymentInfoListEntity info = new MiyakohPaymentInfoListEntity
                        {
                            //件数のみ確認するためUserIDのみ取得
                            UserId = row["UserId"].ToString(),
                        };

                        result.Add(info);
                    }
                    return result;
                }
            }





        }

        /// <summary>
        /// 宮交クーポン利用情報取得
        /// </summary>
        public class MIyakohCouponModel
        { }

    }
}
