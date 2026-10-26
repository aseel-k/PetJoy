using Microsoft.AspNetCore.Mvc;
using PetJoy.Models;
using System.Collections.Generic;
using System.Linq;
using PetJoy.Extensions;
using PetJoy.Data;
using Microsoft.EntityFrameworkCore;

namespace PetJoy.Controllers
{
    using System;

    public class CartController : Controller
    {
        private readonly PetJoyContext _context;

        public CartController(PetJoyContext context)
        {
            _context = context;
        }

        private const string CartSessionKey = "CartItemsPetJoy";

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }

        public IActionResult Cart()
        {
            var cartItems = GetCart();

            if (cartItems.Count == 0)
            {
                return RedirectToAction("EmptyCart");
            }

            return View(cartItems);
        }

        public IActionResult EmptyCart()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            try
            {
                var product = _context.ShopProduct.FirstOrDefault(p => p.ShopProductId == id);

                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }

                var cart = GetCart();
                var existingItem = cart.FirstOrDefault(i => i.ProductId == id);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ShopProductId,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = (decimal)product.Price,
                        Note = product.Note
                    });
                }

                SaveCart(cart);

                return Json(new { success = true, message = "Product added to your cart" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult CartSidebar()
        {
            var cartItems = GetCart();

            var total = cartItems.Sum(item => item.Price * item.Quantity);
            ViewData["Total"] = total;

            return PartialView("SideCart", cartItems);
        }


        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var itemToRemove = cart.FirstOrDefault(i => i.ProductId == id);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);

                if (cart.Count == 0)
                {
                    HttpContext.Session.Remove(CartSessionKey);

                    return RedirectToAction("EmptyCart");
                }
                else
                {
                    SaveCart(cart);
                }
            }

            return RedirectToAction("Cart");
        }


        public IActionResult IncreaseQuantity(int id)
        {
            var cart = GetCart();
            var itemToUpdate = cart.FirstOrDefault(i => i.ProductId == id);

            if (itemToUpdate != null)
            {
                itemToUpdate.Quantity++;
                SaveCart(cart);
            }

            return RedirectToAction("Cart");
        }

        public IActionResult DecreaseQuantity(int id)
        {
            var cart = GetCart();
            var itemToUpdate = cart.FirstOrDefault(i => i.ProductId == id);

            if (itemToUpdate != null)
            {
                if (itemToUpdate.Quantity > 1)
                {
                    itemToUpdate.Quantity--;
                    SaveCart(cart);
                }
                else
                {
                    return RedirectToAction("Remove", new { id = id });
                }
            }

            return RedirectToAction("Cart");
        }


        [HttpPost]
        public IActionResult AddToCartAjax([FromBody] AddToCartRequest model)
        {
            var product = _context.ShopProduct
                .FirstOrDefault(p => p.ShopProductId == model.ProductId);

            if (product == null)
            {
                return Json(new { count = 0 });
            }

            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(i => i.ProductId == model.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ShopProductId,
                    ProductName = product.Name,
                    Quantity = 1,
                    Price = (decimal)product.Price,
                    Note = product.Note
                });
            }

            SaveCart(cart);

            int totalCount = cart.Sum(i => i.Quantity);

            return Json(new { count = totalCount });
        }

        public class AddToCartRequest
        {
            public int ProductId { get; set; }
        }


    }
}