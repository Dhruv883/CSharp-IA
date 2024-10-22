using System;
using System.Web.Mvc;
using System.Web.Security;
using BankManagement.Models;
using System.Linq;
using System.Collections.Generic;

namespace BankManagement.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real application, validate against your database
                // This is just a sample implementation
                if (IsValidUser(model.Email, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);
                    return RedirectToAction("Index", "Dashboard");
                }
                ModelState.AddModelError("", "Invalid email or password");
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real application, save to your database
                // This is just a sample implementation
                if (RegisterUser(model))
                {
                    FormsAuthentication.SetAuthCookie(model.Email, false);
                    return RedirectToAction("Index", "Dashboard");
                }
                ModelState.AddModelError("", "Email already exists");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        private bool IsValidUser(string email, string password)
        {
            // Implement your authentication logic here
            // This is just a sample - DO NOT use in production
            return email == "test@example.com" && password == "password123";
        }

        public ActionResult Details()
        {
            // Sample account details - replace with actual data from your database
            var model = new AccountDetailsViewModel
            {
                AccountHolder = "John Doe",
                AccountNumber = "1234567890",
                Balance = 5000.00m,
                AccountType = "Savings",
                Status = "Active"
            };
            return View(model);
        }


        private bool RegisterUser(RegisterViewModel model)
        {
            // Implement your user registration logic here
            // Return false if email already exists
            // Return true if registration successful
            return true;
        }
    }
}