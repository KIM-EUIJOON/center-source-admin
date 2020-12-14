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
    /// 昭和バス利用実績データ
    /// </summary>
    public class ShowabusTicketUsageRepository : TicketUsageRepositoryBase<ShowabusUsageInsuranceInfo>
    {
        public ShowabusTicketUsageRepository()
            : base(CompanyDefine.ShowabusCompanies)
        {

        }

        /// <summary>
        /// DBデータ変換
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        protected override ShowabusUsageInsuranceInfo Parse(TicketUsageQueryRaw raw)
            => new ShowabusUsageInsuranceInfo()
            {
                UserId = raw.UserId.ToString(),
                UsageStartDatetime = Option(raw.UsageStartDatetime, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                UsageEndDatetime = Option(raw.UsageEndDatetime, d => d.ToString("yyyy/MM/dd HH:mm:ss")),
                InquiryId = raw.InquiryId,
            };

    }
}