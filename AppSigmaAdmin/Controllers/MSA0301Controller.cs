using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Attribute;
using static AppSigmaAdmin.Models.AuthIpAddressModel;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;
using System.Diagnostics;
using Microsoft.VisualBasic;
using IpMatcher;
using System.Text.RegularExpressions;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// システム管理者機能コントローラクラス
    /// </summary>
    public class MSA0301Controller : Controller
    {
       private const string SESSION_AUTH_ADDRESS_LIST = "Session_AuthAddressList";
        
        /// <summary>
        /// セッション更新関数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [SessionCheck(WindowName = "システム管理者機能画面")]
        public void IPreload()
        {
            List<AuthIpAddressEntity> authIpAddressEntities;
            //セッションを更新する
            authIpAddressEntities = new AuthIpAddressModel().GetAuthIpAddress();
            HttpContext.Application[name: SESSION_AUTH_ADDRESS_LIST] = authIpAddressEntities;
        }

        /// <summary>
        /// IPリスト取得関数
        /// </summary>
        /// <returns></returns>
        public AuthIpAddressEntityList GetIPListTable()
        {
            List<AuthIpAddressEntityList> GetIpList = null;
            //DBからIPアドレスリストを取得
            GetIpList = new AuthIpAddressEntityList().GetIpList();

            AuthIpAddressEntityList result = new AuthIpAddressEntityList();

            if (GetIpList.Count > 0)
            {
                result.IPAddressList = GetIpList;
            }
            return result;
        }

        /// <summary>
        /// システム管理者機能画面
        /// </summary>
        /// <returns>システム管理者機能画面</returns>
        [SessionCheck(WindowName = "システム管理者機能画面")]
        public ActionResult Index()
        {
            ViewData["message"] = "";
            //画面表示用のリストをDBから取得
            AuthIpAddressEntityList result = GetIPListTable();
            return View(result);
        }

        /// <summary>
        /// IPアドレスリストセッション更新処理
        /// </summary>
        /// <returns>システム管理者機能画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "管理者機能画面")]
        public ActionResult IpRefresh(object sender,EventArgs e)
        {
            ViewData["message"] = "";

            //画面表示用のリストをDBから取得
            AuthIpAddressEntityList result = GetIPListTable();

            //セッションを更新する
            try
              {
                  IPreload();
                  //正常に更新が完了した場合は1を設定する
                  ViewBag.IpUpdate = 1;
                  return View("~/Views/MSA0301/Index.cshtml", result);
              }
              catch(Exception error)
              {
                  //エラーが発生した場合は2を設定する
                  ViewBag.IpUpdate = 2;
                  Trace.TraceError(Logger.GetExceptionMessage(error));
                  return View("~/Views/MSA0301/Index.cshtml", result);
              }
        }
        /// <summary>
        /// IPアドレスリスト修正選択時処理
        /// </summary>
        /// <returns>システム管理者機能画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "管理者機能画面")]
        public ActionResult Edit(AuthIpAddressEntityList model)
        {
            ViewData["message"] = "";

                //DBからIPアドレスリストを取得する
                AuthIpAddressEntityList result = GetIPListTable();

            if (model.NetAddress != null && model.NetAddress != "" && model.IPAddressName != null && model.IPAddressName != "")
            {
                List<AuthIpAddressEntityList> authIpAddressEntities;
                authIpAddressEntities = result.IPAddressList;

                //入力内容が正しいか判定する
                string IPAdd = "";
                string SearchOb = "/";
                string SubNetAdd = "";
                int num;
                try
                {
                    num = model.NetAddress.IndexOf(SearchOb);
                    IPAdd = model.NetAddress.Substring(0, num);
                }
                catch (Exception error)
                {
                    //「/」が抜けている場合のエラー
                    Trace.TraceError(Logger.GetExceptionMessage(error));
                    ModelState.AddModelError("", "入力されたネットワークアドレスの形式が正しくありません。再入力してください。");
                    return View("~/Views/MSA0301/Index.cshtml", result);
                }

                //入力されたIPアドレスの形式が正しいか判定する
                //サブネットアドレスを取得
                int SubNetAddLength = model.NetAddress.Length - (num + 1);
                SubNetAdd = model.NetAddress.Substring(num + 1, SubNetAddLength);

                string addtemp = @"^(([01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}([01]?\d{1,2}|2[0-4]\d|25[0-5])$";
                bool IpAddcheck = Regex.IsMatch(IPAdd, addtemp);

                if (IpAddcheck == false)
                {
                    ModelState.AddModelError("", "入力されたIPアドレスの形式が正しくありません。再入力してください。");
                    return View("~/Views/MSA0301/Index.cshtml", result);
                }

                bool SubNetAddCheck = Regex.IsMatch(SubNetAdd, addtemp);
                if (SubNetAddCheck == false)
                {
                    ModelState.AddModelError("", "入力されたサブネットアドレスの形式が正しくありません。再入力してください。");
                    return View("~/Views/MSA0301/Index.cshtml", result);
                }

                List<string> IPAddCheck = new List<string>();

                //IPアドレスリストからネットアドレスリストを作成する
                if (authIpAddressEntities.Count > 0)
                {
                    foreach (var IpAddlist in authIpAddressEntities)
                    {
                        string Address = IpAddlist.NetAddress.ToString();
                        IPAddCheck.Add(Address);
                    }
                }

                //IPアドレスリストに入力されたIPアドレスが存在するか確認する
                if (IPAddCheck.Contains(model.NetAddress) == true)
                {
                    //選択したネットアドレスと更新時のネットアドレスが異なる場合は更新を行わない。
                    if (model.NetAddress != model.BeforeNetAddress)
                    {
                        //既存IPアドレスのためエラーを表示する
                        ModelState.AddModelError("", "入力されたIPアドレスは既に登録されています。");
                        return View("~/Views/MSA0301/Index.cshtml", result);
                    }
                    else
                    {
                        //既存IPアドレスのため更新処理を行う
                        string BeforeNetAddress = model.BeforeNetAddress;
                        string NetAddress = model.NetAddress;
                        string Memo = model.IPAddressName;

                        //IPアドレスの更新情報をDBに登録する
                        try
                        {
                            new AuthIpAddressEntityList().EditIPList(BeforeNetAddress, NetAddress, Memo);
                        }
                        catch (Exception error)
                        {
                            ViewBag.IpUpdate = 2;
                            Trace.TraceError(Logger.GetExceptionMessage(error));
                            return View("~/Views/MSA0301/Index.cshtml", result);
                        }
                    }
                }
                else
                {
                    //既存IPアドレスのため更新処理を行う
                    string BeforeNetAddress = model.BeforeNetAddress;
                    string NetAddress = model.NetAddress;
                    string Memo = model.IPAddressName;

                    //IPアドレスの更新情報をDBに登録する
                    try
                    {
                        new AuthIpAddressEntityList().EditIPList(BeforeNetAddress, NetAddress, Memo/*, EnvDev, EnvPre, EnvProd*/);
                    }
                    catch (Exception error)
                    {
                        ViewBag.IpUpdate = 2;
                        Trace.TraceError(Logger.GetExceptionMessage(error));
                        return View("~/Views/MSA0301/Index.cshtml", result);
                    }
                }

            }
            else if (model.NetAddress == null || model.NetAddress == "")
            {
                ModelState.AddModelError("", "ネットアドレスが空白になっています。再度入力してください。");
                return View("~/Views/MSA0301/Index.cshtml", result);
            }
            else if (model.IPAddressName == null || model.IPAddressName == "")
            {
                ModelState.AddModelError("", "IPアドレス名が空白になっています。再度入力してください。");
                return View("~/Views/MSA0301/Index.cshtml", result);
            }

            //リストを再取得して表示する
            AuthIpAddressEntityList Newresult = GetIPListTable();

            //セッションを更新する
            try
            {
                IPreload();
                //正常に更新が完了した場合は1を設定する
                ViewBag.IpUpdate = 1;
                return View("~/Views/MSA0301/Index.cshtml", Newresult);
            }
            catch (Exception error)
            {
                //エラーが発生した場合は2を設定する
                ViewBag.IpUpdate = 2;
                Trace.TraceError(Logger.GetExceptionMessage(error));
                return View("~/Views/MSA0301/Index.cshtml", Newresult);
            }

            
        }

        /// <summary>
        /// IPアドレスリスト新規追加選択時処理
        /// </summary>
        /// <returns>システム管理者機能画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "管理者機能画面")]
        public ActionResult Add(AuthIpAddressEntityList model)
        {

            ViewData["message"] = "";

            //入力されたIPアドレス
            string NetAddress = model.NetAddress;
            string IPAddressName = model.IPAddressName;
            //DBからIPアドレスを取得する
            AuthIpAddressEntityList IpList = GetIPListTable();

            if (NetAddress != null && NetAddress != "" && IPAddressName != null && IPAddressName != "")
            {
                //DBから取得したIPアドレスからリストを取得する
                List<AuthIpAddressEntityList> authIpAddressEntities;
                authIpAddressEntities = IpList.IPAddressList;

                //入力内容が正しいか判定する
                string IPAdd = "";
                string SearchOb = "/";
                string SubNetAdd = "";
                int num;
                try
                {
                    num = NetAddress.IndexOf(SearchOb);
                    IPAdd = NetAddress.Substring(0, num);
                }
                catch (Exception error)
                {
                    //「/」が抜けている場合のエラー
                    Trace.TraceError(Logger.GetExceptionMessage(error));
                    ModelState.AddModelError("", "入力されたネットワークアドレスの形式が正しくありません。再入力してください。");
                    return View("~/Views/MSA0301/Index.cshtml", IpList);
                }

                //入力されたIPアドレスの形式が正しいか判定する
                //サブネットアドレスを取得
                int SubNetAddLength = NetAddress.Length - (num + 1);
                SubNetAdd = NetAddress.Substring(num + 1, SubNetAddLength);

                string addtemp = @"^(([01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}([01]?\d{1,2}|2[0-4]\d|25[0-5])$";
                bool IpAddcheck = Regex.IsMatch(IPAdd, addtemp);

                if (IpAddcheck == false)
                {
                    ModelState.AddModelError("", "入力されたIPアドレスの形式が正しくありません。再入力してください。");
                    return View("~/Views/MSA0301/Index.cshtml", IpList);
                }

                bool SubNetAddCheck = Regex.IsMatch(SubNetAdd, addtemp);
                if (SubNetAddCheck == false)
                {
                    ModelState.AddModelError("", "入力されたサブネットアドレスの形式が正しくありません。再入力してください。");
                    return View("~/Views/MSA0301/Index.cshtml", IpList);
                }

                //入力されたIPアドレスの既存チェック
                List<string> IPAddCheck = new List<string>();

                if (authIpAddressEntities.Count > 0)
                {
                    foreach (var IpAddlist in authIpAddressEntities)
                    {
                        string Address = IpAddlist.NetAddress.ToString();
                        IPAddCheck.Add(Address);
                    }
                }


                if (IPAddCheck.Contains(NetAddress) == true)
                {
                    //既存IPアドレスのためエラーを表示する
                    ModelState.AddModelError("", "入力されたIPアドレスは既に登録されています。");
                    return View("~/Views/MSA0301/Index.cshtml", IpList);

                }
                else
                {
                    //新規IPアドレスのため追加処理を行う
                    string Memo = "";
                    if (IPAddressName != null)
                    {
                        Memo = model.IPAddressName;
                    }
                    string EnvDev = "0";
                    string EnvPre = "0";
                    string EnvProd = "0";
                    if (model.EnvDev != null)
                    {
                        EnvDev = "1";
                    }
                    if (model.EnvPre != null)
                    {
                        EnvPre = "1";
                    }
                    if (model.EnvProd != null)
                    {
                        EnvProd = "1";
                    }

                    //新規IPアドレスをDBに登録する
                    try
                    {
                        new AuthIpAddressEntityList().UpdateIPList(NetAddress, Memo, EnvDev, EnvPre, EnvProd);
                    }
                    catch (Exception error)
                    {
                        ViewBag.IpUpdate = 2;
                        Trace.TraceError(Logger.GetExceptionMessage(error));
                        ModelState.AddModelError("", "IPアドレスの追加処理にエラーが発生しました。");
                        return View("~/Views/MSA0301/Index.cshtml", IpList);
                    }
                }
            }
            else if(NetAddress == null || NetAddress == "")
            {
                ModelState.AddModelError("", "ネットアドレスが空白になっています。再度入力してください。");
                return View("~/Views/MSA0301/Index.cshtml", IpList);
            }
            else if (IPAddressName == null || IPAddressName == "")
            {
                ModelState.AddModelError("", "IPアドレス名が空白になっています。再度入力してください。");
                return View("~/Views/MSA0301/Index.cshtml", IpList);
            }

            //リストを再取得して表示する
            AuthIpAddressEntityList result = GetIPListTable();

            //セッションを更新する
            try
            {
                IPreload();
                //正常に更新が完了した場合は1を設定する
                ViewBag.IpUpdate = 1;
                return View("~/Views/MSA0301/Index.cshtml", result);
            }
            catch (Exception error)
            {
                //エラーが発生した場合は2を設定する
                ViewBag.IpUpdate = 2;
                Trace.TraceError(Logger.GetExceptionMessage(error));
                return View("~/Views/MSA0301/Index.cshtml",result);
            }
        }

        /// <summary>
        /// IPアドレスリスト更新処理
        /// </summary>
        /// <returns>システム管理者機能画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "管理者機能画面")]
        public ActionResult IpDelete()
        {
            ViewData["message"] = "";

            //アドレス削除処理
            string NetAddress = Request.Form.ToString();
            string DeleteAddress = NetAddress.Replace("%2f","/");
            new AuthIpAddressEntityList().DeleteIPAdd(DeleteAddress);

            //IPアドレスリストをDBから取得
            AuthIpAddressEntityList result = GetIPListTable();
            
            //セッションを更新する
            try
            {
                IPreload();
                //正常に更新が完了した場合は1を設定する
                ViewBag.IpUpdate = 1;
                return View("~/Views/MSA0301/Index.cshtml", result);
            }
            catch (Exception error)
            {
                //エラーが発生した場合は2を設定する
                ViewBag.IpUpdate = 2;
                Trace.TraceError(Logger.GetExceptionMessage(error));
                return View("~/Views/MSA0301/Index.cshtml", result);
            }

        }
    }
}