using Microsoft.WindowsAzure.Storage.Table;

namespace AppSigmaAdmin.Models
{
    /// <summary>
    /// インフォメーションログテーブルクラス
    /// </summary>
    public class InformationLogEntity : TableEntity
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InformationLogEntity() { }

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
        /// 入力パラメータ
        /// </summary>
        public string InputParams { get; set; }
    }
}
