using Backend.Common;
using Backend.DTOs;
using Backend.Extensions;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    //This sets the base URL for this controller’s endpoints.
    //api/ is fixed
    //[controller] automatically becomes the controller name without “Controller”
    [Route("api/[controller]")]

    // This enables features specifically for APIs:
    // Automatic model validation
    //  Automatic HTTP 400 (BadRequest) responses
    //  etter binding behavior
    // [FromBody]/[FromForm] rules applied automatically
    // It basically tells ASP.NET that this controller returns JSON, not HTML.
    [ApiController]
    public class AccountController : ControllerBase
    {
        //Stores the UserManager service for use in methods.
        //UserManager<AppUser> handles:
        // creating users
        // hashing passwords
        //validating credentials
        // checking roles
        // updating accounts
        private readonly UserManager<AppUser> _userManager;

        private readonly TokenService _tokenService;

        // ASP.NET injects UserManager automatically
        public AccountController(UserManager<AppUser> userManager, TokenService tokenService)
        {
            //you store it in _userManager to use later
            _userManager = userManager;
            _tokenService = tokenService;

        }//This is constructor dependency injection.

        [HttpPost("Register")]
        //Parameters are received from a FORM-DATA POST request (like Angular form submission)
        public async Task<IActionResult> Register([FromForm] RegisterDto model)
        {
            //Searches database for a user with this email
            //Returns null if not found
            var userFromDb = await _userManager.FindByEmailAsync(model.Email!);
            if (userFromDb is not null)
            {
                //tells the frontend that the email is already registered
                return BadRequest(Response<string>.Failure("User already exists."));
            }

            //Validates that the user uploaded a file, check for null
            if (model.ProfileImage == null)
            {
                return BadRequest(Response<string>.Failure("Profile Image is Required."));
            }

            //Saves the file to wwwroot/uploads and returns the filename.
            var fileName = await FileUpload.Upload(model.ProfileImage);

            //Converts the filename into a publicly accessible URL that the frontend can use.
            var pictureUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/uploads/{fileName}";

            //Create a new user object
            var user = new AppUser //AppUser is your user model that extends IdentityUser.
            {
                Email = model.Email,
                FullName = model.FullName,
                UserName = model.UserName,
                ProfileImage = pictureUrl
            };

            //Create user in Identity system
            //This does:
            //validates password strength
            // generates salt
            // hashes the password
            // stores hash in PasswordHash
            // inserts the user into database
            var result = await _userManager.CreateAsync(user, model.Password!);

            //Check if creation failed
            if (!result.Succeeded)
            {
                return BadRequest(Response<string>.Failure(result.Errors.Select(x => x.Description).FirstOrDefault()!));
            }
            //if creation is successful
            return Ok(Response<string>.Success("", "User registered successfully."));
        }

        [HttpPost("Login")]
        //This defines an API endpoint that receives a LoginDto object from the frontend.
        public async Task<IActionResult> Login(LoginDto dto)
        {
            //Checks if the incoming data (the body of the request) is missing or invalid.
            if (dto is null)
            {
                return BadRequest(Response<string>.Failure("Invalid Login Details"));
            }

            //Uses ASP.NET Identity’s UserManager to search for a user by email in the database.
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                //If no user exists → returns a “User Not Found” error.
                return BadRequest(Response<string>.Failure("User Not Found"));
            }

            //This verifies the password the user entered against the hashed password stored in the database.
            //You don’t manually hash passwords here — CheckPasswordAsync does it internally.
            //ASP.NET Identity handles hashing and comparison automatically
            var result = await _userManager.CheckPasswordAsync(user!, dto.Password);

            if (!result)
            {//If password check fails
                return BadRequest(Response<string>.Failure("Invalid Password"));
            }

            //This calls your custom TokenService (which you wrote earlier).
            //It generates a JWT (JSON Web Token) that includes:
            //user.id
            //user.username
            var token = _tokenService.GenerateToken(user.Id, user.UserName!);

            return Ok(Response<string>.Success(token, "Login Successfully"));
        }

        [HttpGet("GetUser")]
        [Authorize]
        //Fetch the current logged in user
        public async Task<IActionResult> GetUser()
        {
            // 🔹 Get the currently logged-in user's ID from the JWT token (claims)
            var currentLoggedInUserId = HttpContext.User.GetUserId()!;
            // 🔹 Fetch the full user record from the database using the UserManager
            //     - Finds the user whose Id matches the one from the token
            var currentLoggedInUser = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == currentLoggedInUserId.ToString());
            //return success
            return Ok(Response<AppUser>.Success(currentLoggedInUser!, "User Fetched Succesfully"));
        }
    }
}
