using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{

    public class Detail
    {
        public string? fullName { get; set; }
        public string? email { get; set; }
        public string? idNumber { get; set; }
        public string? idType { get; set; }
        public string? plateNumber { get; set; }
        public string? gender { get; set; }
        public string? occupation { get; set; }
        public DateTime? dob { get; set; }
        public string? profilePicture { get; set; }
    }

    public class Review
    {
        public double? totalRating { get; set; }
        public int? totalReviews { get; set; }
    }

    public class Driver
    {
        public string? companyTin { get; set; }
        public string? deviceID { get; set; }
        public string? firstName { get; set; }
        public bool? isDisabled { get; set; }
        public long? lastUpdatedAt { get; set; }
        public string? lastUpdatedAtIso { get; set; }
        public Location latLng { get; set; }
        public string? phoneNumber { get; set; }
        public string? status { get; set; }
        public Detail detail { get; set; }
        public int? numberOfAcceptedOrders { get; set; }
        public int? numberOfRejectedOrders { get; set; }
        public Review? review { get; set; }
        public int? traveledDistance { get; set; }
    }

}
