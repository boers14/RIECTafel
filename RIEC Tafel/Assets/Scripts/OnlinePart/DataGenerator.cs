using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Geocoding;
using Mapbox.Unity;
using Mapbox.Unity.Utilities;
using Mapbox.Json;
using Mapbox.Utils.JsonConverters;
using UnityEngine.Networking;

public class DataGenerator : MonoBehaviour
{
    [SerializeField]
    private AbstractMap map = null;

    private bool isProcessingAddingData = false;

    private List<string> loremIpsumWords = new List<string>();

    [SerializeField]
    private List<string> feats = new List<string>();

    public int maxAmountOfHits = 10;

    private ReverseGeocodeResource resource = null;

    private Geocoder geocoder = null;

    private Vector2d coordinate = Vector2d.zero;

    private ReverseGeocodeResponse response = null;

    private event System.EventHandler<System.EventArgs> onGeocoderResponse = null;

    private string location = "";

    /// <summary>
    /// Initialize variables
    /// </summary>

    private void Start()
    {
        string[] words = new string[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};
        loremIpsumWords.AddRange(words);

        resource = new ReverseGeocodeResource(coordinate);
        geocoder = MapboxAccess.Instance.Geocoder;
    }

    /// <summary>
    /// Create new fake data when q is pressed
    /// </summary>

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isProcessingAddingData)
        {
            StartCreatingData();
        }
    }

    /// <summary>
    /// Start creating new fake data
    /// </summary>

    private void StartCreatingData()
    {
        isProcessingAddingData = true;
        StartCoroutine(SelectRandomCityLocation());
    }

    /// <summary>
    /// Selects a random location a random distance away from the starting center
    /// Proceed further if the random location is in the netherlands in a city, else try again
    /// </summary>

    private IEnumerator SelectRandomCityLocation()
    {
        Vector2d locationPoint = map.CenterLatitudeLongitude;
        locationPoint.x += Random.Range(-1f, 1f);
        locationPoint.y += Random.Range(-1.5f, 1.5f);

        double xLocationValue = locationPoint.x;
        locationPoint.x = locationPoint.y;
        locationPoint.y = xLocationValue;

        coordinate = Conversions.StringToLatLon(locationPoint.ToString());
        resource.Query = coordinate;
        geocoder.Geocode(resource, HandleGeocoderResponse);
        location = JsonConvert.SerializeObject(response, Formatting.Indented, JsonConverters.Converters);

        if (location != "null")
        {
            string[] placenames = location.Split(new string[] { "place_name" }, System.StringSplitOptions.None);
            if (placenames.Length > 2)
            {
                location = placenames[1];
                location = location.Split(new string[] { "relevance" }, System.StringSplitOptions.None)[0];
                location = location.Split('"')[2];
            }
        }
        yield return new WaitForSeconds(0.15f);

        if (!location.EndsWith("Belgium") && !location.EndsWith("Germany") && !location.StartsWith("{") && location != "null" 
            && location != "" && location.Split(',').Length > 2)
        {
            StartCoroutine(AddDataToDatabase());
        } else
        {
            StartCoroutine(SelectRandomCityLocation());
        }
    }

    /// <summary>
    /// Add the selected location to the database with a random amount of hits, between the 3 and 10
    /// For every hit select a randomly non-selected feat, give every feat a conclusion and indication
    /// </summary>

    private IEnumerator AddDataToDatabase()
    {
        int hits = Random.Range(3, maxAmountOfHits + 1);
        string featureAmount = "Hoeveelheid hits: " + hits.ToString();
        string extraDataExplanation = "";
        List<string> addedFeats = new List<string>();

        for (int i = 0; i < hits; i++)
        {
            string addedFeat = "";
            while (addedFeat == "" || extraDataExplanation.Contains(addedFeat))
            {
                addedFeat = feats[Random.Range(0, feats.Count)];
            }
            addedFeats.Add(addedFeat);

            extraDataExplanation += addedFeat;
            if (i < hits - 1)
            {
                extraDataExplanation += ",\n";
            }
        }

        string conclusion = LoremIpsum(5, 15, 2, 11, addedFeats);
        string indication = LoremIpsum(5, 15, 2, 11, addedFeats);

        string completeDataset = location + "/*datatype*/" + LogInManager.datatype + "/*featureAmount*/" + featureAmount + "/*extraDataExplanation*/" +
             extraDataExplanation + "/*conclusion*/" + conclusion + "/*indication*/" + indication + "/*endOfRow*/";
        WWWForm form = new WWWForm();
        form.AddField("completeDataset", completeDataset);
        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/addcitydata.php", form);
        yield return www.SendWebRequest();

        isProcessingAddingData = false;
        if (www.downloadHandler.text[0] == '0')
        {
            print("Data generated!");
        }
        else
        {
            Debug.LogError("Error#" + www.downloadHandler.text);
        }
    }

    /// <summary>
    /// Set mapbox response to response
    /// </summary>

    private void HandleGeocoderResponse(ReverseGeocodeResponse res)
    {
        response = res;
        if (onGeocoderResponse != null)
        {
            onGeocoderResponse(this, System.EventArgs.Empty);
        }
    }

    /// <summary>
    /// Create random lorem ipsum dialogue for each added feat
    /// </summary>

    private string LoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences, List<string> addedFeats)
    {
        System.Text.StringBuilder result = new System.Text.StringBuilder();

        for (int p = 0; p < addedFeats.Count; p++)
        {
            // Make added feat a bold title
            result.Append("<b>" + addedFeats[p] + ":</b>\n");
            int numSentences = Random.Range(minSentences, maxSentences + 1);
            for (int s = 0; s < numSentences; s++)
            {
                result.Append("\t-");
                int numWords = Random.Range(minWords, maxWords + 1);
                for (int w = 0; w < numWords; w++)
                {
                    result.Append(" ");
                    string word = loremIpsumWords[Random.Range(0, loremIpsumWords.Count)];

                    // The first word of each sentence is capatalized
                    if (w == 0)
                    {
                        word = word[0].ToString().ToUpper() + word.Substring(1);
                    }

                    result.Append(word);
                }
                // Put space between each sentence
                result.Append(". \n");
            }
            // Put extra space between each feat
            result.Append("\n");
        }

        return result.ToString();
    }
}
