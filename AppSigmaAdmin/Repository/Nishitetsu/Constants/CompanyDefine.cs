using AppSigmaAdmin.Repository.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Nishitetsu.Constants
{
    public class CompanyDefine
    {
        public static readonly Company[] NishitetsuCompanies = new[]
        {
            new Company("NNR", "4", "鉄道"),
            new Company("NIS", "2", "バス(福岡)"),
            new Company("NISK", "5", "バス(北九州)"),
            new Company("NISG", "6", "マルチ"),
            new Company("NKUM", "5", "鉄道(北九州モノレール)"),
            new Company("NKB", "5", "西鉄バス北九州"),
            new Company("NKCER", "5", "鉄道(北九州)"),
            new Company("NKKK", "5", "船"),
        };
    }
}