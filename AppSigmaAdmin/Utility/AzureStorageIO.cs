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
            string connetctionString = ApplicationConfig.StorageConnectionString;

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
        /// ディレクトリ配下の全てのブロブ名を取得
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="directoryName">ディレクトリ名</param>
        /// <returns>ディクショナリー（キー：ファイルパス、値：ファイル名）</returns>
        public Dictionary<string, string> GetBlobNameDictionary(string containerName, string directoryName)
        {
            Dictionary<string, string> dicFileName = new Dictionary<string, string>();
            // コンテナを取得
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            // ディレクトリを取得
            CloudBlobDirectory directory = container.GetDirectoryReference(directoryName);

            BlobContinuationToken token = null;

            // ディレクトリ配下の全てのブロブを取得
            foreach (IListBlobItem item in directory.ListBlobsSegmentedAsync(true, BlobListingDetails.Metadata, null, token, null, null).Result.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    dicFileName.Add(blob.Name, blob.Uri.Segments[blob.Uri.Segments.Length - 1].ToString());
                }
            }
            return dicFileName;
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
            //if (!blockBlob.Exists())
            if (!blockBlob.ExistsAsync().Result)
            {
                memoryStream = null;
            }

            //blockBlob.DownloadToStream(memoryStream);
            blockBlob.DownloadToStreamAsync(memoryStream).Wait();
        }

        /// <summary>
        /// StorageTableよりパフォーマンスカウンターデータを取得
        /// </summary>
        /// <returns>パフォーマンスカウンター</returns>
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

        /// <summary>
        /// インフォメーションログ情報を取得
        /// </summary>
        /// <param name="start">開始日時</param>
        /// <param name="end">終了日時</param>
        /// <param name="userId">ユーザID</param>
        /// <param name="messageList">メッセージリスト</param>
        /// <returns>インフォメーションログ情報</returns>
        public List<UserLogInfoEntity> GetInformationLogTable(DateTime start, DateTime end, string userId, List<string> messageList)
        {
            CloudTable table = tableClient.GetTableReference(SystemConst.STORAGE_TABLE_NAME_INFORMATION_LOG);

            string query = TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, start.ToString("yyyy-MM-dd HH:mm:ss")),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, end.ToString("yyyy-MM-dd HH:mm:ss")));

            if (!string.IsNullOrEmpty(userId))
            {
                query = TableQuery.CombineFilters(
                            query,
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId));
            }

            string queryMessage = string.Empty;
            foreach (string message in messageList)
            {
                if (string.IsNullOrEmpty(queryMessage))
                {
                    queryMessage = TableQuery.GenerateFilterCondition("Message", QueryComparisons.Equal, message);
                }
                else
                {
                    queryMessage = TableQuery.CombineFilters(
                                queryMessage,
                                TableOperators.Or,
                                TableQuery.GenerateFilterCondition("Message", QueryComparisons.Equal, message));
                }
            }

            if (!string.IsNullOrEmpty(queryMessage))
            {
                query = TableQuery.CombineFilters(
                            query,
                            TableOperators.And,
                            queryMessage);
            }

            TableQuery<InformationLogEntity> tableQuery = new TableQuery<InformationLogEntity>().Where(query);
            var result = table.ExecuteQuery(tableQuery);

            return result.Select(_ => new UserLogInfoEntity
            {
                Timestamp = _.Timestamp.DateTime.AddHours(9),   // TimestampはUTC→JSTに変換
                PartitionKey = DateTime.Parse(_.PartitionKey),
                UserId = _.UserId,
                Level = _.Level,
                Message = _.Message,
                InputParams = _.InputParams,
            }).ToList();
        }

        /// <summary>
        /// デバッグログ情報を取得
        /// </summary>
        /// <param name="start">開始日時</param>
        /// <param name="end">終了日時</param>
        /// <param name="userId">ユーザID</param>
        /// <param name="messageList">メッセージリスト</param>
        /// <returns>デバッグログ情報</returns>
        public List<UserLogInfoEntity> GetDebugLogTable(DateTime start, DateTime end, string userId, List<string> messageList)
        {
            CloudTable table = tableClient.GetTableReference(SystemConst.STORAGE_TABLE_NAME_DEBUG_LOG);

            string query = TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, start.ToString("yyyy-MM-dd HH:mm:ss")),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, end.ToString("yyyy-MM-dd HH:mm:ss")));

            if (!string.IsNullOrEmpty(userId))
            {
                query = TableQuery.CombineFilters(
                            query,
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId));
            }

            string queryMessage = string.Empty;
            foreach (string message in messageList)
            {
                if (string.IsNullOrEmpty(queryMessage))
                {
                    queryMessage = TableQuery.GenerateFilterCondition("Message", QueryComparisons.Equal, message);
                }
                else
                {
                    queryMessage = TableQuery.CombineFilters(
                                queryMessage,
                                TableOperators.Or,
                                TableQuery.GenerateFilterCondition("Message", QueryComparisons.Equal, message));
                }
            }

            if (!string.IsNullOrEmpty(queryMessage))
            {
                query = TableQuery.CombineFilters(
                            query,
                            TableOperators.And,
                            queryMessage);
            }

            TableQuery<DebugLogEntity> tableQuery = new TableQuery<DebugLogEntity>().Where(query);
            var result = table.ExecuteQuery(tableQuery);

            return result.Select(_ => new UserLogInfoEntity
            {
                Timestamp = _.Timestamp.DateTime.AddHours(9),   // TimestampはUTC→JSTに変換
                PartitionKey = DateTime.Parse(_.PartitionKey),
                UserId = _.UserId,
                Level = _.Level,
                TaskId = _.TaskId,
                Message = _.Message,
            }).ToList();
        }

        /// <summary>
        /// 端末インフォメーションログ情報を取得
        /// </summary>
        /// <param name="start">開始日時</param>
        /// <param name="end">終了日時</param>
        /// <param name="userId">ユーザID</param>
        /// <param name="messageList">メッセージリスト</param>
        /// <param name="infoTypeName">情報種別名</param>
        /// <param name="mobileId">端末ID</param>
        /// <returns>端末インフォメーションログ情報</returns>
        public List<UserLogInfoEntity> GetMobileInformationLogTable(DateTime start, DateTime end, string userId, List<string> messageList, string infoTypeName, string mobileId)
        {
            CloudTable table = tableClient.GetTableReference(SystemConst.STORAGE_TABLE_NAME_MOBILE_INFORMATION_LOG);

            string query = TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, start.ToString("yyyy-MM-dd HH:mm:ss")),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, end.ToString("yyyy-MM-dd HH:mm:ss")));

            if (!string.IsNullOrEmpty(userId))
            {
                query = TableQuery.CombineFilters(
                            query,
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId));
            }

            string queryMessage = string.Empty;
            foreach (string message in messageList)
            {
                if (string.IsNullOrEmpty(queryMessage))
                {
                    queryMessage = TableQuery.GenerateFilterCondition("Message", QueryComparisons.Equal, message);
                }
                else
                {
                    queryMessage = TableQuery.CombineFilters(
                                queryMessage,
                                TableOperators.Or,
                                TableQuery.GenerateFilterCondition("Message", QueryComparisons.Equal, message));
                }
            }

            if (!string.IsNullOrEmpty(queryMessage))
            {
                query = TableQuery.CombineFilters(
                            query,
                            TableOperators.And,
                            queryMessage);
            }

            if (!string.IsNullOrEmpty(infoTypeName))
            {
                query = TableQuery.CombineFilters(
                            query,
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("InfoTypeName", QueryComparisons.Equal, infoTypeName));
            }

            if (!string.IsNullOrEmpty(mobileId))
            {
                query = TableQuery.CombineFilters(
                            query,
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("MobileId", QueryComparisons.Equal, mobileId));
            }

            TableQuery<MobileInformationLogEntity> tableQuery = new TableQuery<MobileInformationLogEntity>().Where(query);
            var result = table.ExecuteQuery(tableQuery);

            return result.Select(_ => new UserLogInfoEntity
            {
                Timestamp = _.MobileTimestamp,
                PartitionKey = DateTime.Parse(_.PartitionKey),
                InfoTypeName = _.InfoTypeName,
                RequestUserId = _.RequestUserId,
                RequestMobileId = _.RequestMobileId,
                Level = _.Level,
                Message = _.Message,
                UserId = _.UserId,
                MobileId = _.MobileId,
                MobileName = _.MobileName,
                OsName = _.OsName,
                OsVersion = _.OsVersion,
                GooglePlayServicesVersion = _.GooglePlayServicesVersion,
                Language = _.Language,
                LocationInformation = _.LocationInformation,
            }).ToList();
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
