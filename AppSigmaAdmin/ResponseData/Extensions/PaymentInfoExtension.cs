using AppSigmaAdmin.Contstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.ResponseData.Extensions
{
    /// <summary>
    /// 決済情報拡張メソッド
    /// </summary>
    public static class PaymentInfoExtension
    {
        /// <summary>
        /// 決済名取得
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string GetPaymentName(this PaymentInfo self)
            => self.PaymentMeansCode == "1" ? self.PaymentMeansName  // "1"(GMO)の場合、決済手段名
             : self.PaymentMeansCode == "2" ? self.PaymentDetailName // "2"(TW)の場合、決済詳細名
             : null;

        /// <summary>
        /// 仕向先名取得
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string GetForwardName(this PaymentInfo self)
            => string.IsNullOrEmpty(self.ForwardCode) ? null // 空は出さない
             : ForwardCode.All.FirstOrDefault(c => c.Value == self.ForwardCode)?.Name ?? "その他"; // 定義がないものは、"その他"を返す
    }
}