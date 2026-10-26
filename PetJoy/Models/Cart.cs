using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetJoy.Models
{
    public class ProductModel
    {
        [Key]
        public int ShopProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; }


    }

    public class CartItem
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; }
    }
}