using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Driver
    {
        [Display(Name = "Company TIN")]
        public string companyTin { get; set; }
        [Display(Name = "Device ID")]
        public string deviceID { get; set; }
        [Display(Name = "First Name")]
        public string firstName { get; set; }
        [Display(Name = "Eligibility To Work")]
        public bool isDisabled { get; set; }
        [Display(Name = "Last Updated At")]
        public long lastUpdatedAt { get; set; }
        [Display(Name = "Latitude & Longtuide")]
        public latLng latLng { get; set; }
        [Display(Name = "Phone Number")]
        public string phoneNumber { get; set; }
        [Display(Name = "Status")]
        public string status { get; set; }
    }
}
