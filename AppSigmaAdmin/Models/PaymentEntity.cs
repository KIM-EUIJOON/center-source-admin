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
        public string ServiceName { get; set; }

        /// <summary>
        /// 会員ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// オーダーID
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 枝番
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 取引ID
        /// </summary>
        public string AccessId { get; set; }

        /// <summary>
        /// 取引パスワード
        /// </summary>
        public string AccessPass { get; set; }

        /// <summary>
        /// 料金確定日付
        /// </summary>
        public DateTime FixedAmountDate { get; set; }

        /// <summary>
        /// 予約注文日付
        /// </summary>
        public DateTime ReserveOrderDate { get; set; }

        /// <summary>
        /// 料金
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 注文番号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 仕向先コード
        /// </summary>
        public string ForwardCode { get; set; }

        /// <summary>
        /// 支払方法
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// 支払回数
        /// </summary>
        public int? PayTimes { get; set; }

        /// <summary>
        /// 承認番号
        /// </summary>
        public string ApproveNo { get; set; }
        
        /// <summary>
        /// トランザクションID
        /// </summary>
        public string TranId { get; set; }

        /// <summary>
        /// 決済日時
        /// </summary>
        public DateTime? TranDate { get; set; }

        /// <summary>
        /// GMOステータス
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// エラー内容
        /// </summary>
        public string ErrorContents { get; set; }
    }

    /// <summary>
    /// 決済データCSVマッピングクラス
    /// </summary>
    public sealed class PaymentClassMap : ClassMap<PaymentEntity>
    {
        public PaymentClassMap()
        {
            Map(m => m.ServiceName).Index(0).Name("サービス名");
            Map(m => m.UserId).Index(1).Name("会員ID");
            Map(m => m.OrderId).Index(2).Name("オーダーID");
            Map(m => m.No).Index(3).Name("枝番");
            Map(m => m.AccessId).Index(4).Name("取引ID");
            Map(m => m.AccessPass).Index(5).Name("取引パスワード");
            Map(m => m.FixedAmountDate).Index(6).Name("料金確定日付");
            Map(m => m.ReserveOrderDate).Index(7).Name("予約注文日付");
            Map(m => m.Amount).Index(8).Name("料金");
            Map(m => m.OrderNo).Index(9).Name("注文番号");
            Map(m => m.ForwardCode).Index(10).Name("仕向先コード");
            Map(m => m.PaymentMethod).Index(11).Name("支払方法");
            Map(m => m.PayTimes).Index(12).Name("支払回数");
            Map(m => m.ApproveNo).Index(13).Name("承認番号");
            Map(m => m.TranId).Index(14).Name("トランザクションID");
            Map(m => m.TranDate).Index(15).Name("決済日時");
            Map(m => m.Status).Index(16).Name("ステータス");
            Map(m => m.Status).Index(17).Name("エラー内容");
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
                sb.AppendLine("     , UserId");
                sb.AppendLine("     , OrderId");
                sb.AppendLine("     , No");
                sb.AppendLine("     , AccessId");
                sb.AppendLine("     , AccessPass");
                sb.AppendLine("     , FixedAmountDate");
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
                sb.AppendLine("     , ErrorContents");
                sb.AppendLine("  from PaymentManage");
                sb.AppendLine(" where FixedAmountDate between '" + fromDate.ToString("yyyy-MM-dd") + "' and '" + toDate.ToString("yyyy-MM-dd") + " 23:59:59'");

                DataTable dt = dbInterface.ExecuteReader(sb.ToString());

                if(dt != null || dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        PaymentEntity paymentEntity = new PaymentEntity()
                        {
                            ServiceName = row["ServiceName"].ToString(),
                            UserId = row["MemberId"].ToString(),
                            OrderId = row["OrderId"] == DBNull.Value ? null : row["OrderId"].ToString(),
                            No = row["No"] == DBNull.Value ? null : ((int)row["No"]).ToString("00"),
                            AccessId = row["AccessId"] == DBNull.Value ? null : row["AccessId"].ToString(),
                            AccessPass = row["AccessPass"] == DBNull.Value ? null : row["AccessPass"].ToString(),
                            FixedAmountDate = (DateTime)row["FixedAmountDate"],
                            ReserveOrderDate = (DateTime)row["ReserveOrderDate"],
                            Amount = (int)row["Amount"],
                            OrderNo = row["OrderNo"] == DBNull.Value ? null : row["OrderNo"].ToString(),
                            ForwardCode = row["ApproveNo"] == DBNull.Value ? null : row["ForwardCode"].ToString(),
                            PaymentMethod = row["PaymentMethod"] == DBNull.Value ? null : row["PaymentMethod"].ToString(),
                            PayTimes = row["PayTimes"] == DBNull.Value ? null : (int?)row["PayTimes"] ,
                            ApproveNo = row["ApproveNo"] == DBNull.Value ? null : row["ApproveNo"].ToString(),
                            TranId = row["TranId"] == DBNull.Value ? null : row["TranId"].ToString(),
                            TranDate = row["TranDate"] ==DBNull.Value ? null : (DateTime?)row["TranDate"],
                            Status = row["Status"] == DBNull.Value ? null : row["Status"].ToString(),
                            ErrorContents = row["ErrorContents"] == DBNull.Value ? null : row["ErrorContents"].ToString()
                        };

                        payment.Add(paymentEntity);
                    }
                }
            }

            return payment;
        }
    }
}