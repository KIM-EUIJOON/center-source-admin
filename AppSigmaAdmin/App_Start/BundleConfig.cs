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
            bundles.Add(new ScriptBundle("~/sigma/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/sigma/bundles/jquery-ui").Include(
                        "~/Scripts/jquery-ui.js"));
            bundles.Add(new ScriptBundle("~/sigma/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));



            // 開発と学習には、Modernizr の開発バージョンを使用します。次に、実稼働の準備が
            // 運用の準備が完了したら、https://modernizr.com のビルド ツールを使用し、必要なテストのみを選択します。
            bundles.Add(new ScriptBundle("~/sigma/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/sigma/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

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

        }
    }
}
