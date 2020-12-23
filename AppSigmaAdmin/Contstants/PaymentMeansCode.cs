using AppSigmaAdmin.Contstants.AbstractLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Contstants
{
    /// <summary>
    /// 決済手段コード<para>
    /// dbo.PaymentManage.PaymentMeansCodeの値</para>
    /// </summary>
    public class PaymentMeansCode : AbstractConstants<PaymentMeansCode>
    {
        public static PaymentMeansCode GMO = new PaymentMeansCode("クレジットカード", "1");
        public static PaymentMeansCode TW = new PaymentMeansCode("TW", "2");

        private PaymentMeansCode(string name, string value) : base(name, value) { }
    }
}