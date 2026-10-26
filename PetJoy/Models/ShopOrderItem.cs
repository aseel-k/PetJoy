using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetJoy.Models
{
    public class ShopOrderItem
    {
        [Key]
        public int ShopOrderItemId { get; set; }


        [ForeignKey("ShopOrderMaster")]
        public int ShopOrderMasterId { get; set; }
        public ShopOrderMaster? ShopOrderMaster { get; set; }


        [ForeignKey("ShopProduct")]
        public int ShopProductId { get; set; }
        public ShopProduct? ShopProduct { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "int")]
        public decimal Quantity { get; set; }


    }
}