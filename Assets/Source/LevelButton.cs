using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public AreaManager areaManager;
    public Text levelText;

    void Start()
    {
        areaManager = FindObjectOfType<AreaManager>();

        if (areaManager.isFinished)
        {
            levelText.text = "Finished";
        }
        else
        {
            levelText.text = "Level " + areaManager.currentLevelNumber.ToString();
        }
    }

    private void OnMouseDown()
    {
        if(levelText.text != "Finished")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelScene");
        }
    }
}
