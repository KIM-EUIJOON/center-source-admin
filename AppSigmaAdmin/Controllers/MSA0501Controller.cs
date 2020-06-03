using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// 利用者ログ情報ダウンロードコントローラクラス
    /// </summary>
    public class MSA0501Controller : Controller
    {
        // 情報種別を取得・格納する
        private const string infoTypes = SystemConst.UPLOAD_FILE_INFO_TYPES;
        private Dictionary<string, string> dicInfoType = infoTypes.Split(',').Select((v, i) => new { Key = v.Split(':')[0], Value = v.Split(':')[1] }).ToDictionary(v => v.Key, v => v.Value);

        /// <summary>
        /// 利用者ログ情報ダウンロード画面
        /// </summary>
        /// <returns>利用者ログ情報ダウンロード画面</returns>
        [SessionCheck(WindowName = "利用者ログ情報ダウンロード画面")]
        public ActionResult Index()
        {
            UserLogInfoListEntity info = new UserLogInfoListEntity();

            this.InitList();
            info.IsInformationLog = true;
            info.IsDebugLog = false;
            info.IsMobileLog = true;

            return View(info);
        }

        /// <summary>
        /// 利用者ログ情報ダウンロード(CSV)
        /// </summary>
        /// <param name="model"></param>
        /// <returns>利用者ログ情報CSVファイル</returns>
        [HttpPost]
        [SessionCheck(WindowName = "利用者ログ情報ダウンロード画面")]
        public ActionResult UserLogOutPutData(UserLogInfoListEntity model)
        {
            ViewData["message"] = "";
            this.InitList();

            if (!this.CheckParam(model))
            {
                return View("~/Views/MSA0501/Index.cshtml", model);
            }

            // 利用者ログ情報を取得
            AzureStorageIO azureStorage = new AzureStorageIO();
            List<UserLogInfoEntity> table = new List<UserLogInfoEntity>();
            DateTime targetDateBegin = DateTime.Parse(model.TargetDateBegin + " " + model.StartHour + ":" + model.StartMinute);
            DateTime targetDateEnd = DateTime.Parse(model.TargetDateEnd + " " + model.EndHour + ":" + model.EndMinute);
            string userId = model.UserId;

            // インフォメーションログを取得
            if (model.IsInformationLog)
            {
                List<string> messageList = new List<string>();
                if (!string.IsNullOrEmpty(model.InformationLogMessage1))
                {
                    messageList.Add(model.InformationLogMessage1);
                }
                if (!string.IsNullOrEmpty(model.InformationLogMessage2))
                {
                    messageList.Add(model.InformationLogMessage2);
                }
                List<UserLogInfoEntity> informationLog = azureStorage.GetInformationLogTable(targetDateBegin, targetDateEnd, userId, messageList);
                if (informationLog != null && informationLog.Count() > 0)
                {
                    table.AddRange(informationLog);
                }
            }

            // デバッグログを取得
            if (model.IsDebugLog)
            {
                List<string> messageList = new List<string>();
                if (!string.IsNullOrEmpty(model.DebugLogMessage1))
                {
                    messageList.Add(model.DebugLogMessage1);
                }
                if (!string.IsNullOrEmpty(model.DebugLogMessage2))
                {
                    messageList.Add(model.DebugLogMessage2);
                }
                List<UserLogInfoEntity> debugLog = azureStorage.GetDebugLogTable(targetDateBegin, targetDateEnd, userId, messageList);
                if (debugLog != null && debugLog.Count() > 0)
                {
                    table.AddRange(debugLog);
                }
            }

            // 端末ログを取得
            if (model.IsMobileLog)
            {
                List<string> messageList = new List<string>();
                if (!string.IsNullOrEmpty(model.MobileInformationLogMessage1))
                {
                    messageList.Add(model.MobileInformationLogMessage1);
                }
                if (!string.IsNullOrEmpty(model.MobileInformationLogMessage2))
                {
                    messageList.Add(model.MobileInformationLogMessage2);
                }
                string infoTypeName = null;
                if (!string.IsNullOrEmpty(model.MobileLogKey))
                {
                    infoTypeName = dicInfoType[model.MobileLogKey];
                }
                List<UserLogInfoEntity> mobileLog = azureStorage.GetMobileInformationLogTable(targetDateBegin, targetDateEnd, userId, messageList, infoTypeName, model.MobileInformationLogMobileId);
                if (mobileLog != null && mobileLog.Count() > 0)
                {
                    table.AddRange(mobileLog);
                }
            }

            // 利用者ログ情報をタイムスタンプの昇順に並び替える
            List<UserLogInfoEntity> sortedTable = table.OrderBy(_ => _.Timestamp).ToList();

            // 利用者ログ情報が取得できているか確認
            if (table.Count() == 0)
            {
                // 取得0件の場合はCSV出力しない
                ModelState.AddModelError("", "条件に該当する情報はありません。");
                return View("~/Views/MSA0501/Index.cshtml", model);
            }

            // CSV出力内容を取得
            byte[] buffer = GetCsvDownloadStream(sortedTable, new UserLogInfoClassMap());

            // 出力日を取得
            DateTime NowDate = System.DateTime.Now;

            // ファイルダウンロード
            return File(buffer, SystemConst.USER_LOG_FILE_CONTENTTYPE, string.Format(SystemConst.USER_LOG_FILE_NAME, NowDate.ToString("yyyyMMddHHmmss"), targetDateBegin.ToString("yyyyMMddHHmm"), targetDateEnd.ToString("yyyyMMddHHmm")));
        }

        #region プライベートメソッド
        /// <summary>
        /// 入力パラメータチェック
        /// </summary>
        /// <param name="model"></param>
        /// <returns>true:正常、false:異常</returns>
        private bool CheckParam(UserLogInfoListEntity model)
        {
            if (string.IsNullOrEmpty(model.TargetDateBegin))
            {
                ModelState.AddModelError("", "検索条件の開始年月日を指定してください。");
                return false;
            }
            else if (string.IsNullOrEmpty(model.TargetDateEnd))
            {
                ModelState.AddModelError("", "検索条件の終了年月日を指定してください。");
                return false;
            }

            if (!IsDate(model.TargetDateBegin.ToString()))
            {
                ModelState.AddModelError("", "検索条件の開始年月日が正しくありません。半角英数字で再入力してください。");
                return false;
            }
            else if (!IsDate(model.TargetDateEnd.ToString()))
            {
                ModelState.AddModelError("", "検索条件の終了年月日が正しくありません。半角英数字で再入力してください。");
                return false;
            }

            if (string.IsNullOrEmpty(model.StartHour) || string.IsNullOrEmpty(model.StartMinute))
            {
                ModelState.AddModelError("", "検索条件の開始時刻を指定してください。");
                return false;
            }
            else if (string.IsNullOrEmpty(model.EndHour) || string.IsNullOrEmpty(model.EndMinute))
            {
                ModelState.AddModelError("", "検索条件の終了時刻を指定してください。");
                return false;
            }

            if (!int.TryParse(model.StartHour, out int resStartHour) || !int.TryParse(model.StartMinute, out int resStartMinute)
                || resStartHour < 0 || resStartHour >= 24 || resStartMinute < 0 || resStartMinute >= 60)
            {
                ModelState.AddModelError("", "検索条件の開始時刻が正しくありません。時(00～23)、分(00～59)で再入力してください。");
                return false;
            }
            else if (!int.TryParse(model.EndHour, out int resEndHour) || !int.TryParse(model.EndMinute, out int resEndMinute)
                || resEndHour < 0 || resEndHour >= 24 || resEndMinute < 0 || resEndMinute >= 60)
            {
                ModelState.AddModelError("", "検索条件の終了時刻が正しくありません。時(00～23)、分(00～59)で再入力してください。");
                return false;
            }

            DateTime targetDateBegin = DateTime.Parse(model.TargetDateBegin + " " + model.StartHour + ":" + model.StartMinute);
            DateTime targetDateEnd = DateTime.Parse(model.TargetDateEnd + " " + model.EndHour + ":" + model.EndMinute);
            if (targetDateBegin == targetDateEnd)
            {
                ModelState.AddModelError("", "検索条件の開始日時と終了日時が同じ値です。再入力してください。");
                return false;
            }
            else if (targetDateBegin > targetDateEnd)
            {
                ModelState.AddModelError("", "検索条件の開始日時が終了日時よりも未来です。再入力してください。");
                return false;
            }

            if (!string.IsNullOrEmpty(model.UserId))
            {
                try
                {
                    int.Parse(model.UserId.ToString());
                }
                catch (OverflowException)
                {
                    // myrouteIDのテキストボックスに誤った数値が入力された場合
                    ModelState.AddModelError("", "myroute会員IDに誤った数値が入力されました。半角数字で再入力してください。");
                    return false;
                }
                catch
                {
                    // myrouteIDのテキストボックスに半角数字以外が入力された場合
                    ModelState.AddModelError("", "myroute会員IDが数字以外で入力されました。半角数字で再入力してください。");
                    return false;
                }
            }

            if (model.IsMobileLog && !string.IsNullOrEmpty(model.MobileLogKey))
            {
                if (!dicInfoType.ContainsKey(model.MobileLogKey))
                {
                    ModelState.AddModelError("", "端末ログ情報の取得対象が正しく選択されていません。再選択してください。");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 入力日付チェック関数
        /// </summary>
        /// <param name="s">日付</param>
        /// <returns>true:正常、false:異常</returns>
        private bool IsDate(string s)
        {
            // 入力された日時が年/月/日以外はエラーで返す
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

        /// <summary>
        /// リスト初期化
        /// </summary>
        private void InitList()
        {
            this.InitDateTimeList();
            this.InitMobileLogList();
        }

        /// <summary>
        /// 端末ログ取得対象ドロップダウンリスト初期化
        /// </summary>
        private void InitMobileLogList()
        {
            ViewBag.MobileLogList = this.CreateMobileLogList();
        }

        /// <summary>
        /// 端末ログ取得対象リスト生成
        /// </summary>
        /// <returns>端末ログ取得対象リスト</returns>
        private List<SelectListItem> CreateMobileLogList()
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            itemList.Add(new SelectListItem { Text = "全て", Value = "", Selected = true });
            foreach (string key in dicInfoType.Keys)
            {
                itemList.Add(new SelectListItem { Text = dicInfoType[key], Value = key });
            }

            return itemList;
        }

        /// <summary>
        /// 時刻ドロップダウンリスト初期化
        /// </summary>
        private void InitDateTimeList()
        {
            ViewBag.StartHourList = this.CreateDateTimeList(0, 24);
            ViewBag.StartMinuteList = this.CreateDateTimeList(0, 60);
            ViewBag.EndHourList = this.CreateDateTimeList(0, 24);
            ViewBag.EndMinuteList = this.CreateDateTimeList(0, 60);
        }

        /// <summary>
        /// 時刻生成
        /// </summary>
        /// <param name="start">開始</param>
        /// <param name="end">終了</param>
        /// <returns>時刻リスト</returns>
        private List<SelectListItem> CreateDateTimeList(int start, int end)
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            itemList.Add(new SelectListItem { Text = start.ToString("00"), Value = start.ToString("00"), Selected = true });
            for (int i = start + 1; i < end; i++)
            {
                itemList.Add(new SelectListItem { Text = i.ToString("00"), Value = i.ToString("00") });
            }

            return itemList;
        }

        /// <summary>
        /// ダウンロードデータ取得
        /// </summary>
        /// <param name="list">対象リスト</param>
        /// <param name="map">マッピング情報</param>
        /// <returns>ダウンロードデータ</returns>
        private byte[] GetCsvDownloadStream<T, TMap>(List<T> list, TMap map) where TMap : ClassMap
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.GetEncoding("Shift_JIS")))
                using (var csvWriter = new CsvWriter(streamWriter))
                {
                    csvWriter.Configuration.HasHeaderRecord = true;
                    csvWriter.Configuration.QuoteAllFields = true;
                    csvWriter.Configuration.RegisterClassMap<TMap>();
                    csvWriter.WriteRecords(list);
                    streamWriter.Flush();
                }

                return memoryStream.ToArray();
            }
        }
        #endregion
    }
}
