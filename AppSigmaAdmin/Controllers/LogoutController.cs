﻿using System;
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
        /// ログアウト
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            HttpContext.Session.RemoveAll();
            return RedirectToAction("Index", "Login");
        }
    }
}