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
    /// クーポン運用画面　リクエストクラス
    /// </summary>
    public class CouponInfoModel
    {
        /// <summary>
        /// クーポン管理データ全取得
        /// </summary>
        /// <returns></returns>
        private string GetCouponManage()
        {
            //using (SqlDbInterface db = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("SELECT CoupTBL.*, ");
                sb.AppendLine("       facM.FacilityName, ");
                sb.AppendLine("       shpM.ShopName ");
                sb.AppendLine("FROM CouponManage CoupTBL ");
                sb.AppendLine("LEFT JOIN FacilityMaster facM ON CoupTBL.UsageFacilityId = facM.FacilityId ");
                sb.AppendLine("     JOIN ShopMaster shpM On CoupTBL.UsageShopCode = shpM.ShopCode ");
                return sb.ToString();
            }
        }


        /// <summary>
        /// 西鉄の決済情報リスト取得
        /// </summary>
        /// <param name="stDate">抽出範囲開始日</param>
        /// <param name="edDate">抽出範囲終了日</param>
        /// <returns>西鉄決済情報</returns>
        public List<NishitetsuPaymentInfo> GetNishitetsuPaymentDate(DateTime stDate, DateTime edDate, int pageNo, int ListNoEnd, string MyrouteNo, string PaymentType, string TicketNumType, string TransportType, string TicketId)
        {
            List<NishitetsuPaymentInfo> result = new List<NishitetsuPaymentInfo>();

            using (SqlDbInterface NishidbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder Nsb = new StringBuilder();
                string NishitetsuInfo = GetCouponManage();
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
                        ReceiptNo = row["ReceiptNo"].ToString()
                    };
                    result.Add(infoN);
                }
                return result;
            }
        }
        /// <summary>
        /// クーポン管理情報リスト取得
        /// </summary>
        /// <param name="model">検索情報</param>
        /// <returns>検索情報</returns>
        public DataTable GetCouponDateList(CouponInfoEntityList model)
        {
            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("SELECT Cp.*, ");
                sb.AppendLine("       facM.FacilityName, ");
                sb.AppendLine("       shpM.ShopName, ");
                sb.AppendLine("       uio.AplType, ");
                sb.AppendLine("       indM.IndustryName ");
                sb.AppendLine("FROM CouponManage Cp ");
                sb.AppendLine("LEFT JOIN FacilityMaster facM ON Cp.UsageFacilityId = facM.FacilityId ");
                sb.AppendLine("     JOIN ShopMaster shpM ON Cp.UsageShopCode = shpM.ShopCode ");
                sb.AppendLine("     JOIN UserInfoOid uio ON Cp.UserId = uio.UserId ");
                sb.AppendLine("     JOIN IndustryMaster indM ON shpM.IndustryCode = indM.IndustryCode ");

                // 検索条件
                sb.AppendLine("WHERE 1 = 1");
                if (false == string.IsNullOrEmpty(model.UserId))
                {
                    sb.AppendLine("   AND Cp.UserId = @UserId");
                    cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = model.UserId;
                }
                if (false == string.IsNullOrEmpty(model.AplType))
                {
                    sb.AppendLine("   AND uio.AplType = @AplType ");
                    cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = model.AplType;
                }
                if ("-"  != model.FacilityId)
                //if (false == string.IsNullOrEmpty(model.FacilityName))
                {
                    sb.AppendLine("   AND facM.FacilityId = @FacilityId ");
                    cmd.Parameters.Add("@FacilityId", SqlDbType.NVarChar).Value = model.FacilityId;
                }
                if ("-" != model.ShopCode)
                //if (false == string.IsNullOrEmpty(model.ShopName))
                {
                    sb.AppendLine("   AND shpM.ShopCode = @ShopCode ");
                    cmd.Parameters.Add("@ShopCode", SqlDbType.NVarChar).Value = model.ShopCode;
                }

                sb.AppendLine(" AND Cp.UsageDateTime BETWEEN @StartDatatTime AND @EndDatatTime");
                sb.AppendLine(" ORDER BY Cp.UsageDateTime");
                
                cmd.CommandText = sb.ToString();

                cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = model.TargetDateBegin;
                cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = model.TargetDateEnd + " 23:59:59";

                return dbInterface.ExecuteReader(cmd);
            }
        }

        /// <summary>
        /// 施設マスタ取得
        /// </summary>
        /// <returns></returns>
        public List<CouponInfoEntity> NishitetsuSelectList()
        {
            List<CouponInfoEntity> result = new List<CouponInfoEntity>();
            using (SqlDbInterface NTdbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("SELECT * from FacilityMaster");

                cmd.CommandText = sb.ToString();

                DataTable dt = NTdbInterface.ExecuteReader(cmd);

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new CouponInfoEntity { FacilityName = row["FacilityName"].ToString()});
                    result.Add(new CouponInfoEntity { FacilityId = row["FacilityId"].ToString() });
                }
                return result;
            }
        }

        /// <summary>
        /// 施設マスタ取得
        /// </summary>
        /// <returns>SQL実行結果</returns>
        public DataTable GetFacilityNames()
        {
            using (SqlDbInterface NTdbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("SELECT fclM.FacilityName,");
                sb.AppendLine("       fclM.FacilityId");
                sb.AppendLine("FROM FacilityMaster fclM");
                //sb.AppendLine("SELECT * FROM FacilityMaster");

                cmd.CommandText = sb.ToString();

                return NTdbInterface.ExecuteReader(cmd);
            }
        }
   
        /// <summary>
        /// テナントマスタ取得
        /// </summary>
        /// <returns>SQL実行結果</returns>
        public DataTable GetShopNames()
        {
            using (SqlDbInterface NTdbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("SELECT shpM.ShopName,");
                sb.AppendLine("       shpM.ShopCode");
                sb.AppendLine("FROM ShopMaster shpM");
                cmd.CommandText = sb.ToString();

                return NTdbInterface.ExecuteReader(cmd);
            }
        }
    }
}