using AppSigmaAdmin.Library;
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
        /// <summary>
        /// 画面名
        /// </summary>
        public string WindowName { set; get; }

        /// <summary>
        /// セッション確認
        /// </summary>
        /// <param name="filterContext">コンテキスト</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session == null ||
                (filterContext.HttpContext.Session[SystemConst.SESSION_SIGMA_TOKEN] == null))
            {
                Logger.TraceInfo(Common.GetNowTimestamp(), null, "セッションタイムアウト", null);
                filterContext.HttpContext.Session.Abandon();
                filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

                // ログイン画面へリダイレクト
                filterContext.Result = new RedirectResult(Common.CreateUrl("/"));

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