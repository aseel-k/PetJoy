using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetJoy.Models
{
    [Table("AdoptionForm")]
    public class AdoptionForm
    {
        [Key]
        public int AdoptionFormId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        public int? Age { get; set; }

        [StringLength(10)]
        public string? HasCurrentChildren { get; set; }

        [StringLength(10)]
        public string? HasCurrentPets { get; set; }

        public string? CurrentPetDetails { get; set; }

        [StringLength(10)]
        public string? HasPreviousPets { get; set; }

        public string? PreviousPetFate { get; set; }

        public string? PetConfinement { get; set; }

        public bool FinancialAck { get; set; }

        public string? HousingStatus { get; set; }



        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [ForeignKey("AdoptPet")]
        public int AdoptionId { get; set; }
        public AdoptPet? AdoptPet { get; set; }
    }
}