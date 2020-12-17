using AppSigmaAdmin.Contstants.AbstractLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Contstants.Extensions
{
    /// <summary>
    /// 定数クラス拡張メソッド
    /// </summary>
    public static class AbstractConstantsExtension
    {
        /// <summary>
        /// 定義値連結
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="separator"></param>
        /// <param name="parse"></param>
        /// <returns></returns>
        public static string JoinDefines<T>(this IEnumerable<AbstractConstants<T>> self, string separator, Func<AbstractConstants<T>, string> parse)
            where T : class
            => string.Join(separator, self.Select(c => parse(c)).ToArray());
    }
}