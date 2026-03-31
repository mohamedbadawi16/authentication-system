using AuthenticationSystem.Web.Models;
using AuthenticationSystem.Web.Services;
using AuthenticationSystem.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AuthenticationSystem.Web.Controllers;

public sealed class AuthController : Controller
{
    private readonly AuthApiClient _api;

    public AuthController(AuthApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _api.RegisterAsync(new RegisterRequest(model.Email, model.Password), cancellationToken);
        if (result.Success)
        {
            TempData["Success"] = "We've sent a one-time code to your email.";
            return RedirectToAction(nameof(VerifyOtp), new { email = model.Email });
        }

        ApplyErrors(result);
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _api.LoginAsync(new LoginRequest(model.Email, model.Password), cancellationToken);
        if (result.Success && result.Data is not null)
        {
            TempData["Success"] = "Login successful.";
            StoreSession(result.Data);
            return RedirectToAction("Index", "Home");
        }

        if (result.ErrorCode == "email_not_verified")
        {
            TempData["Warning"] = "Email verification required. Check your inbox for the code.";
            return RedirectToAction(nameof(VerifyOtp), new { email = model.Email });
        }

        ApplyErrors(result);
        return View(model);
    }

    [HttpGet]
    public IActionResult VerifyOtp(string? email)
    {
        return View(new VerifyOtpViewModel { Email = email ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _api.VerifyOtpAsync(new VerifyOtpRequest(model.Email, model.Code), cancellationToken);
        if (result.Success && result.Data is not null)
        {
            TempData["Success"] = "Email verified. You are now signed in.";
            StoreSession(result.Data);
            return RedirectToAction("Index", "Home");
        }

        if (result.ErrorCode == "otp_expired")
        {
            ModelState.AddModelError(nameof(model.Code), "That code has expired. Request a new one.");
            return View(model);
        }

        if (result.ErrorCode == "otp_invalid")
        {
            ModelState.AddModelError(nameof(model.Code), "Invalid code. Please try again.");
            return View(model);
        }

        ApplyErrors(result);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string email, CancellationToken cancellationToken)
    {
        var result = await _api.ResendOtpAsync(new ResendOtpRequest(email), cancellationToken);
        if (result.Success)
        {
            TempData["Success"] = "If the account exists, a new OTP has been sent.";
            return RedirectToAction(nameof(VerifyOtp), new { email });
        }

        if (result.ErrorCode == "otp_rate_limited")
        {
            TempData["Warning"] = result.ErrorMessage ?? "Too many requests. Try again later.";
            return RedirectToAction(nameof(VerifyOtp), new { email });
        }

        TempData["Warning"] = result.ErrorMessage ?? "Unable to send OTP.";
        return RedirectToAction(nameof(VerifyOtp), new { email });
    }

    private void ApplyErrors<T>(ApiResult<T> result)
    {
        if (result.ValidationErrors is not null)
        {
            foreach (var (key, messages) in result.ValidationErrors)
            {
                foreach (var message in messages)
                {
                    ModelState.AddModelError(key, message);
                }
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
        }
    }

    private void StoreSession(AuthResponse auth)
    {
        HttpContext.Session.SetString("auth:token", auth.Token);
        HttpContext.Session.SetString("auth:email", auth.Email);
        HttpContext.Session.SetString("auth:expires", auth.ExpiresAt.ToString("u"));
    }
}
