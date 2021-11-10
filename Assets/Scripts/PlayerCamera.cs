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
        public GameObject playerScore;
        public GameObject playerPingValue;
        // For making active, if necessary
        public GameObject playerPing; 

        // Start is called before the first frame update
        void Start()
        {

        }

        public void DispalayScore(int score)
        {
            playerScore.GetComponent<Text>().text = score.ToString();
        }

#if DEBUG
        /// <summary>
        /// For debugging multiplayer client only.
        /// </summary>
        public void SetPing()
        {
            playerPing.SetActive(true);
            playerPingValue.GetComponent<Text>().text = NetworkTime.rtt.ToString() + " ms";
        }


        // Update is called once per frame
        void Update()
        {
            SetPing();
        }
#endif
    }
}