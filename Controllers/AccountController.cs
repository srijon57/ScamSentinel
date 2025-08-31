using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SupabaseService _supabaseService;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;

    public AccountController(SupabaseService supabaseService, EmailService emailService, IConfiguration configuration)
    {
        _supabaseService = supabaseService;
        _emailService = emailService;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (model.Password != model.ConfirmPassword)
        {
            TempData["ErrorMessage"] = "Password and Confirm Password do not match.";
            return View(model);
        }

        if (model.Password.Length < 6)
        {
            TempData["ErrorMessage"] = "Password must be at least 6 characters long.";
            return View(model);
        }

        if (!model.Email.EndsWith("@gmail.com"))
        {
            TempData["ErrorMessage"] = "Please provide a valid Gmail address.";
            return View(model);
        }

        try
        {
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            // Check if the username or email already exists
            var existingUser = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE UserName = @UserName OR Email = @Email",
                new { model.UserName, model.Email }
            );

            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "Username or email already exists.";
                return View(model);
            }

            // Hash the password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Insert the new user with the hashed password
            var userId = await connection.ExecuteScalarAsync<int>(
                "INSERT INTO Users (UserName, Email, PasswordHash, ContactNumber) VALUES (@UserName, @Email, @PasswordHash, @ContactNumber) RETURNING UserID",
                new { model.UserName, model.Email, PasswordHash = passwordHash, model.ContactNumber }
            );

            // Generate OTP and send email for verification
            var otpCode = new Random().Next(100000, 999999).ToString();
            await connection.ExecuteAsync(
                "INSERT INTO OTP (UserID, OTPCode, ExpiresAt) VALUES (@UserID, @OTPCode, @ExpiresAt)",
                new { UserID = userId, OTPCode = otpCode, ExpiresAt = DateTime.UtcNow.AddMinutes(10) }
            );

            await _emailService.SendOTPEmail(model.Email, otpCode);

            TempData["SuccessMessage"] = "Registration successful! Please verify your email with the OTP sent.";
            return RedirectToAction("VerifyOTP", new { email = model.Email });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Registration failed. Please try again.";
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult VerifyOTP(string email)
    {
        return View(new VerifyOTPModel { Email = email });
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOTP(VerifyOTPModel model)
    {
        try
        {
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            var otp = await connection.QueryFirstOrDefaultAsync<OTP>(
                "SELECT * FROM OTP WHERE UserID = (SELECT UserID FROM Users WHERE Email = @Email) AND OTPCode = @OTPCode AND ExpiresAt > @Now AND IsUsed = FALSE",
                new { model.Email, model.OTPCode, Now = DateTime.UtcNow }
            );

            if (otp == null)
            {
                TempData["ErrorMessage"] = "Invalid OTP or expired.";
                return View("VerifyOTP", new VerifyOTPModel { Email = model.Email });
            }

            // Mark OTP as used and verify user
            await connection.ExecuteAsync(
                "UPDATE OTP SET IsUsed = TRUE WHERE OTPID = @OTPID",
                new { otp.OTPID }
            );

            await connection.ExecuteAsync(
                "UPDATE Users SET IsVerified = TRUE WHERE Email = @Email",
                new { model.Email }
            );

            // Generate JWT token
            var user = await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Email = @Email", new { model.Email });
            var token = GenerateJwtToken(user);
            Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true, Secure = true });

            TempData["SuccessMessage"] = "Email verified successfully!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred. Please try again.";
            return View("VerifyOTP", new VerifyOTPModel { Email = model.Email });
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        try
        {
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            // Fetch the user by email
            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { model.Email }
            );

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Invalid email or password.";
                return View(model);
            }

            // Generate JWT tokens
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Store tokens in cookies
            Response.Cookies.Append("access_token", accessToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTime.UtcNow.AddDays(7) });

            TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Login failed. Please try again.";
            return View(model);
        }
    }
    [HttpPost]
    public IActionResult Logout()
    {
        // Delete cookies
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        TempData["SuccessMessage"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.SuperUser ? "Admin" : "User")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}