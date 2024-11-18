using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptureZone : MonoBehaviour
{
    public enum Team { Neutral, Player, Enemy }

    public Team currentOwner = Team.Neutral;  // Поточний власник зони
    public float captureTime = 5.0f;         // Час, необхідний для захоплення
    public Slider captureProgressBar;          // UI-індикатор захоплення
    public Text captureMessageText;
    public string pointName;
    public GameObject playerFlag, enemyFlag;

    private float captureProgress = 0.0f;     // Поточний прогрес захоплення
    private List<Transform> playersInZone = new List<Transform>();  // Гравці в зоні
    private List<Transform> enemiesInZone = new List<Transform>();  // Вороги в зоні

	void Update()
    {
        UpdateCaptureState();
        UpdateUI();
    }

    void UpdateCaptureState()
    {
        if (playersInZone.Count > 0 && enemiesInZone.Count == 0)
        {
            // Захоплює гравець
            if (currentOwner != Team.Player)
            {
                captureProgress += Time.deltaTime / captureTime;
                if (captureProgressBar.value >= 100.0f)
                {
                    currentOwner = Team.Player;
                    captureProgress = 1.0f; // Захоплено гравцем
                    enemyFlag.gameObject.SetActive(false);
                    playerFlag.gameObject.SetActive(true);
                    ShowCaptureMessage("Your team captured " + pointName + " point!");
                }
            }
        }
        else if (enemiesInZone.Count > 0 && playersInZone.Count == 0)
        {
            // Захоплює ворог
            if (currentOwner != Team.Enemy)
            {
                captureProgress -= Time.deltaTime / captureTime;
                if (captureProgressBar.value <= 0.0f)
                {
                    currentOwner = Team.Enemy;
                    captureProgress = 0.0f; // Захоплено ворогом
                    playerFlag.gameObject.SetActive(false);
                    enemyFlag.gameObject.SetActive(true);
                    ShowCaptureMessage("Skeletons' team captured " + pointName + " point!");
                }
            }
        }
    }

    public void UpdateUI()
    {
        if (captureProgressBar != null)
        {
            captureProgressBar.value += captureProgress;
        }
    }

    void ShowCaptureMessage(string message)
    {
        if (captureMessageText != null)
        {
            captureMessageText.text = message;
            captureMessageText.gameObject.SetActive(true);
            if (currentOwner == Team.Player)
			{
                captureMessageText.color = Color.blue;
            }
            else if (currentOwner == Team.Enemy)
            {
                captureMessageText.color = Color.red;
            }
            StartCoroutine(HideMessageAfterDelay(3f)); // Приховати повідомлення через 3 секунди
        }
    }

    IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        captureMessageText.gameObject.SetActive(false);
    }

    // Перевірка входу гравців/ворогів у зону захоплення
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Add(other.transform);
            captureProgressBar.gameObject.SetActive(true);
        }
        else if (other.CompareTag("Enemy"))
        {
            enemiesInZone.Add(other.transform);
        }
    }

    // Перевірка виходу гравців/ворогів із зони захоплення
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Remove(other.transform);
            captureProgressBar.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Enemy"))
        {
            enemiesInZone.Remove(other.transform);
        }
    }

    public void OnEntityDestroyed(Transform entity, string tag)
    {
        // Видалення юнітів із списку при їхньому знищенні
        if (tag == "Player" && playersInZone.Contains(entity))
        {
            playersInZone.Remove(entity);
        }
        else if (tag == "Enemy" && enemiesInZone.Contains(entity))
        {
            enemiesInZone.Remove(entity);
            Debug.Log("Enemy removed from zone.");
        }
    }
}