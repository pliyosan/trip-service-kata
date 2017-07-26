using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TripServiceKata.Exception;
using TripServiceKata.Trip;
using TripServiceKata.User;

namespace TripServiceKata.Tests
{
    public class TestableTripService : TripService
    {
        readonly ISessionWrapper _sessionWrapper;
        private readonly ITripDataAccess _tripDataAccess;

        public TestableTripService(ISessionWrapper sessionWrapper, ITripDataAccess tripDataAccess)
        {
            _sessionWrapper = sessionWrapper;
            _tripDataAccess = tripDataAccess;
        }

        protected override User.User GetLoggedUser()
        {
            return _sessionWrapper.GetLoggedUser();
        }

        protected override List<Trip.Trip> FindTripsByUser(User.User user)
        {
            return _tripDataAccess.FindTripsByUser(user);
        }
    }

    public interface ITripDataAccess
    {
        List<Trip.Trip> FindTripsByUser(User.User user);
    }

    public interface ISessionWrapper
    {
        User.User GetLoggedUser();
    }

    public class SessionWrapper : ISessionWrapper
    {
        public User.User GetLoggedUser()
        {
            return UserSession.GetInstance().GetLoggedUser();
        }
    }

    public class TripDataAccess : ITripDataAccess
    {
        public List<Trip.Trip> FindTripsByUser(User.User user)
        {
            return TripDAO.FindTripsByUser(user);
        }
    }

    [TestFixture]
    public class TripServiceTest
    {
        private Mock<ISessionWrapper> _session;
        private Mock<ITripDataAccess> _tripDataAccess;
        private TestableTripService _testableTripService;

        [SetUp]
        public void Setup()
        {
            _session = new Mock<ISessionWrapper>();
            _tripDataAccess = new Mock<ITripDataAccess>();

            _testableTripService = new TestableTripService(_session.Object, _tripDataAccess.Object);
        }

        [Test]
        public void When_Calling_GetTripsByUser_It_BlowsUp_Trying_To_Get_TheLogged_User()
        {
            var tripService = new TestableTripService(new SessionWrapper(), _tripDataAccess.Object);

            Action getTripsByUserAction = () => tripService.GetTripsByUser(null);

            getTripsByUserAction.ShouldThrow<System.Exception>()
                .WithMessage("UserSession.GetLoggedUser() should not be called in an unit test");
        }

        [Test]
        public void An_exception_is_thrown_when_calling_the_FindTripsByUser()
        {
            var user = new User.User();
            var loggedUser = new User.User();
            user.AddFriend(loggedUser);

            _session.Setup(s => s.GetLoggedUser()).Returns(loggedUser);
            _tripDataAccess.Setup(t => t.FindTripsByUser(user))
                .Returns(new List<Trip.Trip>());

            var tripService = new TestableTripService(_session.Object, new TripDataAccess());

            Action getTripsByUserAction = () => tripService.GetTripsByUser(user);

            getTripsByUserAction.ShouldThrow<System.Exception>()
                .WithMessage("TripDAO should not be invoked on an unit test.");
        }

        [Test]
        public void An_exception_is_thrown_if_user_is_not_logged_in()
        {
            _session.Setup(s => s.GetLoggedUser()).Returns((User.User) null);

            Action getTripsByUserAction = () => _testableTripService.GetTripsByUser(null);

            getTripsByUserAction.ShouldThrow<UserNotLoggedInException>();
        }

        [Test]
        public void No_trips_are_found_if_user_passed_has_no_friends()
        {
            var user = new User.User();
            _session.Setup(s => s.GetLoggedUser()).Returns(user);

            var trips = _testableTripService.GetTripsByUser(user);

            trips.Should().BeEmpty();
        }

        [Test]
        public void No_trips_are_found_if_user_passed_is_not_a_friend_of_logged_in_user()
        {
            var user = new User.User();
            var aFriend = new User.User();
            user.AddFriend(aFriend);

            _session.Setup(s => s.GetLoggedUser()).Returns(user);

            var trips = _testableTripService.GetTripsByUser(user);

            trips.Should().BeEmpty();
        }

        [Test]
        public void No_trips_are_found_if_user_passed_is_a_friend_of_logged_in_user_but_has_no_trips()
        {
            var user = new User.User();
            var loggedUser = new User.User();
            user.AddFriend(loggedUser);

            _session.Setup(s => s.GetLoggedUser()).Returns(loggedUser);
            _tripDataAccess.Setup(t => t.FindTripsByUser(user))
                .Returns(new List<Trip.Trip>());

            var trips = _testableTripService.GetTripsByUser(user);

            trips.Should().BeEmpty();
        }

        [Test]
        public void Trips_are_found_if_user_passed_is_a_friend_of_logged_in_user_and_has_trips()
        {
            var user = new User.User();
            var loggedUser = new User.User();
            user.AddFriend(loggedUser);

            var expectedTrips = new List<Trip.Trip>
            {
                new Trip.Trip(),
                new Trip.Trip()
            };

            _session.Setup(s => s.GetLoggedUser()).Returns(loggedUser);
            _tripDataAccess.Setup(t => t.FindTripsByUser(user))
                .Returns(expectedTrips);

            var trips = _testableTripService.GetTripsByUser(user);

            trips.Should().BeEquivalentTo(expectedTrips);
        }

        //[Test]
        //public void Friend_DoesntHave_Trips_So_It_BlowsUp()
        //{
        //    var fakeSessionWrapper = new SessionWrapper();
        //    var tripService = new TestableTripService(fakeSessionWrapper);

        //    Action getTripsByUserAction = () => tripService.GetTripsByUser(null);

        //    getTripsByUserAction.ShouldThrow<System.Exception>()
        //        .WithMessage("UserSession.GetLoggedUser() should not be called in an unit test");
        //}
    }
}
