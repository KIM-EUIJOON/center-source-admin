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
    /// あいの風とやま利用実績データ
    /// </summary>
    public class AinokazeTicketUsageRepository : TicketUsageRepositoryBase<AinokazeUsageInsuranceInfo>
    {
        public AinokazeTicketUsageRepository()
            : base(CompanyDefine.AinokazeCompanies)
        {

        }

        /// <summary>
        /// DBデータ変換
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        protected override AinokazeUsageInsuranceInfo Parse(TicketUsageQueryRaw raw)
            => new AinokazeUsageInsuranceInfo()
            {
                UserId = raw.UserId.ToString(),
                TicketName = raw.TicketName,
                UsageStartDatetime = Option(raw.UsageStartDatetime, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                UsageEndDatetime = Option(raw.UsageEndDatetime, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                InquiryId = raw.InquiryId,
            };

    }
}