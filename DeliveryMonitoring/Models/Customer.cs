using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Customer
    {
        [Display(Name = "Device ID")]
        public string deviceId { get; set; }

        [Display(Name = "Customer Name")]
        public string firstName { get; set; }

        [Display(Name = "Customer Address")]
        public string geocodeAddress { get; set; }

        public latLng latLng { get; set; }
        [Display(Name = "Customer Phone Number")]

        public string phoneNumber { get; set; }
        [Display(Name = "Specific Address")]

        public string specificAddress { get; set; }
    }
}
