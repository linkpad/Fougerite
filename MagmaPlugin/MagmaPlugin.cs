﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Timers;

namespace MagmaPlugin
{
    using Fougerite;
    using Fougerite.Events;
    using Jint;
    using Jint.Expressions;

    public class Plugin
    {
        public readonly JintEngine Engine;
        public readonly string Name;
        public readonly string Code;
        public readonly DirectoryInfo RootDirectory;
        public readonly Dictionary<String, TimedEvent> Timers;

        public Plugin(DirectoryInfo directory, string name, string code)
        {
            Name = name;
            Code = code;
            RootDirectory = directory;
            Timers = new Dictionary<String, TimedEvent>();

            Engine = new JintEngine();
            Engine.AllowClr(true);

            InitGlobals();
            Engine.Run(code);
            try
            {
                Engine.CallFunction("On_PluginInit");
            }
            catch { }
        }

        public void InitGlobals()
        {
            Engine.SetParameter("Server", Fougerite.Server.GetServer());
            Engine.SetParameter("Data", Magma.Data.GetData());
            Engine.SetParameter("DataStore", Fougerite.DataStore.GetInstance());
            Engine.SetParameter("Util", Fougerite.Util.GetUtil());
            Engine.SetParameter("Web", new Fougerite.Web());
            Engine.SetParameter("Time", this);
            Engine.SetParameter("World", Fougerite.World.GetWorld());
            Engine.SetParameter("Plugin", this);
        }

        private void Invoke(string func, params object[] obj)
        {
            try
            {
                Engine.CallFunction(func, obj);
            }
            catch (Exception ex)
            {
                Logger.LogError("[MagmaPlugin] Error invoking function " + func + " in " + Name + " plugin.");
                Logger.LogException(ex);
            }
        }

        public IEnumerable<FunctionDeclarationStatement> GetSourceCodeGlobalFunctions()
        {
            foreach (Statement statement in JintEngine.Compile(Code, false).Statements)
            {
                if (statement.GetType() == typeof(FunctionDeclarationStatement))
                {
                    FunctionDeclarationStatement funcDecl = (FunctionDeclarationStatement)statement;
                    if (funcDecl != null)
                    {
                        yield return funcDecl;
                    }
                }
            }
        }

        public void InstallHooks()
        {
            foreach (var funcDecl in GetSourceCodeGlobalFunctions())
            {
                Logger.LogDebug("[MagmaPlugin] Found Function: " + funcDecl.Name);
                switch (funcDecl.Name)
                {
                    case "On_ServerInit": Hooks.OnServerInit += OnServerInit; break;
                    case "On_PluginInit": Hooks.OnPluginInit += OnPluginInit; break;
                    case "On_ServerShutdown": Hooks.OnServerShutdown += OnServerShutdown; break;
                    case "On_ItemsLoaded": Hooks.OnItemsLoaded += OnItemsLoaded; break;
                    case "On_TablesLoaded": Hooks.OnTablesLoaded += OnTablesLoaded; break;
                    case "On_Chat": Hooks.OnChat += OnChat; break;
                    case "On_Console": Hooks.OnConsoleReceived += OnConsole; break;
                    case "On_Command": Hooks.OnCommand += OnCommand; break;
                    case "On_PlayerConnected": Hooks.OnPlayerConnected += OnPlayerConnected; break;
                    case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected += OnPlayerDisconnected; break;
                    case "On_PlayerKilled": Hooks.OnPlayerKilled += OnPlayerKilled; break;
                    case "On_PlayerHurt": Hooks.OnPlayerHurt += OnPlayerHurt; break;
                    case "On_PlayerSpawning": Hooks.OnPlayerSpawning += OnPlayerSpawn; break;
                    case "On_PlayerSpawned": Hooks.OnPlayerSpawned += OnPlayerSpawned; break;
                    case "On_PlayerGathering": Hooks.OnPlayerGathering += OnPlayerGathering; break;
                    case "On_EntityHurt": Hooks.OnEntityHurt += OnEntityHurt; break;
                    case "On_EntityDecay": Hooks.OnEntityDecay += OnEntityDecay; break;
                    case "On_EntityDeployed": Hooks.OnEntityDeployed += OnEntityDeployed; break;
                    case "On_NPCHurt": Hooks.OnNPCHurt += OnNPCHurt; break;
                    case "On_NPCKilled": Hooks.OnNPCKilled += OnNPCKilled; break;
                    case "On_BlueprintUse": Hooks.OnBlueprintUse += OnBlueprintUse; break;
                    case "On_DoorUse": Hooks.OnDoorUse += OnDoorUse; break;
                }
            }
        }

