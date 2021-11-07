using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breakout
{
    public class Brick : MonoBehaviour
    {

        const int brickScore = 100;

        // Start is called before the first frame update
        void Start()
        {
            Camera.main.GetComponent<Game>().OnRestart += Restart;
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
}
