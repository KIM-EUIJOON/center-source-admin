using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Configuration;
using System.Data.SqlClient;

namespace AppSigmaAdmin.Models
{
        
    /// <summary>
    /// 未決済情報クラス
    /// </summary>
    public class PaymentOverPeriodEntity
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// カナ名字
        /// </summary>
        public string KanaLastName { get; set; }

        /// <summary>
        /// カナ名前
        /// </summary>
        public string KanaFirstName { get; set; }

        /// <summary>
        /// 電話番号
        /// </summary>
        public string TelNo { get; set; }

        /// <summary>
        /// 経路No
        /// </summary>
        public string RouteNo { get; set; }

        /// <summary>
        /// 地点No
        /// </summary>
        public string PointNo { get; set; }

        /// <summary>
        /// 交通手段
        /// </summary>
        public string Transportation { get; set; }

        /// <summary>
        /// 出発地名称
        /// </summary>
        public string Departure { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 乗車時刻
        /// </summary>
        public DateTime PickUpDatetime { get; set; }

        /// <summary>
        /// 料金
        /// </summary>
        public int? Price { get; set; }

        /// <summary>
        /// 支払い方法
        /// </summary>
        public string Payment { get; set; }

        /// <summary>
        /// 乗車緯度
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        public double? DepLat { get; set; }

        /// <summary>
        /// 乗車経度
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        public double? DepLng { get; set; }

        /// <summary>
        /// 目的地緯度
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        public double? DesLat { get; set; }

        /// <summary>
        /// 目的地経度
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        public double? DesLng { get; set; }

        /// <summary>
        /// 無線番号
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        public string RadioNo { get; set; }

        /// <summary>
        /// 迎車料金
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        public int? PickUpPrice { get; set; }

        /// <summary>
        /// ナンバープレート
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        public string NumberPlate { get; set; }

        /// <summary>
        /// オーダID
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        public string OrderId { get; set; }

        /// <summary>
        /// 予約状況
        /// </summary>
        public string Status { get; set; }

    }

    /// <summary>
    /// 未決済データCSVマッピングクラス
    /// </summary>
    public sealed class PaymentOverPeriodClassMap : ClassMap<PaymentOverPeriodEntity>
    {
        public PaymentOverPeriodClassMap()
        {
            Map(m => m.UserId).Index(0).Name("ユーザID");
            Map(m => m.KanaLastName).Index(1).Name("名字");
            Map(m => m.KanaFirstName).Index(2).Name("名前");
            Map(m => m.TelNo).Index(3).Name("電話番号");
            Map(m => m.RouteNo).Index(4).Name("経路No");
            Map(m => m.PointNo).Index(5).Name("地点No");
            Map(m => m.Transportation).Index(6).Name("交通手段");
            Map(m => m.Departure).Index(7).Name("出発地名称");
            Map(m => m.Destination).Index(8).Name("目的地名称");
            Map(m => m.CompanyName).Index(9).Name("会社名");
            Map(m => m.PickUpDatetime).Index(10).Name("乗車時刻");
            Map(m => m.Price).Index(11).Name("料金");
            Map(m => m.Payment).Index(12).Name("支払方法");
            Map(m => m.DepLat).Index(13).Name("乗車緯度");
            Map(m => m.DepLng).Index(14).Name("乗車経度");
            Map(m => m.DesLat).Index(15).Name("目的地緯度");
            Map(m => m.DesLng).Index(16).Name("目的地経度");
            Map(m => m.RadioNo).Index(17).Name("無線番号");
            Map(m => m.PickUpPrice).Index(18).Name("迎車料金");
            Map(m => m.NumberPlate).Index(19).Name("ナンバープレート");
            Map(m => m.OrderId).Index(20).Name("オーダID");
            Map(m => m.Status).Index(21).Name("予約状況");
        }
    }

    /// <summary>
    /// 未決済データ取得クラス
    /// </summary>
    public class PaymentOverPeriodModel
    {
        public List<PaymentOverPeriodEntity> GetPaymentOverPeriodModel(DateTime fromDate, DateTime toDate)
        {
            List<PaymentOverPeriodEntity> payment = new List<PaymentOverPeriodEntity>();

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            using (SqlCommand cmd = new SqlCommand())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select mr.UserId");
                sb.AppendLine("     , ud.KanaLastName");
                sb.AppendLine("     , ud.KanaFirstName");
                sb.AppendLine("     , ud.TelNo");
                sb.AppendLine("     , mr.RouteNo");
                sb.AppendLine("     , mr.PointNo");
                sb.AppendLine("     , mr.TrsType");
                sb.AppendLine("     , mr.Departure");
                sb.AppendLine("     , mr.Destination");
                sb.AppendLine("     , mr.CompanyName");
                sb.AppendLine("     , mr.PickUpDatetime");
                sb.AppendLine("     , mr.Price");
                sb.AppendLine("     , mr.Payment");
                sb.AppendLine("     , mr.DepLat");
                sb.AppendLine("     , mr.DepLng");
                sb.AppendLine("     , mr.DesLat");
                sb.AppendLine("     , mr.DesLng");
                sb.AppendLine("     , mr.RadioNo");
                sb.AppendLine("     , mr.PickUpPrice");
                sb.AppendLine("     , mr.NumberPlate");
                sb.AppendLine("     , mr.OrderId");
                sb.AppendLine("     , mr.Status");
                sb.AppendLine("  from MobilityReserve mr");
                sb.AppendLine(" left join UserDetail ud");
                sb.AppendLine("    on mr.UserId = ud.UserId");
                sb.AppendLine(" where mr.Status = '5'");
                sb.AppendLine("   and PickUpDatetime between @FromDate and @ToDate");

                cmd.CommandText = sb.ToString();
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = fromDate;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = toDate.AddDays(1).AddSeconds(-1);

                DataTable dt = dbInterface.ExecuteReader(cmd);

                if(dt != null || dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        PaymentOverPeriodEntity paymentEntity = new PaymentOverPeriodEntity()
                        {
                            UserId = row["UserId"].ToString(),
                            KanaLastName = row["KanaLastName"].ToString(),
                            KanaFirstName = row["KanaFirstName"].ToString(),
                            TelNo = row["TelNo"].ToString(),
                            RouteNo = row["RouteNo"].ToString(),
                            PointNo = row["PointNo"].ToString(),
                            Transportation = row["TrsType"].ToString(),
                            Departure = row["Departure"].ToString(),
                            Destination = row["Destination"].ToString(),
                            CompanyName = row["CompanyName"].ToString(),
                            PickUpDatetime = (DateTime)row["PickUpDatetime"],
                            Price = row["Price"] == DBNull.Value ? null : (int?)row["Price"],
                            Payment = row["Payment"].ToString(),
                            DepLat = row["DepLat"] == DBNull.Value ? null : (double?)row["DepLat"],
                            DepLng = row["DepLng"] == DBNull.Value ? null : (double?)row["DepLng"],
                            DesLat = row["DesLat"] == DBNull.Value ? null : (double?)row["DesLat"],
                            DesLng = row["DesLng"] == DBNull.Value ? null : (double?)row["DesLng"],
                            RadioNo = row["RadioNo"].ToString(),
                            PickUpPrice = row["PickUpPrice"] == DBNull.Value ? null : (int?)row["PickUpPrice"],
                            NumberPlate = row["NumberPlate"].ToString(),
                            OrderId = row["OrderId"].ToString(),
                            Status = row["Status"].ToString()
                        };

                        payment.Add(paymentEntity);
                    }
                }
            }

            return payment;
        }
    }

}