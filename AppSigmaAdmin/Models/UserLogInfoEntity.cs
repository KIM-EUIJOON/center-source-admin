using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Library;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// 利用者ログ情報出力クラス
    /// </summary>
    public class UserLogInfoEntity
    {
        /// <summary>
        /// タイムスタンプ(JST)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// パーティションキー(JST)
        /// </summary>
        public DateTime PartitionKey { get; set; }

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
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 言語種別
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// OS名
        /// </summary>
        public string OsName { get; set; }

        /// <summary>
        /// OSバージョン
        /// </summary>
        public string OsVersion { get; set; }

        /// <summary>
        /// 端末ID
        /// </summary>
        public string MobileId { get; set; }

        /// <summary>
        /// 端末名
        /// </summary>
        public string MobileName { get; set; }

        /// <summary>
        /// Google Play Servicesバージョン
        /// </summary>
        public string GooglePlayServicesVersion { get; set; }

        /// <summary>
        /// 位置情報(経度,緯度)
        /// </summary>
        public string LocationInformation { get; set; }

        /// <summary>
        /// レベル
        /// </summary>
        public string Level { get; set; }

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

        /// <summary>
        /// Pushトークン
        /// </summary>
        public string PushToken { get; set; }

        /// <summary>
        /// プロダクト種別
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// アプリバージョン
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// 通信キャリア
        /// </summary>
        public string TelecomCarrier { get; set; }

        /// <summary>
        /// その他情報(JSON形式)
        /// </summary>
        public string ExtraInformation { get; set; }
    }

    /// <summary>
    /// 利用者ログ情報レポートデータCSVマッピングクラス
    /// </summary>
    public sealed class UserLogInfoClassMap : ClassMap<UserLogInfoEntity>
    {
        /// <summary>
        /// 利用者ログ情報レポートデータCSVマッピング
        /// </summary>
        public UserLogInfoClassMap()
        {
            Map(m => m.Timestamp).Index(0).Name("タイムスタンプ(JST)");
            Map(m => m.PartitionKey).Index(1).Name("パーティションキー(JST)");
            Map(m => m.InfoTypeName).Index(2).Name("情報種別");
            Map(m => m.RequestUserId).Index(3).Name("端末ログ保存時のユーザID");
            Map(m => m.RequestMobileId).Index(4).Name("端末ログ保存時の端末ID");
            Map(m => m.UserId).Index(5).Name("ユーザID");
            Map(m => m.Language).Index(6).Name("言語種別");
            Map(m => m.OsName).Index(7).Name("OS名");
            Map(m => m.OsVersion).Index(8).Name("OSバージョン");
            Map(m => m.MobileId).Index(9).Name("端末ID");
            Map(m => m.MobileName).Index(10).Name("端末名");
            Map(m => m.GooglePlayServicesVersion).Index(11).Name("Google Play Servicesバージョン");
            Map(m => m.LocationInformation).Index(12).Name("位置情報(経度,緯度)");
            Map(m => m.Level).Index(13).Name("レベル");
            Map(m => m.TaskId).Index(14).Name("タスクID");
            Map(m => m.Message).Index(15).Name("メッセージ");
            Map(m => m.InputParams).Index(16).Name("入力パラメータ");
            Map(m => m.PushToken).Index(17).Name("Pushトークン");
            Map(m => m.ProductType).Index(18).Name("プロダクト種別");
            Map(m => m.AppVersion).Index(19).Name("アプリバージョン");
            Map(m => m.TelecomCarrier).Index(20).Name("通信キャリア");
            Map(m => m.ExtraInformation).Index(21).Name("その他情報(JSON形式)");
        }
    }

    /// <summary>
    /// 利用者ログ情報リスト作成用情報
    /// </summary>
    public class UserLogInfoListEntity : UserLogInfoEntity
    {
        /// <summary>
        /// 抽出開始指定日(YYYY-MM-DD)
        /// </summary>
        [DataMember(Name = "targetDateBegin")]
        public string TargetDateBegin { get; set; }

        /// <summary>
        /// 抽出開始指定時刻(HH)
        /// </summary>
        [DataMember(Name = "startHour")]
        public string StartHour { get; set; }

        /// <summary>
        /// 抽出開始指定時刻(mm)
        /// </summary>
        [DataMember(Name = "startMinute")]
        public string StartMinute { get; set; }

        /// <summary>
        /// 抽出終了指定日(YYYY-MM-DD)
        /// </summary>
        [DataMember(Name = "targetDateEnd")]
        public string TargetDateEnd { get; set; }

        /// <summary>
        /// 抽出終了指定時刻(HH)
        /// </summary>
        [DataMember(Name = "endHour")]
        public string EndHour { get; set; }

        /// <summary>
        /// 抽出終了指定時刻(mm)
        /// </summary>
        [DataMember(Name = "endMinute")]
        public string EndMinute { get; set; }

        /// <summary>
        /// インフォメーションログ取得有無
        /// </summary>
        [DataMember(Name = "isInformationLog")]
        public bool IsInformationLog { get; set; }

        /// <summary>
        /// インフォメーションログ検索条件：メッセージ1
        /// </summary>
        [DataMember(Name = "informationLogMessage1")]
        public string InformationLogMessage1 { get; set; }

        /// <summary>
        /// インフォメーションログ検索条件：メッセージ2
        /// </summary>
        [DataMember(Name = "informationLogMessage2")]
        public string InformationLogMessage2 { get; set; }

        /// <summary>
        /// デバッグログ取得有無
        /// </summary>
        [DataMember(Name = "isDebugLog")]
        public bool IsDebugLog { get; set; }

        /// <summary>
        /// デバッグログ検索条件：メッセージ1
        /// </summary>
        [DataMember(Name = "debugLogMessage1")]
        public string DebugLogMessage1 { get; set; }

        /// <summary>
        /// デバッグログ検索条件：メッセージ2
        /// </summary>
        [DataMember(Name = "debugLogMessage2")]
        public string DebugLogMessage2 { get; set; }

        /// <summary>
        /// 端末ログ取得有無
        /// </summary>
        [DataMember(Name = "isMobileLog")]
        public bool IsMobileLog { get; set; }

        /// <summary>
        /// 端末インフォメーションログ検索条件：端末ID
        /// </summary>
        [DataMember(Name = "mobileInformationLogMobileId")]
        public string MobileInformationLogMobileId { get; set; }

        /// <summary>
        /// 端末インフォメーションログ検索条件：メッセージ1
        /// </summary>
        [DataMember(Name = "mobileInformationLogMessage1")]
        public string MobileInformationLogMessage1 { get; set; }

        /// <summary>
        /// 端末インフォメーションログ検索条件：メッセージ2
        /// </summary>
        [DataMember(Name = "mobileInformationLogMessage2")]
        public string MobileInformationLogMessage2 { get; set; }

        /// <summary>
        /// 端末ログ取得キー
        /// </summary>
        [DataMember(Name = "mobileLogKey")]
        public string MobileLogKey { get; set; }

        /// <summary>
        /// 利用者ログ情報一覧
        /// </summary>
        public List<UserLogInfoEntity> UserLogInfoList { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UserLogInfoListEntity()
        {
            this.UserLogInfoList = new List<UserLogInfoEntity>();
        }
    }
}
