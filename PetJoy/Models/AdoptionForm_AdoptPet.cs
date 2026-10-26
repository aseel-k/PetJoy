using PetJoy.Models;

namespace PetJoy.Models
{
    public class AdoptionForm_AdoptPet
    {
        public AdoptionForm Form { get; set; }
        public AdoptPet Pet { get; set; }

        public AdoptionForm_AdoptPet()
        {
            Form = new AdoptionForm();
        }
    }
}