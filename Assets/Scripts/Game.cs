using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout{
    public class Game : MonoBehaviour
    {
        public delegate void RestartEvent();
        public RestartEvent OnRestart;

        #region DebugButtons
        public GameObject SaveButton;
        public GameObject LoadButton;
        public GameObject RestartButton;
        #endregion

        const string JsonFileName = "Level.json";

        // Start is called before the first frame update
        void Start()
        {
            BrickGenerator.PlaceBricksRandom();

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
            BrickGenerator.LoadFromJson(JsonFileName);
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
