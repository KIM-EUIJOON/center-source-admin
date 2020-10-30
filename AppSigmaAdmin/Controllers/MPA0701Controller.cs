using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.ResponseData;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static AppSigmaAdmin.Models.MiyakohInfoModels;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;

namespace AppSigmaAdmin.Controllers
{
    public class MPA0701Controller : Controller
    {
        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";


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

        private const string SESSION_SEARCH_MiyakohPayment = "SESSION_SEARCH_MiyakohPayment";


        /// <summary>
        /// 宮交決済データ画面
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "宮交決済画面")]
        public ActionResult Index(string page)
        {
            //RoleIDによる振り分け
            UserInfoAdminEntity UserInfo = null;
            //セッションに保存されているユーザー情報を取得する
            UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = "-";
            UserRole = UserInfo.Role.ToString();

            //プルダウンリスト
            List<MiyakohPaymentInfoListEntity> MiyakohTicket = null;
            MiyakohTicket = new MiyakohPaymentModel().MiyakohPassportList(UserRole);
            MiyakohPaymentInfoListEntity info = new MiyakohPaymentInfoListEntity();

            //商品種別プルダウン
            List<SelectListItem> TicketTypeList = new List<SelectListItem>();
            int selectflg = 0;

            //チケット種別プルダウン
            List<SelectListItem> TranseTypeList = new List<SelectListItem>();

            //アプリ種別プルダウン
            List<SelectListItem> AplTypeList = new List<SelectListItem>();

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                foreach (var TicketList in MiyakohTicket)
                {
                    string TicketName = TicketList.TicketName.ToString(); //商品種別名称
                    string ListTicketType = TicketList.TicketId.ToString() + "/" + TicketList.TransportType.ToString(); //チケットID/交通手段
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = ListTicketType });
                }
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
                ViewBag.TicketList = TicketTypeList;

                //チケット種別プルダウン作成
                TranseTypeList.Add(new SelectListItem { Text = "バス", Value = "11" });
                TranseTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
                ViewBag.TranseList = TranseTypeList;

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
            searchKey = (Dictionary<string, string>)Session[SESSION_SEARCH_MiyakohPayment];
            //検索条件：開始日時
            string TargetDateBegin = searchKey["TargetDateBegin"];
            //検索条件：終了日時
            string TargetDateEnd = searchKey["TargetDateEnd"];
            //リスト全件数
            string maxListCount = searchKey["maxListCount"];
            //検索条件：Myroute番号
            string MyrouteNo = searchKey["MyrouteNo"];
            //検索条件：チケット種別
            string TransportType = searchKey["TransportType"];
            //検索条件：決済種別
            string PaymentType = searchKey["PaymentType"];
            //検索条件：枚数種別
            string TicketNumType = searchKey["TicketNumType"];
            //リスト件数
            int ListNum = int.Parse(searchKey["ListNum"]);
            string TicketInfo = searchKey["TicketInfo"];
            //検索条件：チケットID
            string TicketId = searchKey["TicketId"];
            //検索条件：アプリ種別
            string AplType = searchKey["AplType"];

            foreach (var TicketList in MiyakohTicket)
            {
                string TicketName = TicketList.TicketName.ToString(); //商品種別名称
                string ListTicketType = TicketList.TicketId.ToString() + "/" + TicketList.TransportType.ToString(); //チケットID/交通手段


                if (TicketInfo != ListTicketType)
                {
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = ListTicketType });
                }
                else if (TicketInfo == ListTicketType)
                {
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = ListTicketType, Selected = true });
                    selectflg = 1;
                }
            }
            if (selectflg == 1)
            {
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }
            else
            {
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }

            ViewBag.TicketList = TicketTypeList;

            //チケット種別プルダウン作成
            TranseTypeList.Add(new SelectListItem { Text = "バス", Value = "11" });
            TranseTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            ViewBag.TranseList = TranseTypeList;

            //アプリ種別プルダウン
            /*ユーザーIDがKDDIの場合*/
            if (UserRole == "13")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
                AplType = "1";
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
                    info.TransportType = "-";
                    info.TicketId = "-";
                    ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                    return View(info);
                }
            }
            catch (FormatException error)
            {
                Trace.TraceError(Logger.GetExceptionMessage(error));
                info.TransportType = "-";
                info.TicketId = "-";
                ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                return View(info);
            }
            int EndListNo = pageNo * ListNum;
            //ページ数から取得するリストの開始位置を指定(10件ずつのリスト)
            int ListNoBegin = EndListNo - (ListNum - 1);

            List<MiyakohPaymentInfoListEntity> SelectMiyakohPaymentDateList = null;

            //表示情報を取得
            SelectMiyakohPaymentDateList = new MiyakohPaymentModel().GetMiyakohPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, MyrouteNo, PaymentType, TicketNumType, TransportType, TicketId, AplType);

            int ListCount = int.Parse(maxListCount);

            //開始日時
            info.TargetDateBegin = TargetDateBegin;
            //終了日時
            info.TargetDateEnd = TargetDateEnd;
            //全リスト件数
            info.ListMaxCount = ListCount;
            //現在のページ位置
            info.ListPageNo = pageNo;
            //指定MyrouteID
            info.UserId = MyrouteNo;
            //指定チケットID
            info.TicketId = TicketId;
            //チケット種別
            info.TransportType = TransportType;
            //指定決済種別
            info.PaymentType = PaymentType;
            //指定枚数種別
            info.TicketNumType = TicketNumType;
            //表示リスト件数
            info.ListNum = ListNum;
            //アプリ種別
            info.Apltype = AplType;

            //取得したリスト件数が0以上
            if (SelectMiyakohPaymentDateList.Count > 0)
            {
                info.MiyakohPaymentInfoList = SelectMiyakohPaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
            }

            return View(info);

        }

        /// <summary>
        /// 宮交決済データ画面
        /// </summary>
        /// <returns>宮交決済データ画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "西鉄決済データ画面")]
        public ActionResult Index(MiyakohPaymentInfoListEntity model)
        {

            ViewData["message"] = "";

            //商品種別選択肢から交通種別を分離する
            string SearchOb = "/"; //「/」判定用
            int num;
            //選択チケットID
            string TicketId = "-";
            //選択チケット種別
            string TransePortCheck = "-";       //チケット種別保存用
            string TransportType = "-";         //交通種別保存用

            if (model.TicketInfo != "-" && model.TicketInfo != null)//商品種別が選択済でチケット種別が未選択の場合
            {

                num = model.TicketInfo.IndexOf(SearchOb);
                TicketId = model.TicketInfo.Substring(0, num);       //チケットID分離
                int Tpt = model.TicketInfo.Length - (num + 1);
                TransePortCheck = model.TicketInfo.Substring(num + 1, Tpt).ToString();  //交通種別分離

                if (model.TransportType == "-")
                {
                    TransportType = TransePortCheck;
                }
                else
                {
                    TransportType = model.TransportType;
                }
            }
            else if (model.TicketInfo == null)
            {
                TransportType = "-";
                model.TransportType = TransportType;
            }
            else
            {
                TransportType = model.TransportType;
            }


            //RoleIDによる振り分け
            UserInfoAdminEntity UserInfo = null;
            //セッションに保存されているユーザー情報を取得する
            UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = "-";
            UserRole = UserInfo.Role.ToString();

            //商品種別プルダウンリスト項目取得
            List<MiyakohPaymentInfoListEntity> MiyakohTicket = null;
            MiyakohTicket = new MiyakohPaymentModel().MiyakohPassportList(UserRole);
            //商品種別プルダウンリスト
            List<SelectListItem> TicketTypeList = new List<SelectListItem>();
            //商品種別プルダウンリスト作成(商品種別はチケット種別の影響を受けない)
            foreach (var TicketList in MiyakohTicket)
            {
                string TicketName = TicketList.TicketName.ToString(); //商品種別名称
                string TicketType = TicketList.TicketId.ToString() + "/" + TicketList.TransportType.ToString(); //チケットID+/+交通手段

                if (model.TicketInfo != TicketType)
                {
                    //選択されていない商品種別の場合
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = TicketType });
                }
                else if (model.TicketInfo == TicketType)
                {
                    //選択されている商品種別の場合
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = TicketType, Selected = true });
                }
            }

            if (model.TicketInfo != "-" && model.TicketInfo != null)
            {
                //商品種別が選択されている場合
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }
            else
            {
                //未選択の場合
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }
            //商品種別プルダウンリストを保存
            ViewBag.TicketList = TicketTypeList;

            //チケット種別プルダウン
            List<SelectListItem> TranseTypeList = new List<SelectListItem>();

            TranseTypeList.Add(new SelectListItem { Text = "バス", Value = "11" });
            TranseTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });

            //チケット種別ドロップダウンリストを保存
            ViewBag.TranseList = TranseTypeList;

            //アプリ種別プルダウン
            List<SelectListItem> AplTypeList = new List<SelectListItem>();
            string AplType = "";
            /*ユーザーIDがKDDIの場合*/
            if (UserRole == "13")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
                AplType = "1";
            }
            else if (model.Apltype != null && model.Apltype != "-")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
                AplType = model.Apltype;
            }
            else
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
                AplType = model.Apltype;

            }

            //アプリ種別ドロップダウンリストを保存
            ViewBag.AplList = AplTypeList;

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

            //西鉄決済データ最大件数取得用リスト
            List<MiyakohPaymentInfoListEntity> MiyakohPaymentDateListMaxCount = null;
            //西鉄決済データ表示件数分取得用リスト
            List<MiyakohPaymentInfoListEntity> SelectMiyakohPaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);
            //検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //10件ずつ表示する
            int ListNum = 10;
            //表示開始位置を算出
            int EndListNo = PageNo * ListNum;
            int ListNoBegin = EndListNo - (ListNum - 1);
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

            //チケットIDとバス/鉄道選択肢が一致するか確認(どちらかが未選択の場合は確認対象外)
            if (TransePortCheck != "-" && TransportType != "-")
            {
                if (TransePortCheck != TransportType)
                {
                    ModelState.AddModelError("", "商品種別とバス/鉄道の種別が一致しません。再選択してください。");
                    return View(model);
                }
            }
            //検索条件に一致する全リスト件数取得
            MiyakohPaymentDateListMaxCount = new MiyakohPaymentModel().MiyakohPaymentDateListMaxCount(TargetDateStart, TargetDateLast, UserId, model.PaymentType, model.TicketNumType, TransportType, TicketId, AplType);

            //検索条件に一致したリストから表示件数分取得
            SelectMiyakohPaymentDateList = new MiyakohPaymentModel().GetMiyakohPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, UserId, model.PaymentType, model.TicketNumType, TransportType, TicketId, AplType);

            MiyakohPaymentInfoListEntity info = new MiyakohPaymentInfoListEntity();


            //表示リストの総数
            int maxListCount = MiyakohPaymentDateListMaxCount.Count;

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
            //指定チケットID
            info.TicketId = TicketId;

            //チケット種別
            info.TransportType = TransportType;

            //ドロップダウンリスト用チケット種別
            info.TicketInfo = model.TicketInfo;

            //指定決済種別
            info.PaymentType = model.PaymentType;

            //指定枚数種別
            info.TicketNumType = model.TicketNumType;

            //アプリ種別
            info.Apltype = AplType;

            //取得したリスト件数が0以上
            if (SelectMiyakohPaymentDateList.Count > 0)
            {
                info.MiyakohPaymentInfoList = SelectMiyakohPaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
            }

            //ページ切り替え時用に検索条件を保存する
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey.Add("TargetDateBegin", model.TargetDateBegin);
            searchKey.Add("TargetDateEnd", model.TargetDateEnd);
            searchKey.Add("maxListCount", maxListCount.ToString());
            searchKey.Add("MyrouteNo", UserId);
            searchKey.Add("TransportType", model.TransportType);
            searchKey.Add("TicketInfo", model.TicketInfo);
            searchKey.Add("PaymentType", model.PaymentType);
            searchKey.Add("TicketNumType", model.TicketNumType);
            searchKey.Add("ListNum", ListNum.ToString());
            searchKey.Add("TicketId", TicketId);
            searchKey.Add("AplType", model.Apltype);
            Session.Add(SESSION_SEARCH_MiyakohPayment, searchKey);


            return View(info);
        }

        /// <summary>
        /// JR九州決済データダウンロード処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [SessionCheck(WindowName = "JR九州決済データ画面")]
        public ActionResult MiyakohPaymentOutPutDate(MiyakohPaymentInfoListEntity model)
        {

            ViewData["message"] = "";
            MiyakohPaymentInfoListEntity info = new MiyakohPaymentInfoListEntity();

            //商品種別選択肢から交通種別を分離する
            string SearchOb = "/"; //「/」判定用
            int num;
            //選択商品種別
            string TicketId = "-";
            //選択チケット種別
            string TransePortCheck = "-";       //チケット種別保存用
            string TransportType = "-";         //交通種別保存用

            if (model.TicketInfo != "-")//商品種別が選択済の場合
            {
                num = model.TicketInfo.IndexOf(SearchOb);
                TicketId = model.TicketInfo.Substring(0, num);       //商品種別分離
                int Tpt = model.TicketInfo.Length - (num + 1);
                TransePortCheck = model.TicketInfo.Substring(num + 1, Tpt).ToString();  //交通種別分離

                //バス/鉄道種別が未選択の場合
                if (model.TransportType == "-")
                {
                    //商品種別から取得した交通種別が選択されている状態に変更する
                    TransportType = TransePortCheck;
                }
                else
                {
                    TransportType = model.TransportType;
                }
            }
            else
            {
                TransportType = model.TransportType;
            }

            //RoleIDによる振り分け
            UserInfoAdminEntity UserInfo = null;
            //セッションに保存されているユーザー情報を取得する
            UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            string UserRole = "-";
            UserRole = UserInfo.Role.ToString();

            //商品種別プルダウンリスト項目取得
            List<MiyakohPaymentInfoListEntity> MiyakohTicket = null;
            MiyakohTicket = new MiyakohPaymentModel().MiyakohPassportList(UserRole);
            //商品種別プルダウンリスト
            List<SelectListItem> TicketTypeList = new List<SelectListItem>();
            //商品種別プルダウンリスト作成(商品種別はチケット種別の影響を受けない)
            foreach (var TicketList in MiyakohTicket)
            {
                string TicketName = TicketList.TicketName.ToString(); //商品種別名称
                string TicketType = TicketList.TicketId.ToString() + "/" + TicketList.TransportType.ToString(); //チケットID/交通手段

                if (model.TicketInfo != TicketType)
                {
                    //選択されていない商品種別の場合
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = TicketType });
                }
                else if (model.TicketInfo == TicketType)
                {
                    //選択されている商品種別の場合                       
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = TicketType, Selected = true });
                }
            }
            if (model.TicketInfo == "-")
            {
                //商品種別が未選択の場合
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }
            else
            {
                //商品種別が選択されている場合
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }
            //商品種別プルダウンリストを保存
            ViewBag.TicketList = TicketTypeList;

            //チケット種別プルダウンリスト作成
            List<SelectListItem> TranseTypeList = new List<SelectListItem>();
            TranseTypeList.Add(new SelectListItem { Text = "鉄道", Value = "10" });
            TranseTypeList.Add(new SelectListItem { Text = "マルチ", Value = "99" });
            TranseTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });

            //チケット種別ドロップダウンリストを保存
            ViewBag.TranseList = TranseTypeList;

            //アプリ種別プルダウン
            List<SelectListItem> AplTypeList = new List<SelectListItem>();
            string AplType = "";
            /*ユーザーIDがKDDIの場合*/
            if (UserRole == "13")
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1", Selected = true });
                AplType = "1";
            }
            else
            {
                AplTypeList.Add(new SelectListItem { Text = "au", Value = "1" });
                AplType = model.Apltype;
                AplTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }



            //アプリ種別ドロップダウンリストを保存
            ViewBag.AplList = AplTypeList;

            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                //検索期間(開始)が未入力の場合
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View("~/Views/MPA0701/Index.cshtml", model);
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                //検索期間(終了)が未入力の場合
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return View("~/Views/MPA0701/Index.cshtml", model);
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                //検索期間(開始)のテキストボックスに日付として正しくない値が入力された場合
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
                return View("~/Views/MPA0701/Index.cshtml", model);
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                //検索期間(終了)のテキストボックスに日付として正しくない値が入力された場合
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return View("~/Views/MPA0701/Index.cshtml", model);
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
                    return View("~/Views/MPA0701/Index.cshtml", model);
                }
                catch
                {
                    //myrouteIDのテキストボックスに半角数字以外が入力された場合
                    ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角英数字で再入力してください。");
                    return View("~/Views/MPA0701/Index.cshtml", model);
                }
            }

            //西鉄決済データ最大件数取得用リスト
            List<MiyakohPaymentInfoListEntity> MiyakohPaymentDateListMaxCount = null;
            //西鉄決済データ表示件数分取得用リスト
            List<MiyakohPaymentInfoListEntity> SelectMiyakohPaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);
            //検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

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

            if (TransePortCheck != "-" && TransportType != "-")
            {
                if (TransePortCheck != TransportType)
                {
                    ModelState.AddModelError("", "商品種別とバス/鉄道の種別が一致しません。再選択してください。");
                    return View("~/Views/MPA0701/Index.cshtml", model);
                }
            }
            //検索条件に一致する全リスト件数取得
            MiyakohPaymentDateListMaxCount = new MiyakohPaymentModel().MiyakohPaymentDateListMaxCount(TargetDateStart, TargetDateLast, UserId, model.PaymentType, model.TicketNumType, TransportType, TicketId, AplType);

            //表示リストの総数
            int maxListCount = MiyakohPaymentDateListMaxCount.Count;

            //検索条件に一致したリストから表示件数分取得(CSV出力用リストのためリスト全件数分取得する)
            SelectMiyakohPaymentDateList = new MiyakohPaymentModel().GetMiyakohPaymentDate(TargetDateStart, TargetDateLast, PageNo, maxListCount, UserId, model.PaymentType, model.TicketNumType, TransportType, TicketId, AplType);


            //開始日時
            info.TargetDateBegin = TargetDateStart.ToString();

            //終了日時
            info.TargetDateEnd = TargetDateLast.ToString();

            //全リスト件数
            info.ListMaxCount = maxListCount;

            //現在のページ位置
            info.ListPageNo = model.ListPageNo;

            //取得したリスト件数が0以上
            if (SelectMiyakohPaymentDateList.Count > 0)
            {
                info.MiyakohPaymentInfoList = SelectMiyakohPaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                return View("~/Views/MPA0701/Index.cshtml", info);
            }


            MemoryStream MiyakohMem = new MemoryStream();
            StreamWriter Miyakohsw = new StreamWriter(MiyakohMem, System.Text.Encoding.GetEncoding("shift_jis"));
            //ヘッダー部分を書き込む
            Miyakohsw.Write("\"myroute会員ID\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"決済日時\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"決済ID\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"カテゴリー\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"商品種別\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"大人枚数\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"学割枚数\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"子供枚数\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"決済種別\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"金額\"");
            Miyakohsw.Write(',');
            Miyakohsw.Write("\"領収書番号\"");
            Miyakohsw.Write(',');
            Miyakohsw.WriteLine("\"アプリ種別\"");

            foreach (var item in info.MiyakohPaymentInfoList)
            {
                //文字列に"を追加して出力
                Miyakohsw.Write("\"" + item.UserId.ToString() + "\"");        //myrouteID
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.TranDatetime.ToString() + "\"");  //決済日時
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.PaymentId.ToString() + "\"");     //決済ID
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.TransportType.ToString() + "\"");    //チケット種別
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.TicketName.ToString() + "\"");    //商品種別
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.AdultNum.ToString() + "\"");      //大人枚数
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.discountNum.ToString() + "\"");      //学割枚数
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.ChildNum.ToString() + "\"");      //子供枚数
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.PaymentType.ToString() + "\"");   //決済種別
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.Amount.ToString() + "\"");        //金額
                Miyakohsw.Write(',');
                Miyakohsw.Write("\"" + item.ReceiptNo.ToString() + "\""); //領収書番号
                Miyakohsw.Write(',');
                Miyakohsw.WriteLine("\"" + item.Apltype.ToString() + "\""); //アプリ種別
            }
            Miyakohsw.Close();

            //出力日を取得
            DateTime NowDate = System.DateTime.Now;

            //ファイル名を「JRKyushu_Report_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
            return File(MiyakohMem.ToArray(), FILE_CONTENTTYPE, "Miyakoh_Report_" + TargetDateStart.ToString("yyyyMMdd") + "-" + TargetDateLast.ToString("yyyyMMdd") + "_" + NowDate.ToString("yyyyMMdd") + FILE_EXTENSION);
        }
    }
}
