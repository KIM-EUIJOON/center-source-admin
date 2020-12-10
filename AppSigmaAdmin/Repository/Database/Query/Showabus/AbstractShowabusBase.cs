using AppSigmaAdmin.Repository.Database.Query.AbstractLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Database.Query.Showabus
{
    public abstract class AbstractShowabusBase : AbstractExecuteSQLBase
    {
        /// <summary>
        /// 事業体リスト(事業体コード, サービスID, 交通手段名)
        /// </summary>
        protected static readonly IEnumerable<(string Code, string ServiceId, string TransportName)> _Companies = new[]
        {
                ("SWB", "20", "バス"),
        };
    }
}