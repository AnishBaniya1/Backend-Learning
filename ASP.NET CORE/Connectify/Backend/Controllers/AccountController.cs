using Backend.Common;
using Backend.Models;
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
        public async Task<IActionResult> Register([FromForm] string fullName, [FromForm] string email, [FromForm] string password, [FromForm] string userName)
        {
            //Searches database for a user with this email
            //Returns null if not found
            var userFromDb = await _userManager.FindByEmailAsync(email);
            if (userFromDb is not null)
            {
                //tells the frontend that the email is already registered
                return BadRequest(Response<string>.Failure("User already exists."));
            }

            //Create a new user object
            var user = new AppUser //AppUser is your user model that extends IdentityUser.
            {
                Email = email,
                FullName = fullName,
                UserName = userName
            };

            //Create user in Identity system
            var result = await _userManager.CreateAsync(user, password);

            //Check if creation failed
            if (!result.Succeeded)
            {
                return BadRequest(Response<string>.Failure(result.Errors.Select(x => x.Description).FirstOrDefault()!));
            }
            return Ok(Response<string>.Success("", "User registered successfully."));
        }
    }
}
