using AppSigmaAdmin.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Utility
{
    /// <summary>
    /// 汎用機能クラス
    /// </summary>
    public class Common
    {
        /// <summary>
        /// 世界標準時から日本時間に変換します
        /// </summary>
        /// <param name="utcTime">世界標準時</param>
        /// <param name="isForced">true:強制変換する、false:強制変換しない</param>
        /// <returns>日本時間</returns>
        public static DateTime Utc2JstTime(DateTime utcTime, bool isForced = false)
        {
            // タイムゾーンを指定してUTCからJSTへ変換
            if (utcTime.Kind == DateTimeKind.Utc || isForced)
            {
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
            }
            else
            {
                return utcTime;
            }
        }

        /// <summary>
        /// 現在時刻を日本時間で取得
        /// </summary>
        /// <returns>現在時刻（日本時間）</returns>
        public static DateTime GetNowTimestamp()
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        }

        /// <summary>
        /// ビルド環境に合わせてURLを変更する
        /// </summary>
        /// <param name="url">URL（コントローラ名）</param>
        /// <param name="isTilde">チルダを表示するか</param>
        /// <returns>整形後のURL</returns>
        public static string CreateUrl(string url, bool isTilde = true)
        {
            string tilde = isTilde ? "~" : "";
            if (ApplicationConfig.DeployEnv == ApplicationConfig.ENV_DEBUG)
            {
                return tilde + url;
            }
            else
            {
                return tilde + "/sigma" + url;
            }
        }

        /// <summary>
        /// 入力チェック(ユーザID)
        /// </summary>
        /// <param name="userId">ユーザID</param>
        /// <returns>エラーメッセージ</returns>
        public static string CheckUserId(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    int.Parse(userId);
                }
                catch (OverflowException)
                {
                    // myrouteIDのテキストボックスに誤った数値が入力された場合
                    return "myroute会員IDに誤った数値が入力されました。半角数字で再入力してください。";
                }
                catch
                {
                    // myrouteIDのテキストボックスに半角数字以外が入力された場合
                    return "myroute会員IDが数字以外で入力されました。半角数字で再入力してください。";
                }
            }

            return string.Empty;
        }
    }
}