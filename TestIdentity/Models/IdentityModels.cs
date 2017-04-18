using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace TestIdentity.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {

        [Required]
        [Display(Name = "暱稱")]
        public string NickName { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // 注意 authenticationType 必須符合 CookieAuthenticationOptions.AuthenticationType 中定義的項目
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在這裡新增自訂使用者宣告
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<TestIdentity.Models.ApplicationRole> IdentityRoles { get; set; }
    }

    public class ApplicationUserRole : IdentityUserRole
    {

    }
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }

        public ApplicationRole(string name) : base(name) { }

        // add feild



    }
}