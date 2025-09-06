using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using ScamSentinel.Models.Account;
using ScamSentinel.Models.Scam;
using ScamSentinel.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using Newtonsoft.Json;
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SupabaseService _supabaseService;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly CloudinaryService _cloudinaryService;

    public AccountController(SupabaseService supabaseService, EmailService emailService, IConfiguration configuration, CloudinaryService cloudinaryService)
    {
        _supabaseService = supabaseService;
        _emailService = emailService;
        _configuration = configuration;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        // Validate email format
        if (!model.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Please provide a valid Gmail address.";
            return View(model);
        }

        // Validate username is not empty
        if (string.IsNullOrWhiteSpace(model.UserName))
        {
            TempData["ErrorMessage"] = "Username cannot be empty.";
            return View(model);
        }

        // Validate username doesn't start with spaces
        if (model.UserName.StartsWith(" "))
        {
            TempData["ErrorMessage"] = "Username cannot start with spaces.";
            return View(model);
        }

        // Validate username doesn't contain special characters
        if (!Regex.IsMatch(model.UserName, @"^[a-zA-Z0-9_]+$"))
        {
            TempData["ErrorMessage"] = "Username can only contain letters, numbers, and underscores.";
            return View(model);
        }

        // Validate password length
        if (model.Password.Length < 6)
        {
            TempData["ErrorMessage"] = "Password must be at least 6 characters long.";
            return View(model);
        }

        // Validate password confirmation
        if (model.Password != model.ConfirmPassword)
        {
            TempData["ErrorMessage"] = "Password and Confirm Password do not match.";
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
                if (existingUser.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Username already exists.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Email already exists.";
                }
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
            // Log the exception (you should implement proper logging)
            TempData["ErrorMessage"] = "Registration failed due to a server error. Please try again.";
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
    [Authorize]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        if (!User.Identity.IsAuthenticated)
        {
            TempData["ErrorMessage"] = "You are not authenticated. Please log in.";
            return RedirectToAction("Login");
        }

        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = userEmail }
            );

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login");
            }

            var profileModel = new ProfileModel
            {
                UserName = user.UserName,
                Email = user.Email,
                ContactNumber = user.ContactNumber
            };

            return View(profileModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred. Please try again.";
            return RedirectToAction("Login");
        }
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateProfile(ProfileModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Profile", model);
        }

        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            await connection.ExecuteAsync(
                "UPDATE Users SET UserName = @UserName, ContactNumber = @ContactNumber WHERE Email = @Email",
                new { model.UserName, model.ContactNumber, Email = userEmail }
            );

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Failed to update profile. Please try again.");
            return View("Profile", model);
        }
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        if (model.NewPassword != model.ConfirmNewPassword)
        {
            ModelState.AddModelError("ConfirmNewPassword", "New password and confirmation do not match.");
            return View(model);
        }

        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = userEmail }
            );

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            await connection.ExecuteAsync(
                "UPDATE Users SET PasswordHash = @PasswordHash WHERE Email = @Email",
                new { PasswordHash = newPasswordHash, Email = userEmail }
            );

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Failed to change password. Please try again.");
            return View(model);
        }
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> PostScam()
    {
        try
        {
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            var scamTypes = await connection.QueryAsync<ScamType>("SELECT * FROM ScamTypes ORDER BY TypeName");

            var model = new ScamReportModel
            {
                AvailableScamTypes = scamTypes.Select(st => new SelectListItem
                {
                    Value = st.ScamTypeID.ToString(),
                    Text = st.TypeName
                }).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while loading the form. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostScam(ScamReportModel model)
    {
        try
        {
            // Reload scam types for the view in case of validation failure
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            var scamTypes = await connection.QueryAsync<ScamType>("SELECT * FROM ScamTypes ORDER BY TypeName");
            model.AvailableScamTypes = scamTypes.Select(st => new SelectListItem
            {
                Value = st.ScamTypeID.ToString(),
                Text = st.TypeName
            }).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Get user ID
            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT UserID FROM Users WHERE Email = @Email",
                new { Email = userEmail }
            );

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login");
            }

            // Create a proper class for scammer info
            var scammerInfo = new ScammerInfo
            {
                Phone = model.ScammerPhone,
                WhatsApp = model.ScammerWhatsApp,
                Email = model.ScammerEmail,
                Facebook = model.ScammerFacebook,
                Name = model.ScammerName,
                Organization = model.ScammerOrganization
            };

            // Initialize evidence picture links
            string evidenceLink1 = null;
            string evidenceLink2 = null;
            string evidenceLink3 = null;
            string evidenceLink4 = null;
            string evidenceLink5 = null;

            // Handle evidence uploads to Cloudinary
            if (model.EvidenceFiles != null && model.EvidenceFiles.Count > 0)
            {
                var cloudinaryService = HttpContext.RequestServices.GetService<CloudinaryService>();
                if (cloudinaryService == null)
                {
                    throw new Exception("Cloudinary service not available");
                }

                int fileCount = 0;
                foreach (var file in model.EvidenceFiles.Take(5))
                {
                    if (file.Length > 0)
                    {
                        try
                        {
                            var uploadResult = await cloudinaryService.UploadImageAsync(file);
                            if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                fileCount++;
                                switch (fileCount)
                                {
                                    case 1:
                                        evidenceLink1 = uploadResult.Url.ToString();
                                        break;
                                    case 2:
                                        evidenceLink2 = uploadResult.Url.ToString();
                                        break;
                                    case 3:
                                        evidenceLink3 = uploadResult.Url.ToString();
                                        break;
                                    case 4:
                                        evidenceLink4 = uploadResult.Url.ToString();
                                        break;
                                    case 5:
                                        evidenceLink5 = uploadResult.Url.ToString();
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log file upload error but continue with other files
                            Console.WriteLine($"Error uploading file {file.FileName}: {ex.Message}");
                        }
                    }
                }
            }

            // Insert scam report with evidence links
            var reportId = await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO ScamReports 
            (UserID, Title, ScamTypeID, Description, ScammerInfo, LossAmount, Currency, Location, OccurrenceDate,
             Evidencepicturelink1, Evidencepicturelink2, Evidencepicturelink3, Evidencepicturelink4, Evidencepicturelink5) 
            VALUES (@UserID, @Title, @ScamTypeID, @Description, @ScammerInfo::jsonb, @LossAmount, @Currency, @Location, @OccurrenceDate,
                    @EvidenceLink1, @EvidenceLink2, @EvidenceLink3, @EvidenceLink4, @EvidenceLink5)
            RETURNING ReportID",
                new
                {
                    UserID = user.UserID,
                    model.Title,
                    model.ScamTypeID,
                    model.Description,
                    ScammerInfo = Newtonsoft.Json.JsonConvert.SerializeObject(scammerInfo),
                    model.LossAmount,
                    model.Currency,
                    model.Location,
                    model.OccurrenceDate,
                    EvidenceLink1 = evidenceLink1,
                    EvidenceLink2 = evidenceLink2,
                    EvidenceLink3 = evidenceLink3,
                    EvidenceLink4 = evidenceLink4,
                    EvidenceLink5 = evidenceLink5
                }
            );

            TempData["SuccessMessage"] = "Scam report submitted successfully! It will be reviewed by our team.";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            // Log the actual error for debugging
            Console.WriteLine($"Error in PostScam: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            TempData["ErrorMessage"] = $"An error occurred while submitting your report: {ex.Message}";

            // Reload scam types for the view
            using var connection = _supabaseService.CreateConnection();
            await ((NpgsqlConnection)connection).OpenAsync();

            var scamTypes = await connection.QueryAsync<ScamType>("SELECT * FROM ScamTypes ORDER BY TypeName");
            model.AvailableScamTypes = scamTypes.Select(st => new SelectListItem
            {
                Value = st.ScamTypeID.ToString(),
                Text = st.TypeName
            }).ToList();

            return View(model);
        }
    }

    // Add this method to AccountController
    [AllowAnonymous]
    public IActionResult ScamList(int page = 1, string search = "", int? scamType = null)
    {
        try
        {
            using var connection = _supabaseService.CreateConnection();
            connection.Open();

            var viewModel = new ScamListViewModel
            {
                CurrentPage = page,
                PageSize = 7,
                SearchTerm = search,
                ScamTypeFilter = scamType
            };

            // Build WHERE clause for filters
            var whereClause = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(search))
            {
                whereClause.Add("(sr.Title ILIKE @Search OR sr.Description ILIKE @Search)");
                parameters.Add("Search", $"%{search}%");
            }

            if (scamType.HasValue)
            {
                whereClause.Add("sr.ScamTypeID = @ScamTypeID");
                parameters.Add("ScamTypeID", scamType.Value);
            }

            string whereSql = whereClause.Any() ? "WHERE " + string.Join(" AND ", whereClause) : "";

            // Get total count for pagination
            var totalCount = connection.ExecuteScalar<int>($@"
            SELECT COUNT(*) FROM ScamReports sr
            {whereSql}
        ", parameters);

            viewModel.TotalPages = (int)Math.Ceiling(totalCount / (double)viewModel.PageSize);

            // Get scam reports with pagination
            parameters.Add("Offset", (page - 1) * viewModel.PageSize);
            parameters.Add("Limit", viewModel.PageSize);

            var scamReports = connection.Query($@"
            SELECT sr.ReportID, sr.Title, sr.Description, sr.ScammerInfo, 
                   sr.Upvotes, sr.Downvotes, sr.CreatedAt,
                   st.ScamTypeID, st.TypeName as ScamTypeName
            FROM ScamReports sr
            LEFT JOIN ScamTypes st ON sr.ScamTypeID = st.ScamTypeID
            {whereSql}
            ORDER BY sr.CreatedAt DESC
            LIMIT @Limit OFFSET @Offset
        ", parameters);

            // Convert to strongly typed list
            foreach (var report in scamReports)
            {
                ScammerInfo scammerInfo;
                try
                {
                    // Handle null or empty ScammerInfo
                    var scammerInfoJson = report.scammerinfo?.ToString();
                    scammerInfo = !string.IsNullOrEmpty(scammerInfoJson)
                        ? JsonConvert.DeserializeObject<ScammerInfo>(scammerInfoJson)
                        : new ScammerInfo();
                }
                catch
                {
                    scammerInfo = new ScammerInfo();
                }

                var scamReport = new ScamReport
                {
                    ReportID = report.reportid,
                    Title = report.title,
                    Description = report.description,
                    ScammerInfo = scammerInfo,
                    Upvotes = report.upvotes,
                    Downvotes = report.downvotes,
                    CreatedAt = report.createdat,
                    ScamTypeID = report.scamtypeid,
                    ScamTypeName = report.typename
                };

                // Check if user has voted on this report
                if (User.Identity.IsAuthenticated)
                {
                    var userEmail = User.FindFirstValue(ClaimTypes.Email);
                    var user = connection.QueryFirstOrDefault<User>(
                        "SELECT UserID FROM Users WHERE Email = @Email",
                        new { Email = userEmail }
                    );

                    if (user != null)
                    {
                        var vote = connection.QueryFirstOrDefault<Vote>(
                            "SELECT * FROM Votes WHERE UserID = @UserID AND ReportID = @ReportID",
                            new { UserID = user.UserID, ReportID = report.reportid }
                        );

                        scamReport.UserVote = vote != null ? (vote.IsUpvote ? 1 : -1) : (int?)null;
                    }
                }

                viewModel.ScamReports.Add(scamReport);
            }

            // Get available scam types for filter dropdown
            var scamTypes = connection.Query<ScamType>("SELECT * FROM ScamTypes ORDER BY TypeName");
            viewModel.AvailableScamTypes = scamTypes.ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            // Log the actual error for debugging
            Console.WriteLine($"Error in ScamList: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            TempData["ErrorMessage"] = "An error occurred while loading scam reports.";
            return View(new ScamListViewModel());
        }
    }

    // Add vote actions
    [Authorize]
    [HttpPost]
    public IActionResult Vote(int reportId, bool isUpvote)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            using var connection = _supabaseService.CreateConnection();
            connection.Open();

            // Get user ID
            var user = connection.QueryFirstOrDefault<User>(
                "SELECT UserID FROM Users WHERE Email = @Email",
                new { Email = userEmail }
            );

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Check if user already voted
            var existingVote = connection.QueryFirstOrDefault<Vote>(
                "SELECT * FROM Votes WHERE UserID = @UserID AND ReportID = @ReportID",
                new { UserID = user.UserID, ReportID = reportId }
            );

            using var transaction = connection.BeginTransaction();

            try
            {
                if (existingVote != null)
                {
                    // Remove previous vote
                    if (existingVote.IsUpvote)
                    {
                        connection.Execute(
                            "UPDATE ScamReports SET Upvotes = Upvotes - 1 WHERE ReportID = @ReportID",
                            new { ReportID = reportId }, transaction
                        );
                    }
                    else
                    {
                        connection.Execute(
                            "UPDATE ScamReports SET Downvotes = Downvotes - 1 WHERE ReportID = @ReportID",
                            new { ReportID = reportId }, transaction
                        );
                    }

                    // Delete old vote
                    connection.Execute(
                        "DELETE FROM Votes WHERE VoteID = @VoteID",
                        new { existingVote.VoteID }, transaction
                    );
                }

                // Add new vote
                if (isUpvote)
                {
                    connection.Execute(
                        "UPDATE ScamReports SET Upvotes = Upvotes + 1 WHERE ReportID = @ReportID",
                        new { ReportID = reportId }, transaction
                    );
                }
                else
                {
                    connection.Execute(
                        "UPDATE ScamReports SET Downvotes = Downvotes + 1 WHERE ReportID = @ReportID",
                        new { ReportID = reportId }, transaction
                    );
                }

                // Insert new vote record
                connection.Execute(
                    "INSERT INTO Votes (UserID, ReportID, IsUpvote) VALUES (@UserID, @ReportID, @IsUpvote)",
                    new { UserID = user.UserID, ReportID = reportId, IsUpvote = isUpvote }, transaction
                );

                transaction.Commit();

                // Get updated vote counts
                var report = connection.QueryFirstOrDefault(
                    "SELECT Upvotes, Downvotes FROM ScamReports WHERE ReportID = @ReportID",
                    new { ReportID = reportId }
                );

                return Json(new
                {
                    success = true,
                    upvotes = report.upvotes,
                    downvotes = report.downvotes,
                    userVote = isUpvote ? 1 : -1
                });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error in Vote: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            return Json(new { success = false, message = "Error processing vote" });
        }
    }
}