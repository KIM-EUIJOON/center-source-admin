using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.ResponseData;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppSigmaAdmin.Controllers
{
    public class InquiryController : Controller
    {

        public ActionResult Index()
        {
            ViewData["message"] = "";
            return View();
        }

        private const string SESSION_SEARCH_Inquiry = "SESSION_SEARCH_Inquiry";

        /// <summary>
        /// ID検索処理
        /// </summary>
        /// <returns>予約・決済情報確認</returns>
        [HttpPost]
        [SessionCheck(WindowName = "ユーザ情報画面")]
        public ActionResult Index(InquiryInfo model)
        {
            ViewData["message"] = "";
            int InquiryNo = 0;
            string InputNo = model.InquiryNo.ToString();
            string CheckInput = InputNo.Trim();

            try
            {
                InquiryNo = int.Parse(CheckInput);
            }
            catch
            {
                ModelState.AddModelError("", "問い合わせ番号に数字以外が入力されました。半角数字のみで再入力してください。");
                return View(model);
            }
            /*新しい入力値をテキストボックスに反映させるため、model内の値を削除する*/
            ModelState.Clear();
            int UserIdNo = 0;
            UserIdNo = InquiryNo / 9; //仮置きで9掛けされているため9で割る(仮置きのため、エラー判定は設定していません。)
            model.UserId = UserIdNo.ToString();

            return View(model);
        }
    }
}