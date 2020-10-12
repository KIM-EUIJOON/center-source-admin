﻿using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;
using AppSigmaAdmin.ResponseData;

namespace AppSigmaAdmin.Models
{
    public class JRKyushuInfoModels
    {
        /// <summary>
        /// JR九州決済情報取得
        /// </summary>
        public class JRKyushuPaymentModel
        {
            /// <summary>
            /// JR九州販売チケット一覧取得
            /// </summary>
            /// <returns></returns>
            public List<NishitetsuPaymentInfo> JRKyushuPassportList(string UserId)
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();
                using (SqlDbInterface NTdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("select fsm.TicketType");
                    sb.AppendLine("   ,cr.Value");
                    sb.AppendLine("   ,fsm.BizCompanyCd");
                    sb.AppendLine("   ,fsm.TrsType");
                    sb.AppendLine("   ,fsm.TicketId");
                    sb.AppendLine("   ,fsm.TicketGroup");
                    sb.AppendLine("   from FreeTicketSalesMaster fsm");
                    sb.AppendLine("   left join CharacterResource cr");
                    sb.AppendLine("   on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("   and Language = 'ja'");
                    sb.AppendLine("   where fsm.BizCompanyCd='JRK'");//JR九州のコード不明

                    cmd.CommandText = sb.ToString();

                    DataTable dt = NTdbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        NishitetsuPaymentInfo info = new NishitetsuPaymentInfo
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
            public List<NishitetsuPaymentInfo> GetJRKyushuPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string MyrouteNo, string PaymentType, string TicketNumType, string TransportType, string TicketId, string AplType)
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();

                using (SqlDbInterface NishidbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder Jsb = new StringBuilder();
                    string JRKyushuInfo = GetALLGetJRKyushuPaymentDateQuery(stDate, edDate);
                    Jsb.AppendLine("select * from (" + JRKyushuInfo.ToString() + "");

                    if (MyrouteNo != "")
                    {
                        //検索条件にMyrouteID指定
                        Jsb.AppendLine("   and tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                    }
                    if (TransportType != "-")
                    {
                        //検索条件に券種指定
                        Jsb.AppendLine("   and tbl.TrsType = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                    }
                    if (PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        Jsb.AppendLine("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    else if (PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        Jsb.AppendLine("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        Jsb.AppendLine("   and tbl.ChildNum = '0' ");
                    }
                    else if (TicketNumType == "子供")
                    {
                        //検索条件に枚数種別：子供
                        Jsb.AppendLine("   and tbl.AdultNum = '0' ");
                    }
                    if (TicketId != "-")
                    {
                        //検索条件に券種指定
                        Jsb.AppendLine("   and tbl.TicketId = @TicketId ");
                        cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                    }
                    if (AplType != "-")//au用Role番号判定
                    {
                        Jsb.AppendLine("   and tbl.AplName = @AplType");
                        cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = AplType;
                    }

                    Jsb.AppendLine("  ) as MA  where MA.RecNo between @PageNum and @ListEnd");

                    cmd.CommandText = Jsb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");
                    cmd.Parameters.Add("@PageNum", SqlDbType.NVarChar).Value = pageNo;
                    cmd.Parameters.Add("@ListEnd", SqlDbType.NVarChar).Value = ListNoEnd;

                    DataTable dt = NishidbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        NishitetsuPaymentInfo infoN = new NishitetsuPaymentInfo
                        {
                            UserId = row["UserId"].ToString(),
                            TranDatetime = ((DateTime)row["TranDate"]).ToString("yyyy/MM/dd HH:mm:ss"),
                            PaymentId = row["PaymentId"].ToString(),
                            TicketName = row["Value"].ToString(),
                            TicketId = row["TicketId"].ToString(),
                            
                            AdultNum = row["AdultNum"].ToString(),
                            ChildNum = row["ChildNum"].ToString(),
                            PaymentType = row["PaymentType"].ToString(),
                            Amount = (int)row["Amount"],
                            ReceiptNo = row["ReceiptNo"].ToString(),
                            Apltype = row["AplName"].ToString()
                        };
                        if (row["TicketGroup"].ToString() == "1")
                        {
                            infoN.TicketName = infoN.TicketName + "[au]";
                        }
                        if (row["TrsType"].ToString() == "10")
                        {
                            infoN.TransportType = "鉄道";
                        }
                        else if (row["TrsType"].ToString() == "99")
                        {
                            infoN.TransportType = "マルチ";
                        }
                        else { infoN.TransportType = "-"; } /*NULL対策*/
                        if (row["AplName"].ToString() == "1")
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
            private string GetALLGetJRKyushuPaymentDateQuery(DateTime stDate, DateTime edDate)
            {
                using (SqlDbInterface JRKyushudbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    //string PaymentList = GetNishitetsuPaymentList();
                    //string BusPayment = NishitetsuBusPayment();

                    sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY tbl.PaymentId, tbl.PaymentType) as RecNo");    //決済IDと決済種別でソートする
                    sb.AppendLine("     , tbl.UserId");
                    sb.AppendLine("     , tbl.TranDate");
                    sb.AppendLine("     , case when tbl.BizCompanyCd =N'JRK' then N'JR九州'");
                    sb.AppendLine("     else N'チケット種別不明' end as BizCompanyCd");                                  /*チケット種別(交通手段)*/
                    sb.AppendLine("     , tbl.TicketType");                                                              /*チケット種別(au,au以外)*/
                    sb.AppendLine("     , tbl.TicketId");
                    sb.AppendLine("     , tbl.TicketGroup");
                    sb.AppendLine("     , tbl.Value");                                                                   /*チケット名称*/
                    sb.AppendLine("     , tbl.AdultNum");
                    sb.AppendLine("     , tbl.ChildNum");
                    sb.AppendLine("     , tbl.PaymentId");
                    sb.AppendLine("     , tbl.AplName");                                                                /*アプリ種別*/
                    sb.AppendLine("     , case when tbl.PaymentType = '3' then N'即時決済'");
                    sb.AppendLine("           when tbl.PaymentType = '4' then N'払戻し'");
                    sb.AppendLine("           when tbl.PaymentType = '5' then N'取消'");
                    sb.AppendLine("           else N'決済種別不明' end as PaymentType");
                    sb.AppendLine("     , tbl.Amount");
                    sb.AppendLine("     , tbl.ReceiptNo");
                    sb.AppendLine("     , tbl.TrsType");
                    sb.AppendLine("  from (");
                    // 即時決済データ取得
                    sb.AppendLine("           select pm.TranDate");
                    sb.AppendLine("           ,ftm.UserId");
                    sb.AppendLine("           ,fsm.TrsType");
                    sb.AppendLine("           , N'売上' as Summary");
                    sb.AppendLine("           ,fsm.TicketType");
                    sb.AppendLine("           ,cr.Value");                        /*チケット名称(日本語)*/
                    sb.AppendLine("           , ftm.AdultNum");                   /*大人枚数*/
                    sb.AppendLine("           , ftm.ChildNum");                   /*子供枚数*/
                    sb.AppendLine("           , pm.PaymentId");
                    sb.AppendLine("           , pm.PaymentType");
                    sb.AppendLine("           , pm.Amount");
                    sb.AppendLine("           , pm.ReceiptNo");
                    sb.AppendLine("           ,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケット種別(au,au以外)*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("           ,case when uio.AplType =1 then 'au'");
                    sb.AppendLine("           else ''");
                    sb.AppendLine("           end as AplName");
                    sb.AppendLine("           from FreeTicketManage ftm");
                    sb.AppendLine("           left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("           on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("           and (fsm.BizCompanyCd='JRK')");/*JR九州*/
                    sb.AppendLine("           inner join PaymentManage pm");
                    sb.AppendLine("           on ftm.UserId = pm.UserId");
                    sb.AppendLine("           and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("           and(pm.ServiceId = '8')");/*サービスID(JR九州:12)*/
                    sb.AppendLine("           and pm.PaymentType = '3'");
                    sb.AppendLine("           and pm.GmoStatus = '1'");
                    sb.AppendLine("           and pm.GmoProcType = '2'");
                    sb.AppendLine("           left join CharacterResource cr");
                    sb.AppendLine("           on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("           and Language ='ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on ftm.UserId=uio.UserId");
                    sb.AppendLine("           union all");
                    sb.AppendLine("           select pm.TranDate");
                    sb.AppendLine("           ,ftm.UserId");
                    sb.AppendLine("           ,fsm.TrsType");
                    sb.AppendLine("           , N'売上' as Summary");
                    sb.AppendLine("           ,fsm.TicketType");
                    sb.AppendLine("           ,cr.Value");                                /*チケット名称(日本語)*/
                    sb.AppendLine("           , ftm.AdultNum");                           /*大人枚数*/
                    sb.AppendLine("           , ftm.ChildNum");                           /*子供枚数*/
                    sb.AppendLine("           , pm.PaymentId");
                    sb.AppendLine("           , pm.PaymentType");
                    sb.AppendLine("           , pm.Amount* -1 as Amount");
                    sb.AppendLine("           , pm.ReceiptNo");
                    sb.AppendLine("           ,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケット種別(au,au以外)*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("           ,case when uio.AplType =1 then 'au'");
                    sb.AppendLine("           else ''");
                    sb.AppendLine("           end as AplName");
                    sb.AppendLine("           from FreeTicketManage ftm");
                    sb.AppendLine("           left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("           on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("           and (fsm.BizCompanyCd='JRK')");     /*JR九州*/
                    sb.AppendLine("           inner join PaymentManage pm");
                    sb.AppendLine("           on ftm.UserId = pm.UserId");
                    sb.AppendLine("           and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("           and(pm.ServiceId = '12')");/*サービスID(JR九州:12)*/
                    sb.AppendLine("           and pm.PaymentType = '5'");
                    sb.AppendLine("           and pm.GmoStatus = '1'");
                    sb.AppendLine("           and pm.GmoProcType = '3'");
                    sb.AppendLine("           left join CharacterResource cr");
                    sb.AppendLine("           on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("           and Language ='ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on ftm.UserId=uio.UserId");
                    sb.AppendLine("           union all");
                    sb.AppendLine("           select pm.TranDate");
                    sb.AppendLine("           ,ftm.UserId");
                    sb.AppendLine("           ,fsm.TrsType");
                    sb.AppendLine("           , N'払戻し' as Summary");
                    sb.AppendLine("           ,fsm.TicketType");
                    sb.AppendLine("           ,cr.Value");                        /*チケット名称*/
                    sb.AppendLine("           , ftm.AdultNum");                   /*大人枚数*/
                    sb.AppendLine("           , ftm.ChildNum");                   /*子供枚数*/
                    sb.AppendLine("           , pm.PaymentId");
                    sb.AppendLine("           , pm.PaymentType");
                    sb.AppendLine("           , pm.Amount");
                    sb.AppendLine("           , pm.ReceiptNo");
                    sb.AppendLine("           ,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケット種別(au,au以外)*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("           ,case when uio.AplType =1 then 'au'");
                    sb.AppendLine("           else ''");
                    sb.AppendLine("           end as AplName");
                    sb.AppendLine("           from FreeTicketManage ftm");
                    sb.AppendLine("           left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("           on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("           and (fsm.BizCompanyCd='JRK')");     /*JR九州*/
                    sb.AppendLine("           inner join PaymentManage pm");
                    sb.AppendLine("           on ftm.UserId = pm.UserId");
                    sb.AppendLine("           and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("           and(pm.ServiceId = '12')");/*サービスID(JR九州:12)*/
                    sb.AppendLine("           and pm.PaymentType = '4'");
                    sb.AppendLine("           and pm.GmoStatus = '1'");
                    sb.AppendLine("           and pm.GmoProcType = '2'");
                    sb.AppendLine("           left join CharacterResource cr");
                    sb.AppendLine("           on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("           and Language ='ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on ftm.UserId=uio.UserId");

                    sb.AppendLine("      ) tbl");
                    // 決済エラー分は含めない
                    sb.AppendLine("  where not exists(");
                    sb.AppendLine("        select 1");
                    sb.AppendLine("          from PaymentError pe");
                    sb.AppendLine("         where pe.UserId = tbl.UserId");
                    sb.AppendLine("           and pe.PaymentId = tbl.PaymentId");
                    sb.AppendLine("           and pe.PaymentType = tbl.PaymentType");
                    sb.AppendLine("        	and(pe.ServiceId = '12')");/*サービスID(JR九州:12)*/
                    sb.AppendLine("           and pe.IsTreat = 0");         // 運用未処置
                    sb.AppendLine("     )");
                    sb.AppendLine("   and tbl.TranDate between @StartDatatTime and @EndDatatTime ");

                    return sb.ToString();
                }
            }

            /// <summary>
            /// JR九州表示用決済情報リスト総数取得
            /// </summary>
            /// <param name="stDate"></param>
            /// <param name="edDate"></param>
            /// <param name="MyrouteNo"></param>
            /// <param name="TicketType"></param>
            /// <param name="PaymentType"></param>
            /// <param name="TicketNumType"></param>
            /// <param name="TransportType"></param>
            /// <returns></returns>
            public List<NishitetsuPaymentInfo> JRKyushuPaymentDateListMaxCount(DateTime stDate, DateTime edDate, string MyrouteNo, string PaymentType, string TicketNumType, string TransportType, string TicketId, string AplType)
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();
                //現在表示されているリストの通し番号

                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder JRKyushuSb = new StringBuilder();

                    string JRKyushuinfo = GetALLGetJRKyushuPaymentDateQuery(stDate, edDate);
                    JRKyushuSb.AppendLine(JRKyushuinfo.ToString());

                    if (MyrouteNo != "")
                    {
                        //検索条件にMyrouteID指定
                        JRKyushuSb.AppendLine("   and tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                    }
                    if (TransportType != "-")
                    {
                        //検索条件に券種指定
                        JRKyushuSb.AppendLine("   and tbl.BizCompanyCd = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                    }
                    if (PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        JRKyushuSb.AppendLine("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    else if (PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        JRKyushuSb.AppendLine("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        JRKyushuSb.AppendLine("   and tbl.ChildNum = '0' ");
                    }
                    else if (TicketNumType == "子供")
                    {
                        //検索条件に枚数種別：子供
                        JRKyushuSb.AppendLine("   and tbl.AdultNum = '0' ");
                    }
                    if (TicketId != "-")
                    {
                        //検索条件に券種指定
                        JRKyushuSb.AppendLine("   and tbl.TicketId = @TicketId ");
                        cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                    }
                    if (AplType != "-")//au用Role番号判定
                    {
                        JRKyushuSb.AppendLine("   and tbl.AplName = @AplType");
                        cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = AplType;
                    }


                    cmd.CommandText = JRKyushuSb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");

                    DataTable dt = dbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        NishitetsuPaymentInfo info = new NishitetsuPaymentInfo
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
        /// JR九州クーポン利用情報取得
        /// </summary>
        public class JRKyushuCouponModel
        {
            /// <summary>
            /// JR九州クーポン券種ドロップダウンリスト項目取得
            /// </summary>
            /// <returns></returns>
            public DataTable JRKyushuPassportList(string UserId)
            {
                List<JRKyushuCouponInfoEntity> result = new List<JRKyushuCouponInfoEntity>();
                using (SqlDbInterface JRdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("select cpm.CouponId");
                    sb.AppendLine("   ,cr.Value");
                    sb.AppendLine("   ,cpm.BizCompanyCd");
                    sb.AppendLine("   from CouponMaster cpm");
                    sb.AppendLine("   left join CharacterResource cr");
                    sb.AppendLine("   on cpm.CouponName = cr.ResourceId");
                    sb.AppendLine("   and Language = 'ja'");
                    sb.AppendLine("   where BizCompanyCd = @Bizid");

                    cmd.Parameters.Add("@Bizid", SqlDbType.NVarChar).Value = "JRK";/*JR九州のBizCompanyCD*/

                    cmd.CommandText = sb.ToString();

                    return JRdbInterface.ExecuteReader(cmd);
                   
                }
            }

            /// <summary>
            /// JR九州クーポン情報取得
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            public DataTable GetCouponDateList(JRKyushuCouponInfoEntity model)
            {
                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    // データ取得
                    StringBuilder Jsb = new StringBuilder();
                    string JRCouponInfo = GetALLGetJRCouponDateQuery();
                    Jsb.AppendLine("select * from (" + JRCouponInfo.ToString() + "");


                    if (false == string.IsNullOrEmpty(model.UserId))
                    {
                        Jsb.AppendLine("    AND cm.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = model.UserId;
                    }
                    if (false == string.IsNullOrEmpty(model.AplType))
                    {
                        Jsb.AppendLine("    AND AplType = @AplName ");
                        cmd.Parameters.Add("@AplName", SqlDbType.NVarChar).Value = "au";
                    }
                    if (false == string.IsNullOrEmpty(model.CouponId))
                    {
                        Jsb.AppendLine("   and cm.CouponId = @TicketId  ");
                        cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = model.CouponId;
                    }
                    if (false == string.IsNullOrEmpty(model.FacilityName)) 
                    {
                        //検索条件に決済種別：決済種別不明指定
                        Jsb.AppendLine("   and sm.FacilityName = @FacilityId  ");
                        cmd.Parameters.Add("@FacilityId", SqlDbType.NVarChar).Value = model.FacilityName;
                    }
                    else if (false == string.IsNullOrEmpty(model.ShopCode)) 
                    {
                        //検索条件に決済種別指定
                        Jsb.AppendLine("   and cs.ShopCode = @ShopCode ");
                        cmd.Parameters.Add("@ShopCode", SqlDbType.NVarChar).Value = model.ShopCode;
                    }
                    
                    Jsb.AppendLine("  ) as MA");

                    // 検索条件
                    Jsb.AppendLine("  WHERE 1 = 1");
                    cmd.CommandText = Jsb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = model.TargetDateBegin;
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = model.TargetDateEnd + " 23:59:59";
                    cmd.Parameters.Add("@lang", SqlDbType.NVarChar).Value = model.Language;
                    return dbInterface.ExecuteReader(cmd);
                }
            }


            ///<summary>
            ///JR九州クーポン利用情報取得内容
            ///<summary>
            private string GetALLGetJRCouponDateQuery()
            {
                using (SqlDbInterface JRKyushuCoupondbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("select cm.UserId");
                    sb.AppendLine("       , cm.CouponId");
                    sb.AppendLine("       , cm.UsageDateTime");
                    sb.AppendLine("       , cm.UsageShopCode");
                    sb.AppendLine("       ,cma.BizCompanyCd");
                    sb.AppendLine("       ,cr.Value");
                    sb.AppendLine("       ,cs.ShopCode");
                    sb.AppendLine("       ,sm.ShopName");
                    sb.AppendLine("       ,fm.FacilityName");
                    sb.AppendLine("       ,im.IndustryName");
                    sb.AppendLine("       ,case when uio.AplType =1 then 'au' ");   /*アプリタイプ*/
                    sb.AppendLine("       else ''");
                    sb.AppendLine("       end as AplName");
                    sb.AppendLine("       from CouponManage cm");
                    sb.AppendLine("       left join CouponMaster cma");
                    sb.AppendLine("       on cm.CouponId=cma.CouponId");
                    sb.AppendLine("       left join CharacterResource cr");
                    sb.AppendLine("       on cma.CouponName = cr.ResourceId");
                    sb.AppendLine("       and cr.Language='ja'");
                    sb.AppendLine("       left join CouponShop cs");
                    sb.AppendLine("       on cm.CouponId = cs.CouponId");
                    sb.AppendLine("       left join ShopMaster sm");
                    sb.AppendLine("       on cs.ShopCode = sm.ShopCode");
                    sb.AppendLine("       left join FacilityMaster fm");
                    sb.AppendLine("       on sm.FacilityId = fm.FacilityId");
                    sb.AppendLine("       left join IndustryMaster im");
                    sb.AppendLine("       on sm.IndustryCode = im.IndustryCode");
                    sb.AppendLine("       left join UserInfoOid uio");
                    sb.AppendLine("       on cm.UserId = uio.UserId");
                    sb.AppendLine("       where cm.UsageStatus='3'");
                    sb.AppendLine("       and cma.BizCompanyCd='JRK'");           /*JR九州固定*/
                    sb.AppendLine("       and cm.UsageDateTime between @StartDatatTime and @EndDatatTime ");

                    return sb.ToString();
                }
            }

            /// <summary>
            /// 施設マスタ取得
            /// </summary>
            /// <returns>SQL実行結果</returns>
            public DataTable GetFacilityNames(string language)
            {
                using (SqlDbInterface NTdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("SELECT fclM.FacilityName,");
                    sb.AppendLine("       fclM.FacilityId");
                    sb.AppendLine("FROM FacilityMaster fclM");
                    sb.AppendLine("Where fclM.Language = @lng");
                    sb.AppendLine("and fclm.BizCompanyCd='JRK'");           /*JR九州固定*/

                    cmd.Parameters.Add("@lng", SqlDbType.NVarChar).Value = language;
                    cmd.CommandText = sb.ToString();

                    return NTdbInterface.ExecuteReader(cmd);
                }
            }

            /// <summary>
            /// テナント名マスタ取得
            /// </summary>
            /// <returns>SQL実行結果</returns>
            public DataTable GetShopName(string language)
            {
                using (SqlDbInterface NTdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("SELECT sm.ShopName");
                    sb.AppendLine("       ,sm.FacilityId");
                    sb.AppendLine("       ,sm.ShopCode");
                    sb.AppendLine("       ,fm.BizCompanyCd");
                    sb.AppendLine("FROM ShopMaster sm");
                    sb.AppendLine("inner join FacilityMaster fm");
                    sb.AppendLine("on sm.FacilityId=fm.FacilityId");
                    sb.AppendLine("Where sm.Language = @lng");
                    sb.AppendLine("and fm.BizCompanyCd='JRK'");/*JR九州固定*/

                    cmd.Parameters.Add("@lng", SqlDbType.NVarChar).Value = language;
                    cmd.CommandText = sb.ToString();

                    return NTdbInterface.ExecuteReader(cmd);
                }
            }
        }
    }
}
