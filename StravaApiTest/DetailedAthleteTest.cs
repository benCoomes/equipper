using System;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coomes.Equipper.StravaApi.Test
{
    [TestClass]
    public class DetailedAthleteTest
    {
        [TestMethod]
        public void ConvertsToDomainModelWithAthleteId()
        {
            var detailedAthleteJson = @"
{
  ""id"" : 1234567890987654321,
  ""username"" : ""marianne_t"",
  ""resource_state"" : 3,
  ""firstname"" : ""Marianne"",
  ""lastname"" : ""Teutenberg"",
  ""city"" : ""San Francisco"",
  ""state"" : ""CA"",
  ""country"" : ""US"",
  ""sex"" : ""F"",
  ""premium"" : true,
  ""created_at"" : ""2017-11-14T02:30:05Z"",
  ""updated_at"" : ""2018-02-06T19:32:20Z"",
  ""badge_type_id"" : 4,
  ""profile_medium"" : ""https://xxxxxx.cloudfront.net/pictures/athletes/123456789/123456789/2/medium.jpg"",
  ""profile"" : ""https://xxxxx.cloudfront.net/pictures/athletes/123456789/123456789/2/large.jpg"",
  ""friend"" : null,
  ""follower"" : null,
  ""follower_count"" : 5,
  ""friend_count"" : 5,
  ""mutual_friend_count"" : 0,
  ""athlete_type"" : 1,
  ""date_preference"" : ""%m/%d/%Y"",
  ""measurement_preference"" : ""feet"",
  ""clubs"" : [ ],
  ""ftp"" : null,
  ""weight"" : 0,
  ""bikes"" : [ {
    ""id"" : ""b12345678987655"",
    ""primary"" : true,
    ""name"" : ""EMC"",
    ""resource_state"" : 2,
    ""distance"" : 0
  } ],
  ""shoes"" : [ {
    ""id"" : ""g12345678987655"",
    ""primary"" : true,
    ""name"" : ""adidas"",
    ""resource_state"" : 2,
    ""distance"" : 4904
  } ]
}";
            var jsonBytes = Encoding.UTF8.GetBytes(detailedAthleteJson);
            
            // when
            var apiModelAthlete = StravaApi.Models.DetailedAthlete.FromJsonBytes(jsonBytes);
            var domainModelAthlete = apiModelAthlete.ToDomainModel();

            // then
            domainModelAthlete.Should().BeEquivalentTo(new Athlete(){
              Id = 1234567890987654321,
              FirstName = "Marianne",
              LastName = "Teutenberg",
              ProfileMediumUrl = "https://xxxxxx.cloudfront.net/pictures/athletes/123456789/123456789/2/medium.jpg",
              ProfileUrl = "https://xxxxx.cloudfront.net/pictures/athletes/123456789/123456789/2/large.jpg"
            });
        }
    }
}