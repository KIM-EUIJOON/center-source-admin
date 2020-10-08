using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.ResponseData;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static AppSigmaAdmin.Models.JTXPaymentModel;

namespace AppSigmaAdmin.Controllers
{
    public class MPA0401Controller : Controller
    {

        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";

        List<string> header = new List<string>()
        {
            "myroute会員ID",
            "決済日時",
            "決済ID",
            "駐輪事業者名",
            "予約ID",
            "決済種別",
            "金額",
            "アプリ種別"
        };

        /// <summary>
        ///入力日付チェック関数
        /// </summary>
        public static bool IsDate(string s)
        {
            //入力された日時が年/月/日以外はエラーで返す
            string baseDatePaturn = "yyyy/M/d";

            try
            {
                //入力された日時がDateTime型に変換できるか確認することにより入力日付のチェックを行う
                DateTime.ParseExact(s, baseDatePaturn, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
            }
            catch
            {
                //変換ができない場合はfalseを返す
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 文字列を"で囲む
        /// </summary>
        /// <param name="column">変換前文字列</param>
        /// <returns>変換後文字列</returns>
        private string EncloseDbQuotes(string column)
        {
            return "\"" + column + "\"";
        }

        private const string SESSION_SEARCH_Docomo = "SESSION_SEARCH_Docomo";

        // GET: DocomoBicycleSharing
        [SessionCheck(WindowName = "Docomo決済画面")]
        public ActionResult Index(string page)
        {

            DocomoPaymentInfoListEntity info = new DocomoPaymentInfoListEntity();


            // 検索条件初期化
            this.InitSearchList(info);

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                return View(info);
            }

            //セッション情報の取得
            DocomoPaymentInfoListEntity sessiondata = (DocomoPaymentInfoListEntity)Session[SESSION_SEARCH_Docomo];
            InitAplTypeList(sessiondata.Apltype);
            int pageNo = 0;
            //ページ数から取得するリストの終了位置を指定(10件ずつのリスト)
            try
            {
                pageNo = int.Parse(page);

                // 直接入力されたページ数が存在しない場合
                if (pageNo > sessiondata.PageCount)
                {
                    ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                    return View(info);
                }
            }
            catch (FormatException error)
            {
                Trace.TraceError(Logger.GetExceptionMessage(error));
                ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                return View(info);
            }

            // 取得開始行と取得範囲を指定
            int index = (pageNo - 1) * sessiondata.rowsPerPage;
            int count = (pageNo == sessiondata.PageCount) ? sessiondata.ListMaxCount - index : sessiondata.rowsPerPage;
            //現在のページ位置
            sessiondata.ListPageNo = pageNo;

            // 取得したリスト件数が0以上
            if (0 == sessiondata.ListMaxCount)
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                info.DocomoPaymentList = null;
                return View(info);
            }
            // セッション情報を複製
            info = new DocomoPaymentInfoListEntity(sessiondata);
            // 指定範囲のデータを取得
            info.DocomoPaymentList = sessiondata.DocomoPaymentListAll.GetRange(index, count);

            return View(info);
        }


        /// <summary>
        /// 検索結果表示
        /// </summary>
        /// <param name="model">検索条件</param>
        /// <returns>検索結果描画</returns>
        [HttpPost]
        [SessionCheck(WindowName = "Docomo決済画面")]
        public ActionResult Index(DocomoPaymentInfoListEntity model)
        {
            ViewData["message"] = "";

            // 検索条件初期化
            this.InitSearchList(model);

            //検索条件:エラー判定
            if (false == this.CheckSearchError(model))
            {
                return View("~/Views/MPA0401/Index.cshtml", model);
            }

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new DocomoBikeShare().GetPaymentDateList(model);
            DocomoPaymentInfoListEntity info = new DocomoPaymentInfoListEntity();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;

            // SQL取得結果(検索条件)を出力
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.ListPageNo = model.ListPageNo;
            info.Language = model.Language;
            info.Apltype = model.Apltype;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);

            // 取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                info.DocomoPaymentList = null;
                return View(info);
            }
            // SQL取得結果(検索結果)を出力
            foreach (DataRow row in GetData.Rows)
            {
                info.DocomoPaymentListAll.Add(new DocomoPaymentInfoListEntity()
                {
                    UserId = row["UserId"].ToString(),
                    
                    TranDatetime = (DateTime.Parse(row["TranDate"].ToString())).ToString("yyyy/MM/dd HH:mm:ss"),
                    /*　F1_SYSTEM_OPERATION-201　対応時期未定のためコメントアウト
                    TranDatetime = ((DateTime)row["TranDate"]).ToString("yyyy/MM/dd HH:mm:ss"),
                     */
                    PaymentId = row["PaymentId"].ToString(),
                    CycleBizName = row["Value"].ToString(),
                    ReserveId = row["ReserveId"].ToString(),
                    PaymentType = row["PaymentType"].ToString(),
                    Amount = (int)row["Amount"],
                    Apltype = row["AplName"].ToString(),
                });
            }

