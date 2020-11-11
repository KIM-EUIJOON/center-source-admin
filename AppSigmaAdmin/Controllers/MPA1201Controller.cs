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
using static AppSigmaAdmin.Models.MiyakohInfoModels;

namespace AppSigmaAdmin.Controllers
{
    public class MPA1201Controller : Controller
    {
        // GET: MPA1201
        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";

        List<string> header = new List<string>()
        {
            "利用日時",
            "myroute会員ID",
            "施設名",
            //"テナントコード",
            //"テナント名",
            "利用件数",
            "業種",
            "アプリ種別"
        };

        private const string PAGE_NAME = "MPA1201";

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

        private const string SESSION_SEARCH_MuseumUsage = "SESSION_SEARCH_MuseumUsage";

        /// <summary>
        /// 施設利用実績画面
        /// </summary>
        /// <param name="page">ページ数</param>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "施設利用実績画面")]
        public ActionResult Index(string page)
        {
            MuseumUseInfo info = new MuseumUseInfo();
            //セッションに保存されているユーザー情報を取得する
            UserInfoAdminEntity UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = UserInfo.Role.ToString();

            // 検索条件初期化
            this.InitSearchList(info, UserRole);

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                return View(info);
            }

            //セッション情報の取得
            MuseumUseInfo sessiondata = (MuseumUseInfo)Session[SESSION_SEARCH_MuseumUsage];
            SelectTicketTypeList(sessiondata.Language, sessiondata.FacilityId);
            InitAplTypeList(sessiondata.Apltype); // アプリ種別情報
            InitFacilityNameList(sessiondata.Language, sessiondata.FacilityId);
            InitShopNameList(sessiondata.Language, sessiondata.ShopType);
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
            sessiondata.ListPageNo = pageNo;

            // 取得したリスト件数が0以上
            if (0 == sessiondata.ListMaxCount)
            {
                ModelState.AddModelError("", "一致する利用履歴がありませんでした。");
                info.MuseumUseInfoList = null;
                return View(info);
            }
            // セッション情報を複製
            info = new MuseumUseInfo(sessiondata);
            // 指定範囲のデータを取得
            info.MuseumUseInfoList = sessiondata.MuseumUseInfoListAll.GetRange(index, count);

            return View(info);
        }

        /// <summary>
        /// 検索結果表示
        /// </summary>
        /// <param name="model">検索条件</param>
        /// <returns>検索結果描画</returns>
        [HttpPost]
        [SessionCheck(WindowName = "JR九州クーポン画面")]
        public ActionResult Index(MuseumUseInfo model)
        {
            ViewData["message"] = "";
            //セッションに保存されているユーザー情報を取得する
            UserInfoAdminEntity UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = UserInfo.Role.ToString();

            // 検索条件初期化
            this.InitSearchList(model, UserRole);

            //検索条件:エラー判定
            if (false == this.CheckSearchError(model))
            {
                return View("~/Views/MPA1201/Index.cshtml", model);
            }

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new MuseumInfoModels().GetMuseumUsageDateList(model, PAGE_NAME);
            MuseumUseInfo info = new MuseumUseInfo();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;

            // SQL取得結果(検索条件)を出力
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.ListPageNo = model.ListPageNo;
            info.UserId = model.UserId;
            info.Language = model.Language;
            info.TenantID = model.TenantID;
            info.FacilityId = model.FacilityId;
            info.ShopType = model.ShopType;
            info.Apltype = model.Apltype;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);

