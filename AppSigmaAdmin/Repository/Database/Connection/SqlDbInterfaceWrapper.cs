using AppSigmaAdmin.Repository.Database.Connection.AbstractLayer;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Repository.Database.Connection
{
    /// <summary>
    /// SqlDbInterfaceラッパークラス
    /// </summary>
    public class SqlDbInterfaceWrapper : ISqlDbInterface
    {

        /// <summary>
        /// インスタンス生成関数
        /// </summary>
        internal static Func<ISqlDbInterface> _genSqlDbInterface = GenSqlDbInterface;

        /// <summary>
        /// SqlDbInterface生成
        /// </summary>
        /// <returns></returns>
        public static ISqlDbInterface Create()
        {
            return _genSqlDbInterface();
        }

        /// <summary>
        /// インスタンス生成(運用)
        /// </summary>
        /// <returns></returns>
        private static ISqlDbInterface GenSqlDbInterface()
        {
            return new SqlDbInterfaceWrapper(new SqlDbInterface());
        }

        private SqlDbInterface dbInterface;

        private SqlDbInterfaceWrapper(SqlDbInterface dbInterface)
        {
            this.dbInterface = dbInterface;
        }

        public int ExecuteQuery(SqlCommand sqlCommand, int timeOut = -1) => dbInterface.ExecuteQuery(sqlCommand, timeOut);

        public DataTable ExecuteReader(SqlCommand sqlCommand, int timeOut = -1) => dbInterface.ExecuteReader(sqlCommand, timeOut);

        public void Dispose() => ((IDisposable)dbInterface).Dispose();

        public void BeginTransaction(IsolationLevel level) => dbInterface.BeginTransaction(level);

        public void CommitTransaction() => dbInterface.CommitTransaction();

        public void RollbackTransaction() => dbInterface.RollbackTransaction();
    }
}