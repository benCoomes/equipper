using System;

namespace Equipper.FunctionApp
{
    internal static class Settings
    {
         public static string ClientId => 
            Environment.GetEnvironmentVariable("StravaApi__ClientId");
        public static string ClientSecret =>
            Environment.GetEnvironmentVariable("StravaApi__ClientSecret");
            
        public static string SubscriptionVerificationToken => 
            Environment.GetEnvironmentVariable("FunctionApp__VerificationToken");
    
        public static string EventCallbackUri => 
            Environment.GetEnvironmentVariable("FunctionApp__SubscriptionCallback");
        
        public static string CosmosConnectionString => 
            Environment.GetEnvironmentVariable("CosmosStorage__ConnectionString");
    }
}