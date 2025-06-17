using System.ComponentModel;

namespace DeliveryMonitoring.Models
{
    public class VerifyIdModel
    {
        [DisplayName("Identification No.")]
        public string? myId { get; set; }
        [DisplayName("Remember")]
        public bool remember { get; set; }
    }
    public class EntityModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int GslType { get; set; }
        public string Tin { get; set; }
        public string BioId { get; set; }
        public string NationalId { get; set; }
        public string PassportId { get; set; }
        public bool IsPerson { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string Gender { get; set; }
        public string BusinessType { get; set; }
        public int Preference { get; set; }
        public DateTime StartDate { get; set; }
        public string Nationality { get; set; }
        public bool IsActive { get; set; }
        public string MaritalStatus { get; set; }
        public string Note { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; }
        public int MainConsigneeUnit { get; set; }
        public string BaseUrl { get; set; }
        public int? ParentId { get; set; }
        public string Department { get; set; }
        public string Branch { get; set; }
        public string Position { get; set; }
        public string CommunicationSource { get; set; }
        public string DefaultLanguage { get; set; }
        public string DefaultCurrency { get; set; }
        public string DefaultImageUrl { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? TransactionLimit { get; set; }
        public bool Locked { get; set; }
        public string Remark { get; set; }
    }
}
