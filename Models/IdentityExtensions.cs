using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace CarMessenger.Models
{
    public static class IdentityExtensions
    {
        public static string GetNickname(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Nickname");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
        //public static int GetMaxOwners(this IIdentity identity)
        //{
        //    var claim = ((ClaimsIdentity)identity).FindFirst("Nickname");
        //    // Test for null to avoid issues during local testing
        //    return (claim != null) ? Int32.Parse(claim.Value) : 0;
        //}
        //public static int GetCoMaxOwners(this IIdentity identity)
        //{
        //    var claim = ((ClaimsIdentity)identity).FindFirst("Nickname");
        //    // Test for null to avoid issues during local testing
        //    return (claim != null) ? Int32.Parse(claim.Value) : 0;
        //}
        public static void AddUpdateClaim(this IPrincipal currentPrincipal, string key, string value)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return;

            // check for existing claim and remove it
            var existingClaims = identity.FindAll(key);
            if (existingClaims != null)
                foreach(var existingClaim in existingClaims)
                    identity.RemoveClaim(existingClaim);

            // add new claim
            identity.AddClaim(new Claim(key, value));
        }

        public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
            return claim.Value;
        }
    }
}