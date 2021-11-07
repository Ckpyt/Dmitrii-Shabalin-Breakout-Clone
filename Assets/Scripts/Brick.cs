using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breakout
{
    public class Brick : NetworkBehaviour
    {

        const int brickScore = 100;

        [SyncVar]
        public Color color;

        // Start is called before the first frame update
        void Start()
        {
            Camera.main.GetComponent<Game>().OnRestart += Restart;
            GetComponent<MeshRenderer>().material.color = color;
        }



        void Restart()
        {
            Destroy(gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name != Ball.ballName || collision.gameObject.GetComponent<Ball>()?.IsLaunched == false) return;

            Camera.main.GetComponent<Game>().AddScores(brickScore);
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
            catch (NullReferenceException) //happens when the game closed or restarted by Unity
            {
                Debug.Log("NullReferenceException");
            }

        }
    }
}
