using Amazon.Extensions.CognitoAuthentication;
using Amazon.AspNetCore.Identity.Cognito;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public AccountsController(SignInManager<CognitoUser> signinManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signinManager;
            _userManager = userManager;
            _pool = pool;
        }
        public IActionResult Signup()
        {
            return View(new SignupModel());
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel signupModel)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(signupModel.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User already exists");
                    return View(signupModel);
                }

                user.Attributes.Add(CognitoAttribute.Name.AttributeName, signupModel.Email);
                var userCreated = await _userManager.CreateAsync(user, signupModel.Password);

                if (userCreated.Succeeded)
                {
                    RedirectToAction("Confirm");
                }

            }
            return View();
        }

        public IActionResult Confirm()
        {
            return View(new ConfirmModel());
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "Email not found");
                    return View(model);
                }

                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code, true);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }

                    return View(model);
                }
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else 
                {
                    ModelState.AddModelError("LoginError", "Login Error");
                }
            }
            return View(model);
        }
    }
}
