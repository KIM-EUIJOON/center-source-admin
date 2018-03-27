﻿using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;


namespace AppSigmaAdmin.Library
{
    /// <summary>
    /// アプリケーション設定情報クラス
    /// </summary>
    public class ApplicationConfig
    {
        /// <summary>
        /// データベース接続文字列
        /// </summary>
        private const string DB_CONNECTION_STRING = "DatabaseConnectionString";

        /// <summary>
        /// ストレージ接続文字列
        /// </summary>
        private const string STORAGE_CONNECTION_STRING = "StorageConnectionString";

        /// <summary>
        /// ストレージ接続文字列(パフォーマンスカウンター)
        /// </summary>
        private const string STORAGE_CONNECTION_STRING2 = "StorageConnectionString2";

        /// <summary>
        /// AppSigmaAPIサーバURI
        /// </summary>
        private const string SIGMA_API_SERVER_URI = "SigmaAPIServerURI";

        /// <summary>
        /// GMOサイト管理URI
        /// </summary>
        private const string GMO_SITE_MANAGE_URI = "GMOSiteManageURI";

        /// <summary>
        /// GMOショップ管理URI
        /// </summary>
        private const string GMO_SHOP_MANAGE_URI = "GMOShopManageURI";

        /// <summary>
        /// AzureモニターURI
        /// </summary>
        private const string AZURE_MONITOR_URI = "AzureMonitorURI";


        #region パブリックメソッド

        /// <summary>
        /// データベース接続文字列
        /// </summary>
        public static string DbConnectionString
        {
            get
            {
                return GetDbConectionString(DB_CONNECTION_STRING);
            }
        }

        /// <summary>
        /// ストレージ接続文字列
        /// </summary>
        public static string StorageConnectionString
        {
            get
            {
                return GetStorageString(STORAGE_CONNECTION_STRING);
            }
        }

        /// <summary>
        /// ストレージ接続文字列
        /// </summary>
        public static string StorageConnectionString2
        {
            get
            {
                return GetStorageString(STORAGE_CONNECTION_STRING2);
            }
        }

        /// <summary>
        /// AppSigmaAPIサーバURI
        /// </summary>
        public static string SigmaAPIServerURI
        {
            get
            {
                return GetString(SIGMA_API_SERVER_URI);
            }
        }

        /// <summary>
        /// GMOサイト管理URI
        /// </summary>
        public static string GMOSiteManageURI
        {
            get
            {
                return GetString(GMO_SITE_MANAGE_URI);
            }
        }

        /// <summary>
        /// GMOショップ管理URI
        /// </summary>
        public static string GMOShopManageURI
        {
            get
            {
                return GetString(GMO_SHOP_MANAGE_URI);
            }
        }

        /// <summary>
        /// AzureモニターURI
        /// </summary>
        public static string AzureMonitorURI
        {
            get
            {
                return GetString(AZURE_MONITOR_URI);
            }
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// Webコンフィグから指定値を取得する
        /// </summary>
        private static string GetString(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key]; 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Webコンフィグから指定値を取得する
        /// </summary>
        private static string GetDbConectionString(string key)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[key].ConnectionString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Webコンフィグから指定値を取得する
        /// </summary>
        private static string GetStorageString(string key)
        {
            try
            {
                return CloudConfigurationManager.GetSetting(key);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
