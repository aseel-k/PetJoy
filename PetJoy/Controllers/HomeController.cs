using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetJoy.Models;
using Microsoft.EntityFrameworkCore;
using PetJoy.Data; 

namespace PetJoy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PetJoyContext _context; 

        public HomeController(ILogger<HomeController> logger, PetJoyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Get Best Sellers (BASED ON ACTUAL SALES)
            var bestSellersList = _context.ShopOrderItem
                .GroupBy(oi => oi.ShopProductId)
                .Select(g => new
                {
                    ShopProductId = g.Key,
                    TotalQuantitySold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(80)
                .Join(
                    _context.ShopProduct.Where(p => !p.IsDeleted),
                    s => s.ShopProductId,
                    p => p.ShopProductId,
                    (s, p) => p
                )
                .ToList();


            // 2. Get New Products 
            var newProductsList = _context.ShopProduct
                 .Where(p => !p.IsDeleted) // ADDDD HERE
                .ToList() // Pull into memory to allow the grouping logic
                .GroupBy(p => p.PetTypeId)
                .SelectMany(group => group
                    .OrderByDescending(p => p.ShopProductId)
                    .Take(25))
                .ToList();

            // 3. Use ViewBag for Best Sellers so you don't need a New Model class
            ViewBag.BestSellers = bestSellersList;

            // 4. Return newProductsList as the main Model
            return View(newProductsList);
        }


        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            return Json(new { success = true });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}