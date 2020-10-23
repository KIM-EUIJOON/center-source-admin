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
                    sb.AppendLine("   where ftpd.BizCompanyCd='MYF'");//宮交観光チケット画面の場合
                }
                sb.AppendLine(string.Format("and ftpd.ID NOT IN ({0})", AbolishedFacilityTickets));

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
                    // IDの末尾が"a"のものはau用
                    if (info.TicketID.EndsWith("a"))
                    {
                        info.TicketName = info.TicketName + "[au]";
                    }

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
                

                //呼び出し画面から事業者を指定する
                if (PageName =="MPA0801")
                {
                    string MuseumPaymentInfo_FOC = GetALLFOCMuseumPaymentDateQuery(stDate, edDate);
                    Jsb.AppendLine("select * from (" + MuseumPaymentInfo_FOC.ToString() + "");
                    Jsb.AppendLine("   and tbl.BizCompanyCd = 'FOC'");
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        Jsb.AppendLine("   and tbl.DiscountNum = '0' ");
                    }
                    else if (TicketNumType == "学割")
                    {
                        //検索条件に枚数種別：学割
                        Jsb.AppendLine("   and tbl.AdultNum = '0' ");
                    }
                }
                else if (PageName == "MPA1101")
                {
                    string MuseumPaymentInfo = GetALLMuseumPaymentDateQuery(stDate, edDate);
                    Jsb.AppendLine("select * from (" + MuseumPaymentInfo.ToString() + "");
                    Jsb.AppendLine("   and tbl.BizCompanyCd = 'MYF'");
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        Jsb.AppendLine("   and tbl.ChildNum = '0' ");
                        Jsb.AppendLine("   and tbl.DiscountNum = '0' ");
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
                    //cmd.Parameters.Add("@GmoStatus", SqlDbType.NVarChar).Value = PaymentType;
                }
                else if (PaymentType != "-")
                {
                    //検索条件に決済種別指定
                    Jsb.AppendLine("   and tbl.GmoStatus = @GmoStatus ");
                    cmd.Parameters.Add("@GmoStatus", SqlDbType.NVarChar).Value = PaymentType;
                }
                

                if (TicketId != "-")
                {
                    //検索条件に券種指定
                    Jsb.AppendLine("   and tbl.ID = @TicketId ");
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

                    if (infoN.TicketID.EndsWith("a"))
                    {
                        infoN.TicketName = infoN.TicketName + "[au]";
                    }
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
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountPrice+Refund.sumRefundPrice+Child.sumChildPrice)as Amount");
                sb.AppendLine("         ,ftdd.Denomination");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else '-'");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         and ftdd.DistributedNo='1'");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("         and Language = 'ja'");
                sb.AppendLine("    left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult ");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumDiscountPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("        (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("        ISNULL(sum(Price),0) as sumRefundPrice");
                sb.AppendLine("        from FTicketUsersPrices");
                sb.AppendLine("        where PriceNo='-1'");
                sb.AppendLine("        group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("        )Refund on Adult.UserId=Refund.UserId");
                sb.AppendLine("        and Adult.PurchasedNo =Refund.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumChildPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='3'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Child on Adult.UserId=Child.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Child.PurchasedNo)");
                sb.AppendLine("    on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("    ");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("         where ftup.GmoStatus = '1'");
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
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountPrice+Refund.sumRefundPrice+Child.sumChildPrice) * - 1 as Amount");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                sb.AppendLine("         ,ftdd.Denomination");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else '-'");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         and ftdd.DistributedNo='1'");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("          and Language = 'ja'");
                sb.AppendLine("        left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumDiscountPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("        (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("        ISNULL(sum(Price),0) as sumRefundPrice");
                sb.AppendLine("        from FTicketUsersPrices");
                sb.AppendLine("        where PriceNo='-1'");
                sb.AppendLine("        group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("        )Refund on Adult.UserId=Refund.UserId");
                sb.AppendLine("        and Adult.PurchasedNo =Refund.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumChildPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='3'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Child on Adult.UserId=Child.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Child.PurchasedNo)");
                sb.AppendLine("    on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("    ");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("         where ftup.GmoStatus = '3'");
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
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountRefundFee+sumRefundRefundFee+Child.sumChildPrice) as Amount");
                sb.AppendLine("         ,ftdd.Denomination");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else '-'");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         and ftdd.DistributedNo='1'");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("          and Language = 'ja'");
                sb.AppendLine("        left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(RefundFee),0) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(RefundFee),0) as sumDiscountRefundFee");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("        (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("        ISNULL(sum(RefundFee),0) as sumRefundRefundFee");
                sb.AppendLine("        from FTicketUsersPrices");
                sb.AppendLine("        where PriceNo='-1'");
                sb.AppendLine("        group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("        )Refund on Adult.UserId=Refund.UserId");
                sb.AppendLine("        and Adult.PurchasedNo =Refund.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(RefundFee),0) as sumChildPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='3'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Child on Adult.UserId=Child.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Child.PurchasedNo)");
                sb.AppendLine("    on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("      ");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("    where ftup.GmoStatus = '5'");
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
        /// 施設取得情報一覧
        /// </summary>
        /// <param name="stDate"></param>
        /// <param name="edDate"></param>
        /// <returns></returns>
        private string GetALLFOCMuseumPaymentDateQuery(DateTime stDate, DateTime edDate)
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
                /*枚数*/
                sb.AppendLine("         ,ftup.PaymentId");
                sb.AppendLine("         ,ftup.GmoStatus");
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountPrice+Refund.sumRefundPrice)as Amount");
                sb.AppendLine("         ,ftdd.Denomination");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else '-'");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         and ftdd.DistributedNo='1'");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("         and Language = 'ja'");
                sb.AppendLine("    left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult ");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumDiscountPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("        (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("        ISNULL(sum(Price),0) as sumRefundPrice");
                sb.AppendLine("        from FTicketUsersPrices");
                sb.AppendLine("        where PriceNo='-1'");
                sb.AppendLine("        group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("        )Refund on Adult.UserId=Refund.UserId");
                sb.AppendLine("        and Adult.PurchasedNo =Refund.PurchasedNo");
                sb.AppendLine("   ) on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("    ");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("         where ftup.GmoStatus = '1'");
                sb.AppendLine("     union all");
                sb.AppendLine("     select ftup.PurchaseDatetime");
                sb.AppendLine("         ,ftup.UserId");
                sb.AppendLine("         ,ftpd.BizCompanyCd");
                sb.AppendLine("         ,N'売上' as Summary");
                sb.AppendLine("         ,cr.Value");
                /*チケット名称(日本語)*/
                sb.AppendLine("         ,Adult.Num as AdultNum");
                sb.AppendLine("     ,Discount.Num as DiscountNum");
                sb.AppendLine("         /*枚数*/");
                sb.AppendLine("         ,ftup.PaymentId");
                sb.AppendLine("         ,ftup.GmoStatus");
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountPrice+Refund.sumRefundPrice) * - 1 as Amount");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                sb.AppendLine("         ,ftdd.Denomination");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else '-'");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         and ftdd.DistributedNo='1'");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("          and Language = 'ja'");
                sb.AppendLine("        left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(Price),0) as sumDiscountPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("        (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("        ISNULL(sum(Price),0) as sumRefundPrice");
                sb.AppendLine("        from FTicketUsersPrices");
                sb.AppendLine("        where PriceNo='-1'");
                sb.AppendLine("        group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("        )Refund on Adult.UserId=Refund.UserId");
                sb.AppendLine("        and Adult.PurchasedNo =Refund.PurchasedNo");
                sb.AppendLine("    )");
                sb.AppendLine("    on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("    ");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("         where ftup.GmoStatus = '3'");
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
                
                sb.AppendLine("                    /*枚数*/");
                sb.AppendLine("         ,ftup.PaymentId");
                sb.AppendLine("         ,ftup.GmoStatus");
                sb.AppendLine("         ,(Adult.sumAdultPrice+Discount.sumDiscountRefundFee+sumRefundRefundFee) as Amount");
                sb.AppendLine("         ,ftdd.Denomination");
                sb.AppendLine("         /*, pm.ReceiptNo*/");
                /*ReceiptTypeは存在しているが、領収書番号は不明*/
                sb.AppendLine("         ,ftup.ID");
                /*チケット種別(au,au以外)*/
                sb.AppendLine("         ,case when uio.AplType = 1 then 'au'");
                sb.AppendLine("         else '-'");
                sb.AppendLine("         end as AplName");
                sb.AppendLine("         from FTicketUsersPurchased ftup");
                sb.AppendLine("         left join FTicketDistributedDefinition ftdd");
                sb.AppendLine("         on ftdd.ID = ftup.ID");
                sb.AppendLine("         and ftdd.DistributedNo='1'");
                sb.AppendLine("         left join FTicketPublishInformation ftpi");
                sb.AppendLine("         on ftpi.ID = ftup.ID");
                sb.AppendLine("         left join CharacterResource cr");
                sb.AppendLine("         on ftpi.Name = cr.ResourceId");
                sb.AppendLine("          and Language = 'ja'");
                sb.AppendLine("        left join(");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(RefundFee),0) as sumAdultPrice");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='1'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Adult");
                sb.AppendLine("    left join");
                sb.AppendLine("    (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("    ISNULL(sum(RefundFee),0) as sumDiscountRefundFee");
                sb.AppendLine("    from FTicketUsersPrices");
                sb.AppendLine("    where PriceNo='2'");
                sb.AppendLine("    group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("    )Discount on Adult.UserId=Discount.UserId");
                sb.AppendLine("    and Adult.PurchasedNo =Discount.PurchasedNo");
                sb.AppendLine("    left join");
                sb.AppendLine("        (select UserId,ID,Num,PriceNo,PurchasedNo,");
                sb.AppendLine("        ISNULL(sum(RefundFee),0) as sumRefundRefundFee");
                sb.AppendLine("        from FTicketUsersPrices");
                sb.AppendLine("        where PriceNo='-1'");
                sb.AppendLine("        group by PriceNo,UserId,ID,Num,PurchasedNo");
                sb.AppendLine("        )Refund on Adult.UserId=Refund.UserId");
                sb.AppendLine("        and Adult.PurchasedNo =Refund.PurchasedNo");
                sb.AppendLine("    )");
                sb.AppendLine("    on ftup.PurchasedNo= Adult.PurchasedNo");
                sb.AppendLine("    and Adult.UserId=ftup.UserId");
                sb.AppendLine("      ");
                sb.AppendLine("         left join UserInfoOid uio");
                sb.AppendLine("         on ftup.UserId = uio.UserId");
                sb.AppendLine("         left join FTicketPublishDefinition ftpd");
                sb.AppendLine("         on ftup.ID = ftpd.ID");
                sb.AppendLine("    where ftup.GmoStatus = '5'");
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

                if (PageName == "MPA0801")
                {
                    string Miyakohinfo = GetALLFOCMuseumPaymentDateQuery(stDate, edDate);
                    MuseumSb.AppendLine(Miyakohinfo.ToString());
                    MuseumSb.AppendLine("   and tbl.BizCompanyCd = @BizCd");
                    cmd.Parameters.Add("@BizCd", SqlDbType.NVarChar).Value = "FOC";
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        MuseumSb.AppendLine("   and tbl.DiscountNum = '0' ");
                    }
                    else if (TicketNumType == "学割")
                    {
                        //検索条件に枚数種別：学割
                        MuseumSb.AppendLine("   and tbl.AdultNum = '0' ");
                    }
                }
                else if (PageName == "MPA1101")
                {
                    string Miyakohinfo = GetALLMuseumPaymentDateQuery(stDate, edDate);
                    MuseumSb.AppendLine(Miyakohinfo.ToString());
                    MuseumSb.AppendLine("   and tbl.BizCompanyCd = @BizCd");
                    cmd.Parameters.Add("@BizCd", SqlDbType.NVarChar).Value = "MYF";
                    if (TicketNumType == "大人")
                    {
                        //検索条件に枚数種別：大人
                        MuseumSb.AppendLine("   and tbl.ChildNum = '0' ");
                        MuseumSb.AppendLine("   and tbl.DiscountNum = '0' ");
                    }
                    else if (TicketNumType == "中学生")
                    {
                        //検索条件に枚数種別：学割
                        MuseumSb.AppendLine("   and tbl.AdultNum = '0' ");
                        MuseumSb.AppendLine("   and tbl.ChildNum = '0' ");
                    }
                    else if (TicketNumType == "小児")
                    {
                        //検索条件に枚数種別：学割
                        MuseumSb.AppendLine("   and tbl.AdultNum = '0' ");
                        MuseumSb.AppendLine("   and tbl.DiscountNum = '0' ");
                    }
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
                    MuseumSb.AppendLine("   and tbl.GmoStatus not in ('1','3','5')");
                  //  cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                }
                else if (PaymentType != "-")
                {
                    //検索条件に決済種別指定
                    MuseumSb.AppendLine("   and tbl.GmoStatus = @PaymentType ");
                    cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar).Value = PaymentType;
                }
                
                if (TicketId != "-")
                {
                    //検索条件に券種指定
                    MuseumSb.AppendLine("   and tbl.ID = @TicketId ");
                    cmd.Parameters.Add("@TicketId", SqlDbType.NVarChar).Value = TicketId;
                }
                if (AplType != "-")//au用Role番号判定
                {
                    MuseumSb.AppendLine("   and tbl.AplName = @AplName");
                    cmd.Parameters.Add("@AplName", SqlDbType.NVarChar).Value = "au";
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
                if (false == string.IsNullOrEmpty(model.FacilityId))
                {
                    //施設名
                    Jsb.AppendLine("   and ftud.FacilityId = @FacilityId  ");
                    cmd.Parameters.Add("@FacilityId", SqlDbType.NVarChar).Value = model.FacilityId;
                }
                if (false == string.IsNullOrEmpty(model.ShopType))
                {
                    string SearchOb = "/"; //「/」判定用
                    int num = model.ShopType.IndexOf(SearchOb);
                    string ShopId = model.ShopType.Substring(0, num);       //チケットID分離
                    int Tpt = model.ShopType.Length - (num + 1);
                    string TransePortCheck = model.ShopType.Substring(num + 1, Tpt).ToString();  //交通種別分離
                    //入場イベント名
                    Jsb.AppendLine("   and ftud.ServiceResourceId = @TenantID ");
                    cmd.Parameters.Add("@TenantID", SqlDbType.NVarChar).Value = TransePortCheck;
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
                sb.AppendLine("select ftup.UserId");             /*myrouteID*/
                sb.AppendLine("    , ftup.PurchasedNo");                      /*購入番号*/
                sb.AppendLine("    , ftup.ID");                               /*入場券ID*/
                sb.AppendLine("    , ftud.UsageStartDatetime");               /*利用開始時間*/
                sb.AppendLine("    , ftud.FacilityId");                       /*施設ID*/
                sb.AppendLine("    , ftud.ServiceResourceId");                /*サービスリソースID*/
                sb.AppendLine("    , ftsrdName.Value as ServiceName");        /*サービス名*/
                sb.AppendLine("    , ftfdName.Value as MuseumName");          /*施設名*/
                sb.AppendLine("    , ftdd.Denomination");                     /*業種(クーポンか入場券かはわかるが業種はないかも)*/
                sb.AppendLine("    ,case when uio.AplType =1 then 'au'");     /*アプリ種別*/
                sb.AppendLine("    else '-'");
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
                sb.AppendLine("    left join FTicketPublishDefinition ftpd");
                sb.AppendLine("    on ftpd.ID = ftup.ID");
                sb.AppendLine("    where (ftdd.BizCompanyCd = 'FOC' or (ftdd.BizCompanyCd is null and ftpd.BizCompanyCd ='FOC'))");
                sb.AppendLine("    and ftud.UsageEndDatetime IS NOT NULL");
                sb.AppendLine("       and ftud.UsageStartDatetime between @StartDatatTime and @EndDatatTime ");
                return sb.ToString();
            }
        }

        /// <summary>廃止チケット（福岡市博物館）</summary>
        private const string AbolishedFacilityTickets = "'FA02', 'FA02a'";

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
                sb.AppendLine(", ffd.FacilityId");
                sb.AppendLine(", ftdd.BizCompanyCd");
                sb.AppendLine(", ftpd.BizCompanyCd");
                sb.AppendLine("from FTicketFacilityDefinition ffd");
                sb.AppendLine("left join CharacterResource cr");
                sb.AppendLine("on cr.ResourceId = ffd.Name and cr.Language= @lng");
                sb.AppendLine("inner join FTicketDistributedAppliedService ftdas");
                sb.AppendLine("on ftdas.FacilityId = ffd.FacilityId");
                sb.AppendLine("inner join FTicketDistributedDefinition ftdd");
                sb.AppendLine("on ftdd.ID = ftdas.ID");
                sb.AppendLine("inner join FTicketPublishDefinition ftpd");
                sb.AppendLine("on ftpd.ID = ftdas.ID");
                sb.AppendLine("where (ftdd.BizCompanyCd =@biz");
                sb.AppendLine("or (ftdd.BizCompanyCd IS NULL and ftpd.BizCompanyCd = @biz))");
                sb.AppendLine(string.Format("and ftdas.ID NOT IN ({0})", AbolishedFacilityTickets));
                sb.AppendLine("order by ffd.FacilityId");

                cmd.Parameters.Add("@lng", SqlDbType.NVarChar).Value = language;
                cmd.Parameters.Add("@biz", SqlDbType.NVarChar).Value = "FOC";
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
                sb.AppendLine(", ftsrd.FacilityId");
                sb.AppendLine(", ftdd.BizCompanyCd");
                sb.AppendLine(", ftsrd.ServiceResourceId");
                sb.AppendLine("from FTicketServiceResouceDefinition ftsrd");
                sb.AppendLine("left join CharacterResource cr");
                sb.AppendLine("on cr.ResourceId = ftsrd.Name and cr.Language = @lng");
                sb.AppendLine("inner join FTicketDistributedAppliedService ftdas");
                sb.AppendLine("on ftdas.FacilityId = ftsrd.FacilityId");
                sb.AppendLine("inner join FTicketDistributedDefinition ftdd");
                sb.AppendLine("on ftdd.ID = ftdas.ID");
                sb.AppendLine("inner join FTicketPublishDefinition ftpd");
                sb.AppendLine("on ftpd.ID = ftdas.ID");
                sb.AppendLine("Where (ftdd.BizCompanyCd = @biz");
                sb.AppendLine("or (ftdd.BizCompanyCd IS NULL and ftpd.BizCompanyCd = @biz))");
                sb.AppendLine(string.Format("and ftdas.ID NOT IN ({0})", AbolishedFacilityTickets));
                sb.AppendLine("order by ftsrd.FacilityId, ftsrd.ServiceResourceId");

                cmd.Parameters.Add("@lng", SqlDbType.NVarChar).Value = language;
                cmd.Parameters.Add("@biz", SqlDbType.NVarChar).Value = "FOC";   /*福岡施設固定*/
                cmd.CommandText = sb.ToString();

                return NTdbInterface.ExecuteReader(cmd);
            }
        }
    }
}