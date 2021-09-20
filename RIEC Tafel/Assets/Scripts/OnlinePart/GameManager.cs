using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public enum DataType
    {
        Regular,
        Police,
        Tax,
        PPO,
        Bank
    }

    private Dictionary<DataType, List<string>> locationDataForClients = new Dictionary<DataType, List<string>>();

    [Command]
    public void CmdStartRetrieveCityData(string cityName)
    {
        StartCoroutine(RetrieveCityData(cityName));
    }

    private IEnumerator RetrieveCityData(string cityName)
    {
        WWWForm form = new WWWForm();
        form.AddField("cityName", cityName);
        WWW www = new WWW("http://localhost/riectafel/retrievecitydata.php", form);
        yield return www;

        if (www.text[0] == '0')
        {
            locationDataForClients.Clear();
            //string input = "abc][rfd][5][,][.";
            //string[] parts1 = input.Split(new string[] { "][" }, System.StringSplitOptions.None);

            string[] allLocationData = www.text.Split('/');

            DataType currentDataType = DataType.Regular;
            List<List<string>> allDataTypesLists = new List<List<string>>();
            for (int i = 0; i < System.Enum.GetNames(typeof(DataType)).Length; i++)
            {
                List<string> dataTypeList = new List<string>();
                allDataTypesLists.Add(dataTypeList);
            }

            for (int i = 1; i < allLocationData.Length; i++)
            {
                if (System.Enum.IsDefined(typeof(DataType), allLocationData[i]))
                {
                    currentDataType = (DataType)System.Enum.Parse(typeof(DataType), allLocationData[i]);
                } else
                {
                    allDataTypesLists[(int)currentDataType].Add(allLocationData[i]);
                }
            }

            for (int i = 0; i < allDataTypesLists.Count; i++)
            {
                locationDataForClients.Add((DataType)i, allDataTypesLists[i]);
            }
        }
        else
        {
            Debug.LogError("Error#" + www.text);
        }
    }

    [Command (channel = 0, requiresAuthority = false)]
    public void CmdGiveBackLocationData(string dataType, int playerNumber)
    {
        PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(currentConnections);
        PlayerConnection player = playerConnections.Find(i => i.playerNumber == playerNumber);

        DataType requiredDatatype = (DataType)System.Enum.Parse(typeof(DataType), dataType);
        List<List<string>> locationData = new List<List<string>>();
        List<string> dataTypes = new List<string>();

        locationData.Add(locationDataForClients[DataType.Regular]);
        dataTypes.Add(DataType.Regular.ToString());
        if (requiredDatatype != DataType.Regular)
        {
            locationData.Add(locationDataForClients[requiredDatatype]);
            dataTypes.Add(requiredDatatype.ToString());
        }

        player.RpcSetLocationDataForPlayer(locationData, dataTypes, playerNumber);
    }
}
