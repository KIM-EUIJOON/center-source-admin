﻿using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.ResponseData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static AppSigmaAdmin.Controllers.JTXPaymentModel;

namespace AppSigmaAdmin.Controllers
{
    public class MPA0301Controller : Controller
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
        private const string SESSION_SEARCH_Nasse = "SESSION_SEARCH_Nasse";

        /// <summary>
        /// Nasse決済データ画面
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "Nasse決済データ画面")]
        public ActionResult Index(string page)
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
        public ActionResult Index(NassePaymentInfoListEntity model)
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
    }
}