using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;
using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class GetConnectedAthleteTest
    {
      [TestMethod]
      public async Task GetsAssociatedAthlete()
      {
          // given
          var user = new LoggedInUser("userId", "user name");
          var tokens = new AthleteTokens 
          { 
            AccessToken = "valid_access_token",
            AthleteID = 123456,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
          };
          var expectedAthlete = new Athlete 
          {
            Id = tokens.AthleteID,
            FirstName = "Super",
            LastName = "Fast",
          };

          var tokenStorageMock = new Mock<ITokenStorage>();
          tokenStorageMock
            .Setup(ts => ts.GetTokenForUser(user.UserId))
            .ReturnsAsync(tokens);
          
          var athleteClientMock = new Mock<IAthleteClient>();
          athleteClientMock
            .Setup(ac => ac.GetAthlete(tokens.AccessToken, tokens.AthleteID))
            .ReturnsAsync(expectedAthlete);

          var sut = new GetConnectedAthlete(
            athleteClientMock.Object, 
            tokenStorageMock.Object,
            Mock.Of<ITokenProvider>(),
            Mock.Of<ILogger>());

          // when
          var actualAthlete = await sut.Execute(user);

          // then
          actualAthlete.Id.Should().Be(expectedAthlete.Id);
          actualAthlete.FirstName.Should().Be(expectedAthlete.FirstName);
          actualAthlete.LastName.Should().Be(expectedAthlete.LastName);
      }

      [TestMethod]
      public async Task ReturnsNullIfNoAssociatedAthlete()
      {
          // given
          var user = new LoggedInUser("userId", "user name");
          
          // no Strava account means no athlete tokens
          var tokenStorageMock = new Mock<ITokenStorage>();
          tokenStorageMock
            .Setup(ts => ts.GetTokenForUser(user.UserId))
            .ReturnsAsync(default(AthleteTokens));
          
          var sut = new GetConnectedAthlete(
            Mock.Of<IAthleteClient>(), 
            tokenStorageMock.Object,
            Mock.Of<ITokenProvider>(),
            Mock.Of<ILogger>());

          // when
          var actualAthlete = await sut.Execute(user);

          // then
          actualAthlete.Should().BeNull();
      }

      [TestMethod]
      public async Task ThrowsNotAuthorized_ForAnonymousUser() 
      {
        // given
        var anonUser = new AnonymousUser();
        var sut = new GetConnectedAthlete(null, null, null, null);

        // when 
        Func<Task> tryAnonUser = () => sut.Execute(anonUser);
        Func<Task> tryNullUser = () => sut.Execute(null);

        // then 
        await tryAnonUser.Should().ThrowAsync<UnauthorizedException>();
        await tryNullUser.Should().ThrowAsync<UnauthorizedException>();
      }

      // todo - what if tokens exist but are no good - user has revoked equipper access? In this case
      // the use _should_ be removed, but its always possible deletion failed.
    }
}