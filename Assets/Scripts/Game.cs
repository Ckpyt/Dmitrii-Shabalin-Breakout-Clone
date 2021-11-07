using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout{
    public class Game : NetworkBehaviour
    {
        public delegate void RestartEvent();
        public RestartEvent OnRestart;

        [SyncVar]
        int m_scores = 0;

        [SyncVar]
        string sceneJson;

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
                //sceneJson = BrickGenerator.SceneToJson();
            }
            else
            {
                //BrickGenerator.LoadFromString(sceneJson);
            }

#if DEBUG
            SaveButton.SetActive(true);
            LoadButton.SetActive(true);
            RestartButton.SetActive(true);
#endif

        }

        public void Restart()
        {
            OnRestart?.Invoke();
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
                Camera.main.GetComponent<PlayerCamera>().SetScore(m_scores);
            }
        }

        public void SaveLevelToJson()
        {
            BrickGenerator.SaveToJson(JsonFileName);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
