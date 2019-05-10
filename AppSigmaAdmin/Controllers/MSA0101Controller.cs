using System;
using System.Collections.Generic;
using System.Data;
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
    public class MSA0101Controller : Controller
    {
        /// <summary>
        /// ユーザ情報画面
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "ユーザ情報画面")]
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
            UserInfoAdminEntity userInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            Logger.TraceInfo(Common.GetNowTimestamp(), userInfo.AdminId, "ユーザ情報画面-ID紐づけ実施", null);

            if (string.IsNullOrEmpty(model.MailAddress))
            {
                // メールアドレス未入力
                ModelState.AddModelError("", "メールアドレスを入力してください。");
                return View(model);
            }

            List<UserIdInfoRespons> response = null;
            response = new UserInfo().GetUserInternalId(Crypto.Encryption(ApplicationConfig.ApplicationID, model.MailAddress));
            ResonsID info = new ResonsID();

            if (response.Count > 0)
            {
                //入力メールアドレス
                info.MailAddress = model.MailAddress;
                //検索結果
                info.UserIdInfoList = response;
            }
            else
            {
                ModelState.AddModelError("", "一致するアドレスがありませんでした。");
            }
            return View(info);
        }
    }
 
}