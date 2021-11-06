using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    //start speed of the ball
    public const float speed = 4;


    // Start is called before the first frame update
    void Start()
    {
        Launch();
    }

    /// <summary>
    /// launch a ball with a random direction
    /// </summary>
    public void Launch()
    {
        float direction = ((Random.value * 90) + 45) / 180 * Mathf.PI;
        GetComponent<Rigidbody2D>().velocity = new Vector2(speed * Mathf.Cos(direction), speed * Mathf.Sin(direction));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
