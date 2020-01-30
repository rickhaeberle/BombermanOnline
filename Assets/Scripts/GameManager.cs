using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> OnFloorChanged;
    public event Action OnQuitMatchEvent;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void QuitOnlineMatch()
    {
        OnQuitMatchEvent?.Invoke();
    }

    public void ChangeFloor(int floorIdx)
    {
        OnFloorChanged?.Invoke(floorIdx);

    }
}
