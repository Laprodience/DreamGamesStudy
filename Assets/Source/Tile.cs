using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector2 tempPosition;
    private Grid grid;
    private Matches matchHelper;
    private TargetManager targetManager;
    private SpriteRenderer rend;
    private List<GameObject> adjCubes;

    public int xInGrid;
    public int yInGrid;
    public int xTarget;
    public int yTarget;
    public bool isMatched;
    public bool isChecked;

    // Start is called before the first frame update
    void Start()
    {
        grid = FindObjectOfType<Grid>();
        matchHelper = FindObjectOfType<Matches>();
        targetManager = FindObjectOfType<TargetManager>();
        rend = this.gameObject.GetComponent<SpriteRenderer>();
        adjCubes = new List<GameObject>();

        isMatched = false;
        isChecked = false;
    }

    //Update is called once per frame
    void Update()
    {
        xTarget = xInGrid;
        yTarget = yInGrid;

        //Move in y axis
        if (Mathf.Abs(yTarget - transform.position.y) > 0.1)
        {
            tempPosition = new Vector2(transform.position.x, yTarget);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.02f);
            if(grid.gridTiles[xTarget, yTarget] != this.gameObject)
            {
                grid.gridTiles[xTarget, yTarget] = this.gameObject;
            }
        }
        else
        {
            tempPosition = new Vector2(transform.position.x, yTarget);
            transform.position = tempPosition;

            //Set upper boxes on top
            rend.sortingOrder = yInGrid;

            //Set tile names for utility
            this.gameObject.name = "(" + xInGrid + ", " + yInGrid + ")";
        }
    }

    private void OnMouseDown()
    {
        if(grid.currentState == GameState.move)
        {
            FindMatches();
        }
    }

    private void FindMatches()
    {
        matchHelper.FindMatchAtSpot(adjCubes, this.gameObject.tag, xInGrid, yInGrid);
        Debug.Log(adjCubes.Count);

        //Have a match
        if (adjCubes.Count >= 2 || this.gameObject.tag == "TNT")
        {
            grid.currentState = GameState.wait;
            matchHelper.SetHintTimer();
            targetManager.DecreaseMoveCount();
        }
        

        foreach (GameObject currentCube in adjCubes)
        {
            currentCube.GetComponent<Tile>().isChecked = false;

            //TNT explosion
            if(this.gameObject.tag == "TNT")
            {
                //Double or more adjacent TNT -> 7x7 explosion
                if (adjCubes.Count >= 2)
                {
                    CreateBoom(currentCube.GetComponent<Tile>().xInGrid, currentCube.GetComponent<Tile>().yInGrid, -3, 4);
                }
                //Single TNT -> 5x5 explosion
                else
                {
                    CreateBoom(currentCube.GetComponent<Tile>().xInGrid, currentCube.GetComponent<Tile>().yInGrid, -2, 3);
                }
            }
            //Have a match, set current cube as matched
            else if (adjCubes.Count >= 2)
            {
                currentCube.GetComponent<Tile>().isMatched = true;

                //Look for targets to set matched (explosion)
                ApplyDamage(currentCube.GetComponent<Tile>().xInGrid, currentCube.GetComponent<Tile>().yInGrid);
            }
        }

        //Create TNT at selected spot
        if (adjCubes.Count >= 5)
        {
            //Remove damage from below
            //TNT might fall down if an obstacle below is damaged by this cube
            RemoveDamage(xInGrid, yInGrid);

            //Replace with TNT
            grid.ReplaceObject(xInGrid, yInGrid, 4);
        }

        grid.DestroyAllMatches();
        adjCubes.Clear();
    }

    private void ApplyDamage(int x_pos, int y_pos)
    {
        SetDamageFromCubeAtPosition(x_pos + 1, y_pos, true);
        SetDamageFromCubeAtPosition(x_pos - 1, y_pos, true);
        SetDamageFromCubeAtPosition(x_pos, y_pos + 1, true);
        SetDamageFromCubeAtPosition(x_pos, y_pos - 1, true);
    }

    private void RemoveDamage(int x_pos, int y_pos)
    {
        SetDamageFromCubeAtPosition(x_pos, y_pos - 1, false);
    }

    private void SetDamageFromCubeAtPosition(int x_pos, int y_pos, bool damage)
    {
        if (x_pos >= 0 && x_pos < grid.width && y_pos >= 0 && y_pos < grid.height)
        {
            if (grid.gridTiles[x_pos, y_pos] != null)
            {
                //A colored cube can only damage box or vase
                if (grid.gridTiles[x_pos, y_pos].tag == "box" || grid.gridTiles[x_pos, y_pos].tag == "vase1" || grid.gridTiles[x_pos, y_pos].tag == "vase2")
                {
                    grid.gridTiles[x_pos, y_pos].GetComponent<Tile>().isMatched = damage;
                }
            }
        }
    }

    private void CreateBoom(int x_pos, int y_pos, int range_1, int range_2)
    {
        //Set self as matched
        grid.gridTiles[x_pos, y_pos].GetComponent<Tile>().isMatched = true;

        //Traverse 5x5, 7x7
        for (int i = range_1; i < range_2; i++)
        {
            for (int j = range_1; j < range_2; j++)
            {
                if (x_pos + i >= 0 && x_pos + i < grid.width && y_pos + j >= 0 && y_pos + j < grid.height)
                { 
                    if (grid.gridTiles[x_pos + i, y_pos + j] != null)
                    {
                        //If a TNT get hits by another TNT, it will explode too
                        if (grid.gridTiles[x_pos + i, y_pos + j].tag == "TNT" && grid.gridTiles[x_pos + i, y_pos + j].GetComponent<Tile>().isMatched == false)
                        {
                            //I have set it as 5x5 explosion
                            grid.gridTiles[x_pos + i, y_pos + j].GetComponent<Tile>().CreateBoom(x_pos + i, y_pos + j, -2, 3);
                        }

                        grid.gridTiles[x_pos + i, y_pos + j].GetComponent<Tile>().isMatched = true;
                    }
                }   
            }
        }
    }
}