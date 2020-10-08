using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static AppSigmaAdmin.Models.JRKyushuInfoModels;

namespace AppSigmaAdmin.Controllers
{
    public class MPA0901Controller : Controller
    {
        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";

        List<string> header = new List<string>()
        {
            "利用日時",
            "myroute会員ID",
            "施設名",
            "クーポン名",
            "テナントコード",
            "テナント名",
            "利用件数",
            "業種",
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

        private const string SESSION_SEARCH_JRKyushu_Coupon = "SESSION_SEARCH_JRKyushu_Coupon";

        /// <summary>
        /// JR九州クーポン画面
        /// </summary>
        /// <param name="page">ページ数</param>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "JR九州クーポン管理画面")]
        public ActionResult Index(string page)
        {
            JRKyushuCouponInfoEntity info = new JRKyushuCouponInfoEntity();

            // 検索条件初期化
            this.InitSearchList(info);

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                return View(info);
            }

            //セッション情報の取得
            JRKyushuCouponInfoEntity sessiondata = (JRKyushuCouponInfoEntity)Session[SESSION_SEARCH_JRKyushu_Coupon];
            SelectTicketTypeList(sessiondata.Language, sessiondata.CouponId);
            InitAplTypeList(sessiondata.AplType); // アプリ種別情報
            InitTicketTypeList(sessiondata.Language, sessiondata.CouponId);
            InitFacilityNameList(sessiondata.FacilityId);
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
                info.CouponInfoList = null;
                return View(info);
            }
            // セッション情報を複製
            info = new JRKyushuCouponInfoEntity(sessiondata);
            // 指定範囲のデータを取得
            info.CouponInfoList = sessiondata.CouponInfoListAll.GetRange(index, count);

            return View(info);
        }

        /// <summary>
        /// 検索結果表示
        /// </summary>
        /// <param name="model">検索条件</param>
        /// <returns>検索結果描画</returns>
        [HttpPost]
        [SessionCheck(WindowName = "JR九州クーポン画面")]
        public ActionResult Index(JRKyushuCouponInfoEntity model)
        {
            ViewData["message"] = "";

            // 検索条件初期化
            this.InitSearchList(model);

            //検索条件:エラー判定
            if (false == this.CheckSearchError(model))
            {
                return View("~/Views/MPA0901/Index.cshtml", model);
            }

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new JRKyushuCouponModel().GetCouponDateList(model);
            JRKyushuCouponInfoEntity info = new JRKyushuCouponInfoEntity();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;

            // SQL取得結果(検索条件)を出力
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.PageNo = model.PageNo;
            info.UserId = model.UserId;
            info.Language = model.Language;
            info.CouponId = model.CouponId;
            info.FacilityId = model.FacilityId;
            info.ShopCode = model.ShopCode;
            info.AplType = model.AplType;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);

