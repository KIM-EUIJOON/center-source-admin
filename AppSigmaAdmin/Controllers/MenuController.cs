using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Library;

namespace AppSigmaAdmin.Controllers
{
    public class MenuController : Controller
    {
        public ActionResult Index()
        {
            if (HttpContext.Session[SystemConst.SESSION_SIGMA_TOKEN] == null)
            {
                // セッションタイムアウト時はログイン画面に遷移
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}