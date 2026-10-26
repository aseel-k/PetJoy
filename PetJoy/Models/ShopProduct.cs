namespace PetJoy.Models
{
    public class ShopProduct
    {
        public int ShopProductId { get; set; }

        public string? Name { get; set; }
        public int? ShopTypeId { get; set; }
        public int? PetTypeId { get; set; }
        public decimal? Price { get; set; }
        public string? Note { get; set; }
        public int? BrandsId { get; set; }

        public virtual ShopType? ShopType { get; set; }
        public virtual Brands? Brands { get; set; }
        public virtual PetType? PetType { get; set; }

        public bool IsDeleted { get; set; } // soft delete

    }

    public class PetType
    {
        public int PetTypeId { get; set; }
        public string Name { get; set; }
    }

    public class Brands
    {
        public int BrandsId { get; set; }
        public string Name { get; set; }
    }

    public class ShopType
    {
        public int ShopTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
    }
}