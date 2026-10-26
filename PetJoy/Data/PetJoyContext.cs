using Microsoft.EntityFrameworkCore;
using PetJoy.Models;
using System.Diagnostics.Eventing.Reader;


namespace PetJoy.Data
{
    public class PetJoyContext : DbContext
    {
        public PetJoyContext(DbContextOptions<PetJoyContext> options)
            : base(options)
        {
        }

        public DbSet<ShopProduct> ShopProduct { get; set; }
        public DbSet<PetType> PetType { get; set; }
        public DbSet<Brands> Brands { get; set; }
        public DbSet<ShopType> ShopType { get; set; }
        public DbSet<AdoptPet> AdoptPet { get; set; }
        public DbSet<AdoptionForm> AdoptionForm { get; set; }
        public DbSet<CheckOut> CheckOut { get; set; } //ShopOrderDetial 
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<ShopOrderMaster> ShopOrderMaster { get; set; }
        public DbSet<ShopOrderItem> ShopOrderItem { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdoptPet>().ToTable("Adoption");

            modelBuilder.Entity<ShopProduct>().ToTable("ShopProduct");

            modelBuilder.Entity<CheckOut>().ToTable("ShopOrderDetail");

            // One payment method can have many order masters.
            modelBuilder.Entity<ShopOrderMaster>()
            .HasOne(o => o.PaymentMethod)
            .WithMany()
            .HasForeignKey(o => o.PaymentMethodId)
            .IsRequired();

            //One order master can have many order items.
            modelBuilder.Entity<ShopOrderItem>()
            .HasOne(i => i.ShopOrderMaster)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(i => i.ShopOrderMasterId)
            .OnDelete(DeleteBehavior.Cascade);

        }
    }
}