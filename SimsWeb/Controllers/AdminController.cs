using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Models;
using SimsWeb.ViewModels;

namespace SimsWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminController(UserManager<Users> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // /Admin
        public IActionResult Index()
        {
            return View();
        }

        


        // TODO: sau này thêm:
        // EditUser, EditRoles, DeleteUser, v.v.
    }
}
