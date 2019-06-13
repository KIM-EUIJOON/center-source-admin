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
using static AppSigmaAdmin.Models.JTXPaymentModel;

namespace AppSigmaAdmin.Controllers
{
    public class MPA0101Controller : Controller
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
                DateTime.ParseExact(s, baseDatePaturn, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private const string SESSION_SEARCH_Nishitetsu = "SESSION_SEARCH_Nishitetsu";

        /// <summary>
        /// 西鉄決済データ画面
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "西鉄決済画面")]
        public ActionResult Index(string page)
        {
            List<NishitetsuPaymentInfo> NishitetsuTicket = null;
            NishitetsuTicket = new NishitetsuPaymentModel().NishitetsuSelectList();
            NishitetsuPaymentInfoListEntity info = new NishitetsuPaymentInfoListEntity();
            //プルダウンリスト
            
            //券種プルダウン
            List<SelectListItem> TicketTypeList = new List<SelectListItem>();
            int selectflg = 0;
            
            //チケット種別プルダウン
            List<SelectListItem> TranseTypeList = new List<SelectListItem>();

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                foreach (var TicketList in NishitetsuTicket)
                {
                    string TicketName = TicketList.TicketName.ToString(); //券種名称
                    string ListTicketType = TicketList.TicketType.ToString() + "/" + TicketList.TransportType.ToString(); //券種/交通手段

                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = ListTicketType });
                }
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
                ViewBag.TicketList = TicketTypeList;

                //チケット種別プルダウン作成
                TranseTypeList.Add(new SelectListItem { Text = "バス", Value = "14" });
                TranseTypeList.Add(new SelectListItem { Text = "鉄道", Value = "10" });
                TranseTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
                ViewBag.TranseList = TranseTypeList;

                return View(info);
            }

            //セッション情報の取得
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey = (Dictionary<string, string>)Session[SESSION_SEARCH_Nishitetsu];
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
            //検索条件：券種
            string TicketType = searchKey["TicketType"];
            //検索条件：決済種別
            string PaymentType = searchKey["PaymentType"];
            //検索条件：枚数種別
            string TicketNumType = searchKey["TicketNumType"];
            //リスト件数
            int ListNum = int.Parse(searchKey["ListNum"]);
            string TicketInfo = searchKey["TicketInfo"];

            foreach (var TicketList in NishitetsuTicket)
            {
                string TicketName = TicketList.TicketName.ToString(); //券種名称
                string ListTicketType = TicketList.TicketType.ToString() + "/" + TicketList.TransportType.ToString(); //券種/交通手段

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
                TranseTypeList.Add(new SelectListItem { Text = "バス", Value = "14" });
                TranseTypeList.Add(new SelectListItem { Text = "鉄道", Value = "10" });
                TranseTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });

            ViewBag.TranseList = TranseTypeList;

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
                    info.TicketType = "-";
                    ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                    return View(info);
                }
            }
            catch(FormatException error)
            {
                Trace.TraceError(Logger.GetExceptionMessage(error));
                info.TransportType = "-";
                info.TicketType = "-";
                ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                return View(info);
            }
            int EndListNo = pageNo * ListNum;
            //ページ数から取得するリストの開始位置を指定(10件ずつのリスト)
            int ListNoBegin = EndListNo - (ListNum -1);

            List<NishitetsuPaymentInfo> SelectNishitetsuPaymentDateList = null;


            //表示情報を取得
            SelectNishitetsuPaymentDateList = new NishitetsuPaymentModel().GetNishitetsuPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, MyrouteNo, TicketType, PaymentType, TicketNumType, TransportType);

            
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
            //指定券種
            info.TicketType = TicketType;
            //チケット種別
            info.TransportType = TransportType;
            //指定決済種別
            info.PaymentType = PaymentType;
            //指定枚数種別
            info.TicketNumType = TicketNumType;
            //表示リスト件数
            info.ListNum = ListNum;
            //ドロップダウンリスト用チケット種別
            //info.TicketInfo = TicketType + "/" + TransportType;


            //取得したリスト件数が0以上
            if (SelectNishitetsuPaymentDateList.Count > 0)
            {
                info.NishitetsuPaymentInfoList = SelectNishitetsuPaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
            }

            return View(info);
        }
        /// <summary>
        /// 西鉄決済データ画面
        /// </summary>
        /// <returns>西鉄決済データ画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "西鉄決済データ画面")]
        public ActionResult Index(NishitetsuPaymentInfoListEntity model)
        {

            ViewData["message"] = "";

            //券種選択肢から交通種別を分離する
            string SearchOb = "/"; //「/」判定用
            int num;
            //選択券種
            string Tickettype = "-";
            //選択チケット種別
            string TransePortCheck = "-";       //チケット種別保存用
            string TransportType = "-";         //交通種別保存用

            if (model.TicketInfo != "-")//券種が選択済でチケット種別が未選択の場合
            {
                num = model.TicketInfo.IndexOf(SearchOb);
                Tickettype = model.TicketInfo.Substring(0, num);       //券種分離
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
            else
            {
                TransportType = model.TransportType;
            }

            //券種プルダウンリスト項目取得
            List<NishitetsuPaymentInfo> NishitetsuTicket = null;
            NishitetsuTicket = new NishitetsuPaymentModel().NishitetsuSelectList();
            //券種プルダウンリスト
            List<SelectListItem> TicketTypeList = new List<SelectListItem>();

            //券種プルダウンリスト作成(券種はチケット種別の影響を受けない)
            foreach (var TicketList in NishitetsuTicket)
            {
                string TicketName = TicketList.TicketName.ToString(); //券種名称
                string TicketType = TicketList.TicketType.ToString() + "/" + TicketList.TransportType.ToString(); //券種/交通手段

                if (model.TicketInfo != TicketType)
                {
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = TicketType });
                }
                else if (model.TicketInfo == TicketType)
                {
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = TicketType, Selected = true });
                }
            }
            if (model.TicketInfo == "-")
            {
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }
            else
            {
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }
            //券種プルダウンリストを保存
            ViewBag.TicketList = TicketTypeList;


            //チケット種別プルダウン
            List<SelectListItem> TranseTypeList = new List<SelectListItem>();

            TranseTypeList.Add(new SelectListItem { Text = "バス", Value = "14" });
            TranseTypeList.Add(new SelectListItem { Text = "鉄道", Value = "10" });
            TranseTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });

            //チケット種別ドロップダウンリストを保存
            ViewBag.TranseList = TranseTypeList;

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
                //操作なし
            }
            else {
                if (!Int32.TryParse(model.UserId.ToString(), out int i))
                {
                    ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角英数字で再入力してください。");
                    return View(model);
                }
            }

            List<NishitetsuPaymentInfo> NishitetsuPaymentDateListMaxCount = null;
            List<NishitetsuPaymentInfo> SelectNishitetsuPaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);
            //検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //10件ずつ表示する
            int ListNum = 10;
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


            if (TransePortCheck != "-" && TransportType != "-")
            {
                if (TransePortCheck != TransportType)
                {
                    ModelState.AddModelError("", "券種とバス/鉄道の種別が一致しません。再選択してください。");
                    return View(model);
                }
            }
            //検索条件に一致する全リスト件数取得
            NishitetsuPaymentDateListMaxCount = new NishitetsuPaymentModel().NishitetsuPaymentDateListMaxCount(TargetDateStart, TargetDateLast, UserId, Tickettype, model.PaymentType, model.TicketNumType, TransportType);

            //検索条件に一致したリストから表示件数分取得
            SelectNishitetsuPaymentDateList = new NishitetsuPaymentModel().GetNishitetsuPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, UserId, Tickettype, model.PaymentType, model.TicketNumType, TransportType);

            NishitetsuPaymentInfoListEntity info = new NishitetsuPaymentInfoListEntity();


            //表示リストの総数
            int maxListCount = NishitetsuPaymentDateListMaxCount.Count;

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
            //指定券種
            info.TicketType = Tickettype;

            //チケット種別
            info.TransportType = TransportType;

            //ドロップダウンリスト用チケット種別
            info.TicketInfo = model.TicketInfo;

            //指定決済種別
            info.PaymentType = model.PaymentType;

            //指定枚数種別
            info.TicketNumType = model.TicketNumType;

            //取得したリスト件数が0以上
            if (SelectNishitetsuPaymentDateList.Count > 0)
            {
                info.NishitetsuPaymentInfoList = SelectNishitetsuPaymentDateList;
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
            searchKey.Add("TransportType", TransportType);
            searchKey.Add("TicketType", Tickettype);
            searchKey.Add("TicketInfo", model.TicketInfo);
            searchKey.Add("PaymentType", model.PaymentType);
            searchKey.Add("TicketNumType", model.TicketNumType);
            searchKey.Add("ListNum", ListNum.ToString());
            Session.Add(SESSION_SEARCH_Nishitetsu, searchKey);

            return View(info);
        }
        /// <summary>
        /// 西鉄決済データダウンロード処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [SessionCheck(WindowName = "西鉄決済データ画面")]
        public ActionResult NishitetsuOutPutDate(NishitetsuPaymentInfoListEntity model)
        {

            ViewData["message"] = "";
            NishitetsuPaymentInfoListEntity info = new NishitetsuPaymentInfoListEntity();

            //券種選択肢から交通種別を分離する
            string SearchOb = "/"; //「/」判定用
            int num;
            //選択券種
            string Tickettype = "-";
            //選択チケット種別
            string TransePortCheck = "-";       //チケット種別保存用
            string TransportType = "-";         //交通種別保存用

            if (model.TicketInfo != "-")//券種が選択済でチケット種別が未選択の場合
            {
                num = model.TicketInfo.IndexOf(SearchOb);
                Tickettype = model.TicketInfo.Substring(0, num);       //券種分離
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
            else 
            {
                TransportType = model.TransportType;
            }

            //券種プルダウンリスト項目取得
            List<NishitetsuPaymentInfo> NishitetsuTicket = null;
            NishitetsuTicket = new NishitetsuPaymentModel().NishitetsuSelectList();
            //券種プルダウンリスト
            List<SelectListItem> TicketTypeList = new List<SelectListItem>();

            //券種プルダウンリスト作成(券種はチケット種別の影響を受けない)
            foreach (var TicketList in NishitetsuTicket)
            {
                string TicketName = TicketList.TicketName.ToString(); //券種名称
                string TicketType = TicketList.TicketType.ToString() + "/" + TicketList.TransportType.ToString(); //券種/交通手段

                if (model.TicketInfo != TicketType)
                {
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = TicketType });
                }
                else if (model.TicketInfo == TicketType)
                {
                    TicketTypeList.Add(new SelectListItem { Text = TicketName, Value = TicketType, Selected = true });
                }
            }
            if (model.TicketInfo == "-")
            {
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-", Selected = true });
            }
            else
            {
                TicketTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });
            }
            //券種プルダウンリストを保存
            ViewBag.TicketList = TicketTypeList;


            //チケット種別プルダウン
            List<SelectListItem> TranseTypeList = new List<SelectListItem>();

                TranseTypeList.Add(new SelectListItem { Text = "バス", Value = "14" });
                TranseTypeList.Add(new SelectListItem { Text = "鉄道", Value = "10" });
                TranseTypeList.Add(new SelectListItem { Text = "種別未選択", Value = "-" });

            //チケット種別ドロップダウンリストを保存
            ViewBag.TranseList = TranseTypeList;

            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View("~/Views/MPA0101/Index.cshtml", model);
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return View("~/Views/MPA0101/Index.cshtml", model);
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
                return View("~/Views/MPA0101/Index.cshtml", model);
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return View("~/Views/MPA0101/Index.cshtml", model);
            }
            if (string.IsNullOrEmpty(model.UserId))
            {
                //操作なし
            }
            else
            {
                if (!Int32.TryParse(model.UserId.ToString(), out int i))
                {
                    ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角数字で再入力してください。");
                    return View("~/Views/MPA0101/Index.cshtml", model);
                }
            }

            List<NishitetsuPaymentInfo> NishitetsuPaymentDateListMaxCount = null;
            List<NishitetsuPaymentInfo> SelectNishitetsuPaymentDateList = null;

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
                    ModelState.AddModelError("", "券種とバス/鉄道の種別が一致しません。再選択してください。");
                    return View("~/Views/MPA0101/Index.cshtml", model);
                }
            }
            //検索条件に一致する全リスト件数取得
            NishitetsuPaymentDateListMaxCount = new NishitetsuPaymentModel().NishitetsuPaymentDateListMaxCount(TargetDateStart, TargetDateLast, UserId, Tickettype, model.PaymentType, model.TicketNumType, TransportType);

            //表示リストの総数
            int maxListCount = NishitetsuPaymentDateListMaxCount.Count;

            //検索条件に一致したリストから表示件数分取得
            SelectNishitetsuPaymentDateList = new NishitetsuPaymentModel().GetNishitetsuPaymentDate(TargetDateStart, TargetDateLast, PageNo, maxListCount, UserId, Tickettype, model.PaymentType, model.TicketNumType, TransportType);


            //開始日時
            info.TargetDateBegin = TargetDateStart.ToString();

            //終了日時
            info.TargetDateEnd = TargetDateLast.ToString();

            //全リスト件数
            info.ListMaxCount = maxListCount;

            //現在のページ位置
            info.ListPageNo = model.ListPageNo;

            //取得したリスト件数が0以上
            if (SelectNishitetsuPaymentDateList.Count > 0)
            {
                info.NishitetsuPaymentInfoList = SelectNishitetsuPaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
                return View("~/Views/MPA0101/Index.cshtml", info);
            }


            MemoryStream NishiMem = new MemoryStream();
            StreamWriter Nishisw = new StreamWriter(NishiMem, System.Text.Encoding.GetEncoding("shift_jis"));
            //ヘッダー部分を書き込む
            Nishisw.Write("\"myroute会員ID\"");
            Nishisw.Write(',');
            Nishisw.Write("\"決済日時\"");
            Nishisw.Write(',');
            Nishisw.Write("\"決済ID\"");
            Nishisw.Write(',');
            Nishisw.Write("\"バス/鉄道\"");
            Nishisw.Write(',');
            Nishisw.Write("\"券種\"");
            Nishisw.Write(',');
            Nishisw.Write("\"大人枚数\"");
            Nishisw.Write(',');
            Nishisw.Write("\"子供枚数\"");
            Nishisw.Write(',');
            Nishisw.Write("\"決済種別\"");
            Nishisw.Write(',');
            Nishisw.Write("\"金額\"");
            Nishisw.Write(',');
            Nishisw.WriteLine("\"領収書番号\"");

            foreach (var item in info.NishitetsuPaymentInfoList)
            {
                //文字列に"を追加して出力
                Nishisw.Write("\"" + item.UserId.ToString() + "\"");        //myrouteID
                Nishisw.Write(',');
                Nishisw.Write("\"" + item.TranDatetime.ToString() + "\"");  //決済日時
                Nishisw.Write(',');
                Nishisw.Write("\"" + item.PaymentId.ToString() + "\"");     //決済ID
                Nishisw.Write(',');
                Nishisw.Write("\"" + item.TransportType.ToString() + "\"");    //チケット種別
                Nishisw.Write(',');
                Nishisw.Write("\"" + item.TicketName.ToString() + "\"");    //券種
                Nishisw.Write(',');
                Nishisw.Write("\"" + item.AdultNum.ToString() + "\"");      //大人枚数
                Nishisw.Write(',');
                Nishisw.Write("\"" + item.ChildNum.ToString() + "\"");      //子供枚数
                Nishisw.Write(',');
                Nishisw.Write("\"" + item.PaymentType.ToString() + "\"");   //決済種別
                Nishisw.Write(',');
                Nishisw.Write("\"" + item.Amount.ToString() + "\"");        //金額
                Nishisw.Write(',');
                Nishisw.WriteLine("\"" + item.ReceiptNo.ToString() + "\""); //領収書番号
            }
            Nishisw.Close();

            //出力日を取得
            DateTime NowDate = System.DateTime.Now;

            //ファイル名を「Nishitetsu_Report_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
            return File(NishiMem.ToArray(), FILE_CONTENTTYPE, "Nishitetsu_Report_" + TargetDateStart.ToString("yyyyMMdd") + "-" + TargetDateLast.ToString("yyyyMMdd") + "_" + NowDate.ToString("yyyyMMdd") + FILE_EXTENSION);
        }
    }
}