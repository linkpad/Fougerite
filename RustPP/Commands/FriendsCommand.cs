﻿namespace RustPP.Commands
{
    using Fougerite;
    using RustPP.Social;
    using System;
    using System.Collections;

    public class FriendsCommand : ChatCommand
    {
        public static Hashtable friendsLists = new Hashtable();

        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {
            if (!friendsLists.ContainsKey(Arguments.argUser.userID))
            {
                Util.sayUser(Arguments.argUser.networkPlayer, "You currently have no friend.");
            }
            else
            {
                ((FriendList)friendsLists[Arguments.argUser.userID]).OutputList(ref Arguments);
            }
        }

        public Hashtable GetFriendsLists()
        {
            return friendsLists;
        }

        public void SetFriendsLists(Hashtable fl)
        {
            friendsLists = fl;
        }
    }
}