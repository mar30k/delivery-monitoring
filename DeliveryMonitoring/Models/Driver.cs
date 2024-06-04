using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Driver
    {
        [Display(Name = "Company TIN")]
        public string companyTin { get; set; }

        [Display(Name = "Device ID")]
        public string deviceID { get; set; }

        [Display(Name = "Name")]
        public string firstName { get; set; }

        [Display(Name = "Last Seen")]
        public long lastUpdatedAt { get; set; }

        [Display(Name = "Latitude & Longtuide")]
        public latLng latLng { get; set; }

        [Display(Name = "Accepted Orders")]
        public int numberOfAcceptedOrders { get; set; }

        [Display(Name = "Phone Number")]
        public string phoneNumber { get; set; }

        [Display(Name = "Review")]
        public Review review { get; set; }

        [Display(Name = "Status")]
        public string status { get; set; }

        [Display(Name = "Traveled Distance")]
        public int traveledDistance { get; set; }

        [Display(Name = "Detail")]
        public Detail detail { get; set; }

        //[Display(Name = "Work Eligibility")]
        //public bool isDisabled { get; set; }
    }
    public class Review
    {
        [Display(Name = "Total Rating")]
        public int totalRating { get; set; }

        [Display(Name = "Total Reviews")]
        public int totalReviews { get; set; }
    }

    public class Detail
    {
        [Display(Name = "Full Name")]
        public string fullName { get; set; }
        
        [Display(Name = "Email")]
        public string email { get; set; }
        
        [Display(Name = "Gender")]
        public string gender { get; set; }
        
        [Display(Name = "Occupation")]
        public string? occupation { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime dob { get; set; }

        [Display(Name = "Profile Picture")]
        public string? profilePicture { get; set; }
    }
}
