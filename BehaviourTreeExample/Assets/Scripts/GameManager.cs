using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private TMP_Text timerText;
    private float timer;

    private void Start()
    {
        Instance = this;
        Time.timeScale = 1;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if(Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void EndGame(bool result)
    {
        (result ? winUI : loseUI).SetActive(true);
        Time.timeScale = 0;
        timerText.text = GetTimerText(timer);
    }

    public static string GetTimerText(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int percent = (int)(time * 100 % 100);
        return $"Time: {minutes:00}:{seconds:00}.{percent:00}";
    }
}
