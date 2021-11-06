using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Brick : MonoBehaviour
{
    const string prefabPath = "Assets/Prefabs/Brick.prefab";
    const float height = 1.0f;
    const int brickScore = 100;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static void CreateBrick(Color color, Vector2 position, float width)
    {
        Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
        GameObject brick = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        brick.transform.localScale = new Vector3(width, height);
        brick.GetComponent<SpriteRenderer>().color = color;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name != "Ball") return;
        Camera.main.GetComponent<PlayerCamera>().SetScore(brickScore);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
