using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shabalin.Breakout
{
    /// <summary>
    /// Paddle controller. Also, player's controller.
    /// </summary>
    public class Paddle : NetworkBehaviour
    {
        /// <summary> current player on the client </summary>
        public static Paddle serverPlayer = null;

        /// <summary> the speed of a paddle </summary>
        const float speed = 5;

        [SyncVar]
        public Ball m_ball;

        /// <summary> synchronized height </summary>
        [SyncVar]
        public float paddleYcoord;
        /// <summary> launching the player's ball synchronization </summary>
        [SyncVar]
        public bool isBallLaunched = false;

        /// <summary> synchronized position </summary>
        [SyncVar]
        Vector2 position;
        /// <summary> synchronized velocity </summary>
        [SyncVar]
        Vector2 velocity;
        /// <summary> the last synchronization of position and velocity for fixing jerky movement </summary>
        [SyncVar]
        float gameTime;

        public NetworkIdentity networkIdentity;
        Rigidbody m_rigidbody;

        /// <summary> left and right wall's position </summary>
        float Xleft, Xright;

        // Start is called before the first frame update
        void Start()
        {
            paddleYcoord = transform.position.y;
            m_rigidbody = GetComponent<Rigidbody>();

            Camera.main.GetComponent<Game>().OnRestart += OnRestart;
            networkIdentity = GetComponent<NetworkIdentity>();

            CalcBorders();
            if (isServer)
            {
                if(connectionToClient.connectionId == 0)
                    Camera.main.GetComponent<Game>().StartGame();

                if (serverPlayer == null) serverPlayer = this;
                m_ball = Ball.CreateBallForPlayer(this);
                m_ball.paddle = this;
            }
            
        }

        void CalcBorders()
        {
            var camerScript = Camera.main.GetComponent<PlayerCamera>();
            var size = GetComponent<BoxCollider>().bounds.size;
            var leftSize = camerScript.leftBorder.GetComponent<BoxCollider>().bounds.size;
            var leftPos = camerScript.leftBorder.transform.position;
            var rightSize = camerScript.rightBorder.GetComponent<BoxCollider>().bounds.size;
            var rightPos = camerScript.rightBorder.transform.position;
            Xright = rightPos.x - rightSize.x / 2f - size.x / 2f - 0.1f;
            Xleft = leftPos.x + leftSize.x / 2f + size.x / 2f + 0.1f;
        }

        /// <summary>
        /// Choosing a player's spawn on the server
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();

            var paddles = FindObjectsOfType<Paddle>();
            int count = 0;
            var pos = transform.position;

            float middle = (BrickGenerator.bottomPosition + Camera.main.GetComponent<PlayerCamera>().bottomBorder.transform.position.y) / 2;

            if (pos.y > BrickGenerator.bottomPosition -1)
                pos.y = middle;
            transform.position = pos;

            if (paddles.Length > 1) //sometimes, paddles could be in one spawn spot;
                foreach (var paddle in paddles)
                    if (paddle.transform.position.y + 1 > pos.y && paddle.transform.position.y - 1 < pos.y)
                        count++;

            if (count > 1)
                pos = SwitchPlayersSpawn(middle);

            paddleYcoord = pos.y;
        }

        /// <summary>
        /// choosing another height for a player's paddle
        /// </summary>
        Vector3 SwitchPlayersSpawn(float middle)
        {
            var pos = transform.position;
            if (pos.y < middle)
                pos.y += 1;
            else
                pos.y -= 1;
            transform.position = pos;

            ChangePosition(pos, Vector2.zero);
            return pos;
        }


        void OnRestart(System.Object sender, EventArgs e)
        {
            Vector3 pos = transform.position;
            pos.y = paddleYcoord;
            pos.x = 0;

            transform.position = pos;
        }

        /// <summary>
        /// Synchronization of position and velocity
        /// </summary>
        [Command]
        void ChangePosition(Vector2 pos, Vector2 vel)
        {
            position = pos;
            velocity = vel;
            gameTime = Game.CalcTime();
        }

        /// <summary>
        /// launching ball on the server side
        /// </summary>
        [Command]
        void LaunchBall()
        {
            isBallLaunched = true;
            m_ball.Launch();
        }

        /// <summary>
        /// The ball is attached to the paddle
        /// </summary>
        public void RestartBall()
        {
            isBallLaunched = false;
        }

        /// <summary>
        /// Movement by keyboard, if the paddle within allowed area
        /// </summary>
        void CheckMovement()
        {
            float curSpeed = 0;
            var pos = transform.position;

            if (Input.GetKeyDown(KeyCode.Space) && m_ball.IsLaunched == false)
            {
                LaunchBall();
            }

            if (pos.x > Xleft && Input.GetKey(KeyCode.A)) curSpeed = -speed;
            if (pos.x < Xright && Input.GetKey(KeyCode.D)) curSpeed = speed;
            var vel = new Vector3(curSpeed, 0);
            GetComponent<Rigidbody>().velocity = vel;
            
            ChangePosition(pos, vel);
        }

        /// <summary>
        /// No z movement allowed.
        /// Also, no rotation allowed
        /// </summary>
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
            catch (MissingReferenceException) { } //normal behaviour when the game has closed by Unity
        }

        /// <summary>
        /// Link to a ball while it is not a player's paddle
        /// </summary>
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
                    m_rigidbody.velocity = velocity;
                }
            }
            FixMovementAndRotation();
        }
    }
}
