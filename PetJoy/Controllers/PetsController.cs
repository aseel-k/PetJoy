using Microsoft.AspNetCore.Mvc;
using PetJoy.Data;
using PetJoy.Filters;
using PetJoy.Models;
using System;
using System.Linq;

namespace PetJoy.Controllers
{
    [AdminAuthorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class PetsController : Controller
    {
        private readonly PetJoyContext _context;
        private const int PageSize = 12;

        public PetsController(PetJoyContext context)
        {
            _context = context;
        }

        // READ + FILTER + PAGINATION
        public IActionResult OurPets(int? petTypeId, int page = 1)
        {
            var query = _context.AdoptPet.AsQueryable();

            if (petTypeId.HasValue && petTypeId.Value != 0)
            {
                string selectedAnimal = petTypeId.Value switch
                {
                    1 => "Cat",
                    2 => "Dog",
                    3 => "Fish",
                    4 => "Bird",
                    5 => "Small Pets",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(selectedAnimal))
                    query = query.Where(p => p.Animal == selectedAnimal);
            }

            int totalCount = query.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var pets = query
                .OrderByDescending(p => p.CreationDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var model = new FilterSide
            {
                Pets = pets,
                SelectedPetTypeId = petTypeId,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }

        // EDIT DATA 
        [HttpPost]
        public IActionResult EditPet(AdoptPet model)
        {
            var pet = _context.AdoptPet.Find(model.AdoptionId);
            if (pet == null)
                return Json(new { success = false, message = "Pet not found." });

            // Update text fields only
            pet.PetName = model.PetName;
            pet.Animal = model.Animal;
            pet.Age = model.Age;
            pet.Gender = model.Gender;
            pet.Breed = model.Breed;
            pet.MedicalHistory = model.MedicalHistory;
            pet.Description = model.Description;

            _context.SaveChanges();
            return Json(new { success = true });
        }

        // DELETE ENTIRE ROW
        [HttpPost]
        public IActionResult DeletePet(int id)
        {
            var pet = _context.AdoptPet.Find(id);
            if (pet == null)
                return Json(new { success = false, message = "Pet not found." });

            _context.AdoptPet.Remove(pet);
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}