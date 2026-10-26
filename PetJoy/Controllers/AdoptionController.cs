using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PetJoy.Filters;
using PetJoy.Models;

namespace PetJoy.Controllers
{
    [AdminAuthorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdoptionController : Controller
    {
        private readonly string _connectionString;

        public AdoptionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult PetAdoption()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PetAdoption(AdoptionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string imagePath = null;

            if (model.PetImage != null && model.PetImage.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/adoption");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.PetImage.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.PetImage.CopyTo(stream);
                }

                imagePath = "/images/adoption/" + fileName;
            }


            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO adoption 
                    (PetName, animal, age, gender, Breed, MedicalHistory, 
                     description, creationdate, note)
                    VALUES
                    (@PetName, @Animal, @Age, @Gender, @Breed, @MedicalHistory,
                     @Description, @CreationDate, @Note)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@PetName", model.PetName);
                cmd.Parameters.AddWithValue("@Animal", model.Animal);
                cmd.Parameters.AddWithValue("@Age", model.Age);
                cmd.Parameters.AddWithValue("@Gender", model.Gender);
                cmd.Parameters.AddWithValue("@Breed", model.Breed);

                cmd.Parameters.AddWithValue("@MedicalHistory", (object?)model.MedicalHistory ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", model.Description);

                cmd.Parameters.AddWithValue("@CreationDate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Note", (object?)imagePath ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = $"'{model.PetName}' added for adoption successfully!";
            return RedirectToAction(nameof(PetAdoption));
        }
    }
}