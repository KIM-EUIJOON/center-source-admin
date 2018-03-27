using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AppSigmaAdmin.Utility
{
    /// <summary>
    /// イベントソース
    /// </summary>
    [EventSource(Name = "sigma-service-app")]
    public sealed class LogEventSource : EventSource
    {
        /// <summary>
        /// ログイベントソース
        /// </summary>
        public static readonly LogEventSource Log = new LogEventSource();
        /// <summary>
        /// キーワード
        /// </summary>
        public class Keywords
        {
            /// <summary>
            /// イベントキーワード
            /// </summary>
            public const EventKeywords Logging = (EventKeywords)1;
        }


        #region パブリップ メソッド

        /// <summary>
        /// トレースログ（エラー出力）
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        [NonEvent]
        //public void Error(string message)
        public void Error(string message)
        {
            errorTrace(
                "AppSigmaAdmin",
                message);
            //WriteEvent(1, serviceName, serviceTypeName, replicaOrInstanceId, partitionId, applicationName, applicationTypeName, nodeName, message);
        }

        #endregion

        #region プライベート メソッド

        ///// <summary>
        ///// トレースログ（エラー出力）
        ///// </summary>
        ///// <param name="serviceName"></param>
        ///// <param name="serviceTypeName"></param>
        ///// <param name="replicaOrInstanceId"></param>
        ///// <param name="partitionId"></param>
        ///// <param name="applicationName"></param>
        ///// <param name="applicationTypeName"></param>
        ///// <param name="nodeName"></param>
        ///// <param name="message"></param>
        //[Event(1, Level = EventLevel.Error, Keywords = Keywords.Logging, Message = "An error has occurred")]
        //private void errorTrace(
        //    string serviceName,
        //    string serviceTypeName,
        //    long replicaOrInstanceId,
        //    string partitionId,
        //    string applicationName,
        //    string applicationTypeName,
        //    string nodeName,
        //    string message)
        //{
        //    WriteEvent(1, serviceName, serviceTypeName, replicaOrInstanceId, partitionId, applicationName, applicationTypeName, nodeName, message);
        //}

        /// <summary>
        /// トレースログ（エラー出力）
        /// </summary>
        /// <param name="serviceName">サービス名</param>
        /// <param name="message">エラーメッセージ</param>
        [Event(1, Level = EventLevel.Error, Keywords = Keywords.Logging, Message = "An error has occurred")]
        private void errorTrace(
            string serviceName,
            string message)
        {
            WriteEvent(1, serviceName, message);
        }

        #endregion
    }
}