using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// エラーコントローラクラス
    /// </summary>
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
#if DEBUG
            ViewBag.ErrorStack = HttpContext.Application["ErrorStack"];
            HttpContext.Application["ErrorStack"] = "";
#endif
            return View();
        }
    }
}