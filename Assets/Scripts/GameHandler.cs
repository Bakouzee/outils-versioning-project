using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    static GameHandler _instance;
    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    
    static bool paused;
    public static bool Paused => paused;

    // UI
    #region UI
    public GameObject gameOverScreen;
    public GameObject winScreen;
    public GameObject pauseScreen;
    #endregion

    // Layers
    #region Layers
    // Enemy layer
    public LayerMask enemyLayer;
    public static LayerMask EnemyLayer => _instance.enemyLayer;

    // Player layer
    public LayerMask playerLayer;
    public static LayerMask PlayerLayer => _instance.playerLayer;
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused) SetPause(true);
            else if(!gameOverScreen.activeSelf && !winScreen.activeSelf)
            {
                SetPause(false);
            }
        }
    }

    public static void SetPause(bool pause)
    {
        paused = pause;
        Time.timeScale = pause ? 0 : 1;

        //if (pause) Time.timeScale = 0;
        //else Time.timeScale = 1;
    }

    #region UI funcs
    public static void SetPauseScreen(bool enable)
    {
        _instance.pauseScreen.SetActive(enable);
    }
    public static void SetGameOverScreen(bool enable)
    {
        _instance.gameOverScreen.SetActive(enable);
    }
    public static void SetWinScreen(bool enable)
    {
        _instance.winScreen.SetActive(enable);
    }
    #endregion
}
