using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Database.Query.AbstractLayer
{
    /// <summary>
    /// SQL実行基底クラス
    /// </summary>
    public abstract class AbstractExecuteSQLBase
    {
        // パラメータ設定(任意必須項目)
        protected static Action<SqlCommand, string, SqlDbType, object> setParam = (cmd, name, type, value) =>
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.SqlDbType = type;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            cmd.Parameters.Add(param);
        };

        // パラメータ設定(必須bool -> bit)
        protected static Action<SqlCommand, string, bool> setParam4RqrBool = (cmd, name, value) =>
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.SqlDbType = SqlDbType.Bit;
            param.Direction = ParameterDirection.Input;
            param.Value = value ? 1 : 0;
            cmd.Parameters.Add(param);
        };
        // パラメータ設定(任意string -> nvarchar)
        protected static Action<SqlCommand, string, string> setParam4NulString = (cmd, name, value) =>
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.IsNullable = true;
            param.SqlDbType = SqlDbType.NVarChar;
            param.Direction = ParameterDirection.Input;
            if (value != null)
                param.Value = value;
            else
                param.Value = DBNull.Value;
            cmd.Parameters.Add(param);
        };
        // パラメータ設定(任意int -> int)
        protected static Action<SqlCommand, string, int?> setParam4NulInt32 = (cmd, name, value) =>
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.IsNullable = true;
            param.SqlDbType = SqlDbType.Int;
            param.Direction = ParameterDirection.Input;
            if (value.HasValue)
                param.Value = value.Value;
            else
                param.Value = DBNull.Value;
            cmd.Parameters.Add(param);
        };
        // パラメータ設定(任意float -> float)
        protected static Action<SqlCommand, string, double?> setParam4NulDouble = (cmd, name, value) =>
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.IsNullable = true;
            param.SqlDbType = SqlDbType.Float;
            param.Direction = ParameterDirection.Input;
            if (value.HasValue)
                param.Value = value.Value;
            else
                param.Value = DBNull.Value;
            cmd.Parameters.Add(param);
        };

        // パラメータ設定(任意DateTime -> datetime)
        protected static Action<SqlCommand, string, DateTime?> setParam4NulDateTime = (cmd, name, value) =>
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.IsNullable = true;
            param.SqlDbType = SqlDbType.DateTime;
            param.Direction = ParameterDirection.Input;
            if (value.HasValue)
                param.Value = value.Value;
            else
                param.Value = DBNull.Value;
            cmd.Parameters.Add(param);
        };

        // パラメータ設定(任意int -> int)
        protected static Action<SqlCommand, string, bool?> setParam4NulBool = (cmd, name, value) =>
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.IsNullable = true;
            param.SqlDbType = SqlDbType.Bit;
            param.Direction = ParameterDirection.Input;
            if (value.HasValue)
                param.Value = value.Value ? 1 : 0;
            else
                param.Value = DBNull.Value;
            cmd.Parameters.Add(param);
        };

        // 更新項目、パラメータ設定
        protected static Action<SqlCommand, IList<string>, string, Action<SqlCommand, string>> buildUpdateSqlWizParam = (cmd, lst, name, setParamF) =>
        {
            // SQLを組み立て
            lst.Add($"{name} = @{name}");

            // その変数に実値を設定する関数を呼ぶ(SQLコマンドと変数名を渡す)
            setParamF(cmd, $"@{name}");
        };

        /// <summary>
        /// DB値を属性型のNullableに変換します。DBNull.Valueの場合、nullを返します
        /// </summary>
        /// <typeparam name="T">データ型</typeparam>
        /// <param name="dbValue">DB値</param>
        /// <returns></returns>
        protected static T? Option<T>(object dbValue)
            where T : struct
        {
            return dbValue == DBNull.Value ? null : (T?)dbValue;
        }

        /// <summary>
        /// DB値を文字列(string)に変換します。DBNull.Valueの場合、nullを返します。
        /// </summary>
        /// <param name="dbValue">DB値</param>
        /// <returns></returns>
        protected static string OptionString(object dbValue)
        {
            return dbValue == DBNull.Value ? null : dbValue.ToString();
        }

        /// <summary>
        /// Nullableの値を取得します。nullの場合、指定した値を返します。
        /// </summary>
        /// <typeparam name="T">データ型</typeparam>
        /// <param name="value">元値(Nullable)</param>
        /// <param name="defValue">valueがnull(HasValue=false)の場合に返す値</param>
        /// <returns></returns>
        protected static T Default<T>(T? value, T defValue)
            where T : struct
        {
            return value.HasValue ? value.Value : defValue;
        }

        /// <summary>
        /// Nullable -> string 変換
        /// </summary>
        /// <typeparam name="T">データ型(struct)</typeparam>
        /// <param name="value">値</param>
        /// <returns></returns>
        protected static string Option<T>(T? value)
            where T : struct => Option(value, v => v);

        /// <summary>
        /// Nullable -> string 変換
        /// </summary>
        /// <typeparam name="T">データ型(struct)</typeparam>
        /// <param name="value">値</param>
        /// <param name="shaper">整形関数</param>
        /// <returns></returns>
        protected static string Option<T>(T? value, Func<T, T> shaper)
            where T : struct => Option(value, shaper, v => v.ToString());

        /// <summary>
        /// Nullable -> string 変換
        /// </summary>
        /// <typeparam name="T">データ型(struct)</typeparam>
        /// <param name="value">値</param>
        /// <param name="parser">型変換関数</param>
        /// <returns></returns>
        protected static string Option<T>(T? value, Func<T, string> parser)
            where T : struct => Option(value, v => v, parser);

        /// <summary>
        /// Nullable -> string 変換
        /// </summary>
        /// <typeparam name="T">データ型(struct)</typeparam>
        /// <param name="value">値</param>
        /// <param name="shaper">整形関数</param>
        /// <param name="parser">型変換関数</param>
        /// <returns></returns>
        protected static string Option<T>(T? value, Func<T, T> shaper, Func<T, string> parser)
            where T : struct => value.HasValue ? parser(shaper(value.Value)) : null;
    }
}