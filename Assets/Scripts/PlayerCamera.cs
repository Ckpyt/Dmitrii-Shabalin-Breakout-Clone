using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Breakout
{
    public class PlayerCamera : MonoBehaviour
    {
        //Text fields for filling
        public GameObject m_playerScore;
        public GameObject m_playerPing;
        public GameObject m_playerPingValue;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void SetScore(int score)
        {
            m_playerScore.GetComponent<Text>().text = score.ToString();
        }

#if DEBUG
        /// <summary>
        /// For multiplayer client only.
        /// </summary>
        /// <param name="ping"></param>
        public void SetPing()
        {
            m_playerPing.SetActive(true);
            m_playerPingValue.GetComponent<Text>().text = NetworkTime.rtt.ToString() + " ms";
        }
#endif

        // Update is called once per frame
        void Update()
        {

            SetPing();
        }
    }
}