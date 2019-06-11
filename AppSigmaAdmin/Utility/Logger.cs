using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;
using System.Threading;
using AppSigmaAdmin.Library;

namespace AppSigmaAdmin.Utility
{
    /// <summary>
    /// ログ出力クラス
    /// </summary>
    public class Logger
    {
        #region コンフィグ情報（プライベート）

        /// <summary>Debugログテーブル名</summary>
        private const string STORAGE_TABLE_NAME = "SigmaAdminLogsTable";

        /// <summary>Infomationログテーブル名</summary>
        private const string STORAGE_INFO_TABLE_NAME = "SigmaAdminInfomationLogsTable";

        #endregion

        #region プライベート変数定義

        private static CloudStorageAccount storageAccount;
        private static CloudTableClient tableClient;
        private static CloudTable table;
        private static CloudTable infoTable;
        private static Object thisLock = new Object();
        private static long logCounter;
        private static long infoLogCounter;
        // private static DateTime infoLogLastTimestamp = new DateTime();

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        static Logger()
        {
            // ログ出力テーブルへの参照を取得
            storageAccount = CloudStorageAccount.Parse(ApplicationConfig.StorageConnectionString);

            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(STORAGE_TABLE_NAME);
            infoTable = tableClient.GetTableReference(STORAGE_INFO_TABLE_NAME);
            infoTable.CreateIfNotExists();

            // テーブルがなければ作成
            table.CreateIfNotExists();
            infoTable.CreateIfNotExists();

            // カウンタの初期化
            Interlocked.Exchange(ref logCounter, 0);
            Interlocked.Exchange(ref infoLogCounter, 0);
        }

        #endregion

        #region パブリック メソッド メンバー

        /// <summary>
        /// ログ出力（デバック）
        /// </summary>
        /// <param name="timestamp">タイムスタンプ</param>
        /// <param name="message">メッセージ</param>
        /// <param name="userId">ユーザID</param>
        /// <param name="taskId">タスクID</param>
        public static void TraceDebug(DateTime timestamp, string message, string userId, string taskId)
        {
            // レコード追加用エンティティ生成
            long _logCounter = Interlocked.Increment(ref logCounter);
            LogEntity logEntity = new LogEntity(LogLevel.Debug, timestamp, _logCounter, message, userId, taskId);
            TableOperation insertOperation = TableOperation.Insert(logEntity);

            table.Execute(insertOperation);
        }

        /// <summary>
        /// ログ出力（インフォメーション）
        /// </summary>
        /// <param name="timestamp">タイムスタンプ</param>
        /// <param name="userId">ユーザID</param>
        /// <param name="message">メッセージ</param>
        /// <param name="inputParams">入力値</param>
        public static void TraceInfo(DateTime timestamp, string userId, string message, string inputParams)
        {
            //// 2019/05/20 Y.Furuyama コンフリクトエラー回避のため、一旦コメント化
            //// 日付が変わった場合
            //if (timestamp.Date != infoLogLastTimestamp.Date)
            //{
            //    // カウンタ初期化
            //    Interlocked.Exchange(ref infoLogCounter, 0);
            //}
            //infoLogLastTimestamp = timestamp;

            //long _infoLogCounter = Interlocked.Increment(ref infoLogCounter);
            //// レコード追加用エンティティ生成
            //LogEntity logEntity = new LogEntity(LogLevel.Information, timestamp, _infoLogCounter, message, userId, null, inputParams);
            //TableOperation insertOperation = TableOperation.Insert(logEntity);

            //infoTable.Execute(insertOperation);

            string infoString = "UserID:" + userId + "  Message:" + message;
            System.Diagnostics.Trace.TraceInformation(infoString);
        }

        /// <summary>
        /// 指定されたExceptionのメッセージを取得します。
        /// </summary>
        /// <param name="e">メッセージを取得したいExceptionクラス</param>
        /// <returns>取得したメッセージ</returns>
        public static string GetExceptionMessage(Exception e)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("== タイプ ==");
            sb.AppendLine("  " + e.GetType().ToString());
            sb.AppendLine("== メッセージ ==");
            sb.AppendLine("  " + e.Message);
            sb.AppendLine("== スタックトレース ==");
            sb.AppendLine(e.StackTrace);

            if (e.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine(GetExceptionMessage(e.InnerException));
            }

            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// ログテーブルクラス
        /// </summary>
        public class LogEntity : TableEntity
        {
            /// <summary>
            /// ログ出力メソッド
            /// </summary>
            /// <param name="logLevel">レベル</param>
            /// <param name="procTimestamp">タイムスタンプ</param>
            /// <param name="logCounter">ログカウンタ</param>
            /// <param name="message">メッセージ</param>
            /// <param name="userId">ユーザID</param>
            /// <param name="taskId">[タスクID](省略可)</param>
            /// <param name="inputParams">入力パラメータ(省略可)</param>
            public LogEntity(LogLevel logLevel, DateTime procTimestamp, long logCounter, string message, string userId, string taskId = null, string inputParams = null)
            {
                this.PartitionKey = Utility.Common.Utc2JstTime(DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss.ffff");
                this.RowKey = DateTime.Now.Ticks.ToString();
                this.Level = logLevel.ToString();
                this.Message = message;
                this.UserId = userId;
                this.TaskId = taskId;
                this.InputParams = inputParams;
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public LogEntity() { }
            /// <summary>
            /// ログレベル
            /// </summary>
            public string Level { get; set; }
            /// <summary>
            /// ユーザID
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// タスクID
            /// </summary>
            public string TaskId { get; set; }
            /// <summary>
            /// メッセージ
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 入力パラメータ
            /// </summary>
            public string InputParams { get; set; }
        }
    }

    /// <summary>
    /// ログレベル
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// エラー
        /// </summary>
        Error,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 情報
        /// </summary>
        Information,
        /// <summary>
        /// デバッグ
        /// </summary>
        Debug,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class LogLevelExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public static string ToString(this LogLevel logLevel)
        {
            string[] names = { "Error", "Warning", "Information", "Debug" };
            return names[(int)logLevel];
        }
    }
}