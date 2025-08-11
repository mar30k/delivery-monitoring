namespace DeliveryMonitoring.Models
{
    public class SupervisorsDTO
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public bool LoggedInStatus { get; set; }
        public bool IsActive { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? ThirdName { get; set; }
        public int? TotalSupervisedOrders { get; set; }
    }
    public class AssignSuperVisorDTO
    {
        public string? voucherCode { get; set; }
        public string? phoneNumber { get; set; }
    }
}
