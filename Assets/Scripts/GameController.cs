using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public Text winMessageText;
    public Image winPanel; // Додай це поле у GameController
    CaptureZone captureZone;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CheckVictoryCondition()
    {
        CaptureZone[] zones = FindObjectsOfType<CaptureZone>();
        Debug.Log($"Number Of Zones : {zones.Length}");
        if (zones.Length == 0) return;

        CaptureZone.Team capturedBy = zones[0].currentOwner;

        if (capturedBy == CaptureZone.Team.Neutral) return;

        foreach (var zone in zones)
        {
            if (zone.currentOwner != capturedBy)
            {
                return; 
            }
        }

        EndGame(capturedBy);
    }
    void ShowWinMessage(CaptureZone.Team winningTeam)
    {
        if (winPanel != null)
        {
            winPanel.gameObject.SetActive(true);
            if (winningTeam == CaptureZone.Team.Player)
                winPanel.color = new Color(0f, 0.3f, 1f, 0.5f); // синя напівпрозора
            else
                winPanel.color = new Color(1f, 0f, 0f, 0.5f); // червона напівпрозора
        }
        if (winMessageText != null)
        {
            winMessageText.gameObject.SetActive(true);
            winMessageText.color = Color.white;
            winMessageText.alignment = TextAnchor.MiddleCenter;
            winMessageText.fontSize = 64;
            winMessageText.text = (winningTeam == CaptureZone.Team.Player) ? "Your Team Won!" : "Skeletons' Team Won!";
        }
    }

    void EndGame(CaptureZone.Team winningTeam)
    {
        Debug.Log($"Команда {winningTeam} перемогла!");
        ShowWinMessage(winningTeam);
        StartCoroutine(EndGameSequence());
    }

    IEnumerator EndGameSequence()
    {
        yield return new WaitForSeconds(3f);
        yield return new WaitForSeconds(2f);
        if (winPanel != null) winPanel.gameObject.SetActive(false);
        if (winMessageText != null) winMessageText.gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Quit();
        }
    }
    public void Quit()
    {
       Application.Quit();
    }
}

