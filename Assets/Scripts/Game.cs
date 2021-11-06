using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    const int maxLayers = 5;
    const float bottomPos = -1;
    const float Height = 1;
    const float leftPos = -4.5f;
    const float minWidth = 0.1f;
    const float maxWidth = 1.5f;
    const float rightPos = 4.5f;
    const float layerWidth = rightPos - leftPos; 
    const int bricksPerLayer = 10;

    // Start is called before the first frame update
    void Start()
    {
        PlaceBricksRandom();
    }

    float maxBrickWidth(float xPos, int currentBrik)
    {
        return ((layerWidth - (xPos - leftPos))) / (bricksPerLayer - currentBrik);
    }

    void LayLayerRandom(float yPosition, Color color)
    {
        float xPos = leftPos;
        float width = 0;
        for (int i = 0; i < bricksPerLayer; i++)
        {
            width = (minWidth + (i < bricksPerLayer - 1 ? Random.value * maxWidth : 0.9f)) * maxBrickWidth(xPos, i);
            Brick.CreateBrick(color, new Vector2(0.2f + xPos + width / 2, yPosition), width);
            xPos += width + 0.1f;
        }
    }

    void PlaceBricksRandom()
    {
        float yPos = bottomPos;
        for(int i = 0; i < maxLayers; i++)
        {
            LayLayerRandom(yPos, DefinedColors.GetColor((Colors)i));
            yPos += Height;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
