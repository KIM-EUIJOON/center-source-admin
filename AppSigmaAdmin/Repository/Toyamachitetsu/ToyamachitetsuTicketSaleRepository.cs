using AppSigmaAdmin.Models;
using AppSigmaAdmin.Repository.Database.Entity.Base;
using AppSigmaAdmin.Repository.Entity.Base.Models;
using AppSigmaAdmin.Repository.Entity.Model;
using AppSigmaAdmin.Repository.Toyamachitetsu.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Toyamachitetsu
{
    /// <summary>
    /// 富山地方鉄道販売チケットデータ
    /// </summary>
    public class ToyamachitetsuTicketSaleRepository : TicketSaleRespositoryBase<ToyamachitetsuPaymentInfo>
    {
        public ToyamachitetsuTicketSaleRepository(params Company[] companies)
            : base(CompanyDefine.ToyamachitetsuCompanies)
        {

        }

        protected override ToyamachitetsuPaymentInfo Parse(TicketSaleQueryRaw raw)
        {
            var info = new ToyamachitetsuPaymentInfo()
            {
                TicketId = raw.TicketId,
                TransportType = raw.BizCompanyCd,
                TicketType = raw.TicketType,
                TicketName = raw.TicketName,
            };
            //au版での販売はない為、不要
            /*if (raw.TicketGroup == "1")
                info.TicketName += "[au]";*/

            return info;
        }
    }
}