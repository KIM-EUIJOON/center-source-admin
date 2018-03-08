
namespace AppSigmaAdmin.Library
{
    /// <summary>
    /// WebAPIの処理結果定数クラス
    /// </summary>
    public class ApiResultConst
    {
        /**************/
        /* 結果コード */
        /**************/
        /// <summary>成功</summary>
        public const string SUCCESS = "000";

        /// <summary>失敗</summary>
        public const string ERROR = "001";

        /// <summary>致命的エラー</summary>
        public const string FAIL = "999";

        /// <summary>パラメータ不正</summary>
        public const string INVALID_PARAM = "101";

        /// <summary>未認証（または、認証切れ）</summary>
        public const string AUTH_TIMEOUT = "109";

        /// <summary>アプリバージョンエラー（バージョンアップが必要）</summary>
        public const string VERSION_INCOMPLETE = "900";

        /// <summary>アカウントロック中</summary>
        public const string ACCOUNT_LOCKED = "901";
    }
}
