using AppSigmaAdmin.Contstants.AbstractLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Contstants
{
    /// <summary>
    /// 枚数種別<para>
    /// dbo.FreeTicketManage.AdultNum(大人)、ChildNum(子供)、DiscountNum(学割)を示す</para>
    /// </summary>
    public class TicketNumType : AbstractConstants<TicketNumType>
    {
        public static TicketNumType Adult = new TicketNumType("大人", "A");
        public static TicketNumType Child = new TicketNumType("小児", "C");
        public static TicketNumType Discount = new TicketNumType("学割", "D");

        private TicketNumType(string name, string value) : base(name, value) { }
    }
}