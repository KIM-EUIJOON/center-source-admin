using AppSigmaAdmin.Contstants.AbstractLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Contstants
{
    /// <summary>
    /// 仕向先コード<para>
    /// dbo.PaymentManage.ForwardCodeの値</para>
    /// </summary>
    public class ForwardCode : AbstractConstants<ForwardCode>
    {
        public static ForwardCode TFS = new ForwardCode("TFS", "2s77334");
        public static ForwardCode JCB = new ForwardCode("JCB", "2a99661");

        private ForwardCode(string name, string value) : base(name, value) { }
    }
}