using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetJoy.Data;
using PetJoy.Extensions;
using PetJoy.Models;
using PetJoy.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetJoy.Controllers
{
    public class CheckoutController : Controller
    {
        //start for Stripe
        private readonly IConfiguration _config;
        //end for Stripe
        private readonly PetJoyContext _context;
        private const string CartSessionKey = "CartItemsPetJoy";

        private readonly EmailService _emailService;


        public CheckoutController(PetJoyContext context, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;

            //start for Stripe
            _config = config;
            //end for Stripe
        }




        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var model = new CheckOut
            {
                CartItems = cart,
                ShopOrderMaster = new ShopOrderMaster
                {
                    SubTotal = cart.Sum(c => c.Price * c.Quantity),
                    ShippingCost = 2m,
                    TotalPrice = cart.Sum(c => c.Price * c.Quantity) + 2m
                }
            };
            ViewBag.StripePk = _config["Stripe:PublishableKey"];
            return View(model);
        }



        // POST: Place Order

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckOut model)
        {
            // Restore cart
            var cartItems = HttpContext.Session
                .GetObjectFromJson<List<CartItem>>(CartSessionKey)
                ?? new List<CartItem>();

            if (!cartItems.Any())
            {
                return RedirectToAction("Cart", "Shop");
            }

            decimal subtotal = cartItems.Sum(i => i.Price * i.Quantity);
            decimal shipping = 2.00m;
            decimal grandTotal = subtotal + shipping;

            // start for Stripe
            // safety check:
            if (model.SelectedPaymentMethodId == 2 && string.IsNullOrWhiteSpace(model.StripePaymentIntentId))
            {
                ModelState.AddModelError("", "Payment was not completed. Please try again.");
                return View("Checkout", model);
            }
            // end for Stripe

            // Create ShopOrderMaster
            var orderMaster = new ShopOrderMaster
            {
                OrderDate = DateTime.Now,
                TotalPrice = grandTotal,
                SubTotal = subtotal,
                ShippingCost = shipping,
                PaymentMethodId = model.SelectedPaymentMethodId
            };

            _context.ShopOrderMaster.Add(orderMaster);
            await _context.SaveChangesAsync(); // get PK

            // Create ShopOrderItems
            var orderItems = cartItems.Select(item => new ShopOrderItem
            {
                ShopOrderMasterId = orderMaster.ShopOrderMasterId,
                ShopProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            }).ToList();

            _context.ShopOrderItem.AddRange(orderItems);

            // Create ShopOrderDetail 
            var orderDetail = new CheckOut
            {
                ShopOrderMasterId = orderMaster.ShopOrderMasterId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Apartment = model.Apartment,
                Country = model.Country,
                City = model.City,
                PostalCode = model.PostalCode,
                StripePaymentIntentId = model.StripePaymentIntentId
            };

            // force EF to track as INSERT
            _context.Entry(orderDetail).State = EntityState.Added;

            // Save everything
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "An error occurred while saving your order.");
                return View("Checkout", model);
            }

            // Clear cart
            HttpContext.Session.Remove(CartSessionKey);

            TempData["ConfirmedOrderId"] = orderMaster.ShopOrderMasterId;

            return RedirectToAction("OrderConfirmation");
        }




        //HTTP 

        public async Task<IActionResult> OrderConfirmation()
        {
            if (TempData["ConfirmedOrderId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            int orderMasterId = (int)TempData["ConfirmedOrderId"];

            TempData.Keep("ConfirmedOrderId");

            // Load master & payment
            var orderMaster = await _context.ShopOrderMaster
                .Include(o => o.PaymentMethod)
                .FirstOrDefaultAsync(o => o.ShopOrderMasterId == orderMasterId);

            if (orderMaster == null)
            {
                return NotFound();
            }

            // Load order items explicitly 
            var orderItems = await _context.ShopOrderItem
                .Include(i => i.ShopProduct)
                .Where(i => i.ShopOrderMasterId == orderMasterId)
                .ToListAsync();

            // Load order detail
            var orderDetail = await _context.CheckOut
                .FirstOrDefaultAsync(d => d.ShopOrderMasterId == orderMasterId);

            if (orderDetail == null)
                return NotFound();

            // Attach for the view
            orderDetail.OrderItems = orderItems;
            orderDetail.ShopOrderMaster = orderMaster;

            // AUTOMATIC EMAIL 
            try
            {
                var emailBody = _emailService.BuildOrderConfirmationEmail(orderDetail);
                await _emailService.SendAsync(orderDetail.Email,
                    $"Order #{orderMaster.ShopOrderMasterId} Successfully Confirmed",
                    emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending order confirmation email: {ex.Message}");
            }

            return View(orderDetail);
        }

        //start for Stripe
        [HttpPost]
        public async Task<IActionResult> CreateStripeIntent()
        {
            // Recalculate total from session cart
            var cartItems = HttpContext.Session
                .GetObjectFromJson<List<CartItem>>(CartSessionKey)
                ?? new List<CartItem>();

            if (!cartItems.Any())
                return BadRequest("Cart empty");

            decimal subtotal = cartItems.Sum(i => i.Price * i.Quantity);
            decimal shipping = 2.00m;
            decimal totalJod = subtotal + shipping;

            decimal rate = decimal.Parse(_config["Currency:JodToUsd"]);
            decimal totalUsd = totalJod * rate;

            // USD => cents
            long amount = (long)Math.Round(totalUsd * 100m);

            var service = new Stripe.PaymentIntentService();
            var intent = await service.CreateAsync(new Stripe.PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" }
            });

            return Json(new { clientSecret = intent.ClientSecret });
        }
        //end for Stripe
    }
}