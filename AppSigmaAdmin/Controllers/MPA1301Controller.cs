using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Exceptions;
using AppSigmaAdmin.Repository.Showabus;
using AppSigmaAdmin.ResponseData.Extensions;
using AppSigmaAdmin.Contstants;

namespace AppSigmaAdmin.Controllers
{
    public class MPA1301Controller : Controller
    {    
        //ファイル出力関連
        private string FILE_CONTENTTYPE = "text/comma-separated-values";
        private string FILE_EXTENSION = ".csv";

        // セッションキー
        private const string SESSION_SEARCH_MPA1301 = "SESSION_SEARCH_MPA1301";

        // 一覧ページ単位の表示件数
        private const int LISTMAXROW = 10;

        // ルートパス
        private const string CONTENTROOT = "~/Views/MPA1301";

        // ドロップダウンリスト項目 - アプリ種別
        private readonly IEnumerable<(string Text, string Value)> ddlAplType = new[]
        {
            ("au", "1"),
        };

        // ドロップダウンリスト項目 - 枚数種別
        private readonly IEnumerable<TicketNumType> ddlTicketNumType = new[]
        {
            TicketNumType.Adult,
            TicketNumType.Child,
        };

        /// <summary>
        /// 決済データ画面(初期, ページ遷移)
        /// </summary>
        /// <returns>ログイン画面</returns>
        [SessionCheck(WindowName = "昭和自動車決済画面")]
        public ActionResult Index(string page)
        {
            // 権限取得
            var UserRole = GetUserRole(Session);

            //商品種別プルダウンリスト項目取得
            var tickets = GetTicketListRepository().GetTicketList();

            //初回Null判定
            if (string.IsNullOrEmpty(page))
            {
                //商品種別プルダウン
                ViewBag.TicketList = CreatePDLItemsTicketList(tickets); //チケットID/交通手段

                //アプリ種別プルダウン
                ViewBag.AplList = CreatePDLItemsAplList(UserRole);

                //決済種別プルダウン
                ViewBag.PaymentTypeList = CreatePDLItemsPaymentTypeList();

                //枚数種別プルダウン
                ViewBag.TicketNumTypeList = CreatePDLItemsTicketNumTypeList();

                return View(new MPA1301Context());
            }

            //セッション情報の取得
            Dictionary<string, string> searchKey = new Dictionary<string, string>();
            searchKey = (Dictionary<string, string>)Session[SESSION_SEARCH_MPA1301];
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

            //商品種別プルダウン
            ViewBag.TicketList = CreatePDLItemsTicketList(tickets, TicketInfo); // 初期選択項目

            //アプリ種別プルダウン
            ViewBag.AplList = CreatePDLItemsAplList(UserRole, AplType);

            //決済種別プルダウン
            ViewBag.PaymentTypeList = CreatePDLItemsPaymentTypeList(PaymentType);

            //枚数種別プルダウン
            ViewBag.TicketNumTypeList = CreatePDLItemsTicketNumTypeList(TicketNumType);


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
                    return View(new MPA1301Context()
                    {
                        TransportType = "-",
                        TicketId = "-",
                    });
                }
            }
            catch (FormatException error)
            {
                Trace.TraceError(Logger.GetExceptionMessage(error));
                ModelState.AddModelError("", "誤ったページ番号にアクセスされました。");
                return View(new MPA1301Context()
                {
                    TransportType = "-",
                    TicketId = "-",
                });
            }

            // リスト番号範囲
            var listNo = GetListRowNum(pageNo);

            //表示情報を取得
            var paymentList = GetTicketPaymentRepository().GetPayments(targetDateStart, targetDateLast, MyrouteNo, PaymentType, TicketNumType, TransportType, TicketId, AplType, listNo.Begin, listNo.End);



            var info = new MPA1301Context()
            {
                TargetDateBegin = TargetDateBegin, // 開始日時
                TargetDateEnd = TargetDateEnd, // 終了日時
                ListPageNo = pageNo, // 現在のページ位置
                UserId = MyrouteNo, // 指定MyrouteID
                TicketId = TicketId, // 指定チケットID
                TransportType = TransportType, // チケット種別
                PaymentType = PaymentType, // 指定決済種別
                TicketNumType = TicketNumType, // 指定枚数種別
                Apltype = AplType // アプリ種別
            };

