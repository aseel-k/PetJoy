using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetJoy.Models
{
    [Table("ShopOrderMaster")]

    public class ShopOrderMaster
    {
        [Key]
        public int ShopOrderMasterId { get; set; }

        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(19,2)")]
        public decimal TotalPrice { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }



        [ForeignKey("PaymentMethod")]
        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";


        public ICollection<ShopOrderItem> OrderItems { get; set; } = new List<ShopOrderItem>();


    }
}