using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.ResponseData;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static AppSigmaAdmin.Controllers.JTXPaymentModel;

namespace AppSigmaAdmin.Controllers
{
    public class MPA0101Controller : Controller
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

        private const string SESSION_SEARCH_Nishitetsu = "SESSION_SEARCH_Nishitetsu";

        /// <summary>
        /// 西鉄決済データ画面
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "西鉄決済画面")]
        public ActionResult Index(string page)
        {
#if DEBUG
            ViewBag.Debug = 1;
#else
            ViewBag.Debug = 0;
#endif
            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                return View();
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
            //検索条件：券種
            string TicketType = searchKey["TicketType"];
            //検索条件：決済種別
            string PaymentType = searchKey["PaymentType"];
            //検索条件：枚数種別
            string TicketNumType = searchKey["TicketNumType"];


            DateTime TargetDateStart = DateTime.Parse(TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(TargetDateEnd);

            //ページ数から取得するリストの終了位置を指定(10件ずつのリスト)
            int pageNo = int.Parse(page);
            int EndListNo = pageNo * 10;
            //ページ数から取得するリストの開始位置を指定(10件ずつのリスト)
            int ListNoBegin = EndListNo - 9;

            List<NishitetsuPaymentInfo> SelectNishitetsuPaymentDateList = null;

            //表示情報を取得
            SelectNishitetsuPaymentDateList = new NishitetsuPaymentModel().GetNishitetsuPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, MyrouteNo, TicketType, PaymentType, TicketNumType);

            NishitetsuPaymentInfoListEntity info = new NishitetsuPaymentInfoListEntity();
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
            //指定決済種別
            info.PaymentType = PaymentType;
            //指定枚数種別
            info.TicketNumType = TicketNumType;


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
#if DEBUG
            ViewBag.Debug = 1;
#else
            ViewBag.Debug = 0;
#endif
            ViewData["message"] = "";

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
                return View();
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return View();
            }
            if (string.IsNullOrEmpty(model.UserId))
            {
                //操作なし
            }
            else {
                if (!Int32.TryParse(model.UserId.ToString(), out int i))
                {
                    ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角英数字で再入力してください。");
                    return View();
                }
            }

            List<NishitetsuPaymentInfo> NishitetsuPaymentDateListMaxCount = null;
            List<NishitetsuPaymentInfo> SelectNishitetsuPaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);
            //検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //10件ずつ表示する
            int EndListNo = PageNo * 10;
            int ListNoBegin = EndListNo - 9;
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
            NishitetsuPaymentDateListMaxCount = new NishitetsuPaymentModel().NishitetsuPaymentDateListMaxCount(TargetDateStart, TargetDateLast, UserId, model.TicketType, model.PaymentType, model.TicketNumType);

            //検索条件に一致したリストから表示件数分取得
            SelectNishitetsuPaymentDateList = new NishitetsuPaymentModel().GetNishitetsuPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, UserId, model.TicketType, model.PaymentType, model.TicketNumType);
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
            searchKey.Add("TicketType", model.TicketType);
            searchKey.Add("PaymentType", model.PaymentType);
            searchKey.Add("TicketNumType", model.TicketNumType);
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

            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View("~/Views/MPA0101/Index.cshtml");
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return View("~/Views/MPA0101/Index.cshtml");
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
                return View("~/Views/MPA0101/Index.cshtml");
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
                return View("~/Views/MPA0101/Index.cshtml");
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
                    return View("~/Views/MPA0101/Index.cshtml");
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

            //検索条件に一致する全リスト件数取得
            NishitetsuPaymentDateListMaxCount = new NishitetsuPaymentModel().NishitetsuPaymentDateListMaxCount(TargetDateStart, TargetDateLast, UserId, model.TicketType, model.PaymentType, model.TicketNumType);

            //表示リストの総数
            int maxListCount = NishitetsuPaymentDateListMaxCount.Count;

            //検索条件に一致したリストから表示件数分取得
            SelectNishitetsuPaymentDateList = new NishitetsuPaymentModel().GetNishitetsuPaymentDate(TargetDateStart, TargetDateLast, PageNo, maxListCount, UserId, model.TicketType, model.PaymentType, model.TicketNumType);
            NishitetsuPaymentInfoListEntity info = new NishitetsuPaymentInfoListEntity();

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
                return View("~/Views/MPA0101/Index.cshtml");
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
                Nishisw.Write("\"" + item.TicketType.ToString() + "\"");    //券種
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