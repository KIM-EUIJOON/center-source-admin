using AppSigmaAdmin.Library;
using System.Web;
using System.Web.Optimization;

namespace AppSigmaAdmin
{
    public class BundleConfig
    {
        // バンドルの詳細については、https://go.microsoft.com/fwlink/?LinkId=301862 を参照してください
        public static void RegisterBundles(BundleCollection bundles)
        {
            // JavaScript: JQuery関連
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                        "~/Scripts/jquery-ui.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // JavaScript: Modernizr関連
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // JavaScript: アプリケーション共通
            bundles.Add(new ScriptBundle("~/bundles/jquery/common").Include(
                        "~/Scripts/Pages/common.js"));

            //JavaScript: システム管理者機能画面(MSA0301)
            bundles.Add(new ScriptBundle("~/bundles/jquery/MSA0301").Include(
                        "~/Scripts/Pages/MSA0301.js"));

<<<<<<< HEAD
            // StyleSheet: JQueryUI関連
            bundles.Add(new StyleBundle("~/Content/jquery-ui/css").Include(
                        "~/Content/jquery-ui.css"));

            // StyleSheet: Application固有
            bundles.Add(new StyleBundle("~/Content/app-site/css").Include(
                        "~/Content/Site.css"));
=======
            if (ApplicationConfig.DeployEnv == ApplicationConfig.ENV_DEBUG)
            {
                bundles.Add(new StyleBundle("~/sigma/Content/css").Include(
                          "~/Content/bootstrap.css",
                          "~/Content/jquery-ui.css",
                          "~/Content/site.css"));
            }
            else
            {
                bundles.Add(new StyleBundle("~/sigma/Content/css").Include(
                          "~/sigma/Content/bootstrap.css",
                          "~/sigma/Content/jquery-ui.css",
                          "~/sigma/Content/site.css"));
            }
            //JavaScript: システム管理者機能画面(MSA0301)
            bundles.Add(new ScriptBundle("~/bundles/jquery/MSA0301").Include(
                        "~/Scripts/Pages/MSA0301.js"));

>>>>>>> remotes/origin/Feature/#1795_西鉄鉄道フリー乗車券の売上状況確認を追加
        }
    }
}
