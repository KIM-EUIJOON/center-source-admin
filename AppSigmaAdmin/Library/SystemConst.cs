using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Library
{
    /// <summary>
    /// システム定数クラス
    /// </summary>
    public class SystemConst
    {
        /// <summary>
        /// アプリバージョン
        /// </summary>
        /// <remarks>作成時時点（20181026）では、真ん中文字が「C」の場合、アプリバージョンはチェックしていない</remarks>
        public const string APL_VERSION = "MyRoute:C:0.0.1";

        /// <summary>
        /// セッション名（トークン）
        /// </summary>
        public const string SESSION_SIGMA_TOKEN = "SigmaToken";

        /// <summary>
        /// セッション名（管理ユーザ情報）
        /// </summary>
        public const string SESSION_USER_INFO_ADMIN = "UserInfoAdmin";

        /// <summary>
        /// セッション名（管理者権限情報）
        /// </summary>
        public const string SESSION_ROLE_INFO_ADMIN = "RoleInfoAdmin";
        /// <summary>
        /// HTTP通信(POST)
        /// </summary>
        public const string HTTP_METHOD_POST = "POST";

        /// <summary>
        /// HTTP通信(GET)
        /// </summary>
        public const string HTTP_METHOD_GET = "GET";

        /// <summary>HTTPタイムアウト</summary>
        public const string HTTP_STATUS_CODE_TIMEOUT = "901";
        /// <summary>HTTP受信エラー</summary>
        public const string HTTP_STATUS_CODE_RECEIVE_FAIL = "902";
        /// <summary>HTTP送信エラー</summary>
        public const string HTTP_STATUS_CODE_SEND_FAIL = "903";

        /**************************/
        /* 運用画面の定数         */
        /**************************/
        /// <summary>ページング行数</summary>
        public const int ROWS_PER_PAGE = 10;

        /// <summary>表示言語</summary>
        public const string LANGUAGE_JA = "ja";
        public const string LANGUAGE_EN = "en";

        /**************************/
        /* モビリティ予約の定数   */
        /**************************/
        /// <summary>
        /// 予約状況　未予約
        /// </summary>
        public const string MOBRSV_STATUS_UNRESERVED = "1";
        /// <summary>
        /// 予約状況　予約受付済
        /// </summary>
        public const string MOBRSV_STATUS_RECEIVED = "2";
        /// <summary>
        /// 予約状況　配車待
        /// </summary>
        public const string MOBRSV_STATUS_FINDING = "3";
        /// <summary>
        /// 予約状況　配車確定
        /// </summary>
        public const string MOBRSV_STATUS_DISPATCHED = "4";
        /// <summary>
        /// 予約状況　キャンセル済
        /// </summary>
        public const string MOBRSV_STATUS_CANCELED = "5";
        /// <summary>
        /// 予約状況　決済済
        /// </summary>
        public const string MOBRSV_STATUS_SETTLED = "11";
        /// <summary>
        /// 予約状況　決済期間超過
        /// </summary>
        public const string MOBRSV_STATUS_SETTLEMENT_PERIOD_OVER = "12";
        /// <summary>
        /// 予約状況　受付失敗
        /// </summary>
        public const string MOBRSV_STATUS_ACCEPT_FAILED = "96";
        /// <summary>
        /// 予約状況　配車割り当て不可(予約失敗)
        /// </summary>
        public const string MOBRSV_STATUS_DISPATCH_FAILED = "97";
        /// <summary>
        /// 予約状況　配車状況不明
        /// </summary>
        public const string MOBRSV_STATUS_DISPATCH_UNKNOWN = "98";
        /// <summary>
        /// 予約状況　システムエラー
        /// </summary>
        public const string MOBRSV_STATUS_FAIL = "99";

        /// <summary>
        /// 予約結果コード　成功
        /// </summary>
        public const string MOBRSV_CODE_SUCCESS = "0";
        /// <summary>
        /// 予約結果コード　配車依頼に失敗
        /// </summary>
        public const string MOBRSV_CODE_DISPATCH_ERROR = "1";
        /// <summary>
        /// 予約結果コード　既に予約済のため再予約不可
        /// </summary>
        public const string MOBRSV_CODE_RESERVED = "2";
        /// <summary>
        /// 予約結果コード　予約できない日時(20分以上〜1時間未満)
        /// </summary>
        public const string MOBRSV_CODE_UNAVAILABLE_DATETIME_20MIN_1H = "3";
        /// <summary>
        /// 予約結果コード　予約可能な会社が存在しない
        /// </summary>
        public const string MOBRSV_CODE_NO_COMPANY = "4";
        /// <summary>
        /// 予約結果コード　入力エラー
        /// </summary>
        public const string MOBRSV_CODE_INPUT_ERROR = "5";
        /// <summary>
        /// 予約結果コード　予約機能が無効
        /// </summary>
        public const string MOBRSV_CODE_DISABLE = "6";
        /// <summary>
        /// 予約結果コード　予約できない日時(1週間以降)
        /// </summary>
        public const string MOBRSV_CODE_UNAVAILABLE_DATETIME_1WEEK = "7";
        /// <summary>
        /// 予約結果コード　失敗(システムエラー)
        /// </summary>
        public const string MOBRSV_CODE_FAIL = "9";

        /// <summary>
        /// 予約キャンセル結果コード　成功
        /// </summary>
        public const string MOBRSV_CANCEL_CODE_SUCCESS = "0";
        /// <summary>
        /// 予約結果コード　キャンセルに失敗
        /// </summary>
        public const string MOBRSV_CANCEL_CODE_ERROR = "1";
        /// <summary>
        /// 予約キャンセル結果コード　キャンセル可能な時間を超過している
        /// </summary>
        public const string MOBRSV_CANCEL_CODE_TIME_OVER = "2";
        /// <summary>
        /// 予約キャンセル結果コード　キャンセル対応していない会社
        /// </summary>
        public const string MOBRSV_CANCEL_CODE_NOT_COVERED = "3";
        /// <summary>
        /// 予約結果コード　予約機能が無効
        /// </summary>
        public const string MOBRSV_CANCEL_CODE_DISABLE = "4";
        /// <summary>
        /// 予約キャンセル結果コード　失敗(システムエラー)
        /// </summary>
        public const string MOBRSV_CANCEL_CODE_FAIL = "9";

        /// <summary>
        /// 種別　予約中
        /// </summary>
        public const string MOBRSV_TYPE_UNDER_RESERVE = "0";
        /// <summary>
        /// 種別　履歴
        /// </summary>
        public const string MOBRSV_TYPE_HISTORY = "1";

        /// <summary>
        /// 電話番号国コードデフォルト(日本)
        /// </summary>
        public const string MOBRSV_DEFAULT_TEL_COUNTRY_CODE = "81";

        /// <summary>
        /// 支払い方法　現金
        /// </summary>
        public const string MOBRSV_PAYMENT_CASH = "0";
        /// <summary>
        /// 支払い方法　クレジットカード
        /// </summary>
        public const string MOBRSV_PAYMENT_CREDIT = "1";

        /// <summary>
        /// タクシーの配車状況　タクシーを探している
        /// </summary>
        public const string MOBRSV_TAXI_STATUS_FINDING = "finding";
        /// <summary>
        /// タクシーの配車状況　タクシー会社に予約ができた
        /// </summary>
        public const string MOBRSV_TAXI_STATUS_RESERVED = "reserved";
        /// <summary>
        /// タクシーの配車状況　タクシーが乗車場所に向かっている
        /// </summary>
        public const string MOBRSV_TAXI_STATUS_DISPATCHING = "dispatching";
        /// <summary>
        /// タクシーの配車状況　タクシーが見つからなかった
        /// </summary>
        public const string MOBRSV_TAXI_STATUS_DISPATCH_FAILED = "dispatch_failed";
        /// <summary>
        /// タクシーの配車状況　タクシーが到着している or 乗車済み
        /// </summary>
        public const string MOBRSV_TAXI_STATUS_SUCCEED = "succeed";
        /// <summary>
        /// タクシーの配車状況　タクシー会社にキャンセルの依頼をした
        /// </summary>
        public const string MOBRSV_TAXI_STATUS_CANCELING = "canceling";
        /// <summary>
        /// タクシーの配車状況　タクシー会社がキャンセルを受け入れた
        /// </summary>
        public const string MOBRSV_TAXI_STATUS_CANCELED = "canceled";

        /// <summary>
        /// インフォメーションログテーブル名
        /// </summary>
        public const string STORAGE_TABLE_NAME_INFORMATION_LOG = "SigmaInfomationLogsTable";
        /// <summary>
        /// デバッグログテーブル名
        /// </summary>
        public const string STORAGE_TABLE_NAME_DEBUG_LOG = "SigmaLogsTable";
        /// <summary>
        /// 端末インフォメーションログテーブル名
        /// </summary>
        public const string STORAGE_TABLE_NAME_MOBILE_INFORMATION_LOG = "SigmaMobileInformationLogsTable";
        /// <summary>
        /// アップロードファイルの情報種別　「情報種別:格納先 (カンマ区切り)」形式で設定
        /// </summary>
        public const string UPLOAD_FILE_INFO_TYPES = "0:mobile-info,1:general-info";
        /// <summary>
        /// 利用者ログ情報ダウンロード：出力ファイル名(User_Log_Report_作成日(yyyyMMddHHmmss)_S検索開始日時(yyyyMMddHHmm)-E終了日時(yyyyMMddHHmm))
        /// </summary>
        public const string USER_LOG_FILE_NAME = "User_Log_Report_{0}_S{1}_E{2}.csv";
        /// <summary>
        /// 利用者ログ情報ダウンロード：出力ファイルコンテンツタイプ(CSV)
        /// </summary>
        public const string USER_LOG_FILE_CONTENTTYPE = "text/comma-separated-values";
        /// <summary>
        /// 利用者ログ情報ダウンロード：検索条件の上限期間（開始日時～終了日時）
        /// </summary>
        public const int USER_LOG_SEARCH_LIMIT_DAYS = 31;
    }
}
