namespace DeliveryMonitoring.Models
{
    public class DeviceControl
    {

        public string? Tin { get; init; }
        public string? CompanyName { get; init; }
        public string? BranchName { get; init; }
        public string? DeviceName { get; init; }
        public string? DeviceType { get; init; }
        public string? MachineNo { get; init; }
        public string? HostDevice { get; set; }
        public string? UserName { get; init; }
        public string? Note { get; init; }
        public DateTime? TimeStamp { get; init; }
        public bool Seen { get; set; }
        public string? Version { get; init; }
        public string? Remark { get; init; }

    }
}
