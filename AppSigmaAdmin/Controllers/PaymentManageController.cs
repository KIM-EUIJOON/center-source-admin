using System;
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
        /// <returns></returns>
        [SessionCheck(WindowName = "決済管理画面")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 決済データダウンロード処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [SessionCheck(WindowName = "決済管理画面")]
        [HttpPost]
        public ActionResult Download(PaymentManageModel model)
        {
            DateTime fromDate;
            DateTime toDate;

            TempData["fromYear"] = model.FromYear;
            TempData["fromMonth"] = model.FromMonth;
            TempData["fromDay"] = model.FromDay;
            TempData["toYear"] = model.ToYear;
            TempData["toMonth"] = model.ToMonth;
            TempData["toDay"] = model.ToDay;
            TempData["paymentOver"] = model.PaymentOver;

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
        /// <param name="list"></param>
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