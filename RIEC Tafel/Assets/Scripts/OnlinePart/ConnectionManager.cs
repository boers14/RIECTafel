using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConnectionManager
{
    public enum ConnectFunction {
        ServerClient,
        Server,
        Client
    }

    /// <summary>
    /// Use connection function to start server or join discussion and use cityname as starting point for discussion
    /// </summary>

    public static ConnectFunction connectFunction = ConnectFunction.Client;

    public static string cityName = "";
}
