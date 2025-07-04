using Auth.MVC.DTO;
using Auth.MVC.Interfaces;
using Auth.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace Auth.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthServices _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthServices authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        // GET: /Auth/Login 
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LoginResult = new LoginDTO
                {
                    IsSuccess = false,
                    Message = "Invalid login attempt."
                };
                _logger.LogWarning("Login attempt failed due to invalid model state.");
                return View(model);
            }
            LoginViewModel loginVM = new LoginViewModel()
            {
                Email = model.Email,
                Password = model.Password
            };
            _logger.LogInformation("Attempting to log in user with email: {Email}", loginVM.Email);
            var result = await _authService.LoginAsync(loginVM);

            if (result.IsSuccess)
            {

                ViewBag.LoginResult = new LoginDTO
                {
                    IsSuccess = result.IsSuccess,
                    Message = result.Message,
                    UserName = result.UserName,
                    Email = result.Email,
                    Token = result.Token
                };
                _logger.LogInformation("User logged in successfully: {Email}", result.Email);

                return RedirectToAction("CachedEmails", "Auth");
            }
            else
            {
                _logger.LogInformation($"Login failed: {result.Message}");
                ViewBag.LoginResult = new LoginDTO
                {
                    IsSuccess = false,
                    Message = "Invalid email or password"
                };
                return View("Login");
            }


        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid data model.... failed to register user");
                ViewBag.RegisterResult = new LoginDTO
                {
                    IsSuccess = false,
                    Message = "Invalid registration attempt."
                };
                return View(model);
            }

            RegisterViewModel registerDTO = new RegisterViewModel()
            {
                Email = model.Email,
                UserName = model.UserName,
                Password = model.Password
            };

            _logger.LogInformation($"Registering new user with email: {registerDTO.Email}");
            var result = await _authService.RegisterAsync(registerDTO);

            ViewBag.RegisterResult = new LoginDTO
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message,
            };
            _logger.LogInformation($"User with {registerDTO.Email} email registered successfully");
            return View("Register");
        }

        // GET: /Auth/CachedEmails
        [HttpGet("cached-emails")]
        public async Task<IActionResult> CachedEmails()
        {
            try
            {
                _logger.LogInformation("Getting emails from catch");

                var result = await _authService.GetCacheEmail();

                if (result == null)
                {
                    ViewBag.CachedEmails = new EmailListDTO { Error = "Service returned null" };
                }
                else if (result.Emails?.Any() == true)
                {
                    ViewBag.CachedEmails = result; 
                }
                else
                {
                    ViewBag.CachedEmails = new EmailListDTO
                    {
                        Message = result.Message ?? "No emails found"
                    };
                }

                return View("CachedEmails");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CachedEmails");
                ViewBag.CachedEmails = new EmailListDTO { Error = "An error occurred" };
                return View("CachedEmails");
            }
        }
    }
}