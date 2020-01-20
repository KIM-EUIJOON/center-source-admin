using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;


namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// クーポン運用画面　リクエストクラス
    /// </summary>
    public class CouponInfoModel
    {
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
                // データ取得
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("SELECT Cp.*, ");
                sb.AppendLine("    facM.FacilityName, ");
                sb.AppendLine("    shpM.ShopName, ");
                sb.AppendLine("    CASE");
                sb.AppendLine("       WHEN uio.AplType =1 THEN 'au' ");
                sb.AppendLine("       ELSE '' ");
                sb.AppendLine("    END AS AplName, ");
                sb.AppendLine("    indM.IndustryName ");
                sb.AppendLine("FROM CouponManage Cp ");
                sb.AppendLine("LEFT JOIN FacilityMaster facM ON Cp.UsageFacilityId = facM.FacilityId ");
                sb.AppendLine("     JOIN ShopMaster shpM ON Cp.UsageShopCode = shpM.ShopCode ");
                sb.AppendLine("     JOIN UserInfoOid uio ON Cp.UserId = uio.UserId ");
                sb.AppendLine("     JOIN IndustryMaster indM ON shpM.IndustryCode = indM.IndustryCode ");

                // 検索条件
                sb.AppendLine("WHERE 1 = 1");
                if (false == string.IsNullOrEmpty(model.UserId))
                {
                    sb.AppendLine("    AND Cp.UserId = @UserId");
                    cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = model.UserId;
                }
                if (false == string.IsNullOrEmpty(model.AplType))
                {
                    sb.AppendLine("    AND uio.AplType = @AplType ");
                    cmd.Parameters.Add("@AplType", SqlDbType.NVarChar).Value = model.AplType;
                }
                if (false == string.IsNullOrEmpty(model.FacilityId))
                {
                    sb.AppendLine("    AND facM.FacilityId = @FacilityId ");
                    cmd.Parameters.Add("@FacilityId", SqlDbType.NVarChar).Value = model.FacilityId;
                }
                if (false == string.IsNullOrEmpty(model.ShopCode))
                {
                    sb.AppendLine("    AND shpM.ShopCode = @ShopCode ");
                    cmd.Parameters.Add("@ShopCode", SqlDbType.NVarChar).Value = model.ShopCode;
                }
                sb.AppendLine("    AND Cp.UsageDateTime BETWEEN @StartDatatTime AND @EndDatatTime");

                sb.AppendLine("ORDER BY Cp.UsageDateTime");
                
                cmd.CommandText = sb.ToString();

                cmd.Parameters.Add("@StartDatatTime", SqlDbType.NVarChar).Value = model.TargetDateBegin;
                cmd.Parameters.Add("@EndDatatTime", SqlDbType.NVarChar).Value = model.TargetDateEnd + " 23:59:59";

                return dbInterface.ExecuteReader(cmd);
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
                sb.AppendLine("       shpM.ShopCode,");
                sb.AppendLine("       shpM.FacilityId,");
                sb.AppendLine("       facM.FacilityName");
                sb.AppendLine("FROM ShopMaster shpM");
                sb.AppendLine("LEFT JOIN FacilityMaster facM ON shpM.FacilityId = facM.FacilityId ");
                cmd.CommandText = sb.ToString();

                return NTdbInterface.ExecuteReader(cmd);
            }
        }
    }
}