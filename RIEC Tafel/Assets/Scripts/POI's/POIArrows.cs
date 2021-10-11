using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class POIArrows : MonoBehaviour
{
    private TMP_Text poiIndicationText = null;

    private enum ArrowDirection
    {
        Left,
        UpLeft,
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft
    }

    [SerializeField]
    private ArrowDirection arrowDirection = ArrowDirection.Left;

    [SerializeField]
    private Transform table = null;

    private void Start()
    {
        poiIndicationText = GetComponentInChildren<TMP_Text>();
    }

    public void SetArrowText(List<GameObject> allPOIs, List<string> dutchNames, List<bool> poiVisibility)
    {
        switch(arrowDirection)
        {
            case ArrowDirection.Left:
                CalculateDirectionOfPOIs(allPOIs, dutchNames, poiVisibility, -112.5f, -67.5f);
                break;
            case ArrowDirection.UpLeft:
                CalculateDirectionOfPOIs(allPOIs, dutchNames, poiVisibility, -67.5f, -22.5f);
                break;
            case ArrowDirection.Up:
                CalculateDirectionOfPOIs(allPOIs, dutchNames, poiVisibility, -22.5f, 22.5f);
                break;
            case ArrowDirection.UpRight:
                CalculateDirectionOfPOIs(allPOIs, dutchNames, poiVisibility, 22.5f, 67.5f);
                break;
            case ArrowDirection.Right:
                CalculateDirectionOfPOIs(allPOIs, dutchNames, poiVisibility, 67.5f, 112.5f);
                break;
            case ArrowDirection.DownRight:
                CalculateDirectionOfPOIs(allPOIs, dutchNames, poiVisibility, 112.5f, 157.5f);
                break;
            case ArrowDirection.Down:
                CalculateDirectionOfPOIs(allPOIs, dutchNames, poiVisibility, 157.5f, -157.5f, true);
                break;
            case ArrowDirection.DownLeft:
                CalculateDirectionOfPOIs(allPOIs, dutchNames, poiVisibility, -157.5f, -112.5f);
                break;
        }
    }

    private void CalculateDirectionOfPOIs(List<GameObject> allPOIs, List<string> dutchNames, List<bool> poiVisibility, float minAngle, float maxAngle, 
        bool topplingAngle = false)
    {
        List<List<string>> countsForRoles = new List<List<string>>();

        for (int i = 0; i < allPOIs.Count; i++)
        {
            if (!poiVisibility[i]) { continue; }

            Vector3 dir = allPOIs[i].transform.position - table.position;
            float angle = Vector3.Angle(table.forward, dir);
            Vector3 relativePoint = table.InverseTransformPoint(allPOIs[i].transform.position);
            if (relativePoint.x < 0)
            {
                angle = Mathf.Abs(angle) * -1;
            }
            else if (relativePoint.x > 0)
            {
                angle = Mathf.Abs(angle);
            }

            if (topplingAngle)
            {
                if (angle > minAngle && !allPOIs[i].activeSelf || angle <= maxAngle && !allPOIs[i].activeSelf)
                {
                    AddRolesToLists(countsForRoles, dutchNames, i);
                }
            }
            else
            {
                if (angle > minAngle && angle <= maxAngle && !allPOIs[i].activeSelf)
                {
                    AddRolesToLists(countsForRoles, dutchNames, i);
                }
            }
        }

        poiIndicationText.text = "";
        for (int i = 0; i < countsForRoles.Count; i++)
        {
            poiIndicationText.text += countsForRoles[i][0] + ": " + countsForRoles[i].Count + "\n";
        }
    }

    private void AddRolesToLists(List<List<string>> countsForRoles, List<string> dutchNames, int index)
    {
        List<string> roleList = null;

        for (int j = 0; j < countsForRoles.Count; j++)
        {
            if (countsForRoles[j].Contains(dutchNames[index]))
            {
                roleList = countsForRoles[j];
                break;
            }
        }

        if (roleList == null)
        {
            roleList = new List<string>();
            countsForRoles.Add(roleList);
        }

        roleList.Add(dutchNames[index]);
    }
}
