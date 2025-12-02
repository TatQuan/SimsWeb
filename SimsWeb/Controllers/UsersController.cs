using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IAdminUserService adminUserService;

        public UsersController(IAdminUserService adminUserService)
        {
            this.adminUserService = adminUserService;
        }

        // GET: /AdminUsers
        public async Task<IActionResult> Index()
        {
            var model = await adminUserService.GetActiveUsersAsync();
            return View(model);
        }

        // GET: /AdminUsers/Deleted
        public async Task<IActionResult> Deleted()
        {
            var model = await adminUserService.GetDeletedUsersAsync();
            return View(model);
        }

        // GET: /AdminUsers/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await adminUserService.GetCreateViewModelAsync();
            return View(vm);
        }

        // POST: /AdminUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // load lại RoleList
                var vm = await adminUserService.GetCreateViewModelAsync();
                model.RoleList = vm.RoleList;
                return View(model);
            }

            var (success, errors) = await adminUserService.CreateUserAsync(model);

            if (!success)
            {
                var vm = await adminUserService.GetCreateViewModelAsync();
                model.RoleList = vm.RoleList;

                foreach (var e in errors)
                    ModelState.AddModelError("", e);

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminUsers/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var vm = await adminUserService.GetEditViewModelAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        // POST: /AdminUsers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // load lại RoleList cho dropdown
                var vm = await adminUserService.GetEditViewModelAsync(model.Id);
                if (vm != null)
                    model.RoleList = vm.RoleList;

                return View(model);
            }

            var (success, errors) = await adminUserService.UpdateUserAsync(model);

            if (!success)
            {
                var vm = await adminUserService.GetEditViewModelAsync(model.Id);
                if (vm != null)
                    model.RoleList = vm.RoleList;

                foreach (var e in errors)
                    ModelState.AddModelError("", e);

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminUsers/Delete (soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var ok = await adminUserService.SoftDeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminUsers/Restore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var ok = await adminUserService.RestoreAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Deleted));
        }

        // POST: /AdminUsers/HardDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(string id)
        {
            var ok = await adminUserService.HardDeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Deleted));
        }
    }
}
