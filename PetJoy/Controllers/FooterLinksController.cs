using Microsoft.AspNetCore.Mvc;
using PetJoy.Models;
using System.Diagnostics;

namespace PetJoy.Controllers
{
    public class FooterLinksController : Controller
    {
        private readonly ILogger<PaymentController> _logger;

        public IActionResult FAQs()
        {
            return View();
        }

        public IActionResult Guidelines()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult ReturnsandRefunds()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Payment()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }


        public class PaymentController : Controller
        {
            private readonly ILogger<PaymentController> _logger;

            public PaymentController(ILogger<PaymentController> logger)
            {
                _logger = logger;
            }

            public IActionResult Payment()
            {
                _logger.LogInformation("Payment page requested.");
                return View();
            }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }

}
