using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class UpdateDriverModel
    {
        [Display(Name = "First Name")]
        public string firstName { get; set; }
        [Display(Name = "Eligibility To Work")]
        public bool isDisabled { get; set; }
        [Display(Name = "Phone Number")]
        public string phoneNumber { get; set; }
        [Display(Name = "Company TIN")]
        public string companyTin { get; set; }
    }
}
