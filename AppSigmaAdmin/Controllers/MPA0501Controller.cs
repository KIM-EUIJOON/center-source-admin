using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.ResponseData;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Utility;
using System.Diagnostics;
using static AppSigmaAdmin.Models.JTXPaymentModel;



namespace AppSigmaAdmin.Controllers
{
    public class MPA0501Controller : Controller
    {
        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";

        List<string> header = new List<string>()
        {
            "myroute会員ID",
            "決済日時",
            "決済ID",
            "商品種別",
            "大人枚数",
            "子供枚数",
            "決済種別",
            "金額",
            "領収書番号",
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

        private const string SESSION_SEARCH_Yokohama = "SESSION_SEARCH_Yokohama";

        // GET: MPA0501
        [SessionCheck(WindowName = "横浜決済画面")]
        public ActionResult Index(string page)
        {
            YokohamaPaymentInfo info = new YokohamaPaymentInfo();

            // 検索条件初期化
            this.InitSearchList(info);

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                return View(info);
            }

            //セッション情報の取得
            YokohamaPaymentInfo sessiondata = (YokohamaPaymentInfo)Session[SESSION_SEARCH_Yokohama];
            SelectTicketTypeList(sessiondata.Language, sessiondata.TicketId);
            InitAplTypeList(sessiondata.Apltype); // アプリ種別情報
            InitTicketUserTypeList(sessiondata.TicketNumType);
            InitPaymentTypeList(sessiondata.PaymentType);
            info.UserId = sessiondata.UserId;
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
            sessiondata.PageNo = pageNo;

            // 取得したリスト件数が0以上
            if (0 == sessiondata.ListMaxCount)
            {
                ModelState.AddModelError("", "一致する決済情報がありませんでした。");
                info.YokohamaPaymentInfoList = null;
                return View(info);
            }
            // セッション情報を複製
            info = new YokohamaPaymentInfo(sessiondata);
            // 指定範囲のデータを取得
            info.YokohamaPaymentInfoList = sessiondata.YokohamaPaymentInfoListAll.GetRange(index, count);

            return View(info);
        }

        /// <summary>
        /// 検索結果表示
        /// </summary>
        /// <param name="model">検索条件</param>
        /// <returns>検索結果描画</returns>
        [HttpPost]
        [SessionCheck(WindowName = "横浜決済画面")]
        public ActionResult Index(YokohamaPaymentInfo model)
        {
            ViewData["message"] = "";

            // 検索条件初期化
            this.InitSearchList(model);

            //検索条件:エラー判定
            if (false == this.CheckSearchError(model))
            {
                return View("~/Views/MPA0501/Index.cshtml", model);
            }

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new YokohamaPayment().GetPaymentDateList(model);
            YokohamaPaymentInfo info = new YokohamaPaymentInfo();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;

            // SQL取得結果(検索条件)を出力
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.PageNo = model.PageNo;
            info.UserId = model.UserId;
            info.Language = model.Language;
            info.TicketNumType = model.TicketNumType;
            info.TicketId = model.TicketId;
            info.PaymentType = model.PaymentType;
            info.Apltype = model.Apltype;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);

            // 取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                info.YokohamaPaymentInfoList = null;
                return View(info);
            }
            // SQL取得結果(検索結果)を出力
            
            //au付加用
            string TicketNameValue;
            foreach (DataRow row in GetData.Rows)
            {
                if (row["TicketGroup"].ToString() == "1")
                {
                    TicketNameValue = row["Value"].ToString() + "[au]";
                }
                else
                {
                    TicketNameValue = row["Value"].ToString();
                }

                info.YokohamaPaymentInfoListAll.Add(new YokohamaPaymentInfo()
                {
                    UserId = row["UserId"].ToString(),
                    TranDatetime = ((DateTime)row["TranDate"]).ToString("yyyy/MM/dd HH:mm:ss"),
                    PaymentId = row["PaymentId"].ToString(),
                    TicketName = TicketNameValue,
                    AdultNum = row["AdultNum"].ToString(),
                    ChildNum = row["ChildNum"].ToString(),
                    PaymentType = row["PaymentType"].ToString(),
                    Amount = (int)row["Amount"],
                    ReceiptNo = row["ReceiptNo"].ToString(),
                    Apltype = row["AplName"].ToString(),

                });
            }

