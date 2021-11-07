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
        bool m_launched = false;

        public Paddle paddle;
        public bool IsLaunched { get { return m_launched; } }

        const string ballPrefabPath = "Assets/Prefabs/Ball.prefab";

        [SyncVar]
        Vector3 position;
        [SyncVar]
        Vector3 velocity;

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
                float direction = ((Random.value * 90) + 45) / 180 * Mathf.PI;
                GetComponent<Rigidbody>().velocity = new Vector2(speed * Mathf.Cos(direction), speed * Mathf.Sin(direction));
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
            m_launched = false;
        }

        public static Ball CreateBallForPlayer(Paddle player)
        {

                GameObject prefab = AssetDatabase.LoadAssetAtPath(ballPrefabPath, typeof(GameObject)) as GameObject;
                NetworkClient.RegisterPrefab(prefab);

                var ballObj = Instantiate(prefab, Vector2.zero, Quaternion.identity) as GameObject;
                Ball ball = ballObj.GetComponent<Ball>();
                ball.paddle = player;
                NetworkServer.Spawn(ballObj);
                return ball;

        }

        void FixingMovement()
        {
            position = transform.position;
            if (m_launched)
            {
                //fixing flying horizontal or vertical
                velocity = GetComponent<Rigidbody>().velocity;

                if (velocity.x > -minimalSpeed && velocity.x < minimalSpeed) velocity.x = position.x < 0 ? minimalSpeed : -minimalSpeed;
                if (velocity.y > -minimalSpeed && velocity.y < minimalSpeed) velocity.y = position.y < 0 ? minimalSpeed : -minimalSpeed;

                //fixing flying in the Z coord;
                if (velocity.z != 0)
                {
                    velocity.z = 0;
                }
                position.z = 0;

                //fixing loosing speed on the z direction
                if (velocity.magnitude < speed - 0.1f || velocity.magnitude > speed + 0.1f)
                {
                    float mag = velocity.magnitude;
                    velocity.x = speed * velocity.x / mag;
                    velocity.y = speed * velocity.y / mag;
                }

                GetComponent<Rigidbody>().velocity = velocity;
            }
            else
            {
                position = paddle.transform.position;
                position.y += 0.2f;
            }

            transform.position = position;
        }


        // Update is called once per frame
        void Update()
        {
            if (isServer)
            {
                FixingMovement();
            }
            else
            {
                transform.position = position;
                GetComponent<Rigidbody>().velocity = velocity;
            }
        }
    }        
    
}