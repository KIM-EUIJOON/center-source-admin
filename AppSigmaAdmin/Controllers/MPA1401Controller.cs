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
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Repository.Database.Entity.Base;

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
        [SessionCheck(WindowName = "乗車券利用実績画面(昭和自動車様)")]
        public ActionResult Index(string page)
        {
            var UserRole = GetUserRole(Session);

            //商品種別プルダウンリスト項目取得
            var tickets = GetTicketListRespository().GetTicketList();

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                //商品種別プルダウン
                ViewBag.TicketList = CreatePDLItemsTicketList(tickets);

                return View(new MPA1401Context());
            }

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
            //検索条件：チケット情報
            string TicketInfo = searchKey["TicketInfo"];
            //検索条件：チケット種別
            string TransportType = searchKey["TransportType"];
            //検索条件：チケットID
            string TicketId = searchKey["TicketId"];
            //リスト件数
            int ListNum = int.Parse(searchKey["ListNum"]);



            //商品種別プルダウン
            ViewBag.TicketList = CreatePDLItemsTicketList(tickets, TicketInfo); // 初期選択項目

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
            var usageList = GetTicketUsageRepository().GetUsages(targetDateStart, targetDateLast, MyrouteNo, TicketId, listNo.Begin, listNo.End);



            var info = new MPA1401Context()
            {
                TargetDateBegin = TargetDateBegin, // 開始日時
                TargetDateEnd = TargetDateEnd, // 終了日時
                TransportType = TransportType, // チケット種別
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
        [SessionCheck(WindowName = "乗車券利用実績画面(昭和自動車様)")]
        public ActionResult Index(MPA1401Context model)
        {
            try
            {
                ViewData["message"] = "";

                // 権限取得
                var userRole = GetUserRole(Session);

                //商品種別プルダウンリスト項目取得
                var tickets = GetTicketListRespository().GetTicketList();

                //商品種別プルダウン
                ViewBag.TicketList = CreatePDLItemsTicketList(tickets, model.TicketInfo); // 初期選択項目

                // 入力チェック
                IsValidInput(model);



                //選択交通種別(ここでは使用していないため、"-"固定)
                const string transportType = "-";

                //選択チケットID取得
                var ticketId = GetSelectedTicket(model);

                var targetDateStart = DateTime.Parse(model.TargetDateBegin);
                var targetDateLast = DateTime.Parse(model.TargetDateEnd);

                // リスト番号範囲
                // ボタン押下で取得されるページ数は0のため1加算する
                var listNo = GetListRowNum(model.ListPageNo + 1);



                //表示リストの総数
                var maxListCount = GetTicketUsageRepository().GetUsagesMaxCount(targetDateStart, targetDateLast, model.UserId, ticketId);

                //検索条件に一致したリストから表示件数分取得
                var usageList = GetTicketUsageRepository().GetUsages(targetDateStart, targetDateLast, model.UserId, ticketId, listNo.Begin, listNo.End);

                var info = new MPA1401Context()
                {
                    TargetDateBegin = targetDateStart.ToString(), // 開始日時
                    TargetDateEnd = targetDateLast.ToString(), // 終了日時
                    TransportType = transportType, // チケット種別
                    TicketInfo = model.TicketInfo, // ドロップダウンリスト用チケット種別
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
                searchKey.Add("TransportType", transportType);
                searchKey.Add("TicketInfo", model.TicketInfo);
                searchKey.Add("TicketId", ticketId);
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
        [SessionCheck(WindowName = "乗車券利用実績画面(昭和自動車様)")]
        public ActionResult OutPutCSV(MPA1401Context model)
        {
            try
            {
                ViewData["message"] = "";

                // 権限取得
                var userRole = GetUserRole(Session);

                //商品種別プルダウンリスト項目取得
                var tickets = GetTicketListRespository().GetTicketList();

                //商品種別プルダウン
                ViewBag.TicketList = CreatePDLItemsTicketList(tickets, model.TicketInfo); // 初期選択項目

                // 入力チェック
                IsValidInput(model);


                //選択チケットID取得
                var ticketId = GetSelectedTicket(model);

                var targetDateStart = DateTime.Parse(model.TargetDateBegin);
                var targetDateLast = DateTime.Parse(model.TargetDateEnd);

                // リスト番号範囲
                // ボタン押下で取得されるページ数は0のため1加算する
                var listNo = GetListRowNum(model.ListPageNo + 1);

                //表示リストの総数
                var maxListCount = GetTicketUsageRepository().GetUsagesMaxCount(targetDateStart, targetDateLast, model.UserId, ticketId);

                //検索条件に一致したリストから表示件数分取得(CSV出力用リストのためリスト全件数分取得する)
                var usageList = GetTicketUsageRepository().GetUsages(targetDateStart, targetDateLast, model.UserId, ticketId, listNo.Begin, maxListCount);



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
                    sw.Write("\"商品\"");
                    sw.Write(',');
                    sw.Write("\"利用開始日時\"");
                    sw.Write(',');
                    sw.Write("\"利用終了日時\"");
                    sw.Write(',');
                    sw.WriteLine("\"チケットID\"");

                    foreach (var item in info.UsageReportData.ReportList)
                    {
                        //文字列に"を追加して出力
                        sw.Write("\"" + item.UserId + "\"");        //myrouteID
                        sw.Write(',');
                        sw.Write("\"" + item.TicketName + "\"");  //商品種別名
                        sw.Write(',');
                        sw.Write("\"" + item.UsageStartDatetime + "\"");  //利用開始日時
                        sw.Write(',');
                        sw.Write("\"" + item.UsageEndDatetime + "\"");     //利用終了日時
                        sw.Write(',');
                        sw.WriteLine("\"" + item.InquiryId + "\""); //チケットID
                    }
                    sw.Close();

                    //出力日を取得
                    var NowDate = System.DateTime.Now;

                    //ファイル名を「Showa-bus_Usage_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
                    return File(ms.ToArray(), FILE_CONTENTTYPE, "Showa-bus_Usage_" + targetDateStart.ToString("yyyyMMdd") + "-" + targetDateLast.ToString("yyyyMMdd") + "_" + NowDate.ToString("yyyyMMdd") + FILE_EXTENSION);
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
        /// チケットリストデータ取得
        /// </summary>
        /// <returns></returns>
        private static ShowabusTicketSaleRepository GetTicketListRespository()
        {
            return new ShowabusTicketSaleRepository();
        }

        /// <summary>
        /// 利用実績データ取得
        /// </summary>
        /// <returns></returns>
        private static ShowabusTicketUsageRepository GetTicketUsageRepository()
        {
            return new ShowabusTicketUsageRepository();
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
        /// 選択チケットID取得
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static string GetSelectedTicket(MPA1401Context model)
        {
            if (!string.IsNullOrEmpty(model.TicketInfo) && model.TicketInfo != "-")//商品種別が選択済でチケット種別が未選択の場合
            {
                //商品種別選択肢から交通種別を分離する
                var sepPos = model.TicketInfo.IndexOf("/"); //「/」判定用
                var tId = model.TicketInfo.Substring(0, sepPos);       //チケットID分離
                var trp = model.TicketInfo.Substring(sepPos + 1).ToString();  //交通種別分離

                // 選択されたチケットID
                return tId;
            }

            // 未選択
            return "-";
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
                    TicketName = t.TicketName,
                    UsageStartDatetime = t.UsageStartDatetime,
                    UsageEndDatetime = t.UsageEndDatetime,
                    InquiryId = t.InquiryId,
                };

        /// <summary>
        /// 権限取得
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private static string GetUserRole(HttpSessionStateBase session)
        {
            //セッションに保存されているユーザー情報を取得する
            var userInfo = (UserInfoAdminEntity)session[SystemConst.SESSION_USER_INFO_ADMIN];
            //現在ログイン中のUserRole取得
            return userInfo.Role.ToString();
        }

        /// <summary>
        /// 商品プルダウンリスト生成
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        private static List<SelectListItem> CreatePDLItemsTicketList(IEnumerable<ShowabusPaymentInfo> infos, string selected = null)
            => BuildDropDownList(infos,
                    s => s.TicketName, // 商品種別名称
                    s => $"{s.TicketId}/{s.TransportType}", //チケットID/交通手段
                    selected);

        /// <summary>
        /// ドロップダウンリスト生成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sources"></param>
        /// <param name="textF">Textプロパティ設定値生成関数</param>
        /// <param name="valueF">Valueプロパティ設定値生成関数</param>
        /// <param name="needNoSelectedItem">未選択項目有無(true: 有, false: 無)</param>
        /// <param name="selectedValue">初期選択項目Value値</param>
        /// <returns></returns>
        private static List<SelectListItem> BuildDropDownList<T>(IEnumerable<T> sources, Func<T, string> textF, Func<T, string> valueF, bool needNoSelectedItem, string selectedValue)
        {
            var results = new List<SelectListItem>();
            var itemNoSelected = new SelectListItem { Text = "種別未選択", Value = "-" };

            // 渡された要素をリスト項目化
            results.AddRange(sources.Select(s => new SelectListItem() { Text = textF(s), Value = valueF(s) }));

            // 未選択用アイテムの追加有無
            if (needNoSelectedItem)
                results.Add(itemNoSelected);

            // 初期選択の設定
            return results.Select(r => r.Value != (selectedValue ?? itemNoSelected.Value) ? r : new SelectListItem() { Text = r.Text, Value = r.Value, Selected = true }).ToList();
        }
        private static List<SelectListItem> BuildDropDownList<T>(IEnumerable<T> sources, Func<T, string> textF, Func<T, string> valueF)
            => BuildDropDownList(sources, textF, valueF, true, null);
        private static List<SelectListItem> BuildDropDownList<T>(IEnumerable<T> sources, Func<T, string> textF, Func<T, string> valueF, string selectedValue)
            => BuildDropDownList(sources, textF, valueF, true, selectedValue);

    }
}