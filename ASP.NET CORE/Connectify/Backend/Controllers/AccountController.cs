using Backend.Common;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        // ASP.NET injects UserManager automatically
        public AccountController(UserManager<AppUser> userManager)
        {
            //you store it in _userManager to use later
            _userManager = userManager;
        }//This is constructor dependency injection.

        [HttpPost]
        //Parameters are received from a FORM-DATA POST request (like Angular form submission)
        public async Task<IActionResult> Register([FromForm] Register model)
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
            return Ok(Response<string>.Success("", "User registered successfully."));
        }
    }
}
