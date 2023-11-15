using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Companies
    {
        [Display(Name="Company TIN")]
        public List<string> companyTins { get; set; }
    }
}
