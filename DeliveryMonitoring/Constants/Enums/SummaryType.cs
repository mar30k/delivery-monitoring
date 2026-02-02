namespace DeliveryMonitoring.Constants.Enums
{
    /// <summary>
    /// Represents the different types of summaries that can be generated in the system.
    /// </summary>
    public enum SummaryType
    {
        Merchant,
        Driver,
        Supervisor,
        Consignee
    }

    /// <summary>
    /// Represents the different types of orders that can be used to generate reports in the system.
    /// </summary>
    public enum ReportByOrderType
    {
        Delivery,
        ScheduledDeliveryToLocation,
        ScheduledPickUp,
        DineIn,
        Takeaway,
        All
    }

}
