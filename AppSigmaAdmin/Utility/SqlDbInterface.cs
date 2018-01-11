using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using AppSigmaAdmin.Library;

namespace AppSigmaAdmin.Utility
{
    /// <summary>
    /// DBアクセスクラス
    /// </summary>
    public class SqlDbInterface : IDisposable
    {
        /// <summary>
        /// SQLコネクション
        /// </summary>
        private SqlConnection _con = null;

        /// <summary>
        /// トランザクション・オブジェクト
        /// </summary>
        /// <remarks></remarks>
        private SqlTransaction _trn = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SqlDbInterface()
        {
            connect(ApplicationConfig.DbConnectionString, -1);
        }

        /// <summary>
        /// DB接続
        /// </summary>
        /// <param name="connectionString">DB接続文字列</param>
        /// <param name="timeOut">タイムアウト値</param>
        private void connect(String connectionString, int timeOut = -1)
        {
            try
            {
                if (_con == null)
                {
                    _con = new SqlConnection();
                }
                _con.ConnectionString = connectionString +
                        (timeOut > -1 ? ";Connect Timeout=" + timeOut.ToString() : "");
                _con.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Connect Error", ex);
            }
        }

        /// <summary>
        /// DB切断
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_con != null && _con.State != ConnectionState.Closed)
                {
                    _con.Close();
                    _con = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disconnect Error", ex);
            }
        }

        /// <summary>
        /// SQL(Select)の実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="timeOut">タイムアウト値</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable ExecuteReader(String sql, int timeOut = -1)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_con.State != ConnectionState.Open)
                {
                    _con.Open();
                }

                using (SqlCommand sqlCommand = new SqlCommand(sql, _con))
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                {
                    if (timeOut > -1)
                    {
                        sqlCommand.CommandTimeout = timeOut;
                    }

                    adapter.Fill(dt);
                    adapter.Dispose();
                    sqlCommand.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteReader Error", ex);
            }

            return dt;
        }

        /// <summary>
        /// SQL(Select)の実行
        /// </summary>
        /// <param name="sqlCommand">SQLコマンド</param>
        /// <param name="timeOut">タイムアウト値</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable ExecuteReader(SqlCommand sqlCommand, int timeOut = -1)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_con.State != ConnectionState.Open)
                {
                    _con.Open();
                }

                sqlCommand.Connection = _con;

                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                {
                    if (timeOut > -1)
                    {
                        sqlCommand.CommandTimeout = timeOut;
                    }

                    adapter.Fill(dt);
                    adapter.Dispose();
                    sqlCommand.Dispose();
                }

            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteReader Error", ex);
            }

            return dt;
        }

        /// <summary>
        /// SQL(INSERT,UPDATE,DELETE)の実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="timeOut">タイムアウト値</param>
        /// <returns>影響を受けた行の数</returns>
        public int ExecuteQuery(String sql, int timeOut = -1)
        {
            int numRows = 0;

            try
            {
                if (_con.State != ConnectionState.Open)
                {
                    _con.Open();
                }

                SqlCommand sqlCommand = new SqlCommand(sql, _con, _trn);

                if (timeOut > -1)
                {
                    sqlCommand.CommandTimeout = timeOut;
                }

                numRows = sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteQuery Error", ex);
            }

            return numRows;
        }

        /// <summary>
        /// SQL(INSERT,UPDATE,DELETE)の実行
        /// </summary>
        /// <param name="sqlCommand">SQLコマンド</param>
        /// <param name="timeOut">タイムアウト値</param>
        /// <returns>影響を受けた行の数</returns>
        public int ExecuteQuery(SqlCommand sqlCommand, int timeOut = -1)
        {
            int numRows = 0;

            try
            {
                if (_con.State != ConnectionState.Open)
                {
                    _con.Open();
                }

                sqlCommand.Connection = _con;
                sqlCommand.Transaction = _trn;

                if (timeOut > -1)
                {
                    sqlCommand.CommandTimeout = timeOut;
                }

                numRows = sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteQuery Error", ex);
            }

            return numRows;
        }

        /// <summary>
        /// SQL(INSERT,UPDATE,DELETE)の実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="timeOut">タイムアウト値</param>
        /// <returns>結果セットの最初の行の最初の列</returns>
        public object ExecuteScalar(String sql, int timeOut = -1)
        {
            object ret = null;

            try
            {
                if (_con.State != ConnectionState.Open)
                {
                    _con.Open();
                }

                SqlCommand _sqlCommand = new SqlCommand(sql, _con, _trn);

                if (timeOut > -1)
                {
                    _sqlCommand.CommandTimeout = timeOut;
                }

                ret = _sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteScalar Error", ex);
            }

            return ret;
        }

        /// <summary>
        /// トランザクション開始
        /// </summary>
        /// <remarks></remarks>
        public void BeginTransaction()
        {
            try
            {
                _trn = _con.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception("BeginTransaction Error", ex);
            }
        }

        /// <summary>
        /// トランザクション開始
        /// </summary>
        /// <param name="level">分離レベル</param>
        /// <remarks></remarks>
        public void BeginTransaction(IsolationLevel level)
        {
            try
            {
                _trn = _con.BeginTransaction(level);
            }
            catch (Exception ex)
            {
                throw new Exception("BeginTransaction Error", ex);
            }
        }

        /// <summary>
        /// コミット
        /// </summary>
        /// <remarks></remarks>
        public void CommitTransaction()
        {
            try
            {
                if (_trn != null)
                {
                    _trn.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CommitTransaction Error", ex);
            }
            finally
            {
                _trn = null;
            }
        }

        /// <summary>
        /// ロールバック
        /// </summary>
        /// <remarks></remarks>
        public void RollbackTransaction()
        {
            try
            {
                if (_trn != null)
                {
                    _trn.Rollback();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("RollbackTransaction Error", ex);
            }
            finally
            {
                _trn = null;
            }
        }

        void IDisposable.Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        /// <remarks></remarks>
        ~SqlDbInterface()
        {
            Disconnect();
        }

        /// <summary>
        /// DBNull変換
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object ConvertDBNull(object obj)
        {
            if (obj == System.DBNull.Value)
            {
                return null;
            }
            return obj;
        }

        /// <summary>
        /// DBNull変換(ゼロにする)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object ConvertDBNullToZero(object obj)
        {
            if (obj == System.DBNull.Value)
            {
                return 0;
            }
            return obj;
        }
    }
}