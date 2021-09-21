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

    private List<DataType> dataTypes = new List<DataType>();

    private List<string> allLocations = new List<string>(), conclusions = new List<string>(), indications = new List<string>();

    [Command]
    public void CmdStartRetrieveCityData(string cityName)
    {
        StartCoroutine(RetrieveCityData(cityName));
    }

    private IEnumerator RetrieveCityData(string cityName)
    {
        WWWForm form = new WWWForm();
        cityName = cityName.ToLower();
        cityName = System.Text.RegularExpressions.Regex.Replace(cityName, @"\s+", "");
        form.AddField("cityName", cityName);
        WWW www = new WWW("http://localhost/riectafel/retrievecitydata.php", form);
        yield return www;

        if (www.text[0] == '0')
        {
            dataTypes.Clear();
            allLocations.Clear();
            conclusions.Clear();
            indications.Clear();

            string allData = www.text;
            allData = allData.Remove(0, 1);

            string[] allLocationData = allData.Split(new string[] { "/*endOfRow*/" }, System.StringSplitOptions.None);

            for (int i = 0; i < allLocationData.Length - 1; i++)
            {
                string[] location = allLocationData[i].Split(new string[] { "/*datatype*/" }, System.StringSplitOptions.None);
                allLocations.Add(location[0]);

                string[] dataType = location[1].Split(new string[] { "/*conclusion*/" }, System.StringSplitOptions.None);
                dataTypes.Add((DataType)System.Enum.Parse(typeof(DataType), dataType[0]));

                string[] conclusionAndIndication = dataType[1].Split(new string[] { "/*indication*/" }, System.StringSplitOptions.None);
                conclusions.Add(conclusionAndIndication[0]);
                indications.Add(conclusionAndIndication[1]);
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
        List<string> locationData = new List<string>();
        List<string> dataTypes = new List<string>();
        List<string> neededConclusions = new List<string>();
        List<string> neededIndications = new List<string>();

        AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, DataType.Regular);

        if (requiredDatatype != DataType.Regular)
        {
            AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, requiredDatatype);
        }

        player.RpcSetLocationDataForPlayer(locationData, dataTypes, neededConclusions, neededIndications, playerNumber);
    }

    private void AddDataToLists(List<string> locationData, List<string> dataTypes, List<string> neededConclusions, List<string> neededIndications,
        DataType requiredDatatype)
    {
        for (int i = 0; i < this.dataTypes.Count; i++)
        {
            if (this.dataTypes[i] == requiredDatatype)
            {
                dataTypes.Add(requiredDatatype.ToString());
                locationData.Add(allLocations[i]);
                neededConclusions.Add(conclusions[i]);
                neededIndications.Add(indications[i]);
            }
        }
    }
}
