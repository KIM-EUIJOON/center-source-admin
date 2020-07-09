using Microsoft.WindowsAzure.Storage.Table;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// デバッグログテーブルクラス
    /// </summary>
    public class DebugLogEntity : TableEntity
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DebugLogEntity() { }

        /// <summary>
        /// レベル
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// タスクID
        /// </summary>
        public string TaskId { get; set; }
    }
}
