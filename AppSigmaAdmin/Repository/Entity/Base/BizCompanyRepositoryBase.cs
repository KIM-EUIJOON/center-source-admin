using AppSigmaAdmin.Repository.Database.Query.AbstractLayer;
using AppSigmaAdmin.Repository.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Database.Entity.Base
{
    /// <summary>
    /// 事業体サービス状況基底クラス
    /// </summary>
    public abstract class BizCompanyRepositoryBase : AbstractExecuteSQLBase
    {
        /// <summary>
        /// 事業体定義リスト
        /// </summary>
        protected readonly IEnumerable<Company> _Companies;

        protected BizCompanyRepositoryBase(IEnumerable<Company> companies)
        {
            if (!companies.Any())
                throw new ArgumentException("事業体を1つ以上、定義してください。");

            _Companies = companies;
        }

        /// <summary>
        /// 事業体コード(カンマ区切り)
        /// </summary>
        /// <returns></returns>
        protected string BizCompanyCodes
            => string.Join(", ", _Companies.Select(c => $"'{c.Code}'").ToArray());

        /// <summary>
        /// サービスID(カンマ区切り)
        /// </summary>
        /// <returns></returns>
        protected string ServiceIds
            => string.Join(", ", _Companies.Where(c => !string.IsNullOrEmpty(c.ServiceId)) // 定義されているものだけ
                                           .Select(c => c.ServiceId)
                                           .GroupBy(c => c).Select(g => g.Key) // 重複排除
                                           .Select(i => $"'{i}'"));
    }
}