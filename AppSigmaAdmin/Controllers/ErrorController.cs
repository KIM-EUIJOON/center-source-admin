﻿using System;
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
        /// <summary>
        /// エラー画面
        /// </summary>
        /// <returns>エラー画面</returns>
        public ActionResult Index()
        {
            ViewBag.ExFunc = HttpContext.Application["ExFunc"];
            HttpContext.Application["ExFunc"] = "";
#if DEBUG
            ViewBag.ErrorStack = HttpContext.Application["ErrorStack"];
            HttpContext.Application["ErrorStack"] = "";
#endif
            return View();
        }
    }
}