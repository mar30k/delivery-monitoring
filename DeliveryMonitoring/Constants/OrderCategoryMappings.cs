namespace DeliveryMonitoring.Constants
{
    public static class OrderCategoryMappings
    {
        public static readonly Dictionary<string, string> CategoryColors = new()
        {
            { "Good", "green" },
            { "Very Critical", "red" },
            { "Restaurant Related", "purple" },
            { "Vehicle Related", "darkred" },
            { "Customer Related", "orange" },
            { "System Error", "blue" },
            { "Other", "gray" }
        };

        public static readonly Dictionary<string, string> PurposeCategories = new()
        {
            // Good
            { "Successful Delivery", "Good" },
            { "Successful Pickup", "Good" },
            { "Successful Dining", "Good" },

            // Customer Related
            { "Ordered By Mistake", "Customer Related" },
            { "Incorrect Delivery Address", "Customer Related" },
            { "Incorrectly Marked As Delivered", "Customer Related" },
            { "Address Out Of Range", "Customer Related" },
            { "Wrong Order Placed", "Customer Related" },
            { "Customer Unreachable", "Customer Related" },

            // Restaurant Related
            { "Item Out Of Stock", "Restaurant Related" },
            { "Long Preparation Time", "Restaurant Related" },
            { "Order Declined By Restaurant", "Restaurant Related" },
            { "Restaurant Closed", "Restaurant Related" },
            { "Special Request Not Possible", "Restaurant Related" },

            // Vehicle Related
            { "Traffic Or Road Blockage", "Vehicle Related" },
            { "Weather Conditions", "Vehicle Related" },
            { "Vehicle Accident", "Vehicle Related" },
            { "Vehicle Malfunction", "Vehicle Related" },
            { "Vehicle Out of Charge or Fuel", "Vehicle Related" },
            { "Personal Emergency", "Customer Related" },

            // System Error
            { "Duplicate Order", "System Error" },

            // Very Critical
            { "Robbery", "Very Critical" },
            { "Delayed Delivery", "Very Critical" }
        };

        public static readonly Dictionary<string, string> DeliveryStatusMessages = new()
        {
            { "sent", "Order Placed" },
            { "prepared", "Your order invoice is printed" },
            { "received", "Order received and is being prepared" },
            { "accepted", "Order delivery accepted by the driver" },
            { "seen", "Order delivery accepted by the supervisor" },
            { "declined", "Order delivery declined by the driver" },
            { "assigned", "Order delivery assigned to a driver" },
            { "drivernotfound", "No driver found for your delivery" },
            { "completed", "Order delivery completed" },
            { "sos", "Delivery issue reported" },
            { "ontheway", "Your order is picked up and the driver is on the way" },
            { "arrived", "Driver has arrived at the destination" },
            { "arrivedatbranch", "Driver has arrived at the pickup location" },
            { "done", "Kitchen has finished cooking your order" },
        };
    }
}
