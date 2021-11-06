using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFall : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Ball") return;
        collision.gameObject.GetComponent<Ball>().Restart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
