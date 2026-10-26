using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PetJoy.Filters;
using PetJoy.Models;

namespace PetJoy.Controllers
{
    [AdminAuthorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UploadProductController : Controller
    {
        private readonly string _connectionString;

        public UploadProductController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(ShopProductViewModel model)
        {

            if (!ModelState.IsValid)
                return View(model);


            // SAVE IMAGE
            string imagePath = null;

            if (model.ProductImage != null && model.ProductImage.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ProductImage.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProductImage.CopyTo(stream);
                }

                imagePath = "/images/products/" + fileName;
            }

            //  INSERT INTO DATABASE
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO ShopProduct 
                         (Name, ShopTypeId , PetTypeId , Price, Note, BrandsId)
                         VALUES (@Name, @ShopTypeId, @PetTypeId, @Price , @Note ,@BrandsId)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@ShopTypeId", model.ShopTypeId);
                cmd.Parameters.AddWithValue("@PetTypeId", model.PetTypeId);
                cmd.Parameters.AddWithValue("@Price", model.Price);
                cmd.Parameters.AddWithValue("@Note", (object?)imagePath ?? DBNull.Value);
                

                if (model.BrandsId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@BrandsId", model.BrandsId.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@BrandsId", DBNull.Value);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Upload");
        }
    }
}
