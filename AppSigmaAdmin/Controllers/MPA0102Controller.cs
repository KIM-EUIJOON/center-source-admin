using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.ResponseData;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppSigmaAdmin.Controllers
{
    public class MPA0102Controller : Controller
    {
        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";

        private const string SESSION_Coupon_Nishitetsu = "SESSION_Coupon_Nishitetsu";

        /// <summary>
        ///入力日付チェック関数
        /// </summary>
        private static bool IsDate(string s)
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
        /// 西鉄クーポン画面
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "西鉄クーポン画面")]
        public ActionResult Index(string page)
        {
            CouponInfoEntityList info = new CouponInfoEntityList();

            //セッションに保存されているユーザー情報を取得する
            UserInfoAdminEntity UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];

            //現在ログイン中のUserRole取得
            string UserRole = UserInfo.Role;

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {

                // プルダウン初期化
                this.InitFacilityList();        // 施設情報
                this.InitShopList();            // テナント情報
                this.InitAplTypeList(UserRole); // アプリ種別情報

                return View(info);
            }

            //セッション情報の取得
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey = (Dictionary<string, string>)Session[SESSION_Coupon_Nishitetsu];
            //検索条件：開始日時
            string targetDateBegin = searchKey["TargetDateBegin"];
            //検索条件：終了日時
            string targetDateEnd = searchKey["TargetDateEnd"];
            //リスト全件数
            string maxListCount = searchKey["maxListCount"];
            //検索条件：Myroute番号
            string myRouteNo = searchKey["MyrouteNo"];
            //検索条件：施設名
            string facilityName = searchKey["FacilityName"];
            //検索条件：テナント名
            string shopName = searchKey["ShopName"];
            //検索条件：アプリ種別
            string aplType = searchKey["AplType"];
            //リスト件数
            int ListNum = int.Parse(searchKey["ListNum"]);

            DateTime TargetDateStart = DateTime.Parse(targetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(targetDateEnd);
            int pageNo = 0;
            //ページ数から取得するリストの終了位置を指定(10件ずつのリスト)
            try
            {
                pageNo = int.Parse(page);
                float ListMaxPage = (float)(float.Parse(maxListCount) / (float)ListNum);
                int ListMaxPageNum = (int)Math.Ceiling(ListMaxPage);

                // 直接入力されたページ数が存在しない場合
                if (pageNo > ListMaxPageNum)
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

            int EndListNo = pageNo * ListNum;
         
            //ページ数から取得するリストの開始位置を指定(10件ずつのリスト)
            int ListNoBegin = EndListNo - (ListNum - 1);

            //List<NishitetsuPaymentInfo> SelectNishitetsuPaymentDateList = null;


            ////表示情報を取得
            //SelectNishitetsuPaymentDateList = new NishitetsuPaymentModel().GetNishitetsuPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, MyrouteNo, PaymentType, TicketNumType, TransportType, TicketId);


            int listCount = int.Parse(maxListCount);


            //開始日時
            info.TargetDateBegin = targetDateBegin;
            //終了日時
            info.TargetDateEnd = targetDateEnd;
            //全リスト件数
            info.ListMaxCount = listCount;
            //現在のページ位置
            info.ListPageNo = pageNo;
            //指定MyrouteID
            info.UserId = myRouteNo;
            //指定チケットID
            info.FacilityName = facilityName;
            //指定チケットID
            info.ShopName = shopName;
            //指定チケットID
            info.AplType = aplType;

            //取得したリスト件数が0以上
            //if (SelectNishitetsuPaymentDateList.Count > 0)
            //{
            //    info.NishitetsuPaymentInfoList = SelectNishitetsuPaymentDateList;
            //}
            //else
            //{
            //    ModelState.AddModelError("", "一致する決済データがありませんでした。");
            //}

            return View(info);
        }

        /// <summary>
        /// クーポン管理画面
        /// </summary>
        /// <returns>クーポン管理画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "クーポン管理画面")]
        public ActionResult Index(CouponInfoEntityList model)
        {

            ViewData["message"] = "";

            //セッションに保存されているユーザー情報を取得する
            UserInfoAdminEntity UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];

            //現在ログイン中のUserRole取得
            string UserRole = UserInfo.Role;


            // プルダウン初期化
            this.InitFacilityList();        // 施設情報
            this.InitShopList();            // テナント情報
            this.InitAplTypeList(UserRole); // アプリ種別情報

            // 異常判定処理
            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                //検索期間(開始)が未入力の場合
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View(model);
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                //検索期間(終了)が未入力の場合
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return View(model);
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                //検索期間(開始)のテキストボックスに日付として正しくない値が入力された場合
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
                return View(model);
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                //検索期間(終了)のテキストボックスに日付として正しくない値が入力された場合
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return View(model);
            }

            if ((false == string.IsNullOrEmpty(model.UserId)) &&
                (!Int32.TryParse(model.UserId.ToString(), out int i)))
            {
                //myrouteIDのテキストボックスに半角数字以外が入力された場合
                ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角英数字で再入力してください。");
                return View(model);
            }

            //検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //10件ずつ表示する
            int ListNum = 10;
            //表示開始位置を算出
            int EndListNo = PageNo * ListNum;
            int ListNoBegin = EndListNo - (ListNum - 1);

            //検索条件に一致する全リスト件数取得
            DataTable GetData = new CouponInfoModel().GetCouponDateList(model);
            CouponInfoEntityList info = new CouponInfoEntityList();

            //表示リストの総数
            int maxListCount = GetData.Rows.Count;
            
            //取得したリスト件数が0以上
            if (maxListCount == 0)
            {
                ModelState.AddModelError("", "一致するクーポンがありませんでした。");
                info.CouponInfoList = null;
                return View(info);
            }

            // SQL取得結果を反映
            info.TargetDateBegin = model.TargetDateBegin;
            info.TargetDateEnd = model.TargetDateEnd;
            info.ListMaxCount = maxListCount;
            info.ListPageNo = model.ListPageNo;
            info.CouponInfoList = new List<CouponInfoEntity>();
            foreach (DataRow row in GetData.Rows)
            {
                info.CouponInfoList.Add(new CouponInfoEntity()
                {
                    UserId = row["UserId"].ToString(),
                    FacilityName = row["FacilityName"].ToString(),
                    ShopCode = row["UsageShopCode"].ToString(),
                    ShopName = row["ShopName"].ToString(),
                    UsageDateTime = DateTime.Parse(row["UsageDateTime"].ToString()),
                    AplType = row["AplType"].ToString(),
                    UseCount = 1, // 暫定
                    IndustryName = row["IndustryName"].ToString()
                });
            }

            //ページ切り替え時用に検索条件を保存する
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey.Add("TargetDateBegin", model.TargetDateBegin);
            searchKey.Add("TargetDateEnd", model.TargetDateEnd);
            searchKey.Add("maxListCount", maxListCount.ToString());
            searchKey.Add("MyrouteNo", model.UserId);
            searchKey.Add("FacilityName", model.FacilityName);
            searchKey.Add("ShopName", model.ShopName);
            searchKey.Add("ListNum", ListNum.ToString());
            searchKey.Add("AplType",model.AplType);
            Session.Add(SESSION_Coupon_Nishitetsu, searchKey);

            return View(info);
        }


        /// <summary>
        /// 施設リスト初期化
        /// </summary>
        private void InitFacilityList()
        {
            List<SelectListItem> itemList = new List<SelectListItem>();
            // 施設マスタを取得
           DataTable db = new CouponInfoModel().GetFacilityNames();
           foreach (DataRow row in db.Rows)
           {
                itemList.Add(new SelectListItem { Text = row["FacilityName"].ToString(), Value = row["FacilityId"].ToString()});
            }
           itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });

            // 施設ドロップダウンリストを保存
            ViewBag.FacilityList = itemList;
        }

        /// <summary>
        /// テナントリスト初期化
        /// </summary>
        private void InitShopList()
        {
            // テナントマスタを取得
            List<SelectListItem> itemList = new List<SelectListItem>();
            // 施設マスタを取得
            DataTable db = new CouponInfoModel().GetShopNames();
            foreach (DataRow row in db.Rows)
            {
                itemList.Add(new SelectListItem { Text = row["ShopName"].ToString(), Value = row["ShopCode"].ToString() });
            }
            itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });

            // テナントドロップダウンリストを保存
            ViewBag.ShopList = itemList;

        }

        /// <summary>
        /// アプリ種別リスト初期化
        /// </summary>
        /// <param name="userRole">ログインユーザのロールID</param>
        private void InitAplTypeList(string userRole)
        {
            List<SelectListItem> AplTypeList = new List<SelectListItem>();
            //アプリ種別ドロップダウン作成
            if (userRole == "13")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
            }
            else
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }

            // アプリ種別ドロップダウンリストを保存
            ViewBag.AplList = AplTypeList;
        }
    }
}