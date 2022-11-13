using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Coomes.Equipper;

// this class derived from https://learn.microsoft.com/en-us/azure/static-web-apps/user-information?tabs=csharp#api-functions
public static class StaticWebAppsAuth
{
  private class ClientPrincipal
  {
      public string IdentityProvider { get; set; }
      public string UserId { get; set; }
      public string UserDetails { get; set; }
      public IEnumerable<string> UserRoles { get; set; }
  }

  public static EquipperUser ParseUser(HttpRequest req)
  {
      var principal = new ClientPrincipal();

      if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
      {
          var data = header[0];
          var decoded = Convert.FromBase64String(data);
          var json = Encoding.UTF8.GetString(decoded);
          principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
      }

      principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

      if (!principal.UserRoles?.Any() ?? true)
      {
          return new AnonymousUser();
      }
      return new LoggedInUser(principal.UserId, principal.UserDetails);
  }
}