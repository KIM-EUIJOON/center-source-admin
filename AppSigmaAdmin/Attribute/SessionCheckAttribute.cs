﻿using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppSigmaAdmin.Attribute
{
    /// <summary>
    /// セッション確認クラス
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public string WindowName { set; get; }

        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session == null ||
                (filterContext.HttpContext.Session[SystemConst.SESSION_SIGMA_TOKEN] == null))
            {
                Logger.TraceInfo(Common.GetNowTimestamp(), null, "セッションタイムアウト", null);
                // ログイン画面へリダイレクト
                filterContext.Result = new RedirectResult("/Login");
                return;
            }
            else
            {
                UserInfoEntity userInfo = (UserInfoEntity)filterContext.HttpContext.Session[SystemConst.SESSION_USER_INFO];
                Logger.TraceInfo(Common.GetNowTimestamp(), userInfo.UserId, WindowName, null);
            }
        }
    }
}