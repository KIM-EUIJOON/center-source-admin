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
        {
            if (PaymentMeansCode.All
                .FirstOrDefault(c => c.Value == self.PaymentMeansCode) is PaymentMeansCode meansCode)
            {
                if (PaymentDetailCode.All
                    .FirstOrDefault(c => c.Value.meansCode.Value == meansCode.Value
                                      && c.Value.detailCode == self.PaymentDetailCode) is PaymentDetailCode detailCode)
                    // 決済手段詳細が優先
                    return detailCode.Name;
                // 決済手段詳細がなければ、決済手段
                return meansCode.Name;
            }
            // 定義なしはnull
            return null;
        }

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