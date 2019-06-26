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
    /// <summary>
    /// 決済データ取得クラス
    /// </summary>
    public class JTXPaymentModel
    {
        /// <summary>
        /// Japantaxiの表示用決済情報リスト取得
        /// </summary>
        /// <param name="stDate">抽出範囲開始日</param>
        /// <param name="edDate">抽出範囲終了日</param>
        /// /// <param name="pageNo">表示リスト開始位置</param>
        /// /// <param name="edDate">表示リスト終了位置</param>
        /// <returns>JTX決済情報</returns>
        public List<JtxPaymentInfo> GetJtxPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd)
        {
            List<JtxPaymentInfo> result = new List<JtxPaymentInfo>();
            //現在表示されているリストの通し番号

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder jtxsb = new StringBuilder();

                string Jtxinfo = GetALLJtxPaymentDateQuery(stDate, edDate);
                jtxsb.AppendLine("select * from");
                jtxsb.AppendLine(" (" + Jtxinfo.ToString() + ") as MA");
                jtxsb.AppendLine("    where MA.RecNo between @PageNum and @PageNumEnd");      //表示する件数分の情報のみ取得する

                cmd.CommandText = jtxsb.ToString();

                cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");
                cmd.Parameters.Add("@PageNum", SqlDbType.NVarChar).Value = pageNo;
                cmd.Parameters.Add("@PageNumEnd", SqlDbType.NVarChar).Value = ListNoEnd;

                DataTable dt = dbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows)
                {
                    JtxPaymentInfo info = new JtxPaymentInfo
                    {
                        UserId = row["UserId"].ToString(),
                        TranDatetime = ((DateTime)row["TranDate"]).ToString("yyyy/MM/dd HH:mm:ss"),
                        PaymentId = row["PaymentId"].ToString(),
                        CompanyName = row["CompanyName"].ToString(),
                        OrderId = row["OrderId"].ToString(),
                        PaymentType = row["PaymentType"].ToString(),
                        Amount = (int)row["Amount"],
                    };

                    result.Add(info);
                }
                return result;
            }

        }
        /// <summary>
        /// Japantaxiの表示用決済情報リスト総数取得
        /// </summary>
        /// <param name="stDate">抽出範囲開始日</param>
        /// <param name="edDate">抽出範囲終了日</param>
        /// <returns>JTX決済情報</returns>
        public List<JtxPaymentInfo> GetJtxPaymentDateListCount(DateTime stDate, DateTime edDate)
        {
            List<JtxPaymentInfo> result = new List<JtxPaymentInfo>();
            //現在表示されているリストの通し番号


            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder jtxsb = new StringBuilder();

                string Jtxinfo = GetALLJtxPaymentDateQuery(stDate, edDate);
                jtxsb.AppendLine(Jtxinfo.ToString());

                cmd.CommandText = jtxsb.ToString();

                cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");

                DataTable dt = dbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows)
                {
                    JtxPaymentInfo info = new JtxPaymentInfo
                    {
                        //件数のみ確認するためUserIDのみ取得
                        UserId = row["UserId"].ToString(),
                    };

                    result.Add(info);
                }
                return result;
            }

        }
        /// <summary>
        /// Japantaxiの決済情報リスト取得内容
        /// </summary>
        /// <param name="stDate">抽出範囲開始日</param>
        /// <param name="edDate">抽出範囲終了日</param>
        /// <returns>JTX決済情報</returns>
        private string GetALLJtxPaymentDateQuery(DateTime stDate, DateTime edDate)
        {

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY mr.PaymentId, pm.PaymentType) as RecNo");
                sb.AppendLine("     , mr.UserId");
                sb.AppendLine("     , pm.TranDate");
                sb.AppendLine("     ,mr.PaymentId");
                sb.AppendLine("     ,mr.CompanyName");
                sb.AppendLine("     ,mr.OrderId");
                sb.AppendLine("     ,case when pm.PaymentType = '3' then N'即時決済'");
                sb.AppendLine("           when pm.PaymentType = '4' then N'払戻し'");
                sb.AppendLine("           when pm.PaymentType = '5' then N'取消'");
                sb.AppendLine("           else N'決済種別不明'end as PaymentType");
                sb.AppendLine("     ,pm.Amount");
                sb.AppendLine(" from MobilityReserve mr");
                sb.AppendLine(" inner join PaymentManage pm");
                sb.AppendLine("    on mr.UserId = pm.UserId");
                sb.AppendLine("   and mr.PaymentId = pm.PaymentId");
                sb.AppendLine("   and pm.ServiceId = '1'");     // サービスID(JTX)
                sb.AppendLine("   and pm.PaymentType = '3'");   // 即時決済
                sb.AppendLine("   and pm.GmoStatus = '1'");     // 成功
                sb.AppendLine("   and pm.GmoProcType = '2'");   // 決済実行
                sb.AppendLine(" where mr.Payment = 1");         // クレジット利用
                sb.AppendLine("   and pm.TranDate between @StartDatatTime and @EndDatatTime");
                sb.AppendLine("   and mr.Status = '11'");       //決済済み
                sb.AppendLine("   and mr.TrsType = '20'");      //タクシー
                //決済失敗の未処理分は含めない
                sb.AppendLine("   and not exists(");
                sb.AppendLine("       select 1");
                sb.AppendLine("         from PaymentError pe");
                sb.AppendLine("        where pe.UserId = mr.UserId");
                sb.AppendLine("          and pe.PaymentId = mr.PaymentId");
                sb.AppendLine("          and pe.ServiceId = '1'");      //サービスID(JTX)
                sb.AppendLine("          and pe.IsTreat = 0");          //運用未処理
                sb.AppendLine("     )");

                return sb.ToString();
            }

        }
        public class NassePaymentModel
        {
            ///<summary>
            ///ナッセパスポート名プルダウンリスト内容取得
            ///</summary>
            public List<NassePaymentInfo> NassePassportList()
            {
                List<NassePaymentInfo> result = new List<NassePaymentInfo>();
                StringBuilder NasseSb = new StringBuilder();
                using (SqlDbInterface NassedbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("select gps.PassportId");
                    sb.AppendLine("     ,esfm.Title,gps.Language");
                    sb.AppendLine("     from GourmetPassportSales gps");
                    sb.AppendLine("     left join EventSpotFeatureMaster esfm");
                    sb.AppendLine("     on gps.FeatureId = esfm.FeatureId");
                    sb.AppendLine("     where gps.Language='ja'");
                    sb.AppendLine("     and esfm.Language='ja'");

                    NasseSb.AppendLine(sb.ToString());
                    cmd.CommandText = NasseSb.ToString();
                    DataTable dt = NassedbInterface.ExecuteReader(cmd);
                    foreach (DataRow row in dt.Rows)
                    {
                        NassePaymentInfo info = new NassePaymentInfo
                        {
                            PassportID = row["PassportId"].ToString(),
                            PassportName = row["Title"].ToString(),
                        };

                        result.Add(info);
                    }
                }
                return result;
            }
            /// <summary>
            /// Nasseの決済情報リスト取得
            /// </summary>
            /// <param name="stDate">抽出範囲開始日</param>
            /// <param name="edDate">抽出範囲終了日</param>
            /// <returns>Nasse決済情報</returns>
            public List<NassePaymentInfo> GetNassePaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string PassID)
            {
                List<NassePaymentInfo> result = new List<NassePaymentInfo>();

                using (SqlDbInterface NassedbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder Nsb = new StringBuilder();
                    string NasseInfo = GetALLGetNassePaymentDateQuery(stDate, edDate);
                    Nsb.AppendLine("select * from");
                    if (PassID == "-")
                    {
                        //検索条件にパスポートIDが設定されていない場合は条件を追加しない
                        Nsb.AppendLine(" (" + NasseInfo.ToString() + ") as MA");
                    }
                    else
                    {
                        //検索条件にパスポートIDが設定されている場合は条件を追加する
                        Nsb.AppendLine(" (" + NasseInfo.ToString() + "   and gp.PassportId = @PassportId ) as MA");
                    }
                    //表示する件数分の情報のみ取得する
                    Nsb.AppendLine("    where MA.RecNo between @PageNum and @ListEnd");

                    cmd.CommandText = Nsb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");
                    cmd.Parameters.Add("@PassportId", SqlDbType.NVarChar).Value = PassID;
                    cmd.Parameters.Add("@PageNum", SqlDbType.NVarChar).Value = pageNo;
                    cmd.Parameters.Add("ListEnd", SqlDbType.NVarChar).Value = ListNoEnd;

                    DataTable dt = NassedbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        NassePaymentInfo infoN = new NassePaymentInfo
                        {
                            UserId = row["UserId"].ToString(),
                            TranDatetime = ((DateTime)row["TranDate"]).ToString("yyyy/MM/dd HH:mm:ss"),
                            PaymentId = row["PaymentId"].ToString(),
                            PassportID = row["PassportId"].ToString(),
                            PassportName = row["Title"].ToString(),
                            PaymentType = row["PaymentType"].ToString(),
                            Amount = (int)row["Amount"],
                        };

                        result.Add(infoN);
                    }
                    return result;
                }
            }

            /// <summary>
            /// ナッセ取得情報一覧
            /// </summary>
            /// <param name="stDate"></param>
            /// <param name="edDate"></param>
            /// <returns></returns>
            private string GetALLGetNassePaymentDateQuery(DateTime stDate, DateTime edDate)
            {
                using (SqlDbInterface NassedbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY pm.PaymentId, pm.PaymentType) as RecNo");      //決済IDと決済種別でソートする
                    sb.AppendLine("     , gp.UserId");
                    sb.AppendLine("     , pm.TranDate");
                    sb.AppendLine("     ,gp.PassportId");
                    sb.AppendLine("     ,esfm.Title");
                    sb.AppendLine("     ,pm.PaymentId");
                    sb.AppendLine("     ,case when pm.PaymentType = '3' then N'即時決済'");
                    sb.AppendLine("           else N'決済種別不明'end as PaymentType");
                    sb.AppendLine("     ,pm.Amount");
                    sb.AppendLine(" from GourmetPassport gp");
                    sb.AppendLine(" inner join GourmetPassportSales gps");
                    sb.AppendLine("    on gp.PassportId = gps.PassportId");
                    sb.AppendLine("   and gps.Language = 'ja'");
                    sb.AppendLine("   inner join PaymentManage pm");
                    sb.AppendLine("   on gp.UserId = pm.UserId");
                    sb.AppendLine("   and gp.PaymentId = pm.PaymentId");
                    sb.AppendLine("   and gp.OrderNo = pm.OrderNo");
                    sb.AppendLine("   left join EventSpotFeatureMaster esfm");
                    sb.AppendLine("   on gps.FeatureId = esfm.FeatureId");
                    sb.AppendLine("   and esfm.Language='ja'");
                    sb.AppendLine("   where not exists(");
                    sb.AppendLine("       select 1");
                    sb.AppendLine("         from PaymentError pe");
                    sb.AppendLine("        where pe.UserId = gp.UserId");
                    sb.AppendLine("          and pe.PaymentId = pm.PaymentId");
                    sb.AppendLine("          and pe.PaymentType = pm.PaymentType");
                    sb.AppendLine("           and pe.ServiceId = '3'");     // サービスID(ナッセ)
                    sb.AppendLine("           and pe.IsTreat = 0");         // 運用未処置
                    sb.AppendLine("     )");
                    sb.AppendLine("   and pm.ServiceId = '3'");           // サービスID(ナッセ)
                    sb.AppendLine("   and pm.PaymentType = '3'");                // 即時決済(購入)
                    sb.AppendLine("   and pm.GmoStatus = '1'");                  // 成功
                    sb.AppendLine("   and pm.GmoProcType = '2'");                // 決済実行
                    sb.AppendLine("   and pm.TranDate between @StartDatatTime and @EndDatatTime");                // 決済実行

                    return sb.ToString();


                }
            }
            /// <summary>
            /// ナッセの表示用決済情報リスト総数取得
            /// </summary>
            /// <param name="stDate">抽出範囲開始日</param>
            /// <param name="edDate">抽出範囲終了日</param>
            /// <returns>ナッセ決済情報</returns>
            public List<NassePaymentInfo> NassePaymentDateListMaxCount(DateTime stDate, DateTime edDate, string PassID)
            {
                List<NassePaymentInfo> result = new List<NassePaymentInfo>();
                //現在表示されているリストの通し番号

                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder NasseSb = new StringBuilder();

                    string Nasseinfo = GetALLGetNassePaymentDateQuery(stDate, edDate);


                    if (PassID == "-")
                    {
                        //検索条件にパスポートIDが設定されていない場合は条件を追加しない
                    }
                    else
                    {
                        //検索条件にパスポートIDが設定されている場合は条件を追加する
                        NasseSb.AppendLine(Nasseinfo.ToString() + "   and gp.PassportId = @PassportId ");
                        cmd.Parameters.Add("@PassportId", SqlDbType.NVarChar).Value = PassID;
                    }

                    NasseSb.AppendLine(Nasseinfo.ToString());

                    cmd.CommandText = NasseSb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");


                    DataTable dt = dbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        NassePaymentInfo info = new NassePaymentInfo
                        {
                            UserId = row["UserId"].ToString(),
                        };

                        result.Add(info);
                    }
                    return result;
                }
            }
        }

        public class NishitetsuPaymentModel
        {
            /// <summary>
            /// 西鉄取得情報一覧
            /// </summary>
            /// <param name="stDate"></param>
            /// <param name="edDate"></param>
            /// <returns></returns>
            private string GetALLGetNishitetsuPaymentDateQuery(DateTime stDate, DateTime edDate)
            {
                using (SqlDbInterface NishitetsudbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    string BusPayment = NishitetsuBusPayment();
                    string PaymentList = GetNishitetsuPaymentList();

                    sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY tbl.PaymentId, tbl.PaymentType) as RecNo");    //決済IDと決済種別でソートする
                    sb.AppendLine("     , tbl.UserId");
                    sb.AppendLine("     , tbl.TranDate");
                    sb.AppendLine("     , case when tbl.TrsType =N'10' then N'鉄道'");
                    sb.AppendLine("     when tbl.TrsType =N'14' then N'バス'");
                    sb.AppendLine("     else N'チケット種別不明' end as TrsType");                                  /*チケット種別(交通手段)*/
                    sb.AppendLine("     , tbl.TicketType");
                    sb.AppendLine("     , tbl.TicketName");
                    sb.AppendLine("     , tbl.AdultNum");
                    sb.AppendLine("     , tbl.ChildNum");
                    sb.AppendLine("     , tbl.PaymentId");
                    sb.AppendLine("     , case when tbl.PaymentType = '3' then N'即時決済'");
                    sb.AppendLine("           when tbl.PaymentType = '4' then N'払戻し'");
                    sb.AppendLine("           when tbl.PaymentType = '5' then N'取消'");
                    sb.AppendLine("           else N'決済種別不明' end as PaymentType");
                    sb.AppendLine("     , tbl.Amount");
                    sb.AppendLine("     , tbl.ReceiptNo");
                    sb.AppendLine("  from (");
                    // 即時決済データ取得
                    //暫定処理
                    sb.AppendLine(" " + BusPayment.ToString() + "");        //バスチケットのみ
                    sb.AppendLine("        union all  ");
                    sb.AppendLine(" " + PaymentList.ToString() + "");        //鉄道チケットのみ
                    sb.AppendLine("      ) tbl");
                    // 決済エラー分は含めない
                    sb.AppendLine("  where not exists(");
                    sb.AppendLine("        select 1");
                    sb.AppendLine("          from PaymentError pe");
                    sb.AppendLine("         where pe.UserId = tbl.UserId");
                    sb.AppendLine("           and pe.PaymentId = tbl.PaymentId");
                    sb.AppendLine("           and pe.PaymentType = tbl.PaymentType");
                    sb.AppendLine("           and pe.ServiceId = '2'");     // サービスID(西鉄)
                    sb.AppendLine("           and pe.IsTreat = 0");         // 運用未処置
                    sb.AppendLine("     )");
                    sb.AppendLine("   and tbl.TranDate between @StartDatatTime and @EndDatatTime ");

                    return sb.ToString();
                }
            }
            private string NishitetsuBusPayment()
            {
                using (SqlDbInterface NishitetsudbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("       select pm.TranDate");
                    sb.AppendLine("            , nft.UserId");
                    sb.AppendLine("            , fsm.TrsType");
                    sb.AppendLine("            , N'売上' as Summary");
                    sb.AppendLine("            , nft.TicketType");
                    sb.AppendLine("            , fsm.TicketName");
                    sb.AppendLine("            , nft.AdultNum");
                    sb.AppendLine("            , nft.ChildNum ");
                    sb.AppendLine("            , pm.PaymentId");
                    sb.AppendLine("            , pm.PaymentType");
                    sb.AppendLine("            , pm.Amount");
                    sb.AppendLine("            , pm.ReceiptNo");
                    sb.AppendLine("         from NishitetsuFreeTicket nft");
                    sb.AppendLine("        inner join PaymentManage pm");
                    sb.AppendLine("           on nft.UserId = pm.UserId");
                    sb.AppendLine("          and nft.PaymentId = pm.PaymentId");
                    sb.AppendLine("          and pm.ServiceId = '2'");                  // サービスID(西鉄)
                    sb.AppendLine("          and pm.PaymentType = '3'");                // 即時決済(購入)
                    sb.AppendLine("          and pm.GmoStatus = '1'");                  // 成功
                    sb.AppendLine("          and pm.GmoProcType = '2'");                // 決済実行
                    sb.AppendLine("          left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("          on nft.TicketType = fsm.TicketType");
                    sb.AppendLine("          and fsm.BizCompanyCd='NNR002'");           //西鉄
                    sb.AppendLine("          and fsm.TrsType ='14'");                   //バス情報のみ取得
                    sb.AppendLine("        union all  ");
                    // 払戻し返金データ取得
                    sb.AppendLine("        select pm.TranDate");
                    sb.AppendLine("             , nft.UserId");
                    sb.AppendLine("            , fsm.TrsType");
                    sb.AppendLine("             , N'売上' as Summary");
                    sb.AppendLine("            , nft.TicketType");
                    sb.AppendLine("            , fsm.TicketName");
                    sb.AppendLine("             , nft.AdultNum");
                    sb.AppendLine("             , nft.ChildNum ");
                    sb.AppendLine("             , pm.PaymentId");
                    sb.AppendLine("             , pm.PaymentType");
                    sb.AppendLine("             , pm.Amount * -1 as Amount");
                    sb.AppendLine("             , pm.ReceiptNo");
                    sb.AppendLine("          from NishitetsuFreeTicket nft");
                    sb.AppendLine("         inner join PaymentManage pm");
                    sb.AppendLine("            on nft.UserId = pm.UserId");
                    sb.AppendLine("           and nft.PaymentId = pm.PaymentId");
                    sb.AppendLine("           and pm.ServiceId = '2'");                 // サービスID(西鉄)
                    sb.AppendLine("           and pm.PaymentType = '5'");               // 取消(返金)
                    sb.AppendLine("           and pm.GmoStatus = '1'");                 // 成功
                    sb.AppendLine("           and pm.GmoProcType = '3'");               // 決済変更
                    sb.AppendLine("          left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("          on nft.TicketType = fsm.TicketType");
                    sb.AppendLine("          and fsm.BizCompanyCd='NNR002'");           //西鉄
                    sb.AppendLine("          and fsm.TrsType ='14'");                   //バス情報のみ取得
                    sb.AppendLine("        union all");
                    // 払戻し手数料取得
                    sb.AppendLine("        select pm.TranDate ");
                    sb.AppendLine("             , nft.UserId");
                    sb.AppendLine("            , fsm.TrsType");
                    sb.AppendLine("             , N'払戻し' as Summary");
                    sb.AppendLine("            , nft.TicketType");
                    sb.AppendLine("            , fsm.TicketName");
                    sb.AppendLine("             , nft.AdultNum");
                    sb.AppendLine("             , nft.ChildNum ");
                    sb.AppendLine("             , pm.PaymentId");
                    sb.AppendLine("             , pm.PaymentType");
                    sb.AppendLine("             , pm.Amount");
                    sb.AppendLine("             , pm.ReceiptNo");
                    sb.AppendLine("          from NishitetsuFreeTicket nft");
                    sb.AppendLine("         inner join PaymentManage pm");
                    sb.AppendLine("            on nft.UserId = pm.UserId");
                    sb.AppendLine("           and nft.PaymentId = pm.PaymentId");
                    sb.AppendLine("           and nft.RefundOrderNo = pm.OrderNo");
                    sb.AppendLine("           and pm.ServiceId = '2'");                 // サービスID(西鉄)
                    sb.AppendLine("           and pm.PaymentType = '4'");               // 払戻し(手数料徴収)
                    sb.AppendLine("           and pm.GmoStatus = '1'");                 // 成功
                    sb.AppendLine("           and pm.GmoProcType = '2'");               // 決済実行
                    sb.AppendLine("          left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("          on nft.TicketType = fsm.TicketType");
                    sb.AppendLine("          and fsm.BizCompanyCd='NNR002'");           //西鉄
                    sb.AppendLine("          and fsm.TrsType ='14'");                   //バス情報のみ取得

                    return sb.ToString();
                }
            }
            private string GetNishitetsuPaymentList()
            {
                using (SqlDbInterface NishitetsudbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("        select pm.TranDate");
                    sb.AppendLine("           ,ftm.UserId");
                    sb.AppendLine("           ,fsm.TrsType");
                    sb.AppendLine("           , N'売上' as Summary");
                    sb.AppendLine("           ,fsm.TicketType");
                    sb.AppendLine("            ,fsm.TicketName");   /*チケット名称*/
                    sb.AppendLine("           , ftm.AdultNum"); /*大人枚数*/
                    sb.AppendLine("           , ftm.ChildNum"); /*子供枚数*/
                    sb.AppendLine("           , pm.PaymentId");
                    sb.AppendLine("           , pm.PaymentType");
                    sb.AppendLine("           , pm.Amount");
                    sb.AppendLine("           , pm.ReceiptNo");
                    sb.AppendLine("           from FreeTicketManage ftm");
                    sb.AppendLine("          left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("          on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("          and fsm.BizCompanyCd='NNR001'");
                    sb.AppendLine("          inner join PaymentManage pm");
                    sb.AppendLine("          on ftm.UserId = pm.UserId");
                    sb.AppendLine("          and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("          and pm.ServiceId = '4'");
                    sb.AppendLine("          and pm.PaymentType = '3'");
                    sb.AppendLine("          and pm.GmoStatus = '1'");
                    sb.AppendLine("          and pm.GmoProcType = '2'");
                    sb.AppendLine("          union all");
                    sb.AppendLine("          ");    /*払戻し返金データ取得*/
                    sb.AppendLine("          select pm.TranDate");
                    sb.AppendLine("           ,ftm.UserId");
                    sb.AppendLine("           ,fsm.TrsType");
                    sb.AppendLine("           , N'売上' as Summary");
                    sb.AppendLine("           ,fsm.TicketType");
                    sb.AppendLine("            ,fsm.TicketName");   /*チケット名称*/
                    sb.AppendLine("           , ftm.AdultNum"); /*大人枚数*/
                    sb.AppendLine("           , ftm.ChildNum"); /*子供枚数*/
                    sb.AppendLine("           , pm.PaymentId");
                    sb.AppendLine("           , pm.PaymentType");
                    sb.AppendLine("           , pm.Amount* -1 as Amount");
                    sb.AppendLine("           , pm.ReceiptNo");
                    sb.AppendLine("           from FreeTicketManage ftm");
                    sb.AppendLine("          left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("          on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("          and fsm.BizCompanyCd='NNR001'");
                    sb.AppendLine("          inner join PaymentManage pm");
                    sb.AppendLine("          on ftm.UserId = pm.UserId");
                    sb.AppendLine("          and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("          and pm.ServiceId = '4'");
                    sb.AppendLine("          and pm.PaymentType = '5'");
                    sb.AppendLine("          and pm.GmoStatus = '1'");
                    sb.AppendLine("          and pm.GmoProcType = '3'");
                    sb.AppendLine("          union all");
                    /*払戻し手数料取得*/
                    sb.AppendLine("          select pm.TranDate");
                    sb.AppendLine("           ,ftm.UserId");
                    sb.AppendLine("           ,fsm.TrsType");
                    sb.AppendLine("           , N'払戻し' as Summary");
                    sb.AppendLine("           ,fsm.TicketType");
                    sb.AppendLine("            ,fsm.TicketName");   /*チケット名称*/
                    sb.AppendLine("           , ftm.AdultNum"); /*大人枚数*/
                    sb.AppendLine("           , ftm.ChildNum"); /*子供枚数*/
                    sb.AppendLine("           , pm.PaymentId");
                    sb.AppendLine("           , pm.PaymentType");
                    sb.AppendLine("           , pm.Amount");
                    sb.AppendLine("           , pm.ReceiptNo");
                    sb.AppendLine("           from FreeTicketManage ftm");
                    sb.AppendLine("          left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("          on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("          and fsm.BizCompanyCd='NNR001'");
                    sb.AppendLine("          inner join PaymentManage pm");
                    sb.AppendLine("          on ftm.UserId = pm.UserId");
                    sb.AppendLine("          and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("          and pm.ServiceId = '4'");
                    sb.AppendLine("          and pm.PaymentType = '4'");
                    sb.AppendLine("          and pm.GmoStatus = '1'");
                    sb.AppendLine("          and pm.GmoProcType = '2'");

                    return sb.ToString();
                }
            }

            /// <summary>
            /// 西鉄の決済情報リスト取得
            /// </summary>
            /// <param name="stDate">抽出範囲開始日</param>
            /// <param name="edDate">抽出範囲終了日</param>
            /// <returns>西鉄決済情報</returns>
            public List<NishitetsuPaymentInfo> GetNishitetsuPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string MyrouteNo, string TicketType, string PaymentType, string TicketNumType, string TransportType)
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();

                using (SqlDbInterface NishidbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder Nsb = new StringBuilder();
                    string NishitetsuInfo = GetALLGetNishitetsuPaymentDateQuery(stDate, edDate);
                    Nsb.AppendLine("select * from (" + NishitetsuInfo.ToString() + "");

                    if (MyrouteNo != "")
                    {
                        //検索条件にMyrouteID指定
                        Nsb.AppendLine("   and tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                    }
                    if (TransportType != "-")
                    {
                        //検索条件に券種指定
                        Nsb.AppendLine("   and tbl.TrsType = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                    }
                    if (TicketType != "-")
                    {
                        //検索条件に券種指定
                        Nsb.AppendLine("   and tbl.TicketType = @TicketType ");
                        cmd.Parameters.Add("@TicketType", SqlDbType.NVarChar).Value = TicketType;
                    }
                    if (PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        Nsb.AppendLine("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    else if (PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        Nsb.AppendLine("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        Nsb.AppendLine("   and tbl.ChildNum = '0' ");
                    }
                    else if (TicketNumType == "子供")
                    {
                        //検索条件に枚数種別：子供
                        Nsb.AppendLine("   and tbl.AdultNum = '0' ");
                    }
                    Nsb.AppendLine("  ) as MA  where MA.RecNo between @PageNum and @ListEnd");

                    cmd.CommandText = Nsb.ToString();

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
                            TicketName = row["TicketName"].ToString(),
                            TicketType = row["TicketType"].ToString(),
                            TransportType = row["TrsType"].ToString(),
                            AdultNum = row["AdultNum"].ToString(),
                            ChildNum = row["ChildNum"].ToString(),
                            PaymentType = row["PaymentType"].ToString(),
                            Amount = (int)row["Amount"],
                            ReceiptNo = row["ReceiptNo"].ToString()
                        };
                        result.Add(infoN);
                    }
                    return result;
                }
            }
            /// <summary>
            /// 西鉄表示用決済情報リスト総数取得
            /// </summary>
            /// <param name="stDate"></param>
            /// <param name="edDate"></param>
            /// <returns></returns>
            public List<NishitetsuPaymentInfo> NishitetsuPaymentDateListMaxCount(DateTime stDate, DateTime edDate, string MyrouteNo, string TicketType, string PaymentType, string TicketNumType, string TransportType)
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();
                //現在表示されているリストの通し番号

                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder NishiSb = new StringBuilder();

                    string Nishiinfo = GetALLGetNishitetsuPaymentDateQuery(stDate, edDate);
                    NishiSb.AppendLine(Nishiinfo.ToString());

                    if (MyrouteNo != "")
                    {
                        //検索条件にMyrouteID指定
                        NishiSb.AppendLine("   and tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                    }
                    if (TransportType != "-")
                    {
                        //検索条件に券種指定
                        NishiSb.AppendLine("   and tbl.TrsType = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                    }
                    if (TicketType != "-")
                    {
                        //検索条件に券種指定
                        NishiSb.AppendLine("   and tbl.TicketType = @TicketType ");
                        cmd.Parameters.Add("@TicketType", SqlDbType.NVarChar).Value = TicketType;
                    }
                    if (PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        NishiSb.AppendLine("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    else if (PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        NishiSb.AppendLine("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        NishiSb.AppendLine("   and tbl.ChildNum = '0' ");
                    }
                    else if (TicketNumType == "子供")
                    {
                        //検索条件に枚数種別：子供
                        NishiSb.AppendLine("   and tbl.AdultNum = '0' ");
                    }

                    cmd.CommandText = NishiSb.ToString();

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

            /// <summary>
            /// 西鉄券種ドロップダウンリスト項目取得
            /// </summary>
            /// <returns></returns>
            public List<NishitetsuPaymentInfo> NishitetsuSelectList()
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();
                using (SqlDbInterface NTdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("select fsm.TicketType");
                    sb.AppendLine("   ,fsm.TicketName");
                    sb.AppendLine("   ,fsm.TrsType");
                    sb.AppendLine("   from FreeTicketSalesMaster fsm");

                    cmd.CommandText = sb.ToString();

                    DataTable dt = NTdbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        NishitetsuPaymentInfo info = new NishitetsuPaymentInfo
                        {
                            TicketType = row["TicketType"].ToString(),
                            TransportType = row["TrsType"].ToString(),
                            TicketName = row["TicketName"].ToString(),
                        };

                        result.Add(info);
                    }
                    return result;

                }

            }
        }
    }
}