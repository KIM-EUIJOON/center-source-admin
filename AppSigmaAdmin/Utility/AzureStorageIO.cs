using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;

namespace AppSigmaAdmin.Utility
{
    /// <summary>
    /// AzureStorageIOクラス
    /// </summary>
    public class AzureStorageIO
    {
        private CloudBlobClient blobClient;
        private CloudTableClient tableClient;

        /// <summary>パフォーマンスカウンターテーブル名</summary>
        private const string STORAGE_TABLE_NAME = "WADPerformanceCountersTable";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AzureStorageIO()
        {
            string connetctionString = ApplicationConfig.StorageConnectionString2;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connetctionString);
            blobClient = storageAccount.CreateCloudBlobClient();
            tableClient = storageAccount.CreateCloudTableClient();
        }

        /// <summary>
        /// AzureStorageにファイルを格納する
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">ブロブ名</param>
        /// <returns>成否</returns>
        public bool FileUploadToAzureStorage(Stream stream, string containerName, string blobName)
        {
            try
            {
                // コンテナを取得
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                // コンテナが存在しなければ作成する
                container.CreateIfNotExists();
                // ブロックを取得
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
                // ストリームをアップロード
                blockBlob.UploadFromStream(stream);

                return true;
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error(Logger.GetExceptionMessage(ex));
                return false;
            }
        }

        /// <summary>
        /// AzureStorageのファイルを削除する
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">ブロブ名</param>
        public void FileDelete(string containerName, string blobName)
        {
            // コンテナを取得
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            if (container.Exists())
            {
                // ブロックを取得
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
                if (blockBlob.Exists())
                {
                    // Azureストレージに取り込んだファイルを削除
                    blockBlob.Delete();
                }
            }
        }

        /// <summary>
        /// ディレクトリ内の全てのブロブ名を取得
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="directoryName">ディレクトリ名</param>
        /// <returns>ファイルリスト</returns>
        public List<string> GetBlobName(string containerName, string directoryName)
        {
            List<string> fileNameList = new List<string>();
            // コンテナを取得
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            // ディレクトリを取得
            CloudBlobDirectory directory = container.GetDirectoryReference(directoryName);

            // ディレクトリ内の全てのブロブを取得
            foreach (IListBlobItem item in directory.ListBlobs(false, BlobListingDetails.Metadata, null, null))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    fileNameList.Add(blob.Uri.Segments[blob.Uri.Segments.Length - 1].ToString());
                }
            }
            return fileNameList;
        }

        /// <summary>
        /// StorageBlobからファイルデータを取得
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">ブロブ名</param>
        /// <param name="memoryStream">メモリストリーム</param>
        public void GetMemoryStream(string containerName, string blobName, ref MemoryStream memoryStream)
        {
            // コンテナを取得
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            // ブロックを取得
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // コンテナに対象ファイルが存在しない場合、処理中止
            if (!blockBlob.Exists())
            {
                memoryStream = null;
            }

            blockBlob.DownloadToStream(memoryStream);
        }

        /// <summary>
        /// StorageTableよりパフォーマンスカウンターデータを取得
        /// </summary>
        /// <returns></returns>
        public List<OperationMonitoringEntity> GetPerformanceCounterTable()
        {
            DateTime dateTime = Common.GetNowTimestamp().ToUniversalTime();
            CloudTable table = tableClient.GetTableReference(STORAGE_TABLE_NAME);
            
            var query = TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, dateTime.AddDays(-1)),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, dateTime));

            var query2 = TableQuery.CombineFilters(
                        query,
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("CounterName", QueryComparisons.Equal, @"\Processor(_Total)\% Processor Time"));

            TableQuery<PerformanceCounterEntity> tableQuery = new TableQuery<PerformanceCounterEntity>().Where(query2);
            var retsurt = table.ExecuteQuery(tableQuery);

            return retsurt.Select(_ => new OperationMonitoringEntity { Timestamp= _.Timestamp.DateTime , RoleInstance= _.RoleInstance, CounterName= _.CounterName, CounterValue= _.CounterValue }).ToList();
        }
    }

    /// <summary>
    /// ログテーブルクラス
    /// </summary>
    public class PerformanceCounterEntity : TableEntity
    {
        /// <summary>
        /// ログ出力メソッド
        /// </summary>
        /// <param name="procTimestamp">タイムスタンプ</param>
        /// <param name="role">ロールインスタンス</param>
        /// <param name="counterName">パフォーマンスカウンタ名</param>
        /// <param name="counterValue">パフォーマンスカウンタ値</param>
        public PerformanceCounterEntity(string role, string counterName, double counterValue)
        {
            this.RoleInstance = role;
            this.CounterName = counterName;
            this.CounterValue = counterValue;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PerformanceCounterEntity() { }

        /// <summary>
        /// ロールインスタンス(スケールセットのノードインスタンス)
        /// </summary>
        public string RoleInstance { get; set; }

        /// <summary>
        /// パフォーマンスカウンタ名
        /// </summary>
        public string CounterName { get; set; }

        /// <summary>
        /// パフォーマンスカウンタ値
        /// </summary>
        public double CounterValue { get; set; }
    }
}