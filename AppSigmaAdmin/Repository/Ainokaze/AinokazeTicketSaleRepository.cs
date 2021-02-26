using AppSigmaAdmin.Models;
using AppSigmaAdmin.Repository.Database.Entity.Base;
using AppSigmaAdmin.Repository.Entity.Base.Models;
using AppSigmaAdmin.Repository.Entity.Model;
using AppSigmaAdmin.Repository.Ainokaze.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Ainokaze
{
    /// <summary>
    /// あいの風とやま販売チケットデータ
    /// </summary>
    public class AinokazeTicketSaleRepository : TicketSaleRespositoryBase<AinokazePaymentInfo>
    {
        public AinokazeTicketSaleRepository(params Company[] companies)
            : base(CompanyDefine.AinokazeCompanies)
        {

        }

        protected override AinokazePaymentInfo Parse(TicketSaleQueryRaw raw)
        {
            var info = new AinokazePaymentInfo()
            {
                TicketId = raw.TicketId,
                TransportType = raw.BizCompanyCd,
                TicketType = raw.TicketType,
                TicketName = raw.TicketName,
            };

            /*if (raw.TicketGroup == "1")
                info.TicketName += "[au]";*/

            return info;
        }
    }
}