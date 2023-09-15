using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matches : MonoBehaviour
{
    private Grid grid;

    public float hintDelay;
    public float hintDelaySeconds;
    public bool haveHint;

    void Start()
    {
        grid = FindObjectOfType<Grid>();
        hintDelay = 5;
        hintDelaySeconds = hintDelay;
        haveHint = false;
    }

    void Update()
    {
        hintDelaySeconds -= Time.deltaTime;

        if(hintDelaySeconds <= 0 && haveHint == false)
        {
            ActivateHint();
            SetHintTimer();
        }
    }

    public void SetHintTimer()
    {
        hintDelaySeconds = hintDelay;
    }

    //Recursive function to find set of neighboring objects with same type
    public void FindMatchAtSpot(List<GameObject> cubeList, string inTag, int x_pos, int y_pos)
    {
        if (x_pos >= 0 && x_pos < grid.width && y_pos >= 0 && y_pos < grid.height)
        {
            if(grid.gridTiles[x_pos, y_pos] != null)
            {
                GameObject currentCube = grid.gridTiles[x_pos, y_pos];

                if ((currentCube.tag == inTag || currentCube.tag == inTag + "_tnt") && currentCube.GetComponent<Tile>().isChecked == false)
                {
                    cubeList.Add(currentCube);
                    currentCube.GetComponent<Tile>().isChecked = true;

                    FindMatchAtSpot(cubeList, inTag, x_pos + 1, y_pos); //Traverse right
                    FindMatchAtSpot(cubeList, inTag, x_pos - 1, y_pos); //Traverse left
                    FindMatchAtSpot(cubeList, inTag, x_pos, y_pos + 1); //Traverse up
                    FindMatchAtSpot(cubeList, inTag, x_pos, y_pos - 1); //Traverse down
                }
            }
        }
    }

    private List<GameObject> FindHint()
    {
        List<GameObject> tempList = new List<GameObject>();

        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                if (grid.gridTiles[i, j] != null)
                {
                    string curTag = grid.gridTiles[i, j].tag;
                    if (curTag == "red" || curTag == "green" || curTag == "blue" || curTag == "yellow")
                    {
                        FindMatchAtSpot(tempList, curTag, i, j);

                        foreach (GameObject curObj in tempList)
                        {
                            curObj.GetComponent<Tile>().isChecked = false;
                        }

                        if (tempList.Count >= 5){
                            haveHint = true;

                            return tempList;
                        }
                        else
                        {
                            tempList.Clear();
                        }
                    }
                }
            }
        }

        return tempList;
    }

    private void ActivateHint()
    {
        List<GameObject> hintCubeList = new List<GameObject>();
        hintCubeList = FindHint();

        if(hintCubeList.Count >= 5)
        {
            grid.CreateHint(hintCubeList);
        }
    }
}
