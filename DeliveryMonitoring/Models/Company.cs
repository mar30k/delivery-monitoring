using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Company
    {
        public int Code { get; set; }
        public string? TradeName { get; set; }
        [Display(Name = "Brand Name")]
        public string? BrandName { get; set; }
        public string? Description { get; set; }
        public DateTime RegisterDate { get; set; }
        [Display(Name = "Rating")]
        public decimal Rating { get; set; }
        [Display(Name = "Rating")]
        public int RatingCount { get; set; }
        public bool IsSponsored { get; set; }
        [Display(Name = "TIN")]
        public string Tin { get; set; }
        public string? Logo { get; set; }
        public List<Branch>? Branches { get; set; }
    }
    public class Branch
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public int? CityId { get; set; }
        public string?   City { get; set; }
        public List<string>? Cuisine { get; set; }
        public string? SubCity { get; set; }
        public string? Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Rating { get; set; }
        public int RatingCount { get; set; }
        public int StarRating { get; set; }
        public string? Logo { get; set; }
        public string? Contact { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? SpecificAddress { get; set; }
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }
        public string? PoBox { get; set; }
        public string? Kebele { get; set; }
        public string? Wereda { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public DateTime RegisterDate { get; set; }
        public string? TermsAndConditionsUrl { get; set; }
    }

}
