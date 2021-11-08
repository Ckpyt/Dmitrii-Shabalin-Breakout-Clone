using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breakout
{
    public class Paddle : NetworkBehaviour
    {
        const string PaddlePrefabPath = "Assets/Prefabs/Paddle.prefab";
        public static Paddle serverPlayer = null;

        [SyncVar]
        public Ball m_ball;
        const float speed = 5;
        const float xBorder = 5.7f;

        [SyncVar]
        public float paddleYcoord;
        [SyncVar]
        public bool IsBallLaunched = false;

        [SyncVar]
        int m_scores = 0;

        [SyncVar]
        Vector2 position;
        [SyncVar]
        Vector2 velocity;
        // for fixing jerky movement;
        [SyncVar]
        float gameTime;


        public float Scores { get { return m_scores; } }

        // Start is called before the first frame update
        void Start()
        {
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

        public override void OnStartServer()
        {
            base.OnStartServer();

            var paddles = FindObjectsOfType<Paddle>();
            int count = 0;
            var pos = transform.position;

            if (pos.y > -1)
                pos.y = -4f;
            transform.position = pos;

            if (paddles.Length > 1) //sometimes, paddles could be in one spawn spot;
                foreach (var paddle in paddles)
                    if (paddle.transform.position.y + 1 > pos.y && paddle.transform.position.y - 1 < pos.y)
                        count++;

            if (count > 1)
                pos = SwitchPlayersSpawn();

            paddleYcoord = pos.y;
        }

        Vector3 SwitchPlayersSpawn()
        {
            var pos = transform.position;
            if (pos.y < -3.5)
                pos.y += 1;
            else
                pos.y -= 1;
            transform.position = pos;

            Debug.Log("switched");
            ChangePosition(pos, Vector2.zero);
            return pos;
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
            gameTime = Game.CalcTime();
        }

        [Command]
        void LaunchBall(bool launch)
        {
            IsBallLaunched = launch;
            m_ball.Launch();
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
                float time = Game.CalcTime();
                if (gameTime + 0.03 > time) //1 / 60fps  ~ 0.03 - should be applied exactly after received, no more then two frames in a row
                {
                    transform.position = position;
                    GetComponent<Rigidbody>().velocity = velocity;
                }
            }

            FixMovementAndRotation();
            
        }
    }
}
