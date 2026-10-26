using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetJoy.Data;
using PetJoy.Filters;
using PetJoy.Models;

namespace PetJoy.Controllers
{
    public class AdoptPetController : Controller
    {
        private readonly PetJoyContext _context;

        public AdoptPetController(PetJoyContext context)
        {
            _context = context;
        }

        //  LIST OF PETS (WITH ADOPTION STATUS)
        public async Task<IActionResult> Adopt()
        {
            var pets = await _context.AdoptPet
            .Include(p => p.AdoptionForms)
            //.OrderByDescending(p => p.AdoptionId) // NEWEST FIRST
            .ToListAsync();

            return View(pets);
        }

        public IActionResult Success()
        {
            return View();
        }

        // ADOPTION FORM PAGE
        public async Task<IActionResult> AdopterInfo(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Adopt));
            }

            var pet = await _context.AdoptPet
                .Include(p => p.AdoptionForms)
                .FirstOrDefaultAsync(x => x.AdoptionId == id);

            if (pet == null)
            {
                return NotFound();
            }

            // BLOCK IF PET IS ALREADY ADOPTED
            bool isAdopted = pet.AdoptionForms.Any(a => a.Status == "ACCEPT");
            if (isAdopted)
            {
                TempData["Error"] = "This pet has already been adopted.";
                return RedirectToAction(nameof(Adopt));
            }

            var vm = new AdoptionForm_AdoptPet
            {
                Pet = pet,
                Form = new AdoptionForm
                {
                    AdoptionId = pet.AdoptionId
                }
            };

            return View(vm);
        }

        // SUBMIT ADOPTION REQUEST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitApplication(AdoptionForm_AdoptPet model)
        {
            var pet = await _context.AdoptPet
                .Include(p => p.AdoptionForms)
                .FirstOrDefaultAsync(x => x.AdoptionId == model.Form.AdoptionId);

            if (pet == null)
            {
                return RedirectToAction(nameof(Adopt));
            }

            //  DOUBLE CHECK (VERY IMPORTANT)
            bool isAdopted = pet.AdoptionForms.Any(a => a.Status == "ACCEPT");
            if (isAdopted)
            {
                TempData["Error"] = "Sorry, this pet has already been adopted.";
                return RedirectToAction(nameof(Adopt));
            }

            try
            {
                model.Form.Status = "Pending"; // default
                _context.AdoptionForm.Add(model.Form);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Success));
            }
            catch
            {
                model.Pet = pet;
                return View("AdopterInfo", model);
            }
        }
    }
}