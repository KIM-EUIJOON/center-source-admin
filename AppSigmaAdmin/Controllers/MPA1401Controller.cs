using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Exceptions;
using AppSigmaAdmin.Repository.Showabus;

namespace AppSigmaAdmin.Controllers
{
    public class MPA1401Controller : Controller
    {    
        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";

        // セッションキー
        private const string SESSION_SEARCH_MPA1401 = "SESSION_SEARCH_MPA1401";

        // 一覧ページ単位の表示件数
        private const int LISTMAXROW = 10;

        // ルートパス
        private const string CONTENTROOT = "~/Views/MPA1401";

        /// <summary>
        /// 決済データ画面(初期, ページ遷移)
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "保険付き乗車券利用実績画面")]
        public ActionResult Index(string page)
        {
            //初回Null判定
            if (string.IsNullOrEmpty(page))
                return View(new MPA1401Context());

            //セッション情報の取得
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey = (Dictionary<string, string>)Session[SESSION_SEARCH_MPA1401];
            //検索条件：開始日時
            string TargetDateBegin = searchKey["TargetDateBegin"];
            //検索条件：終了日時
            string TargetDateEnd = searchKey["TargetDateEnd"];
            //リスト全件数
            string maxListCount = searchKey["maxListCount"];
            //検索条件：Myroute番号
            string MyrouteNo = searchKey["MyrouteNo"];
            //リスト件数
            int ListNum = int.Parse(searchKey["ListNum"]);



            var targetDateStart = DateTime.Parse(TargetDateBegin);
            var targetDateLast = DateTime.Parse(TargetDateEnd);

            //ページ数から取得するリストの終了位置を指定(10件ずつのリスト)
            var pageNo = 0;
            try
            {
                pageNo = int.Parse(page);
                float ListMaxPage = (float)(float.Parse(maxListCount) / (float)ListNum);
                int ListMaxPageNum = (int)Math.Ceiling(ListMaxPage);

                //直接入力されたページ数が存在しない場合
                if (pageNo > ListMaxPageNum)
                {
                    ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                    return View(new MPA1401Context());
                }
            }
            catch (FormatException error)
            {
                Trace.TraceError(Logger.GetExceptionMessage(error));
                ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                return View(new MPA1401Context());
            }

            // リスト番号範囲
            var listNo = GetListRowNum(pageNo);

            //表示情報を取得
            var usageList = new ShowabusTicketUsageRepository().GetUsages(targetDateStart, targetDateLast, MyrouteNo, listNo.Begin, listNo.End);



            var info = new MPA1401Context()
            {
                TargetDateBegin = TargetDateBegin, // 開始日時
                TargetDateEnd = TargetDateEnd, // 終了日時
                ListPageNo = pageNo, // 現在のページ位置
                UserId = MyrouteNo, // 指定MyrouteID
            };

            //取得したリスト件数が0以上
            if (usageList.Any())
            {
                info.UsageReportData = new MPA1401ReportData()
                {
                    ListMaxCount = int.Parse(maxListCount),
                    ListNum = ListNum,
                    ReportList = usageList.Select(ToMPA1401ReportDataRow()).ToList(),
                };
            }
            else
            {
                ModelState.AddModelError("", "一致する決済データがありませんでした。");
            }

            return View(info);
        }

        /// <summary>
        /// 決済データ画面(検索)
        /// </summary>
        /// <returns>決済データ画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "保険付き乗車券利用実績画面")]
        public ActionResult Index(MPA1401Context model)
        {
            try
            {
                ViewData["message"] = "";

                // 入力チェック
                IsValidInput(model);



                var targetDateStart = DateTime.Parse(model.TargetDateBegin);
                var targetDateLast = DateTime.Parse(model.TargetDateEnd);

                // リスト番号範囲
                // ボタン押下で取得されるページ数は0のため1加算する
                var listNo = GetListRowNum(model.ListPageNo + 1);



                //表示リストの総数
                var maxListCount = new ShowabusTicketUsageRepository().GetUsagesMaxCount(targetDateStart, targetDateLast, model.UserId);

                //検索条件に一致したリストから表示件数分取得
                var usageList = new ShowabusTicketUsageRepository().GetUsages(targetDateStart, targetDateLast, model.UserId, listNo.Begin, listNo.End);

                var info = new MPA1401Context()
                {
                    TargetDateBegin = targetDateStart.ToString(), // 開始日時
                    TargetDateEnd = targetDateLast.ToString(), // 終了日時
                    ListPageNo = model.ListPageNo, // 現在のページ位置
                };

                //取得したリスト件数が0以上
                if (usageList.Any())
                {
                    info.UsageReportData = new MPA1401ReportData()
                    {
                        ListMaxCount = maxListCount,
                        ListNum = LISTMAXROW,
                        ReportList = usageList.Select(ToMPA1401ReportDataRow()).ToList(),
                    };
                }
                else
                {
                    ModelState.AddModelError("", "一致する利用データがありませんでした。");
                }

                //ページ切り替え時用に検索条件を保存する
                Dictionary<string, string> searchKey = new Dictionary<string, string>();
                searchKey.Add("TargetDateBegin", model.TargetDateBegin);
                searchKey.Add("TargetDateEnd", model.TargetDateEnd);
                searchKey.Add("maxListCount", maxListCount.ToString());
                searchKey.Add("MyrouteNo", model.UserId);
                searchKey.Add("ListNum", LISTMAXROW.ToString());
                Session.Add(SESSION_SEARCH_MPA1401, searchKey);

                return View(info);
            }
            // 入力エラー
            catch (InvalidInputException iie) 
            {
                ModelState.AddModelError("", iie.Message);
                return View(model);
            }
        }

        /// <summary>
        /// 決済データダウンロード処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [SessionCheck(WindowName = "保険付き乗車券利用実績画面")]
        public ActionResult OutPutCSV(MPA1401Context model)
        {
            try
            {
                ViewData["message"] = "";

                // 入力チェック
                IsValidInput(model);


                var targetDateStart = DateTime.Parse(model.TargetDateBegin);
                var targetDateLast = DateTime.Parse(model.TargetDateEnd);

                // リスト番号範囲
                // ボタン押下で取得されるページ数は0のため1加算する
                var listNo = GetListRowNum(model.ListPageNo + 1);

                //表示リストの総数
                var maxListCount = new ShowabusTicketUsageRepository().GetUsagesMaxCount(targetDateStart, targetDateLast, model.UserId);

                //検索条件に一致したリストから表示件数分取得(CSV出力用リストのためリスト全件数分取得する)
                var usageList = new ShowabusTicketUsageRepository().GetUsages(targetDateStart, targetDateLast, model.UserId, listNo.Begin, maxListCount);



                var info = new MPA1401Context()
                {
                    TargetDateBegin = targetDateStart.ToString(), // 開始日時
                    TargetDateEnd = targetDateLast.ToString(), // 終了日時
                    ListPageNo = model.ListPageNo, // 現在のページ位置
                };

                //取得したリスト件数が0以上
                if (usageList.Any())
                {
                    info.UsageReportData = new MPA1401ReportData()
                    {
                        ListMaxCount = maxListCount,
                        ReportList = usageList.Select(ToMPA1401ReportDataRow()).ToList(),
                    };
                }
                else
                {
                    ModelState.AddModelError("", "一致する利用データがありませんでした。");
                    return View($"{CONTENTROOT}/Index.cshtml", info);
                }


                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms, System.Text.Encoding.GetEncoding("shift_jis")))
                {
                    //ヘッダー部分を書き込む
                    sw.Write("\"myroute会員ID\"");
                    sw.Write(',');
                    sw.Write("\"利用開始日時\"");
                    sw.Write(',');
                    sw.Write("\"利用終了日時\"");
                    sw.Write(',');
                    sw.WriteLine("\"問い合わせID\"");

                    foreach (var item in info.UsageReportData.ReportList)
                    {
                        //文字列に"を追加して出力
                        sw.Write("\"" + item.UserId + "\"");        //myrouteID
                        sw.Write(',');
                        sw.Write("\"" + item.UsageStartDatetime + "\"");  //利用開始日時
                        sw.Write(',');
                        sw.Write("\"" + item.UsageEndDatetime + "\"");     //利用終了日時
                        sw.Write(',');
                        sw.WriteLine("\"" + item.InquiryId + "\""); //問い合わせID
                    }
                    sw.Close();

                    //出力日を取得
                    var NowDate = System.DateTime.Now;

                    //ファイル名を「Showa_Report_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
                    return File(ms.ToArray(), FILE_CONTENTTYPE, "Insurance_Report_" + targetDateStart.ToString("yyyyMMdd") + "-" + targetDateLast.ToString("yyyyMMdd") + "_" + NowDate.ToString("yyyyMMdd") + FILE_EXTENSION);
                }
            }
            // 入力エラー
            catch(InvalidInputException iie)
            {
                ModelState.AddModelError("", iie.Message);
                return View($"{CONTENTROOT}/Index.cshtml", model);
            }
        }



        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="model"></param>
        /// <returns>IsValid: 結果(true: 正常, false: 異常), ErrMsg: エラーメッセージ(結果がfalse時のみ)</returns>
        private static void IsValidInput(MPA1401Context model)
        {
            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                //検索期間(開始)が未入力の場合
                throw new InvalidInputException("表示期間の開始年月日を指定してください");
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                //検索期間(終了)が未入力の場合
                throw new InvalidInputException("表示期間の終了年月日を指定してください");
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                //検索期間(開始)のテキストボックスに日付として正しくない値が入力された場合
                throw new InvalidInputException("表示期間の開始年月日が正しくありません。半角英数字で再入力してください。");
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                //検索期間(終了)のテキストボックスに日付として正しくない値が入力された場合
                throw new InvalidInputException("表示期間の終了年月日が正しくありません。半角英数字で再入力してください。");
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
                    throw new InvalidInputException("myroute会員IDに誤った数値が入力されました。半角数字で再入力してください。");
                }
                catch
                {
                    //myrouteIDのテキストボックスに半角数字以外が入力された場合
                    throw new InvalidInputException("myroute会員IDが数字以外で入力されました。半角英数字で再入力してください。");
                }
            }

        }

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
        /// 取得リスト番号算出
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Begin: 開始番号, End: 終了番号</returns>
        private static (int Begin, int End) GetListRowNum(MPA1401Context model)
            => GetListRowNum(model.ListPageNo);

        /// <summary>
        /// 取得リスト番号算出
        /// </summary>
        /// <param name="listPageNo"></param>
        /// <returns>Begin: 開始番号, End: 終了番号</returns>
        private static (int Begin, int End) GetListRowNum(int pageNo)
        {            
            //表示開始位置を算出
            var end = pageNo * LISTMAXROW;
            var begin = end - (LISTMAXROW - 1);

            return (begin, end);
        }

        /// <summary>
        /// 検索結果リストデータ変換
        /// </summary>
        /// <returns></returns>
        private static Func<ShowabusUsageInsuranceInfo, MPA1401ReportDataRow> ToMPA1401ReportDataRow() =>
            t =>
                new MPA1401ReportDataRow()
                {
                    UserId = t.UserId,
                    UsageStartDatetime = t.UsageStartDatetime,
                    UsageEndDatetime = t.UsageEndDatetime,
                    InquiryId = t.InquiryId,
                };
   }
}