            // 取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                info.CouponInfoList = null;
                return View(info);
            }
            // SQL取得結果(検索結果)を出力

            foreach (DataRow row in GetData.Rows)
            {
                info.CouponInfoListAll.Add(new JRKyushuCouponInfoEntity()
                {
                    UserId = row["UserId"].ToString(),
                    UsageDateTime = (DateTime.Parse(row["UsageDateTime"].ToString())).ToString("yyyy/MM/dd HH:mm:ss"),
                    CouponName = row["Value"].ToString(),
                    ShopName = row["ShopName"].ToString(),
                    ShopCode = row["ShopCode"].ToString(),
                    FacilityName = row["FacilityName"].ToString(),
                    IndustryName = row["IndustryName"].ToString(),
                    UseCount = 1, // 利用件数=1(暫定)
                    AplType = row["AplName"].ToString(),

                });
            }

            // 取得開始行と取得範囲を指定
            int index = 0;
            int count = (maxListCount < model.rowsPerPage) ? maxListCount - index : model.rowsPerPage;
            info.CouponInfoList = info.CouponInfoListAll.GetRange(index, count);

            Session.Add(SESSION_SEARCH_JRKyushu_Coupon, info);
            return View(info);
        }

        /// <summary>
        /// 横浜決済一覧ダウンロード処理
        /// </summary>
        /// <param name="model">検索情報</param>
        /// <returns>CSVファイル出力</returns>
        [HttpPost]
        [SessionCheck(WindowName = "横浜ダウンロード処理")]
        public ActionResult JRKyushuCoupomOutPutDate(JRKyushuCouponInfoEntity model)
        {
            ViewData["message"] = "";

            // 検索条件初期化
            this.InitSearchList(model);

            //検索条件:エラー判定
            if (false == this.CheckSearchError(model))
            {
                return View("~/Views/MPA0901/Index.cshtml", model);
            }

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new JRKyushuCouponModel().GetCouponDateList(model);
            JRKyushuCouponInfoEntity info = new JRKyushuCouponInfoEntity();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;

            // SQL取得結果(検索条件)を出力
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.PageNo = model.PageNo;
            info.UserId = model.UserId;
            info.Language = model.Language;
            info.CouponId = model.CouponId;
            info.FacilityId = model.FacilityId;
            info.ShopCode = model.ShopCode;
            info.AplType = model.AplType;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);



            // 取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                info.CouponInfoList = null;
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
                    strings.Add(EncloseDbQuotes(row["UsageDateTime"].ToString()));
                    strings.Add(EncloseDbQuotes(row["UserId"].ToString()));
                    strings.Add(EncloseDbQuotes(row["FacilityName"].ToString()));
                    strings.Add(EncloseDbQuotes(row["Value"].ToString()));
                    strings.Add(EncloseDbQuotes(row["ShopCode"].ToString()));
                    strings.Add(EncloseDbQuotes(row["ShopName"].ToString()));
                    strings.Add(EncloseDbQuotes("1")); // 利用件数=1(暫定)
                    strings.Add(EncloseDbQuotes(row["IndustryName"].ToString()));
                    strings.Add(EncloseDbQuotes(row["AplName"].ToString()));
                    sw.WriteLine(string.Join(",", strings));
                }
            }
            //ファイル名を「Nishitetsu_Coupon_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
            return File(ms.ToArray(), FILE_CONTENTTYPE, "JRCoupon_Report_" + DateTime.Parse(model.TargetDateBegin).ToString("yyyyMMdd") + "-" + DateTime.Parse(model.TargetDateEnd).ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("yyyyMMdd") + FILE_EXTENSION);
        }


        /// <summary>
        /// 検索条件リスト作成
        /// </summary>
        /// <param name="model"></param>
        private void InitSearchList(JRKyushuCouponInfoEntity model)
        {
            //検索条件のUserRole取得
            string UserRole = model.AplType;
           // string TicetType = model.TicketType;

            // プルダウン初期化
            this.InitShopNameList(model.Language, model.ShopCode); // チケット種別情報
            this.InitAplTypeList(UserRole); // アプリ種別情報
            this.InitTicketTypeList(UserRole,model.CouponId);
            this.InitFacilityNameList(model.FacilityId);

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
        /// クーポンドロップダウンリスト初期化
        /// </summary>
        /// <param name="language">言語</param>
        /// <returns>券種リスト取得結果</returns>
        private List<SelectListItem> InitTicketTypeList(string language, string id)
        {
            // チケットリストを取得
            DataTable db = new JRKyushuCouponModel().JRKyushuPassportList(language);
            List<SelectListItem> itemList = new List<SelectListItem>();

            // DataTable → SelectList型に変換
            foreach (DataRow row in db.Rows)
            {
                string idCheck = row["CouponId"].ToString();
                string TicketName = row["Value"].ToString();
                
                if (idCheck == id)
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = TicketName,
                        Value = row["CouponId"].ToString(),
                        Selected = true,
                    });
                }
                else
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = TicketName,
                        Value = row["CouponId"].ToString(),
                    });
                }
            }
            if (id != null) { itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty }); }
            else { itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty, Selected = true }); }

            ViewBag.TicketList = itemList;
            return itemList;
        }

        /// <summary>
        /// 施設名ドロップダウンリスト初期化
        /// </summary>
        /// <param name="language">言語</param>
        /// <returns>券種リスト取得結果</returns>
        private List<SelectListItem> InitFacilityNameList(string id)
        {
            string language = "ja";
            // チケットリストを取得
            DataTable db = new JRKyushuCouponModel().GetFacilityNames(language);
            List<SelectListItem> itemList = new List<SelectListItem>();

            // DataTable → SelectList型に変換
            foreach (DataRow row in db.Rows)
            {
                string TicketName = row["FacilityName"].ToString();
                string idCheck = row["FacilityId"].ToString();

                if (idCheck == id)
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = TicketName,
                        Value = row["FacilityId"].ToString(),
                        Selected = true,
                    });
                }
                else
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = TicketName,
                        Value = row["FacilityId"].ToString(),
                    });
                }
            }
            if (id != null) { itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty }); }
            else { itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty, Selected = true }); }

            ViewBag.FacilityList = itemList;
            return itemList;
        }

        /// <summary>
        /// テナント名ドロップダウンリスト初期化
        /// </summary>
        /// <param name="language">言語</param>
        /// <returns>券種リスト取得結果</returns>
        private List<SelectListItem> InitShopNameList(string language, string id)
        {
            // チケットリストを取得
            DataTable db = new JRKyushuCouponModel().GetShopName(language);
            List<SelectListItem> itemList = new List<SelectListItem>();

            // DataTable → SelectList型に変換
            foreach (DataRow row in db.Rows)
            {
                string TicketName = row["ShopName"].ToString();
                string idCheck = row["ShopCode"].ToString();

                if (idCheck == id)
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = TicketName,
                        Value = row["ShopCode"].ToString(),
                        Selected = true,
                    });
                }
                else
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = TicketName,
                        Value = row["ShopCode"].ToString(),
                    });
                }
            }
            if (id != null) { itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty }); }
            else { itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty, Selected = true }); }

            ViewBag.ShopList = itemList;
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
        /// 検索条件エラー判定
        /// </summary>
        /// <param name="model">検索条件</param>
        /// <returns>判定結果</returns>
        private bool CheckSearchError(CouponInfoEntityList model)
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