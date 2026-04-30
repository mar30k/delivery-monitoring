using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DeliveryMonitoring.Models
{
    public class OrderDto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("assigned_driver_phone_number")]
        public string? AssignedDriverPhoneNumber { get; set; }
        public string? AssignedDriverName { get; set; }
        public bool IsDriverFreelnace { get; set; }
        public string? DriverPhoneNumber => AssignedDriverPhoneNumber;

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("voucher_code")]
        public string? VoucherCode { get; set; }

        [JsonProperty("except_drivers")]
        public string? ExceptDrivers { get; set; }

        [JsonProperty("company_code")]
        public int? CompanyCode { get; set; }

        [JsonProperty("company_name")]
        public string? CompanyName { get; set; }

        [JsonProperty("branch_name")]
        public string? BranchName { get; set; }

        [JsonProperty("company_tin")]
        public string? CompanyTin { get; set; }

        [JsonProperty("delivery_tin")]
        public string? DeliveryTin { get; set; }

        [JsonProperty("target_branch_lat")]
        public double? TargetBranchLat { get; set; }

        [JsonProperty("target_branch_lng")]
        public double? TargetBranchLng { get; set; }

        [JsonProperty("eta")]
        public DateTime? EtaTime { get; set; }
        public double Eta
        {
            get
            {
                if (!EtaTime.HasValue)
                    return 0;

                var minutes = (double)(EtaTime.Value - CreatedAt).TotalMinutes;

                return minutes;
            }
        }
        public double Duration
        {
            get
            {
                var minutes = (double)(UpdatedAt - CreatedAt).TotalMinutes;

                return minutes;
            }
        }
        public double EtaDifference => Math.Round(Eta - Duration, 2);

        [JsonProperty("delivery_date_time")]
        public DateTime? DeliveryDateTime { get; set; } // ✅ FIXED

        [JsonProperty("supervised_by")]
        public string? SupervisedBy { get; set; }
        public string? SupervisorName => SupervisedBy;

        [JsonProperty("sos_reason")]
        public string? SosReason { get; set; }

        [JsonProperty("grand_total")]
        public decimal? GrandTotal { get; set; }
        public decimal? TotalAmount => GrandTotal;

        [JsonProperty("customer_device_id")]
        public string? CustomerDeviceId { get; set; }

        [JsonProperty("customer_first_name")]
        public string? CustomerName { get; set; }
        public string? FirstName => CustomerName;

        [JsonProperty("customer_phone_number")]
        public string? CustomerPhoneNumber { get; set; }
        public string? PhoneNumber => CustomerPhoneNumber;

        [JsonProperty("customer_geocode_address")]
        public string? CustomerGeocodeAddress { get; set; }

        [JsonProperty("customer_specific_address")]
        public string? CustomerSpecificAddress { get; set; }

        [JsonProperty("customer_lat")]
        public double? CustomerLat { get; set; }

        [JsonProperty("customer_lng")]
        public double? CustomerLng { get; set; }

        [JsonProperty("driver_assigned_time")]
        public DateTime? DriverAssignedTime { get; set; }

        [JsonProperty("is_assigned_ack")]
        public bool? IsAssignedAck { get; set; }

        [JsonProperty("is_no_drivers_ack")]
        public bool? IsNoDriversAck { get; set; }

        [JsonProperty("order_arrived_ack_by_customer")]
        public bool? OrderArrivedAckByCustomer { get; set; }

        [JsonProperty("order_arrived_ack_by_driver")]
        public bool? OrderArrivedAckByDriver { get; set; }

        [JsonProperty("platform")]
        public string? Platform { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        public DateTime RequestCreatedAt => CreatedAt;
        public string RequestCreatedAtString => CreatedAt.ToString("yyyy-MM-dd hh:mm:ss tt");

        [JsonProperty("preparation_time")]
        public int? PreparationTime { get; set; }

        [JsonProperty("status_tracker_id")]
        public string? StatusTrackerId { get; set; }

        [JsonProperty("customer_special_request")]
        public string? CustomerSpecialRequest { get; set; }

        [JsonProperty("status_report")]
        public string? StatusReport { get; set; }

        [JsonProperty("order_printed")]
        public bool? OrderPrinted { get; set; }

        [JsonProperty("photo_attachment")]
        public string? PhotoAttachment { get; set; }

        [JsonProperty("payment_method")]
        public string? PaymentMethod { get; set; }

        [JsonProperty("preserve_e_t_a")]
        public object? PreserveETA { get; set; }

        [JsonProperty("driver_commission_amount")]
        public decimal? DriverCommissionAmount { get; set; }

        [JsonProperty("delivery_distance")]
        public double? DeliveryDistance { get; set; }
        public double? Distance => DeliveryDistance;

        [JsonProperty("items_json")]
        public object? ItemsJson { get; set; }

        [JsonProperty("payment_ref_number")]
        public string? PaymentRefNumber { get; set; }
    }
}
