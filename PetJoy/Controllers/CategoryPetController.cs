using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetJoy.Data;
using PetJoy.Models; 

namespace PetJoy.Controllers
{
    public class CategoryPetController : Controller
    {
        private readonly PetJoyContext _context;

        public CategoryPetController(PetJoyContext context)
        {
            _context = context;
        }

        // generalized the action to handle all pet types
        public async Task<IActionResult> PetTypeCategories(string petName)
        {
            if (string.IsNullOrEmpty(petName))
            {
                return NotFound("Pet type not specified.");
            }

            // 1. Find the PetType by its Name (e.g., "Dog", "Cat")
            var pet = await _context.PetType
                .FirstOrDefaultAsync(p => p.Name == petName);

            if (pet == null)
            {
                return NotFound($"Pet type '{petName}' not found.");
            }

            // 2. Filter ShopTypes based on the Description column for the current pet
            // The Description format is assumed to be like "All", "Cats & Dogs", "Fish", etc.
            var shopTypes = await _context.ShopType
                .Where(s => s.Description == "All" || s.Description.Contains(petName))
                .OrderBy(s => s.Name) // Order them alphabetically
                .ToListAsync();

            // Store PetId and PetName in ViewBag for use in the generic view
            ViewBag.PetId = pet.PetTypeId;
            ViewBag.PetName = pet.Name;

            // Pass the filtered shop types to the view
            return View(shopTypes);
        }
    }
}