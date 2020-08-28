using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Basic.CustomPolicyProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public HomeController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }

        [SecurityLevel(5)]
        public IActionResult SecretLevel()
        {
            return View("Secret");
        }
        [RankLevel(10)]
        public IActionResult SecretHigherLevel()
        {
            return View("Secret");
        }

        [Authorize(Policy = "Claim.DoB")]
        public IActionResult SecretPolicy()
        {
            return View("Secret");
        }

        [AllowAnonymous]
        public IActionResult Authenticate()
        {
            var grandmaClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Farhan"),
                new Claim(ClaimTypes.Email, "km1472233@gmail.com"),
                new Claim(ClaimTypes.DateOfBirth, "10-10-1997"),
                new Claim(DynamicPolicies.SecurityLevel, "7"),
                new Claim(DynamicPolicies.Rank, "12")
            };

            var licenseClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Farhan Kaioum"),
                new Claim("DrivingLicense", "da-metro-kha-1020")
            };

            var gramdmaIdentity = new ClaimsIdentity(grandmaClaims, "Grandma Identity");
            var licenseIdentity = new ClaimsIdentity(licenseClaims, "Govt");

            var userPrincipal = new ClaimsPrincipal(new[] { gramdmaIdentity, licenseIdentity });

            HttpContext.SignInAsync(userPrincipal);

            return RedirectToAction("Index");
        }

        // works with AuthorizationService
        public async Task<IActionResult> DoStuff()
        {
            var builder = new AuthorizationPolicyBuilder("Schema");
            var customPolicy = builder.RequireClaim("Hello").Build();


            var authResult = await _authorizationService.AuthorizeAsync(User, customPolicy);

            if (authResult.Succeeded)
            {

            }

            return View("Index");
        }
    }
}
