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
    public class MuseumInfoModels
    {
        /// <summary>
        /// 福岡施設販売チケット一覧取得
        /// </summary>
        /// <returns></returns>
        public List<MuseumPaymentInfo> MuseumPassportList(string UserId, string PageName)
        {
            List<MuseumPaymentInfo> result = new List<MuseumPaymentInfo>();
            using (SqlDbInterface NTdbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("select distinct ftpi.ID");
                sb.AppendLine("   ,cr.Value");
                sb.AppendLine("   ,ftpd.BizCompanyCd");
                sb.AppendLine("   ,ftdd.Denomination");
                sb.AppendLine("   from FTicketPublishInformation ftpi");
                sb.AppendLine("   left join CharacterResource cr");
                sb.AppendLine("   on ftpi.Name = cr.ResourceId");
                sb.AppendLine("   and Language = 'ja'");
                sb.AppendLine("   left join FTicketPublishDefinition ftpd");
                sb.AppendLine("   on ftpd.ID=ftpi.ID");
                sb.AppendLine("   left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("   on ftpi.ID=ftdd.ID");

                if (PageName == "MPA0801")
                {
                    sb.AppendLine("   where ftpd.BizCompanyCd='FOC'");//福岡施設画面の場合
                }
                else if(PageName == "MPA1101")
                {
                    sb.AppendLine("   where ftpd.BizCompanyCd='MYZ'");//宮交観光チケット画面の場合
                }
                

                cmd.CommandText = sb.ToString();

                DataTable dt = NTdbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows)
                {
                    MuseumPaymentInfo info = new MuseumPaymentInfo
                    {
                        TicketID = row["ID"].ToString(),
                        TicketName = row["Value"].ToString(),
                        BizCompanyCd = row["BizCompanyCd"].ToString(),
                        Denomination = row["Denomination"].ToString(),
                    };
                    /* au限定商品がないため判定削除
                    if (row["TicketGroup"].ToString() == "1")
                    {
                        info.TicketName = info.TicketName + "[au]";
                    }*/

                    result.Add(info);
                }
                return result;
            }
        }

        /// <summary>
        /// 福岡施設の決済情報リスト取得
        /// </summary>
        /// <param name="stDate">抽出範囲開始日</param>
        /// <param name="edDate">抽出範囲終了日</param>
        /// <returns>福岡施設決済情報</returns>
        public List<MuseumPaymentInfo> GetMuseumPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string MyrouteNo, string PaymentType, string TicketNumType, string Denomination, string TicketId, string AplType, string PageName)
        {
            List<MuseumPaymentInfo> result = new List<MuseumPaymentInfo>();

            using (SqlDbInterface MuseumdbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder Jsb = new StringBuilder();
                string MuseumPaymentInfo = GetALLMuseumPaymentDateQuery(stDate, edDate);
                Jsb.AppendLine("select * from (" + MuseumPaymentInfo.ToString() + "");

                //呼び出し画面から事業者を指定する
                if (PageName =="MPA0801")
                {
                    Jsb.AppendLine("   and tbl.BizCompanyCd = 'FOC'");
                    //cmd.Parameters.Add("@PageName", SqlDbType.NVarChar).Value = "FOC";
                }
                else if (PageName == "MPA1101")
                {
                    Jsb.AppendLine("   and tbl.BizCompanyCd = 'MYZ'");
                    //cmd.Parameters.Add("@PageName", SqlDbType.NVarChar).Value = "MYZ";
                }
                if (MyrouteNo != "")
                {
                    //検索条件にMyrouteID指定
                    Jsb.AppendLine("   and tbl.UserId = @UserId");
                    cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                }
                if (Denomination != "-")
                {
                    //検索条件に券種指定
                    Jsb.AppendLine("   and tbl.Denomination = @Denomination ");
                    cmd.Parameters.Add("@Denomination", SqlDbType.NVarChar).Value = Denomination;
                }
                if (PaymentType == "決済種別不明")
                {
                    //検索条件に決済種別：決済種別不明指定
                    Jsb.AppendLine("   and tbl.GmoStatus not in ('1','3','5')");
                    cmd.Parameters.Add("@GmoStatus", SqlDbType.NVarChar).Value = PaymentType;
                }
                else if (PaymentType != "-")
                {
                    //検索条件に決済種別指定
                    Jsb.AppendLine("   and tbl.GmoStatus = @GmoStatus ");
                    cmd.Parameters.Add("@GmoStatus", SqlDbType.NVarChar).Value = PaymentType;
                }
                if (TicketNumType == "大人")
                {
                    //検索条件に枚数種別：大人
                    Jsb.AppendLine("   and tbl.ChildNum = '0' ");
                    Jsb.AppendLine("   and tbl.DiscountNum = '0' ");
                }
                else if (TicketNumType == "学割")
                {
                    //検索条件に枚数種別：学割
                    Jsb.AppendLine("   and tbl.AdultNum = '0' ");
                    Jsb.AppendLine("   and tbl.ChildNum = '0' ");
                }
                else if (TicketNumType == "中学生")
                {
                    //検索条件に枚数種別：学割
                    Jsb.AppendLine("   and tbl.AdultNum = '0' ");
                    Jsb.AppendLine("   and tbl.ChildNum = '0' ");
                }
                else if (TicketNumType == "小児")
                {
                    //検索条件に枚数種別：学割
                    Jsb.AppendLine("   and tbl.AdultNum = '0' ");
                    Jsb.AppendLine("   and tbl.DiscountNum = '0' ");
                }

                if (TicketId != "-")
                {
                    //検索条件に券種指定
                    Jsb.AppendLine("   and tbl.ID = @TicketId ");
                    cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                }
                if (AplType != "-")//au用Role番号判定
                {
                    Jsb.AppendLine("   and tbl.AplType = @AplType");
                    cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = AplType;
                }

                Jsb.AppendLine("  ) as MA  where MA.RecNo between @PageNum and @ListEnd");

                cmd.CommandText = Jsb.ToString();

                cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");
                cmd.Parameters.Add("@PageNum", SqlDbType.NVarChar).Value = pageNo;
                cmd.Parameters.Add("@ListEnd", SqlDbType.NVarChar).Value = ListNoEnd;

                DataTable dt = MuseumdbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows)
                {
                    MuseumPaymentInfo infoN = new MuseumPaymentInfo
                    {
                        UserId = row["UserId"].ToString(),
                         TranDatetime = ((DateTime)row["PurchaseDatetime"]).ToString("yyyy/MM/dd HH:mm:ss"),
                         PaymentId = row["PaymentId"].ToString(),
                         TicketName = row["Value"].ToString(),
                         TicketID = row["ID"].ToString(),
                         Denomination = row["Denomination"].ToString(),
                         AdultNum = row["AdultNum"].ToString(),
                         ChildNum = row["DiscountNum"].ToString(),
                        Apltype = row["AplName"].ToString(),
                        PaymentType = row["GmoStatus"].ToString(),
                        Amount = row["Amount"].ToString()
                    };
                 
                    if (row["AplName"].ToString() == "1")
                    {
                        infoN.Apltype = "au";
                    }
                    else
                    {
                        infoN.Apltype = "-";
                    }
                    if (row["Denomination"].ToString()== "admission")
                    {
                        infoN.Denomination = "施設";
                    }
                    else if(row["Denomination"].ToString() == "coupon")
                    {
                        infoN.Denomination = "テナント";
                    }
                    if (PageName == "MPA1101")
                    {
                        infoN.infantNum = row["ChildNum"].ToString();
                    }
                    result.Add(infoN);
                }
                return result;
            }
        }

        /// <summary>
        /// 施設取得情報一覧
        /// </summary>
        /// <param name="stDate"></param>
        /// <param name="edDate"></param>
        /// <returns></returns>
        private string GetALLMuseumPaymentDateQuery(DateTime stDate, DateTime edDate)
        {
            using (SqlDbInterface MiyakohdbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("    select ROW_NUMBER() OVER(ORDER BY tbl.PaymentId) as RecNo");
                /*決済IDと決済種別でソートする*/
                        sb.AppendLine("    ,tbl.UserId");
                sb.AppendLine("    ,tbl.PurchaseDatetime");
                sb.AppendLine("    ,tbl.BizCompanyCd");
                /*チケット種別(交通手段)*/
                sb.AppendLine("         ,tbl.ID");
                sb.AppendLine("         ,tbl.Value");
                /*チケット名称*/
                sb.AppendLine("         ,tbl.AdultNum");
                sb.AppendLine("     ,tbl.DiscountNum");
                sb.AppendLine("     ,tbl.ChildNum");
                sb.AppendLine("         ,tbl.PaymentId");
                sb.AppendLine("         ,tbl.Denomination");
                sb.AppendLine("         ,tbl.AplName");
                /*アプリ種別*/
                sb.AppendLine("         ,case when tbl.GmoStatus = '1' then N'即時決済'");
                sb.AppendLine("         when tbl.GmoStatus = '5' then N'払戻し'");
                sb.AppendLine("         when tbl.GmoStatus = '3' then N'取消'");
                sb.AppendLine("         else N'決済種別不明'");
                sb.AppendLine("         end as GmoStatus");
                sb.AppendLine("         ,tbl.Amount");
                sb.AppendLine("    /*, tbl.ReceiptNo*/");
                sb.AppendLine("         from (");
                sb.AppendLine("     select ftup.PurchaseDatetime");
                sb.AppendLine("         ,ftup.UserId");
                
                sb.AppendLine("         ,ftpd.BizCompanyCd");
                sb.AppendLine("         ,N'売上' as Summary");
                sb.AppendLine("         ,cr.Value");
                /*チケット名称(日本語)*/
                sb.AppendLine("         ,Adult.Num as AdultNum");
                sb.AppendLine("     ,Discount.Num as DiscountNum");
                sb.AppendLine("     ,Child.Num as ChildNum");
                /*枚数*/
                sb.AppendLine("         ,ftup.PaymentId");
                sb.AppendLine("         ,ftup.GmoStatus");
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountPrice+Child.sumChildPrice)as Amount");
                sb.AppendLine("         ,ftdd.Denomination");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else ''");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("         and Language = 'ja'");
                sb.AppendLine("    left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(Price) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult ");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(Price) as sumDiscountPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(Price) as sumChildPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='3'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Child on Adult.UserId=Child.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Child.PurchasedNo)");
                sb.AppendLine("    on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("    ");
                sb.AppendLine("         and ftup.GmoStatus = '1'");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("     union all");
                sb.AppendLine("     select ftup.PurchaseDatetime");
                sb.AppendLine("         ,ftup.UserId");
                sb.AppendLine("         ,ftpd.BizCompanyCd");
                sb.AppendLine("         ,N'売上' as Summary");
                sb.AppendLine("         ,cr.Value");
                /*チケット名称(日本語)*/
                sb.AppendLine("         ,Adult.Num as AdultNum");
                sb.AppendLine("     ,Discount.Num as DiscountNum");
                sb.AppendLine("     ,Child.Num as ChildNum");
                sb.AppendLine("         /*枚数*/");
                sb.AppendLine("         ,ftup.PaymentId");
                sb.AppendLine("         ,ftup.GmoStatus");
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountPrice+Child.sumChildPrice) * - 1 as Amount");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                sb.AppendLine("         ,ftdd.Denomination");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else ''");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("          and Language = 'ja'");
                sb.AppendLine("        left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(Price) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(Price) as sumDiscountPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(Price) as sumChildPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='3'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Child on Adult.UserId=Child.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Child.PurchasedNo)");
                sb.AppendLine("    on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("    ");
                sb.AppendLine("         and ftup.GmoStatus = '3'");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("     union all");
                sb.AppendLine("     select ftup.PurchaseDatetime");
                sb.AppendLine("         ,ftup.UserId");
                sb.AppendLine("         ,ftpd.BizCompanyCd");
                sb.AppendLine("         ,N'払戻し' as Summary");
                sb.AppendLine("         ,cr.Value");
                /*チケット名称(日本語)*/
                sb.AppendLine("        /* ,ftupr.PriceNo*/");
                
                /*チケット種別判定条件(1:大人,2:学生)*/
                sb.AppendLine("         ,Adult.Num as AdultNum");
                sb.AppendLine("     ,Discount.Num as DiscountNum");
                sb.AppendLine("     ,Child.Num as ChildNum");
                sb.AppendLine("                    /*枚数*/");
                sb.AppendLine("         ,ftup.PaymentId");
                sb.AppendLine("         ,ftup.GmoStatus");
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountRefundFee+Child.sumChildPrice) as Amount");
                sb.AppendLine("         ,ftdd.Denomination");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else ''");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("          and Language = 'ja'");
                sb.AppendLine("        left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(RefundFee) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(RefundFee) as sumDiscountRefundFee");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    sum(RefundFee) as sumChildPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='3'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Child on Adult.UserId=Child.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Child.PurchasedNo)");
                sb.AppendLine("    on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("      ");
                sb.AppendLine("    and ftup.GmoStatus = '5'");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("     ) tbl");
                /* 決済エラー分は含めない*/
                sb.AppendLine("         where not exists(");
                sb.AppendLine("         select 1");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         where ftup.IsPaymentError = 1");
                sb.AppendLine("     )");
                /*運用未処置*/
                sb.AppendLine("     and tbl.PurchaseDatetime between @StartDatatTime and @EndDatatTime");


                return sb.ToString();
            }
        }

        /// <summary>
        /// 施設表示用決済情報リスト総数取得
        /// </summary>
        /// <param name="stDate"></param>
        /// <param name="edDate"></param>
        /// <param name="MyrouteNo"></param>
        /// <param name="TicketType"></param>
        /// <param name="PaymentType"></param>
        /// <param name="TicketNumType"></param>
        /// <param name="TransportType"></param>
        /// <returns></returns>
        public List<MuseumPaymentInfo> MuseumPaymentDateListMaxCount(DateTime stDate, DateTime edDate, string MyrouteNo, string PaymentType, string TicketNumType, string TransportType, string TicketId, string AplType,string PageName)
        {
            List<MuseumPaymentInfo> result = new List<MuseumPaymentInfo>();
            //現在表示されているリストの通し番号

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder MuseumSb = new StringBuilder();

                string Miyakohinfo = GetALLMuseumPaymentDateQuery(stDate, edDate);
                MuseumSb.AppendLine(Miyakohinfo.ToString());

                if (PageName == "MPA0801")
                {
                    MuseumSb.AppendLine("   and tbl.BizCompanyCd = @PageName");
                    cmd.Parameters.Add("@PageName", SqlDbType.NVarChar).Value = "FOC";
                }
                else if (PageName == "MPA1101")
                {
                    MuseumSb.AppendLine("   and tbl.BizCompanyCd = @PageName");
                    cmd.Parameters.Add("@PageName", SqlDbType.NVarChar).Value = "MYZ";
                }
                if (MyrouteNo != "")
                {
                    //検索条件にMyrouteID指定
                    MuseumSb.AppendLine("   and tbl.UserId = @UserId");
                    cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = MyrouteNo;
                }
                if (TransportType != "-")
                {
                    //検索条件に券種指定
                    MuseumSb.AppendLine("   and tbl.Denomination = @TransportType ");
                    cmd.Parameters.Add("@TransportType", SqlDbType.NVarChar).Value = TransportType;
                }
                if (PaymentType == "決済種別不明")
                {
                    //検索条件に決済種別：決済種別不明指定
                    MuseumSb.AppendLine("   and tbl.PaymentType not in ('3','4','5')");
                    cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                }
                else if (PaymentType != "-")
                {
                    //検索条件に決済種別指定
                    MuseumSb.AppendLine("   and tbl.GmoStatus = @PaymentType ");
                    cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                }
                if (TicketNumType == "大人")
                {
                    //検索条件に枚数種別：大人
                    MuseumSb.AppendLine("   and tbl.PriceNo = '1' ");
                }
                else if (TicketNumType == "学割")
                {
                    //検索条件に枚数種別：学割
                    MuseumSb.AppendLine("   and tbl.PriceNo = '2' ");
                }
                else if (TicketNumType == "中学生")
                {
                    //検索条件に枚数種別：学割
                    MuseumSb.AppendLine("   and tbl.PriceNo = '2' ");
                }
                else if (TicketNumType == "小児")
                {
                    //検索条件に枚数種別：学割
                    MuseumSb.AppendLine("   and tbl.PriceNo = '3' ");
                }
                if (TicketId != "-")
                {
                    //検索条件に券種指定
                    MuseumSb.AppendLine("   and tbl.ID = @TicketId ");
                    cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                }
                if (AplType != "-")//au用Role番号判定
                {
                    MuseumSb.AppendLine("   and tbl.AplType = @AplType");
                    cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = AplType;
                }


                cmd.CommandText = MuseumSb.ToString();

                cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = stDate.ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = edDate.ToString("yyyy-MM-dd 23:59:59");

                DataTable dt = dbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows)
                {
                    MuseumPaymentInfo info = new MuseumPaymentInfo
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
        /// 施設利用情報取得
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public DataTable GetMuseumUsageDateList(MuseumUseInfo model)
        {
            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                // データ取得
                StringBuilder Jsb = new StringBuilder();
                string MuseumUsageInfo = GetALLMuseumUsageDateQuery();
                Jsb.AppendLine("select * from (" + MuseumUsageInfo.ToString() + "");


                if (false == string.IsNullOrEmpty(model.UserId))
                {
                    Jsb.AppendLine("    AND ftup.UserId = @UserId");
                    cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = model.UserId;
                }
                if (false == string.IsNullOrEmpty(model.Apltype))
                {
                    Jsb.AppendLine("    AND AplType = @AplName ");
                    cmd.Parameters.Add("@AplName", SqlDbType.NVarChar).Value = "au";
                }
                if (model.FacilityId != "-")
                {
                    //施設名
                    Jsb.AppendLine("   and ftud.FacilityId = @FacilityId  ");
                    cmd.Parameters.Add("@FacilityId", SqlDbType.NVarChar).Value = model.FacilityId;
                }
                if (model.TenantID != "-")
                {
                    //入場イベント名
                    Jsb.AppendLine("   and ftud.ServiceResourceId = @TenantID ");
                    cmd.Parameters.Add("@TenantID", SqlDbType.NVarChar).Value = model.TenantID;
                }

                Jsb.AppendLine("  ) as MA");

                // 検索条件
                Jsb.AppendLine("WHERE 1 = 1");
                cmd.CommandText = Jsb.ToString();

                cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = model.TargetDateBegin;
                cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = model.TargetDateEnd + " 23:59:59";
                cmd.Parameters.Add("@lang", SqlDbType.NVarChar).Value = model.Language;
                return dbInterface.ExecuteReader(cmd);
            }
        }


        ///<summary>
        ///施設利用情報取得内容
        ///<summary>
        private string GetALLMuseumUsageDateQuery()
        {
            using (SqlDbInterface MuseumUsagedbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select ftup.UserId/*myrouteID*/");             /*myrouteID*/
                sb.AppendLine("    , ftup.PurchasedNo");                      /*購入番号*/
                sb.AppendLine("    , ftup.ID");                               /*入場券ID*/
                sb.AppendLine("    , ftud.UsageStartDatetime");               /*利用開始時間*/
                sb.AppendLine("    , ftud.FacilityId");                       /*施設ID*/
                sb.AppendLine("    , ftud.ServiceResourceId");                /*サービスリソースID*/
                sb.AppendLine("    , ftsrdName.Value as ServiceName");        /*サービス名*/
                sb.AppendLine("    , ftfdName.Value as MuseumName");          /*施設名*/
                sb.AppendLine("    , ftdd.Denomination");                     /*業種(クーポンか入場券かはわかるが業種はないかも)*/
                sb.AppendLine("    ,case when uio.AplType =1 then 'au'");     /*アプリ種別*/
                sb.AppendLine("    else ''");
                sb.AppendLine("    end as AplName");
                sb.AppendLine("    , ftdd.BizCompanyCd");
                sb.AppendLine("    , ftud.UsageEndDatetime");
                sb.AppendLine("    from FTicketUsersDistributed ftud");
                sb.AppendLine("    left join FTicketUsersPurchased ftup");
                sb.AppendLine("    on ftup.UserId =ftud.UserId");
                sb.AppendLine("    and ftup.PurchasedNo=ftud.PurchasedNo");
                sb.AppendLine("    and ftup.ID=ftud.ID");
                sb.AppendLine("    left join FTicketServiceResouceDefinition ftsrd");
                sb.AppendLine("    on ftud.FacilityId =ftsrd.FacilityId");
                sb.AppendLine("    and ftud.ServiceResourceId =ftsrd.ServiceResourceId");
                sb.AppendLine("    left join FTicketFacilityDefinition ftfd");
                sb.AppendLine("    on ftud.FacilityId=ftfd.FacilityId");
                sb.AppendLine("    left join CharacterResource ftsrdName");
                sb.AppendLine("    on ftsrd.Name=ftsrdName.ResourceId");
                sb.AppendLine("    and ftsrdName.Language ='ja'");
                sb.AppendLine("    left join CharacterResource ftfdName");
                sb.AppendLine("    on ftfd.Name=ftfdName.ResourceId");
                sb.AppendLine("    and ftfdName.Language ='ja'");
                sb.AppendLine("    left join UserInfoOid uio");
                sb.AppendLine("    on ftup.UserId = uio.UserId");
                sb.AppendLine("    left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("    on ftud.ID =ftdd.ID");
                sb.AppendLine("    and ftud.DistributedNo = ftdd.DistributedNo");
                sb.AppendLine("    where ftdd.BizCompanyCd ='FOC'");
                sb.AppendLine("    and ftud.UsageEndDatetime IS NOT NULL");
                sb.AppendLine("       and ftud.UsageStartDatetime between @StartDatatTime and @EndDatatTime ");
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

                sb.AppendLine("select distinct cr.Value");
                sb.AppendLine("    , ffd.FacilityId");
                sb.AppendLine("    , ftdas.ID");
                sb.AppendLine("    , ftdd.BizCompanyCd");
                sb.AppendLine("    from FTicketFacilityDefinition ffd");
                sb.AppendLine("    left join CharacterResource cr");
                sb.AppendLine("    on cr.ResourceId = ffd.Name");
                sb.AppendLine("    left join FTicketDistributedAppliedService ftdas");
                sb.AppendLine("    on ftdas.FacilityId = ffd.FacilityId");
                sb.AppendLine("    left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("    on ftdd.ID = ftdas.ID");
                sb.AppendLine("    where cr.Language= @lng");
                sb.AppendLine("    and  ftdd.BizCompanyCd ='FOC'");


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

                sb.AppendLine("select distinct cr.Value");
                sb.AppendLine("                    , ftsrd.FacilityId");
                sb.AppendLine("                    , ftdas.ID");
                sb.AppendLine("                    , ftdd.BizCompanyCd");
                sb.AppendLine("                   , ftsrd.ServiceResourceId");
                sb.AppendLine("                    from FTicketServiceResouceDefinition ftsrd");
                sb.AppendLine("                    left join CharacterResource cr");
                sb.AppendLine("                    on cr.ResourceId = ftsrd.Name");
                sb.AppendLine("                    left join FTicketDistributedAppliedService ftdas");
                sb.AppendLine("                    on ftdas.FacilityId = ftsrd.FacilityId");
                sb.AppendLine("                    left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("                    on ftdd.ID = ftdas.ID");
                sb.AppendLine("                    Where cr.Language = @lng");
                sb.AppendLine("                    and  ftdd.BizCompanyCd ='FOC'/*福岡施設固定*/");

                cmd.Parameters.Add("@lng", SqlDbType.NVarChar).Value = language;
                cmd.CommandText = sb.ToString();

                return NTdbInterface.ExecuteReader(cmd);
            }
        }
    }
}