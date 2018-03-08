using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AppSigmaAdmin
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// アプリケーションエラー発生イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e)
        {
            // 直前に発生しエラーを取得
            Exception exception = Server.GetLastError();

            // ログにエラー内容を出力
            LogEventSource.Log.Error(Logger.GetExceptionMessage(exception));

            // サーバエラー情報のクリア
            Server.ClearError();

            // メッセージ及びスタックトレースを取得し、セッションへ格納
            Application["ErrorStack"] = "";
            Application["ErrorStack"] = Logger.GetExceptionMessage(exception);

            // エラー表示画面へリダイレクト
            Response.Redirect(Common.CreateUrl("/Error"));
        }

        /// <summary>
        /// クライアント送信直前に発生するイベント
        /// </summary>
        protected void Application_PreSendRequestHeaders()
        {
            // キャッシュ無効化
            Response.AppendHeader("Pragma", "no-cache");
            Response.AppendHeader("Cache-Control", "no-cache, no-store");
            Response.AppendHeader("Expires", "Mon, 31-Dec-1979 00:00:00 GMT");
            // クリックジャッキング対応
            Response.AppendHeader("X-FRAME-OPTIONS", "SAMEORIGIN");//DENY
        }

        /// <summary>
        /// 要求応答の最後に呼ばれるイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (Response.Cookies.Count > 0)
            {
                foreach (string s in Response.Cookies.AllKeys)
                {
                    // session_idに対してセキュア属性を付与
                    if (s.ToLower().Equals("asp.net_sessionid"))
                    {
                        Response.Cookies[s].Secure = true;
                    }
                }
            }
        }
    }
}
