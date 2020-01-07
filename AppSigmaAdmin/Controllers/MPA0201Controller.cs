using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.ResponseData;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static AppSigmaAdmin.Models.JTXPaymentModel;

namespace AppSigmaAdmin.Controllers
{
    public class MPA0201Controller : Controller
    {
        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";

        //入力日付チェック関数
        public static bool IsDate(string s)
        {
            //入力された日時が年/月/日以外はエラーで返す
            string baseDatePaturn = "yyyy/M/d";

            try
            {
                DateTime.ParseExact(s, baseDatePaturn, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private const string SESSION_SEARCH_JapanTaxi = "SESSION_SEARCH_JapanTaxi";

        /// <summary>
        /// Japantaxi決済データ画面処理(検索ボタン押下以外)
        /// </summary>
        /// <returns>Japantaxi決済データ画面</returns>
        /// [SessionCheck(WindowName = "Japantaxi決済データ画面")]
        [SessionCheck(WindowName = "Japantaxi決済データ画面")]
        public ActionResult Index(string page)
        {
            //RoleIDによる振り分け
            UserInfoAdminEntity UserInfo = null;
            //セッションに保存されているユーザー情報を取得する
            UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = "-";
            UserRole = UserInfo.Role.ToString();

            //アプリ種別プルダウン
            List<SelectListItem> AplTypeList = new List<SelectListItem>();

            JtxPaymentInfoListEntity info = new JtxPaymentInfoListEntity();

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                //アプリ種別ドロップダウン作成
                /*ユーザーIDがKDDIの場合*/
                if (UserRole == "13")
                {
                    AplTypeList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
                }
                else
                {
                    AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                    AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
                }

                //アプリ種別ドロップダウンリストを保存
                ViewBag.AplList = AplTypeList;
                return View(info);
            }

            //セッション情報の取得
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey = (Dictionary<string, string>)Session[SESSION_SEARCH_JapanTaxi];
            //検索条件：開始日時
            string TargetDateBegin = searchKey["TargetDateBegin"];
            //検索条件：終了日時
            string TargetDateEnd = searchKey["TargetDateEnd"];
            //リスト全件数
            string maxListCount = searchKey["maxListCount"];
            //リスト表示件数
            int ListNum = int.Parse(searchKey["ListNum"]);
            //検索条件：Myroute番号
            string MyrouteNo = searchKey["MyrouteNo"];
            //検索条件：アプリ種別
            string Apltype = searchKey["Apltype"];

            //アプリ種別プルダウン
            /*ユーザーIDがKDDIの場合*/
            if (UserRole == "13")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
                Apltype = "1";
            }
            else
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }

            

            //アプリ種別ドロップダウンリストを保存
            ViewBag.AplList = AplTypeList;


            DateTime TargetDateStart = DateTime.Parse(TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(TargetDateEnd);

            int pageNo = 0;
            //ページ数から取得するリストの終了位置を指定(10件ずつのリスト)
            try
            {
                pageNo = int.Parse(page);
                float ListMaxPage = (float)(float.Parse(maxListCount) / (float)ListNum);
                int ListMaxPageNum = (int)Math.Ceiling(ListMaxPage);
                //直接入力されたページ数が存在しない場合
                if (pageNo > ListMaxPageNum)
                {
                    ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                    return View();
                }
            }
            catch (FormatException error)
            {
                Trace.TraceError(Logger.GetExceptionMessage(error));
                ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                return View();
            }
            int EndListNo = pageNo * ListNum;
            //ページ数から取得するリストの開始位置を指定(10件ずつのリスト)
            int ListNoBegin = EndListNo - (ListNum -1);

            List<JtxPaymentInfo> SelectPaymentDateList = null;

            //表示情報を取得
            SelectPaymentDateList = new JTXPaymentModel().GetJtxPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, MyrouteNo, Apltype);

            
            int ListCount = int.Parse(maxListCount);


            //開始日時
            info.TargetDateBegin = TargetDateBegin;
            //終了日時
            info.TargetDateEnd = TargetDateEnd;
            //全リスト件数
            info.ListMaxCount = ListCount;
            //現在のページ位置
            info.ListPageNo = pageNo;
            //表示リスト件数
            info.ListNum = ListNum;
            //指定MyrouteID
            info.UserId = MyrouteNo;
            //アプリ種別
            info.Apltype = Apltype;

            //取得したリスト件数が0以上
            if (SelectPaymentDateList.Count > 0)
            {
                info.JtxPaymentInfoList = SelectPaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
            }

            return View(info);
        }

