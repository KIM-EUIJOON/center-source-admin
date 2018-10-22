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
    /// ユーザ情報コントローラクラス
    /// </summary>
    public class UserInfoController : Controller
    {
        /// <summary>
        /// ユーザ情報画面
        /// </summary>
        /// <returns>ユーザ情報画面</returns>
        [SessionCheck(WindowName = "ユーザ情報画面")]
        public ActionResult Index()
        {
            return View();
        }
    }
}