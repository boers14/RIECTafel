using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConnectionManager
{
    public enum ConnectFunction {
        Host,
        Join
    }

    public static ConnectFunction connectFunction = ConnectFunction.Join;

    public static string cityName = "", roomName = "";
}
