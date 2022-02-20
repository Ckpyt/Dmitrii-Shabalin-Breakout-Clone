using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;



namespace Shabalin.Breakout
{
    //static class for loading/saving/generating level. 
    static class BrickGenerator
    {
        //classes for save/load level
        #region SaveLoadClasses

        [Serializable]
        class BrickSerializale
        {
            public float positionX;
            public float positionY;
            public float width;

        }

        [Serializable]
        class LayerSerializale
        {
            public float height;
            public Color color;
            public BrickSerializale[] bricks;
        }

        [Serializable]
        class LevelSerializale
        {
            public int layersCount;
            public LayerSerializale[] layers;
        }

        /// <summary>
        /// For sorting width and some math operations
        /// </summary>
        private class bricks : IComparable
        {
            public float width;
            public int pos;

            public virtual int CompareTo(object obj)
            {
                return (this < (obj as bricks)) ? 1 : -1;
            }

            public static bool operator <(bricks left, bricks right)
            {
                return left.width > right.width;
            }

            public static bool operator >(bricks left, bricks right)
            {
                return !(left < right);
            }
        }
        /// <summary>
        /// For sorting by position
        /// </summary>
        private class bricksByPos : bricks{

            public override int CompareTo(object obj)
            {
                return (this < (obj as bricksByPos)) ? 1 : -1;
            }

            public static bool operator <(bricksByPos left, bricksByPos right)
            {
                return left.pos > right.pos;
            }
            public static bool operator >(bricksByPos left, bricksByPos right)
            {
                return !(left < right);
            }
        };

        #endregion

        const string brickName = "Brick";

        #region BrickGenereator constants
        /// <summary> all the bricks height </summary>
        const float height = 0.5f;
        const int maxLayers = 5;
        /// <summary> lowest layer's position </summary>
        public const float bottomPosition = 0;
        /// <summary> left layer's position </summary>
        const float leftPosition = -4.5f;
        /// <summary> right layer's position </summary>
        const float rightPosition = 4.5f;
        const float layerWidth = rightPosition - leftPosition;
        const float brickWidth = (layerWidth - brickDistance * (bricksPerLayer - 1)) / bricksPerLayer;
        const int bricksPerLayer = 10;
        /// <summary> minimal brick width </summary>
        const float minWidth = 0.3f;
        /// <summary> maximum brick width </summary>
        const float maxWidth = 1.5f;
        /// <summary> distance between bricks </summary>
        const float brickDistance = 0.05f;
        #endregion

        /// <summary>
        /// place all the bricks with random width
        /// </summary>
        public static void PlaceBricksRandom()
        {
            float yPos = bottomPosition;
            for (int i = 0; i < maxLayers; i++)
            {
                LayLayerRandomProp(yPos, DefinedColors.GetColor((Colors)i));
                yPos += height;
            }
        }


        /// <summary>
        /// place bricks with random width in one layer
        /// The total summ of all bricks proportions shrinks to required width.
        /// </summary>
        /// <param name="yPosition"> layer's height </param>
        /// <param name="color"> layer's color </param>
        static void LayLayerRandomProp(float yPosition, Color color)
        {

            float xPos;
            var brickByPos = new bricksByPos[bricksPerLayer];
            bricks[] allBricks = brickByPos;

            float width = 0;
            for (int i = 0; i < bricksPerLayer; i++)
            {
                allBricks[i] = new bricksByPos();
                allBricks[i].pos = i;
                allBricks[i].width = UnityEngine.Random.value * (maxWidth - minWidth);
                width += allBricks[i].width;
            }

            float widthMul = (rightPosition - leftPosition - brickDistance * (bricksPerLayer - 1) - minWidth * bricksPerLayer) / (width);
            foreach (var wdt in allBricks)
                wdt.width *= widthMul;

            Array.Sort(allBricks);
            do
            {
                if(allBricks[allBricks.Length - 1].width >= maxWidth - minWidth)
                {
                    float summ = UnityEngine.Random.value * (maxWidth - allBricks[0].width);
                    allBricks[0].width += summ;
                    allBricks.Last().width -= summ;
                }
                Array.Sort(allBricks);
            } while (allBricks[allBricks.Length - 1].width >= maxWidth);


            
            Array.Sort(brickByPos);

            xPos = leftPosition;
            for (int i = 0; i < bricksPerLayer; i++)
            {
                var wdt = brickByPos[i].width;
                CreateBrick(color, new Vector2(brickDistance + xPos + wdt / 2, yPosition), wdt + minWidth);
                xPos += wdt + brickDistance + minWidth;
            }

        }

