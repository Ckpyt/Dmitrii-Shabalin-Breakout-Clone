using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shabalin.Breakout
{
    public class Ball : NetworkBehaviour
    {
        public const string ballName = "Ball";

        const float speed = 4;
        const float minimalSpeed = 0.2f;

        /// <summary> if the ball is not launched, it is attached to the player's paddle </summary>
        [SyncVar]
        public bool m_launched = false;

        /// <summary> used only for launching the ball. Fixing jerky movement  </summary>
        [SyncVar]
        Vector3 startVelocity;

        /// <summary> link to player's paddle</summary>
        public Paddle paddle;

        public bool IsLaunched { get { return m_launched; } }

        /// <summary> synchronizing position of a ball </summary>
        [SyncVar]
        Vector3 position;
        /// <summary> synchronizing velocity of a ball </summary>
        [SyncVar]
        Vector3 velocity;

        /// <summary> for fixing jerky movement - last time of getting a packet from the server </summary> 
        [SyncVar]
        float gameTime;

        /// <summary>for fixing loosing speed </summary>
        int frame = 0;


        Rigidbody m_rigidbody;
        public NetworkIdentity networkIdentity;

        // Start is called before the first frame update
        void Start()
        {
            Camera.main.GetComponent<Game>().OnRestart += OnRestart;

            gameObject.name = ballName;
            m_rigidbody = GetComponent<Rigidbody>();
            networkIdentity = GetComponent<NetworkIdentity>();
        }

        /// <summary>
        /// launch a ball with in a random direction
        /// </summary>
        public void Launch()
        {
            if (!m_launched)
            {
                float direction = ((UnityEngine.Random.value * 90) + 45); //as it shown on the test task picture
                float dir = direction / 180 * Mathf.PI;
                velocity = new Vector3(speed * Mathf.Cos(dir), speed * Mathf.Sin(dir));
                startVelocity = velocity;
                m_rigidbody.velocity = velocity;
                Debug.Log("Direction:" + direction);
                ChangePosition(transform.position, velocity);
            }
            m_launched = true;
        }

        /// <summary> detecting touching the bottom wall </summary>
        /// <param name="collision"></param>
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name != "Bottom") return;
            OnRestart(this, null);
        }

        /// <summary> if a ball fall of or the game restarted, the ball should be attached to the player's paddle</summary>
        public void OnRestart(System.Object sender, EventArgs e)
        {
            if (paddle != null)
                paddle.RestartBall();
            m_launched = false;
            
        }

        /// <summary>
        /// Crating a ball for a player. Should works on the same client, where the player is
        /// </summary>
        /// <param name="player">player, who owns the ball</param>
        /// <returns>created ball</returns>
        public static Ball CreateBallForPlayer(Paddle player)
        {
            GameObject prefab = Camera.main.GetComponent<PrefabHolder>().ballPrefab;
            var ballObj = Instantiate(prefab, Vector2.zero, Quaternion.identity) as GameObject;
            Ball ball = ballObj.GetComponent<Ball>();

            ball.paddle = player;
            NetworkServer.Spawn(ballObj);
            return ball;

        }

        /// <summary>
        /// Synchronizing position and velocity between clients and server
        /// </summary>
        [Command]
        void ChangePosition(Vector3 pos, Vector3 vel)
        {
            position = pos;
            velocity = vel;
            gameTime = Game.CalcTime();
        }

        /// <summary>
        /// It is a 2d game in the 3d world. 
        /// So, the ball should not to flay in the Z direction
        /// Also, prevent infinitive loops: flying straight vertical or horizontal
        /// </summary>
        void FixingMovement()
        {
            Vector3 pos = transform.position;
            Vector3 vel = Vector3.zero;
            if (m_launched)
            {

                //fixing flying horizontal or vertical
                vel = m_rigidbody.velocity;

                if (startVelocity.magnitude > 0)
                {
                    vel = startVelocity;
                    startVelocity = Vector3.zero;
                }

                if (vel.y > -minimalSpeed && vel.y < minimalSpeed)
                {
                    frame++;  //if a ball hits something, it could be a 1 frame issue, but the constant issue should be prevented
                    if (frame > 10)
                    {
                        vel.y = pos.y < 0 ? minimalSpeed : -minimalSpeed;
                        frame = 0;
                    }
                }

                if (vel.x > -minimalSpeed && vel.x < minimalSpeed)
                {
                    frame++;
                    if (frame > 10)
                    {
                        vel.x = pos.x < 0 ? minimalSpeed : -minimalSpeed;
                        frame = 0;
                    }
                }

                //fixing flying in the Z coord;
                vel.z = 0;
                pos.z = 0;

                //fixing loosing/increasing speed (by hits, z direction and so on)
                if (vel.magnitude < speed - 0.1f || vel.magnitude > speed + 0.1f)
                {
                    frame++;
                    if (frame > 10)
                    {
                        float mag = vel.magnitude;
                        vel.x = speed * velocity.x / mag;
                        vel.y = speed * velocity.y / mag;
                        frame = 0;
                    }
                }

                m_rigidbody.velocity = vel;
            }
            transform.position = pos;

            networkIdentity.AssignClientAuthority(Paddle.serverPlayer.networkIdentity.connectionToClient);
            ChangePosition(pos, vel);
        }

        /// <summary>
        /// Looking for the nearest paddle. Works on clients without owner
        /// </summary>
        void LinkToPaddle()
        {
            if (paddle == null)
            {
                transform.position = position;
                var Paddles = FindObjectsOfType<Paddle>();
                foreach (var locPaddle in Paddles)
                    if (locPaddle.transform.position.y + 0.5 > transform.position.y && locPaddle.transform.position.y -0.1 < transform.position.y)
                    {
                        paddle = locPaddle;
                        paddle.m_ball = this;
                    }
            }
        }

        /// <summary>
        /// Holding position a little bit higher then the player's paddle
        /// </summary>
        void StickToPaddle()
        {
            var pos = paddle.transform.position;
            pos.y += 0.2f;
            transform.position = pos;
        }

        /// <summary>
        /// changing authority to the player on the client. 
        /// For working commands
        /// </summary>
        void ChangeAutority()
        {
            networkIdentity.RemoveClientAuthority();
            networkIdentity.AssignClientAuthority(Paddle.serverPlayer.networkIdentity.connectionToClient);
            Debug.Log("Authorized");
        }

        // Update is called once per frame
        void Update()
        {
            LinkToPaddle();

            if (isServer && paddle.isServer)
            {
                m_launched = paddle.isBallLaunched;
                var network = networkIdentity;
                if (network.connectionToClient != Paddle.serverPlayer.networkIdentity.connectionToClient)
                {
                    ChangeAutority();
                }
                else
                {
                    if (IsLaunched)
                        FixingMovement();
                    else
                    {
                        StickToPaddle();
                        ChangePosition(paddle.transform.position, m_rigidbody.velocity);
                    }
                }
            }
            else
            {
                if (IsLaunched || paddle == null)
                {
                    float time = Game.CalcTime();
                    if (gameTime + 0.03 > time) //1 / 60fps  ~ 0.03 - should be applied exactly after received, no more then two frames in a row
                    {
                        transform.position = position;
                        m_rigidbody.velocity = velocity;
                    }
                }
                else
                    StickToPaddle();
                
            }

        }
    }        
    
}