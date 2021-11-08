using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout
{
    public class Paddle : NetworkBehaviour
    {
        public static Paddle serverPlayer = null;

        [SyncVar]
        public Ball m_ball;
        const float speed = 5;
        const float xBorder = 5.7f;

        public float paddleYcoord;
        [SyncVar]
        public bool IsBallLaunched = false;

        [SyncVar]
        int m_scores = 0;

        [SyncVar]
        Vector2 position;
        [SyncVar]
        Vector2 velocity;

        public float Scores { get { return m_scores; } }

        // Start is called before the first frame update
        void Start()
        {
            var pos = transform.position;
            pos.y = -4;
            paddleYcoord = transform.position.y;
            Camera.main.GetComponent<Game>().OnRestart += Restart;
            if (isServer)
            {
                if(serverPlayer == null) serverPlayer = this;
                m_ball = Ball.CreateBallForPlayer(this);
                m_ball.paddle = this;
            }
            else
            {
            }
        }


        void Restart()
        {
            Vector3 pos = transform.position;
            pos.y = paddleYcoord;
            pos.x = 0;

            transform.position = pos;
        }

        [Command]
        void ChangePosition(Vector2 pos, Vector2 vel)
        {
            position = pos;
            velocity = vel;
        }

        [Command]
        void LaunchBall(bool launch)
        {
            IsBallLaunched = launch;
        }

        void CheckMovement()
        {
            float curSpeed = 0;
            var pos = transform.position;



            if (Input.GetKeyDown(KeyCode.Space) && m_ball.IsLaunched == false)
            {
                //var network = m_ball.GetComponent<NetworkIdentity>();
                //network.RemoveClientAuthority();
                //network.AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
                LaunchBall(true);
            }

            if (pos.x > -xBorder && Input.GetKey(KeyCode.A)) curSpeed = -speed;
            if (pos.x < xBorder && Input.GetKey(KeyCode.D)) curSpeed = speed;
            var vel = new Vector3(curSpeed, 0);
            GetComponent<Rigidbody>().velocity = vel;

            ChangePosition(pos, vel);
        }

        void FixMovementAndRotation()
        {
            //rotation does not allowed
            transform.rotation = Quaternion.identity;

            //no movement in y or z coordinates allowed.
            var pos = transform.position;
            pos.z = 0;
            pos.y = paddleYcoord;
            transform.position = pos;
        }

        //for disconnecting the player issue
        private void OnDestroy()
        {
            try
            {
                Destroy(m_ball.gameObject);
            }
            catch (NullReferenceException) //normal behaviour when the game has closed by Unity
            {
                Debug.Log("NullReferenceException");
            }
        }

        void LinkToBall()
        {
            if (m_ball == null)
            {
                var balls = FindObjectsOfType<Ball>();
                foreach (var ball in balls)
                    if (ball.transform.position.y + 0.5 > transform.position.y && ball.transform.position.y - 0.5 < transform.position.y)
                    {
                        m_ball = ball;
                        ball.paddle = this;
                    }
            }
        }

        // Update is called once per frame
        void Update()
        {
            LinkToBall();

            if (isLocalPlayer)
            {
                CheckMovement();
            }
            else
            {
                transform.position = position;
                GetComponent<Rigidbody>().velocity = velocity;
            }

            FixMovementAndRotation();
            
        }
    }
}
