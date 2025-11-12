using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCShop.Repository
{
    public class UserRepository
    {
        private readonly PcshopDbContext _context;

        public UserRepository()
        {
            _context = new PcshopDbContext();
        }

        public User? Login(string username, string password)
        {
            return _context.Users
                .FirstOrDefault(u =>
                    u.Username == username
                    && u.Password == password
                    && u.IsActive == true);   
        }


        /// <summary>
        /// Lấy tất cả người dùng
        /// </summary>
        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        /// <summary>
        /// Lấy người dùng bằng ID
        /// </summary>
        public User? GetById(int userId)
        {
            return _context.Users.Find(userId);
        }

        /// <summary>
        /// Thêm người dùng mới (nghiệp vụ: Thêm tài khoản)
        /// </summary>
        public void AddUser(User user)
        {
            // Kiểm tra trùng tên đăng nhập
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                throw new Exception("Tên đăng nhập đã tồn tại.");
            }
            user.IsActive = true;

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        /// <summary>
        /// Cập nhật thông tin người dùng (nghiệp vụ: Cập nhật, Gán quyền, Đặt lại mật khẩu)
        /// </summary>
        public void UpdateUser(User user)
        {
            var existingUser = _context.Users.Find(user.UserId);
            if (existingUser != null)
            {
                // Kiểm tra trùng tên đăng nhập (trừ chính nó)
                if (_context.Users.Any(u => u.Username == user.Username && u.UserId != user.UserId))
                {
                    throw new Exception("Tên đăng nhập đã tồn tại.");
                }

                existingUser.Username = user.Username;
                existingUser.FullName = user.FullName;
                existingUser.Role = user.Role;

                // Nếu mật khẩu được cung cấp (không rỗng) thì mới cập nhật
                // (Nghiệp vụ: Đặt lại mật khẩu)
                if (!string.IsNullOrEmpty(user.Password))
                {
                    existingUser.Password = user.Password;
                }

                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Xóa người dùng (nghiệp vụ: Vô hiệu hóa tài khoản)
        /// </summary>
        public void DeleteUser(int userId)
        {
            // Không cho phép xóa tài khoản admin gốc (ID = 1)
            if (userId == 1)
            {
                throw new Exception("Không thể xóa tài khoản Quản trị viên gốc.");
            }

            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.IsActive = false; // Vô hiệu hóa tài khoản (soft delete)
                _context.Users.Update(user);
                _context.SaveChanges();
            }
        }
    }
}