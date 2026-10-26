using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetJoy.Models
{
    [Table("PaymentMethod")]

    public class PaymentMethod
    {
        [Key]
        public int PaymentMethodId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethodName { get; set; } = string.Empty;
    }
}
