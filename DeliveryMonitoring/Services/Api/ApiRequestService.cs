using CNET_V7_Domain.Domain.SecuritySchema;
using CNET_V7_Domain.Misc;
using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Cache;
using MediaBrowser.Model.Services;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryMonitoring.Services.Api
{
    public class ApiRequestService : IApiRequestService
    {
        #region Fields
        private readonly ICacheService _cacheService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _getDeviceControl;
        private readonly string _getRouteDetails;
        private readonly string _assignOrderSupervisor;
        private readonly string _changeorderbranch;
        private readonly string _updateOrderStatus;
        private readonly string _updateDriverDetails;
        private readonly HttpClient _client;
        private readonly HttpClient _deliveryClient;
        private readonly HttpClient _deviceControlClient;
        private readonly HttpClient _deliveryLoginClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _getOrderRequests;
        private readonly string _getDrivers;
        private readonly string _reDispatchDrivers;
        private readonly string _sendMessage;
        private readonly string _insertActivityLog;
        private readonly string _getDriverDetails;
        private readonly string _getDriverReviews;
        private readonly string _getSupervisors;
        private readonly string _getCompanies;
        private readonly string _getCompany;
        private readonly string _getOrderDetailByVoucher;
        private readonly string _getDriverActivityAsync;
        private readonly string _getHistroyDetail;
        private readonly string _saveDeliveryNote;
        private readonly string _getFilteredCustomers;
        private readonly string _updateOnlineStatus;
        private readonly string _authenicateUser;
        private readonly string _getUserByUserName;
        private readonly string _getCompletedOrders;
        private readonly string _getPendingOrders;
        private readonly string _completePendingOrder;
        private readonly string _getCompletedOrdersByType;
        private readonly IDataProtector _protector;
        private readonly string _googleMapsKey;
        private const string AdminCompanyTin = AppConstants.Company.AdminTin;
        private string CompanyTin => GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? "";
        private string ApibaseAddress => GetSecureCookie(CNET_WebConstantes.ApiBaseAddress) ?? "";
        #endregion

        #region Constructor
        public ApiRequestService(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration, 
            ICacheService cacheService,
            IDataProtectionProvider dataProtectionProvider)
        {
            _cacheService = cacheService;
            _client = httpClientFactory.CreateClient("CnetApiBaseUrl");
            _deliveryClient = httpClientFactory.CreateClient("Delivery");
            _deviceControlClient = httpClientFactory.CreateClient("ApiBaseUrl");
            _deliveryLoginClient = httpClientFactory.CreateClient("DeliveryLogin");
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _getOrderRequests = $"orderRequests?";
            _getOrderDetailByVoucher = "orderRequests/";
            _getDriverActivityAsync = "driveractivity/get?";
            _getHistroyDetail = "voucher/gethistorydetail?";
            _saveDeliveryNote = "delivery/savenote?";
            _getDriverDetails = "drivers/";
            _updateDriverDetails = "drivers/";
            _getDriverReviews = "review/get";
            _getRouteDetails = "routing/getRouteDetail";
            _getSupervisors = "auth/getsupervisors";
            _getCompanies = "companies";
            _getCompany= "routing/getcompanybytin";
            _sendMessage = "messaging/sendMessage";
            _insertActivityLog = "delivery/insertActivityLog";
            _reDispatchDrivers = "driver/dispatch";
            _getDrivers = "drivers";
            _getDeviceControl = $"deviceControl";
            _assignOrderSupervisor = "orderRequests/assignOrderSupervisor";
            _updateOrderStatus = "orderRequests/updateOrderStatus";
            _getFilteredCustomers = "Consignee/filter?";
            _updateOnlineStatus = "auth/status?";
            _authenicateUser = "SysInitialize/authenticate?";
            _getUserByUserName = "User/filter?";
            _getCompletedOrders = "voucher/getcompletedorders";
            _completePendingOrder = "voucher/completeorder";
            _getPendingOrders = "voucher/getpendingorders";
            _changeorderbranch = "voucher/changeorderbranch";
            _getCompletedOrdersByType = "voucher/getordersbytype?";
            _configuration = configuration;
            _googleMapsKey = _configuration["GoogleMapsApiKey"]; // ✅ Initialize here
            _protector = dataProtectionProvider.CreateProtector("DeliveryMonitoring.Cookies");
        }
        #endregion

        #region Order Requests
        public async Task<List<OrderDetail>> GetOrderRequestsAsync()
        {
            
            var response = await _deliveryClient.GetAsync($"{_getOrderRequests}companyTin={CompanyTin}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<OrderDetail>>(data) ?? new List<OrderDetail>();
            }
            return new List<OrderDetail>();
        }

        public async Task<OrderDetail?> GetOrderDetailByVoucher(string voucherNumber)
        {
            var response = await _deliveryClient.GetAsync($"{_getOrderDetailByVoucher}{voucherNumber}");
            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OrderDetail>(data);
        }

        public Task<HulubejeResponse<bool>> ChangeOrderStatusAsync(object changeOrderStatusDto)
            => SendAsync<bool>(_deliveryClient, _updateOrderStatus, changeOrderStatusDto, HttpMethod.Patch);
        #endregion

        #region Drivers
        public async Task<List<Driver>> GetAvailableDriversAsync(bool skipCahce = true)
        {
            string cacheKey = "available_drivers";
            string endpoint = CompanyTin == AdminCompanyTin ? _getDrivers : $"{_getDrivers}?companyTin={CompanyTin}";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _deliveryClient.GetAsync(endpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<Driver>>(data) ?? new List<Driver>();
                    }
                    return new List<Driver>();
                },
                skipCahce,
                10
            );
            
        }

        public async Task<T?> GetDriverDetailsByPhoneNumber<T>(string phoneNumber, bool skipCache = true)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return default;
            var cacheKey = $"driver_details_{phoneNumber}";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _deliveryClient.GetAsync($"{_getDriverDetails}{phoneNumber}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(data);
                    }
                    return default;
                },
                skipCache,
                60 // cache for 60 minutes
            );
        }

        /// <summary>
        /// Updates driver details for a given phone number.
        /// </summary>
        public Task<HulubejeResponse<bool>> UpdateDriverDetailsAsync(UpdateDriverModel driverModel, string phoneNumber)
        {
            // Use PostAsync<T> helper (could be renamed PatchAsync<T> if needed)
            return SendAsync<bool>(_deliveryClient, $"{_updateDriverDetails}{phoneNumber}", driverModel, HttpMethod.Patch);
        }

        /// <summary>
        /// Redispatches a driver for the specified order.
        /// </summary>
        public Task<HulubejeResponse<bool>> RedispatchDriversAsync(OrderDetail orderDetail)
        {
            return SendAsync<bool>(_client, _reDispatchDrivers, orderDetail);
        }


        /// <summary>
        /// Retrieves driver reviews for a given article with optional pagination.
        /// </summary>
        /// <param name="article">Article or driver identifier.</param>
        /// <param name="page">Page number for pagination.</param>
        /// <param name="retriveAllReviews">If true, retrieves all reviews ignoring pagination.</param>
        public async Task<HulubejeResponse<DriverReview>?> GetDriverReviewsAsync(string article, int page, bool retriveAllReviews = true)
        {
            // Create request payload
            var payload = new { article, retriveAllReviews };
            var json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(_getDriverReviews, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new HulubejeResponse<DriverReview>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { $"Error {response.StatusCode}: {responseBody}" }
                };
            }
            return JsonConvert.DeserializeObject<HulubejeResponse<DriverReview>>(responseBody);
        }

        public async Task<RouteModel> GetDriverRouteDetailAsync(string lat1, string lng1, string lat2, string lng2, string profile)
        {
            var query = $"?lat1={Uri.EscapeDataString(lat1)}&lng1={Uri.EscapeDataString(lng1)}" +
                        $"&lat2={Uri.EscapeDataString(lat2)}&lng2={Uri.EscapeDataString(lng2)}" +
                        $"&profile={Uri.EscapeDataString(profile)}";

            var response = await _deliveryClient.GetAsync( $"{_getRouteDetails}{query}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RouteModel>(data) ?? new RouteModel();
            }
            return new RouteModel();
        }

        public async Task<HulubejeResponse<Activities>?> GetDriverActivityAsync(string companyCode, string voucherCode, bool skipCache = true)
        {
            var cacheKey = $"driver_activity{companyCode}{voucherCode}";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _client.GetAsync($"{_getDriverActivityAsync}companyCode={companyCode}&voucherCode={voucherCode}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<HulubejeResponse<Activities>>(data) ?? new HulubejeResponse<Activities>();
                    }
                    return null;
                },
                skipCache,
                30
            );

        }
        #endregion

        #region Supervisors & Companies
        public async Task<List<SupervisorsDTO>> GetSupervisorsAsync()
        {
            var response = await _client.GetAsync(_getSupervisors);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<SupervisorsDTO>>(data) ?? new List<SupervisorsDTO>();
            }
            return new List<SupervisorsDTO>();
        }
        public Task<HulubejeResponse<bool>> AssignOrderSupervisorAsync(AssignSuperVisorDTO assignSupervisorDto)
            => SendAsync<bool>(_deliveryClient, _assignOrderSupervisor, assignSupervisorDto, HttpMethod.Patch);

        public async Task<Companies> GetCompaniesAsync()
        {
            var response = await _deliveryClient.GetAsync(_getCompanies);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Companies>(data) ?? new Companies();
            }
            return new Companies();
        }
        public async Task<HulubejeResponse<Company>?> GetCompanyDetailsAsync(string companyTin, bool skipCache = false)
        {
            if (string.IsNullOrWhiteSpace(companyTin))
                return null;

            string cacheKey = $"company_details_{companyTin}";

            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _client.GetAsync($"{_getCompany}?tin={companyTin}");

                    if (!response.IsSuccessStatusCode)
                        return null; // Don't cache failed requests

                    var data = await response.Content.ReadAsStringAsync();

                    return !string.IsNullOrWhiteSpace(data)
                        ? JsonConvert.DeserializeObject<HulubejeResponse<Company>>(data) ?? new HulubejeResponse<Company>()
                        : null;

                },
                skipCache,
                24 * 60 // cache for 24 hours (in minutes)
            );
        }
        public Task<HulubejeResponse<bool>> ChangeOrderBranchAsync(ChangeBranchDTO changeBranchDTO)
        {
            return SendAsync<bool>(_client, _changeorderbranch, changeBranchDTO);
        }
        #endregion

        #region Completed Orders
        public Task<HulubejeResponse<bool>> CompletePendingOrderAsync(OrderCompletionRequest request)
            => SendAsync<bool>(_client, _completePendingOrder, request);
        public async Task<HulubejeResponse<List<CompletedOrders>>> GetPendingOrdersAsync(bool skipCache = true)
        {
            string cacheKey = "pending_order";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _client.GetAsync(_getPendingOrders);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(data)
                               ?? new HulubejeResponse<List<CompletedOrders>>();
                    }
                    return new HulubejeResponse<List<CompletedOrders>>();
                },
                skipCache,
                10 // cache for 10 minutes
            );
        }
        public async Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(bool skipCache = true)
        {
            string cacheKey = "completed_orders";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _client.GetAsync(_getCompletedOrders);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(data)
                               ?? new HulubejeResponse<List<CompletedOrders>>();
                    }
                    return new HulubejeResponse<List<CompletedOrders>>();
                },
                skipCache,
                10 // cache for 10 minutes
            );
        }

        public async Task<HulubejeResponse<List<CompletedOrders>>> GetOrdersByTypeAsync(int type, bool skipCache = true)
        {
            string cacheKey = $"orders_by_type_{type}";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var response = await _client.GetAsync($"{_getCompletedOrdersByType}type={type}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(data)
                               ?? new HulubejeResponse<List<CompletedOrders>>();
                    }
                    return new HulubejeResponse<List<CompletedOrders>>();
                },
                skipCache,
                10
            );
        }
        public async Task<HulubejeResponse<LineItemsDetail>> Gethistorydetail(string voucherCode, string companyCode, int industryType = 1992, bool skipCache = true)
        {
            var cacheKey = $"history_{companyCode}_{voucherCode}_{industryType}";
            return await _cacheService.GetOrSetAsync(
                cacheKey, 
                async () =>
                {
                    var response = await _client.GetAsync($"{_getHistroyDetail}companyCode={companyCode}&voucherCode={voucherCode}&industryType={industryType}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<HulubejeResponse<LineItemsDetail>>(data) ?? new HulubejeResponse<LineItemsDetail>();
                    }
                    return new HulubejeResponse<LineItemsDetail>();
                },
                skipCache,
                10
            );
            
        }
        #endregion

        #region Device Control
        public async Task<List<DeviceControl>> GetDeviceControlAsync(string date)
        {
            var response = await _deviceControlClient.GetAsync($"{_getDeviceControl}?StartDate={date}&EndDate={date}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DeviceControl>>(data) ?? new List<DeviceControl>();
            }
            return new List<DeviceControl>();
        }
        #endregion

        #region Delivery Purpose and Note
        public async Task<string> GetDeliveryPurposeAsync()
        {
            var response = await _client.GetAsync("delivery/getpurpose");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null!;
        }

        public async Task<HulubejeResponse<bool>> SaveDeliveryNote(string voucherCode, string note, string purpose)
        {
            try
            {
                var url = $"{_saveDeliveryNote}voucherCode={Uri.EscapeDataString(voucherCode)}&note={Uri.EscapeDataString(note)}&purpose={Uri.EscapeDataString(purpose)}";
                var response = await _client.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new HulubejeResponse<bool>
                    {
                        IsSuccessful = false,
                        Data = false,
                        ErrorMessages = new List<string> { $"Error {response.StatusCode}: {responseBody}" }
                    };

                return new HulubejeResponse<bool> { IsSuccessful = true, Data = true };
            }
            catch (Exception ex)
            {
                return new HulubejeResponse<bool>
                {
                    IsSuccessful = false,
                    Data = false,
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
        }

        #endregion

        #region Message Sending & Activity Log
        public Task<HulubejeResponse<bool>> SendMessageAsync(AlertMessageDto messageDto)
            => SendAsync<bool>(_deliveryClient, _sendMessage, messageDto);

        public Task<HulubejeResponse<bool>> InsertActivityLogAsync(object request)
            => SendAsync<bool>(_client, _insertActivityLog, request);

        #endregion

        #region map
        public async Task<string> GetGoogleMapsJsAsync()
        {
            string apiUrl = $"https://maps.googleapis.com/maps/api/js?key={_googleMapsKey}&callback=initMap&libraries=places,geometry&v=weekly";

            using var client = _httpClientFactory.CreateClient();
            var result = await client.GetStringAsync(apiUrl);
            return result;
        }

        #endregion

        #region login and authentication
        public async Task<List<EntityModel>> GetFilteredConsigneesAsync(string tin)
        {
            var response = await _deliveryLoginClient.GetAsync($"{_getFilteredCustomers}Tin={tin}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<EntityModel>>(data)
                       ?? new List<EntityModel>();
            }
            return new List<EntityModel>();
        }
        public async Task<ResponseModel<LoginResponse>> AuthenticateUser(string phoneNumber, string password)
        {
            try
            {
                var _client = new HttpClient
                {
                    BaseAddress = new Uri(ApibaseAddress)
                };
                // Construct the query
                string queryString = $"userName={Uri.EscapeDataString(phoneNumber)}&password={Uri.EscapeDataString(password)}&tin={Uri.EscapeDataString(CompanyTin)}";
                string requestUrl = $"{_authenicateUser}{queryString}";

                var response = await _client.GetAsync(requestUrl); // _client already has BaseAddress configured
                string responseBody = await response.Content.ReadAsStringAsync();

                var userValidation = JsonConvert.DeserializeObject<ResponseModel<LoginResponse>>(responseBody);

                // Handle HTTP error or null response
                if (!response.IsSuccessStatusCode || userValidation == null)
                {
                    return new ResponseModel<LoginResponse>
                    {
                        Success = false,
                        Message = userValidation?.Message ?? "Empty or invalid response",
                        Data = new LoginResponse()
                    };
                }

                return userValidation;
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message
                return new ResponseModel<LoginResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = new LoginResponse()
                };
            }
        }

        public async Task<HulubejeResponse<bool>> UpdateSupervisorsOnlineStatusAsync(bool isOnline, string phoneNumber)
        {
            try
            {
                // Build URL for DeliveryLogin API
                string url = $"{_updateOnlineStatus}code={Uri.EscapeDataString(phoneNumber)}&online={isOnline}";

                var response = await _client.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new HulubejeResponse<bool>
                    {
                        IsSuccessful = false,
                        Data = false,
                        ErrorMessages = new List<string> { $"HTTP {response.StatusCode}: {responseContent}" }
                    };
                }

                // Deserialize boolean response
                bool isUpdated = JsonConvert.DeserializeObject<bool>(responseContent);

                return new HulubejeResponse<bool>
                {
                    IsSuccessful = true,
                    Data = isUpdated
                };
            }
            catch (Exception ex)
            {
                return new HulubejeResponse<bool>
                {
                    IsSuccessful = false,
                    Data = false,
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
        }

        public virtual async Task<UserDTO?> GetUserByUserName(string userName)
        {
            var _client = new HttpClient
            {
                BaseAddress = new Uri(ApibaseAddress)
            };

            var response = await _client.GetAsync($"{_getUserByUserName}userName={userName}");
            if (!response.IsSuccessStatusCode)
                return null;

            var juser = await response.Content.ReadAsStringAsync();
            var usernameUser = JsonConvert.DeserializeObject<List<UserDTO>>(juser);

            return usernameUser != null && usernameUser.Count > 0 ? usernameUser.FirstOrDefault() : null;

        }
        #endregion

        #region HTTP POST/PATCH Helper
        private static async Task<HulubejeResponse<T>> SendAsync<T>(
            HttpClient client,
            string endpoint,
            object payload,
            HttpMethod? method = null)
        {
            try
            {
                // Default to POST if not specified
                var httpMethod = method ?? HttpMethod.Post;

                var json = JsonConvert.SerializeObject(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(httpMethod, endpoint)
                {
                    Content = content
                };

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new HulubejeResponse<T>
                    {
                        IsSuccessful = false,
                        ErrorMessages = new List<string> { $"Error {response.StatusCode}: {responseBody}" }
                    };
                }

                // Try to parse JSON
                T? data = default;
                try
                {
                    data = JsonConvert.DeserializeObject<T>(responseBody);
                }
                catch
                {
                    // Ignore parsing error if not JSON
                }

                return new HulubejeResponse<T> { IsSuccessful = true, Data = data };
            }
            catch (Exception ex)
            {
                return new HulubejeResponse<T>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { $"Exception: {ex.Message}" }
                };
            }
        }
        #endregion

        private string? GetSecureCookie(string key)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request?.Cookies.TryGetValue(key, out var protectedValue) ?? false)
            {
                try { return _protector.Unprotect(protectedValue!); }
                catch { return null; }
            }
            return null;
        }

    }
}