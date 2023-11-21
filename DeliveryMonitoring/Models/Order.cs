using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Order
    {
        [Display(Name = "Driver Phone Number")]
        public string assignedDriverPhoneNumber { get; set; }
        [Display(Name = "Branch Name")]
        public string branchName { get; set; }
        [Display(Name = "Company TIN")]
        public string companyTin { get; set; }
        [Display(Name = "Customer")]
        public Customer customer { get; set; }
        [Display(Name = "Driver Assigned Acknowledged")]
        public bool isAssignedAck { get; set; }
        [Display(Name = "No Driver Acknowledged")]
        public bool isNoDriversAck {  get; set; }
        [Display(Name = "Order Arrived Acknowledged by Customer")]
        public bool orderArrivedAckByCustomer { get; set; }
        [Display(Name = "Order Arrived Acknowledged by Driver")]
        public bool orderArrivedAckByDriver { get; set; }
        [Display(Name = "Request Created At")]
        public long requestCreatedAt { get; set; }
        [Display(Name = "Status")]
        public string status { get; set; }
        public latLng latlng { get; set; }
        [Display(Name = "Voucher Code")]
        public string voucherCode { get; set; }
    }
}
