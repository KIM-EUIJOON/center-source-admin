using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppSigmaAdmin.Attribute;
using AppSigmaAdmin.Library;
using AppSigmaAdmin.Models;
using AppSigmaAdmin.Utility;

namespace AppSigmaAdmin.Controllers
{
    /// <summary>
    /// メニューコントローラクラス
    /// </summary>
    public class MenuController : Controller
    {
        /// <summary>
        /// メニュー画面
        /// </summary>
        /// <returns>メニュー画面</returns>
        [SessionCheck(WindowName ="メニュー画面")]
        public ActionResult Index()
        {
            UserInfoAdminEntity UserInfo = null;
            //セッションに保存されているユーザー情報を取得する
            UserInfo = (UserInfoAdminEntity)Session[SystemConst.SESSION_USER_INFO_ADMIN];
            if (Session[SystemConst.SESSION_ROLE_INFO_ADMIN] == null)
            {
                //セッションに権限リストが保存されていない場合は取得する
                List<RoleFunction> RoleFuncList = null;
                //ログイン時に保存されたユーザー情報をもとに権限情報を取得する
                RoleFuncList = new LoginModel().GetRoleFunctions(UserInfo.Role);
                RoleList roleFunction = new RoleList();
                roleFunction.LowerRoleFunctionList = new List<RoleFunction>();
                roleFunction.UpperRoleFunctionList = new List<RoleFunction>();

                //取得リストが0件でない場合
                if (RoleFuncList.Count > 0)
                {
                    foreach (RoleFunction item in RoleFuncList)
                    {
                        //DispOrderが99以上のものはシステム管理者用のメニューのため、分けてリストを作成する
                        if (int.Parse(item.DispOrder) < 99)
                        {
                            //DispOrderが99未満のリストを作成する
                            roleFunction.LowerRoleFunctionList.Add(item);
                        }
                        else if (int.Parse(item.DispOrder) > 99)
                        {
                            //DispOrderが99以上のリストを作成する
                            roleFunction.UpperRoleFunctionList.Add(item);
                        }
                    };
                }
                //メニュー表示用のリスト情報をセッションに保存する
                HttpContext.Session.Add(SystemConst.SESSION_ROLE_INFO_ADMIN, roleFunction);
            }

            List<RoleFunction> roleInfo = null;
            List<RoleFunction> upperroleInfo = null;
            RoleList RoleInfoAdminEntity = null;
            RoleList response = new RoleList();

            //セッションに保存してある権限情報を取得する
            RoleInfoAdminEntity = (RoleList)Session[SystemConst.SESSION_ROLE_INFO_ADMIN];
            roleInfo = RoleInfoAdminEntity.LowerRoleFunctionList;
            upperroleInfo = RoleInfoAdminEntity.UpperRoleFunctionList;
            response.LowerRoleFunctionList = roleInfo;
            response.UpperRoleFunctionList = upperroleInfo;

            //セッションに保存してある情報をViewに渡す
            return View(response);
        }
    }
}