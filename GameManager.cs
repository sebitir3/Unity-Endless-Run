using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    CollisionManager cManager;
    ScoreManager sManager;

    public GameObject titleScreen;
    public bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        cManager = GetComponent<CollisionManager>();
        sManager = GetComponent<ScoreManager>();

        //subscribe our PauseGameplay method to our collisionManagers OnObstacleCollision Event
        cManager.OnObstacleCollision += PauseGameplay;
    }

    // Update is called once per frame
    void Update()
    {
        if (titleScreen.activeSelf)
        {
            PauseGameplay();
        }
    }

    public void ResetLevel()
    {
        GameManager.Instance.isPaused = false;
        sManager.CloseLeaderboard();
        RoadManager.Instance.Reset();
        PlayerController.Instance.Reset();
        cManager.enabled = true;
        cManager.rend.enabled = true;
    }

    public void PauseGameplay()
    {
        GameManager.Instance.isPaused = true;
        PlayerController.Instance.active = false;
        RoadManager.Instance.enabled = false;
        cManager.enabled = false;
    }

    public void StartGameplay()
    {
        titleScreen.SetActive(false);
        ResetLevel();
    }
}
