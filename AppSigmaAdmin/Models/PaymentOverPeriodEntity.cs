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
        [Name("ユーザID")]
        public string UserId { get; set; }

        /// <summary>
        /// カナ名字
        /// </summary>
        [Name("名字")]
        public string KanaLastName { get; set; }

        /// <summary>
        /// カナ名前
        /// </summary>
        [Name("名前")]
        public string KanaFirstName { get; set; }

        /// <summary>
        /// 電話番号
        /// </summary>
        [Name("電話番号")]
        public string TelNo { get; set; }

        /// <summary>
        /// 経路No
        /// </summary>
        [Name("経路No")]
        public string RouteNo { get; set; }

        /// <summary>
        /// 地点No
        /// </summary>
        [Name("地点No")]
        public string PointNo { get; set; }

        /// <summary>
        /// 交通手段
        /// </summary>
        [Name("交通手段")]
        public string Transportation { get; set; }

        /// <summary>
        /// 出発地名称
        /// </summary>
        [Name("出発地名称")]
        public string Departure { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        [Name("目的地名称")]
        public string Destination { get; set; }

        /// <summary>
        /// 会社名
        /// </summary>
        [Name("会社名")]
        public string CompanyName { get; set; }

        /// <summary>
        /// 乗車時刻
        /// </summary>
        [Name("乗車時刻")]
        public DateTime PickUpDatetime { get; set; }

        /// <summary>
        /// 料金
        /// </summary>
        [Name("料金")]
        public int? Price { get; set; }

        /// <summary>
        /// 支払い方法
        /// </summary>
        [Name("支払い方法")]
        public string Payment { get; set; }

        /// <summary>
        /// 乗車緯度
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        [Name("乗車緯度")]
        public double? DepLat { get; set; }

        /// <summary>
        /// 乗車経度
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        [Name("乗車経度")]
        public double? DepLng { get; set; }

        /// <summary>
        /// 目的地緯度
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        [Name("目的地緯度")]
        public double? DesLat { get; set; }

        /// <summary>
        /// 目的地経度
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        [Name("目的地経度")]
        public double? DesLng { get; set; }

        /// <summary>
        /// 無線番号
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        [Name("無線番号")]
        public string RadioNo { get; set; }

        /// <summary>
        /// 迎車料金
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        [Name("迎車料金")]
        public int? PickUpPrice { get; set; }

        /// <summary>
        /// ナンバープレート
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        [Name("ナンバープレート")]
        public string NumberPlate { get; set; }

        /// <summary>
        /// オーダID
        /// </summary>
        /// <remarks>
        /// タクシーの場合に使用
        /// </remarks>
        [Name("オーダID")]
        public string OrderId { get; set; }

        /// <summary>
        /// 予約状況
        /// </summary>
        [Name("予約状況")]
        public string Status { get; set; }

    }

    /// <summary>
    /// 未決済データCSVマッピングクラス
    /// </summary>
    public sealed class PaymentOverPeriodClassMap : ClassMap<PaymentOverPeriodEntity>
    {
        public PaymentOverPeriodClassMap()
        {
            Map(m => m.UserId).Index(0);
            Map(m => m.KanaLastName).Index(1);
            Map(m => m.KanaFirstName).Index(2);
            Map(m => m.TelNo).Index(3);
            Map(m => m.RouteNo).Index(4);
            Map(m => m.PointNo).Index(5);
            Map(m => m.Transportation).Index(6);
            Map(m => m.Departure).Index(7);
            Map(m => m.Destination).Index(8);
            Map(m => m.CompanyName).Index(9);
            Map(m => m.PickUpDatetime).Index(10);
            Map(m => m.Price).Index(11);
            Map(m => m.Payment).Index(12);
            Map(m => m.DepLat).Index(13);
            Map(m => m.DepLng).Index(14);
            Map(m => m.DesLat).Index(13);
            Map(m => m.DesLng).Index(14);
            Map(m => m.RadioNo).Index(15);
            Map(m => m.PickUpPrice).Index(16);
            Map(m => m.NumberPlate).Index(17);
            Map(m => m.OrderId).Index(18);
            Map(m => m.Status).Index(19);
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
                sb.AppendLine("   and PickUpDatetime between '" + fromDate.ToString("yyyy-MM-dd") + "' and '" + toDate.ToString("yyyy-MM-dd") + " 23:59:59'");

                DataTable dt = dbInterface.ExecuteReader(sb.ToString());

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