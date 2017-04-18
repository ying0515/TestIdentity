using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity.Owin;  //請加入
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TestIdentity.Models;

namespace TestIdentity.Areas.Admin.Controllers
{
    public class RoleController : Controller
    {
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

        // GET: Role
        public ActionResult Index()
        {
            var source = RoleManager.Roles.ToList();
            return View(source);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(IdentityRole roleViewModel)
        {
            if (ModelState.IsValid)
            {
                var role = new ApplicationRole(roleViewModel.Name);
                var roleresult = await RoleManager.CreateAsync(role);
                if (!roleresult.Succeeded)
                {
                    ModelState.AddModelError("", roleresult.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(IdentityRole roleModel)
        {
            if (ModelState.IsValid)
            {
                var role = await RoleManager.FindByIdAsync(roleModel.Id);
                role.Name = roleModel.Name;
                await RoleManager.UpdateAsync(role);
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<ActionResult> Details(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            return View(role.Users);
        }

        public async Task<ActionResult> Delete(string id)
        {
            //HACK 此範例接受使用 Get 來刪除角色群組，這是非常不安全的作法，請配合現有機制調整
            var role = await RoleManager.FindByIdAsync(id);
            await RoleManager.DeleteAsync(role);
            return RedirectToAction("Index");
        }
    }
}
