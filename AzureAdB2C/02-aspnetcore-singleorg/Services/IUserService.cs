using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountTypesSingleOrg.Models;

namespace AccountTypesSingleOrg.Services
{
    public interface IUserService
    {
        User Create(User user);
        User GetById(string b2cObjectId);
        Task<string> GetB2CTokenAsync();
        User GetUserFromSession();
    }
}
