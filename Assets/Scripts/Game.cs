using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shabalin.Breakout
{
    public class Game : NetworkBehaviour
    {
        public delegate void RestartEventHandler(object sender, EventArgs e);
        public RestartEventHandler OnRestart;

        /// <summary>  Synchronized scores </summary>
        [SyncVar]
        int m_scores = 0;

        #region DebugButtons
        public GameObject SaveButton;
        public GameObject LoadButton;
        public GameObject RestartButton;
        #endregion

        const string JsonFileName = "Level.json";

        // Start is called before the first frame update
        void Start()
        {
            if (isServer)
            {
                BrickGenerator.PlaceBricksRandom();
            }

#if DEBUG
            SaveButton.SetActive(true);
            LoadButton.SetActive(true);
            RestartButton.SetActive(true);
#endif

        }

        /// <summary>
        /// Restart the game. Should be called on the server only
        /// </summary>
        public void Restart()
        {
            OnRestart?.Invoke(this, null);
            BrickGenerator.PlaceBricksRandom();
        }


        public void LoadLevelFromJson()
        {
            BrickGenerator.LoadFromFile(JsonFileName);
        }

        public void AddScores(int Scores)
        {
            if (isServer)
            {
                m_scores += Scores;
            }
        }

        public void SaveLevelToJson()
        {
            BrickGenerator.SaveToFile(JsonFileName);
        }

        // Update is called once per frame
        void Update()
        {
            Camera.main.GetComponent<PlayerCamera>().DispalayScore(m_scores);
        }

        /// <summary>
        /// Calculate time in my format for catching when the last packet income
        /// </summary>
        public static float CalcTime()
        {
            return (System.DateTime.Now.Second + System.DateTime.Now.Minute * 60) + (float)System.DateTime.Now.Millisecond / 1000f;
        }
    }
}
