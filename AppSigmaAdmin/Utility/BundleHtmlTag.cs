using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Library
{
    /// <summary>
    /// 
    /// </summary>
    public class BundleHtmlTag
    {
        /// <summary>
        /// HTML Styleタグ
        /// </summary>
        public static string StyleTag
        {
            get
            {
                // 環境判別
                if (ApplicationConfig.DeployEnv == ApplicationConfig.ENV_DEBUG)
                {
                    // デバッグ環境
                    return @"<link href=""{0}"" rel=""stylesheet"" type=""text/css"" />";
                }
                else
                {
                    // 検証・号口環境
                    return @"<link href=""/sigma/{0}"" rel=""stylesheet"" type=""text/css"" />";
                }
            }
        }

        /// <summary>
        /// HTML Scriptタグ
        /// </summary>
        public static string ScriptTag
        {
            get
            {
                // 環境判別
                if (ApplicationConfig.DeployEnv == ApplicationConfig.ENV_DEBUG)
                {
                    // デバッグ環境
                    return @"<script src= ""{0}"" type=""text/javascript""></script>";
                }
                else
                {
                    // 検証・号口環境
                    return @"<script src= ""/sigma/{0}"" type=""text/javascript""></script>";
                }
            }
        }
    }
}