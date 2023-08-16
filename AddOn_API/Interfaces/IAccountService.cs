using AddOn_API.DTOs.Account;
using AddOn_API.Entities;

namespace AddOn_API.Interfaces
{
    public interface IAccountService
    {
         Task <IEnumerable<VwWebUser>> FindAll ();
        
         Task <VwWebUser> Login(LoginRequest loginRequest);
         string GenerateToken(VwWebUser account);
         VwWebUser GetInfo(string accessToken);
         Task <PlRole> GetRole(int RoleId);

         Task <String> CheckPermissionAccess(string? accessToken,string menu);

         
    }
}