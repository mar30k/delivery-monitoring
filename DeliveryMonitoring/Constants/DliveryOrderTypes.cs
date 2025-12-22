namespace DeliveryMonitoring.Constants
{
    /// <summary>
    /// Represents the different types of delivery services available.
    /// </summary>
    public enum DeliveryOrderTypes
    {
        /// <summary>
        /// The customer will pick up the order at the branch.
        /// </summary>
        PickUpAtBranch = 2076,

        /// <summary>
        /// The order will be delivered directly to the customer’s location.
        /// </summary>
        DeliveryToLocation = 2077,

        /// <summary>
        /// The order will be picked up at a scheduled time.
        /// </summary>
        ScheduledPickUp = 2078,

        /// <summary>
        /// The order will be delivered at a scheduled time to the customer’s location.
        /// </summary>
        ScheduledDeliveryToLocation = 2079,

        /// <summary>
        /// The customer will dine in at the restaurant.
        /// </summary>
        InHouseDining = 3203
    }


    
}
