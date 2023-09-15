using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Level
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public List<string> grid = new List<string>();

    public static Level readLevel(string target)
    {
        string levelData = System.IO.File.ReadAllText(target);

        return JsonUtility.FromJson<Level>(levelData);
    }
}