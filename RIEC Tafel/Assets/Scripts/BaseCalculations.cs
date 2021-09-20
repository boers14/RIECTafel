using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BaseCalculations
{
    public static float CalculatePosDiff(float oldMaxScale, float newMaxScale, float offsetParameter, float movementPower)
    {
        float percentageOnMap = offsetParameter / oldMaxScale;
        float newPos = percentageOnMap * newMaxScale;
        float possDiff = newPos - offsetParameter;
        return possDiff / movementPower;
    }
}
