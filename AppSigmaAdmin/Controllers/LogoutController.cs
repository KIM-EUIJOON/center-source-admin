using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Models;
using Newtonsoft.Json;
using System.Configuration;
using AppSigmaAdmin.Utility;
using AppSigmaAdmin.Library;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// ログアウトコントローラクラス
    /// </summary>
    public class LogoutController : Controller
    {
        /// <summary>
        /// ログアウト画面
        /// </summary>
        /// <returns>ログアウト画面</returns>
        public ActionResult Index()
        {
            HttpContext.Session.Abandon();
            HttpContext.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

            return Redirect(Common.CreateUrl("/"));
        }
    }
}