using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class UpdateDriverModel
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required.")]
        public string firstName { get; set; }

        [Display(Name = "Eligibility To Work")]
        [Required(ErrorMessage = "Eligibility To Work is required.")]
        public bool isDisabled { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required.")]
        public string phoneNumber { get; set; }

        [Display(Name = "Company TIN")]
        [Required(ErrorMessage = "Company TIN is required.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Company TIN must contain only numbers.")]
        public string companyTin { get; set; }
    }
}