            // 取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致する利用履歴がありませんでした。");
                info.MuseumUseInfoList = null;
                return View(info);
            }
            // SQL取得結果(検索結果)を出力

            foreach (DataRow row in GetData.Rows)
            {
                info.MuseumUseInfoListAll.Add(new MuseumUseInfo()
                {
                    UserId = row["UserId"].ToString(),
                    UseDatetime = (DateTime.Parse(row["UsageStartDatetime"].ToString())).ToString("yyyy/MM/dd HH:mm:ss"),
                    FacilityName = row["MuseumName"].ToString(),
                    //TenantName = row["ServiceName"].ToString(),
                    //TenantID = row["ServiceResourceId"].ToString(),
                    UseCount = 1, // 利用件数=1(暫定)
                    Denomination = "",/*業種：暫定的にブランクを設定（F1_SYSTEM_OPERATION-224）*/
                    Apltype = row["AplName"].ToString(),
                });
            }

            // 取得開始行と取得範囲を指定
            int index = 0;
            int count = (maxListCount < model.rowsPerPage) ? maxListCount - index : model.rowsPerPage;
            info.MuseumUseInfoList = info.MuseumUseInfoListAll.GetRange(index, count);

            Session.Add(SESSION_SEARCH_MuseumUsage, info);
            return View(info);
        }

        /// <summary>
        /// CSVダウンロード処理
        /// </summary>
        /// <param name="model">検索情報</param>
        /// <returns>CSVファイル出力</returns>
        [HttpPost]
        [SessionCheck(WindowName = "横浜ダウンロード処理")]
        public ActionResult CsvOutPutDate(MuseumUseInfo model)
        {
            ViewData["message"] = "";
            //セッションに保存されているユーザー情報を取得する
            UserInfoAdminEntity UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = UserInfo.Role.ToString();

            // 検索条件初期化
            this.InitSearchList(model, UserRole);

            //検索条件:エラー判定
            if (false == this.CheckSearchError(model))
            {
                return View("~/Views/MPA1201/Index.cshtml", model);
            }

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new MuseumInfoModels().GetMuseumUsageDateList(model, PAGE_NAME);
            MuseumUseInfo info = new MuseumUseInfo();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;

            // SQL取得結果(検索条件)を出力
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.ListPageNo = model.ListPageNo;
            info.UserId = model.UserId;
            info.Language = model.Language;
            info.TenantID = model.TenantID;
            info.FacilityId = model.FacilityId;
            info.ShopType = model.ShopType;
            info.Apltype = model.Apltype;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);



            // 取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致する利用履歴がありませんでした。");
                info.MuseumUseInfoList = null;
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
                    strings.Add(EncloseDbQuotes((DateTime.Parse(row["UsageStartDatetime"].ToString())).ToString("yyyy/MM/dd HH:mm:ss")));
                    strings.Add(EncloseDbQuotes(row["UserId"].ToString()));
                    strings.Add(EncloseDbQuotes(row["MuseumName"].ToString()));
                    //strings.Add(EncloseDbQuotes(row["ServiceResourceId"].ToString()));
                    //strings.Add(EncloseDbQuotes(row["ServiceName"].ToString()));
                    strings.Add(EncloseDbQuotes("1")); // 利用件数=1(暫定)
                    strings.Add(EncloseDbQuotes("")); /*業種：暫定的にブランクを設定（F1_SYSTEM_OPERATION-224）*/
                    strings.Add(EncloseDbQuotes(row["AplName"].ToString()));
                    sw.WriteLine(string.Join(",", strings));
                }
            }
            //ファイル名を「Nishitetsu_Coupon_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
            return File(ms.ToArray(), FILE_CONTENTTYPE, "MiyazakitourticketUse_Report_" + DateTime.Parse(model.TargetDateBegin).ToString("yyyyMMdd") + "-" + DateTime.Parse(model.TargetDateEnd).ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("yyyyMMdd") + FILE_EXTENSION);
        }


        /// <summary>
        /// 検索条件リスト作成
        /// </summary>
        /// <param name="model"></param>
        /// <param name="UserRole"></param>
        private void InitSearchList(MuseumUseInfo model, string UserRole)
        {
            // プルダウン初期化
            this.InitAplTypeList(UserRole); // アプリ種別情報
            this.InitFacilityNameList(model.Language, model.FacilityId);// チケット種別情報
            this.InitShopNameList(model.Language,model.ShopType);

        }

        /// <summary>
        /// 商品種別リスト(動的)
        /// </summary>
        /// <param name="id">チケットID</param>
        /// <returns>チケットリスト取得結果(JSON)</returns>
        [HttpGet]
        public ActionResult SelectTicketTypeList(string language, string id)
        {

            var itemList = InitShopNameList(language, id);
            return Json(itemList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 施設名ドロップダウンリスト初期化
        /// </summary>
        /// <param name="language">言語</param>
        /// <returns>券種リスト取得結果</returns>
        private List<SelectListItem> InitFacilityNameList(string language, string id)
        {
            // 施設名リストを取得
            DataTable db = new MuseumInfoModels().GetFacilityNames(language, PAGE_NAME);
            List<SelectListItem> itemList = new List<SelectListItem>();

            // DataTable → SelectList型に変換
            foreach (DataRow row in db.Rows)
            {
                string idCheck = row["FacilityId"].ToString();
                string Facility = row["Value"].ToString();

                if (idCheck == id)
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = Facility,
                        Value = row["FacilityId"].ToString(),
                        Selected = true,
                    });
                }
                else
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = Facility,
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
        /// <returns>テナント名リスト取得結果</returns>
        private List<SelectListItem> InitShopNameList(string language, string id)
        {
            // チケットリストを取得
            DataTable db = new MuseumInfoModels().GetShopName(language, PAGE_NAME);
            List<SelectListItem> itemList = new List<SelectListItem>();

            // DataTable → SelectList型に変換
            foreach (DataRow row in db.Rows)
            {
                string ShopName = row["Value"].ToString();
                string ListShopType = row["FacilityId"].ToString() + "/" + row["ServiceResourceId"].ToString(); //施設ID/サービスリソースID


                if (ListShopType == id)
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = ShopName,
                        Value = ListShopType,
                        Selected = true,
                    });
                }
                else
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = ShopName,
                        Value = ListShopType,
                    });
                }
            }
            if (id != null)
            {
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = String.Empty });
            }
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
        private bool CheckSearchError(MuseumUseInfo model)
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
            if (model.FacilityId != "-" && model.ShopType!=null)
            {
                //開催イベント名選択肢から施設IDを分離する
                string SearchOb = "/"; //「/」判定用
                int num = model.ShopType.IndexOf(SearchOb);
                string ShopId = model.ShopType.Substring(0, num);       //チケットID分離
                
                if (ShopId != model.FacilityId)
                {
                    ModelState.AddModelError("", "施設種別とテナント名の種別が一致しません。再選択してください。");
                    return false;
                }
            }
            return true;
        }
    
    }
}