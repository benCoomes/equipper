using System;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coomes.Equipper.StravaApi.Test
{
    [TestClass]
    public class StravaEventTest
    {
        [TestMethod]
        public void StravaEvent_DeserializesFromJson_ForUpdateActivity()
        {
            // given
            var json = @"
{
    ""aspect_type"": ""update"",
    ""event_time"": 1516126040,
    ""object_id"": 1360128428,
    ""object_type"": ""activity"",
    ""owner_id"": 134815,
    ""subscription_id"": 120475,
    ""updates"": {
        ""title"": ""Messy""
    }
}";
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            
            // when
            var stravaEvent = StravaApi.Models.StravaEvent.FromJsonBytes(jsonBytes);

            // then
            stravaEvent.aspect_type.Should().Be("update");
            stravaEvent.event_time.Should().Be(1516126040);
            stravaEvent.object_id.Should().Be(1360128428);
            stravaEvent.object_type.Should().Be("activity");
            stravaEvent.owner_id.Should().Be(134815);
            stravaEvent.subscription_id.Should().Be(120475);
            stravaEvent.updates.Should().ContainKey("title").WhoseValue.Should().Be("Messy");
        }
    }
}
