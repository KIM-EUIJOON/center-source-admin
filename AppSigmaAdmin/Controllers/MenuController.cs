using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// メニューコントローラクラス
    /// </summary>
    public class MenuController : Controller
    {
        /// <summary>
        /// メニュー画面
        /// </summary>
        /// <returns></returns>
        [SessionCheck(WindowName ="メニュー画面")]
        public ActionResult Index()
        {
            return View();
        }
    }
}