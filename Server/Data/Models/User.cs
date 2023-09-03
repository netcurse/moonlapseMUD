using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Data.Models {
    public class User {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastLoggedIn { get; set; }

        public User() {
            Created = DateTime.Now.ToUniversalTime();
        }

        public bool VerifyPassword(string password) {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public void SetPassword(string password) {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
