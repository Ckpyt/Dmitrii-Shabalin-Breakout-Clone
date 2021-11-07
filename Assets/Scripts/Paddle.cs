using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout
{
    public class Paddle : MonoBehaviour
    {
        public Ball m_ball;
        const float speed = 5;
        const float xBorder = 5.7f;

        public float paddleYcoord;

        static int m_scores = 0;

        public static float Scores { get { return m_scores; } }

        // Start is called before the first frame update
        void Start()
        {
            paddleYcoord = transform.position.y;
            Camera.main.GetComponent<Game>().OnRestart += Restart;
        }


        void Restart()
        {
            Vector3 pos = transform.position;
            pos.y = paddleYcoord;
            pos.x = 0;

            transform.position = pos;
        }

        public static void AddScores(int Scores)
        {
            m_scores += Scores;
            Camera.main.GetComponent<PlayerCamera>().SetScore(m_scores);
        }

        // Update is called once per frame
        void Update()
        {
            float curSpeed = 0;
            var pos = transform.position;
            if (Input.GetKeyDown(KeyCode.Space))
                m_ball.Launch();


            if (pos.x > -xBorder && Input.GetKey(KeyCode.A)) curSpeed = -speed;
            if (pos.x < xBorder && Input.GetKey(KeyCode.D)) curSpeed = speed;

            GetComponent<Rigidbody>().velocity = new Vector3(curSpeed, 0);
            //rotation does not allowed
            transform.rotation = Quaternion.identity;
            //no movement in y or z coordinates allowed.
            pos.z = 0;
            pos.y = paddleYcoord;
            transform.position = pos;
        }
    }
}
