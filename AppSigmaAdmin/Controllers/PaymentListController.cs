using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.ResponseData;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static AppSigmaAdmin.Controllers.JTXPaymentModel;


namespace AppSigmaAdmin.Controllers
{


    public class PaymentListController : Controller
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
        public ActionResult JapanTaxi(string page)
        {
            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                return View();
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

            DateTime TargetDateStart = DateTime.Parse(TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(TargetDateEnd);

            //ページ数から取得するリストの終了位置を指定(10件ずつのリスト)
            int pageNo = int.Parse(page);
            int EndListNo = pageNo * 10;
            //ページ数から取得するリストの開始位置を指定(10件ずつのリスト)
            int ListNoBegin = EndListNo - 9;

            List<JtxPaymentInfo> SelectPaymentDateList = null;

            //表示情報を取得
            SelectPaymentDateList = new JTXPaymentModel().GetJtxPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo);

            JtxPaymentInfoListEntity info = new JtxPaymentInfoListEntity();
            int ListCount = int.Parse(maxListCount);
            

            //開始日時
            info.TargetDateBegin = TargetDateBegin;
            //終了日時
            info.TargetDateEnd = TargetDateEnd;
            //全リスト件数
            info.ListMaxCount = ListCount;
            //現在のページ位置
            info.ListPageNo = pageNo;

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
        public ActionResult JapanTaxi(JtxPaymentInfoListEntity model)
        {
            
            ViewData["message"] = "";

            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View();
            }
            else if(string.IsNullOrEmpty(model.TargetDateEnd))
            {
                ModelState.AddModelError("", "表示期間の終了年月日を指定してください");
                return View();
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。再入力してください。");
                return View();
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。再入力してください。");
                return View();
            }

            List<JtxPaymentInfo> PaymentDateListMaxCount = null;
            List<JtxPaymentInfo> SelectPaymentDateList = null;
           
            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);
            //検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1 ;
            
            //10件ずつ表示する
            int EndListNo = PageNo * 10;

            //検索条件に一致する全リスト件数取得
            PaymentDateListMaxCount = new JTXPaymentModel().GetJtxPaymentDateListCount(TargetDateStart, TargetDateLast);
            
            //検索条件に一致したリストから表示件数分取得
            SelectPaymentDateList = new JTXPaymentModel().GetJtxPaymentDate(TargetDateStart, TargetDateLast, PageNo, EndListNo);
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

            //取得したリスト件数が0以上
            if (SelectPaymentDateList.Count >0)
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
            searchKey.Add("maxListCount", maxListCount.ToString());
            Session.Add(SESSION_SEARCH_JapanTaxi, searchKey);

