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

    /// <summary>
    /// Represents the different types of orders that can be used to generate reports in the system.
    /// </summary>
    public enum ReportByOrderType
    {
        /// <summary>
        /// Standard delivery orders.
        /// </summary>
        Delivery,

        /// <summary>
        /// Delivery orders that are scheduled in advance for a specific location.
        /// </summary>
        ScheduledDeliveryToLocation,

        /// <summary>
        /// Orders scheduled for pickup at the restaurant or branch.
        /// </summary>
        ScheduledPickUp,

        /// <summary>
        /// Orders placed to be consumed on-site at the restaurant.
        /// </summary>
        Dinein,

        /// <summary>
        /// Orders prepared for immediate customer pickup (not scheduled).
        /// </summary>
        Takeaway,

        /// <summary>
        /// Represents all order types, used for generating reports without filtering by a specific type.
        /// </summary>
        All
    }

}
