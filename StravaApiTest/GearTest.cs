using System;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coomes.Equipper.StravaApi.Test
{
    [TestClass]
    public class GearTest
    {
        [TestMethod]
        public void ConvertsToDomainModel()
        {
            // given
            var gearJson = @"
{
    ""id"": ""b1234567"",
    ""primary"": false,
    ""name"": ""RIP Giordano Libero"",
    ""nickname"": ""RIP Gio Lib"",
    ""resource_state"": 3,
    ""retired"": true,
    ""distance"": 2219169,
    ""converted_distance"": 1378.9,
    ""brand_name"": ""Giordano "",
    ""model_name"": ""Libero 1.6"",
    ""frame_type"": 3,
    ""description"": """",
    ""weight"": 26.0
}";
            var jsonBytes = Encoding.UTF8.GetBytes(gearJson);
            
            // when
            var apiModelGear = StravaApi.Models.Gear.FromJsonBytes(jsonBytes);
            var domainModelGear = apiModelGear.ToDomainModel();

            // then
            domainModelGear.Id.Should().Be("b1234567");
            domainModelGear.Name.Should().Be("RIP Giordano Libero");
            domainModelGear.Retired.Should().Be(true);
        }
    }
}