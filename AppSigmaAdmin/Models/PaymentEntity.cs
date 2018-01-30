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
    /// 決済管理リクエストクラス
    /// </summary>
    public class PaymentEntity
    {
        /// <summary>
        /// サービス名
        /// </summary>
        [Name("サービス名")]
        public string ServiceName { get; set; }

        /// <summary>
        /// 会員ID
        /// </summary>
        [Name("会員ID")]
        public string MemberId { get; set; }

        /// <summary>
        /// オーダーID
        /// </summary>
        [Name("オーダーID")]
        public string OrderId { get; set; }

        /// <summary>
        /// 取引ID
        /// </summary>
        [Name("取引ID")]
        public string AccessId { get; set; }

        /// <summary>
        /// 取引パスワード
        /// </summary>
        [Name("取引パスワード")]
        public string AccessPass { get; set; }

        /// <summary>
        /// 注文日付
        /// </summary>
        [Name("注文日付")]
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// 予約注文日付
        /// </summary>
        [Name("予約注文日付")]
        public DateTime ReserveOrderDate { get; set; }

        /// <summary>
        /// 料金
        /// </summary>
        [Name("料金")]
        public int Amount { get; set; }

        /// <summary>
        /// 注文番号
        /// </summary>
        [Name("注文番号")]
        public string OrderNo { get; set; }

        /// <summary>
        /// 仕向先コード
        /// </summary>
        [Name("仕向先コード")]
        public string ForwardCode { get; set; }

        /// <summary>
        /// 支払方法
        /// </summary>
        [Name("支払方法")]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// 支払回数
        /// </summary>
        [Name("支払回数")]
        public int? PayTimes { get; set; }

        /// <summary>
        /// 承認番号
        /// </summary>
        [Name("承認番号")]
        public string ApproveNo { get; set; }
        
        /// <summary>
        /// トランザクションID
        /// </summary>
        [Name("トランザクションID")]
        public string TranId { get; set; }

        /// <summary>
        /// 取引パスワード
        /// </summary>
        [Name("決済日時")]
        public DateTime? TranDate { get; set; }

        /// <summary>
        /// GMOステータス
        /// </summary>
        [Name("ステータス")]
        public string Status { get; set; }

    }

    /// <summary>
    /// 決済データCSVマッピングクラス
    /// </summary>
    public sealed class PaymentClassMap : ClassMap<PaymentEntity>
    {
        public PaymentClassMap()
        {
            Map(m => m.ServiceName).Index(0);
            Map(m => m.MemberId).Index(1);
            Map(m => m.OrderId).Index(2);
            Map(m => m.AccessId).Index(3);
            Map(m => m.AccessPass).Index(4);
            Map(m => m.OrderDate).Index(5);
            Map(m => m.ReserveOrderDate).Index(6);
            Map(m => m.Amount).Index(7);
            Map(m => m.OrderNo).Index(8);
            Map(m => m.ForwardCode).Index(9);
            Map(m => m.PaymentMethod).Index(10);
            Map(m => m.PayTimes).Index(11);
            Map(m => m.ApproveNo).Index(12);
            Map(m => m.TranId).Index(13);
            Map(m => m.TranDate).Index(14);
            Map(m => m.Status).Index(15);

        }
    }

    /// <summary>
    /// 決済データ取得クラス
    /// </summary>
    public class PaymentModel
    {
        /// <summary>
        /// 決済データ取得
        /// </summary>
        /// <param name="fromDate">検索開始日</param>
        /// <param name="toDate">検索終了日</param>
        /// <returns>決済データ</returns>
        public List<PaymentEntity> GetPaymentModel(DateTime fromDate, DateTime toDate)
        {
            List<PaymentEntity> payment = new List<PaymentEntity>();

            using (SqlDbInterface dbInterface = new SqlDbInterface())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select ServiceId as ServiceName");
                sb.AppendLine("     , MemberId");
                sb.AppendLine("     , OrderId");
                sb.AppendLine("     , AccessId");
                sb.AppendLine("     , AccessPass");
                sb.AppendLine("     , OrderDate");
                sb.AppendLine("     , ReserveOrderDate");
                sb.AppendLine("     , Amount");
                sb.AppendLine("     , OrderNo");
                sb.AppendLine("     , ForwardCode");
                sb.AppendLine("     , PaymentMethod");
                sb.AppendLine("     , PayTimes");
                sb.AppendLine("     , ApproveNo");
                sb.AppendLine("     , TranId");
                sb.AppendLine("     , TranDate");
                sb.AppendLine("     , case GmoStatus");
                sb.AppendLine("            when '1' then N'取引OK'");
                sb.AppendLine("            when '2' then N'取引NG'");
                sb.AppendLine("            when '3' then N'決済OK'");
                sb.AppendLine("            when '4' then N'決済NG'");
                sb.AppendLine("            end as Status");
                sb.AppendLine("  from PaymentManage");
                sb.AppendLine(" where OrderDate between '" + fromDate.ToString("yyyy-MM-dd") + "' and '" + toDate.ToString("yyyy-MM-dd") + " 23:59:59'");

                DataTable dt = dbInterface.ExecuteReader(sb.ToString());

                if(dt != null || dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        PaymentEntity paymentEntity = new PaymentEntity()
                        {
                            ServiceName = row["ServiceName"].ToString(),
                            MemberId = row["MemberId"].ToString(),
                            OrderId = row["OrderId"] == DBNull.Value ? null : row["OrderId"].ToString(),
                            AccessId = row["AccessId"] == DBNull.Value ? null : row["AccessId"].ToString(),
                            AccessPass = row["AccessPass"] == DBNull.Value ? null : row["AccessPass"].ToString(),
                            OrderDate = (DateTime)row["OrderDate"],
                            ReserveOrderDate = (DateTime)row["ReserveOrderDate"],
                            Amount = (int)row["Amount"],
                            OrderNo = row["OrderNo"] == DBNull.Value ? null : row["OrderNo"].ToString(),
                            ForwardCode = row["ApproveNo"] == DBNull.Value ? null : row["ForwardCode"].ToString(),
                            PaymentMethod = row["PaymentMethod"] == DBNull.Value ? null : row["PaymentMethod"].ToString(),
                            PayTimes = row["PayTimes"] == DBNull.Value ? null : (int?)row["PayTimes"] ,
                            ApproveNo = row["ApproveNo"] == DBNull.Value ? null : row["ApproveNo"].ToString(),
                            TranId = row["TranId"] == DBNull.Value ? null : row["TranId"].ToString(),
                            TranDate = row["TranDate"] ==DBNull.Value ? null : (DateTime?)row["TranDate"],
                            Status = row["Status"] == DBNull.Value ? null : row["Status"].ToString()
                        };

                        payment.Add(paymentEntity);
                    }
                }
            }

            return payment;
        }
    }
}