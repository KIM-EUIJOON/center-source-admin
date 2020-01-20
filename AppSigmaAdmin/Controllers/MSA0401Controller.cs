using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.ResponseData;
using AppSigmaAdmin.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppSigmaAdmin.Controllers
{
    public class MSA0401Controller : Controller
    {
        /// <summary>
        /// ID検索画面(ボタン押下以外)
        /// </summary>
        /// <returns>ID検索画面</returns>
        [SessionCheck(WindowName = "問い合わせID検索画面")]
        public ActionResult Index()
        {
            ViewData["message"] = "";
            return View();
        }

        private const string SESSION_SEARCH_Inquiry = "SESSION_SEARCH_Inquiry";

        /// <summary>
        /// ID検索処理
        /// </summary>
        /// <returns>ID検索画面</returns>
        [HttpPost]
        [SessionCheck(WindowName = "問い合わせID検索画面")]
        public ActionResult Index(InquiryInfo model)
        {
            ViewData["message"] = "";

            //問い合わせ番号入力チェック
            if (model.InquiryNo == null)
            {
                ModelState.AddModelError("", "問い合わせIDが入力されていません。半角数字のみで再入力してください。");
                model.UserId = null;
                return View(model);
            }

            Int64 InquiryNo = 0;
            string InputNo = model.InquiryNo.ToString();
            string CheckInput = InputNo.Trim();

            try
            {
                InquiryNo = Int64.Parse(CheckInput);
            }
            catch (OverflowException)
            {
                ModelState.AddModelError("", "問い合わせIDに誤った数値が入力されました。再入力してください。");
                model.UserId = null;
                return View(model);
            }
            catch
            {
                ModelState.AddModelError("", "問い合わせIDに数字以外が入力されました。半角数字のみで再入力してください。");
                model.UserId = null;
                return View(model);
            }
            /*新しい入力値をテキストボックスに反映させるため、model内の値を削除する*/
            ModelState.Clear();

            /*関数でmyrouteIDを割り出す*/
            string UserIdNo = GetID(InputNo);

            if (UserIdNo != "")
            {
                string text = "UserId";
                model.UserId = UserIdNo;

                if (UserIdNo != "")
                {
                    Dictionary<string, string> result = new UserInfo().UserIdSearch(text, UserIdNo);
                    if (result.Count > 0)
                    {
                        if (result["AplTypeNo"].ToString() == "1")
                        {
                            model.Apltype = "1";
                        }
                        else
                        {

                        }
                        if (result["Deleteflugkey"].ToString() == "0")
                        {
                            ModelState.AddModelError("", "入力されたIDに該当するユーザーが存在していません。");
                            return View(model);
                        }
                        else if (result["Deleteflugkey"].ToString() == "-1")
                        {
                            ModelState.AddModelError("", "入力されたIDに該当するユーザーは退会済です。");
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "入力されたIDに該当するユーザーが存在していません。");
                        return View(model);
                    }

                }
            }


            return View(model);
        }

        ///<sunmmary>
        ///ID算出関数
        ///</sunmmary>
        public string GetID(string InputId)
        {
            //文字数をカウントする
            int idnum = InputId.Length;
            string Idnum = "";
            //10文字存在していた場合に5文字ずつに分ける
            if (idnum == 10)
            {
                string beg5 = InputId.Substring(0, 5);
                string end5 = InputId.Substring(5, 5);
                //0詰めを削除する
                int begnum = 0;
                int endnum = 0;
                begnum = int.Parse(beg5);
                endnum = int.Parse(end5);

                if (begnum == 0)    //前半部に0詰め部分のみしか取得されなかった場合
                {
                    Idnum = endnum.ToString();
                }
                else if (endnum == 0)    //後半部に0詰め部分のみしか取得されなかった場合
                {
                    Idnum = begnum.ToString();
                }
                else
                {
                    //前後を入れ替えたものをIDとして返却する
                    Idnum = endnum.ToString() + begnum.ToString();
                }
                return Idnum;
            }
            else
            {
                ModelState.AddModelError("", "誤った問い合わせIDが入力されました。再入力してください。");
                return Idnum;
            }

        }
    }
}