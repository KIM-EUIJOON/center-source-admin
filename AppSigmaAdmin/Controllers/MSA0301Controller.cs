using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Attribute;
using static AppSigmaAdmin.Models.AuthIpAddressModel;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;
using System.Diagnostics;


namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// システム管理者機能コントローラクラス
    /// </summary>
    public class MSA0301Controller : Controller
    {
       private const string SESSION_AUTH_ADDRESS_LIST = "Session_AuthAddressList";

        /// <summary>
        /// システム管理者機能画面
        /// </summary>
        /// <returns>システム管理者機能画面</returns>
        [SessionCheck(WindowName = "システム管理者機能画面")]
        public ActionResult Index()
        {
#if DEBUG
            ViewBag.Debug = 1;
#else
            ViewBag.Debug = 0;
#endif
            ViewData["message"] = "";

            return View();
        }

        /// <summary>
        /// IPアドレスリスト更新処理
        /// </summary>
        /// <returns>システム管理者機能画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "管理者機能画面")]
        public ActionResult IpRefresh(object sender,EventArgs e)
        {
#if DEBUG
            ViewBag.Debug = 1;
#else
            ViewBag.Debug = 0;
#endif
            ViewData["message"] = "";

            List<AuthIpAddressEntity> authIpAddressEntities;
            //セッションを更新する
            try
            {
                authIpAddressEntities = new AuthIpAddressModel().GetAuthIpAddress();
                HttpContext.Application[name: SESSION_AUTH_ADDRESS_LIST] = authIpAddressEntities;
                //正常に更新が完了した場合は1を設定する
                ViewBag.IpUpdate = 1;
                return View("~/Views/MSA0301/Index.cshtml");
            }
            catch(Exception error)
            {
                //エラーが発生した場合は2を設定する
                ViewBag.IpUpdate = 2;
                Trace.TraceError(Logger.GetExceptionMessage(error));
                return View("~/Views/MSA0301/Index.cshtml");
            }
             
            
        }

    }
}