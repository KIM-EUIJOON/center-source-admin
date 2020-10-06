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
    
    // 決済データ取得クラス
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
        public List<JtxPaymentInfo> GetJtxPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string MyrouteNo, string Apltype)
        {
            List<JtxPaymentInfo> result = new List<JtxPaymentInfo>();
            //現在表示されているリストの通し番号

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder jtxsb = new StringBuilder();

                string Jtxinfo = GetALLJtxPaymentDateQuery(stDate, edDate);
                jtxsb.AppendLine("select * from");
                jtxsb.AppendLine(" (" + Jtxinfo.ToString() );
                if (MyrouteNo != "")
                {
                    jtxsb.AppendLine("    and mr.UserId = @MyrouteIdNo");
                    cmd.Parameters.Add("@MyrouteIdNo", SqlDbType.NVarChar).Value = MyrouteNo;
                }
                if (Apltype != "-")//au用Role番号判定
                {
                    jtxsb.AppendLine("   and uio.AplType = @AplType");
                    cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = Apltype;
                }
                jtxsb.AppendLine(") as MA");
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
                        Apltype = row["AplType"].ToString(),
                    };
                    if (row["AplType"].ToString() == "1")
                    {
                        info.Apltype = "au";
                    }
                    else
                    {
                        info.Apltype = "-";
                    }
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
        public List<JtxPaymentInfo> GetJtxPaymentDateListCount(DateTime stDate, DateTime edDate, string MyrouteNo, string Apltype)
        {
            List<JtxPaymentInfo> result = new List<JtxPaymentInfo>();
            //現在表示されているリストの通し番号


            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder jtxsb = new StringBuilder();

                string Jtxinfo = GetALLJtxPaymentDateQuery(stDate, edDate);
                jtxsb.AppendLine(Jtxinfo.ToString());

                if (MyrouteNo != "")
                {
                    jtxsb.AppendLine("    and mr.UserId = @MyrouteId");
                    cmd.Parameters.Add("@MyrouteId", SqlDbType.NVarChar).Value = MyrouteNo;
                }
                if (Apltype != "-")//au用Role番号判定
                {
                    jtxsb.AppendLine("   and uio.AplType = @AplType");
                    cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = Apltype;
                }

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
                sb.AppendLine("     ,uio.AplType");
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
                sb.AppendLine("   left join UserInfoOid uio");   // アプリ種別取得
                sb.AppendLine("   on  mr.UserId=uio.UserId");   
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
            public List<NassePaymentInfo> GetNassePaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string PassID, string MyrouteNo)
            {
                List<NassePaymentInfo> result = new List<NassePaymentInfo>();

                using (SqlDbInterface NassedbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder Nsb = new StringBuilder();
                    string NasseInfo = GetALLGetNassePaymentDateQuery(stDate, edDate);
                    Nsb.AppendLine("select * from (" + NasseInfo.ToString() );
                    if (PassID != "-")
                    {
                        //検索条件にパスポートIDが設定されている場合は条件を追加する
                        Nsb.AppendLine("   and gp.PassportId = @PassportId ");
                        cmd.Parameters.Add("@PassportId", SqlDbType.NVarChar).Value = PassID;
                    }

                    if (MyrouteNo != "")
                    {
                        //検索条件にユーザーIDが設定されている場合は条件を追加する
                        Nsb.AppendLine("    and gp.UserId = @MyrouteNo");
                        cmd.Parameters.Add("@MyrouteNo", SqlDbType.NVarChar).Value = MyrouteNo;
                    }


                    Nsb.AppendLine(" ) as MA");

                    //表示する件数分の情報のみ取得する
                    Nsb.AppendLine("    where MA.RecNo between @PageNum and @ListEnd");

                    cmd.CommandText = Nsb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");
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
            public List<NassePaymentInfo> NassePaymentDateListMaxCount(DateTime stDate, DateTime edDate, string PassID, string MyrouteNo)
            {
                List<NassePaymentInfo> result = new List<NassePaymentInfo>();
                //現在表示されているリストの通し番号

                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder NasseSb = new StringBuilder();

                    string Nasseinfo = GetALLGetNassePaymentDateQuery(stDate, edDate);
                    NasseSb.AppendLine(Nasseinfo.ToString());

                    if (PassID != "-")
                    {
                        //検索条件にパスポートIDが設定されている場合は条件を追加する
                        NasseSb.AppendLine("   and gp.PassportId = @PassportId ");
                        cmd.Parameters.Add("@PassportId", SqlDbType.NVarChar).Value = PassID;
                    }
                    if (MyrouteNo != "")
                    {
                        //検索条件にユーザーIDが設定されている場合は条件を追加する
                        NasseSb.AppendLine("   and gp.UserId = @MyrouteNo ");
                        cmd.Parameters.Add("@MyrouteNo", SqlDbType.NVarChar).Value = MyrouteNo;
                    }

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

                    string PaymentList = GetNishitetsuPaymentList();
                    string BusPayment = NishitetsuBusPayment();

                    sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY tbl.PaymentId, tbl.PaymentType) as RecNo");    //決済IDと決済種別でソートする
                    sb.AppendLine("     , tbl.UserId");
                    sb.AppendLine("     , tbl.TranDate");
                    sb.AppendLine("     , case when tbl.BizCompanyCd =N'NNR' then N'鉄道'");
                    sb.AppendLine("     when tbl.BizCompanyCd =N'NIS' then N'バス(福岡)'");
                    sb.AppendLine("     when tbl.BizCompanyCd =N'NISK' then N'バス(北九州)'");
                    sb.AppendLine("     when tbl.BizCompanyCd =N'NISG' then N'マルチ'");
                    sb.AppendLine("     else N'チケット種別不明' end as BizCompanyCd");                                  /*チケット種別(交通手段)*/
                    sb.AppendLine("     , tbl.TicketType");                                                              /*チケット種別(au,au以外)*/
                    sb.AppendLine("     , tbl.TicketId");
                    sb.AppendLine("     , tbl.TicketGroup");
                    sb.AppendLine("     , tbl.Value");                                                                   /*チケット名称*/
                    sb.AppendLine("     , tbl.AdultNum");
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
                    // 即時決済データ取得
                    sb.AppendLine(" " + BusPayment.ToString() + "");        //旧テーブル取得
                    sb.AppendLine("        union all  ");
                    sb.AppendLine(" " + PaymentList.ToString() + "");        //新テーブル取得
                    sb.AppendLine("      ) tbl");
                    // 決済エラー分は含めない
                    sb.AppendLine("  where not exists(");
                    sb.AppendLine("        select 1");
                    sb.AppendLine("          from PaymentError pe");
                    sb.AppendLine("         where pe.UserId = tbl.UserId");
                    sb.AppendLine("           and pe.PaymentId = tbl.PaymentId");
                    sb.AppendLine("           and pe.PaymentType = tbl.PaymentType");
                    sb.AppendLine("        	and(pe.ServiceId = '2' or pe.ServiceId = '4' or pe.ServiceId = '5' or pe.ServiceId = '6')");/*サービスID(西鉄バス(福岡):2,鉄道:4,西鉄バス(北九州):5,にしてつグループ:6)*/
                    sb.AppendLine("           and pe.IsTreat = 0");         // 運用未処置
                    sb.AppendLine("     )");
                    sb.AppendLine("   and tbl.TranDate between @StartDatatTime and @EndDatatTime ");

                    return sb.ToString();
                }
            }

            /// <summary>
            /// 西鉄バス旧DB内容取得
            /// </summary>
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
                    sb.AppendLine("            , cr.Value");
                    sb.AppendLine("            , nft.AdultNum");
                    sb.AppendLine("            , nft.ChildNum ");
                    sb.AppendLine("            , pm.PaymentId");
                    sb.AppendLine("            , pm.PaymentType");
                    sb.AppendLine("            , pm.Amount");
                    sb.AppendLine("            , pm.ReceiptNo");
                    sb.AppendLine("            ,fsm.BizCompanyCd");
                    sb.AppendLine("            ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("            ,fsm.TicketGroup");
                    sb.AppendLine("            ,uio.AplType");                       /*アプリ種別*/
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
                    sb.AppendLine("          and fsm.BizCompanyCd='NIS'");              //西鉄
                    sb.AppendLine("          and fsm.TrsType ='11'");                   //バス情報のみ取得
                    sb.AppendLine("          and fsm.TicketId in ('3', '4')");          //福岡6H、24Hに限定する
                    sb.AppendLine("          left join CharacterResource cr");
                    sb.AppendLine("          on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("          and Language = 'ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on nft.UserId=uio.UserId");
                    sb.AppendLine("        union all  ");
                    // 払戻し返金データ取得
                    sb.AppendLine("        select pm.TranDate");
                    sb.AppendLine("             , nft.UserId");
                    sb.AppendLine("            , fsm.TrsType");
                    sb.AppendLine("             , N'売上' as Summary");
                    sb.AppendLine("            , nft.TicketType");
                    sb.AppendLine("            , cr.Value");
                    sb.AppendLine("             , nft.AdultNum");
                    sb.AppendLine("             , nft.ChildNum ");
                    sb.AppendLine("             , pm.PaymentId");
                    sb.AppendLine("             , pm.PaymentType");
                    sb.AppendLine("             , pm.Amount * -1 as Amount");
                    sb.AppendLine("             , pm.ReceiptNo");
                    sb.AppendLine("            ,fsm.BizCompanyCd");
                    sb.AppendLine("            ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("            ,fsm.TicketGroup");
                    sb.AppendLine("            ,uio.AplType");                       /*アプリ種別*/
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
                    sb.AppendLine("          and fsm.BizCompanyCd='NIS'");           //西鉄
                    sb.AppendLine("          and fsm.TrsType ='11'");                   //バス情報のみ取得
                    sb.AppendLine("          and fsm.TicketId in ('3', '4')");          //福岡6H、24Hに限定する
                    sb.AppendLine("          left join CharacterResource cr");
                    sb.AppendLine("          on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("          and Language = 'ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on nft.UserId=uio.UserId");
                    sb.AppendLine("        union all");
                    // 払戻し手数料取得
                    sb.AppendLine("        select pm.TranDate ");
                    sb.AppendLine("             , nft.UserId");
                    sb.AppendLine("            , fsm.TrsType");
                    sb.AppendLine("             , N'払戻し' as Summary");
                    sb.AppendLine("            , nft.TicketType");
                    sb.AppendLine("            , cr.Value");
                    sb.AppendLine("             , nft.AdultNum");
                    sb.AppendLine("             , nft.ChildNum ");
                    sb.AppendLine("             , pm.PaymentId");
                    sb.AppendLine("             , pm.PaymentType");
                    sb.AppendLine("             , pm.Amount");
                    sb.AppendLine("             , pm.ReceiptNo");
                    sb.AppendLine("            ,fsm.BizCompanyCd");
                    sb.AppendLine("            ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("            ,fsm.TicketGroup");
                    sb.AppendLine("            ,uio.AplType");                       /*アプリ種別*/
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
                    sb.AppendLine("          and fsm.BizCompanyCd='NIS'");           //西鉄
                    sb.AppendLine("          and fsm.TrsType ='11'");                   //バス情報のみ取得
                    sb.AppendLine("          and fsm.TicketId in ('3', '4')");          //福岡6H、24Hに限定する
                    sb.AppendLine("          left join CharacterResource cr");
                    sb.AppendLine("          on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("          and Language = 'ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on nft.UserId=uio.UserId");
                    return sb.ToString();
                }
            }

            /// <summary>
            /// 西鉄DB内容取得
            /// </summary>
            private string GetNishitetsuPaymentList()
            {
                using (SqlDbInterface NishitetsudbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("        	select pm.TranDate");
                    sb.AppendLine("        	,ftm.UserId");
                    sb.AppendLine("        	,fsm.TrsType");
                    sb.AppendLine("        	, N'売上' as Summary");
                    sb.AppendLine("        	,fsm.TicketType");
                    sb.AppendLine("        	,cr.Value");                        /*チケット名称(日本語)*/
                    sb.AppendLine("        	, ftm.AdultNum ");                  /*大人枚数*/
                    sb.AppendLine("        	, ftm.ChildNum ");                  /*子供枚数*/
                    sb.AppendLine("        	, pm.PaymentId");
                    sb.AppendLine("        	, pm.PaymentType");
                    sb.AppendLine("        	, pm.Amount");
                    sb.AppendLine("        	, pm.ReceiptNo");
                    sb.AppendLine("        	,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケット種別(au,au以外)*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                    sb.AppendLine("        	from FreeTicketManage ftm");
                    sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("        	and (fsm.BizCompanyCd='NISK' or fsm.BizCompanyCd='NIS' or fsm.BizCompanyCd='NNR' or fsm.BizCompanyCd='NISG')");
                    sb.AppendLine("        	inner join PaymentManage pm");
                    sb.AppendLine("        	on ftm.UserId = pm.UserId");
                    sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("        	and(pm.ServiceId = '2' or pm.ServiceId = '4' or pm.ServiceId = '5' or pm.ServiceId = '6')");/*サービスID(西鉄バス(福岡):2,鉄道:4,西鉄バス(北九州):5,にしてつグループ:6)*/
                    sb.AppendLine("        	and pm.PaymentType = '3'");
                    sb.AppendLine("        	and pm.GmoStatus = '1'");
                    sb.AppendLine("        	and pm.GmoProcType = '2'");
                    sb.AppendLine("        	left join CharacterResource cr");
                    sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("        	and Language ='ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on ftm.UserId=uio.UserId");
                    sb.AppendLine("        	union all");
                    /*払戻し返金データ取得*/
                    sb.AppendLine("        	select pm.TranDate");
                    sb.AppendLine("        	,ftm.UserId");
                    sb.AppendLine("        	,fsm.TrsType");
                    sb.AppendLine("        	, N'売上' as Summary");
                    sb.AppendLine("        	,fsm.TicketType");
                    sb.AppendLine("        	,cr.Value");                                /*チケット名称(日本語)*/
                    sb.AppendLine("        	, ftm.AdultNum ");                          /*大人枚数*/
                    sb.AppendLine("        	, ftm.ChildNum ");                          /*子供枚数*/
                    sb.AppendLine("        	, pm.PaymentId");
                    sb.AppendLine("        	, pm.PaymentType");
                    sb.AppendLine("        	, pm.Amount* -1 as Amount");
                    sb.AppendLine("        	, pm.ReceiptNo");
                    sb.AppendLine("        	,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                    sb.AppendLine("        	from FreeTicketManage ftm");
                    sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("        	and (fsm.BizCompanyCd='NISK' or fsm.BizCompanyCd='NIS' or fsm.BizCompanyCd='NNR' or fsm.BizCompanyCd='NISG')");
                    sb.AppendLine("        	inner join PaymentManage pm");
                    sb.AppendLine("        	on ftm.UserId = pm.UserId");
                    sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("        	and(pm.ServiceId = '2' or pm.ServiceId = '4' or pm.ServiceId = '5' or pm.ServiceId = '6')");/*サービスID(西鉄バス(福岡):2,鉄道:4,西鉄バス(北九州):5,にしてつグループ:6)*/
                    sb.AppendLine("        	and pm.PaymentType = '5'");
                    sb.AppendLine("        	and pm.GmoStatus = '1'");
                    sb.AppendLine("        	and pm.GmoProcType = '3'");
                    sb.AppendLine("        	left join CharacterResource cr");
                    sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("        	and Language ='ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on ftm.UserId=uio.UserId");
                    sb.AppendLine("        	union all");
                    /*払戻し手数料取得*/
                    sb.AppendLine("        	select pm.TranDate");
                    sb.AppendLine("        	,ftm.UserId");
                    sb.AppendLine("        	,fsm.TrsType");
                    sb.AppendLine("        	, N'払戻し' as Summary");
                    sb.AppendLine("        	,fsm.TicketType");
                    sb.AppendLine("        	,cr.Value");                        /*チケット名称*/
                    sb.AppendLine("        	, ftm.AdultNum ");                  /*大人枚数*/
                    sb.AppendLine("        	, ftm.ChildNum ");                  /*子供枚数*/
                    sb.AppendLine("        	, pm.PaymentId");
                    sb.AppendLine("        	, pm.PaymentType");
                    sb.AppendLine("        	, pm.Amount");
                    sb.AppendLine("        	, pm.ReceiptNo");
                    sb.AppendLine("        	,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("         ,uio.AplType");                       /*アプリ種別*/
                    sb.AppendLine("        	from FreeTicketManage ftm");
                    sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("        	and (fsm.BizCompanyCd='NISK' or fsm.BizCompanyCd='NIS' or fsm.BizCompanyCd='NNR' or fsm.BizCompanyCd='NISG')");
                    sb.AppendLine("        	inner join PaymentManage pm");
                    sb.AppendLine("        	on ftm.UserId = pm.UserId");
                    sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("        	and(pm.ServiceId = '2' or pm.ServiceId = '4' or pm.ServiceId = '5' or pm.ServiceId = '6')");/*サービスID(西鉄バス(福岡):2,鉄道:4,西鉄バス(北九州):5,にしてつグループ:6)*/
                    sb.AppendLine("        	and pm.PaymentType = '4'");
                    sb.AppendLine("        	and pm.GmoStatus = '1'");
                    sb.AppendLine("        	and pm.GmoProcType = '2'");
                    sb.AppendLine("        	left join CharacterResource cr");
                    sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("        	and Language ='ja'");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on ftm.UserId=uio.UserId");
                    return sb.ToString();
                }
            }

            /// <summary>
            /// 西鉄の決済情報リスト取得
            /// </summary>
            /// <param name="stDate">抽出範囲開始日</param>
            /// <param name="edDate">抽出範囲終了日</param>
            /// <returns>西鉄決済情報</returns>
            public List<NishitetsuPaymentInfo> GetNishitetsuPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string MyrouteNo, string PaymentType, string TicketNumType, string TransportType,string TicketId, string AplType)
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
                        Nsb.AppendLine("   and tbl.BizCompanyCd = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
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
                    if (TicketId != "-")
                    {
                        //検索条件に券種指定
                        Nsb.AppendLine("   and tbl.TicketId = @TicketId ");
                        cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                    }
                    if (AplType != "-")//au用Role番号判定
                    {
                        Nsb.AppendLine("   and tbl.AplType = @AplType");
                        cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = AplType;
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
                            TicketName = row["Value"].ToString(),
                            TicketId = row["TicketId"].ToString(),
                            TransportType = row["BizCompanyCd"].ToString(),
                            AdultNum = row["AdultNum"].ToString(),
                            ChildNum = row["ChildNum"].ToString(),
                            PaymentType = row["PaymentType"].ToString(),
                            Amount = (int)row["Amount"],
                            ReceiptNo = row["ReceiptNo"].ToString(),
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
            /// <param name="MyrouteNo"></param>
            /// <param name="TicketType"></param>
            /// <param name="PaymentType"></param>
            /// <param name="TicketNumType"></param>
            /// <param name="TransportType"></param>
            /// <returns></returns>
            public List<NishitetsuPaymentInfo> NishitetsuPaymentDateListMaxCount(DateTime stDate, DateTime edDate, string MyrouteNo, string PaymentType, string TicketNumType, string TransportType ,string TicketId, string AplType)
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
                        NishiSb.AppendLine("   and tbl.BizCompanyCd = @TransportType ");
                        cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
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
                    if (TicketId != "-")
                    {
                        //検索条件に券種指定
                        NishiSb.AppendLine("   and tbl.TicketId = @TicketId ");
                        cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                    }
                    if (AplType != "-")//au用Role番号判定
                    {
                        NishiSb.AppendLine("   and tbl.AplType = @AplType");
                        cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = AplType;
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
            public List<NishitetsuPaymentInfo> NishitetsuSelectList(string UserRole)
            {
                List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();
                using (SqlDbInterface NTdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
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

                    cmd.CommandText = sb.ToString();

                    DataTable dt = NTdbInterface.ExecuteReader(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        NishitetsuPaymentInfo info = new NishitetsuPaymentInfo
                        {
                            TicketType = row["TicketType"].ToString(),
                            TransportType = row["BizCompanyCd"].ToString(),
                            TicketName = row["Value"].ToString(),
                            TicketId = row["TicketId"].ToString(),
                        };
                        if (row["TicketGroup"].ToString()=="1")
                        {
                            info.TicketName = info.TicketName + "[au]";
                        }
						if (info.TransportType != "NIS"&info.TransportType!= "NISK"& info.TransportType != "NNR" & info.TransportType != "NISG") { continue; }//西鉄以外のものをリストに含めない
                        result.Add(info);
                    }
                    return result;

                }

            }
        }


        public class DocomoBikeShare
        {
            /// <summary>
            /// ドコモ・バイクシェア決済内容取得
            /// </summary>
            public DataTable GetPaymentDateList(DocomoPaymentInfoListEntity model)
            {
                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    // データ取得
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY dbsu.PaymentId, pm.PaymentType) as RecNo");
                    sb.AppendLine("     , dbsu.UserId");
                    sb.AppendLine("     , pm.TranDate");
                    sb.AppendLine("     ,dbsu.PaymentId");
                    sb.AppendLine("     ,dbsu.CycleBizId");
                    sb.AppendLine("     ,dbsu.ReserveId");
                    sb.AppendLine("     ,cr.Value");
                    sb.AppendLine("     ,case when uio.AplType =1 then 'au' ");
                    sb.AppendLine("     else ''");
                    sb.AppendLine("     end as AplName");
                    sb.AppendLine("     ,case when pm.PaymentType = '3' then N'即時決済'");
                    sb.AppendLine("           when pm.PaymentType = '4' then N'払戻し'");
                    sb.AppendLine("           when pm.PaymentType = '5' then N'取消'");
                    sb.AppendLine("           else N'決済種別不明'end as PaymentType");
                    sb.AppendLine("     ,pm.Amount");
                    sb.AppendLine(" from DocomoBicycleShareUsages dbsu");
                    sb.AppendLine(" left join DocomoBicycleShareEnterpriseMaster dbsem");
                    sb.AppendLine(" on dbsu.CycleBizId=dbsem.BizId");
                    sb.AppendLine(" left join CharacterResource cr");
                    sb.AppendLine(" on dbsem.BizName = cr.ResourceId");
                    sb.AppendLine(" and Language = 'ja'");
                    sb.AppendLine(" inner join PaymentManage pm");
                    sb.AppendLine("    on dbsu.UserId = pm.UserId");
                    sb.AppendLine("   and dbsu.PaymentId = pm.PaymentId");
                    sb.AppendLine("   and pm.ServiceId = '7' ");    /* サービスID(DBS)*/
                    sb.AppendLine("   and pm.PaymentType = '3'");  /* 即時決済*/
                    sb.AppendLine("   and pm.GmoStatus = '1'");     /* 成功*/
                    sb.AppendLine("   and pm.GmoProcType = '2'");   /* 決済実行*/
                    sb.AppendLine("   left join UserInfoOid uio");   /* アプリ種別取得*/
                    sb.AppendLine("   on  dbsu.UserId=uio.UserId");
                    sb.AppendLine(" where pm.TranDate between @StartDatatTime and @EndDatatTime ");        /* クレジット利用*/
                    sb.AppendLine("   and dbsu.Status = '9'");       /*決済済み*/
                                                                      /*決済失敗の未処理分は含めない*/
                    sb.AppendLine("   and not exists(");
                    sb.AppendLine("       select 1");
                    sb.AppendLine("         from PaymentError pe");
                    sb.AppendLine("        where pe.UserId = dbsu.UserId");
                    sb.AppendLine("          and pe.PaymentId = dbsu.PaymentId");
                    sb.AppendLine("          and pe.ServiceId = '7' ");     /*サービスID(DBS)*/
                    sb.AppendLine("          and pe.IsTreat = 0");          /*運用未処理*/
                    sb.AppendLine("     )");
                    // 検索条件
                    if (false == string.IsNullOrEmpty(model.UserId))
                    {
                        sb.AppendLine("    AND dbsu.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = model.UserId;
                    }
                    if (false == string.IsNullOrEmpty(model.Apltype))
                    {
                        sb.AppendLine("    AND uio.AplType = @AplType ");
                        cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = model.Apltype;
                    }

                    cmd.CommandText = sb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = model.TargetDateBegin;
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = model.TargetDateEnd + " 23:59:59";
                    return dbInterface.ExecuteReader(cmd);
                }
            }
        }

        public class YokohamaPayment
        {

            /// <summary>
            /// 横浜決済情報取得
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            public DataTable GetPaymentDateList(YokohamaPaymentInfo model)
            {
                using (SqlDbInterface dbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    // データ取得
                    StringBuilder Ysb = new StringBuilder();
                    string YokohamaInfo = GetALLGetYokohamaPaymentDateQuery();
                    Ysb.AppendLine("select * from (" + YokohamaInfo.ToString() + "");


                    if (false == string.IsNullOrEmpty(model.UserId))
                    {
                        Ysb.AppendLine("    AND tbl.UserId = @UserId");
                        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = model.UserId;
                    }
                    if (false == string.IsNullOrEmpty(model.Apltype))
                    {
                        Ysb.AppendLine("    AND tbl.AplName = @AplName ");
                        cmd.Parameters.Add("@AplName", SqlDbType.NVarChar).Value = "au";
                    }
                    if (false == string.IsNullOrEmpty(model.TicketId))
                    {
                        Ysb.AppendLine("   and tbl.TicketId = @TicketId  ");
                        cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = model.TicketId;
                    }
                    if (model.PaymentType == "決済種別不明")
                    {
                        //検索条件に決済種別：決済種別不明指定
                        Ysb.AppendLine("   and tbl.PaymentType not in ('3','4','5')");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = model.PaymentType;
                    }
                    else if (model.PaymentType != "-")
                    {
                        //検索条件に決済種別指定
                        Ysb.AppendLine("   and tbl.PaymentType = @PaymentType ");
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = model.PaymentType;
                    }
                    if (model.TicketNumType == "1")
                    {
                        //検索条件に枚数種別：大人
                        Ysb.AppendLine("   and tbl.ChildNum = '0' ");
                    }
                    else if (model.TicketNumType == "2")
                    {
                        //検索条件に枚数種別：子供
                        Ysb.AppendLine("   and tbl.AdultNum = '0' ");
                    }
                    Ysb.AppendLine("  ) as MA");

                    // 検索条件
                    Ysb.AppendLine("WHERE 1 = 1");
                    cmd.CommandText = Ysb.ToString();

                    cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = model.TargetDateBegin;
                    cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = model.TargetDateEnd + " 23:59:59";
                    cmd.Parameters.Add("@lang", SqlDbType.NVarChar).Value = model.Language;
                    return dbInterface.ExecuteReader(cmd);
                }
            }

            ///<summary>
            ///横浜決済情報取得内容
            ///<summary>
            private string GetALLGetYokohamaPaymentDateQuery()
            {
                using (SqlDbInterface YokohamadbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("select ROW_NUMBER() OVER(ORDER BY tbl.PaymentId, tbl.PaymentType) as RecNo");    //決済IDと決済種別でソートする
                    sb.AppendLine("     , tbl.UserId");
                    sb.AppendLine("     , tbl.TranDate");
                    sb.AppendLine("     , tbl.BizCompanyCd" );                                /*チケット種別(交通手段)*/
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
                    sb.AppendLine("  from (");
                    sb.AppendLine("        	select pm.TranDate");
                    sb.AppendLine("        	,ftm.UserId");
                    sb.AppendLine("        	,fsm.TrsType");
                    sb.AppendLine("        	, N'売上' as Summary");
                    sb.AppendLine("        	,fsm.TicketType");
                    sb.AppendLine("        	,cr.Value");                        /*チケット名称(日本語)*/
                    sb.AppendLine("        	, ftm.AdultNum ");                  /*大人枚数*/
                    sb.AppendLine("        	, ftm.ChildNum ");                  /*子供枚数*/
                    sb.AppendLine("        	, pm.PaymentId");
                    sb.AppendLine("        	, pm.PaymentType");
                    sb.AppendLine("        	, pm.Amount");
                    sb.AppendLine("        	, pm.ReceiptNo");
                    sb.AppendLine("        	,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケット種別(au,au以外)*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("     ,case when uio.AplType =1 then 'au' ");   /*アプリタイプ*/
                    sb.AppendLine("     else ''");
                    sb.AppendLine("     end as AplName");
                    sb.AppendLine("        	from FreeTicketManage ftm");
                    sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("        	and (fsm.BizCompanyCd='TBCY')");      /*横浜*/
                    sb.AppendLine("        	inner join PaymentManage pm");
                    sb.AppendLine("        	on ftm.UserId = pm.UserId");
                    sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("        	and(pm.ServiceId = '8')");/*サービスID(横浜:8)*/
                    sb.AppendLine("        	and pm.PaymentType = '3'");
                    sb.AppendLine("        	and pm.GmoStatus = '1'");
                    sb.AppendLine("        	and pm.GmoProcType = '2'");
                    sb.AppendLine("        	left join CharacterResource cr");
                    sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("        	and Language =@lang");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on ftm.UserId=uio.UserId");
                    sb.AppendLine("        	union all");
                    /*払戻し返金データ取得*/
                    sb.AppendLine("        	select pm.TranDate");
                    sb.AppendLine("        	,ftm.UserId");
                    sb.AppendLine("        	,fsm.TrsType");
                    sb.AppendLine("        	, N'売上' as Summary");
                    sb.AppendLine("        	,fsm.TicketType");
                    sb.AppendLine("        	,cr.Value");                                /*チケット名称(日本語)*/
                    sb.AppendLine("        	, ftm.AdultNum ");                          /*大人枚数*/
                    sb.AppendLine("        	, ftm.ChildNum ");                          /*子供枚数*/
                    sb.AppendLine("        	, pm.PaymentId");
                    sb.AppendLine("        	, pm.PaymentType");
                    sb.AppendLine("        	, pm.Amount* -1 as Amount");
                    sb.AppendLine("        	, pm.ReceiptNo");
                    sb.AppendLine("        	,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("     ,case when uio.AplType =1 then 'au' ");   /*アプリタイプ*/
                    sb.AppendLine("     else ''");
                    sb.AppendLine("     end as AplName");
                    sb.AppendLine("        	from FreeTicketManage ftm");
                    sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("        	and (fsm.BizCompanyCd='TBCY')");　　　　　/*横浜*/
                    sb.AppendLine("        	inner join PaymentManage pm");
                    sb.AppendLine("        	on ftm.UserId = pm.UserId");
                    sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("        	and(pm.ServiceId = '8')");/*サービスID(横浜:)*/
                    sb.AppendLine("        	and pm.PaymentType = '5'");
                    sb.AppendLine("        	and pm.GmoStatus = '1'");
                    sb.AppendLine("        	and pm.GmoProcType = '3'");
                    sb.AppendLine("        	left join CharacterResource cr");
                    sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("        	and Language =@lang");
                    sb.AppendLine("          left join UserInfoOid uio");
                    sb.AppendLine("          on ftm.UserId=uio.UserId");
                    sb.AppendLine("        	union all");
                    /*払戻し手数料取得*/
                    sb.AppendLine("        	select pm.TranDate");
                    sb.AppendLine("        	,ftm.UserId");
                    sb.AppendLine("        	,fsm.TrsType");
                    sb.AppendLine("        	, N'払戻し' as Summary");
                    sb.AppendLine("        	,fsm.TicketType");
                    sb.AppendLine("        	,cr.Value");                        /*チケット名称*/
                    sb.AppendLine("        	, ftm.AdultNum ");                  /*大人枚数*/
                    sb.AppendLine("        	, ftm.ChildNum ");                  /*子供枚数*/
                    sb.AppendLine("        	, pm.PaymentId");
                    sb.AppendLine("        	, pm.PaymentType");
                    sb.AppendLine("        	, pm.Amount");
                    sb.AppendLine("        	, pm.ReceiptNo");
                    sb.AppendLine("        	,fsm.BizCompanyCd");
                    sb.AppendLine("         ,fsm.TicketId");                      /*チケットID*/
                    sb.AppendLine("         ,fsm.TicketGroup");
                    sb.AppendLine("     ,case when uio.AplType =1 then 'au' ");   /*アプリタイプ*/
                    sb.AppendLine("     else ''");
                    sb.AppendLine("     end as AplName");
                    sb.AppendLine("        	from FreeTicketManage ftm");
                    sb.AppendLine("        	left join FreeTicketSalesMaster fsm");
                    sb.AppendLine("        	on ftm.TicketId = fsm.TicketId");
                    sb.AppendLine("        	and (fsm.BizCompanyCd='TBCY')");　　　　　/*横浜*/
                    sb.AppendLine("        	inner join PaymentManage pm");
                    sb.AppendLine("        	on ftm.UserId = pm.UserId");
                    sb.AppendLine("        	and ftm.PaymentId = pm.PaymentId");
                    sb.AppendLine("        	and(pm.ServiceId = '8')");/*サービスID(横浜:8)*/
                    sb.AppendLine("        	and pm.PaymentType = '4'");
                    sb.AppendLine("        	and pm.GmoStatus = '1'");
                    sb.AppendLine("        	and pm.GmoProcType = '2'");
                    sb.AppendLine("        	left join CharacterResource cr");
                    sb.AppendLine("        	on fsm.TicketName = cr.ResourceId");
                    sb.AppendLine("        	and Language =@lang");
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
                    sb.AppendLine("        	and(pe.ServiceId = '8')");/*サービスID(横浜:8)*/
                    sb.AppendLine("           and pe.IsTreat = 0");         // 運用未処置
                    sb.AppendLine("     )");
                    sb.AppendLine("   and tbl.TranDate between @StartDatatTime and @EndDatatTime ");
                    return sb.ToString();
                }
            }



            /// <summary>
            /// 横浜券種ドロップダウンリスト項目取得
            /// </summary>
            /// <returns></returns>
            public DataTable GetTicketName(string lang)
            {
                List<YokohamaPaymentInfo> result = new List<YokohamaPaymentInfo>();
                using (SqlDbInterface NTdbInterface = new SqlDbInterface())
                using (SqlCommand cmd = new SqlCommand())
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
                    sb.AppendLine("   and Language = @lang");
                    sb.AppendLine("   where BizCompanyCd = @Bizid");

                    cmd.Parameters.Add("@lang", SqlDbType.NVarChar).Value = lang;
                    cmd.Parameters.Add("@Bizid", SqlDbType.NVarChar).Value = "TBCY";/*横浜のBizCompanyCD*/
                    cmd.CommandText = sb.ToString();

                    return NTdbInterface.ExecuteReader(cmd);

                }

            }
        }
    }
}