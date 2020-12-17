using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AppSigmaAdmin.Contstants.AbstractLayer
{
    public abstract class AbstractConstants<T>
        where T : class
    {
        protected AbstractConstants(string name, string value)
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
        public string Value { get; private set; }

        public static IEnumerable<T> All => typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null) as T)
            .Where(f => f != default(T)); // 変換できなかったものは除外
    }
}