using System;
namespace Ifound.Services
{
    public interface IUserService
    {
        Ifound.Models.User GetUserByUserNo(string userno);
    }
}
