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
    public class OrdersController : Controller
    {
        private readonly PetJoyContext _context;
        private readonly EmailService _emailService;

        public OrdersController(PetJoyContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ADMIN – VIEW ALL ORDERS
        public async Task<IActionResult> ViewOrders()
        {
            // 1️ Load orders + master + payment
            var orders = await _context.CheckOut
                .Include(o => o.ShopOrderMaster)
                    .ThenInclude(m => m.PaymentMethod)
                .OrderByDescending(o => o.ShopOrderMaster.OrderDate)
                .ToListAsync();

            // 2️ Get order master IDs
            var orderIds = orders
                .Select(o => o.ShopOrderMasterId)
                .ToList();

            // 3️ Load items separately 
            var orderItems = await _context.ShopOrderItem
                .Include(i => i.ShopProduct)
                .Where(i => orderIds.Contains(i.ShopOrderMasterId))
                .ToListAsync();

            // 4️ Attach items to each order
            foreach (var order in orders)
            {
                order.OrderItems = orderItems
                    .Where(i => i.ShopOrderMasterId == order.ShopOrderMasterId)
                    .ToList();
            }

            return View(orders);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderMasterId, string status)
        {
            var order = await _context.CheckOut
        .Include(o => o.ShopOrderMaster)
            .ThenInclude(m => m.PaymentMethod) // Correct
        .FirstOrDefaultAsync(o => o.ShopOrderMasterId == orderMasterId);

            if (order == null)
                return NotFound();

            // Load items separately
            order.OrderItems = await _context.ShopOrderItem
                .Include(i => i.ShopProduct)
                .Where(i => i.ShopOrderMasterId == orderMasterId)
                .ToListAsync();


            if (order == null)
                return NotFound();

            order.ShopOrderMaster.Status = status;
            await _context.SaveChangesAsync();

            // EMAIL LOGIC
            string subject;
            string body;

            if (status == "Received")
            {
                subject = $"PetJoy – Order #{order.ShopOrderMasterId} Received";
                body = _emailService.BuildOrderReceivedEmail(order);
            }
            else if (status == "Cancelled")
            {
                subject = $"PetJoy – Order #{order.ShopOrderMasterId} Cancelled";
                body = _emailService.BuildOrderCancelledEmail(order);
            }
            else
            {
                return RedirectToAction("Orders");
            }

            await _emailService.SendAsync(order.Email, subject, body);

            return RedirectToAction("ViewOrders");
        }


    }
}