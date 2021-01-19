using AppSigmaAdmin.Contstants.AbstractLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AppSigmaAdmin.Contstants
{
    /// <summary>
    /// 決済区分<para>
    /// dbo.PaymentManage.PaymentTypeの値</para>
    /// </summary>
    public class PaymentType : AbstractConstants<PaymentType>
    {
        public static PaymentType Settle = new PaymentType("即時決済", "3");
        public static PaymentType Refund = new PaymentType("払戻し", "4");
        public static PaymentType Cancel = new PaymentType("取消", "5");

        public static PaymentType Unknown = new PaymentType("決済種別不明", "X");

        private PaymentType(string name, string value) : base(name, value) { }

        /// <summary>
        /// 定義値
        /// </summary>
        public static IEnumerable<PaymentType> Defines = new[] { Settle, Refund, Cancel };
    }
}