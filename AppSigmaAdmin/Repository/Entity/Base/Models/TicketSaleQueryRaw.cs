using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Entity.Base.Models
{
    public class TicketSaleQueryRaw
    {
        public string TicketId { get; set; }
        public string BizCompanyCd { get; set; }
        public string TicketType { get; set; }
        public string TicketGroup { get; set; }
        public string TicketName { get; set; }
    }
}