            // 取得開始行と取得範囲を指定
            int index = 0;
            int count = (maxListCount < model.rowsPerPage) ? maxListCount - index : model.rowsPerPage;
            info.DocomoPaymentList = info.DocomoPaymentListAll.GetRange(index, count);

            Session.Add(SESSION_SEARCH_Docomo, info);
            return View(info);
        }

        /// <summary>
        /// バイクシェア決済一覧ダウンロード処理
        /// </summary>
        /// <param name="model">検索情報</param>
        /// <returns>CSVファイル出力</returns>
        [HttpPost]
        [SessionCheck(WindowName = "ドコモバイクシェアダウンロード処理")]
        public ActionResult DocomoBikeShareOutPutDate(DocomoPaymentInfoListEntity model)
        {
            ViewData["message"] = "";

            // 検索条件初期化
            this.InitSearchList(model);

            //検索条件:エラー判定
            if (false == this.CheckSearchError(model))
            {
                return View("~/Views/MPA0401/Index.cshtml", model);
            }

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new DocomoBikeShare().GetPaymentDateList(model);
            DocomoPaymentInfoListEntity info = new DocomoPaymentInfoListEntity();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;

            // SQL取得結果(検索条件)を出力
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.ListPageNo = model.ListPageNo;
            info.Language = model.Language;
            info.Apltype = model.Apltype;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);

            // 取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                info.DocomoPaymentList = null;
                return View(info);
            }

            // CSVファイルに書き込み
            MemoryStream ms = new MemoryStream();
            using (StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.GetEncoding("shift_jis")))
            {
                List<string> strings = new List<string>();

                // ヘッダー部
                foreach (string str in header)
                {
                    strings.Add(EncloseDbQuotes(str));
                }
                sw.WriteLine(string.Join(",", strings));

                // データ部
                foreach (DataRow row in GetData.Rows)
                {
                    strings.Clear();
                    strings.Add(EncloseDbQuotes(row["UserId"].ToString()));
                    strings.Add(EncloseDbQuotes(row["TranDate"].ToString()));
                    strings.Add(EncloseDbQuotes(row["PaymentId"].ToString()));
                    strings.Add(EncloseDbQuotes(row["Value"].ToString()));
                    strings.Add(EncloseDbQuotes(row["ReserveId"].ToString()));
                    strings.Add(EncloseDbQuotes(row["PaymentType"].ToString()));
                    strings.Add(EncloseDbQuotes(row["Amount"].ToString()));
                    strings.Add(EncloseDbQuotes(row["AplName"].ToString()));
                    sw.WriteLine(string.Join(",", strings));
                }
            }
            //ファイル名を「Nishitetsu_Coupon_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
            return File(ms.ToArray(), FILE_CONTENTTYPE, "DocomoBicycleShare_Report_" + DateTime.Parse(model.TargetDateBegin).ToString("yyyyMMdd") + "-" + DateTime.Parse(model.TargetDateBegin).ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("yyyyMMdd") + FILE_EXTENSION);
            /*　F1_SYSTEM_OPERATION-201　対応時期未定のためコメントアウト
              return File(ms.ToArray(), FILE_CONTENTTYPE, "DocomoBicycleShare_Report_" + DateTime.Parse(model.TargetDateBegin).ToString("yyyyMMdd") + "-" + DateTime.Parse(model.TargetDateEnd).ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("yyyyMMdd") + FILE_EXTENSION);
              */
        }

        /// <summary>
        /// 検索条件リスト作成
        /// </summary>
        /// <param name="model"></param>
        private void InitSearchList(DocomoPaymentInfoListEntity model)
        {
            // 検索条件のUserRole取得
            string UserRole = model.Apltype;

            // プルダウン初期化
            this.InitAplTypeList(UserRole); // アプリ種別情報
        }

        /// <summary>
        /// アプリ種別ドロップダウンリスト初期化
        /// </summary>
        /// <param name="userRole">ログインユーザのロールID</param>
        private void InitAplTypeList(string userRole)
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            // セッションに保存されているユーザー情報を取得する
            UserInfoAdminEntity UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //auチェック
            if (UserInfo.Role == "13")
            {
                userRole = UserInfo.Role;
            }

            //アプリ種別ドロップダウンリスト作成
            if (userRole == "13")
            {
                itemList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
            }
            else
            {
                itemList.Add(new SelectListItem { Text = "au", Value = "1" });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty, Selected = true });
            }

            ViewBag.AplList = itemList;
        }

        /// <summary>
        /// 検索条件エラー判定
        /// </summary>
        /// <param name="model">検索条件</param>
        /// <returns>判定結果</returns>
        private bool CheckSearchError(DocomoPaymentInfoListEntity model)
        {
            // 検索期間:未入力
            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return false;
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return false;
            }

            // 検索期間:日付以外
            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
                return false;
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return false;
            }

            // myrouteID:半角数字以外
            if ((false == string.IsNullOrEmpty(model.UserId)) &&
                (false == Int32.TryParse(model.UserId.ToString(), out int i)))
            {
                ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角英数字で再入力してください。");
                return false;
            }
            return true;
        }
    }
}