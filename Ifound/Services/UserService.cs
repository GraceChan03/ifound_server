using Ifound.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ifound.Services
{
    public class UserService : Ifound.Services.IUserService
    {
        private IfoundDbContext db = new IfoundDbContext();
        public User GetUserByUserNo(string userno)
        {
            if (string.IsNullOrWhiteSpace(userno))
                return null;
            IQueryable<User> query = from u in db.Users
                                     where u.UserNo == userno
                                     select u;
            User user = query.FirstOrDefault();
            return user;
        }

    }
}