        public void RemoveHooks()
        {
            foreach (var funcDecl in GetSourceCodeGlobalFunctions())
            {
                Logger.LogDebug("[MagmaPlugin] RemoveHooks, found function " + funcDecl.Name);
                switch (funcDecl.Name)
                {
                    case "On_ServerInit": Hooks.OnServerInit -= OnServerInit; break;
                    case "On_PluginInit": Hooks.OnPluginInit -= OnPluginInit; break;
                    case "On_ServerShutdown": Hooks.OnServerShutdown -= OnServerShutdown; break;
                    case "On_ItemsLoaded": Hooks.OnItemsLoaded -= OnItemsLoaded; break;
                    case "On_TablesLoaded": Hooks.OnTablesLoaded -= OnTablesLoaded; break;
                    case "On_Chat": Hooks.OnChat -= OnChat; break;
                    case "On_Console": Hooks.OnConsoleReceived -= OnConsole; break;
                    case "On_Command": Hooks.OnCommand -= OnCommand; break;
                    case "On_PlayerConnected": Hooks.OnPlayerConnected -= OnPlayerConnected; break;
                    case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected -= OnPlayerDisconnected; break;
                    case "On_PlayerKilled": Hooks.OnPlayerKilled -= OnPlayerKilled; break;
                    case "On_PlayerHurt": Hooks.OnPlayerHurt -= OnPlayerHurt; break;
                    case "On_PlayerSpawning": Hooks.OnPlayerSpawning -= OnPlayerSpawn; break;
                    case "On_PlayerSpawned": Hooks.OnPlayerSpawned -= OnPlayerSpawned; break;
                    case "On_PlayerGathering": Hooks.OnPlayerGathering -= OnPlayerGathering; break;
                    case "On_EntityHurt": Hooks.OnEntityHurt -= OnEntityHurt; break;
                    case "On_EntityDecay": Hooks.OnEntityDecay -= OnEntityDecay; break;
                    case "On_EntityDeployed": Hooks.OnEntityDeployed -= OnEntityDeployed; break;
                    case "On_NPCHurt": Hooks.OnNPCHurt -= OnNPCHurt; break;
                    case "On_NPCKilled": Hooks.OnNPCKilled -= OnNPCKilled; break;
                    case "On_BlueprintUse": Hooks.OnBlueprintUse -= OnBlueprintUse; break;
                    case "On_DoorUse": Hooks.OnDoorUse -= OnDoorUse; break;
                }
            }
        }

        #region File operations.

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private String ValidateRelativePath(String path)
        {
            String normalizedPath = NormalizePath(Path.Combine(RootDirectory.FullName, path));
            String rootDirNormalizedPath = NormalizePath(RootDirectory.FullName);

            if (!normalizedPath.StartsWith(rootDirNormalizedPath))
                return null;

            return normalizedPath;
        }

