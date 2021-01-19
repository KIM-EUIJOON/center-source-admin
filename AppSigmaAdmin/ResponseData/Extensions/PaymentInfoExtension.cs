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
            // GMO決済かどうかを判定する。
            if (IsGMO(self))
                return PaymentMeansCode.GMO.Name;

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
             : !IsGMO(self) ? null // 決済手段がGMO以外は出さない
             : ForwardCode.All.FirstOrDefault(c => c.Value == self.ForwardCode)?.Name ?? "その他"; // 定義がないものは、"その他"を返す

        /// <summary>
        /// GMO決済かどうかを返します。<para>
        /// センター仕様より、クレジット決済の払戻時、手数料決済データに決済手段を設定していない(=null)
        /// これを補完するため、当該データをGMO決済として扱うようにする
        /// また、決済手段は必須項目であるため、補完を優先し決済種別(=払戻)は条件に含めないことにした
        /// </para>
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        private static bool IsGMO(PaymentInfo self)
        {
            return self.PaymentMeansCode == null // nullもGMOとして扱う
                || self.PaymentMeansCode == PaymentMeansCode.GMO.Value;
        }

    }

}