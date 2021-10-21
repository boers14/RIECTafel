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

    public static ConnectFunction connectFunction = ConnectFunction.Client;

    public static string cityName = "";
}
