namespace PetJoy.Models
{
    public class FilterSide
    {
        public IEnumerable<ShopProduct> Products { get; set; } = new List<ShopProduct>();
        public IEnumerable<PetType> PetTypes { get; set; } = new List<PetType>();
        public IEnumerable<Brands> Brands { get; set; } = new List<Brands>();
        public IEnumerable<ShopType> ShopTypes { get; set; } = new List<ShopType>(); 

        // Selected filters
        public int? SelectedPetTypeId { get; set; }
        public int? SelectedBrandId { get; set; }
        public int? SelectedTypeId { get; set; } 
        public string? SortOrder { get; set; }



        //Pets
        public IEnumerable<AdoptPet> Pets { get; set; } = new List<AdoptPet>();
        public int TotalCount => Pets.Count();




        // Pagination properties
        public int CurrentPage { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}