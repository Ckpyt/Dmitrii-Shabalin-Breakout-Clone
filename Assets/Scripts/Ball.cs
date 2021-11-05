using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
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
        bool toLeft = (Random.value * 2) < 1;
        GetComponent<Rigidbody2D>().velocity = new Vector2((toLeft ? -speed : speed), speed);    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
