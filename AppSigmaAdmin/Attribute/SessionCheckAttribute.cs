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
                UserInfoAdminEntity userInfoAdmin = (UserInfoAdminEntity)filterContext.HttpContext.Session[SystemConst.SESSION_USER_INFO_ADMIN];

                if (filterContext.HttpContext.Session[SystemConst.SESSION_USER_INFO_ADMIN] != null)
                {
                    //URLの末尾を取得
                    var routeDate = filterContext.RouteData;
                    string path = routeDate.Values["controller"].ToString();

                    if (path != "Menu")
                    {
                        //Menu以外にアクセスしようとした場合に権限の有無を確認する
                        List<RoleFunction> roleInfo = null;
                        RoleList RoleInfoAdminEntity = null;
                        if (filterContext.HttpContext.Session[SystemConst.SESSION_ROLE_INFO_ADMIN] != null)
                        {
                            RoleInfoAdminEntity = (RoleList)filterContext.HttpContext.Session[SystemConst.SESSION_ROLE_INFO_ADMIN];
                            roleInfo = RoleInfoAdminEntity.RoleFunctionList;
                            //閲覧権限のある画面リストを取得する
                            List<string> UrlCheck = new List<string>();
                            foreach (var urlcheck in roleInfo)
                            {
                                string urlvalue = urlcheck.FuncId.ToString();
                                string Url = urlvalue.Trim();
                                UrlCheck.Add(Url);
                            }
                            if (UrlCheck.Contains(path) != true)
                            {
                                //入力されたURLを閲覧する権限がない場合はログイン画面にリダイレクトする
                                filterContext.Result = new RedirectResult(Common.CreateUrl("/Error"));
                                return;
                            }
                            Logger.TraceInfo(Common.GetNowTimestamp(), userInfoAdmin.AdminId, WindowName, null);
                        }
                    }
                    Logger.TraceInfo(Common.GetNowTimestamp(), userInfoAdmin.AdminId, WindowName, null);
                }
            }
        }
    }
}