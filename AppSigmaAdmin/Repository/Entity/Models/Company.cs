using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Entity.Model
{
    /// <summary>
    /// 事業体
    /// </summary>
    public class Company
    {
        public Company(string code, string serviceId, string transportName)
        {
            Code = code;
            ServiceId = serviceId;
            TransportName = transportName;
        }

        /// <summary>
        /// コード
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// サービスID
        /// </summary>
        public string ServiceId { get; private set; }

        /// <summary>
        /// 交通種別名
        /// </summary>
        public string TransportName { get; private set; }
    }
}