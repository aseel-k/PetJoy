using PetJoy.Models;

namespace PetJoy.ViewModels
{
    public class DashboardViewModel
    {
        // Adoption Cards
        public int TotalPets { get; set; }
        public int AdoptedPets { get; set; }
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int AcceptedRequests { get; set; }
        public int RejectedRequests { get; set; }

        // Orders Cards
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }

        // Tables
        public List<AdoptionForm> LatestRequests { get; set; } = new();
        public List<ShopOrderMaster> LatestOrders { get; set; } = new();

        // Charts
        public List<string> CityLabels { get; set; } = new();
        public List<int> CityCounts { get; set; } = new();
        public List<string> MonthLabels { get; set; } = new();
        public List<int> MonthCounts { get; set; } = new();

        // ShopType Chart
        public List<string> ShopTypeLabels { get; set; } = new();
        public List<int> ShopTypeCounts { get; set; } = new();



        // Revenue by Month Chart
        public List<decimal> MonthRevenue { get; set; } = new();

        // Customer Count
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }

        //map
        public List<string> GovernorateLabels { get; set; } = new();
        public List<int> GovernorateCounts { get; set; } = new();


        public int OrdersToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int PetsAddedToday { get; set; }


        // Pet type chart
        public List<string> PetTypeLabels { get; set; } = new();
        public List<int> PetTypeCounts { get; set; } = new();

    }
}