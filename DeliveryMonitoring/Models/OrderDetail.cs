using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeliveryMonitoring.Models
{
    public class OrderDetail
    {
        [Display(Name = "Driver Phone Number")]
        public string assignedDriverPhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Branch Name")]
        public string branchName { get; set; }

        [Display(Name = "Company Name")]
        public string companyName { get; set; }

        [Display(Name = "Company TIN")]
        public string companyTin { get; set; }

        [Display(Name = "Customer")]
        public Customer customer { get; set; }

        [Display(Name = "Driver Assigned Acknowledged")]
        public bool isAssignedAck { get; set; }

        [Display(Name = "No Driver Acknowledged")]
        public bool isNoDriversAck { get; set; }

        [Display(Name = "Order Arrived Acknowledged by Customer")]
        public bool orderArrivedAckByCustomer { get; set; }

        [Display(Name = "Order Arrived Acknowledged by Driver")]
        public bool orderArrivedAckByDriver { get; set; }

        [Display(Name = "Request Created At")]
        public long requestCreatedAt { get; set; }

        [Display(Name = "Status")]
        public string status { get; set; }

        public latLng targetBranchLocation { get; set; }

        [Display(Name = "Voucher Code")]
        public string voucherCode { get; set; }

        [Display(Name = "Item Name")]
        public LineItemsDetail lineItemsDetail { get; set; }
    }

    public class LineItemsDetail
    {
        public List<LineItems> lineItems { get; set; }
        public ExtraCharge extraCharge { get; set; }
        public double grandTotal { get; set; }
        public ExtraInformation extraInformation { get; set; }
        public ExtraData extraData { get; set; }
    }
    public class LineItems
    {
        [Display(Name = "Article")]
        public int article { get; set; }

        [Display(Name = "Name")]
        public string name { get; set; }

        [Display(Name = "Unit Amount")]
        public double unitAmount { get; set; }

        [Display(Name = "Quantity")]
        public int quantity { get; set; }

        [Display(Name = "Taxable Amount")]
        public double taxableAmount { get; set; }
    }
    public class ExtraCharge
    {
        [JsonPropertyName("TXBL 1")]
        public double TXBL1 { get; set; }
        [JsonPropertyName("TAX1 15%")]
        public double TAX115 { get; set; }
    }
    public class ExtraInformation
    {

    }
    public class ExtraData
    {
        public int voucherId { get; set; }
        public string tin { get; set; }
    }
}
