using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BaseCalculations
{
    public static float CalculatePosDiff(float oldMaxScale, float newMaxScale, float offsetParameter)
    {
        float percentageOnMap = offsetParameter / oldMaxScale;
        float newPos = percentageOnMap * newMaxScale;
        float possDiff = newPos - offsetParameter;
        return possDiff;
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10, digits);
        return Mathf.Round(value * mult) / mult;
    }
}