            //取得したリスト件数が0以上
            if (paymentList.Any())
            {
                info.PaymentReportData = new MPA1301ReportData()
                {
                    ListMaxCount = int.Parse(maxListCount),
                    ListNum = ListNum,
                    ReportList = paymentList.Select(ToMPA1301ReportDataRow()).ToList(),
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
        [SessionCheck(WindowName = "昭和自動車決済データ画面")]
        public ActionResult Index(MPA1301Context model)
        {
            try
            {
                ViewData["message"] = "";

                // 権限取得
                var userRole = GetUserRole(Session);

                //商品種別プルダウンリスト項目取得
                var tickets = GetTicketListRepository().GetTicketList();

                //商品種別プルダウン
                ViewBag.TicketList = CreatePDLItemsTicketList(tickets, model.TicketInfo); // 初期選択項目

                //アプリ種別プルダウン
                ViewBag.AplList = CreatePDLItemsAplList(userRole, model.Apltype);

                //決済種別プルダウン
                ViewBag.PaymentTypeList = CreatePDLItemsPaymentTypeList(model.PaymentType);

                //枚数種別プルダウン
                ViewBag.TicketNumTypeList = CreatePDLItemsTicketNumTypeList(model.TicketNumType);

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
                var maxListCount = GetTicketPaymentRepository().GetPaymentsMaxCount(targetDateStart, targetDateLast, model.UserId, model.PaymentType, model.TicketNumType, transportType, ticketId, model.Apltype);

                //検索条件に一致したリストから表示件数分取得
                var paymentList = GetTicketPaymentRepository().GetPayments(targetDateStart, targetDateLast, model.UserId, model.PaymentType, model.TicketNumType, transportType, ticketId, model.Apltype, listNo.Begin, listNo.End);

                var info = new MPA1301Context()
                {
                    TargetDateBegin = targetDateStart.ToString(), // 開始日時
                    TargetDateEnd = targetDateLast.ToString(), // 終了日時
                    ListPageNo = model.ListPageNo, // 現在のページ位置
                    TicketId = ticketId, // 指定チケットID
                    TransportType = transportType, // チケット種別
                    TicketInfo = model.TicketInfo, // ドロップダウンリスト用チケット種別
                    PaymentType = model.PaymentType, // 指定決済種別
                    TicketNumType = model.TicketNumType, // 指定枚数種別
                    Apltype = model.Apltype, // アプリ種別
                };

                //取得したリスト件数が0以上
                if (paymentList.Any())
                {
                    info.PaymentReportData = new MPA1301ReportData()
                    {
                        ListMaxCount = maxListCount,
                        ListNum = LISTMAXROW,
                        ReportList = paymentList.Select(ToMPA1301ReportDataRow()).ToList(),
                    };
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
                searchKey.Add("MyrouteNo", model.UserId);
                searchKey.Add("TransportType", transportType);
                searchKey.Add("TicketInfo", model.TicketInfo);
                searchKey.Add("PaymentType", model.PaymentType);
                searchKey.Add("TicketNumType", model.TicketNumType);
                searchKey.Add("ListNum", LISTMAXROW.ToString());
                searchKey.Add("TicketId", ticketId);
                searchKey.Add("AplType", model.Apltype);
                Session.Add(SESSION_SEARCH_MPA1301, searchKey);

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
        [SessionCheck(WindowName = "昭和自動車決済データ画面")]
        public ActionResult OutPutCSV(MPA1301Context model)
        {
            try
            {
                ViewData["message"] = "";

                // 権限取得
                var userRole = GetUserRole(Session);

                //商品種別プルダウンリスト項目取得
                var tickets = GetTicketListRepository().GetTicketList();

                //商品種別プルダウン
                ViewBag.TicketList = CreatePDLItemsTicketList(tickets, model.TicketInfo); // 初期選択項目

                //アプリ種別プルダウン
                ViewBag.AplList = CreatePDLItemsAplList(userRole, model.Apltype);

                //決済種別プルダウン
                ViewBag.PaymentTypeList = CreatePDLItemsPaymentTypeList(model.PaymentType);

                //枚数種別プルダウン
                ViewBag.TicketNumTypeList = CreatePDLItemsTicketNumTypeList(model.TicketNumType);

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
                var maxListCount = GetTicketPaymentRepository().GetPaymentsMaxCount(targetDateStart, targetDateLast, model.UserId, model.PaymentType, model.TicketNumType, transportType, ticketId, model.Apltype);

                //検索条件に一致したリストから表示件数分取得(CSV出力用リストのためリスト全件数分取得する)
                var paymentList = GetTicketPaymentRepository().GetPayments(targetDateStart, targetDateLast, model.UserId, model.PaymentType, model.TicketNumType, transportType, ticketId, model.Apltype, listNo.Begin, maxListCount);



                var info = new MPA1301Context()
                {
                    TargetDateBegin = targetDateStart.ToString(), // 開始日時
                    TargetDateEnd = targetDateLast.ToString(), // 終了日時
                    ListPageNo = model.ListPageNo, // 現在のページ位置
                };

                //取得したリスト件数が0以上
                if (paymentList.Any())
                {
                    info.PaymentReportData = new MPA1301ReportData()
                    {
                        ListMaxCount = maxListCount,
                        ReportList = paymentList.Select(ToMPA1301ReportDataRow()).ToList(),
                    };
                }
                else
                {
                    ModelState.AddModelError("", "一致する決済データがありませんでした。");
                    return View($"{CONTENTROOT}/Index.cshtml", info);
                }


                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms, System.Text.Encoding.GetEncoding("shift_jis")))
                {
                    //ヘッダー部分を書き込む
                    sw.Write("\"myroute会員ID\"");
                    sw.Write(',');
                    sw.Write("\"決済日時\"");
                    sw.Write(',');
                    sw.Write("\"決済ID\"");

                    sw.Write(',');
                    sw.Write("\"商品種別\"");
                    sw.Write(',');
                    sw.Write("\"大人枚数\"");
                    sw.Write(',');
                    sw.Write("\"子供枚数\"");
                    sw.Write(',');
                    sw.Write("\"決済種別\"");
                    sw.Write(',');
                    sw.Write("\"金額\"");
                    sw.Write(',');
                    sw.Write("\"決済手段\"");
                    sw.Write(',');
                    sw.Write("\"仕向先\"");
                    sw.Write(',');
                    sw.Write("\"領収書番号\"");
                    sw.Write(',');
                    sw.Write("\"アプリ種別\"");
                    sw.Write(',');
                    sw.WriteLine("\"チケットID\"");

                    foreach (var item in info.PaymentReportData.ReportList)
                    {
                        //文字列に"を追加して出力
                        sw.Write("\"" + item.UserId + "\"");        //myrouteID
                        sw.Write(',');
                        sw.Write("\"" + item.TranDatetime + "\"");  //決済日時
                        sw.Write(',');
                        sw.Write("\"" + item.PaymentId + "\"");     //決済ID

                        sw.Write(',');
                        sw.Write("\"" + item.TicketName + "\"");    //商品種別
                        sw.Write(',');
                        sw.Write("\"" + item.AdultNum + "\"");      //大人枚数
                        sw.Write(',');
                        sw.Write("\"" + item.ChildNum + "\"");      //子供枚数
                        sw.Write(',');
                        sw.Write("\"" + item.PaymentType + "\"");   //決済種別
                        sw.Write(',');
                        sw.Write("\"" + item.Amount.ToString() + "\"");        //金額
                        sw.Write(',');
                        sw.Write("\"" + item.PaymentName + "\""); //決済手段
                        sw.Write(',');
                        sw.Write("\"" + item.Forward + "\""); //仕向先
                        sw.Write(',');
                        sw.Write("\"" + item.ReceiptNo + "\""); //領収書番号
                        sw.Write(',');
                        sw.Write("\"" + item.Apltype + "\""); //アプリ種別
                        sw.Write(',');
                        sw.WriteLine("\"" + item.InquiryId + "\""); //問い合わせID
                    }
                    sw.Close();

                    //出力日を取得
                    var NowDate = System.DateTime.Now;

                    //ファイル名を「Payment_Report_検索開始日(yyyyMMdd)-終了日(yyyyMMdd)_作成日(yyyyMMdd)」で出力
                    return File(ms.ToArray(), FILE_CONTENTTYPE, "Payment_Report_" + targetDateStart.ToString("yyyyMMdd") + "-" + targetDateLast.ToString("yyyyMMdd") + "_" + NowDate.ToString("yyyyMMdd") + FILE_EXTENSION);
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
        private static ShowabusTicketSaleRepository GetTicketListRepository()
        {
            return new ShowabusTicketSaleRepository();
        }

        /// <summary>
        /// 決済データ取得
        /// </summary>
        /// <returns></returns>
        private static ShowabusTicketPaymentRepository GetTicketPaymentRepository()
        {
            return new ShowabusTicketPaymentRepository();
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="model"></param>
        /// <returns>IsValid: 結果(true: 正常, false: 異常), ErrMsg: エラーメッセージ(結果がfalse時のみ)</returns>
        private static void IsValidInput(MPA1301Context model)
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
        private static (int Begin, int End) GetListRowNum(MPA1301Context model)
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
        private static string GetSelectedTicket(MPA1301Context model)
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
        private static Func<ShowabusPaymentInfo, MPA1301ReportDataRow> ToMPA1301ReportDataRow() =>
            t =>
                new MPA1301ReportDataRow()
                {
                    UserId = t.UserId,
                    TranDatetime = t.TranDatetime,
                    TicketName = t.TicketName,
                    AdultNum = t.AdultNum,
                    ChildNum = t.ChildNum,
                    Amount = t.Amount,
                    PaymentType = t.PaymentType,
                    PaymentId = t.PaymentId,
                    PaymentName = t.GetPaymentName(),
                    Forward = t.GetForwardName(),
                    ReceiptNo = t.ReceiptNo,
                    Apltype = t.Apltype,
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
        /// アプリ種別プルダウンリスト生成
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        private List<SelectListItem> CreatePDLItemsAplList(string userRole, string selected = null)
            => BuildDropDownList(ddlAplType,
                    t => t.Text,
                    t => t.Value,
                    userRole == "13" ? false : true,
                    !string.IsNullOrEmpty(selected) ? selected : userRole == "13" ? "1" : "-");
        
        private List<SelectListItem> CreatePDLItemsPaymentTypeList(string selected = null)
            => BuildDropDownList(PaymentType.All,
                    t => t.Name,
                    t => t.Value,
                    selected);

        /// <summary>
        /// 枚数種別プルダウンリスト生成
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        private List<SelectListItem> CreatePDLItemsTicketNumTypeList(string selected = null)
            => BuildDropDownList(ddlTicketNumType,
                    t => t.Name,
                    t => t.Value,
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