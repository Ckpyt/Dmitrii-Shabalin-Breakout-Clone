using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breakout
{
    public class Ball : MonoBehaviour
    {
        public const string ballName = "Ball";
        //start speed of the ball
        const float speed = 4;
        const float minimalSpeed = 0.2f;

        bool m_launched = false;

        public Paddle paddle;
        public bool IsLaunched { get { return m_launched; } }

        const string ballPrefabPath = "Assets/Prefabs/Ball.prefab";

        // Start is called before the first frame update
        void Start()
        {
            Camera.main.GetComponent<Game>().OnRestart += Restart;
            gameObject.name = ballName;
        }

        /// <summary>
        /// launch a ball with a random direction
        /// </summary>
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
            Object prefab = AssetDatabase.LoadAssetAtPath(ballPrefabPath, typeof(GameObject));
            var ballObj = Instantiate(prefab, Vector2.zero, Quaternion.identity) as GameObject;
            Ball ball = ballObj.GetComponent<Ball>();
            ball.paddle = player;
            return ball;
        }

        // Update is called once per frame
        void Update()
        {
            var pos = transform.position;
            if (m_launched)
            {
                //fixing flying horizontal or vertical
                var vel = GetComponent<Rigidbody>().velocity;

                if (vel.x > -minimalSpeed && vel.x < minimalSpeed) vel.x = pos.x < 0 ? minimalSpeed : -minimalSpeed;
                if (vel.y > -minimalSpeed && vel.y < minimalSpeed) vel.y = pos.y < 0 ? minimalSpeed : -minimalSpeed;

                //fixing flying in the Z coord;
                if (vel.z != 0)
                {
                    vel.z = 0;
                }
                if (pos.z != 0) pos.z = 0;

                //fixing loosing speed on the z direction
                if (vel.magnitude < speed - 0.1f || vel.magnitude > speed + 0.1f)
                {
                    float mag = vel.magnitude;
                    vel.x = speed * vel.x / mag;
                    vel.y = speed * vel.y / mag;
                }

                GetComponent<Rigidbody>().velocity = vel;
            }
            else
            {
                pos = paddle.transform.position;
                pos.y += 0.2f;
            }

            transform.position = pos;
        }
    }
}