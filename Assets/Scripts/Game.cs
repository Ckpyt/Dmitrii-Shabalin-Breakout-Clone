using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public delegate void RestartEvent();
    public RestartEvent OnRestart;

    // Start is called before the first frame update
    void Start()
    {
        Brick.PlaceBricksRandom();
    }

    public void Restart()
    {
        OnRestart?.Invoke();
        Brick.PlaceBricksRandom();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
