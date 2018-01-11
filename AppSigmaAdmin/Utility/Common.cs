﻿using System;
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
        /// <returns>日本時間</returns>
        public static DateTime Utc2JstTime(DateTime utcTime)
        {
            // タイムゾーンを指定してUTCからJSTへ変換
            if (utcTime.Kind == DateTimeKind.Utc)
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
    }
}