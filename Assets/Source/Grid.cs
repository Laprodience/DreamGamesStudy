using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class Grid : MonoBehaviour
{
    private SpriteRenderer rend;
    private Level currentLevel;
    private Matches matchHelper;
    private CameraScaler cam;
    private TargetManager targetManager;

    //This dictionary is used for creating objects from .json file.
    private Dictionary<string, int> objectsDict = new Dictionary<string, int>()
    {
        {"r", 0},
        {"g", 1},
        {"b", 2},
        {"y", 3},
        {"t", 4},
        {"bo", 5},
        {"s", 6},
        {"v", 7}
    };

    //This dictionary is used for destroy effects. red and red_tnt (hint version) has the same destroy effect.
    private Dictionary<string, int> tagsDict = new Dictionary<string, int>()
    {
        {"red", 0},
        {"red_tnt", 0},
        {"green", 1},
        {"green_tnt", 1},
        {"blue", 2},
        {"blue_tnt", 2},
        {"yellow", 3},
        {"yellow_tnt", 3},
        {"TNT", 4},
        {"box", 5},
        {"stone", 6},
        {"vase1", 7},
        {"vase2", 7}
    };

    public GameObject[] allTiles;
    public GameObject[] destroyEffects;
    public GameObject[,] gridTiles;
    public GameState currentState;
    public string savePath;
    public string saveText;
    public int width;
    public int height;
    public int levelNumber;

    // Start is called before the first frame update
    void Start()
    {
        currentState = GameState.wait;
        savePath = "Assets/save.txt";
        saveText = System.IO.File.ReadAllText(savePath);
        string levelPath = GetLevelPath(saveText);
        currentLevel = Level.readLevel(levelPath);
        cam = FindObjectOfType<CameraScaler>();
        matchHelper = FindObjectOfType<Matches>();
        targetManager = FindObjectOfType<TargetManager>();
        width = currentLevel.grid_width;
        height = currentLevel.grid_height;
        targetManager.leftMoves = currentLevel.move_count;
        gridTiles = new GameObject[width, height];
        SetupGrid();
        cam.RepositionCamera(width, height);
    }

    private string GetLevelPath(string inText)
    {
        string outPath = "Assets/CaseStudyAssets/Levels/level_";
        int i;

        for(i = 0; i < inText.Length; i++)
        {
            if(inText[i] == '0')
            {
                break;
            }
        }

        levelNumber = i;

        i += 1;

        if(i != 10)
        {
            outPath += "0";
        }

        outPath += i.ToString() + ".json";

        return outPath;
    }

    //Creating grid
    private void SetupGrid()
    {
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                //Creating tile
                int selectedTile;
                Vector2 currentPosition = new Vector2(i, j);

                //Read current tile from .json string to create
                string currentTileStr = currentLevel.grid[width*j + i];

                if(currentTileStr == "rand")
                {
                    //Get index for random RGBY cube
                    selectedTile = Random.Range(0, 4);
                }
                else
                {
                    //Get object's index
                    selectedTile = objectsDict[currentTileStr];
                }

                GameObject currentTile = Instantiate(allTiles[selectedTile], currentPosition, Quaternion.identity);
                currentTile.GetComponent<Tile>().xInGrid = i;
                currentTile.GetComponent<Tile>().yInGrid = j;
                currentTile.transform.parent = this.transform;
                currentTile.name = "(" + i + ", " + j + ")";

                //Setting sortingOrder of the tile so that upper tiles look on top
                rend = currentTile.GetComponent<SpriteRenderer>();
                rend.sortingOrder = j;

                //Add the tile on 2D array
                gridTiles[i, j] = currentTile;
            }
        }

        currentState = GameState.move;
    }

    public void DestroyMatchesAt(int x_pos, int y_pos)
    {
        if (gridTiles[x_pos, y_pos].GetComponent<Tile>().isMatched)
        {
            string curTag = gridTiles[x_pos, y_pos].tag;

            if (curTag == "vase1")
            {
                //Replace it with broken vase
                ReplaceObject(x_pos, y_pos, 8);
            }
            else
            {
                GameObject effect = Instantiate(destroyEffects[tagsDict[curTag]], gridTiles[x_pos, y_pos].transform.position, Quaternion.identity);
                Destroy(effect, 1.5f);
                Destroy(gridTiles[x_pos, y_pos]);
                gridTiles[x_pos, y_pos] = null;
            }
        }
    }

    //Instead of just destroying clicked cubes, we traverse whole grid.
    //This is because, clicked cubes or TNTs will damage other tiles aswell,
    //And we have to destroy them too.
    public void DestroyAllMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (gridTiles[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }

        if(matchHelper.haveHint == true)
        {
            RemoveHints();
            matchHelper.haveHint = false;
        }

        targetManager.UpdateNumbers();
        StartCoroutine(DecreaseRowsCo());
    }

    private IEnumerator DecreaseRowsCo()
    {
        int nullCount = 0;

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                //Tile is empty
                if (gridTiles[i, j] == null)
                {
                    nullCount++;
                }
                //Tile not empty
                else
                {
                    //If type is Box or Stone, they will not fall down
                    //and act as a ground
                    if (gridTiles[i, j].tag == "box" || gridTiles[i, j].tag == "stone")
                    {
                        nullCount = 0;
                    }
                    //For other type of objects
                    else
                    {
                        if (nullCount > 0)
                        {
                            gridTiles[i, j].GetComponent<Tile>().yInGrid -= nullCount;
                            gridTiles[i, j] = null;
                        }
                    }
                }
            }

            nullCount = 0;
        }

        yield return new WaitForSeconds(.5f);

        StartCoroutine(FillBoardCo());
    }

    private void RefillGrid()
    {
        for(int i = 0; i < width; i++)
        {
            //Traversing from up to down. If we reach box or stone, they will act as GROUND
            for (int j = height - 1; j >= 0; j--){
                //Stone and Box will act as ground and we will not fill empty tiles under them
                if (gridTiles[i, j] != null)
                {
                    if (gridTiles[i, j].tag == "box" || gridTiles[i, j].tag == "stone")
                    {
                        break;
                    }
                }
                else
                {
                    Vector2 targetPosition = new Vector2(i, j + 20);
                    int selectedTile = Random.Range(0, 4);
                    GameObject newTile = Instantiate(allTiles[selectedTile], targetPosition, Quaternion.identity) as GameObject;
                    newTile.GetComponent<Tile>().xInGrid = i;
                    newTile.GetComponent<Tile>().yInGrid = j;
                    newTile.transform.parent = this.transform;
                    newTile.name = "(" + i + ", " + j + ")";
                    rend = newTile.GetComponent<SpriteRenderer>();
                    rend.sortingOrder = j;

                    gridTiles[i, j] = newTile;
                }
            }
        }
    }

    private IEnumerator FillBoardCo()
    {
        RefillGrid();
        yield return new WaitForSeconds(.5f);

        bool isGameWon = targetManager.IsGameWon();
        bool isMovesOver = targetManager.IsMovesOver();

        if (isGameWon)
        {
            StartCoroutine(GameWonSequence());
        }
        else if (isMovesOver)
        {
            StartCoroutine(GameOverSequence());
        }
        else
        {
            currentState = GameState.move;
        }
    }

    private IEnumerator GameWonSequence()
    {
        targetManager.CreateStar(0, 516.4825f);
        yield return new WaitForSeconds(1f);

        //Gray out screen and animate star and perfect text
        targetManager.Fade();

        yield return new WaitForSeconds(4f);

        SaveLevelCompletion();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(1f);
        DestroyAllTiles();

        yield return new WaitForSeconds(1.5f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    private void SaveLevelCompletion()
    {
        char[] charArray = saveText.ToCharArray();

        for(int i = 0; i < saveText.Length; i++)
        {
            if(i == levelNumber)
            {
                charArray[i] = '1';

                break;
            }
        }

        saveText = new string(charArray);
        System.IO.File.WriteAllText(savePath, saveText);
    }

    private void DestroyAllTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (gridTiles[i, j] != null)
                {
                    string curTag = gridTiles[i, j].tag;

                    GameObject effect = Instantiate(destroyEffects[tagsDict[curTag]], gridTiles[i, j].transform.position, Quaternion.identity);
                    Destroy(effect, 1.5f);
                    Destroy(gridTiles[i, j]);
                    gridTiles[i, j] = null;
                }
            }
        }
    }

    public void CreateHint(List<GameObject> hintCubes)
    {
        foreach (GameObject currentCube in hintCubes)
        {
            Tile currentTile = currentCube.GetComponent<Tile>();
            int x_pos = currentTile.xInGrid;
            int y_pos = currentTile.yInGrid;
            int newObjectIndex = tagsDict[currentCube.tag] + 9;   //ex: red is 0, red_tnt is 9 in allTiles array

            ReplaceObject(x_pos, y_pos, newObjectIndex);
        }
    }

    public void RemoveHints()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (gridTiles[i, j] != null)
                {
                    string curTag = gridTiles[i, j].tag;
                    if(curTag == "red_tnt" || curTag == "green_tnt" || curTag == "blue_tnt" || curTag == "yellow_tnt")
                    {
                        Tile currentTile = gridTiles[i, j].GetComponent<Tile>();
                        int x_pos = currentTile.xInGrid;
                        int y_pos = currentTile.yInGrid;
                        int newObjectIndex = tagsDict[curTag];

                        ReplaceObject(x_pos, y_pos, newObjectIndex);
                    }
                }
            }
        }
    }

    //This method removes current object in grid and places a new object in its place
    //New object's index is given by object_index.
    public void ReplaceObject(int x_pos, int y_pos, int object_index)
    {
        Vector2 targetPosition = new Vector2(x_pos, y_pos);
        GameObject newTile = Instantiate(allTiles[object_index], targetPosition, Quaternion.identity);
        newTile.GetComponent<Tile>().xInGrid = x_pos;
        newTile.GetComponent<Tile>().yInGrid = y_pos;
        newTile.transform.parent = this.transform;
        newTile.name = "(" + x_pos + ", " + y_pos + ")";
        rend = newTile.GetComponent<SpriteRenderer>();
        rend.sortingOrder = y_pos;

        Destroy(gridTiles[x_pos, y_pos]);
        gridTiles[x_pos, y_pos] = newTile;
    }
}