        /// <summary>
        /// Japantaxi決済データ画面処理(検索ボタン押下時)
        /// </summary>
        /// <returns>Japantaxi決済データ画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "Japantaxi決済データ画面")]
        public ActionResult Index(JtxPaymentInfoListEntity model)
        {

            ViewData["message"] = "";
            
            //RoleIDによる振り分け
            UserInfoAdminEntity UserInfo = null;
            //セッションに保存されているユーザー情報を取得する
            UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = "-";
            UserRole = UserInfo.Role.ToString();

            //アプリ種別プルダウン
            List<SelectListItem> AplTypeList = new List<SelectListItem>();
            string Apltype = "";
            /*ユーザーIDがKDDIの場合*/
            if (UserRole == "13")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
                Apltype = "1";
            }
            else if (model.Apltype != null && model.Apltype != "-")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                Apltype = model.Apltype;
                AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-"});
            }
            else
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                Apltype = model.Apltype;
                AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }



            //アプリ種別ドロップダウンリストを保存
            ViewBag.AplList = AplTypeList;

            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View(model);
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return View(model);
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
                return View(model);
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return View(model);
            }
            if (string.IsNullOrEmpty(model.UserId))
            {
                //myrouteIDが入力されていないため操作なし
            }
            else
            {
                try
                {
                    int.Parse(model.UserId.ToString());
                }
                catch (OverflowException)
                {
                    //myrouteIDのテキストボックスに誤った数値が入力された場合
                    ModelState.AddModelError("", "myroute会員IDに誤った数値が入力されました。半角数字で再入力してください。");
                    return View(model);
                }
                catch
                {
                    //myrouteIDのテキストボックスに半角数字以外が入力された場合
                    ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角英数字で再入力してください。");
                    return View(model);
                }
            }

            List<JtxPaymentInfo> PaymentDateListMaxCount = null;
            List<JtxPaymentInfo> SelectPaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);
            //検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //10件ずつ表示する
            int ListNum = 10;
            int EndListNo = PageNo * ListNum;
            int BeginListNo = PageNo - (ListNum -1);
            string UserId;

            if (string.IsNullOrEmpty(model.UserId))
            {
                //myrouteID未入力の場合は空白を設定する
                UserId = "";
            }
            else
            {
                //myrouteIDが指定されている場合は入力内容を設定する
                UserId = model.UserId;
            }

            //検索条件に一致する全リスト件数取得
            PaymentDateListMaxCount = new JTXPaymentModel().GetJtxPaymentDateListCount(TargetDateStart, TargetDateLast, UserId, Apltype);

            //検索条件に一致したリストから表示件数分取得
            SelectPaymentDateList = new JTXPaymentModel().GetJtxPaymentDate(TargetDateStart, TargetDateLast, BeginListNo, EndListNo, UserId, Apltype);
            JtxPaymentInfoListEntity info = new JtxPaymentInfoListEntity();

            //表示リストの総数
            int maxListCount = PaymentDateListMaxCount.Count;

            //開始日時
            info.TargetDateBegin = TargetDateStart.ToString();

            //終了日時
            info.TargetDateEnd = TargetDateLast.ToString();

            //全リスト件数
            info.ListMaxCount = maxListCount;

            //現在のページ位置
            info.ListPageNo = model.ListPageNo;

            //表示リスト件数
            info.ListNum = ListNum;

            //アプリ種別
            info.Apltype = Apltype;

            //取得したリスト件数が0以上
            if (SelectPaymentDateList.Count > 0)
            {
                info.JtxPaymentInfoList = SelectPaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
            }

            //ページ切り替え時用に検索条件を保存する
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey.Add("TargetDateBegin", model.TargetDateBegin);
            searchKey.Add("TargetDateEnd", model.TargetDateEnd);
            searchKey.Add("MyrouteNo", UserId);
            searchKey.Add("maxListCount", maxListCount.ToString());
            searchKey.Add("ListNum", ListNum.ToString());   //リスト表示件数
            searchKey.Add("Apltype", Apltype.ToString());   //アプリ種別
            Session.Add(SESSION_SEARCH_JapanTaxi, searchKey);

            return View(info);
        }

        [HttpPost]
        [SessionCheck(WindowName = "Japantaxi決済データ画面")]
        public ActionResult JtxOutPutDate(JtxPaymentInfoListEntity model)
        {

            ViewData["message"] = "";
            
            //RoleIDによる振り分け
            UserInfoAdminEntity UserInfo = null;
            //セッションに保存されているユーザー情報を取得する
            UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = "-";
            UserRole = UserInfo.Role.ToString();

            //アプリ種別プルダウン
            List<SelectListItem> AplTypeList = new List<SelectListItem>();
            string Apltype = "";
            /*ユーザーIDがKDDIの場合*/
            if (UserRole == "13")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
                Apltype = "1";
            }
            else
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                Apltype = model.Apltype;
                AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }

            //アプリ種別ドロップダウンリストを保存
            ViewBag.AplList = AplTypeList;


            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View("~/Views/MPA0201/Index.cshtml");
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return View("~/Views/MPA0201/Index.cshtml");
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
                return View("~/Views/MPA0201/Index.cshtml");
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return View("~/Views/MPA0201/Index.cshtml");
            }
            if (string.IsNullOrEmpty(model.UserId))
            {
                //myrouteIDが入力されていないため操作なし
            }
            else
            {
                try
                {
                    int.Parse(model.UserId.ToString());
                }
                catch (OverflowException)
                {
                    //myrouteIDのテキストボックスに誤った数値が入力された場合
                    ModelState.AddModelError("", "myroute会員IDに誤った数値が入力されました。半角数字で再入力してください。");
                    return View("~/Views/MPA0201/Index.cshtml");
                }
                catch
                {
                    //myrouteIDのテキストボックスに半角数字以外が入力された場合
                    ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角数字で再入力してください。");
                    return View("~/Views/MPA0201/Index.cshtml");
                }
            }

            List<JtxPaymentInfo> PaymentDateListMaxCount = null;
            List<JtxPaymentInfo> SelectPaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);

            string UserId;

            //検索条件としてmyrouteIDが入力されているかの判定
            if (string.IsNullOrEmpty(model.UserId))
            {
                //myrouteID未入力の場合は空白を設定する
                UserId = "";
            }
            else
            {
                //myrouteIDが指定されている場合は入力内容を設定する
                UserId = model.UserId;
            }

            //検索条件に一致する全リスト件数取得
            PaymentDateListMaxCount = new JTXPaymentModel().GetJtxPaymentDateListCount(TargetDateStart, TargetDateLast, UserId, Apltype);

            //ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //表示リストの総数
            int maxListCount = PaymentDateListMaxCount.Count;

            //検索条件に一致したリストから表示件数分取得
            SelectPaymentDateList = new JTXPaymentModel().GetJtxPaymentDate(TargetDateStart, TargetDateLast, PageNo, maxListCount, UserId , Apltype);
            JtxPaymentInfoListEntity info = new JtxPaymentInfoListEntity();


            //開始日時
            info.TargetDateBegin = TargetDateStart.ToString();

            //終了日時
            info.TargetDateEnd = TargetDateLast.ToString();

            //全リスト件数
            info.ListMaxCount = maxListCount;

            //現在のページ位置
            info.ListPageNo = model.ListPageNo;

            //取得したリスト件数が0以上
            if (SelectPaymentDateList.Count > 0)
            {
                info.JtxPaymentInfoList = SelectPaymentDateList;

            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                return View("~/Views/MPA0201/Index.cshtml");
            }


            MemoryStream JtxMem = new MemoryStream();
            StreamWriter Jtxsw = new StreamWriter(JtxMem, System.Text.Encoding.GetEncoding("shift_jis"));
            //ヘッダー部分を書き込む
            Jtxsw.Write("\"myroute会員ID\"");
            Jtxsw.Write(',');
            Jtxsw.Write("\"決済日\"");
            Jtxsw.Write(',');
            Jtxsw.Write("\"決済ID\"");
            Jtxsw.Write(',');
            Jtxsw.Write("\"加盟店屋号\"");
            Jtxsw.Write(',');
            Jtxsw.Write("\"JTXオーダーID\"");
            Jtxsw.Write(',');
            Jtxsw.Write("\"決済種別\"");
            Jtxsw.Write(',');
            Jtxsw.Write("\"利用金額\"");
            Jtxsw.Write(',');
            Jtxsw.WriteLine("\"アプリ種別\"");

            foreach (var item in info.JtxPaymentInfoList)
            {
                //文字列に"を追加して出力
                Jtxsw.Write("\"" + item.UserId.ToString() + "\"");
                Jtxsw.Write(',');
                Jtxsw.Write("\"" + item.TranDatetime.ToString() + "\"");
                Jtxsw.Write(',');
                Jtxsw.Write("\"" + item.PaymentId.ToString() + "\"");
                Jtxsw.Write(',');
                Jtxsw.Write("\"" + item.CompanyName.ToString() + "\"");
                Jtxsw.Write(',');
                Jtxsw.Write("\"" + item.OrderId.ToString() + "\"");
                Jtxsw.Write(',');
                Jtxsw.Write("\"" + item.PaymentType.ToString() + "\"");
                Jtxsw.Write(',');
                Jtxsw.Write("\"" + item.Amount.ToString() + "\"");
                Jtxsw.Write(',');
                Jtxsw.WriteLine("\"" + item.Apltype.ToString() + "\"");
            }
            Jtxsw.Close();

            //出力日を取得
            DateTime NowDate = System.DateTime.Now;

            //ファイル名を「JapanTaxi_Report_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
            return File(JtxMem.ToArray(), FILE_CONTENTTYPE, "JapanTaxi_Report_" + TargetDateStart.ToString("yyyyMMdd") + "-" + TargetDateLast.ToString("yyyyMMdd") + "_" + NowDate.ToString("yyyyMMdd") + FILE_EXTENSION);
        }
    }
}