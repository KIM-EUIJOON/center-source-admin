using AppSigmaAdmin.Repository.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Toyamachitetsu.Constants
{
    public static class CompanyDefine
    {
        public static readonly Company[] ToyamachitetsuCompanies = new[]
        {
            new Company("TCT", "22", "電車"),
            new Company("TCT", "22", "バス"),
        };
    }
}