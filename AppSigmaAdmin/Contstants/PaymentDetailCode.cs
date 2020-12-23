using AppSigmaAdmin.Contstants.AbstractLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Contstants
{
    /// <summary>
    /// 決済手段詳細コード<para>
    /// dbo.PaymentManage.PaymentDetailCodeの値</para>
    /// </summary>
    public class PaymentDetailCode : ConstantsScheme<PaymentDetailCode, (PaymentMeansCode meansCode, string detailCode)>
    {
        // 決済手段:TWの詳細コード
        public static PaymentDetailCode TWCharge = new PaymentDetailCode("TW残高", (PaymentMeansCode.TW, "00"));
        public static PaymentDetailCode TSPay = new PaymentDetailCode("TS Pay", (PaymentMeansCode.TW, "02"));

        private PaymentDetailCode(string name, (PaymentMeansCode, string) value) : base(name, value) { }
    }
}