using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breakout
{
    public class Ball : NetworkBehaviour
    {
        public const string ballName = "Ball";
        //start speed of the ball
        const float speed = 4;
        const float minimalSpeed = 0.2f;

        [SyncVar]
        public bool m_launched = false;

        [SyncVar]
        Vector3 startVelocity;

        public Paddle paddle;
        public bool IsLaunched { get { return m_launched; } }

        const string ballPrefabPath = "Assets/Prefabs/Ball.prefab";

        [SyncVar]
        Vector3 position;
        [SyncVar]
        Vector3 velocity;
        // for fixing jerky movement;
        [SyncVar]
        float gameTime;

        //for fixing loosing speed
        int frame = 0;

        // Start is called before the first frame update
        void Start()
        {
            Camera.main.GetComponent<Game>().OnRestart += Restart;
            gameObject.name = ballName;
        }

        /// <summary>
        /// launch a ball with a random direction
        /// </summary>
        [Command]
        public void Launch()
        {
            if (!m_launched)
            {
                float direction = ((Random.value * 90) + 45);
                float dir = direction / 180 * Mathf.PI;
                velocity = new Vector3(speed * Mathf.Cos(dir), speed * Mathf.Sin(dir));
                startVelocity = velocity;
                GetComponent<Rigidbody>().velocity = velocity;
                Debug.Log("Direction:" + direction);
            }
            m_launched = true;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name != "Bottom") return;
            Restart();
        }

        //if a ball fall of or the game restarted
        public void Restart()
        {
            if(paddle != null)
                paddle.IsBallLaunched = false;
            m_launched = false;
        }

        public static Ball CreateBallForPlayer(Paddle player)
        {

            GameObject prefab = AssetDatabase.LoadAssetAtPath(ballPrefabPath, typeof(GameObject)) as GameObject;

            var ballObj = Instantiate(prefab, Vector2.zero, Quaternion.identity) as GameObject;
            Ball ball = ballObj.GetComponent<Ball>();
            ball.paddle = player;
            NetworkServer.Spawn(ballObj);
            return ball;

        }

        [Command]
        void ChangePosition(Vector3 pos, Vector3 vel)
        {
            position = pos;
            velocity = vel;
            gameTime = Game.CalcTime();
        }

        
        void FixingMovement()
        {
            Vector3 pos = transform.position;
            Vector3 vel = Vector3.zero;
            if (m_launched)
            {

                //fixing flying horizontal 
                var rig = GetComponent<Rigidbody>();
                vel = rig.velocity;

                if (startVelocity.magnitude > 0)
                {
                    vel = startVelocity;
                    startVelocity = Vector3.zero;
                }

                if (vel.y > -minimalSpeed && vel.y < minimalSpeed)
                {
                    frame++;
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
                if (vel.z != 0)
                {
                    vel.z = 0;
                }
                pos.z = 0;

                //fixing loosing speed on the z direction
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

                GetComponent<Rigidbody>().velocity = vel;
            }
            transform.position = pos;

            GetComponent<NetworkIdentity>().AssignClientAuthority(Paddle.serverPlayer.GetComponent<NetworkIdentity>().connectionToClient);
            ChangePosition(pos, vel);
        }

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

        void StickToPaddle()
        {
            var pos = paddle.transform.position;
            pos.y += 0.2f;
            transform.position = pos;
        }

        void ChangeAutority()
        {
            var network = GetComponent<NetworkIdentity>();
            network.RemoveClientAuthority();
            network.AssignClientAuthority(Paddle.serverPlayer.GetComponent<NetworkIdentity>().connectionToClient);
            Debug.Log("Authorized");
        }

        // Update is called once per frame
        void Update()
        {
            LinkToPaddle();

            if (isServer && paddle.isServer)
            {
                m_launched = paddle.IsBallLaunched;
                var network = GetComponent<NetworkIdentity>();
                if (network.connectionToClient != Paddle.serverPlayer.GetComponent<NetworkIdentity>().connectionToClient)
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
                        ChangePosition(paddle.transform.position, GetComponent<Rigidbody>().velocity);
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
                        GetComponent<Rigidbody>().velocity = velocity;
                    }
                }
                else
                    StickToPaddle();
                
            }

        }
    }        
    
}