using DesignTechPLMIntegrationPro.Application.Services;
using DesignTechPLMIntegrationPro.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using DesignTechPLMIntegrationPro.Application.Interfaces.OzellestirmeModulu.Setup;

namespace DesignTechPLMIntegrationPro.MVC.Controllers
{
    public class LdapController : Controller
    {
        private readonly LdapService _ldapService;
		private readonly ISqlSetupService _sqlSetupService;
		private readonly IConfiguration _configuration;

		public LdapController(LdapService ldapService, ISqlSetupService sqlSetupService, IConfiguration configuration)
		{
			_ldapService = ldapService;
			_sqlSetupService = sqlSetupService;
			_configuration = configuration;
		}
		[HttpGet]
		public IActionResult Index()
        {
			if (_sqlSetupService.IsConnectionStringValid(out var connectionStringValid))
			{
				return RedirectToAction("Index", "Setup");
			}

			if (_sqlSetupService.IsCreatedTableValid(out var createdTableConnectionString))
			{
				return RedirectToAction("Install", "Setup");
			}

			return View();
		}

        [HttpPost]
        public async Task<IActionResult> Login(LdapSettings ldapSettings)
        {

			if (_sqlSetupService.IsConnectionStringValid(out var connectionStringValid))
			{
				TempData["InfoMessage"] = "Kurulumda tamamlanması gereken alanlar var.";
				return RedirectToAction("Index", "Setup");
			}

			if (_sqlSetupService.IsCreatedTableValid(out var createdTableConnectionString))
			{
				TempData["InfoMessage"] = "Kurulumda tamamlanması gereken alanlar var.";
				return RedirectToAction("Install", "Setup");
			}

			// LDAP kimlik doğrulaması
			var isAuthenticated = _ldapService.AuthenticateUser(
                ldapSettings.Username,
                ldapSettings.Password,
                ldapSettings.Server,
                ldapSettings.Port
            );

            if (isAuthenticated)
            {
                // Oturumu başlat

                var claims = new List<Claim>
                 {
                    new Claim(ClaimTypes.Name, ldapSettings.Username),
                    new Claim(ClaimTypes.Dns, ldapSettings.Server),
                    // Diğer talepleri ekleyin (örneğin, rol, izinler vb.)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal
                );

                // Ana sayfaya yönlendir
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Kimlik doğrulaması başarısız oldu

                TempData["ErrorMessage"] = "Geçersiz kullanıcı adı veya şifre";
                return View("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Ldap");
        }





	}







}