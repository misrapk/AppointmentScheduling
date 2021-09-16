
using System.Threading.Tasks;
using AppointmentScheduling.Models;
using AppointmentScheduling.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppointmentScheduling.Helper;

namespace AppointmentScheduling.Controllers
{
	public class AccountController : Controller
	{
		//access to db
		public readonly ApplicationDbContext _db;
		UserManager<ApplicationUser> _userManager;
		SignInManager<ApplicationUser> _signInManager;
		RoleManager<IdentityRole> _roleManager;


		
		public AccountController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
			_signInManager = signInManager;
		}
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RemeberMe, false);
				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Appointment");
				}
				ModelState.AddModelError("", "Invalid Login! Please Register if you are a new User!");
			}
			return View(model);
		}

		public async Task<IActionResult> Register()
		{
			//create roles when Get method is called
			if (!_roleManager.RoleExistsAsync(Helper.Helper.admin).GetAwaiter().GetResult())
			{
				await _roleManager.CreateAsync(new IdentityRole(Helper.Helper.admin));
				await _roleManager.CreateAsync(new IdentityRole(Helper.Helper.Doctor));
				await _roleManager.CreateAsync(new IdentityRole(Helper.Helper.Patient));
			}
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			//server side validation
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					Name = model.Name
				};

				//create user in database
				var result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(user, model.RoleName);
					await _signInManager.SignInAsync(user, isPersistent: false);
					return RedirectToAction("Index", "Home");
				}

				//for validation of input
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View();
		}

		//for logout functionality
		[HttpPost]
		public async Task<IActionResult> Logoff()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Login", "Account");
		}
	}
}
