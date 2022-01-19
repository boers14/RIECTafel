using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine.Networking;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
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

    private string cityName = "Utrecht";

    public void StartRetrieveCityData(string cityName)
    {
        if (cityName != "")
        {
            this.cityName = cityName;
        }
        StartCoroutine(RetrieveCityData());
    }

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

                string[] featureAmount = dataType[1].Split(new string[] { "/*extraDataExplanation*/" }, System.StringSplitOptions.None);
                featureAmounts.Add(featureAmount[0]);

                string[] extraExplanation = featureAmount[1].Split(new string[] { "/*conclusion*/" }, System.StringSplitOptions.None);
                extraExplanations.Add(extraExplanation[0]);

                string[] conclusionAndIndication = extraExplanation[1].Split(new string[] { "/*indication*/" }, System.StringSplitOptions.None);
                conclusions.Add(conclusionAndIndication[0]);
                indications.Add(conclusionAndIndication[1]);
            }
        }
        else
        {
            Debug.LogError("Error#" + www.downloadHandler.text);
        }
    }

    public void CreateTutorialLocationData(string dataType, POIManager poiManager, AbstractMap abstractMap)
    {
        DataType requiredDatatype = (DataType)System.Enum.Parse(typeof(DataType), dataType);
        List<string> locationData = new List<string>();
        List<string> dataTypes = new List<string>();
        List<string> neededAmounts = new List<string>();
        List<string> neededExtraInfo = new List<string>();
        List<string> neededConclusions = new List<string>();
        List<string> neededIndications = new List<string>();

        AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, neededAmounts, neededExtraInfo, DataType.Regular);

        if (requiredDatatype != DataType.Regular)
        {
            AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, neededAmounts, neededExtraInfo, requiredDatatype);
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

        Vector2d coordinate = abstractMap.CenterLatitudeLongitude;
        double xCoordinate = coordinate.x;
        coordinate.x = coordinate.y;
        coordinate.y = xCoordinate;
        locationData[0] = ", 1111 AA " + cityName;
        poiManager.SetLocationData(locationData, dataTypes, neededAmounts, neededExtraInfo, neededConclusions, neededIndications, cityName);
    }

    public void StartGiveBackLocationData(string dataType, int playerNumber)
    {
        Player targetedPlayer = FindObjectOfType<PlayerConnection>().FetchPlayerConnectionBasedOnNumber(playerNumber).controlledPlayer;
        photonView.RPC("RpcGiveBackLocationData", targetedPlayer, dataType, playerNumber);
    }

    [PunRPC]
    private void RpcGiveBackLocationData(string dataType, int playerNumber)
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

        AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, neededAmounts, neededExtraInfo, DataType.Regular);

        if (requiredDatatype != DataType.Regular)
        {
            AddDataToLists(locationData, dataTypes, neededConclusions, neededIndications, neededAmounts, neededExtraInfo, requiredDatatype);
        }

        player.StartSetLocationDataForPlayer(locationData, dataTypes, neededAmounts, neededExtraInfo, neededConclusions, neededIndications, 
            playerNumber, cityName);
    }

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

    private void OnApplicationQuit()
    {
        LogInManager.LogOut(this);
    }
}
