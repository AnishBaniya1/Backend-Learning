using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revision.DTOs;

namespace Revision.IService
{
    public interface IAuthService
    {
        Task<Tuple<int, string>>LoginUser(UserDto dto);
    }
}