            // 取得開始行と取得範囲を指定
            int index = 0;
            int count = (maxListCount < model.rowsPerPage) ? maxListCount - index : model.rowsPerPage;
            info.YokohamaPaymentInfoList = info.YokohamaPaymentInfoListAll.GetRange(index, count);

            Session.Add(SESSION_SEARCH_Yokohama, info);
            return View(info);
        }

        /// <summary>
        /// 横浜決済一覧ダウンロード処理
        /// </summary>
        /// <param name="model">検索情報</param>
        /// <returns>CSVファイル出力</returns>
        [HttpPost]
        [SessionCheck(WindowName = "横浜ダウンロード処理")]
        public ActionResult YokohamaOutPutDate(YokohamaPaymentInfo model)
        {
            ViewData["message"] = "";

            // 検索条件初期化
            this.InitSearchList(model);

            //検索条件:エラー判定
            if (false == this.CheckSearchError(model))
            {
                return View("~/Views/MPA0501/Index.cshtml", model);
            }

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new YokohamaPayment().GetPaymentDateList(model);
            YokohamaPaymentInfo info = new YokohamaPaymentInfo();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;

            // SQL取得結果(検索条件)を出力
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.PageNo = model.PageNo;
            info.Language = model.Language;
            info.UserId = model.UserId;
            info.Language = model.Language;
            info.TicketNumType = model.TicketNumType;
            info.TicketId = model.TicketId;
            info.Apltype = model.Apltype;
            info.PaymentType = model.PaymentType;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);



            // 取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                info.YokohamaPaymentInfoList = null;
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
                    strings.Add(EncloseDbQuotes(row["AdultNum"].ToString()));
                    strings.Add(EncloseDbQuotes(row["ChildNum"].ToString()));
                    strings.Add(EncloseDbQuotes(row["PaymentType"].ToString()));
                    strings.Add(EncloseDbQuotes(row["Amount"].ToString()));
                    strings.Add(EncloseDbQuotes(row["ReceiptNo"].ToString()));
                    strings.Add(EncloseDbQuotes(row["AplName"].ToString()));
                    sw.WriteLine(string.Join(",", strings));
                }
            }
            //ファイル名を「Nishitetsu_Coupon_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
            return File(ms.ToArray(), FILE_CONTENTTYPE, "Yokohama_Report_" + DateTime.Parse(model.TargetDateBegin).ToString("yyyyMMdd") + "-" + DateTime.Parse(model.TargetDateEnd).ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("yyyyMMdd") + FILE_EXTENSION);
        }

        /// <summary>
        /// 検索条件リスト作成
        /// </summary>
        /// <param name="model"></param>
        private void InitSearchList(YokohamaPaymentInfo model)
        {
            //検索条件のUserRole取得
             string UserRole = model.Apltype;
             string TicetType = model.TicketType;

            // プルダウン初期化
            this.InitTicketTypeList(model.Language, TicetType); // チケット種別情報
            this.InitAplTypeList(UserRole); // アプリ種別情報
            this.InitTicketUserTypeList(model.TicketNumType);
            this.InitPaymentTypeList(model.PaymentType);

        }
        /// <summary>
        /// 商品種別リスト(動的)
        /// </summary>
        /// <param name="id">チケットID</param>
        /// <returns>チケットリスト取得結果(JSON)</returns>
        [HttpGet]
        public ActionResult SelectTicketTypeList(string language, string id)
        {
            var itemList = InitTicketTypeList(language, id);
            return Json(itemList, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 券種ドロップダウンリスト初期化
        /// </summary>
        /// <param name="language">言語</param>
        /// <returns>券種リスト取得結果</returns>
        private List<SelectListItem> InitTicketTypeList(string language,string id)
        {
            // チケットリストを取得
            DataTable db = new YokohamaPayment().GetTicketName(language);
            List<SelectListItem> itemList = new List<SelectListItem>();

            // DataTable → SelectList型に変換
            foreach (DataRow row in db.Rows)
            {
                string idCheck = row["TicketId"].ToString();
                string AplCheck = row["TicketGroup"].ToString();
                string TicketName;
                if (AplCheck == "1")
                {
                    TicketName = row["Value"].ToString() + "[au]";
                }
                else
                {
                    TicketName = row["Value"].ToString();
                }
                if (idCheck == id)
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = TicketName,
                        Value = row["TicketId"].ToString(),
                        Selected = true,
                    });
                }
                else
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = TicketName,
                        Value = row["TicketId"].ToString(),
                    });
                }
            }
            if (id != null) { itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty}); }
            else { itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty, Selected = true }); }
            
            ViewBag.TicketList = itemList;
            return itemList;
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
        /// 大人/子供ドロップダウンリスト初期化
        /// </summary>
        /// <param name="userRole">ログインユーザのロールID</param>
        private void InitTicketUserTypeList(string Ticketuser)
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            //大人/子供ドロップダウンリスト作成
            if (Ticketuser == "1")
            {
                itemList.Add(new SelectListItem { Text = "大人", Value = "1", Selected = true });
                itemList.Add(new SelectListItem { Text = "子供", Value = "2" });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "0"});
            }
            else if (Ticketuser == "2")
            {
                itemList.Add(new SelectListItem { Text = "大人", Value = "1"});
                itemList.Add(new SelectListItem { Text = "子供", Value = "2", Selected = true });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "0" });
            }
            else
            {
                itemList.Add(new SelectListItem { Text = "大人", Value = "1" });
                itemList.Add(new SelectListItem { Text = "子供", Value = "2" });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "0", Selected = true });
            }

            ViewBag.TicketUserList = itemList;
        }

        /// <summary>
        /// 決済種別ドロップダウンリスト初期化
        /// </summary>
        /// <param name="userRole"></param>
        private void InitPaymentTypeList(string Paymenttype)
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            //アプリ種別ドロップダウンリスト作成
            if (Paymenttype == "決済種別不明")
            {
                itemList.Add(new SelectListItem { Text = "即時決済", Value = "3"});
                itemList.Add(new SelectListItem { Text = "払戻し", Value = "4" });
                itemList.Add(new SelectListItem { Text = "取消", Value = "5" });
                itemList.Add(new SelectListItem { Text = "決済種別不明", Value = "決済種別不明", Selected = true });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }
            else if(Paymenttype == "3")
            {
                itemList.Add(new SelectListItem { Text = "即時決済", Value = "3", Selected = true });
                itemList.Add(new SelectListItem { Text = "払戻し", Value = "4" });
                itemList.Add(new SelectListItem { Text = "取消", Value = "5" });
                itemList.Add(new SelectListItem { Text = "決済種別不明", Value = "決済種別不明" });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-"});
            }
            else if (Paymenttype == "4")
            {
                itemList.Add(new SelectListItem { Text = "即時決済", Value = "3" });
                itemList.Add(new SelectListItem { Text = "払戻し", Value = "4", Selected = true });
                itemList.Add(new SelectListItem { Text = "取消", Value = "5" });
                itemList.Add(new SelectListItem { Text = "決済種別不明", Value = "決済種別不明" });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }
            else if (Paymenttype == "5")
            {
                itemList.Add(new SelectListItem { Text = "即時決済", Value = "3" });
                itemList.Add(new SelectListItem { Text = "払戻し", Value = "4" });
                itemList.Add(new SelectListItem { Text = "取消", Value = "5", Selected = true });
                itemList.Add(new SelectListItem { Text = "決済種別不明", Value = "決済種別不明" });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }
            else
            {
                itemList.Add(new SelectListItem { Text = "即時決済", Value = "3" });
                itemList.Add(new SelectListItem { Text = "払戻し", Value = "4" });
                itemList.Add(new SelectListItem { Text = "取消", Value = "5" });
                itemList.Add(new SelectListItem { Text = "決済種別不明", Value = "決済種別不明" });
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }
            ViewBag.PaymentList = itemList;
        }

        /// <summary>
        /// 検索条件エラー判定
        /// </summary>
        /// <param name="model">検索条件</param>
        /// <returns>判定結果</returns>
        private bool CheckSearchError(YokohamaPaymentInfo model)
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