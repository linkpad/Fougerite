﻿namespace RustPP.Commands
{
    using Fougerite;
    using RustPP;
    using RustPP.Permissions;
    using System;
    using System.IO;
    using System.Collections.Generic;

    public class ReloadCommand : ChatCommand
    {
        public override void Execute(ConsoleSystem.Arg Arguments, string[] ChatArguments)
        {           
            TimedEvents.startEvents();
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("admins.xml")))
            {
                Administrator.AdminList = Helper.ObjectFromXML<List<Administrator>>(RustPPModule.GetAbsoluteFilePath("admins.xml"));
            }
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("whitelist.xml")))
            {
                Core.whiteList = new PList(Helper.ObjectFromXML<List<PList.Player>>(RustPPModule.GetAbsoluteFilePath("whitelist.xml")));
            }
            else
            {
                Core.whiteList = new PList();
            }
            if (File.Exists(RustPPModule.GetAbsoluteFilePath("bans.xml")))
            {
                Core.blackList = new PList(Helper.ObjectFromXML<List<PList.Player>>(RustPPModule.GetAbsoluteFilePath("bans.xml")));
            }
            else
            {
                Core.blackList = new PList();
            }
        }
    }
}