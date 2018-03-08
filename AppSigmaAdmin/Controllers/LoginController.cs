using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.ResponseData;
using Newtonsoft.Json;
using System.Configuration;
using AppSigmaAdmin.Utility;
using AppSigmaAdmin.Library;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// ログインコントローラクラス
    /// </summary>
    public class LoginController : Controller
    {
        /// <summary>
        /// ログイン画面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            HttpContext.Session.Abandon();
            HttpContext.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

            return View();
        }

        /// <summary>
        /// ログイン処理
        /// </summary>
        /// <param name="model">ログインリクエスト</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginModel model)
        {
            if (model.MailAddress == null || model.Password == null)
            {
                // ID、パスワード未入力
                ModelState.AddModelError("", "アカウント、パスワードを入力してください。");
                return View(model);
            }

            // JSONへ変換
            string requestJson = JsonConvert.SerializeObject(model);
            string server = ApplicationConfig.SigmaAPIServerURI;

            // HTTP通信
            SendHttpRequest serv = new SendHttpRequest(server);
            // TODO:バージョンチェックAPIではアプリのみのため、暫定でアプリバージョンをセット
            serv.DicHttpHeader.Add("aplVersion", "MyRoute:A:0.0.2");

            string responseJson = serv.HttpCon("Api/Login", requestJson);

            if (!string.IsNullOrEmpty(responseJson) && serv.HttpResponseStatusCode == "OK")
            {
                //クラスへ変換
                LoginResponseEntity response = JsonConvert.DeserializeObject<LoginResponseEntity>(responseJson);

                if (response.ProcCode == ApiResultConst.ACCOUNT_LOCKED)
                {
                    ModelState.AddModelError("", response.ProcMessage);
                    return View(model);
                }
                else if (response.ProcCode != ApiResultConst.SUCCESS)
                {
                    ModelState.AddModelError("", "アカウントもしくはパスワードが間違っています。");
                    return View(model);
                }

                UserInfoModel userInfoModel = new UserInfoModel();
                UserInfoEntity userInfo = userInfoModel.GetUserInfoModel(response.UserId);

                // ロールIDが管理者か
                if (userInfo.RoleId != SystemConst.ROLE_ID_ADMIN)
                {
                    ModelState.AddModelError("", "管理者のみログイン可能です。");
                }
                else
                {
                    Logger.TraceInfo(Common.GetNowTimestamp(), response.UserId, "管理者画面ログイン成功", null);
                    ViewBag.ErrorMessage = "";
                    HttpContext.Session.Add(SystemConst.SESSION_SIGMA_TOKEN, response.Token);
                    HttpContext.Session.Add(SystemConst.SESSION_USER_INFO, userInfo);

                    return Redirect(Common.CreateUrl("/Menu"));
                }
            }
            // ログインAPIからユーザ情報を取得できなかった
            else if(string.IsNullOrEmpty(responseJson))
            {
                ModelState.AddModelError("", "AppSigmaが停止しているためログインできません。");
            }

            Logger.TraceInfo(Common.GetNowTimestamp(), null, "管理者画面ログイン失敗", null);
            return View(model);
        }

    }
}