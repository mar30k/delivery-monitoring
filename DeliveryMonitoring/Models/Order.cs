using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class Order
    {
        [Display(Name = "Branch Name")]
        public string branchName { get; set; }
        [Display(Name = "Company TIN")]
        public long companyTin { get; set; }
        [Display(Name = "Customer")]
        public Customer customer { get; set; }
        public bool isNoDriverAck {  get; set; }
        public bool orderArrivedAckByCustomer { get; set; }
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
