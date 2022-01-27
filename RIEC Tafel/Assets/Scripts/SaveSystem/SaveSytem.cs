using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSytem
{
    /// <summary>
    /// Saves the game to a binary file.
    /// </summary>

    public static void SaveGame()
    {
        DeleteGame();
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, "savedGame.RIECTable");
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            PlayerData data = new PlayerData();
            Debug.Log("saved game");
            formatter.Serialize(stream, data);
            stream.Close();
        }
    }

    /// <summary>
    /// Loads the game by grabbing and decoding the saved data if there is a file to be found.
    /// </summary>

    public static PlayerData LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, "savedGame.RIECTable");
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open)) {

                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();
                return data;
            }
        }
        else
        {
            Debug.LogError("No file found");
            return null;
        }
    }

    /// <summary>
    /// Delete the current save file if there is one.
    /// </summary>

    public static void DeleteGame()
    {
        string path = Path.Combine(Application.persistentDataPath, "savedGame.RIECTable");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("deleted game");
        }
        else
        {
            Debug.Log("No file found");
        }
    }

    /// <summary>
    /// Checks whether there is a saved file.
    /// </summary>

    public static bool CheckIfFileExist()
    {
        string path = Path.Combine(Application.persistentDataPath, "savedGame.RIECTable");
        if (File.Exists(path))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
