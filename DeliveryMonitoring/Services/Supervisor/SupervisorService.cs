using CNET_V7_Domain.Domain.SecuritySchema;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;

namespace DeliveryMonitoring.Services.Supervisor
{
    public interface ISupervisorService
    {
        Task<HulubejeResponse<SupervisorsDTO>> CheckIfUserIsSupervisorAsync(UserDTO user);
    }

    public class SupervisorService : ISupervisorService
    {
        private readonly IApiRequestService _api;

        public SupervisorService(IApiRequestService api)
        {
            _api = api;
        }
        public async Task<HulubejeResponse<SupervisorsDTO>> CheckIfUserIsSupervisorAsync(UserDTO user)
        {
            var response = new HulubejeResponse<SupervisorsDTO>
            {
                ErrorMessages = new List<string>()
            };

            if (user == null)
            {
                response.IsSuccessful = false;
                response.ErrorMessages.Add("User information is required.");
                return response;
            }

            var supervisors = await _api.GetSupervisorsAsync();

            if (supervisors == null || supervisors.Count == 0)
            {
                response.IsSuccessful = false;
                response.ErrorMessages.Add("No supervisors available.");
                return response;
            }

            var supervisor = supervisors.FirstOrDefault(s => s.UserName == user.UserName);

            if (supervisor == null)
            {
                response.IsSuccessful = false;
                response.ErrorMessages.Add($"Supervisor not found for user '{user.UserName}'.");
                return response;
            }

            response.IsSuccessful = true;
            response.Data = supervisor;
            return response;
        }
    }
}
