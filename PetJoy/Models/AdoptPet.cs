using PetJoy.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace PetJoy.Models
{
    public class AdoptPet
    {
        [Key]
        public int AdoptionId { get; set; }

        [Required]
        [StringLength(100)]
        public string PetName { get; set; } = string.Empty;

        [StringLength(50)]
        public string Animal { get; set; } = string.Empty;

        public int Age { get; set; }

        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [StringLength(100)]
        public string Breed { get; set; } = string.Empty;

        public string MedicalHistory { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public string Note { get; set; } = string.Empty;


        // Navigation property for related forms
        public ICollection<AdoptionForm>? AdoptionForms { get; set; }

        [NotMapped]
        public bool IsAdopted
        {
            get
            {
                return AdoptionForms != null &&
                       AdoptionForms.Any(a =>
                           !string.IsNullOrWhiteSpace(a.Status) &&
                           a.Status.Trim().Equals("Accepted", StringComparison.OrdinalIgnoreCase)
                       );
            }
        }

    }
}