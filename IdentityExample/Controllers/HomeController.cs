using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityExample.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace IdentityExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(AppDbContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // login functionality
            var user = await _userManager.FindByNameAsync(username);

            if(user != null)
            {
               var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }

            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            // register functionality
            var user = new IdentityUser
            {
                UserName = username,
                Email = username
            };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // generation of the email token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var link = Url.Action(nameof(VerifyEmail), "Home", new { userId = user.Id, code}, Request.Scheme, Request.Host.ToString());

                await _emailService.SendAsync("test@gmail.com", "Click to verity your email address", $"<a href=\"{link}\"> Verify Email </a>", true);

                return RedirectToAction("EmailVerification");
            }

            return View();
        }

        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return View();
            }

            return BadRequest();
        }

        public IActionResult EmailVerification() => View();

        // reset password
        public IActionResult ForgetPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            // generation of the email token

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return View("ResetPasswordMessage");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var link = Url.Action(nameof(ResetPassword), "Home", new { userId = user.Id, code }, Request.Scheme, Request.Host.ToString());

            await _emailService.SendAsync(email, "Click to reset your password", $"<a href=\"{link}\"> Reset Password </a>", true);

            return View("ResetPasswordMessage");
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string code)
        {
            return View(new {userId = userId, code = code });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userId, string code, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return View();

            var result = await _userManager.ResetPasswordAsync(user, code, password);

            if (result.Succeeded)
            {
                return RedirectToAction("Home", "Index");
            }

            return View();
        }
    }
}
