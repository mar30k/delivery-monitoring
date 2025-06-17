using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Order
    {
        [Display(Name = "Driver Phone Number")]
        public string assignedDriverPhoneNumber { get; set; }

        [Display(Name = "Branch Name")]
        public string branchName { get; set; }

        [Display(Name = "Company Code")]
        public int companyCode { get; set; }

        [Display(Name = "Company Name")]
        public string companyName { get; set; }

        [Display(Name = "Company TIN")]
        public string companyTin { get; set; }

        public Customer customer { get; set; }

        [Display(Name = "Driver Assigned At")]
        public long driverAssignedAt { get; set; }

        [Display(Name = "Is Assigned Acknowledged")]
        public bool isAssignedAck { get; set; }

        [Display(Name = "Is No Drivers Acknowledged")]
        public bool isNoDriversAck { get; set; }

        [Display(Name = "Order Arrived Acknowledgment by Customer")]
        public bool orderArrivedAckByCustomer { get; set; }

        [Display(Name = "Order Arrived Acknowledgment by Driver")]
        public bool orderArrivedAckByDriver { get; set; }

        [Display(Name = "Request Created At")]
        public long requestCreatedAt { get; set; }
        public DateTime requestCreatedAtIso { get; set; }

        public string status { get; set; }
        public string lineItemsDetail { get; set; }

        [Display(Name = "Target Branch Location")]
        public Location targetBranchLocation { get; set; }

        [Display(Name = "Voucher Code")]
        public string voucherCode { get; set; }
    }

    public class Customer
    {
        [Display(Name = "Device ID")]
        public string deviceID { get; set; }

        [Display(Name = "First Name")]
        public string firstName { get; set; }

        [Display(Name = "Customer Address")]
        public string geocodeAddress { get; set; }

        public Location latLng { get; set; }

        [Display(Name = "Customer Phone Number")]
        public string phoneNumber { get; set; }

        [Display(Name = "Specific Address")]
        public string specificAddress { get; set; }
    }

}
