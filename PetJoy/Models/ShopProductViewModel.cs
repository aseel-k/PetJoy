namespace PetJoy.Models
{
  public class ShopProductViewModel

    {

            public int ShopProductId { get; set; }

            public int PetTypeId { get; set; }
            public string Name { get; set; }
            public int? BrandsId { get; set; }
            public int ShopTypeId { get; set; }
            public decimal Price { get; set; }

            public IFormFile ProductImage { get; set; }   
  
  }

}
