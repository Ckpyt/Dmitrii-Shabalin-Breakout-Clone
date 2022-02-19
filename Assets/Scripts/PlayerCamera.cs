using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shabalin.Breakout
{
    public class PlayerCamera : MonoBehaviour
    {
        //Text fields for filling
        public Text playerScore;
        public Text playerPingValue;
        // For making active, if necessary
        public GameObject playerPing;
        public GameObject leftBorder;
        public GameObject rightBorder;
        public GameObject bottomBorder;
        public GameObject topBorder;

        public void DispalayScore(int score)
        {
            playerScore.text = score.ToString();
        }

#if DEBUG
        /// <summary>
        /// For debugging multiplayer client only.
        /// </summary>
        public void SetPing()
        {
            playerPing.SetActive(true);
            playerPingValue.text = NetworkTime.rtt.ToString() + " ms";
        }


        // Update is called once per frame
        void Update()
        {
            SetPing();
        }
#endif
    }
}