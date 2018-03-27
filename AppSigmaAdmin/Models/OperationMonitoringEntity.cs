using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// 運用レポート出力クラス
    /// </summary>
    public class OperationMonitoringEntity
    {
        /// <summary>
        /// タイムスタンプ
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// ロールインスタンス(スケールセットのノードインスタンス)
        /// </summary>
        public string RoleInstance { get; set; }
        /// <summary>
        /// パフォーマンスカウンタ名
        /// </summary>
        public string CounterName { get; set; }
        /// <summary>
        /// パフォーマンスカウンタ値
        /// </summary>
        public double CounterValue { get; set; }
    }

    /// <summary>
    /// 運用レポートデータCSVマッピングクラス
    /// </summary>
    public sealed class OperationMonitoringtClassMap : ClassMap<OperationMonitoringEntity>
    {
        /// <summary>
        /// 運用レポートデータCSVマッピング
        /// </summary>
        public OperationMonitoringtClassMap()
        {
            Map(m => m.Timestamp).Index(0).Name("タイムスタンプ");
            Map(m => m.RoleInstance).Index(1).Name("インスタンス名");
            Map(m => m.CounterName).Index(2).Name("カウンター名");
            Map(m => m.CounterValue).Index(3).Name("カウンター値");
        }
    }

}