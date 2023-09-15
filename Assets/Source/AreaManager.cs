using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaManager : MonoBehaviour
{
    public GameObject[] levelBuildings;
    public GameObject lockSymbol;
    public GameObject buildableObject;
    public List<Objective> objectives;
    public string savePath;
    public string saveText;
    public int currentLevelNumber;
    public bool isFinished;

    void Start()
    {
        objectives = new List<Objective>();
        savePath = "Assets/save.txt";
        saveText = System.IO.File.ReadAllText(savePath); ;
        currentLevelNumber = FindLevelNumber();

        if(currentLevelNumber == 11)
        {
            isFinished = true;
        }
        else
        {
            isFinished = false;
        }

        CreateObjectives();
        SetupArea();
    }

    private int FindLevelNumber()
    {
        int levelNumber = 1;

        for (int i = 0; i < saveText.Length; i++)
        {
            if (saveText[i] != '0')
            {
                levelNumber++;
            }
        }

        return levelNumber;
    }

    private void CreateObjectives()
    {
        objectives.Add(new Objective(-0.805f, 2.762f));
        objectives.Add(new Objective(1.303f, 1.061f));
        objectives.Add(new Objective(-1.404f, -0.363f));
        objectives.Add(new Objective(0.542f, 1.319f));
        objectives.Add(new Objective(1.755f, 0.424f));
        objectives.Add(new Objective(0.702f, -0.425f));
        objectives.Add(new Objective(1.383f, -0.992f));
        objectives.Add(new Objective(1.701f, -1.612f));
        objectives.Add(new Objective(-0.166f, -1.992f));
        objectives.Add(new Objective(0.02f, -1.593f));

        for (int i = 0; i < saveText.Length; i++)
        {
            if (saveText[i] == '0')
            {
                objectives[i].Setup(lockSymbol, 0);
            }
            else if (saveText[i] == '1')
            {
                objectives[i].Setup(buildableObject, 1);
            }
            else if (saveText[i] == '2')
            {
                objectives[i].Setup(levelBuildings[i], 2);
            }
        }
    }

    private void SetupArea()
    {
        for(int i = 0; i < objectives.Count; i++)
        {
            GameObject currentGameObject = Instantiate(objectives[i].objectInArea, new Vector2(objectives[i].xPos, objectives[i].yPos), Quaternion.identity, this.transform);

            //Set number on build icon
            if (objectives[i].state == 1)
            {
                Canvas currentCanvas = currentGameObject.GetComponentInChildren<Canvas>();
                Text targetText = currentCanvas.GetComponentInChildren<Text>();
                targetText.text = (i + 1).ToString();
                currentGameObject.GetComponent<ObjectBuilder>().objectIndex = i;
            }
        }
    }
}

public class Objective
{
    public GameObject objectInArea;
    public int state;
    public float xPos;
    public float yPos;

    public Objective(float inX, float inY)
    {
        xPos = inX;
        yPos = inY;
    }

    public void Setup(GameObject inObject, int inState)
    {
        objectInArea = inObject;
        state = inState;
    }
}
