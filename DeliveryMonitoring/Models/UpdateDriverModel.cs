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
        [Display(Name = "Is Freelance")]
        [Required(ErrorMessage = "Is Freelance is required.")]
        public bool isFreelance { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required.")]
        public string phoneNumber { get; set; }

        [Display(Name = "Company TIN")]
        [Required(ErrorMessage = "Company TIN is required.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Company TIN must be exactly 10 numbers and It must not contain letters.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Company TIN must be exactly 10 numbers and It must not contain letters.")]
        public string companyTin { get; set; }
    }
}
