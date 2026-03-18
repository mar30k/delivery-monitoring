using CNET_V7_Domain.Domain.SecuritySchema;
using CNET_V7_Domain.Misc;
using DeliveryMonitoring.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeliveryMonitoring.Services.Api
{
    /// <summary>
    /// Defines all API communication contracts for Delivery Monitoring.
    /// Provides methods to interact with drivers, orders, supervisors, companies, and messaging services.
    /// </summary>
    public interface IApiRequestService
    {
        #region Order Requests

        /// <summary>
        /// Retrieves all active or pending order requests for the current company.
        /// </summary>
        Task<List<OrderDetail>> GetOrderRequestsAsync(string? tin = null);

        /// <summary>
        /// Retrieves detailed information about a specific order by its voucher number.
        /// </summary>
        /// <param name="voucherNumber">The voucher number identifying the order.</param>
        Task<OrderDetail?> GetOrderDetailByVoucher(string voucherNumber);

        /// <summary>
        /// Assigns a supervisor to an order.
        /// </summary>
        /// <param name="assignSuperVisorDTO">Supervisor assignment details.</param>
        Task<HulubejeResponse<bool>> AssignOrderSupervisorAsync(AssignSuperVisorDTO assignSuperVisorDTO);

        /// <summary>
        /// Assigns a supervisor to a completed order in history.
        /// </summary>
        Task<HulubejeResponse<bool>> AssignCompletedOrderSupervisorAsync(string voucherCode, int userId);

        /// <summary>
        /// Changes the status of a specific order (e.g., pending, in transit, delivered).
        /// </summary>
        /// <param name="changeOrderStatusDto">Object containing order status change details.</param>
        Task<HulubejeResponse<bool>> ChangeOrderStatusAsync(object changeOrderStatusDto);

        #endregion

        #region Drivers

        /// <summary>
        /// Retrieves a list of available drivers for dispatching orders.
        /// </summary>
        Task<List<Driver>> GetAvailableDriversAsync(bool skipCache = true);

        /// <summary>
        /// Retrieves driver details by their registered phone number.
        /// </summary>
        /// <typeparam name="T">Type of the driver model to deserialize to.</typeparam>
        /// <param name="phoneNumber">Driver’s phone number.</param>
        Task<T?> GetDriverDetailsByPhoneNumber<T>(string phoneNumber, bool skipCache = true);

        /// <summary>
        /// Updates driver details such as location, availability, or assigned orders.
        /// </summary>
        /// <param name="driverModel">Updated driver details.</param>
        /// <param name="phoneNumber">Driver’s phone number to identify the record.</param>
        Task<HulubejeResponse<bool>> UpdateDriverDetailsAsync(UpdateDriverModel driverModel, string phoneNumber);

        /// <summary>
        /// Redispatches a driver for an order (used when the initial driver is unavailable).
        /// </summary>
        /// <param name="orderDetail">The order details for redispatching.</param>
        Task<HulubejeResponse<bool>> RedispatchDriversAsync(OrderDetail orderDetail);

        /// <summary>
        /// Retrieves driver route details between two geographical points.
        /// </summary>
        /// <param name="lat1">Starting latitude.</param>
        /// <param name="lng1">Starting longitude.</param>
        /// <param name="lat2">Destination latitude.</param>
        /// <param name="lng2">Destination longitude.</param>
        /// <param name="profile">Routing profile (e.g., car).</param>
        Task<RouteModel> GetDriverRouteDetailAsync(string lat1, string lng1, string lat2, string lng2, string profile);

        /// <summary>
        /// Retrieves all reviews and ratings for a driver.
        /// </summary>
        /// <param name="phoneNumber">Driver phone number.</param>
        /// <param name="page">Page number for pagination.</param>
        /// <param name="retriveAllReviews">If true, retrieves all available reviews.</param>
        Task<HulubejeResponse<DriverReview>?> GetDriverReviewsAsync(string phoneNumber, int page, bool retriveAllReviews = true);

        /// <summary>
        /// Retrieves activities and status updates for a specific driver.
        /// </summary>
        /// <param name="companyCode">company code.</param>
        /// <param name="voucherCode">order unique identification</param>
        Task<HulubejeResponse<Activities>?> GetDriverActivityAsync(string companyCode, string voucherCode,  bool skipCache = true);

        #endregion

        #region Supervisors & Companies

        /// <summary>
        /// Retrieves a list of all supervisors in the system.
        /// </summary>
        Task<List<SupervisorsDTO>> GetSupervisorsAsync();

        /// <summary>
        /// Retrieves a list of all companies available in the system.
        /// </summary>
        Task<Companies> GetCompaniesAsync();

        /// <summary>
        /// Retrieves detailed information for a specific company.
        /// </summary>
        /// <param name="companyTin">The Tax Identification Number (TIN) of the company.</param>
        /// <param name="skipCache">
        /// If set to <c>true</c>, bypasses the cache and fetches fresh data.
        /// </param>
        Task<HulubejeResponse<Company>?> GetCompanyDetailsAsync(string companyTin, bool skipCache = false);

        /// <summary>
        /// Updates the branch information for a specific order.
        /// </summary>
        /// <param name="changeBranchDTO">The DTO containing the order voucher, new branch code, branch name, and optional remark.</param>
        /// <returns>
        /// A <see cref="HulubejeResponse{bool}"/> indicating whether the branch change was successful, including any error messages if applicable.
        /// </returns>
        Task<HulubejeResponse<bool>> ChangeOrderBranchAsync(ChangeBranchDTO changeBranchDTO);


        #endregion

        #region Completed Orders

        /// <summary>
        /// Retrieves all pending orders that are not yet in the completed orders list.
        /// </summary>
        /// <param name="skipCache">
        /// If set to <c>true</c>, bypasses the cache and fetches the latest data.
        /// </param>
        Task<HulubejeResponse<List<CompletedOrders>>> GetPendingOrdersAsync(bool skipCache = true);
        /// <summary>
        /// Completes a pending order by submitting the required delivery and assignment details.
        /// </summary>
        /// <param name="request">
        /// The pending order object containing completion information such as
        /// driver assignment, distance, duration, and ETA.
        /// </param>
        /// <returns>
        /// A <see cref="HulubejeResponse{Boolean}"/> indicating whether the operation
        /// completed successfully, along with any validation or processing errors.
        /// </returns>
        Task<HulubejeResponse<bool>> CompletePendingOrderAsync(OrderCompletionRequest request);
        /// <summary>
        /// Retrieves all completed orders associated with the logged-in company.
        /// </summary>
        /// <param name="skipCache">
        /// If set to <c>true</c>, bypasses the cache and fetches the latest data.
        /// </param>
        Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(bool skipCache = true);
        /// <summary>
        /// Retrieves all completed orders and their associated activities.
        /// </summary>
        /// <param name="skipCache">
        /// If set to <c>true</c>, bypasses the cache and fetches the latest data.
        /// </param>
        Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedordersWithTimeineAsync(string startDate, string endDate, bool skipCache = true);

        /// <summary>
        /// Retrieves completed orders filtered by the specified order type.
        /// </summary>
        /// <param name="type">The identifier of the order type to filter by.</param>
        /// <param name="skipCache">
        /// If set to <c>true</c>, bypasses the cache and fetches the latest data.
        /// </param>
        Task<HulubejeResponse<List<CompletedOrders>>> GetOrdersByTypeAsync(int type, bool skipCache = true);
        /// <summary>
        /// Retrieves detailed historical order information for a specific voucher and company.
        /// </summary>
        /// <param name="voucherCode">The voucher code identifying the order.</param>
        /// <param name="companyCode">The company code associated with the order.</param>
        /// <param name="industryType">The industry type identifier (default is 1992).</param>
        /// <param name="skipCache">Indicates whether to bypass the cache and fetch fresh data.</param>
        Task<HulubejeResponse<LineItemsDetail>> Gethistorydetail(string voucherCode, string companyCode, int industryType = 1992, bool skipCache = true);

        #endregion

        #region Device Control

        /// <summary>
        /// Retrieves device control records for a given date (e.g., device activity logs).
        /// </summary>
        /// <param name="date">Date string in yyyy-MM-dd format.</param>
        Task<List<DeviceControl>> GetDeviceControlAsync(string date);

        #endregion

        #region Delivery Purpose and Note

        /// <summary>
        /// Retrieves the list or description of delivery purposes (e.g., documents, goods, food, etc.).
        /// </summary>
        Task<string> GetDeliveryPurposeAsync();

        /// <summary>
        /// Saves the supervisor not about the selected delivery order.
        /// </summary>
        /// <param name="voucherCode">order unique identification</param>
        /// <param name="note">supervisor note about how the delivery went</param>
        /// <param name="voucherCode">option value for selected purpose of the note</param>
        Task<HulubejeResponse<bool>> SaveDeliveryNote(string voucherCode, string note, string purpose);

        #endregion

        #region Messaging & Activity Logging

        /// <summary>
        /// Sends an alert message (push notification or SMS) to a target device or driver.
        /// </summary>
        /// <param name="messageDto">Message details including title, body, and target ID.</param>
        Task<HulubejeResponse<bool>> SendMessageAsync(AlertMessageDto messageDto);

        /// <summary>
        /// Inserts an activity log entry for delivery or order status updates.
        /// </summary>
        /// <param name="request">Log entry data object.</param>
        Task<HulubejeResponse<bool>> InsertActivityLogAsync(object request);

        #endregion

        #region map
        // Existing methods...

        /// <summary>
        /// Fetches Google Maps JavaScript API content using the provided API key.
        /// </summary>
        Task<string> GetGoogleMapsJsAsync();
        #endregion

        #region login and authentication

        /// <summary>
        /// Retrieves filtered consignees based on company TIN.
        /// </summary>
        /// <param name="tin">Company TIN</param>
        Task<List<EntityModel>> GetFilteredConsigneesAsync(string tin);

        /// <summary>
        /// Updates the online status of a driver.
        /// </summary>
        /// <param name="isOnline">True if online, false if offline</param>
        /// <param name="phoneNumber">Supervisors's phone number</param>
        Task<HulubejeResponse<bool>> UpdateSupervisorsOnlineStatusAsync(bool isOnline, string phoneNumber);

        /// <summary>
        /// Authenticates a user with the given phone number and password.
        /// Returns a response containing login information and status.
        /// </summary>
        /// <param name="phoneNumber">The user's phone number used as username.</param>
        /// <param name="password">The user's password.</param>
        Task<ResponseModel<LoginResponse>> AuthenticateUser(string phoneNumber, string password);

        /// <summary>
        /// Retrieves user details based on the provided username (phone number).
        /// </summary>
        /// <param name="userName">The user's phone number or username.</param>
        Task<UserDTO?> GetUserByUserName(string userName);

        #endregion
    }
}