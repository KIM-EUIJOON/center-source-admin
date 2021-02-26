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
    /// 富山地方鉄道利用実績データ
    /// </summary>
    public class ToyamachitetsuTicketUsageRepository : TicketUsageRepositoryBase<ToyamachitetsuUsageInsuranceInfo>
    {
        public ToyamachitetsuTicketUsageRepository()
            : base(CompanyDefine.ToyamachitetsuCompanies)
        {

        }

        /// <summary>
        /// DBデータ変換
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        protected override ToyamachitetsuUsageInsuranceInfo Parse(TicketUsageQueryRaw raw)
            => new ToyamachitetsuUsageInsuranceInfo()
            {
                UserId = raw.UserId.ToString(),
                TicketName = raw.TicketName,
                UsageStartDatetime = Option(raw.UsageStartDatetime, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                UsageEndDatetime = Option(raw.UsageEndDatetime, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                InquiryId = raw.InquiryId,
            };

    }
}