        /// <summary>
        /// place bricks with random width in one layer
        /// </summary>
        /// <param name="yPosition"> layer's height </param>
        /// <param name="color"> layer's color </param>
        static void LayLayerRandom(float yPosition, Color color)
        {
            float xPos;
            float[] width = new float[bricksPerLayer];

            xPos = leftPosition;

            for (int i = 0; i < bricksPerLayer; i++)
            {
                float xMin = minWidth;
                float xMax = maxWidth;

                float allBricksDistance = brickDistance * (bricksPerLayer - 1 - i);
                float allBricksWidth = rightPosition - xPos - allBricksDistance;

                float middleWidth = allBricksWidth / (bricksPerLayer - i);
                float minMiddleWidth = (minWidth * (bricksPerLayer - i - 1) + allBricksDistance + maxWidth) / (bricksPerLayer - i);
                float maxMiddleWidth = (maxWidth * (bricksPerLayer - i - 1) + allBricksDistance + minWidth) / (bricksPerLayer - i);

                if (middleWidth > minMiddleWidth)
                {
                    if (middleWidth > maxMiddleWidth) 
                    {
                        if (i < bricksPerLayer - 1)
                        {
                            width[i] = maxWidth;
                        }
                        else
                        {
                            width[i] = rightPosition - xPos;
                        }
                    }
                    else
                    {
                        if (middleWidth > brickWidth)
                        {
                            xMax = allBricksWidth - ((bricksPerLayer - i - 1) * minWidth);
                            if (xMax > maxWidth)
                                xMax = maxWidth;
                        }
                        else
                        {
                            xMin = allBricksWidth - ((bricksPerLayer - i - 1) * maxWidth);
                            if (xMin < minWidth)
                                xMin = minWidth;
                        }
                            
                        width[i] = (xMin + ((i < bricksPerLayer - 1) ? UnityEngine.Random.value * (xMax - xMin) : maxWidth - xMin));
                    }
                        
                }
                else
                {
                    if(i < bricksPerLayer - 1)
                    {
                        width[i] = minWidth;
                    }
                    else
                    {
                        width[i] = rightPosition - xPos;
                    }
                }
                xPos += width[i] + brickDistance;
            }



            xPos = leftPosition;
            for (int i = 0; i < bricksPerLayer; i++)
            {
                var wdt = width[i];
                CreateBrick(color, new Vector2(brickDistance + xPos + wdt / 2, yPosition), wdt);
                xPos += wdt + brickDistance;
            }
        }

        /// <summary>
        /// creating a brick with color, position and width
        /// according by the test task, all the bricks should have the same height
        /// </summary>
        static GameObject CreateBrick(Color color, Vector2 position, float width)
        {

            UnityEngine.Object prefab = Camera.main.GetComponent<PrefabHolder>().brickPrefab;
            GameObject brick = GameObject.Instantiate(prefab, position, Quaternion.identity) as GameObject;

            brick.transform.localScale = new Vector3(width, height, 1);
            brick.GetComponent<MeshRenderer>().material.color = color;
            brick.name = brickName;
            brick.GetComponent<Breakout.Brick>().color = color;

            NetworkServer.Spawn(brick);
            return brick;
        }

        /// <summary>
        /// Load all the brick from a save file
        /// </summary>
        public static void LoadFromFile(string fileName)
        {
            try
            {
                LoadFromString(File.ReadAllText(fileName));
            }
            catch(FileNotFoundException)
            {
                Debug.Log("File not found");
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        /// <summary>
        /// Load all the bricks from json string
        /// </summary>
        public static void LoadFromString(string json)
        {
            var allBricks = GameObject.FindObjectsOfType<Breakout.Brick>();
            foreach (var brick in allBricks)
                GameObject.Destroy(brick.gameObject);

            var level = JsonUtility.FromJson<LevelSerializale>(json);
            foreach (var layer in level.layers)
                foreach (var brick in layer.bricks)
                    CreateBrick(layer.color, new Vector2(brick.positionX, brick.positionY), brick.width);
        }

        /// <summary>
        /// Save all the bricks to json string
        /// </summary>
        public static string SceneToJson()
        {
            LevelSerializale lvl = new LevelSerializale();
            var allBricks = GameObject.FindObjectsOfType<Brick>();
            List<Brick> layers = new List<Brick>();

            foreach (var brick in allBricks)
            {
                bool check = true;
                foreach (var layer in layers)
                    check &= brick.transform.position.y + 0.1f < layer.transform.position.y || brick.transform.position.y - 0.1f > layer.transform.position.y;
                if (check) layers.Add(brick);
            }

            lvl.layersCount = layers.Count;
            lvl.layers = new LayerSerializale[lvl.layersCount];
            List<Brick>[] briksByLayers = new List<Brick>[lvl.layersCount];

            for (int i = 0; i < lvl.layersCount; i++)
                briksByLayers[i] = new List<Brick>();

            foreach (var brick in allBricks)
                for (int i = 0; i < lvl.layersCount; i++)
                {
                    var layer = layers[i];
                    if (brick.transform.position.y + 0.1f > layer.transform.position.y && brick.transform.position.y - 0.1f < layer.transform.position.y)
                    {
                        briksByLayers[i].Add(brick);
                        break;
                    }
                }

            for (int i = 0; i < lvl.layersCount; i++)
            {
                lvl.layers[i] = new LayerSerializale();
                var layer = briksByLayers[i];
                var jsonLayer = lvl.layers[i];
                jsonLayer.color = layer[0].GetComponent<MeshRenderer>().material.color;
                jsonLayer.height = layer[0].transform.localScale.y;
                jsonLayer.bricks = new BrickSerializale[layer.Count];
                for (int ix = 0; ix < layer.Count; ix++)
                {
                    jsonLayer.bricks[ix] = new BrickSerializale();
                    var brick = layer[ix];
                    var jsonBrick = jsonLayer.bricks[ix];
                    jsonBrick.positionX = brick.transform.position.x;
                    jsonBrick.positionY = brick.transform.position.y;
                    jsonBrick.width = brick.transform.localScale.x;
                }
            }
            return JsonUtility.ToJson(lvl);
        }

        /// <summary>
        /// save all the brick in the save file
        /// </summary>
        /// <param name="fileName">path for saving</param>
        public static void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, SceneToJson());
        }

    }
}
