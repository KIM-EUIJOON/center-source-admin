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
        public List<JtxPaymentInfo> GetJtxPaymentDate(DateTime stDate, DateTime edDate, int pageNo,int ListNoEnd)
         {
             List<JtxPaymentInfo> result = new List<JtxPaymentInfo>();
             //現在表示されているリストの通し番号

             using (SqlDbInterface dbInterface = new SqlDbInterface())
             using (SqlCommand cmd = new SqlCommand())
             {
                 StringBuilder jtxsb = new StringBuilder();
                 
                 string Jtxinfo = GetALLJtxPaymentDateQuery(stDate, edDate);
                 jtxsb.Append("select * from");
                 jtxsb.Append(" (" + Jtxinfo.ToString() + ") as MA");
                 jtxsb.Append("    where MA.RecNo between @PageNum and @PageNumEnd");      //表示する件数分の情報のみ取得する

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
                jtxsb.Append(Jtxinfo.ToString());

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

                sb.Append("select ROW_NUMBER() OVER(ORDER BY mr.PaymentId, pm.PaymentType) as RecNo");
                sb.Append("     , mr.UserId");
                sb.Append("     , pm.TranDate");
                sb.Append("     ,mr.PaymentId");
                sb.Append("     ,mr.CompanyName");
                sb.Append("     ,mr.OrderId");
                sb.Append("     ,case when pm.PaymentType = '3' then N'即時決済'");
                sb.Append("           when pm.PaymentType = '4' then N'払戻し'");
                sb.Append("           when pm.PaymentType = '5' then N'取消'");
                sb.Append("           else N'決済種別不明'end as PaymentType");
                sb.Append("     ,pm.Amount");
                sb.Append(" from MobilityReserve mr");
                sb.Append(" inner join PaymentManage pm");
                sb.Append("    on mr.UserId = pm.UserId");
                sb.Append("   and mr.PaymentId = pm.PaymentId");
                sb.Append("   and pm.ServiceId = '1'");     // サービスID(JTX)
                sb.Append("   and pm.PaymentType = '3'");   // 即時決済
                sb.Append("   and pm.GmoStatus = '1'");     // 成功
                sb.Append("   and pm.GmoProcType = '2'");   // 決済実行
                sb.Append(" where mr.Payment = 1");         // クレジット利用
                sb.Append("   and pm.TranDate between @StartDatatTime and @EndDatatTime");
                sb.Append("   and mr.Status = '11'");       //決済済み
                sb.Append("   and mr.TrsType = '20'");      //タクシー
                //決済失敗の未処理分は含めない
                sb.Append("   and not exists(");
                sb.Append("       select 1");
                sb.Append("         from PaymentError pe");
                sb.Append("        where pe.UserId = mr.UserId");
                sb.Append("          and pe.PaymentId = mr.PaymentId");
                sb.Append("          and pe.ServiceId = '1'");      //サービスID(JTX)
                sb.Append("          and pe.IsTreat = 0");          //運用未処理
                sb.Append("     )");

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
                    sb.Append("select gps.PassportId");
                    sb.Append("     ,esfm.Title,gps.Language");
                    sb.Append("     from GourmetPassportSales gps");
                    sb.Append("     left join EventSpotFeatureMaster esfm");
                    sb.Append("     on gps.FeatureId = esfm.FeatureId");
                    sb.Append("     where gps.Language='ja'");
                    sb.Append("     and esfm.Language='ja'");

                    NasseSb.Append(sb.ToString());
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
                    Nsb.Append("select * from");
                    if (PassID == "-")
                    {
                        //検索条件にパスポートIDが設定されていない場合は条件を追加しない
                        Nsb.Append(" (" + NasseInfo.ToString() + ") as MA");
                    }
                    else
                    {
                        //検索条件にパスポートIDが設定されている場合は条件を追加する
                        Nsb.Append(" (" + NasseInfo.ToString() + "   and gp.PassportId = @PassportId ) as MA");
                    }
                    //表示する件数分の情報のみ取得する
                    Nsb.Append("    where MA.RecNo between @PageNum and @ListEnd");
                    
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
                    sb.Append("select ROW_NUMBER() OVER(ORDER BY pm.PaymentId, pm.PaymentType) as RecNo");      //決済IDと決済種別でソートする
                    sb.Append("     , gp.UserId");
                    sb.Append("     , pm.TranDate");
                    sb.Append("     ,gp.PassportId");
                    sb.Append("     ,esfm.Title");
                    sb.Append("     ,pm.PaymentId");
                    sb.Append("     ,case when pm.PaymentType = '3' then N'即時決済'");
                    sb.Append("           else N'決済種別不明'end as PaymentType");
                    sb.Append("     ,pm.Amount");
                    sb.Append(" from GourmetPassport gp");
                    sb.Append(" inner join GourmetPassportSales gps");
                    sb.Append("    on gp.PassportId = gps.PassportId");
                    sb.Append("   and gps.Language = 'ja'");
                    sb.Append("   inner join PaymentManage pm");
                    sb.Append("   on gp.UserId = pm.UserId");
                    sb.Append("   and gp.PaymentId = pm.PaymentId");
                    sb.Append("   and gp.OrderNo = pm.OrderNo");
                    sb.Append("   left join EventSpotFeatureMaster esfm");
                    sb.Append("   on gps.FeatureId = esfm.FeatureId");
                    sb.Append("   and esfm.Language='ja'");
                    sb.Append("   where not exists(");
                    sb.Append("       select 1");
                    sb.Append("         from PaymentError pe");
                    sb.Append("        where pe.UserId = gp.UserId");
                    sb.Append("          and pe.PaymentId = pm.PaymentId");
                    sb.Append("          and pe.PaymentType = pm.PaymentType");
                    sb.Append("           and pe.ServiceId = '3'");     // サービスID(ナッセ)
                    sb.Append("           and pe.IsTreat = 0");         // 運用未処置
                    sb.Append("     )");
                    sb.Append("   and pm.ServiceId = '3'");           // サービスID(ナッセ)
                    sb.Append("   and pm.PaymentType = '3'");                // 即時決済(購入)
                    sb.Append("   and pm.GmoStatus = '1'");                  // 成功
                    sb.Append("   and pm.GmoProcType = '2'");                // 決済実行
                    sb.Append("   and pm.TranDate between @StartDatatTime and @EndDatatTime");                // 決済実行

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
                        NasseSb.Append( Nasseinfo.ToString() + "   and gp.PassportId = @PassportId ");
                        cmd.Parameters.Add("@PassportId", SqlDbType.NVarChar).Value = PassID;
                    }
                    
                    NasseSb.Append(Nasseinfo.ToString());

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
                using (SqlDbInterface NassedbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    string BusPayment = NishitetsuBusPayment();
                    string PaymentList = GetNishitetsuPaymentList();

                    sb.Append("select ROW_NUMBER() OVER(ORDER BY tbl.PaymentId, tbl.PaymentType) as RecNo");    //決済IDと決済種別でソートする
                    sb.Append("     , tbl.UserId");
                    sb.Append("     , tbl.TranDate");
                    sb.Append("     , case when tbl.TrsType =N'10' then N'鉄道'");
                    sb.Append("     when tbl.TrsType =N'14' then N'バス'");
                    sb.Append("     else N'チケット種別不明' end as TrsType");                                  /*チケット種別(交通手段)*/
                    sb.Append("     , tbl.TicketType");
                    sb.Append("     , tbl.TicketName");
                    sb.Append("     , tbl.AdultNum");
                    sb.Append("     , tbl.ChildNum");
                    sb.Append("     , tbl.PaymentId");
                    sb.Append("     , case when tbl.PaymentType = '3' then N'即時決済'");
                    sb.Append("           when tbl.PaymentType = '4' then N'払戻し'");
                    sb.Append("           when tbl.PaymentType = '5' then N'取消'");
                    sb.Append("           else N'決済種別不明' end as PaymentType");
                    sb.Append("     , tbl.Amount");
                    sb.Append("     , tbl.ReceiptNo");
                    sb.Append("  from (");
                    // 即時決済データ取得
                    //暫定処理
                    sb.Append(" " + BusPayment.ToString() + "");        //バスチケットのみ
                    sb.Append("        union all  ");
                    sb.Append(" " + PaymentList.ToString() + "");        //鉄道チケットのみ
                    sb.Append("      ) tbl");
                    // 決済エラー分は含めない
                    sb.Append("  where not exists(");
                    sb.Append("        select 1");
                    sb.Append("          from PaymentError pe");
                    sb.Append("         where pe.UserId = tbl.UserId");
                    sb.Append("           and pe.PaymentId = tbl.PaymentId");
                    sb.Append("           and pe.PaymentType = tbl.PaymentType");
                    sb.Append("           and pe.ServiceId = '2'");     // サービスID(西鉄)
                    sb.Append("           and pe.IsTreat = 0");         // 運用未処置
                    sb.Append("     )");
                    sb.Append("   and tbl.TranDate between @StartDatatTime and @EndDatatTime ");

                    return sb.ToString();
                }
            }
            private string NishitetsuBusPayment()
            {
                using (SqlDbInterface NassedbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("       select pm.TranDate");
                    sb.Append("            , nft.UserId");
                    sb.Append("            , fsm.TrsType");
                    sb.Append("            , N'売上' as Summary");
                    sb.Append("            , nft.TicketType");
                    sb.Append("            , fsm.TicketName");
                    sb.Append("            , nft.AdultNum");
                    sb.Append("            , nft.ChildNum ");
                    sb.Append("            , pm.PaymentId");
                    sb.Append("            , pm.PaymentType");
                    sb.Append("            , pm.Amount");
                    sb.Append("            , pm.ReceiptNo");
                    sb.Append("         from NishitetsuFreeTicket nft");
                    sb.Append("        inner join PaymentManage pm");
                    sb.Append("           on nft.UserId = pm.UserId");
                    sb.Append("          and nft.PaymentId = pm.PaymentId");
                    sb.Append("          and pm.ServiceId = '2'");                  // サービスID(西鉄)
                    sb.Append("          and pm.PaymentType = '3'");                // 即時決済(購入)
                    sb.Append("          and pm.GmoStatus = '1'");                  // 成功
                    sb.Append("          and pm.GmoProcType = '2'");                // 決済実行
                    sb.Append("          left join FreeTicketSalesMaster fsm");
                    sb.Append("          on nft.TicketType = fsm.TicketType");
                    sb.Append("          and fsm.BizCompanyCd='NNR002'");           //西鉄
                    sb.Append("          and fsm.TrsType ='14'");                   //バス情報のみ取得
                    sb.Append("        union all  ");
                    // 払戻し返金データ取得
                    sb.Append("        select pm.TranDate");
                    sb.Append("             , nft.UserId");
                    sb.Append("            , fsm.TrsType");
                    sb.Append("             , N'売上' as Summary");
                    sb.Append("            , nft.TicketType");
                    sb.Append("            , fsm.TicketName");
                    sb.Append("             , nft.AdultNum");
                    sb.Append("             , nft.ChildNum ");
                    sb.Append("             , pm.PaymentId");
                    sb.Append("             , pm.PaymentType");
                    sb.Append("             , pm.Amount * -1 as Amount");
                    sb.Append("             , pm.ReceiptNo");
                    sb.Append("          from NishitetsuFreeTicket nft");
                    sb.Append("         inner join PaymentManage pm");
                    sb.Append("            on nft.UserId = pm.UserId");
                    sb.Append("           and nft.PaymentId = pm.PaymentId");
                    sb.Append("           and pm.ServiceId = '2'");                 // サービスID(西鉄)
                    sb.Append("           and pm.PaymentType = '5'");               // 取消(返金)
                    sb.Append("           and pm.GmoStatus = '1'");                 // 成功
                    sb.Append("           and pm.GmoProcType = '3'");               // 決済変更
                    sb.Append("          left join FreeTicketSalesMaster fsm");
                    sb.Append("          on nft.TicketType = fsm.TicketType");
                    sb.Append("          and fsm.BizCompanyCd='NNR002'");           //西鉄
                    sb.Append("          and fsm.TrsType ='14'");                   //バス情報のみ取得
                    sb.Append("        union all");
                    // 払戻し手数料取得
                    sb.Append("        select pm.TranDate ");
                    sb.Append("             , nft.UserId");
                    sb.Append("            , fsm.TrsType");
                    sb.Append("             , N'払戻し' as Summary");
                    sb.Append("            , nft.TicketType");
                    sb.Append("            , fsm.TicketName");
                    sb.Append("             , nft.AdultNum");
                    sb.Append("             , nft.ChildNum ");
                    sb.Append("             , pm.PaymentId");
                    sb.Append("             , pm.PaymentType");
                    sb.Append("             , pm.Amount");
                    sb.Append("             , pm.ReceiptNo");
                    sb.Append("          from NishitetsuFreeTicket nft");
                    sb.Append("         inner join PaymentManage pm");
                    sb.Append("            on nft.UserId = pm.UserId");
                    sb.Append("           and nft.PaymentId = pm.PaymentId");
                    sb.Append("           and nft.RefundOrderNo = pm.OrderNo");
                    sb.Append("           and pm.ServiceId = '2'");                 // サービスID(西鉄)
                    sb.Append("           and pm.PaymentType = '4'");               // 払戻し(手数料徴収)
                    sb.Append("           and pm.GmoStatus = '1'");                 // 成功
                    sb.Append("           and pm.GmoProcType = '2'");               // 決済実行
                    sb.Append("          left join FreeTicketSalesMaster fsm");
                    sb.Append("          on nft.TicketType = fsm.TicketType");
                    sb.Append("          and fsm.BizCompanyCd='NNR002'");           //西鉄
                    sb.Append("          and fsm.TrsType ='14'");                   //バス情報のみ取得

                    return sb.ToString();
                }
            }
            private string GetNishitetsuPaymentList()
            {
                using (SqlDbInterface NassedbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("        select pm.TranDate");
                    sb.Append("           ,ftm.UserId");
                    sb.Append("           ,fsm.TrsType");
                    sb.Append("           , N'売上' as Summary");
                    sb.Append("           ,fsm.TicketType");
                    sb.Append("            ,fsm.TicketName");   /*チケット名称*/
                    sb.Append("           , ftm.AdultNum"); /*大人枚数*/
                    sb.Append("           , ftm.ChildNum"); /*子供枚数*/
                    sb.Append("           , pm.PaymentId");
                    sb.Append("           , pm.PaymentType");
                    sb.Append("           , pm.Amount");
                    sb.Append("           , pm.ReceiptNo");
                    sb.Append("           from FreeTicketManage ftm");
                    sb.Append("          left join FreeTicketSalesMaster fsm");
                    sb.Append("          on ftm.TicketId = fsm.TicketId");
                    sb.Append("          and fsm.BizCompanyCd='NNR001'");
                    sb.Append("          inner join PaymentManage pm");
                    sb.Append("          on ftm.UserId = pm.UserId");
                    sb.Append("          and ftm.PaymentId = pm.PaymentId");
                    sb.Append("          and pm.ServiceId = '2'");
                    sb.Append("          and pm.PaymentType = '3'");
                    sb.Append("          and pm.GmoStatus = '1'");
                    sb.Append("          and pm.GmoProcType = '2'");
                    sb.Append("          union all");
                    sb.Append("          ");    /*払戻し返金データ取得*/
                    sb.Append("          select pm.TranDate");
                    sb.Append("           ,ftm.UserId");
                    sb.Append("           ,fsm.TrsType");
                    sb.Append("           , N'売上' as Summary");
                    sb.Append("           ,fsm.TicketType");
                    sb.Append("            ,fsm.TicketName");   /*チケット名称*/
                    sb.Append("           , ftm.AdultNum"); /*大人枚数*/
                    sb.Append("           , ftm.ChildNum"); /*子供枚数*/
                    sb.Append("           , pm.PaymentId");
                    sb.Append("           , pm.PaymentType");
                    sb.Append("           , pm.Amount* -1 as Amount");
                    sb.Append("           , pm.ReceiptNo");
                    sb.Append("           from FreeTicketManage ftm");
                    sb.Append("          left join FreeTicketSalesMaster fsm");
                    sb.Append("          on ftm.TicketId = fsm.TicketId");
                    sb.Append("          and fsm.BizCompanyCd='NNR001'");
                    sb.Append("          inner join PaymentManage pm");
                    sb.Append("          on ftm.UserId = pm.UserId");
                    sb.Append("          and ftm.PaymentId = pm.PaymentId");
                    sb.Append("          and pm.ServiceId = '2'");
                    sb.Append("          and pm.PaymentType = '5'");
                    sb.Append("          and pm.GmoStatus = '1'");
                    sb.Append("          and pm.GmoProcType = '3'");
                    sb.Append("          union all");
                    /*払戻し手数料取得*/
                    sb.Append("          select pm.TranDate");
                    sb.Append("           ,ftm.UserId");
                    sb.Append("           ,fsm.TrsType");
                    sb.Append("           , N'払戻し' as Summary");
                    sb.Append("           ,fsm.TicketType");
                    sb.Append("            ,fsm.TicketName");   /*チケット名称*/
                    sb.Append("           , ftm.AdultNum"); /*大人枚数*/
                    sb.Append("           , ftm.ChildNum"); /*子供枚数*/
                    sb.Append("           , pm.PaymentId");
                    sb.Append("           , pm.PaymentType");
                    sb.Append("           , pm.Amount");
                    sb.Append("           , pm.ReceiptNo");
                    sb.Append("           from FreeTicketManage ftm");
                    sb.Append("          left join FreeTicketSalesMaster fsm");
                    sb.Append("          on ftm.TicketId = fsm.TicketId");
                    sb.Append("          and fsm.BizCompanyCd='NNR001'");
                    sb.Append("          inner join PaymentManage pm");
                    sb.Append("          on ftm.UserId = pm.UserId");
                    sb.Append("          and ftm.PaymentId = pm.PaymentId");
                    sb.Append("          and pm.ServiceId = '2'");
                    sb.Append("          and pm.PaymentType = '4'");
                    sb.Append("          and pm.GmoStatus = '1'");
                    sb.Append("          and pm.GmoProcType = '2'");

                    return sb.ToString();
                }
            }

                    /// <summary>
                    /// 西鉄の決済情報リスト取得
                    /// </summary>
                    /// <param name="stDate">抽出範囲開始日</param>
                    /// <param name="edDate">抽出範囲終了日</param>
                    /// <returns>西鉄決済情報</returns>
                    public List<NishitetsuPaymentInfo> GetNishitetsuPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string MyrouteNo, string TicketType, string PaymentType , string TicketNumType,string TransportType)
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();

                using (SqlDbInterface NishidbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder Nsb = new StringBuilder();
                    string NishitetsuInfo = GetALLGetNishitetsuPaymentDateQuery(stDate, edDate);
                    Nsb.Append("select * from (" + NishitetsuInfo.ToString() +"");
                    
                    if (MyrouteNo != "")
                    {　
                        //検索条件にMyrouteID指定
                        Nsb.Append("   and tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                    }
                    if (TransportType != "-")
                    {
                        //検索条件に券種指定
                        Nsb.Append("   and tbl.TrsType = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                    }
                    if (TicketType != "-")
                    {
                        //検索条件に券種指定
                        Nsb.Append("   and tbl.TicketType = @TicketType ");
                        cmd.Parameters.Add("@TicketType", SqlDbType.NVarChar).Value = TicketType;
                    }
                    if (PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        Nsb.Append("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    else if (PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        Nsb.Append("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        Nsb.Append("   and tbl.ChildNum = '0' ");
                    }
                    else if (TicketNumType == "子供")
                    {
                        //検索条件に枚数種別：子供
                        Nsb.Append("   and tbl.AdultNum = '0' ");
                    }
                    Nsb.Append("  ) as MA  where MA.RecNo between @PageNum and @ListEnd");

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
                            ReceiptNo= row["ReceiptNo"].ToString()
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
            public List<NishitetsuPaymentInfo> NishitetsuPaymentDateListMaxCount(DateTime stDate, DateTime edDate, string MyrouteNo, string TicketType, string PaymentType, string TicketNumType,string TransportType)
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();
                //現在表示されているリストの通し番号

                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder NishiSb = new StringBuilder();

                    string Nishiinfo = GetALLGetNishitetsuPaymentDateQuery(stDate, edDate);
                    NishiSb.Append(Nishiinfo.ToString());

                    if (MyrouteNo != "")
                    {
                        //検索条件にMyrouteID指定
                        NishiSb.Append("   and tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                    }
                    if (TransportType != "-")
                    {
                        //検索条件に券種指定
                        NishiSb.Append("   and tbl.TrsType = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                    }
                    if (TicketType != "-")
                    {
                        //検索条件に券種指定
                        NishiSb.Append("   and tbl.TicketType = @TicketType ");
                        cmd.Parameters.Add("@TicketType", SqlDbType.NVarChar).Value = TicketType;
                    }
                    if (PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        NishiSb.Append("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    else if (PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        NishiSb.Append("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                    }
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        NishiSb.Append("   and tbl.ChildNum = '0' ");
                    }
                    else if (TicketNumType == "子供")
                    {
                        //検索条件に枚数種別：子供
                        NishiSb.Append("   and tbl.AdultNum = '0' ");
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

                    sb.Append("select fsm.TicketType");
                    sb.Append("   ,fsm.TicketName");
                    sb.Append("   ,fsm.TrsType");
                    sb.Append("   from FreeTicketSalesMaster fsm");

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