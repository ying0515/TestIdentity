using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TestIdentity.Models;

namespace TestIdentity.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public ActionResult Index()
        {
            var source = UserManager.Users.ToList();

            var roleDictionary = new Dictionary<string, string>();
            foreach (var item in source)
            {
                var roleName = UserManager.GetRoles(item.Id).SingleOrDefault();
                if (roleName != null)
                {
                    roleDictionary.Add(item.Id, roleName);
                }
                else
                {
                    roleDictionary.Add(item.Id, "");
                }
            }
            ViewData["roleCollection"] = roleDictionary;

            return View(source);
        }
        public ActionResult Create()
        {
            ViewBag.UserInRoles = RoleManager.Roles.ToList().Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Name
            }).OrderBy(d => d.Text);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser()
                    {
                        UserName = model.Email,
                        Email = model.Email,
                    };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {

                        foreach (var role in selectedRole)
                        {
                            UserManager.AddToRole(user.Id, role);
                        }
                        // await SignInAsync(user, isPersistent: false);

                        // 如需如何啟用帳戶確認和密碼重設的詳細資訊，請造訪 http://go.microsoft.com/fwlink/?LinkID=320771
                        // 傳送包含此連結的電子郵件
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "確認您的帳戶", "請按一下此連結確認您的帳戶 <a href=\"" + callbackUrl + "\">這裏</a>");

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {

                    return View();
                }

            }
            return View();
        }


        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = UserManager.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            GetUserRoles(user);
            return View(user);
        }

        private void GetUserRoles(ApplicationUser user)
        {
            var userRoles = UserManager.GetRoles(user.Id);
            ViewBag.UserInRoles = RoleManager.Roles.ToList().Select(x => new SelectListItem()
            {
                Selected = userRoles.Contains(x.Name),
                Text = x.Name,
                Value = x.Name
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUser editUser, params string[] selectedRole)
        {
            var user = UserManager.FindById(editUser.Id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var userRoles = UserManager.GetRoles(user.Id);
            if (ModelState.IsValid)
            {

                user.UserName = editUser.UserName;
                user.Email = editUser.Email;
                user.PhoneNumber = editUser.PhoneNumber;
                if (editUser.LockoutEnabled)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEndDateUtc = DateTime.UtcNow.AddYears(10);
                }
                else
                {
                    user.LockoutEnabled = false;
                    user.LockoutEndDateUtc = DateTime.UtcNow.AddDays(-1);
                }

                selectedRole = selectedRole ?? new string[] { };
                //全殺
                foreach (var role in selectedRole.Except(userRoles).ToArray())
                {
                    var result = UserManager.AddToRole(user.Id, role);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        GetUserRoles(user);
                        return View(user);
                    }
                }
                //全加
                foreach (var role in userRoles.Except(selectedRole).ToArray())
                {
                    var result = UserManager.RemoveFromRole(user.Id, role);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        GetUserRoles(user);
                        return View(user);
                    }
                }
                var vaild = UserManager.Update(user);
                if (vaild.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var item in vaild.Errors)
                    {
                        ModelState.AddModelError("", item);
                    }
                }
            }
            GetUserRoles(user);
            return View(editUser);
        }
        public ActionResult Delete(string id)
        {
            //HACK 此範例接受使用 Get 來刪除角色，這是非常不安全的作法，請配合現有機制調整

            var user = UserManager.FindById(id);
            UserManager.Delete(user);
            return RedirectToAction("Index");

        }


        public ActionResult EditPassword(Guid id)
        {
            var user = UserManager.FindById(id.ToString());
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewData["id"] = id;
            ViewBag.UserName = user.UserName;
            return View();
        }

        [HttpPost]
        public ActionResult EditPassword(Guid id, string newPw)
        {
            var errorMsg = new List<string>();
            if (ModelState.IsValid)
            {
                //Update Password是要先Remove後在AddPassword
                var result = UserManager.RemovePassword(id.ToString());
                if (result.Succeeded)
                {
                    var changeState = UserManager.AddPassword(id.ToString(), newPw);
                    if (changeState.Succeeded)
                    {
                        return Content("新密碼為【" + newPw + "】<br />本系統密碼皆為加密保護，這是您唯一一次可以看到明碼。");
                    }
                }
            }
            return RedirectToAction("Index");
        }

    }
}