            return View(info);
        }

        [HttpPost]
        [SessionCheck(WindowName = "Japantaxi決済データ画面")]
        public ActionResult JtxOutPutDate(JtxPaymentInfoListEntity model)
        {

            ViewData["message"] = "";

            List<JtxPaymentInfo> PaymentDateListMaxCount = null;
            List<JtxPaymentInfo> SelectPaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);

            //検索条件に一致する全リスト件数取得
            PaymentDateListMaxCount = new JTXPaymentModel().GetJtxPaymentDateListCount(TargetDateStart, TargetDateLast);

            //ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //表示リストの総数
            int maxListCount = PaymentDateListMaxCount.Count;

            //検索条件に一致したリストから表示件数分取得
            SelectPaymentDateList = new JTXPaymentModel().GetJtxPaymentDate(TargetDateStart, TargetDateLast, PageNo, maxListCount);
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
            }


            MemoryStream JtxMem = new MemoryStream();
            StreamWriter Jtxsw = new StreamWriter(JtxMem,System.Text.Encoding.GetEncoding("shift_jis"));
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
            Jtxsw.WriteLine("\"利用金額\"");
            
            foreach (var item in info.JtxPaymentInfoList)
            {
                //文字列に"を追加して出力
                Jtxsw.Write("\"" + item.UserId.ToString()+ "\"");
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
                Jtxsw.WriteLine("\"" + item.Amount.ToString() + "\"");
            }
            Jtxsw.Close();

            //ファイル名を「決済データ検索開始日-終了日」で出力
            return File(JtxMem.ToArray(), FILE_CONTENTTYPE, "決済データ" + model.TargetDateBegin +"-"+model.TargetDateEnd+ FILE_EXTENSION);
        }

          private const string SESSION_SEARCH_Nasse = "SESSION_SEARCH_Nasse";

          /// <summary>
          /// Nasse決済データ画面
          /// </summary>
          /// <returns>ログイン画面</returns>
          [SessionCheck(WindowName = "Nasse決済データ画面")]
          public ActionResult Nasse(string page)
          {
            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                return View();
            }

            //セッション情報の取得
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey = (Dictionary<string, string>)Session[SESSION_SEARCH_Nasse];
            //検索条件：開始日時
            string TargetDateBegin = searchKey["TargetDateBegin"];
            //検索条件：終了日時
            string TargetDateEnd = searchKey["TargetDateEnd"];
            //リスト全件数
            string maxListCount = searchKey["maxListCount"];
            //パスポートID
            string PassID = searchKey["PassportID"];

            DateTime TargetDateStart = DateTime.Parse(TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(TargetDateEnd);

            //ページ数から取得するリストの終了位置を指定(10件ずつのリスト)
            int pageNo = int.Parse(page);
            int EndListNo = pageNo * 10;
            //ページ数から取得するリストの開始位置を指定(10件ずつのリスト)
            int ListNoBegin = EndListNo - 9;

            List<NassePaymentInfo> SelectNassePaymentDateList = null;

            //表示情報を取得
            SelectNassePaymentDateList = new NassePaymentModel().GetNassePaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, PassID);

            NassePaymentInfoListEntity info = new NassePaymentInfoListEntity();
            int ListCount = int.Parse(maxListCount);


            //開始日時
            info.TargetDateBegin = TargetDateBegin;
            //終了日時
            info.TargetDateEnd = TargetDateEnd;
            //全リスト件数
            info.ListMaxCount = ListCount;
            //現在のページ位置
            info.ListPageNo = pageNo;
            //指定パスポートID
            info.PassportID = PassID;

            //取得したリスト件数が0以上
            if (SelectNassePaymentDateList.Count > 0)
            {
                info.NassePaymentInfoList = SelectNassePaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
            }

            return View(info);
        }
        /// <summary>
        /// Nasse決済データ画面
        /// </summary>
        /// <returns>Nasse決済データ画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "Nasse決済データ画面")]
        public ActionResult Nasse(NassePaymentInfoListEntity model)
        {
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
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。再入力してください。");
                return View();
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。再入力してください。");
                return View();
            }

            List<NassePaymentInfo> NassePaymentDateListMaxCount = null;
            List<NassePaymentInfo> SelectNassePaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);
            //検索ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //10件ずつ表示する
            int EndListNo = PageNo * 10;
            int ListNoBegin = EndListNo - 9;
            string PassID ;

            if (string.IsNullOrEmpty(model.PassportID))
            {
                PassID ="-";
            }
            else
            {
                //選択されているパスポートIDを保存する
                PassID = model.PassportID;
            }

            //検索条件に一致する全リスト件数取得
            NassePaymentDateListMaxCount = new NassePaymentModel().NassePaymentDateListMaxCount(TargetDateStart, TargetDateLast, PassID);

            //検索条件に一致したリストから表示件数分取得
            SelectNassePaymentDateList = new NassePaymentModel().GetNassePaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, PassID);
            NassePaymentInfoListEntity info = new NassePaymentInfoListEntity();

            //表示リストの総数
            int maxListCount = NassePaymentDateListMaxCount.Count;

            //開始日時
            info.TargetDateBegin = TargetDateStart.ToString();

            //終了日時
            info.TargetDateEnd = TargetDateLast.ToString();

            //全リスト件数
            info.ListMaxCount = maxListCount;

            //現在のページ位置
            info.ListPageNo = model.ListPageNo;

            info.PassportID = model.PassportID;

            //取得したリスト件数が0以上
            if (SelectNassePaymentDateList.Count > 0)
            {
                info.NassePaymentInfoList = SelectNassePaymentDateList;
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
            searchKey.Add("PassportID", model.PassportID);
            Session.Add(SESSION_SEARCH_Nasse, searchKey);

            return View(info);
        }

        [HttpPost]
        [SessionCheck(WindowName = "Nasse決済データ画面")]
        public ActionResult NasseOutPutDate(NassePaymentInfoListEntity model)
        {

            ViewData["message"] = "";

            List<NassePaymentInfo> NassePaymentDateListMaxCount = null;
            List<NassePaymentInfo> SelectNassePaymentDateList = null;

            DateTime TargetDateStart = DateTime.Parse(model.TargetDateBegin);
            DateTime TargetDateLast = DateTime.Parse(model.TargetDateEnd);

            string PassID;

            if (string.IsNullOrEmpty(model.PassportID))
            {
                PassID = "-";
            }
            else
            {
                //選択されているパスポートIDを保存する
                PassID = model.PassportID;
            }

            //検索条件に一致する全リスト件数取得
            NassePaymentDateListMaxCount = new NassePaymentModel().NassePaymentDateListMaxCount(TargetDateStart, TargetDateLast, PassID);

            //ボタン押下で取得されるページ数は0のため1加算する
            int PageNo = model.ListPageNo + 1;

            //表示リストの総数
            int maxListCount = NassePaymentDateListMaxCount.Count;

            //検索条件に一致したリストを取得
            SelectNassePaymentDateList = new NassePaymentModel().GetNassePaymentDate(TargetDateStart, TargetDateLast, PageNo, maxListCount, PassID);
            NassePaymentInfoListEntity info = new NassePaymentInfoListEntity();

            //開始日時
            info.TargetDateBegin = TargetDateStart.ToString();

            //終了日時
            info.TargetDateEnd = TargetDateLast.ToString();

            //全リスト件数
            info.ListMaxCount = maxListCount;

            //現在のページ位置
            info.ListPageNo = model.ListPageNo;

            //取得したリスト件数が0以上
            if (SelectNassePaymentDateList.Count > 0)
            {
                info.NassePaymentInfoList = SelectNassePaymentDateList;
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
            }


            MemoryStream NasseMem = new MemoryStream();
            StreamWriter Nassesw = new StreamWriter(NasseMem, System.Text.Encoding.GetEncoding("shift_jis"));
            //ヘッダー部分を書き込む
            Nassesw.Write("\"myroute会員ID\"");
            Nassesw.Write(',');
            Nassesw.Write("\"決済日\"");
            Nassesw.Write(',');
            Nassesw.Write("\"決済ID\"");
            Nassesw.Write(',');
            Nassesw.Write("\"パスポートID\"");
            Nassesw.Write(',');
            Nassesw.Write("\"パスポート名\"");
            Nassesw.Write(',');
            Nassesw.Write("\"決済種別\"");
            Nassesw.Write(',');
            Nassesw.WriteLine("\"金額\"");

            foreach (var item in info.NassePaymentInfoList)
            {
                //文字列に"を追加して出力
                Nassesw.Write("\"" + item.UserId.ToString() + "\"");        //myrouteID
                Nassesw.Write(',');
                Nassesw.Write("\"" + item.TranDatetime.ToString() + "\"");  //決済日
                Nassesw.Write(',');
                Nassesw.Write("\"" + item.PaymentId.ToString() + "\"");     //決済ID
                Nassesw.Write(',');
                Nassesw.Write("\"" + item.PassportID.ToString() + "\"");    //パスポートID
                Nassesw.Write(',');
                Nassesw.Write("\"" + item.PassportName.ToString() + "\"");  //パスポート名
                Nassesw.Write(',');
                Nassesw.Write("\"" + item.PaymentType.ToString() + "\"");   //決済種別
                Nassesw.Write(',');
                Nassesw.WriteLine("\"" + item.Amount.ToString() + "\"");    //金額
            }
            Nassesw.Close();

            //ファイル名を「決済データ検索開始日-終了日」で出力
            return File(NasseMem.ToArray(), FILE_CONTENTTYPE, "決済データ_" + model.TargetDateBegin + "-" + model.TargetDateEnd + FILE_EXTENSION);
        }


        private const string SESSION_SEARCH_Nishitetsu = "SESSION_SEARCH_Nishitetsu";

        /// <summary>
        /// 西鉄決済データ画面
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "西鉄決済画面")]
        public ActionResult Nishitetsu(string page)
        {
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
        public ActionResult Nishitetsu(NishitetsuPaymentInfoListEntity model)
        {
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
                ModelState.AddModelError("", "表示期間の開始年月日が正しくありません。再入力してください。");
                return View();
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "表示期間の終了年月日が正しくありません。再入力してください。");
                return View();
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
            SelectNishitetsuPaymentDateList = new NishitetsuPaymentModel().GetNishitetsuPaymentDate(TargetDateStart, TargetDateLast, ListNoBegin, EndListNo, UserId,model.TicketType,model.PaymentType,model.TicketNumType);
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

            //ファイル名を「決済データ検索開始日-終了日」で出力
            return File(NishiMem.ToArray(), FILE_CONTENTTYPE, "決済データ_" + model.TargetDateBegin + "-" + model.TargetDateEnd + FILE_EXTENSION);
        }
    }
}
