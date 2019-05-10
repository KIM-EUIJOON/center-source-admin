using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.ResponseData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult Index(string page)
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
        public ActionResult Index(JtxPaymentInfoListEntity model)
        {

            ViewData["message"] = "";

            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                ModelState.AddModelError("", "表示期間の開始年月日を指定してください");
                return View();
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
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
            int PageNo = model.ListPageNo + 1;

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
            Jtxsw.WriteLine("\"利用金額\"");

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
                Jtxsw.WriteLine("\"" + item.Amount.ToString() + "\"");
            }
            Jtxsw.Close();

            //ファイル名を「決済データ検索開始日-終了日」で出力
            return File(JtxMem.ToArray(), FILE_CONTENTTYPE, "決済データ" + model.TargetDateBegin + "-" + model.TargetDateEnd + FILE_EXTENSION);
        }
    }
}