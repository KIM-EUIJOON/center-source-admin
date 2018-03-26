using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;
using CsvHelper;
using CsvHelper.Configuration;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// 運用監視コントローラクラス
    /// </summary>
    public class OperationMonitoringController : Controller
    {

        private const string FILE_NAME_OPERATION = "{0}運用レポート{1}.csv";
        private string FILE_CONTENTTYPE = "text/csv";

        /// <summary>
        /// 運用監視画面
        /// </summary>
        /// <returns></returns>
        [SessionCheck(WindowName = "運用監視画面")]
        public ActionResult Index()
        {
            // 画面表示時に事前設定は不要
            // 運用レポート出力の年月を事前設定/前回指定値を設定する場合はこちらで。

            return View();
        }

        /// <summary>
        /// 運用監視：運用レポート出力
        /// </summary>
        /// <param name="model">運用レポート出力リクエスト</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [SessionCheck(WindowName = "運用監視画面")]
        [HttpPost]
        public ActionResult PutMoniteringReport(OperationMonitoringModel model)
        {
            // 未設定時は無視する
            if (string.IsNullOrEmpty(model.Year) || string.IsNullOrEmpty(model.Month) ||
                !DateTime.TryParse(model.Year + "/" + model.Month, CultureInfo.CreateSpecificCulture("ja-JP"), DateTimeStyles.None, out DateTime requestTime))
            {
                TempData["message"] = "運用レポート出力の年月指定が正しくありません。";

                return View("Index");
            }

            TempData["year"] = HttpUtility.HtmlEncode(model.Year);
            TempData["month"] = HttpUtility.HtmlEncode(model.Month);
            
            // 運用レポート出力の実行
            AzureStorageIO azureStorage = new AzureStorageIO();
            // TODO:暫定処置としてパフォーマンスカウンターはCPU使用率のみとし、直近1日のデータを対象とする
            List<OperationMonitoringEntity> table = azureStorage.GetPerformanceCounterTable();

            var buffer = GetCsvDownloadStream(table, new OperationMonitoringtClassMap());

            if (buffer == null)
            {
                TempData["message"] = "該当データがありません。";
                return View("Index");
            }

            // 結果
            TempData["message"] = "運用レポート出力が完了しました。";

            string fileName = string.Format(FILE_NAME_OPERATION, Common.GetNowTimestamp().ToString("yyyyMMddHHmmss"), requestTime.ToString("yyyyMM"));
            return File(buffer, FILE_CONTENTTYPE, fileName);
        }

        /// <summary>
        /// ダウンロードデータ取得
        /// </summary>
        /// <param name="list"></param>
        /// <param name="map"></param>
        /// <returns>ダウンロードデータ</returns>
        private byte[] GetCsvDownloadStream<T, TMap>(List<T> list, TMap map) where TMap : ClassMap
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.GetEncoding("Shift_JIS")))
                using (var csvWriter = new CsvWriter(streamWriter))
                {
                    csvWriter.Configuration.HasHeaderRecord = true;
                    csvWriter.Configuration.QuoteAllFields = true;
                    csvWriter.Configuration.RegisterClassMap<TMap>();
                    csvWriter.WriteRecords(list);
                    streamWriter.Flush();
                }

                return memoryStream.ToArray();
            }
        }
    }
}