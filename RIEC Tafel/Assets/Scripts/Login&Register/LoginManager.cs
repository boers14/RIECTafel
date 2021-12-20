using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class LogInManager
{
    public static string username = "Sten de Boer", datatype = "Police", pincode = null;
    public static int userID = 25;
    public static bool iCOVWorker = true;
    public static string avatarData = "Head\nstar1\n43,75.35.43,75/*nextbodypart*/Body\nCapsule\n0,5.0,65.0,5\n0,5.0,5";

    public static bool loggedIn { get { return username != null; } }

    public static void LogOut(MonoBehaviour monoBehaviour)
    {
        monoBehaviour.StartCoroutine(LogOutUser());

        username = null;
        datatype = null;
        userID = -1;
        iCOVWorker = false;
        pincode = null;
        avatarData = null;

        SaveSytem.SaveGame();
    }

    private static IEnumerator LogOutUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", userID);
        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/logout.php", form);
        yield return www.SendWebRequest();

        if (www.downloadHandler.text[0] != '0')
        {
            Debug.LogError("Log out failed. Error#" + www.downloadHandler.text);
        }
    }
}
