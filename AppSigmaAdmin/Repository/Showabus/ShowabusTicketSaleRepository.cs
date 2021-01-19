using AppSigmaAdmin.Models;
using AppSigmaAdmin.Repository.Database.Entity.Base;
using AppSigmaAdmin.Repository.Entity.Base.Models;
using AppSigmaAdmin.Repository.Entity.Model;
using AppSigmaAdmin.Repository.Showabus.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Showabus
{
    /// <summary>
    /// 昭和バス販売チケットデータ
    /// </summary>
    public class ShowabusTicketSaleRepository : TicketSaleRespositoryBase<ShowabusPaymentInfo>
    {
        public ShowabusTicketSaleRepository(params Company[] companies)
            : base(CompanyDefine.ShowabusCompanies)
        {

        }

        protected override ShowabusPaymentInfo Parse(TicketSaleQueryRaw raw)
        {
            var info = new ShowabusPaymentInfo()
            {
                TicketId = raw.TicketId,
                TransportType = raw.BizCompanyCd,
                TicketType = raw.TicketType,
                TicketName = raw.TicketName,
            };

            if (raw.TicketGroup == "1")
                info.TicketName += "[au]";

            return info;
        }
    }
}