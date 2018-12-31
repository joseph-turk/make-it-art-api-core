using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MakeItArtApi.Models;

namespace MakeItArtApi.Services
{
    public class UserService : IUserService
    {
        private ModelContext _context;

        public UserService(ModelContext context)
        {
            _context = context;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            User user = _context.Users.SingleOrDefault(x => x.Username.Equals(username));

            if (user == null) return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public User Create(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is required");
            }

            if (_context.Users.Any(x => x.Username.Equals(user.Username, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new ArgumentException("Username is already in use");
            }

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void Update(User userParam, string currentPassword, string newPassword = null)
        {
            User user = _context.Users.Find(userParam.Id);

            if (user == null) throw new Exception("User not found");

            if (!userParam.Username.Equals(user.Username))
            {
                // Check if new username is available
                if (_context.Users.Any(x => x.Username.Equals(userParam.Username)))
                {
                    throw new Exception("Username is already in use");
                }
            }

            // Verify old password is correct
            if (newPassword != null && currentPassword != null)
            {
                if (!VerifyPasswordHash(currentPassword, user.PasswordHash, user.PasswordSalt))
                {
                    throw new Exception("Old password is incorrect");
                }
            }

            user.Username = userParam.Username;
            user.IsAdmin = userParam.IsAdmin;

            // Update password if it was entered
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(newPassword, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            User user = _context.Users.Find(id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        #region Private Methods
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace-only string.", "password");
            }

            using (HMACSHA512 hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace-only string.", "password");
            }

            if (storedHash.Length != 64)
            {
                throw new ArgumentException("Invalid length of password hash (64 bytes expcected).", "storedHash");
            }

            if (storedSalt.Length != 128)
            {
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "storedSalt");
            }

            using (HMACSHA512 hmac = new HMACSHA512(storedSalt))
            {
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
        #endregion
    }
}