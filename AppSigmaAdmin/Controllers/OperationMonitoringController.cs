using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// 運用監視コントローラクラス
    /// </summary>
    public class OperationMonitoringController : Controller
    {
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


            // 結果
            TempData["message"] = "運用レポート出力が完了しました。";

            return View("Index");
        }

    }
}