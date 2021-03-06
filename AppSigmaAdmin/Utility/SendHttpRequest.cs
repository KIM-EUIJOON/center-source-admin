using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AppSigmaAdmin.Library;

namespace AppSigmaAdmin.Utility
{
    /// <summary>
    /// HTTP通信用クラス
    /// </summary>
    public class SendHttpRequest
    {
        #region 定数定義

        /// <summary>コンテントタイプ</summary>
        private const string CONTENT_TYPE = "application/json";
        /// <summary>メソッドタイプ</summary>
        private const string METHOD_TYPE = "POST";
        /// <summary>リファラー</summary>
        private const string REFERER = "";

        #endregion

        #region パブリック変数定義

        /// <summary>接続先サーバ</summary>
        public string Server;

        /// <summary>Httpレスポンスステータス</summary>
        public string HttpResponseStatusCode;

        /// <summary>Httpレスポンスメッセージ</summary>
        public string HttpResponseDescription;

        #endregion

        #region プロパティ定義
        /// <summary>
        /// AOuth認証使用有無
        /// </summary>
        public bool IsOAuth { get; set; }

        /// <summary>
        /// コンテントタイプ
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// メソッドタイプ
        /// </summary>
        public string MethodType { get; set; }

        /// <summary>
        /// リファラー
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// User-Agent
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Accept-Language
        /// </summary>
        public string AcceptLanguage { get; set; }

        /// <summary>
        /// Authorization
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Httpヘッダ
        /// </summary>
        /// <remarks>
        /// 独自に設定するHttpヘッダがある場合に設定する
        /// </remarks>
        public Dictionary<string, string> DicHttpHeader { get; set; }

        /// <summary>
        /// エラートレース出力有無
        /// </summary>
        public bool IsErrorTrace { get; set; }
        #endregion

        #region パブリックメソッド定義
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="connectServer">config用接続先文字列</param>
        public SendHttpRequest(string connectServer)
        {
            // 接続先サーバ
            this.Server = connectServer;
            // AOuth認証使用有無
            IsOAuth = false;
            // コンテントタイプ
            ContentType = CONTENT_TYPE;
            // メソッドタイプ
            MethodType = METHOD_TYPE;
            // リファラー
            Referer = REFERER;
            // エラートレース出力有無
            IsErrorTrace = true;

            DicHttpHeader = new Dictionary<string, string>();
        }

        /// <summary>
        /// Http通信を行い、Webサーバから返されたJSONを返す。
        /// サーバから返されるステータスコードは、クラス変数に登録する。
        /// エラー発生時はエラー情報をログに出力し空文字列を返す。
        /// </summary>
        /// <param name="methodName">実行メソッド名</param>
        /// <param name="postJson">リクエスト文字列(JSON)</param>
        /// <returns>レスポンス文字列(JSON)</returns>
        public string HttpCon(string methodName, string postJson)
        {
            string responseJson = "";         // レスポンスJSONの初期化
            HttpWebRequest request = null;
            // URL
            string requestUri = this.Server + methodName;

            try
            {
                if (MethodType == SystemConst.HTTP_METHOD_GET)
                {
                    requestUri += postJson;
                }
                request = (HttpWebRequest)WebRequest.Create(requestUri);

                // Httpリクエストヘッダを設定
                request.ContentType = ContentType;
                request.Method = MethodType;
                if (!string.IsNullOrEmpty(Authorization))
                {
                    request.Headers.Add(HttpRequestHeader.Authorization, Authorization);
                }
                if (Referer != "")
                {
                    request.Referer = Referer;
                }
                if (!string.IsNullOrEmpty(UserAgent))
                {
                    request.UserAgent = UserAgent;
                }
                if (!string.IsNullOrEmpty(AcceptLanguage))
                {
                    request.Headers.Add(HttpRequestHeader.AcceptLanguage, AcceptLanguage);
                }
                foreach (KeyValuePair<string, string> pair in DicHttpHeader)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }

                if (MethodType == SystemConst.HTTP_METHOD_POST)
                {
                    // リクエストデータを保持するストリームの取得
                    using (var requestStream = new StreamWriter(request.GetRequestStream()))
                    {
                        // リクエストストリームへ書き出す
                        requestStream.Write(postJson);
                        requestStream.Flush();
                    }
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // レスポンスストリームの取得
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    // レスポンスステータスコードをクラスクラス変数に入れる
                    HttpResponseStatusCode = response.StatusCode.ToString();
                    HttpResponseDescription = response.StatusDescription;
                    // レスポンスストリームを文字列にして返す
                    responseJson = sr.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                HttpResponseStatusCode = ApiResultConst.FAIL;
                string requestString = "\r\nリクエスト：" + postJson + Environment.NewLine + this.RequestString(request);

                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    using (var response = e.Response as HttpWebResponse)
                    {
                        // レスポンスステータスコードをクラス変数に入れる
                        HttpResponseStatusCode = response.StatusCode.ToString();
                        HttpResponseDescription = response.StatusDescription;

                        if (response.ContentType.IndexOf("text") >= 0 ||
                            response.ContentType.IndexOf("json") >= 0 ||
                            response.ContentType.IndexOf("xml") >= 0)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseJson = reader.ReadToEnd();
                                if (IsErrorTrace)
                                {
                                    LogEventSource.Log.Error(Logger.GetExceptionMessage(e) + requestString + "\r\nステータスコード：" + HttpResponseStatusCode + "\r\nレスポンス：" + responseJson);
                                }
                            }
                        }
                        else
                        {
                            if (IsErrorTrace)
                            {
                                LogEventSource.Log.Error(Logger.GetExceptionMessage(e) + requestString + "\r\nステータスコード：" + HttpResponseStatusCode);
                            }
                        }
                    }
                }
                else
                {
                    if (IsErrorTrace)
                    {
                        LogEventSource.Log.Error(Logger.GetExceptionMessage(e) + requestString);
                    }

                    switch (e.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            HttpResponseStatusCode = SystemConst.HTTP_STATUS_CODE_TIMEOUT;
                            break;
                        case WebExceptionStatus.ReceiveFailure:
                            HttpResponseStatusCode = SystemConst.HTTP_STATUS_CODE_RECEIVE_FAIL;
                            break;
                        case WebExceptionStatus.SendFailure:
                            HttpResponseStatusCode = SystemConst.HTTP_STATUS_CODE_SEND_FAIL;
                            break;
                        default:
                            throw;
                    }
                }
            }
            finally
            {
            }
            return responseJson;
        }

        #endregion

        private string RequestString(HttpWebRequest request)
        {
            StringBuilder sb = new StringBuilder();

            if (request != null)
            {
                if (request.Address != null)
                {
                    sb.AppendLine("URL:" + request.Address.AbsoluteUri);
                }
                if (!string.IsNullOrEmpty(request.Method))
                {
                    sb.AppendLine("Method:" + request.Method);
                }
                if (!string.IsNullOrEmpty(request.ContentType))
                {
                    sb.AppendLine("ContentType:" + request.ContentType);
                }
                if (!string.IsNullOrEmpty(request.UserAgent))
                {
                    sb.AppendLine("UserAgent:" + request.UserAgent);
                }
                if (!string.IsNullOrEmpty(request.Referer))
                {
                    sb.AppendLine("Referer:" + request.Referer);
                }
                sb.AppendLine("Timeout:" + request.Timeout);
                if (request.Headers != null)
                {
                    sb.AppendLine("Headers:" + request.Headers.ToString());
                }

            }
            return sb.ToString();
        }
    }
}