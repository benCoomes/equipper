using System;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coomes.Equipper.StravaApi.Test
{
    [TestClass]
    public class AthleteTokensTest
    {
        [TestMethod]
        public void ConvertsToDomainModel()
        {
            // given
            var json = @"
{
  ""token_type"": ""Bearer"",
  ""expires_at"": 1568775134,
  ""expires_in"": 21600,
  ""refresh_token"": ""refreshTokenValue"",
  ""access_token"": ""accessTokenValue"",
  ""athlete"": {
    ""id"": 1234,
    ""firstname"": ""foo"",
    ""lastname"": ""bar""
  }
}";
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var stravaAthleteToken = StravaApi.Models.AthleteTokens.FromJsonBytes(jsonBytes);
            
            // when
            var domainAthleteToken = stravaAthleteToken.ToDomainModel();

            // then
            domainAthleteToken.AccessToken.Should().Be("accessTokenValue");
            domainAthleteToken.RefreshToken.Should().Be("refreshTokenValue");
            domainAthleteToken.ExpiresAtUtc.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1568775134).UtcDateTime);
            domainAthleteToken.AthleteID.Should().Be(1234);
        }
    }
}
