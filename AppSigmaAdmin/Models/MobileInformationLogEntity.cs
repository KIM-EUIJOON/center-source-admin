using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// 端末ログテーブルクラス
    /// </summary>
    public class MobileInformationLogEntity : TableEntity
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MobileInformationLogEntity() { }

        /// <summary>
        /// 情報種別名
        /// </summary>
        public string InfoTypeName { get; set; }

        /// <summary>
        /// リクエスト.ユーザID
        /// </summary>
        public string RequestUserId { get; set; }

        /// <summary>
        /// リクエスト.端末ID
        /// </summary>
        public string RequestMobileId { get; set; }

        /// <summary>
        /// ログレベル
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 端末タイムスタンプ
        /// </summary>
        public DateTime MobileTimestamp { get; set; }

        /// <summary>
        /// Pushトークン
        /// </summary>
        public string PushToken { get; set; }

        /// <summary>
        /// 端末ID
        /// </summary>
        public string MobileId { get; set; }

        /// <summary>
        /// 端末名
        /// </summary>
        public string MobileName { get; set; }

        /// <summary>
        /// プロダクト種別
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// アプリバージョン
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// OS名
        /// </summary>
        public string OsName { get; set; }

        /// <summary>
        /// OSバージョン
        /// </summary>
        public string OsVersion { get; set; }

        /// <summary>
        /// 通信キャリア
        /// </summary>
        public string TelecomCarrier { get; set; }

        /// <summary>
        /// Google Play Servicesバージョン
        /// </summary>
        public string GooglePlayServicesVersion { get; set; }

        /// <summary>
        /// 言語情報
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 位置情報(経度,緯度)
        /// </summary>
        public string LocationInformation { get; set; }

        /// <summary>
        /// その他情報(JSON形式)
        /// </summary>
        public string ExtraInformation { get; set; }
    }
}
