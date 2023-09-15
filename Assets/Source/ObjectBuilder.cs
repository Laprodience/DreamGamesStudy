using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBuilder : MonoBehaviour
{
    private AreaManager areaManager;
    private Objective targetObjective;

    public int objectIndex;

    void Start()
    {
        areaManager = FindObjectOfType<AreaManager>();
    }

    private void OnMouseDown()
    {
        targetObjective = areaManager.objectives[objectIndex];

        Instantiate(areaManager.levelBuildings[objectIndex], new Vector2(targetObjective.xPos, targetObjective.yPos), Quaternion.identity, this.transform.parent);

        SaveBuild();

        Destroy(this.gameObject);
    }

    private void SaveBuild()
    {
        char[] charArray = areaManager.saveText.ToCharArray();

        for (int i = 0; i < areaManager.saveText.Length; i++)
        {
            if (i == objectIndex)
            {
                charArray[i] = '2';

                break;
            }
        }

        areaManager.saveText = new string(charArray);
        System.IO.File.WriteAllText(areaManager.savePath, areaManager.saveText);
    }
}
