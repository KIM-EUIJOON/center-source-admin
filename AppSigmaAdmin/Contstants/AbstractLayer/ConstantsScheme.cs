using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AppSigmaAdmin.Contstants.AbstractLayer
{
    public abstract class ConstantsScheme<TConstant, TValue>
        where TConstant : class
    {
        protected ConstantsScheme(string name, TValue value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 定義値
        /// </summary>
        public TValue Value { get; private set; }

        public static IEnumerable<TConstant> All => typeof(TConstant)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null) as TConstant)
            .Where(f => f != default(TConstant)); // 変換できなかったものは除外
    }
}