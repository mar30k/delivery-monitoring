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
        public string? Id { get; set; }
        public string? AssignedDriverPhoneNumber { get; set; }

        public string? BranchName { get; set; }
        public int? CompanyCode { get; set; }

        public string? CompanyName { get; set; }

        public string? CompanyTin { get; set; }
        public string? DeliveryTin { get; set; }
        public string? SupervisedBy { get; set; }
        public string? SupervisorName { get; set; }
        public string? SosReason { get; set; }
        public decimal? GrandTotal { get; set; }

        public CustomerDetail? Customer { get; set; }
        public string? CustomerDeviceID { get; set; }

        public string? CustomerFirstName { get; set; }

        public string? CustomerGeocodeAddress { get; set; }

        public double? CustomerLat { get; set; }

        public double? CustomerLng { get; set; }

        public string? CustomerPhoneNumber { get; set; }

        public string? CustomerSpecificAddress { get; set; }

        public long? DriverAssignedAt { get; set; }

        public bool? IsAssignedAck { get; set; }

        public bool? IsNoDriversAck { get; set; }

        public bool? OrderArrivedAckByCustomer { get; set; }

        public bool? OrderArrivedAckByDriver { get; set; }
        public string? Platform { get; set; }

        public long RequestCreatedAt { get; set; }
        public string CreatedAtString { get; set; }
        public DateTime? RequestCreatedAtIso { get; set; }
        public DateTime? DriverAssignedTime { get; set; }
        public DateTime? DeliveryDateTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? Eta { get; set; }
        public string? Status { get; set; }
        public int? PreparationTime { get; set; }
        public string? StatusTrackerId { get; set; }
        public string? CustomerSpecialRequest { get; set; }
        public string? StatusReport { get; set; }
        public Location? TargetBranchLocation { get; set; }

        public double? TargetBranchLat { get; set; }

        public double? TargetBranchLng { get; set; }

        public string? VoucherCode { get; set; }
        public string? Alert { get; set; }

        public LineItemsDetail? LineItemsDetail { get; set; }

        public Activities? Activities { get; set; }
        public DateTime? OrderAcceptedNotification { get; set; }
        public DateTime? OrderReceiveNotification { get; set; }
        public string? ExceptDrivers { get; set; }
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
        public double? lat { get; set; }

        [DisplayName("Longitude")]
        public double? lng { get; set; }
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
        public DateTime IssuedDate { get; set; }

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
        public decimal? Quantity { get; set; }

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
