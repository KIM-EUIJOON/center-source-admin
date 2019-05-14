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
            List<RoleFunction> roleInfo = null;
            AppSigmaAdmin.Models.RoleList RoleInfoAdminEntity = null;
            RoleList response = new RoleList();
            if (Session[AppSigmaAdmin.Library.SystemConst.SESSION_ROLE_INFO_ADMIN] != null)
            {
                //セッションに保存してある権限情報を取得する
                RoleInfoAdminEntity = (AppSigmaAdmin.Models.RoleList)Session[AppSigmaAdmin.Library.SystemConst.SESSION_ROLE_INFO_ADMIN];
                roleInfo = RoleInfoAdminEntity.RoleFunctionList;
                response.RoleFunctionList = roleInfo;
            }
            //セッションに保存してある情報をViewに渡す
            return View(response);
        }
    }
}