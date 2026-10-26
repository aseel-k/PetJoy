using System.Data;
using Microsoft.Data.SqlClient;
using PetJoy.Models;
using Microsoft.AspNetCore.Mvc;

namespace PetJoy.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _connectionString;

        public AdminController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        [HttpGet]

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.Admin_Login", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", model.Username);
                    cmd.Parameters.AddWithValue("@Password", model.Password);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        // Login successful
                        reader.Read();
                        string fullName = reader["FullName"].ToString();

                        // Store in session
                        HttpContext.Session.SetString("Username", model.Username);
                        HttpContext.Session.SetString("FullName", fullName);

                        Console.WriteLine("LOGIN SESSION SET");


                        // Redirect to a Dashboard page
                        return RedirectToAction("AdminDashboard", "Dashboard");
                    }
                    else
                    {
                        // Invalid login
                        ViewBag.ErrorMessage = "Invalid username or password.";
                        return View(model);
                    }
                }
            }
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home"); 
        }

    }
}
