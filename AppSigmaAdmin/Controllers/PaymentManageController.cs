/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;
using AppSigmaAdmin.Attribute;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// 決済管理コントローラクラス
    /// </summary>
    public class PaymentManageController : Controller
    {
        private const string FILE_NAME_PAYMENT = "{0}決済データ{1}～{2}.csv";
        private const string FILE_NAME_PAYMENT_OVER_PERIOD = "{0}未決済データ{1}～{2}.csv";
        private string FILE_CONTENTTYPE = "text/csv";

        /// <summary>
        /// 決済管理画面
        /// </summary>
        /// <returns>決済管理画面</returns>
        [SessionCheck(WindowName = "決済管理画面")]
        public ActionResult Index()
        {
            // URL直叩きされた場合を考慮して、、エラーを表示する
            throw new HttpException(404, "Not Found");

            //return View();
        }

        /// <summary>
        /// 決済データダウンロード処理
        /// </summary>
        /// <param name="model">決済管理出力リクエスト</param>
        /// <returns>決済管理画面</returns>
        [ValidateAntiForgeryToken]
        [SessionCheck(WindowName = "決済管理画面")]
        [HttpPost]
        public ActionResult Download(PaymentManageModel model)
        {
            DateTime fromDate;
            DateTime toDate;

            if (string.IsNullOrEmpty(model.FromYear) ||
                string.IsNullOrEmpty(model.FromMonth) ||
                string.IsNullOrEmpty(model.FromDay) ||
                string.IsNullOrEmpty(model.ToYear) ||
                string.IsNullOrEmpty(model.ToMonth) ||
                string.IsNullOrEmpty(model.ToDay))
            {
                return View("Index");
            }
            else if (!DateTime.TryParse(model.FromYear + "-" + model.FromMonth + "-" + model.FromDay, out fromDate))
            {
                ModelState.AddModelError("", "検索開始日年月日指定が正しくありません。");
                return View("Index");
            }
            else if (!DateTime.TryParse(model.ToYear + "-" + model.ToMonth + "-" + model.ToDay, out toDate))
            {
                ModelState.AddModelError("", "検索終了日年月日指定が正しくありません。");
                return View("Index");
            }
            else if(fromDate > toDate)
            {
                ModelState.AddModelError("", "検索開始日＜検索終了日となるよう入力してください。");
                return View("Index");
            }

            TempData["fromYear"] = HttpUtility.HtmlEncode(model.FromYear);
            TempData["fromMonth"] = HttpUtility.HtmlEncode(model.FromMonth);
            TempData["fromDay"] = HttpUtility.HtmlEncode(model.FromDay);
            TempData["toYear"] = HttpUtility.HtmlEncode(model.ToYear);
            TempData["toMonth"] = HttpUtility.HtmlEncode(model.ToMonth);
            TempData["toDay"] = HttpUtility.HtmlEncode(model.ToDay);
            TempData["paymentOver"] = HttpUtility.HtmlEncode(model.PaymentOver);

            string datetime = Common.GetNowTimestamp().ToString("yyyyMMddHHmmss");
            string fileName = "";
            byte[] buffer = null;

            if (model.PaymentOver == "on")
            {
                fileName = string.Format(FILE_NAME_PAYMENT_OVER_PERIOD, datetime, fromDate.ToString("yyyyMMdd"), toDate.ToString("yyyyMMdd"));

                List<PaymentOverPeriodEntity> payment = new List<PaymentOverPeriodEntity>();
                payment = new PaymentOverPeriodModel().GetPaymentOverPeriodModel(fromDate, toDate);

                if (payment.Count > 0)
                {
                    buffer = GetCsvDownloadStream(payment, new PaymentOverPeriodClassMap());
                }
            }
            else
            {
                fileName = string.Format(FILE_NAME_PAYMENT, datetime, fromDate.ToString("yyyyMMdd"), toDate.ToString("yyyyMMdd"));

                List<PaymentEntity> payment = new List<PaymentEntity>();
                payment = new PaymentModel().GetPaymentModel(fromDate, toDate);

                if (payment.Count > 0)
                {
                    buffer = GetCsvDownloadStream(payment, new PaymentClassMap());
                }
            }

            if(buffer == null)
            {
                ModelState.AddModelError("", "該当データがありません。");
                return View("Index");
            }

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
}*/