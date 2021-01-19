using AppSigmaAdmin.Repository.Database.Entity.Base;
using AppSigmaAdmin.Repository.Entity.Base.Models;
using AppSigmaAdmin.Repository.Entity.Model;
using AppSigmaAdmin.Repository.Nishitetsu.Constants;
using AppSigmaAdmin.ResponseData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Nishitetsu
{
    /// <summary>
    /// 西鉄販売チケットデータ
    /// </summary>
    public class NishitetsuTicketSaleRepository : TicketSaleRespositoryBase<NishitetsuPaymentInfo>
    {
        public NishitetsuTicketSaleRepository(params Company[] companies)
            : base(CompanyDefine.NishitetsuCompanies)
        {

        }

        protected override NishitetsuPaymentInfo Parse(TicketSaleQueryRaw raw)
        {
            var info = new NishitetsuPaymentInfo()
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