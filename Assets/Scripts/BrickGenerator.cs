using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;



namespace Breakout
{
    //static class for loading/saving/generating level. 
    static class BrickGenerator
    {
        //classes for save/load level
        #region SaveLoadClasses

        [System.Serializable]
        class Brick
        {
            public float positionX;
            public float positionY;
            public float width;

        }

        [System.Serializable]
        class Layer
        {
            public float height;
            public Color color;
            public Brick[] bricks;
        }

        [System.Serializable]
        class Level
        {
            public int layersCount;
            public Layer[] layers;
        }

        #endregion

        const string prefabPath = "Assets/Prefabs/Brick.prefab";
        const string brickName = "Brick";

        #region BrickGenereator constants
        const float height = 0.5f;
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

        static float maxBrickWidth(float xPos, int currentBrik)
        {
            return ((layerWidth - (xPos - leftPos))) / (bricksPerLayer - currentBrik);
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

        static void LayLayerRandom(float yPosition, Color color)
        {
            float xPos = leftPos;
            float width = 0;
            for (int i = 0; i < bricksPerLayer; i++)
            {
                width = (minWidth + (i < bricksPerLayer - 1 ? UnityEngine.Random.value * maxWidth : 1 - minWidth)) * maxBrickWidth(xPos, i);
                CreateBrick(color, new Vector2(brickDistance + xPos + width / 2, yPosition), width);
                xPos += width + brickDistance;
            }
        }

        static GameObject CreateBrick(Color color, Vector2 position, float width)
        {
            UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
            GameObject brick = GameObject.Instantiate(prefab, position, Quaternion.identity) as GameObject;
            brick.transform.localScale = new Vector3(width, height, 1);
            brick.GetComponent<MeshRenderer>().material.color = color;
            brick.name = brickName;
            return brick;
        }


        public static void LoadFromJson(string filename)
        {
            var allBricks = GameObject.FindObjectsOfType<Breakout.Brick>();
            foreach (var brick in allBricks)
                GameObject.Destroy(brick.gameObject);

            var level = JsonUtility.FromJson<Level>(File.ReadAllText(filename));
            foreach (var layer in level.layers)
                foreach (var brick in layer.bricks)
                    CreateBrick(layer.color, new Vector2(brick.positionX, brick.positionY), brick.width);
        }

        public static void SaveToJson(string filename)
        {
            Level lvl = new Level();
            var allBricks = GameObject.FindObjectsOfType<Breakout.Brick>();
            List<Breakout.Brick> layers = new List<Breakout.Brick>();

            foreach(var brick in allBricks)
            {
                bool check = true;
                foreach (var layer in layers)
                    check &= brick.transform.position.y + 0.1f < layer.transform.position.y || brick.transform.position.y - 0.1f > layer.transform.position.y;
                if (check) layers.Add(brick);
            }

            lvl.layersCount = layers.Count;
            lvl.layers = new Layer[lvl.layersCount];
            List<Breakout.Brick>[] briksByLayers = new List<Breakout.Brick>[lvl.layersCount];

            for(int i = 0; i < lvl.layersCount; i++)
                briksByLayers[i] = new List<Breakout.Brick>();

            foreach (var brick in allBricks)
                for (int i = 0; i < lvl.layersCount; i++) {
                    var layer = layers[i];
                    if (brick.transform.position.y + 0.1f > layer.transform.position.y && brick.transform.position.y - 0.1f < layer.transform.position.y)
                    {
                        briksByLayers[i].Add(brick);
                        break;
                    }
                }

            for(int i = 0; i < lvl.layersCount; i++)
            {
                lvl.layers[i] = new Layer();
                var layer = briksByLayers[i];
                var jsonLayer = lvl.layers[i];
                jsonLayer.color = layer[0].GetComponent<MeshRenderer>().material.color;
                jsonLayer.height = layer[0].transform.localScale.y;
                jsonLayer.bricks = new Brick[layer.Count];
                for(int ix = 0; ix < layer.Count; ix++)
                {
                    jsonLayer.bricks[ix] = new Brick();
                    var brick = layer[ix];
                    var jsonBrick = jsonLayer.bricks[ix];
                    jsonBrick.positionX = brick.transform.position.x;
                    jsonBrick.positionY = brick.transform.position.y;
                    jsonBrick.width = brick.transform.localScale.x;
                }
            }

            File.WriteAllText(filename, JsonUtility.ToJson(lvl));
        }

    }
}
