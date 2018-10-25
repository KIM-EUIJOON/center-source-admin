using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// ユーザ情報コントローラクラス
    /// </summary>
    public class UserInfoController : Controller
    {
        /// <summary>
        /// ユーザ情報画面
        /// </summary>
        /// <returns>ログイン画面</returns>
        public ActionResult Index()
        {
            ViewData["message"] = "";
            return View();
        }

        /// <summary>
        /// 内部ID検索処理
        /// </summary>
        /// <returns>ユーザ情報画面</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionCheck(WindowName = "ユーザ情報画面")]
        public ActionResult Index(UserInfoModel model)
        {
            ViewData["message"] = "";

            if (string.IsNullOrEmpty(model.MailAddress))
            {
                // メールアドレス未入力
                ModelState.AddModelError("", "メールアドレスを入力してください。");
                return View(model);
            }

            string response = new UserInfo().GetUserInternalId(Crypto.Encryption(ApplicationConfig.ApplicationID, model.MailAddress));
            if (response != null)
            {
                ViewData["message"] = "内部ID：" + response;
            }
            else
            {
                ModelState.AddModelError("", "一致するアドレスがありませんでした。");
            }

            return View(model);
        }
    }
}