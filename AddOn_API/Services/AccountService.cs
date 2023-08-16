using System.Security.Claims;
using System.Reflection.PortableExecutable;
using AddOn_API.Data;
using AddOn_API.DTOs.Account;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using static AddOn_API.Installers.JWTInstaller;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace AddOn_API.Services
{
    public class AccountService : IAccountService
    {
        private readonly DatabaseContext databaseContext;
        private readonly JwtSettings jwtSettings;
        public AccountService(DatabaseContext databaseContext, JwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;
            this.databaseContext = databaseContext;
        }

        public async Task<IEnumerable<VwWebUser>> FindAll()
        {
            return await databaseContext.VwWebUsers.ToListAsync();
        }

        public string GenerateToken(VwWebUser account)
        {
            var claims = new[]{
                new Claim(JwtRegisteredClaimNames.Sub, account.EmpUsername),
                new Claim("EmpName", account.EmpName),
                new Claim("EmpLname", account.EmpLname),
                new Claim("EmpPosition", account.EmpPosition),
                new Claim("EmpSection", account.EmpSection),
                new Claim("EmpDepartment", account.EmpDepartment),
                new Claim("EmpEmail", account.EmpEmail),
                new Claim("Site", account.Site),
                new Claim("Role", account.EmpDepartment),
               
            };

            return BuildToken(claims);
        }

        private string BuildToken(Claim[] claims){
            var expire = DateTime.Now.AddDays(Convert.ToDouble(jwtSettings.Expire));
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SignKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: expire,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        
        }

         VwWebUser IAccountService.GetInfo(string accessToken)
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var username = token.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            var name = token.Claims.First(claim => claim.Type == "EmpName").Value;
            var lName = token.Claims.First(claim => claim.Type == "EmpLname").Value;
            var position = token.Claims.First(claim => claim.Type == "EmpPosition").Value;
            var section = token.Claims.First(claim => claim.Type == "EmpSection").Value;
            var department = token.Claims.First(claim => claim.Type == "EmpDepartment").Value;
            var email = token.Claims.First(claim => claim.Type == "EmpEmail").Value;
            var site = token.Claims.First(claim => claim.Type == "Site").Value;
            var Role = token.Claims.First(claim => claim.Type == "Role").Value;
          
            var _VwWebUser = new VwWebUser{
                EmpCode = "",
                EmpUsername = username,
                EmpName = name,
                EmpLname = lName,
                EmpNameTh = "",
                EmpSex = "",
                EmpPosition = position,
                EmpSection = section,
                EmpDepartment = department,
                EmpEmail = email,
                Site = site

            
            };

            return _VwWebUser;
        }

        public Task<PlRole> GetRole(int RoleId)
        {
            throw new NotImplementedException();
        }

        public async Task<VwWebUser> Login(LoginRequest loginRequest)
        {
            bool checkauthLDAP = checkPermissionAD(loginRequest);
            var _result = new VwWebUser();

            if (checkauthLDAP == true)
            {
                _result = await databaseContext.VwWebUsers.SingleOrDefaultAsync(b => b.EmpUsername == loginRequest.EmpUsername);

                if (_result != null)
                    return _result;
                else
                    return null;
            }
            else
            {
                return null;
            }

        }
        private bool checkPermissionAD(LoginRequest loginRequest)
        {
            bool check = false;
            try
            {
                System.DirectoryServices.DirectoryEntry directoryEntry = new System.DirectoryServices.DirectoryEntry("LDAP://Rofuth.Local", loginRequest.EmpUsername, loginRequest.EmpPassword, System.DirectoryServices.AuthenticationTypes.Secure);
                System.DirectoryServices.DirectorySearcher directorySearcher = new System.DirectoryServices.DirectorySearcher(directoryEntry);
                System.DirectoryServices.SearchResult searchResult = directorySearcher.FindOne();

                if (searchResult == null)
                    check = false;
                else
                    check = true;
            }
            catch (Exception ex)
            {
                var erroetext = ex.ToString();

                check = false;
            }
            return check;
        }

        public async Task<string> CheckPermissionAccess(string? accessToken,string menu)
        {
            string check = "";
            if (accessToken == null)
            {
                check =  "Null";
            }

            return check;
        }
    }
}