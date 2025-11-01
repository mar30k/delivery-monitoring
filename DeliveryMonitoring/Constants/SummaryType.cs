namespace DeliveryMonitoring.Constants
{
    /// <summary>
    /// Represents the different types of summaries that can be generated in the system.
    /// </summary>
    public enum SummaryType
    {
        /// <summary>
        /// Summary grouped by company tin, company name and branch.
        /// </summary>
        Merchant,

        /// <summary>
        /// Summary grouped by delivery driver.
        /// </summary>
        Driver,

        /// <summary>
        /// Summary grouped by supervisor.
        /// </summary>
        Supervisor,

        /// <summary>
        /// Summary grouped by consignee or customer.
        /// </summary>
        Consignee
    }

}
