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
        /// <returns>ログイン画面</returns>
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
        /// <returns>ログイン画面</returns>
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
            serv.DicHttpHeader.Add("aplVersion", SystemConst.APL_VERSION);

            string responseJson = serv.HttpCon("Api/AdminLogin", requestJson);

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

                UserInfoAdminEntity userInfo = new UserInfoAdminEntity()
                {
                    AdminId = response.UserId,
                    EMailAddress = model.MailAddress,
                    Name = response.Name,
                    Role = response.RoleId
                };

                List<RoleFunction> RoleFuncList = null;
                //入力されたアドレスに関連する権限情報を取得する
                RoleFuncList = new LoginModel().GetRoleFunctions(response.RoleId);
                RoleList roleFunction = new RoleList();
                //取得リストが0件でない場合
                if (RoleFuncList.Count > 0) {
                    roleFunction.RoleFunctionList = RoleFuncList;
                }

                Logger.TraceInfo(Common.GetNowTimestamp(), response.UserId, "管理者画面ログイン成功", null);
                ViewBag.ErrorMessage = "";
                HttpContext.Session.Add(SystemConst.SESSION_SIGMA_TOKEN, response.Token);
                HttpContext.Session.Add(SystemConst.SESSION_USER_INFO_ADMIN, userInfo);
                //セッションに情報を追加する
                HttpContext.Session.Add(SystemConst.SESSION_ROLE_INFO_ADMIN, roleFunction);

                return Redirect(Common.CreateUrl("/Menu"));
            }
            // ログインAPIでタイムアウト
            else if (serv.HttpResponseStatusCode == SystemConst.HTTP_STATUS_CODE_TIMEOUT)
            {
                ModelState.AddModelError("", "センター通信中にタイムアウトが発生しました。");
            }
            // ログインAPIからユーザ情報を取得できなかった
            else if(string.IsNullOrEmpty(responseJson))
            {
                ModelState.AddModelError("", "センター通信に失敗しました。");
            }

            Logger.TraceInfo(Common.GetNowTimestamp(), null, "管理者画面ログイン失敗", null);
            return View(model);
        }

    }
}