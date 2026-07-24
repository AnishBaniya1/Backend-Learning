using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Revision.Data;
using Revision.DTOs;
using Revision.IService;

namespace Revision.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tuple<int, string>> LoginUser(UserDto dto)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
                if (existingUser == null)
                {
                    return new Tuple<int, string>(0, "This User doesn't exist, Please Login");
                }
                if (existingUser.Password != dto.Password)
                {
                    return new Tuple<int, string>(1, "Password Incorrect");
                }
                return new Tuple<int, string>(2, "Login Succesfull");
            }
            catch (System.Exception)
            {

                return new Tuple<int, string>(3, "Something went Wrong");
            }
        }
    }
}