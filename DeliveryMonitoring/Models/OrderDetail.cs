using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Humanizer;
using System.ComponentModel;

namespace DeliveryMonitoring.Models
{

    public class OrderDetail
    {
        [DisplayName("Driver Phone Number")]
        public string? AssignedDriverPhoneNumber { get; set; }

        [DisplayName("Branch Name")]
        public string? BranchName { get; set; }

        [DisplayName("Company Code")]
        public int? CompanyCode { get; set; }

        [DisplayName("Company Name")]
        public string? CompanyName { get; set; }

        [DisplayName("Company TIN")]
        public string? CompanyTin { get; set; }
        [DisplayName("Delivery TIN")]
        public string? DeliveryTin { get; set; }
        public string? SupervisedBy { get; set; }
        public string? SosReason { get; set; }
        public string? GrandTotal { get; set; }

        [DisplayName("Customer")]
        public CustomerDetail? Customer { get; set; }

        [DisplayName("Driver Assigned At")]
        public long? DriverAssignedAt { get; set; }

        [DisplayName("Is Assigned Acknowledged")]
        public bool? IsAssignedAck { get; set; }

        [DisplayName("Is No Drivers Acknowledged")]
        public bool? IsNoDriversAck { get; set; }

        [DisplayName("Order Arrived Ack by Customer")]
        public bool? OrderArrivedAckByCustomer { get; set; }

        [DisplayName("Order Arrived Ack by Driver")]
        public bool? OrderArrivedAckByDriver { get; set; }
        public string? Platform { get; set; }

        [DisplayName("Request Created At")]
        public long RequestCreatedAt { get; set; }
        public DateTime RequestCreatedAtIso { get; set; }

        [DisplayName("Status")]
        public string? Status { get; set; }

        [DisplayName("Target Branch Location")]
        public Location? TargetBranchLocation { get; set; }

        [DisplayName("Voucher Code")]
        public string? VoucherCode { get; set; }
        public string? Alert { get; set; }

        [DisplayName("Line Items Detail")]
        public LineItemsDetail? LineItemsDetail { get; set; }

        [DisplayName("Activities")]
        public Activities? Activities { get; set; }
        public DateTime? OrderAcceptedNotification { get; set; }
        public DateTime? OrderReceiveNotification { get; set; }
        public string[]? ExceptDrivers { get; set; } = Array.Empty<string>();
    }

    public class CustomerDetail
    {
        [DisplayName("Device ID")]
        public string? DeviceID { get; set; }

        [DisplayName("First Name")]
        public string? FirstName { get; set; }

        [DisplayName("Geocode Address")]
        public string? GeocodeAddress { get; set; }

        [DisplayName("LatLng")]
        public Location? LatLng { get; set; }

        [DisplayName("Phone Number")]
        public string? PhoneNumber { get; set; }

        [DisplayName("Specific Address")]
        public string? SpecificAddress { get; set; }
    }

    public class Location
    {
        [DisplayName("Latitude")]
        public double? Lat { get; set; }

        [DisplayName("Longitude")]
        public double? Lng { get; set; }
    }

    public class LineItemsDetail
    {
        [DisplayName("Line Items")]
        public List<LineItem>? LineItems { get; set; }

        [DisplayName("Extra Charge")]
        public Dictionary<string, decimal>? ExtraCharge { get; set; }

        [DisplayName("Grand Total")]
        public decimal? GrandTotal { get; set; }

        [DisplayName("Extra Information")]
        public Dictionary<string, object>? ExtraInformation { get; set; }

        [DisplayName("Extra Data")]
        public ExtraData? ExtraData { get; set; }

        [DisplayName("Issued Date")]
        public DateTime? IssuedDate { get; set; }

        [DisplayName("Branch Code")]
        public int? BranchCode { get; set; }

        [DisplayName("Promo Detail")]
        public string? PromoDetail { get; set; }

        [DisplayName("Phone Number")]
        public string? PhoneNumber { get; set; }

        [DisplayName("Company Name")]
        public string? CompanyName { get; set; }

        [DisplayName("Voucher Code")]
        public string? VoucherCode { get; set; }
    }

    public class LineItem
    {
        [DisplayName("Article")]
        public int? Article { get; set; }

        [DisplayName("Name")]
        public string? Name { get; set; }

        [DisplayName("Unit Amount")]
        public decimal? UnitAmount { get; set; }

        [DisplayName("Quantity")]
        public int? Quantity { get; set; }

        [DisplayName("Taxable Amount")]
        public decimal? TaxableAmount { get; set; }
    }

    public class ExtraData
    {
        [DisplayName("Voucher ID")]
        public int? VoucherId { get; set; }

        [DisplayName("TIN")]
        public string? Tin { get; set; }
    }

    public class Activities
    {
        [DisplayName("Start Time")]
        public DateTime? StartTime { get; set; }

        [DisplayName("Current Time")]
        public DateTime? CurrentTime { get; set; }

        [DisplayName("Expected Time Of Arrival")]
        public DateTime? Eta { get; set; }

        [DisplayName("Actual Arrival Time")]
        public DateTime? ActualArrival { get; set; }

        [DisplayName("Alert")]
        public string? Alert { get; set; }

        [DisplayName("Activity Response")]
        public List<ActivityResponse>? ActivityResponse { get; set; }
    }

    public class ActivityResponse
    {
        [DisplayName("Name")]
        public string? Name { get; set; }

        [DisplayName("Time")]
        public DateTime? Time { get; set; }
        public string? TimeElapsed { get; set; }
    }


    public class OrderViewModel
    {
        public List<OrderDetail>? OrderDetail { get; set; }
        public List<SupervisorsDTO>? Supervisors { get; set; }
    }
}
