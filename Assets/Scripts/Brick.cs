using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Brick : MonoBehaviour
{
    const string prefabPath = "Assets/Prefabs/Brick.prefab";
    const string brickName = "Brick";

    #region BrickGenereator constants
    const float height = 0.5f;
    const int brickScore = 100;
    const int maxLayers = 5;
    const float bottomPos = -1;
    const float leftPos = -4.5f;
    const float minWidth = 0.1f;
    const float maxWidth = 1.5f;
    const float rightPos = 4.5f;
    const float layerWidth = rightPos - leftPos;
    const int bricksPerLayer = 10;
    const float brickDistance = 0.05f;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.GetComponent<Game>().OnRestart += Restart;
    }

    static float maxBrickWidth(float xPos, int currentBrik)
    {
        return ((layerWidth - (xPos - leftPos))) / (bricksPerLayer - currentBrik);
    }

    static void LayLayerRandom(float yPosition, Color color)
    {
        float xPos = leftPos;
        float width = 0;
        for (int i = 0; i < bricksPerLayer; i++)
        {
            width = (minWidth + (i < bricksPerLayer - 1 ? Random.value * maxWidth : 1 - minWidth)) * maxBrickWidth(xPos, i);
            CreateBrick(color, new Vector2(brickDistance + xPos + width / 2, yPosition), width);
            xPos += width + brickDistance;
        }
    }

    public static void PlaceBricksRandom()
    {
        float yPos = bottomPos;
        for (int i = 0; i < maxLayers; i++)
        {
            LayLayerRandom(yPos, DefinedColors.GetColor((Colors)i));
            yPos += height;
        }
    }

    public static void CreateBrick(Color color, Vector2 position, float width)
    {
        Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
        GameObject brick = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        brick.transform.localScale = new Vector3(width, height, 1);
        brick.GetComponent<MeshRenderer>().material.color = color;
        brick.name = brickName;
    }

    void Restart()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Ball") return;
        Paddle.AddScores(brickScore);

        //restart the game, if there is no bricks
        if (FindObjectsOfType(typeof(Brick)).Length <= 1)
            Camera.main.GetComponent<Game>().Restart();

        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        try
        {
            Camera.main.GetComponent<Game>().OnRestart -= Restart;
        }
        catch (System.NullReferenceException) //happens then the game closed or restarted by Unity
        { 
        }

    }
}
