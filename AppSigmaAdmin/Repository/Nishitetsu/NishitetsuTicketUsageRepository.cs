using AppSigmaAdmin.Models;
using AppSigmaAdmin.Repository.Database.Entity.Base;
using AppSigmaAdmin.Repository.Entity.Base.Models;
using AppSigmaAdmin.Repository.Nishitetsu.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Nishitetsu
{
    /// <summary>
    /// 西鉄利用実績データ
    /// </summary>
    public class NishitetsuTicketUsageRepository : TicketUsageRepositoryBase<NishitetsuUsageInfo>
    {
        public NishitetsuTicketUsageRepository()
            : base(CompanyDefine.NishitetsuCompanies)
        {

        }

        /// <summary>
        /// DBデータ変換
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        protected override NishitetsuUsageInfo Parse(TicketUsageQueryRaw raw)
            => new NishitetsuUsageInfo()
            {
                UserId = raw.UserId.ToString(),
                TicketName = raw.TicketName,
                UsageStartDatetime = Option(raw.UsageStartDatetime, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                UsageEndDatetime = Option(raw.UsageEndDatetime, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                InquiryId = raw.InquiryId,
            };

    }
}