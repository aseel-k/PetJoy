    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using PetJoy.Data;
    using PetJoy.Filters;
    using PetJoy.Models;
    using PetJoy.Services;

    namespace PetJoy.Controllers
    {
        [AdminAuthorize]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public class RequestAdoptionController : Controller
        {
            private readonly PetJoyContext _context;
            private readonly EmailService _emailService;

            public RequestAdoptionController(PetJoyContext context, EmailService emailService)
            {
                _context = context;
                _emailService = emailService;
            }

            // LIST ALL ADOPTION REQUESTS
            public async Task<IActionResult> RequestAdopt()
        {
            var requests = await _context.AdoptionForm
                .Include(a => a.AdoptPet)
                .OrderByDescending(a => a.AdoptionFormId)
                .ToListAsync();

            return View(requests);
        }

        // ACCEPT REQUEST + SEND EMAIL
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            var request = await _context.AdoptionForm
                .Include(a => a.AdoptPet)
                .FirstOrDefaultAsync(a => a.AdoptionFormId == id);

            if (request == null)
                return NotFound();

            request.Status = "Accepted";

            try
            {
                await _context.SaveChangesAsync();

                var (subject, body) = _emailService.BuildAdoptionEmail(request.FullName, request.AdoptPet.PetName, "Accepted");

                await _emailService.SendAsync(request.Email, subject, body);
            }
            catch (Exception ex)
            {
                
            }

            return RedirectToAction(nameof(RequestAdopt));
        }

        // REJECT REQUEST + SEND EMAIL
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.AdoptionForm
                .Include(a => a.AdoptPet)
                .FirstOrDefaultAsync(a => a.AdoptionFormId == id);

            if (request == null)
                return NotFound();

            request.Status = "Rejected";

            try
            {
                await _context.SaveChangesAsync();

                var (subject, body) = _emailService.BuildAdoptionEmail(request.FullName, request.AdoptPet.PetName, "Rejected");

                await _emailService.SendAsync(request.Email, subject, body);
            }
            catch (Exception ex)
            {
                
            }

            return RedirectToAction(nameof(RequestAdopt));
        }
    }
}