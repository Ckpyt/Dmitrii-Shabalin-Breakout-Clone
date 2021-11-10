using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shabalin.Breakout
{
    public class Brick : NetworkBehaviour
    {
        /// <summary> scores for each brick</summary>
        const int brickScore = 100;

        /// <summary> synchronization colors on all the clients </summary>
        [SyncVar]
        public Color color;

        // Start is called before the first frame update
        void Start()
        {
            Camera.main.GetComponent<Game>().OnRestart += OnRestart;
            GetComponent<MeshRenderer>().material.color = color;
        }

        /// <summary> restart the game event </summary>
        void OnRestart(System.Object sender, EventArgs e)
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Collision should react on the player's ball only
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name != Ball.ballName || collision.gameObject.GetComponent<Ball>()?.isLaunched == false) return;

            Camera.main.GetComponent<Game>().AddScores(brickScore);
            //restart the game, if there is no bricks
            if (FindObjectsOfType(typeof(Brick)).Length <= 1)
                Camera.main.GetComponent<Game>().Restart();

            Destroy(gameObject);
        }

        /// <summary>
        /// OnDestroy event. 
        /// Unsubscribe from OnRestart event
        /// </summary>
        void OnDestroy()
        {
            try
            {
                Camera.main.GetComponent<Game>().OnRestart -= OnRestart;
            }
            catch (NullReferenceException) //happens when the game closed or restarted by Unity
            {
                Debug.Log("NullReferenceException");
            }

        }
    }
}
