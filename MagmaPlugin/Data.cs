﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Magma
{
    using Fougerite;
    using Facepunch.Utility;

    public class Data
    {
        public readonly System.Collections.Generic.List<string> chat_history = new System.Collections.Generic.List<string>();
        public readonly System.Collections.Generic.List<string> chat_history_username = new System.Collections.Generic.List<string>();
        private static Magma.Data data;
        public static Hashtable inifiles = new Hashtable();
        public Hashtable Fougerite_shared_data = new Hashtable();

        public void AddTableValue(string tablename, object key, object val)
        {
            Hashtable hashtable = (Hashtable)DataStore.GetInstance().datastore[tablename];
            if (hashtable == null)
            {
                hashtable = new Hashtable();
                DataStore.GetInstance().datastore.Add(tablename, hashtable);
            }
            if (hashtable.ContainsKey(key))
            {
                hashtable[key] = val;
            }
            else
            {
                hashtable.Add(key, val);
            }
        }

        public string GetConfigValue(string config, string section, string key)
        {
            IniParser parser = (IniParser)inifiles[config.ToLower()];
            if (parser == null)
            {
                return "Config does not exist";
            }
            return parser.GetSetting(section, key);
        }

        public static Magma.Data GetData()
        {
            if (data == null)
            {
                data = new Magma.Data();
            }
            return data;
        }

        public object GetTableValue(string tablename, object key)
        {
            Hashtable hashtable = (Hashtable)DataStore.GetInstance().datastore[tablename];
            if (hashtable == null)
            {
                return null;
            }
            return hashtable[key];
        }

        public void Load()
        {
            inifiles.Clear();
            foreach (string str in Directory.GetDirectories(Fougerite.Config.GetPublicFolder()))
            {
                string path = "";
                foreach (string str3 in Directory.GetFiles(str))
                {
                    if (Path.GetFileName(str3).Contains(".cfg") && Path.GetFileName(str3).Contains(Path.GetFileName(str)))
                    {
                        path = str3;
                    }
                }
                if (path != "")
                {
                    string key = Path.GetFileName(path).Replace(".cfg", "").ToLower();
                    inifiles.Add(key, new IniParser(path));
                    Logger.LogDebug("Loaded Config: " + key);
                }
            }
        }

        public string[] SplitQuoteStrings(string str)
        {
            return Facepunch.Utility.String.SplitQuotesStrings(str);
        }

        public int StrLen(string str)
        {
            return str.Length;
        }

        public string Substring(string str, int from, int length)
        {
            return str.Substring(from, length);
        }

        public int ToInt(string num)
        {
            return int.Parse(num);
        }

        public string ToLower(string str)
        {
             return str.ToLower();
        }

        public string ToUpper(string str)
        {
            return str.ToUpper();
        }
    }
}