using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetJoy.Data;
using PetJoy.Filters;
using PetJoy.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PetJoy.Controllers
{
    [AdminAuthorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminProductsController : Controller
    {
        private readonly PetJoyContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminProductsController(PetJoyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<IActionResult> Products(
            int? petTypeId,
            int? brandId,
            int? typeId,

            string search,
            int page = 1)
        {
            int pageSize = 68;

            var productsQuery = _context.ShopProduct
           .Where(p => !p.IsDeleted) //ADDD

                .Include(p => p.ShopType)
                .Include(p => p.Brands)
                .Include(p => p.PetType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var terms = search.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var shopTypeMatch = await _context.ShopType
                    .FirstOrDefaultAsync(st => st.Name.ToLower().Contains(search.ToLower()));

                if (shopTypeMatch != null)
                {
                    productsQuery = productsQuery.Where(p => p.ShopTypeId == shopTypeMatch.ShopTypeId);
                }
                else
                {
                    var petType = await _context.PetType
                        .FirstOrDefaultAsync(pt => terms.Contains(pt.Name.ToLower()));

                    if (petType != null)
                    {
                        productsQuery = productsQuery.Where(p => p.PetTypeId == petType.PetTypeId);
                        terms = terms.Where(t => t != petType.Name.ToLower()).ToArray();
                    }

                    foreach (var term in terms)
                    {
                        productsQuery = productsQuery.Where(p =>
                            (p.Name != null && EF.Functions.Like(p.Name.ToLower(), $"%{term}%")) ||
                            (p.Note != null && EF.Functions.Like(p.Note.ToLower(), $"%{term}%")) ||
                            (p.ShopType != null && EF.Functions.Like(p.ShopType.Name.ToLower(), $"%{term}%"))
                        );
                    }
                }
            }


            if (petTypeId.HasValue && petTypeId.Value != 0)
                productsQuery = productsQuery.Where(p => p.PetTypeId == petTypeId.Value);

            if (brandId.HasValue)
                productsQuery = productsQuery.Where(p => p.BrandsId == brandId.Value);

            if (typeId.HasValue)
                productsQuery = productsQuery.Where(p => p.ShopTypeId == typeId.Value);


            productsQuery = productsQuery.OrderByDescending(p => p.ShopProductId);



            int totalProductsCount = await productsQuery.CountAsync();

            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var viewModel = new FilterSide
            {
                Products = products,
                PetTypes = await _context.PetType.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                ShopTypes = await _context.ShopType.ToListAsync(),
                SelectedPetTypeId = petTypeId,
                SelectedBrandId = brandId,
                SelectedTypeId = typeId,

                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalProductsCount / (double)pageSize)
            };

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.ShopProduct
                   .FirstOrDefaultAsync(p => p.ShopProductId == id);

            if (product == null)
                return Json(new { success = false, message = "Product not found." });

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product deleted successfully." });
        }


        [HttpPost]
        public async Task<IActionResult> EditProduct(ShopProductViewModel model)
        {
           
            var product = await _context.ShopProduct
                .FirstOrDefaultAsync(p => p.ShopProductId == model.ShopProductId && !p.IsDeleted);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            // Update fields
            product.Name = model.Name?.Trim();
            product.Price = model.Price;
            product.PetTypeId = model.PetTypeId;
            product.ShopTypeId = model.ShopTypeId;

            // Brand is OPTIONAL
            product.BrandsId = model.BrandsId == 0 ? null : model.BrandsId;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Product updated successfully.",
                newImagePath = product.Note 
            });
        }


    }
}