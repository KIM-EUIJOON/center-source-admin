using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;
using IpMatcher;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using static AppSigmaAdmin.Models.AuthIpAddressModel;

namespace AppSigmaAdmin
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private const string SESSION_AUTH_ADDRESS_LIST = "Session_AuthAddressList"; 

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            List<AuthIpAddressEntity> authIpAddressEntities;

            string connectedIpAddress = Request.Headers["x-forwarded-for"];
            string requestUri = ((HttpApplication)sender).Request.AppRelativeCurrentExecutionFilePath;
            string httpMethod = ((HttpApplication)sender).Request.HttpMethod;

            // HTTPヘッダのヘッダ接続元アドレスが未設定の場合、Request.UserHostAddressを取得
            if (connectedIpAddress == null)
            {
                connectedIpAddress = ((HttpApplication)sender).Request.UserHostAddress;
                if (connectedIpAddress == "::1") connectedIpAddress = "127.0.0.1";
            }
            
            // トップページでの初回呼出し時
            if (requestUri == "~/" && httpMethod == "GET")
            {
                authIpAddressEntities = new AuthIpAddressModel().GetAuthIpAddress();
                Application[name: SESSION_AUTH_ADDRESS_LIST] = authIpAddressEntities;
            }
            else
            {
                authIpAddressEntities = (List<AuthIpAddressEntity>)Application[name: SESSION_AUTH_ADDRESS_LIST];
            }

            Matcher matcher = new Matcher();
            foreach(AuthIpAddressEntity netInfo in authIpAddressEntities)
            {
                matcher.Add(netInfo.IPAddress, netInfo.SubnetAddress);
            }

            if (!matcher.MatchExists(connectedIpAddress))
            {
                Response.StatusCode = 403;
                Response.Flush();
                Response.End();
            }
        }
    }
}