        public bool CreateDir(string path)
        {
            try
            {
                path = ValidateRelativePath(path);

                if (path == null)
                    return false;

                if (Directory.Exists(path))
                    return false;

                Directory.CreateDirectory(path);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return false;
        }

        public IniParser GetIni(string path)
        {
            path = ValidateRelativePath(path + ".ini");

            if (path == null)
                return null;
            
            if (File.Exists(path))
                return new IniParser(path);

            return null;
        }

        public bool IniExists(string path)
        {
            path = ValidateRelativePath(path + ".ini");

            if (path == null)
                return false;

            return File.Exists(path);
        }

        public IniParser CreateIni(string path)
        {
            try
            {
                path = ValidateRelativePath(path + ".ini");

                File.WriteAllText(path, "");
                return new IniParser(path);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return null;
        }

        public List<IniParser> GetInis(string path)
        {
            path = ValidateRelativePath(path);

            if (path == null)
                return new List<IniParser>();

            return Directory.GetFiles(path).Select(p => new IniParser(p)).ToList();
        }

        public void DeleteLog(string path)
        {
            path = ValidateRelativePath(path + ".ini");

            if (path == null)
                return;

            if (File.Exists(path))
                File.Delete(path);
        }

        public void Log(string path, string text)
        {
            path = ValidateRelativePath(path + ".ini");

            if (path == null)
                return;

            File.AppendAllText(path, "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text + "\r\n");
        }

        #endregion

        #region Timer functions.

        public TimedEvent GetTimer(string name)
        {
            if (Timers.ContainsKey(name))
                return Timers[name];
            return null;
        }

        public TimedEvent CreateTimer(string name, int timeoutDelay)
        {
            TimedEvent timer = this.GetTimer(name);
            if (timer == null)
            {
                timer = new TimedEvent(name, (double) timeoutDelay);
                timer.OnFire += OnTimerCB;
                Timers[name] = timer;
                return timer;
            }
            return timer;
        }

        public TimedEvent CreateTimer(string name, int timeoutDelay, object[] args)
        {
            TimedEvent timer = CreateTimer(name, timeoutDelay);
            timer.Args = args;
            timer.OnFire -= OnTimerCB;
            timer.OnFireArgs += OnTimerCBArgs;
            return timer;
        }

        public void KillTimer(string name)
        {
            TimedEvent timer = GetTimer(name);
            if (timer != null)
            {
                timer.Stop();
                Timers.Remove(name);
            }
        }

        public void KillTimers()
        {
            foreach (var timer in Timers.Values)
                timer.Stop();
            Timers.Clear();
        }

        #endregion Timer functions.

        #region Other functions.

        public string GetDate()
        {
            return DateTime.Now.ToShortDateString();
        }

        public int GetTicks()
        {
            return Environment.TickCount;
        }

        public string GetTime()
        {
            return DateTime.Now.ToShortTimeString();
        }

        public long GetTimestamp()
        {
            TimeSpan span = (TimeSpan)(DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0));
            return (long)span.TotalSeconds;
        }

        #endregion

        #region Hooks

        public void OnBlueprintUse(Player player, BPUseEvent evt)
        {
            Invoke("On_BlueprintUse", player, evt);
        }

        public void OnChat(Player player, ref ChatString text)
        {
            Invoke("On_Chat", player, text);
        }

        public void OnCommand(Player player, string command, string[] args)
        {
            Invoke("On_Command", player, command, args);
        }

        public void OnConsole(ref ConsoleSystem.Arg arg, bool external)
        {
            Player player = Player.FindByPlayerClient(arg.argUser.playerClient);

            if (!external)
                Invoke("On_Console", player, arg);
            else
                Invoke("On_Console", null, arg);
        }

        public void OnDoorUse(Player player, DoorEvent evt)
        {
            Invoke("On_DoorUse", player, evt);
        }

        public void OnEntityDecay(DecayEvent evt)
        {
            Invoke("On_EntityDecay", evt);
        }

        public void OnEntityDeployed(Player player, Entity entity)
        {
            Invoke("On_EntityDeployed", player, entity);
        }

        public void OnEntityHurt(HurtEvent evt)
        {
            Invoke("On_EntityHurt", evt);
        }

        public void OnItemsLoaded(ItemsBlocks items)
        {
            Invoke("On_ItemsLoaded", items);
        }

        public void OnNPCHurt(HurtEvent evt)
        {
            Invoke("On_NPCHurt", evt);
        }

        public void OnNPCKilled(DeathEvent evt)
        {
            Invoke("On_NPCKilled", evt);
        }

        public void OnPlayerConnected(Player player)
        {
            Invoke("On_PlayerConnected", player);
        }

        public void OnPlayerDisconnected(Player player)
        {
            Invoke("On_PlayerDisconnected", player);
        }

        public void OnPlayerGathering(Player player, GatherEvent evt)
        {
            Invoke("On_PlayerGathering", player, evt);
        }

        public void OnPlayerHurt(HurtEvent evt)
        {
            Invoke("On_PlayerHurt", evt);
        }

        public void OnPlayerKilled(DeathEvent evt)
        {
            Invoke("On_PlayerKilled", evt);
        }

        public void OnPlayerSpawn(Player player, SpawnEvent evt)
        {
            Invoke("On_PlayerSpawning", player, evt);
        }

        public void OnPlayerSpawned(Player player, SpawnEvent evt)
        {
            Invoke("On_PlayerSpawned", player, evt);
        }

        public void OnPluginInit()
        {
            Invoke("On_PluginInit");
        }

        public void OnServerInit()
        {
            Invoke("On_ServerInit");
        }

        public void OnServerShutdown()
        {
            Invoke("On_ServerShutdown");
        }

        public void OnTablesLoaded(Dictionary<string, LootSpawnList> lists)
        {
            Invoke("On_TablesLoaded", lists);
        }

        public void OnTimerCB(string name)
        {
            if (Code.Contains(name + "Callback"))
                Invoke(name + "Callback");
        }

        public void OnTimerCBArgs(string name, object[] args)
        {
            if (Code.Contains(name + "Callback"))
                Invoke(name + "Callback", args);
        }

        #endregion
    }
}
