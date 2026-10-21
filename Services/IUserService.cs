using ProductApi.DTOs;

namespace ProductApi.Services;

public interface IUserService
{
    List<UserResponse> GetAll();
    List<AgentOption> GetAgents();                      // assignment dropdown වලට
    UserResponse Create(CreateUserRequest request);     // admin account creation
    UserResponse ChangeRole(int id, string role);
    UserResponse SetActive(int id, bool isActive);
}
