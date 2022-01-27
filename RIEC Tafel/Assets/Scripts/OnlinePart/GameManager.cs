using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine.Networking;

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

    private List<string> allLocations = new List<string>(), conclusions = new List<string>(), indications = new List<string>(), 
        featureAmounts = new List<string>(), extraExplanations = new List<string>();

    [SyncVar]
    private string cityName = "Utrecht";

    /// <summary>
    /// Start retrieving city data
    /// </summary>

    [Command]
    public void CmdStartRetrieveCityData(string cityName)
    {
        if (cityName != "")
        {
            this.cityName = cityName;
        }
        StartCoroutine(RetrieveCityData());
    }

    /// <summary>
    /// Clear all data lists and fill these lists with all the retrieved data
    /// </summary>

    public IEnumerator RetrieveCityData()
    {
        WWWForm form = new WWWForm();
        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/retrievecitydata.php", form);
        yield return www.SendWebRequest();

        if (www.downloadHandler.text[0] == '0')
        {
            dataTypes.Clear();
            allLocations.Clear();
            conclusions.Clear();
            indications.Clear();
            featureAmounts.Clear();
            extraExplanations.Clear();

            string allData = www.downloadHandler.text;
            allData = allData.Remove(0, 1);

            string[] allLocationData = allData.Split(new string[] { "/*endOfRow*/" }, System.StringSplitOptions.None);

            for (int i = 0; i < allLocationData.Length - 1; i++)
            {
                string[] location = allLocationData[i].Split(new string[] { "/*datatype*/" }, System.StringSplitOptions.None);
                allLocations.Add(location[0]);

                string[] dataType = location[1].Split(new string[] { "/*featureAmount*/" }, System.StringSplitOptions.None);
                dataTypes.Add((DataType)System.Enum.Parse(typeof(DataType), dataType[0]));

                string[] featureAmount = dataType[1].Split(new string[] { "/*extraDataExplanation*/" }, 
                    System.StringSplitOptions.None);
                featureAmounts.Add(featureAmount[0]);

                string[] extraExplanation = featureAmount[1].Split(new string[] { "/*conclusion*/" }, System.StringSplitOptions.None);
                extraExplanations.Add(extraExplanation[0]);

                string[] conclusionAndIndication = extraExplanation[1].Split(new string[] { "/*indication*/" }, 
                    System.StringSplitOptions.None);
                conclusions.Add(conclusionAndIndication[0]);
                indications.Add(conclusionAndIndication[1]);
            }
        }
        else
        {
            Debug.LogError("Error#" + www.downloadHandler.text);
        }
    }

    /// <summary>
    /// Create the data for the tutorial, based on the first piece of retrieved information gotten
    /// </summary>

    public void CreateTutorialLocationData(string dataType, POIManager poiManager, AbstractMap abstractMap)
    {
        DataType requiredDatatype = (DataType)System.Enum.Parse(typeof(DataType), dataType);
        List<string> locationData = new List<string>();
        List<string> dataTypes = new List<string>();
        List<string> neededAmounts = new List<string>();
        List<string> neededExtraInfo = new List<string>();
        List<string> neededConclusions = new List<string>();
        List<string> neededIndications = new List<string>();

        AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, neededAmounts, neededExtraInfo, 
            DataType.Regular);

        if (requiredDatatype != DataType.Regular)
        {
            AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, neededAmounts, neededExtraInfo, 
                requiredDatatype);
        }

        for (int i = locationData.Count - 1; i > 0; i--)
        {
            locationData.RemoveAt(i);
            dataTypes.RemoveAt(i);
            neededAmounts.RemoveAt(i);
            neededExtraInfo.RemoveAt(i);
            neededConclusions.RemoveAt(i);
            neededIndications.RemoveAt(i);
        }

        // Set the retrieved tutorial location data to the center of the map
        Vector2d coordinate = abstractMap.CenterLatitudeLongitude;
        double xCoordinate = coordinate.x;
        coordinate.x = coordinate.y;
        coordinate.y = xCoordinate;
        locationData[0] = ", 1111 AA " + cityName;
        poiManager.SetLocationData(locationData, dataTypes, neededAmounts, neededExtraInfo, neededConclusions, neededIndications, 
            cityName);
    }

    /// <summary>
    /// Retrieve required location data based on the datatype of the player asking. All players need data, so no authority
    /// is required over this object to ask for the required data.
    /// </summary>
    
    [Command (channel = 0, requiresAuthority = false)]
    public void CmdGiveBackLocationData(string dataType, int playerNumber)
    {
        PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(currentConnections);
        PlayerConnection player = playerConnections.Find(i => i.playerNumber == playerNumber);

        DataType requiredDatatype = (DataType)System.Enum.Parse(typeof(DataType), dataType);
        List<string> locationData = new List<string>();
        List<string> dataTypes = new List<string>();
        List<string> neededAmounts = new List<string>();
        List<string> neededExtraInfo = new List<string>();
        List<string> neededConclusions = new List<string>();
        List<string> neededIndications = new List<string>();

        AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, neededAmounts, neededExtraInfo, 
            DataType.Regular);

        if (requiredDatatype != DataType.Regular)
        {
            AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, neededAmounts, neededExtraInfo, 
                requiredDatatype);
        }

        // Send all data throught the network with the playernumber given at the start so it can be identified which player asked for
        // the data
        player.RpcSetLocationDataForPlayer(locationData, dataTypes, neededAmounts, neededExtraInfo, neededConclusions, 
            neededIndications, playerNumber, cityName);
    }

    /// <summary>
    /// If the datatype aligns with the required datatype then add the data from the list to the list that will be send to the player
    /// that asked for the data (based on player number)
    /// </summary>

    private void AddDataToLists(List<string> locationData, List<string> dataTypes, List<string> neededConclusions, List<string> neededIndications,
        List<string> neededAmounts, List<string> neededExtraInfo, DataType requiredDatatype)
    {
        for (int i = 0; i < this.dataTypes.Count; i++)
        {
            if (this.dataTypes[i] == requiredDatatype)
            {
                dataTypes.Add(requiredDatatype.ToString());
                locationData.Add(allLocations[i]);
                neededConclusions.Add(conclusions[i]);
                neededIndications.Add(indications[i]);
                neededAmounts.Add(featureAmounts[i]);
                neededExtraInfo.Add(extraExplanations[i]);
            }
        }
    }

    /// <summary>
    /// Logout user in the discussion scene if the player quits the application here
    /// </summary>

    private void OnApplicationQuit()
    {
        LogInManager.LogOut(this);
    }
}
