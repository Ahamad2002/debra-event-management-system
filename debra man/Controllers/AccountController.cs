using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;

namespace debra_man.Controllers
{
    public class AccountController : Controller
    {
        private readonly string connectionString = "server=localhost;database=abc;user=root;password=;";

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("index", "Account");
        }

        [HttpPost]
        public IActionResult LoginPost(string username, string password)
        {
           

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT UserID, UserType FROM User1 WHERE Username = @Username AND Password = @Password";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var userId = reader.GetInt32("UserID");
                        var userType = reader.GetString("UserType");

                        HttpContext.Session.SetInt32("UserId", userId);

                        if (userType == "User")
                        {
                            return RedirectToAction("UserHome", "Pages");
                        }
                        else if (userType == "Partner")
                        {
                            return RedirectToAction("PartnerHome", "Pages");
                        }
                        else if (userType == "Admin") 
                        {
                            return RedirectToAction("AdminHome", "Pages");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt.");
                    }
                }
            }

            return View("index");
        }



        [HttpPost]
        public IActionResult RegisterPost(User user)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO User1 (Username, Password, Email, MobileNumber, UserType) VALUES (@Username, @Password, @Email, @MobileNumber, @UserType)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@MobileNumber", user.MobileNumber ?? "");
                    cmd.Parameters.AddWithValue("@UserType", user.UserType);
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("index", "Account");
            }

            return View("Register", user);
        }
    }
}
