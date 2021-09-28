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

    [SerializeField]
    private TMP_Dropdown dataTypeDropdown = null, cityDropdown = null;

    private bool isProcessingAddingData = false;

    private List<string> loremIpsumWords = new List<string>();

    [SerializeField]
    private List<string> regularFeats = new List<string>(), policeFeats = new List<string>(), taxFeats = new List<string>(), 
        ppoFeats = new List<string>(), bankFeats = new List<string>();

    private List<List<string>> regularExtra = new List<List<string>>(), policeExtra = new List<List<string>>(), taxExtra = new List<List<string>>(), 
        ppoExtra = new List<List<string>>(), bankExtra = new List<List<string>>();

    private void Start()
    {
        string[] words = new string[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};
        loremIpsumWords.AddRange(words);

        CreateFakeExtraData();
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

        string cityName = cityDropdown.options[cityDropdown.value].text;
        cityName = cityName.ToLower();
        cityName = System.Text.RegularExpressions.Regex.Replace(cityName, @"\s+", "");

        Vector2d locationPoint = map.CenterLatitudeLongitude;
        locationPoint.x += Random.Range(-0.03f, 0.03f);
        locationPoint.y += Random.Range(-0.03f, 0.03f);
        double xLocationValue = locationPoint.x;
        locationPoint.x = locationPoint.y;
        locationPoint.y = xLocationValue;
        string location = locationPoint.ToString();

        string dataTypeString = dataTypeDropdown.options[dataTypeDropdown.value].text;
        GameManager.DataType dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), dataTypeString);
        string featureType = "";
        string extraDataExplanation = "";
        int featureTypeSelection = 0;
        switch (dataType)
        {
            case GameManager.DataType.Regular:
                featureTypeSelection = Random.Range(0, regularFeats.Count);
                featureType = regularFeats[featureTypeSelection];
                extraDataExplanation = regularExtra[featureTypeSelection][Random.Range(0, regularExtra[featureTypeSelection].Count)];
                break;
            case GameManager.DataType.Police:
                featureTypeSelection = Random.Range(0, policeFeats.Count);
                featureType = policeFeats[featureTypeSelection];
                extraDataExplanation = policeExtra[featureTypeSelection][Random.Range(0, policeExtra[featureTypeSelection].Count)];
                break;
            case GameManager.DataType.Tax:
                featureTypeSelection = Random.Range(0, taxFeats.Count);
                featureType = taxFeats[featureTypeSelection];
                extraDataExplanation = taxExtra[featureTypeSelection][Random.Range(0, taxExtra[featureTypeSelection].Count)];
                break;
            case GameManager.DataType.PPO:
                featureTypeSelection = Random.Range(0, ppoFeats.Count);
                featureType = ppoFeats[featureTypeSelection];
                extraDataExplanation = ppoExtra[featureTypeSelection][Random.Range(0, ppoExtra[featureTypeSelection].Count)];
                break;
            case GameManager.DataType.Bank:
                featureTypeSelection = Random.Range(0, bankFeats.Count);
                featureType = bankFeats[featureTypeSelection];
                extraDataExplanation = bankExtra[featureTypeSelection][Random.Range(0, bankExtra[featureTypeSelection].Count)];
                break;
        }

        string featureAmount = "";
        if (dataType != GameManager.DataType.Regular)
        {
            int hits = Random.Range(3, 11);
            featureAmount = "\nHoeveelheid hits: " + hits.ToString();
        }

        string conclusion = LoremIpsum(8, 15, 10, 40, 2);
        string indication = LoremIpsum(8, 15, 10, 40, 2);

        string completeDataset = location + "/*datatype*/" + dataTypeString + "/*featuretype*/" + featureType + "/*extraDataExplanation*/" +
             extraDataExplanation + featureAmount + "/*conclusion*/" + conclusion + "/*indication*/" + indication + "/*endOfRow*/";

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

    private void CreateFakeExtraData()
    {
        List<string> regularExtra1 = new List<string>();
        string[] regularExtra1array = new string[]{"Moord op Harry de Breker van 27 jaar op 22 juli 2018", "Moord op Willem van den Broek van 41 jaar " +
            "op 15 maart 2015", "Moord op Tina Bakker van 32 jaar op 15 januarie 2012", "Moord op Bea de Boer van 36 jaar oud op 3 december 2005", 
            "Moord op Ron van Dijk van 51 jaar oud op 15 oktober 2009"};
        regularExtra1.AddRange(regularExtra1array);
        regularExtra.Add(regularExtra1);

        List<string> regularExtra2 = new List<string>();
        string[] regularExtra2array = new string[]{"15 laptops gestolen op 5 augustus 2017", "3 auto's gestolen op 6 juli 2020", 
            "5 juwelen gestolen op 6 maart 2016", "3 sieraden gestolen op 1 september 2019", "5 telefoons gestolen op 21 februarie 2018"};
        regularExtra2.AddRange(regularExtra2array);
        regularExtra.Add(regularExtra2);

        List<string> regularExtra3 = new List<string>();
        string[] regularExtra3array = new string[]{"Witswas zaak betreffende $15.000 opgeslost op 6 juni 2000",
            "Witswas zaak betreffende $25.000 opgeslost op 29 november 2010", "Witswas zaak betreffende $30.000 opgeslost op 19 april 2003",
            "Witswas zaak betreffende $90.000 opgeslost op 20 augustus 2004", "Witswas zaak betreffende $150.000 opgeslost op 7 maart 2008"};
        regularExtra3.AddRange(regularExtra3array);
        regularExtra.Add(regularExtra3);

        List<string> regularExtra4 = new List<string>();
        string[] regularExtra4array = new string[]{"Illigale wietplantage gevonden op 11 november 2012",
            "Illigale wietplantage gevonden op 8 oktober 2007", "Illigale wietplantage gevonden op 4 april 2015",
            "Harddrugs handel gevonden op 23 maart 2017", "Harddrugs handel gevonden op 26 juni 2001"};
        regularExtra4.AddRange(regularExtra4array);
        regularExtra.Add(regularExtra4);

        List<string> policeExtra1 = new List<string>();
        string[] policeExtra1array = new string[]{"Er zijn hier gemiddeld 2 per jaar", "Er zijn hier gemiddeld 3 per jaar",
            "Er zijn hier gemiddeld 5 per jaar", "Er zijn hier gemiddeld 7 per jaar", "Er zijn hier gemiddeld 10 per jaar"};
        policeExtra1.AddRange(policeExtra1array);
        policeExtra.Add(policeExtra1);
        ppoExtra.Add(policeExtra1);

        List<string> policeExtra2 = new List<string>();
        string[] policeExtra2array = new string[]{"Gebeurt gemiddeld 5 keer per jaar", "Gebeurt gemiddeld 8 keer per jaar",
            "Gebeurt gemiddeld 13 keer per jaar", "Gebeurt gemiddeld 17 keer per jaar", "Gebeurt gemiddeld 25 keer per jaar"};
        policeExtra2.AddRange(policeExtra2array);
        policeExtra.Add(policeExtra2);
        ppoExtra.Add(policeExtra2);

        List<string> policeExtra3 = new List<string>();
        string[] policeExtra3array = new string[]{"1 wordt hier gevonden per 5 jaar", "2 worden hier gevonden per 5 jaar",
            "3 worden hier gevonden per 5 jaar", "4 worden hier gevonden per 5 jaar", "5 worden hier gevonden per 5 jaar"};
        policeExtra3.AddRange(policeExtra3array);
        policeExtra.Add(policeExtra3);
        ppoExtra.Add(policeExtra3);

        List<string> taxExtra1 = new List<string>();
        string[] taxExtra1array = new string[]{ "Buitelands R.P.", "Snelle groeier", "VT", "Inkomen" };
        taxExtra1.AddRange(taxExtra1array);
        taxExtra.Add(taxExtra1);
        bankExtra.Add(taxExtra1);

        List<string> taxExtra2 = new List<string>();
        string[] taxExtra2array = new string[]{"Geen hypotheek", "Meer dan 3 hypotheken", "Hypotheeknemer NP", "Hypotheeknemer RP", 
            "Hypotheeknemer buitenland", "Hypotheeknemer risicoland"};
        taxExtra2.AddRange(taxExtra2array);
        taxExtra.Add(taxExtra2);
        bankExtra.Add(taxExtra2);

        List<string> taxExtra3 = new List<string>();
        string[] taxExtra3array = new string[]{ "Veiling", "ABC-transactie" };
        taxExtra3.AddRange(taxExtra3array);
        taxExtra.Add(taxExtra3);
        bankExtra.Add(taxExtra3);
    }
}
