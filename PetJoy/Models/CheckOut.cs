using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PetJoy.Models
{
    [Table("ShopOrderDetail")]

    public class CheckOut
    {

        [Key]
        public int ShopOrderDetailId { get; set; }

        [Required] public string? FirstName { get; set; }
        [Required] public string? LastName { get; set; }
        [Required, EmailAddress] public string? Email { get; set; }
        [Required] public string? PhoneNumber { get; set; }

        // Delivery
        [Required] public string? Address { get; set; }
        public string? Apartment { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        [Required] public string? City { get; set; }




        //start for Stripe
        public string? StripePaymentIntentId { get; set; }
        //end for Stripe



        [ForeignKey("ShopOrderMaster")]
        public int ShopOrderMasterId { get; set; }
        public ShopOrderMaster? ShopOrderMaster { get; set; }


        [NotMapped]
        public PaymentMethod? PaymentMethod => ShopOrderMaster?.PaymentMethod;

        [NotMapped]
        public int SelectedPaymentMethodId { get; set; }


        [NotMapped]
        public IEnumerable<ShopOrderItem> OrderItems { get; set; }


        // helpers
        [NotMapped]
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        [NotMapped]
        public decimal ShippingCost { get; set; } = 2.00m;
        [NotMapped]
        public decimal SubTotal => CartItems.Sum(item => item.Price * item.Quantity);
        [NotMapped]
        public decimal GrandTotal => SubTotal + ShippingCost;
        [NotMapped]
        public int TotalItemCount => CartItems.Sum(item => item.Quantity);

    }

}