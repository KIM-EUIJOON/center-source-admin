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
            Response.Redirect("~/Error/Index");
        }
    }
}
