using System.Collections.Generic;
using TripServiceKata.Exception;
using TripServiceKata.User;

namespace TripServiceKata.Trip
{
    public class TripService
    {
        public List<Trip> GetTripsByUser(User.User user)
        {
            var tripList = new List<Trip>();
            var loggedUser = GetLoggedUser();
            if (loggedUser == null)
            {
                throw new UserNotLoggedInException();
            }

            var isFriend = IsLoggedUserFriend(user, loggedUser);
            if (isFriend)
            {
                tripList = FindTripsByUser(user);
            }

            return tripList;
        }

        private static bool IsLoggedUserFriend(User.User user, User.User loggedUser)
        {
            var isFriend = false;

            foreach (User.User friend in user.GetFriends())
            {
                if (friend.Equals(loggedUser))
                {
                    isFriend = true;
                    break;
                }
            }

            return isFriend;
        }

        protected virtual List<Trip> FindTripsByUser(User.User user)
        {
            return TripDAO.FindTripsByUser(user);
        }

        protected virtual User.User GetLoggedUser()
        {
            return UserSession.GetInstance().GetLoggedUser();
        }
    }
}
