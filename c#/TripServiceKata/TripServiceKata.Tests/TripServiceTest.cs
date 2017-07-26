using System;
using FluentAssertions;
using NUnit.Framework;
using TripServiceKata.Trip;

namespace TripServiceKata.Tests
{
    public class TestableTripService : TripService
    {
        private readonly SessionWrapper _sessionWrapper;

        public TestableTripService(SessionWrapper sessionWrapper)
        {
            _sessionWrapper = sessionWrapper;
        }
        protected override User.User GetLoggedUser()
        {
            return _sessionWrapper.GetLoggedUser();
        }
    }

    public class SessionWrapper
    {
        public User.User GetLoggedUser()
        {
            return new User.User();
        }
    }


    [TestFixture]
    public class TripServiceTest
    {
        [Test]
        public void When_Calling_GetTripsByUser_It_BlowsUp_Trying_To_Get_TheLogged_User()
        {
            var tripService = new TripService();

            Action getTripsByUserAction = () => tripService.GetTripsByUser(null);

            getTripsByUserAction.ShouldThrow<System.Exception>()
                .WithMessage("UserSession.GetLoggedUser() should not be called in an unit test");
        }

        [Test]
        public void Friend_DoesntHave_Trips_So_It_BlowsUp()
        {
            var fakeSessionWrapper = new SessionWrapper();
            var tripService = new TestableTripService(fakeSessionWrapper);

            Action getTripsByUserAction = () => tripService.GetTripsByUser(null);

            getTripsByUserAction.ShouldThrow<System.Exception>()
                .WithMessage("UserSession.GetLoggedUser() should not be called in an unit test");
        }
    }
}
