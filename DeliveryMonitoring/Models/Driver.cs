using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{

    public class Detail
    {
        public string? FullName { get; set; }
        public string? IdNumber { get; set; }
        public int? IdType { get; set; }
        public string? BikeType { get; set; }
        public string? PlateNumber { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public DateTime? Dob { get; set; }
        public string? ProfilePicture { get; set; }
        public int? TotalDelivery { get; set; }
        public double? TotalKm { get; set; }
        public int? WeekDelivery { get; set; }
        public int? TodayDelivery { get; set; }
    }
    public class Review
    {
        public double? totalRating { get; set; }
        public int? totalReviews { get; set; }
    }

    public class Driver
    {
        public string? Id { get; set; }
        public string? CompanyTin { get; set; }
        public string? DeviceId { get; set; }
        public string? FirstName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsDisabled { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public int NumberOfAcceptedOrders { get; set; }
        public int NumberOfRejectedOrders { get; set; }
        public string? Status { get; set; }
        public string? LastUpdatedAtIso { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Location? LatLng { get; set; }
        public Detail? Detail { get; set; }
        public Review? Review { get; set; }
        public int? TraveledDistance { get; set; }
        public List<Location>? Coordinates { get; set; }
    }

}
