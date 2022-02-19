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

        Game game;

        // Start is called before the first frame update
        void Start()
        {
            game = Camera.main.GetComponent<Game>();
            game.OnRestart += OnRestart;
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
        [Server]
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name != Ball.ballName || collision.gameObject.GetComponent<Ball>()?.IsLaunched == false) return;

            game.AddScores(brickScore);
            //restart the game, if there is no bricks
            if (FindObjectsOfType(typeof(Brick)).Length <= 1)
                game.Restart();

            DestroyBrick();
        }

        [ClientRpc]
        void DestroyBrick()
        {
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
                game.OnRestart -= OnRestart;
            }
            catch (NullReferenceException) //happens when the game closed or restarted by Unity
            {
                Debug.Log("NullReferenceException");
            }

        }
    }
}
