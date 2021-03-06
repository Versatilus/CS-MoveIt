﻿using ColossalFramework.Plugins;
using MoveIt.Localization;
using System;
using System.Linq;
using System.Reflection;

namespace MoveIt
{
    internal class NodeController_Manager
    {
        internal bool Enabled = false;
        internal readonly Type tNodeManager;
        internal readonly MethodInfo mCopy, mPaste;
        internal readonly Assembly Assembly;
        internal readonly bool NoCopy; // using integration instead.
        internal const UInt64 ID = 2085403475ul;
        internal const string NAME = "NodeController";

        internal NodeController_Manager()
        {
            if (isModInstalled())
            {
                Enabled = true;

                Assembly = null;
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {

                    if (assembly.FullName.StartsWith(NAME))
                    {
                        Assembly = assembly;
                        break;
                    }
                }
                if (Assembly == null) throw new Exception("Assembly not found (Failed [NC-F1])");
                var version = Assembly.GetName().Version;
                NoCopy = version >= new Version(2,0,0,0);

                tNodeManager = Assembly.GetType("NodeController.NodeManager")
                    ?? throw new Exception("Type NodeManager not found (Failed [NC-F2])");
                BindingFlags f = BindingFlags.Public | BindingFlags.Static;

                mCopy = tNodeManager.GetMethod("CopyNodeData", f)
                    ?? throw new Exception("NodeController.NodeManager.CopyNodeData() not found (Failed [NC-F3])");
                
                mPaste = tNodeManager.GetMethod("PasteNodeData", f)
                    ?? throw new Exception("NodeController.NodeManager.PasteNodeData() not found (Failed [NC-F4])");
            }
            else
            {
                Enabled = false;
            }
        }

        public void PasteNode(ushort nodeID, NodeState state)
        {
            if (!Enabled) return;
            byte[] data = state.NodeControllerData;
            mPaste.Invoke(null, new object[] {nodeID, data});
        }

        public byte[] CopyNode(ushort nodeID)
        {
            if (!Enabled || NoCopy) return null;
            return mCopy.Invoke(null, new object[] {nodeID}) as byte[];
        }

        internal string Encode64(byte[] data)
        {
            if (!Enabled || data == null || data.Length == 0) return null;
            return Convert.ToBase64String(data);
        }
        internal byte[] Decode64(string base64Data)
        {
            if (!Enabled || base64Data == null || base64Data.Length == 0) return null;
            return Convert.FromBase64String(base64Data);
        }

        internal static bool isModInstalled()
        {
            return PluginManager.instance.GetPluginsInfo().Any( mod =>{
                bool found = mod.publishedFileID.AsUInt64 == ID || mod.name.Contains(NAME) || mod.name.Contains(ID.ToString());
                return found && mod.isEnabled;
            }); 
        }

        internal static string getVersionText()
        {
            if (isModInstalled())
                return Str.integration_NC_Found;
            else
                return Str.integration_NC_Notfound;
        }
    }
}
