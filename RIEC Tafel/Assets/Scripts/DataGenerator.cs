using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine.UI;
using TMPro;

public class DataGenerator : MonoBehaviour
{
    [SerializeField]
    private AbstractMap map = null;

    private bool isProcessingAddingData = false;

    private List<string> loremIpsumWords = new List<string>();

    [SerializeField]
    private List<string> feats = new List<string>();

    public int maxAmountOfHits = 10;

    private void Start()
    {
        string[] words = new string[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};
        loremIpsumWords.AddRange(words);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isProcessingAddingData)
        {
            StartCoroutine(AddDataToDatabase());
        }
    }

    private IEnumerator AddDataToDatabase()
    {
        isProcessingAddingData = true;

        string cityName = ConnectionManager.cityName;
        cityName = cityName.ToLower();
        cityName = System.Text.RegularExpressions.Regex.Replace(cityName, @"\s+", "");

        Vector2d locationPoint = map.CenterLatitudeLongitude;
        locationPoint.x += Random.Range(-0.03f, 0.03f);
        locationPoint.y += Random.Range(-0.03f, 0.03f);
        double xLocationValue = locationPoint.x;
        locationPoint.x = locationPoint.y;
        locationPoint.y = xLocationValue;
        string location = locationPoint.ToString();

        int hits = Random.Range(3, maxAmountOfHits + 1);
        string featureAmount = "Hoeveelheid hits: " + hits.ToString();
        string extraDataExplanation = "";

        for (int i = 0; i < hits; i++)
        {
            string addedFeat = "";
            while (addedFeat == "" || extraDataExplanation.Contains(addedFeat))
            {
                addedFeat = feats[Random.Range(0, feats.Count)];
            }

            extraDataExplanation += addedFeat;
            if (i < hits - 1)
            {
                extraDataExplanation += ",\n";
            }
        }

        string conclusion = LoremIpsum(8, 15, 10, 40, 2);
        string indication = LoremIpsum(8, 15, 10, 40, 2);

        string completeDataset = location + "/*datatype*/" + LogInManager.datatype + "/*featureAmount*/" + featureAmount + "/*extraDataExplanation*/" +
             extraDataExplanation + "/*conclusion*/" + conclusion + "/*indication*/" + indication + "/*endOfRow*/";

        WWWForm form = new WWWForm();
        form.AddField("cityName", cityName);
        form.AddField("completeDataset", completeDataset);
        WWW www = new WWW("http://localhost/riectafel/addcitydata.php", form);
        yield return www;

        isProcessingAddingData = false;
        if (www.text[0] == '0')
        {
            print("Data generated!");
        } else
        {
            Debug.LogError("Error#" + www.text);
        }
    }

    private string LoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences, int numParagraphs)
    {
        System.Text.StringBuilder result = new System.Text.StringBuilder();

        for (int p = 0; p < numParagraphs; p++)
        {
            int numSentences = Random.Range(minSentences, maxSentences + 1);
            for (int s = 0; s < numSentences; s++)
            {
                int numWords = Random.Range(minWords, maxWords + 1);
                for (int w = 0; w < numWords; w++)
                {
                    if (w > 0) { result.Append(" "); }
                    string word = loremIpsumWords[Random.Range(0, loremIpsumWords.Count)];

                    if (w == 0)
                    {
                        word = word[0].ToString().ToUpper() + word.Substring(1);
                    }

                    result.Append(word);
                }
                result.Append(". ");
            }
            result.Append("\n\n");
        }

        return result.ToString();
    }
}
