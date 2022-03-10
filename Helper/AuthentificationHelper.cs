using System.Security.Claims;

namespace B3C3GRP6.Helper
{
    public class AuthentificationHelper
    {
        public static string GetIdInClaims(ClaimsPrincipal user)
        {
            //return user.Claims.FirstOrDefault(x => x.Type.Equals("Id", StringComparison.InvariantCultureIgnoreCase)).Value;
            return user.Claims.First(x => x.Type == "Id").Value;
        }
        
    }
}
