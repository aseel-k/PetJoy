using Microsoft.AspNetCore.Mvc;

namespace PetJoy.Controllers
{
    public class CareController : Controller
    {
        public IActionResult PetCare()
        {
            return View();
        }

      
    }
}
