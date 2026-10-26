namespace PetJoy.Models
{
    public class AdoptionViewModel
    {

        public string PetName { get; set; }
        public string Animal { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Breed { get; set; }
        public string? MedicalHistory { get; set; }
        public string Description { get; set; }

        public IFormFile? PetImage { get; set; }


    }
}