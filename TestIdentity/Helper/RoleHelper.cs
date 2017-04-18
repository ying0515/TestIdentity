using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TestIdentity.Models;

namespace TestIdentity.Helper
{
    public static class RoleHelper
    {
        public static void AddToUserRole(this ApplicationUserManager appUserManager,
            string userId,
            string roleName)
        {
            var roleManager = new RoleManager<IdentityRole>
                      (new RoleStore<IdentityRole>
                          (new ApplicationDbContext()));

            if (roleManager.RoleExists(roleName) == false)
            {
                roleManager.Create(new IdentityRole(roleName));
            }
            appUserManager.AddToRole(userId, roleName);
        }
    }
}