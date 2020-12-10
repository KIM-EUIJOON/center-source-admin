using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSigmaAdmin.Repository.Database.Connection.AbstractLayer
{
    /// <summary>
    /// SqlDbInterfaceメソッド定義インタフェース
    /// </summary>
    public interface ISqlDbInterface : IDisposable
    {
        /// <summary>
        /// 操作コマンド
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        int ExecuteQuery(SqlCommand sqlCommand, int timeOut = -1);

        /// <summary>
        /// 抽出コマンド
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        DataTable ExecuteReader(SqlCommand sqlCommand, int timeOut = -1);

        /// <summary>
        /// トランザクション開始
        /// </summary>
        /// <param name="level">分離レベル</param>
        void BeginTransaction(IsolationLevel level);

        /// <summary>
        /// コミット
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// ロールバック
        /// </summary>
        void RollbackTransaction();
    }
}
