using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetJoy.Data;
using PetJoy.Filters;
using PetJoy.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PetJoy.Controllers
{
    [AdminAuthorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]

    public class DashboardController : Controller
    {
        private readonly PetJoyContext _context;

        public DashboardController(PetJoyContext context)
        {
            _context = context;
        }

        public IActionResult AdminDashboard()
        {
            // ================= DATE =================
            var today = DateTime.Today;

            // ================= TODAY STATS =================
            var ordersToday = _context.ShopOrderMaster
                .Count(o => o.OrderDate.Date == today);

            var revenueToday = _context.ShopOrderMaster
                .Where(o => o.Status == "Received" && o.OrderDate.Date == today)
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;

            var petsAddedToday = _context.AdoptPet
                .Count(p => p.CreationDate.Date == today);

            // ================= ADOPTION DEBUG =================
            var efCount = _context.AdoptPet.Count();
            var sqlCount = _context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) FROM Adoption")
                .AsEnumerable()
                .FirstOrDefault();

            ViewBag.EFCount = efCount;
            ViewBag.SQLCount = sqlCount;

            // ================= ADOPTION CITY STATS =================
            var cityStats = _context.AdoptionForm
                .Where(a => !string.IsNullOrEmpty(a.City))
                .GroupBy(a => a.City)
                .Select(g => new { City = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(6)
                .ToList();

            // ================= SHOP ORDER STATS =================
            var totalOrders = _context.ShopOrderMaster.Count();
            var pendingOrders = _context.ShopOrderMaster.Count(o => o.Status == "Pending");
            var completedOrders = _context.ShopOrderMaster.Count(o => o.Status == "Received");

            var totalRevenue = _context.ShopOrderMaster
                .Where(o => o.Status == "Received")
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;

            // ================= TOTAL CUSTOMERS =================
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var totalCustomers = _context.CheckOut
                .Select(o => o.Email)
                .Distinct()
                .Count();

            var newCustomersThisMonth = _context.CheckOut
                .Where(o => o.ShopOrderMaster != null &&
                            o.ShopOrderMaster.OrderDate >= firstDayOfMonth &&
                            o.ShopOrderMaster.OrderDate < firstDayOfNextMonth)
                .Select(o => o.Email)
                .Distinct()
                .Count();

            // ================= SHOP PRODUCTS BY TYPE =================
            var shopTypeStats = _context.ShopProduct
                .Where(p => p.ShopTypeId != null)
                .Include(p => p.ShopType)
                .GroupBy(p => p.ShopType!.Name)
                .Select(g => new { ShopTypeName = g.Key, Count = g.Count() })
                .ToList();

            // ================= REVENUE BY MONTH =================
            var twelveMonthsAgo = today.AddMonths(-11);
            var monthlyRevenue = _context.ShopOrderMaster
                .Where(o => o.Status == "Received" &&
                            o.OrderDate >= new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1))
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var monthLabels = new List<string>();
            var monthRevenueData = new List<decimal>();

            for (int i = 0; i < 12; i++)
            {
                var monthDate = twelveMonthsAgo.AddMonths(i);
                monthLabels.Add(monthDate.ToString("MMM yyyy"));

                var revenueForMonth = monthlyRevenue
                    .FirstOrDefault(r => r.Year == monthDate.Year &&
                                         r.Month == monthDate.Month)?.Revenue ?? 0;

                monthRevenueData.Add(revenueForMonth);
            }

            // ================= PRODUCTS DEMAND BY PET TYPE =================
            var petTypeStats = _context.ShopProduct
                .Include(p => p.PetType) // include navigation property
                .Where(p => p.PetType != null)
                .GroupBy(p => p.PetType!.Name) // group by PetType.Name
                .Select(g => new { PetTypeName = g.Key, Count = g.Count() })
                .ToList();

            // Use the database result directly instead of hardcoding
            var petTypeLabels = petTypeStats.Select(p => p.PetTypeName).ToList();
            var petTypeCounts = petTypeStats.Select(p => p.Count).ToList();


            // ================= DASHBOARD VIEW MODEL =================
            var model = new DashboardViewModel
            {
                // Adoption
                TotalPets = efCount,
                AdoptedPets = _context.AdoptionForm.Count(a => a.Status == "Accepted"),
                TotalRequests = _context.AdoptionForm.Count(),
                PendingRequests = _context.AdoptionForm.Count(a => a.Status == "Pending"),
                AcceptedRequests = _context.AdoptionForm.Count(a => a.Status == "Accepted"),
                RejectedRequests = _context.AdoptionForm.Count(a => a.Status == "Rejected"),

                LatestRequests = _context.AdoptionForm
                    .Include(a => a.AdoptPet)
                    .OrderByDescending(a => a.AdoptionFormId)
                    .Take(6)
                    .ToList(),

                CityLabels = cityStats.Select(c => c.City).ToList(),
                CityCounts = cityStats.Select(c => c.Count).ToList(),

                // Orders
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                TotalRevenue = totalRevenue,

                OrdersToday = ordersToday,
                RevenueToday = revenueToday,
                PetsAddedToday = petsAddedToday,

                LatestOrders = _context.ShopOrderMaster
                    .Include(o => o.PaymentMethod)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(6)
                    .ToList(),

                ShopTypeLabels = shopTypeStats.Select(s => s.ShopTypeName).ToList(),
                ShopTypeCounts = shopTypeStats.Select(s => s.Count).ToList(),

                // Customers
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newCustomersThisMonth,

                // Revenue chart
                MonthLabels = monthLabels,
                MonthRevenue = monthRevenueData,

                // Pet type chart
                PetTypeLabels = petTypeLabels,
                PetTypeCounts = petTypeCounts

            };

            return View(model);
        }
    }
}