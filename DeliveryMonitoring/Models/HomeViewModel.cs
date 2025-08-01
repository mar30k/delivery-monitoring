namespace DeliveryMonitoring.Models
{
    public class HomeViewModel
    {
        public List<Driver>? Drivers { get; set; }
        public HulubejeResponse<List<CompletedOrders>>? CompletedOrders { get; set; }
        public HulubejeResponse<List<CompletedOrders>>? TakeAwayOrders { get; set; }
        public HulubejeResponse<List<CompletedOrders>>? DineInOrders { get; set; }
        public List<OrderDetail>? Orders { get; set; }
        public List<DeviceControl>? DeviceControl { get; set; }
        public List<SupervisorsDTO>? Supervisors { get; set; }
        public Companies? Comps { get; set; }
        public string? CompanyTin { get; set; }
    }
}
