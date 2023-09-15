using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    public float aspectRatio;
    public GameObject grid_outline_prefab;

    void Start()
    {
        aspectRatio = Camera.main.aspect;
    }

    void Update()
    {
        
    }

    //Set camera at the middle of the grid
    public void RepositionCamera(float grid_width, float grid_height)
    {
        //Center camera on grid
        Vector3 cameraPosition = new Vector3((grid_width - 1) / 2, ((grid_height - 1) / 2) + 2, -10);

        //Create grid background centered ad Camera
        GameObject gridOutline = Instantiate(grid_outline_prefab, cameraPosition + new Vector3(0, -2, 10), Quaternion.identity);
        SpriteRenderer rend = gridOutline.GetComponent<SpriteRenderer>();
        
        //Each box is 142 pixels wide, dividing it by constant 69 makes our outline fit them
        float outline_x = (grid_width * 142) / 69;
        //Boxes in grid seem 142 pixel high, but most upper box is 162
        float outline_y = (((grid_height - 1) * 142) + 162) / 69;
        rend.size = new Vector2(outline_x, outline_y);

        transform.position = cameraPosition;

        //Set camera size for grid
        if(grid_width/grid_height > aspectRatio)
        {
            //based on width
            Camera.main.orthographicSize = ((grid_width / 2) + 1) / aspectRatio;
        }
        else
        {
            //based on height
            Camera.main.orthographicSize = (grid_height / 2) + 1;
        }
    }
}
