using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetJoy.Data;
using PetJoy.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PetJoy.Controllers
{
    public class ShopProductController : Controller
    {
        private readonly PetJoyContext _context;

        public ShopProductController(PetJoyContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ShopProduct(
            int? petTypeId,
            int? brandId,
            int? typeId,
            string sortOrder,
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


            //  SMART MULTI-WORD SEARCH 
            if (!string.IsNullOrWhiteSpace(search))
            {
                var terms = search.Trim().ToLower()
                                  .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Check if the full search matches a ShopType
                var shopTypeMatch = await _context.ShopType
                    .FirstOrDefaultAsync(st => st.Name.ToLower().Contains(search.ToLower()));

                if (shopTypeMatch != null)
                {
                    // Only products in this shop type
                    productsQuery = productsQuery.Where(p => p.ShopTypeId == shopTypeMatch.ShopTypeId);
                }
                else
                {
                    // Check if any term matches a PetType
                    var petType = await _context.PetType
                        .FirstOrDefaultAsync(pt => terms.Contains(pt.Name.ToLower()));

                    if (petType != null)
                    {
                        productsQuery = productsQuery.Where(p => p.PetTypeId == petType.PetTypeId);
                        terms = terms.Where(t => t != petType.Name.ToLower()).ToArray();
                    }

                    // Apply remaining terms to Name, Note, and ShopType.Name (AND search)
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


            // Filters
            if (petTypeId.HasValue)
                productsQuery = productsQuery.Where(p => p.PetTypeId == petTypeId.Value);

            if (brandId.HasValue)
                productsQuery = productsQuery.Where(p => p.BrandsId == brandId.Value);

            if (typeId.HasValue)
                productsQuery = productsQuery.Where(p => p.ShopTypeId == typeId.Value);

            // Sorting
            switch (sortOrder)
            {
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                default:
                    productsQuery = productsQuery.OrderBy(p => p.ShopProductId);
                    break;
            }

            // Count
            int totalProducts = await productsQuery.CountAsync();

            // Pagination
            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewModel
            var viewModel = new FilterSide
            {
                Products = products,
                PetTypes = await _context.PetType.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                ShopTypes = await _context.ShopType.ToListAsync(),
                SelectedPetTypeId = petTypeId,
                SelectedBrandId = brandId,
                SelectedTypeId = typeId,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize)
            };

            return View(viewModel);
        }
    }
}
