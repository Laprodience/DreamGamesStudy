using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    private Grid grid;
    private List<Target> targets;

    public GameObject[] possibleTargets;
    public GameObject goal_check;
    public GameObject star_symbol;
    public GameObject fadeOut;
    public GameObject starParticlesEffect;
    public Text leftMovesText;
    public int created;
    public int leftMoves;

    void Start()
    {
        grid = FindObjectOfType<Grid>();
        targets = new List<Target>();

        created = 0;

        int[] targetCounts = CountTargets();

        for (int i = 0; i < targetCounts.Length; i++)
        {
            if(targetCounts[i] == 0)
            {
                targetCounts[i] = -1;
            }
            targets.Add(new Target(possibleTargets[i], targetCounts[i]));
        }

        CreateTargetsOnGame();
        UpdateNumbers();
    }

    public void UpdateNumbers()
    {
        leftMovesText.text = leftMoves.ToString();

        int[] targetCounts = CountTargets();

        for(int i = 0; i < 3; i++)
        {
            if (targetCounts[i] > 0)
            {
                targets[i].targetCount = targetCounts[i];
                targets[i].UpdateText();
            }
            else
            {
                if((created & (1 << i)) != 0)
                {
                    targets[i].targetCount = -1;

                    //update the number with goal_check symbol
                    CreateGoalCheck(targets[i]);

                    //So that this target will not be checked anymore
                    created = created & ~(1 << i);
                }
            }
        }

    }

    public int[] CountTargets()
    {
        int[] targetCounts = new int[3];
        targetCounts[0] = 0;
        targetCounts[1] = 0;
        targetCounts[2] = 0;

        for (int i = 0; i < grid.width; i++)
        {
            for(int j = 0; j < grid.height; j++)
            {
                if(grid.gridTiles[i, j] != null)
                {
                    switch(grid.gridTiles[i, j].tag)
                    {
                        case "box":
                            targetCounts[0]++;
                            break;

                        case "stone":
                            targetCounts[1]++;
                            break;

                        case "vase1":
                            targetCounts[2]++;
                            break;

                        case "vase2":
                            targetCounts[2]++;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        return targetCounts;
    }

    public void CreateTargetsOnGame()
    {
        for(int i = 0; i < 3; i++)
        {
            if(targets[i].targetCount > 0)
            {
                //Bitwise operations to figure out which objects are created
                //It will be one of: 001, 010, 100, 110, 101, 011, 111
                created = created | (1 << i);
            }
        }

        switch (created)
        {
            //Only first target is created: Box
            case 1:
                CreateSingleTarget(targets[0], -300.9f, 531.1f, 90, 90);
                break;

            //Only second target is created: Stone
            case 2:
                CreateSingleTarget(targets[1], -300.9f, 531.1f, 90, 90);
                break;

            //First and second targets are created: Box + Stone
            case 3:
                CreateSingleTarget(targets[0], -356f, 529f, 70, 70);
                CreateSingleTarget(targets[1], -264f, 529f, 70, 70);
                break;

            //Only third target is created: Vase
            case 4:
                CreateSingleTarget(targets[2], -300.9f, 531.1f, 90, 90);
                break;

            //First and third targets are created: Box + Vase
            case 5:
                CreateSingleTarget(targets[0], -356f, 529f, 70, 70);
                CreateSingleTarget(targets[2], -264f, 529f, 70, 70);
                break;

            //Second and third targets are created: Stone + Vase
            case 6:
                CreateSingleTarget(targets[1], -356f, 529f, 70, 70);
                CreateSingleTarget(targets[2], -264f, 529f, 70, 70);
                break;

            //All targets are created: Box + Stone + Vase
            case 7:
                CreateSingleTarget(targets[0], -356f, 575f, 70, 70);
                CreateSingleTarget(targets[1], -260f, 575f, 70, 70);
                CreateSingleTarget(targets[2], -305f, 488f, 70, 70);
                break;
        }
    }

    public void CreateSingleTarget(Target newTarget, float x_pos, float y_pos, int img_size_x, int img_size_y)
    {
        GameObject currentCanvas = Instantiate(newTarget.canvasObject, new Vector2(x_pos, y_pos), Quaternion.identity, this.transform.parent);
        Image targetImage = currentCanvas.GetComponentInChildren<Image>();
        Text targetText = currentCanvas.GetComponentInChildren<Text>();
        targetImage.rectTransform.sizeDelta = new Vector2(img_size_x, img_size_y);
        currentCanvas.transform.localScale = new Vector3(1, 1, 1);
        currentCanvas.transform.localPosition = new Vector3(x_pos, y_pos, 1);

        newTarget.SetProperties(targetImage, targetText);
    }

    public void CreateGoalCheck(Target inTarget)
    {
        GameObject goalCheck = Instantiate(goal_check, new Vector2(inTarget.targetText.transform.position.x, inTarget.targetText.transform.position.y), Quaternion.identity);
        Destroy(inTarget.targetText);
        inTarget.targetText = null;
        inTarget.goalCheck = goalCheck;
    }

    public void CreateStar(float x_pos, float y_pos)
    {
        GameObject starSymbol = Instantiate(star_symbol, new Vector2(x_pos, y_pos), Quaternion.identity, this.transform.parent);
        starSymbol.transform.localPosition = new Vector3(x_pos, y_pos, 0);
    }

    public void Fade()
    {
        GameObject fade = Instantiate(fadeOut, new Vector2(0, 0), Quaternion.identity, this.transform.parent);
        fade.transform.localPosition = new Vector3(0, 0, 0);
        GameObject effect = Instantiate(starParticlesEffect, new Vector2(0, -100), Quaternion.identity, fade.transform);
        effect.transform.localPosition = new Vector3(0, -100, 0);
        effect.transform.localScale = new Vector3(2, 2, 2);
        Destroy(effect, 5f);
    }

    public void DecreaseMoveCount()
    {
        leftMoves--;
    }

    public bool IsMovesOver()
    {
        if(leftMoves <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsGameWon()
    {
        bool success = true;

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].targetCount != -1)
            {
                success = false;
                break;
            }
        }

        return success;
    }
}

public class Target
{
    public GameObject canvasObject;
    public GameObject goalCheck;
    public Image targetImage;
    public Text targetText;
    public int targetCount;

    public Target(GameObject canvasIn, int countIn)
    {
        canvasObject = canvasIn;
        goalCheck = null;
        targetCount = countIn;
    }

    public void SetProperties(Image inImage, Text inText)
    {
        targetImage = inImage;
        targetText = inText;
    }

    public void UpdateText()
    {
        if(targetText != null)
        { 
            targetText.text = targetCount.ToString();
        }
    }
}
