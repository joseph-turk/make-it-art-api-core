using System;
using System.Collections.Generic;
using MakeItArtApi.Models;

namespace MakeItArtApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(User user, string password);
        void Update(User user, string currentPassword, string newPassword = null);
        void Delete(int id);
    }
}