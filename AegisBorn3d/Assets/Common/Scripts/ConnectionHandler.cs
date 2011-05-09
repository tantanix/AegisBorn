﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Sfs2X.Core;
using Sfs2X.Entities.Data;
using UnityEngine;
using Sfs2X;

public class ConnectionHandler : MonoBehaviour
{
    protected SmartFox smartFox;
    public string registerZone = "register";
    public bool debug = true;

    protected delegate void ExtensionHandler(ISFSObject data);

    protected Dictionary<string, ExtensionHandler> handlers;

    protected EncryptionProvider provider;

    protected void Awake()
    {
        handlers = new Dictionary<string, ExtensionHandler>();
        Application.runInBackground = true;

        if (SmartFoxConnection.IsInitialized)
        {
            smartFox = SmartFoxConnection.Connection;
        }
        else
        {
            smartFox = new SmartFox(debug);
        }

        // Create encryption provider
        provider = EncryptionProvider.GetInstance();
        smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);

    }

    protected void FixedUpdate()
    {
        smartFox.ProcessEvents();
    }

    protected void OnApplicationQuit()
    {
        if (smartFox.IsConnected)
        {
            UnregisterSFSSceneCallbacks();
            smartFox.Disconnect();
        }
    }

    protected void UnregisterSFSSceneCallbacks()
    {
        // This should be called when switching scenes, so callbacks from the backend do not trigger code in this scene
        smartFox.RemoveAllEventListeners();
		handlers.Clear();
    }

    // This method handles all the responses from the server
    protected void OnExtensionResponse(BaseEvent evt)
    {
        try
        {
            string cmd = (string)evt.Params["cmd"];
            ISFSObject dt = (SFSObject)evt.Params["params"];

            ExtensionHandler handler;

            if (handlers.TryGetValue(cmd, out handler))
            {
                handler(dt);
            }
            else
            {
                Debug.LogError("Got unhandled cmd: " + cmd);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception handling response: " + e.Message + " >>> " + e.StackTrace);
        }

    }

}
