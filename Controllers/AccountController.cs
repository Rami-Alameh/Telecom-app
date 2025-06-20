using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace internshipPartTwo.Controllers
{
    public class AccountController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //view for cshtml
        public ActionResult Login()
        {
            return View();
        }
        // post function which takes the name and password and uses the fucntion hashpassword 
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            //basic query to check if the hashed password is similar to the hashed password in the db
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                conn.Close();
                //sets session to true and redirects to devices
                if (count > 0)
                {
                    Session["loggedIn"] = true;
                    return RedirectToAction("Index", "Home");
                }
               // error message to be shown in user end
                else
                {
                    ViewBag.Error = "Invalid username or password.";
                    return View();
                }
            }
        }
        //simple logout closes session and redirects to login
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
       
        private static string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
        //----------------------
        //signup functionality
        //------------------------
        // GET: Account/Signup
        public ActionResult Signup()
        {
            return View();
        }

        // POST: Account/Signup
        [HttpPost]
        public ActionResult Signup(string username, string password, string confirmPassword)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            // Check if passwords match
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // Check if username already exists
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Username", username);

                conn.Open();
                int existingCount = (int)checkCmd.ExecuteScalar();

                if (existingCount > 0)
                {
                    ViewBag.Error = "Username already exists. Please choose another one.";
                    return View();
                }

                // Hash the password
                string hashedPassword = HashPassword(password);

                // Insert new user
                string insertQuery = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@Username", username);
                insertCmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                try
                {
                    insertCmd.ExecuteNonQuery();
                    ViewBag.Success = "Account created successfully! You can now login.";
                    return View();
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error creating account: " + ex.Message;
                    return View();
                }
            }
        }
    }
}
