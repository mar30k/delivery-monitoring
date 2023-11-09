using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Company
    {
        [Display(Name = "TIN")]
        public string tin { get; set; }
        [Display(Name = "Rating")]
        public double rating { get; set; }
        [Display(Name = "Rating Count")]
        public int ratingCount { get; set; }
        public List<string> attachments { get; set; }
        [Display(Name = "Brand Name")]
        public string brandName { get; set; }
    }
}
