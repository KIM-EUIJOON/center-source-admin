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

            // プルダウン初期化
            this.InitFacilityList();        // 施設情報
            this.InitShopList();            // テナント情報
            this.InitAplTypeList(UserRole); // アプリ種別情報

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                return View(info);
            }

            //セッション情報の取得
            CouponInfoEntityList sessiondata = (CouponInfoEntityList)Session[SESSION_Coupon_Nishitetsu];

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

            // 取得開始行を指定
            int index = (pageNo -1) * sessiondata.rowsPerPage;
            int count = (pageNo == sessiondata.PageCount) ? sessiondata.ListMaxCount - index : sessiondata.rowsPerPage;
            //現在のページ位置
            sessiondata.PageNo = pageNo;

            // 取得したリスト件数が0以上
            if (0 == sessiondata.ListMaxCount)
            {
                ModelState.AddModelError("", "一致するクーポンがありませんでした。");
                info.CouponInfoList = null;
                return View(info);
            }
            // セッション情報を複製
            info =new CouponInfoEntityList(sessiondata);
            // 指定範囲のデータを取得
            info.CouponInfoList = sessiondata.CouponInfoList.GetRange(index, count);

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

            // セッションに保存されているユーザー情報を取得する
            UserInfoAdminEntity UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];

            // 現在ログイン中のUserRole取得
            string UserRole = UserInfo.Role;

            // プルダウン初期化
            this.InitFacilityList();        // 施設情報
            this.InitShopList();            // テナント情報
            this.InitAplTypeList(UserRole); // アプリ種別情報

            // 異常判定処理
            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                // 検索期間(開始)が未入力の場合
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View(model);
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                // 検索期間(終了)が未入力の場合
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return View(model);
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                // 検索期間(開始)のテキストボックスに日付として正しくない値が入力された場合
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
                return View(model);
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                // 検索期間(終了)のテキストボックスに日付として正しくない値が入力された場合
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return View(model);
            }

            if ((false == string.IsNullOrEmpty(model.UserId)) &&
                (!Int32.TryParse(model.UserId.ToString(), out int i)))
            {
                // myrouteIDのテキストボックスに半角数字以外が入力された場合
                ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角英数字で再入力してください。");
                return View(model);
            }

            // 検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.PageNo + 1;

            // 10件ずつ表示する
            int ListNum = 10;
            // 表示開始位置を算出
            int EndListNo = PageNo * ListNum;
            int BeginListNo = EndListNo - (ListNum - 1);

            // 検索条件に一致する全リスト件数取得
            DataTable GetData = new CouponInfoModel().GetCouponDateList(model);
            CouponInfoEntityList info = new CouponInfoEntityList();

            // 表示リストの総数
            int maxListCount = GetData.Rows.Count;
            
            // 取得したリスト件数が0以上
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
            info.PageNo = model.PageNo;
            info.PageCount = (int)Math.Ceiling((float)maxListCount / (float)model.rowsPerPage);
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

            // セッションを作成
            Session.Add(SESSION_Coupon_Nishitetsu, info);
            return View(info);
        }

        /// <summary>
        /// 施設ドロップダウンリスト初期化
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

            ViewBag.FacilityList = itemList;
        }

        /// <summary>
        /// テナントドロップダウンリスト初期化
        /// </summary>
        private void InitShopList()
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            // 施設マスタを取得
            DataTable db = new CouponInfoModel().GetShopNames();
            foreach (DataRow row in db.Rows)
            {
                itemList.Add(new SelectListItem { Text = row["ShopName"].ToString(), Value = row["ShopCode"].ToString() });
            }
            itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });

            ViewBag.ShopList = itemList;

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
                itemList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }

            ViewBag.AplList = itemList;
        }
    }
}