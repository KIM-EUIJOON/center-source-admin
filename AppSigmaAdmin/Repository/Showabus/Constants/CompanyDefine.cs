using AppSigmaAdmin.Repository.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Showabus.Constants
{
    public static class CompanyDefine
    {
        public static readonly Company[] ShowabusCompanies = new[]
        {
            new Company("SWB", "20", "バス"),
        };
    }
}