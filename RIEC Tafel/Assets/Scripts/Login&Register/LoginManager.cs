using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LogInManager
{
    public static string username = "Sten de Boer";
    public static string datatype = "Police";
    public static int userID = 1;
    public static string pincode = null;
    public static bool iCOVWorker = true;

    public static bool loggedIn { get { return username != null; } }

    public static void LogOut()
    {
        username = null;
        datatype = null;
        userID = -1;
        iCOVWorker = false;
        pincode = null;
    }
}
