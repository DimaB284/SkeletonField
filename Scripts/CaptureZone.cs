using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptureZone : MonoBehaviour
{
    public enum Team { Neutral, Player, Enemy }
    
    public Team currentOwner = Team.Neutral;  // Поточний власник зони
    public float captureTime = 10.0f;         // Час, необхідний для захоплення
    public Image captureProgressBar;          // UI-індикатор захоплення

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
                if (captureProgress >= 1.0f)
                {
                    currentOwner = Team.Player;
                    captureProgress = 1.0f; // Захоплено гравцем
                }
            }
        }
        else if (enemiesInZone.Count > 0 && playersInZone.Count == 0)
        {
            // Захоплює ворог
            if (currentOwner != Team.Enemy)
            {
                captureProgress -= Time.deltaTime / captureTime;
                if (captureProgress <= 0.0f)
                {
                    currentOwner = Team.Enemy;
                    captureProgress = 0.0f; // Захоплено ворогом
                }
            }
        }
    }

    void UpdateUI()
    {
        if (captureProgressBar != null)
        {
            captureProgressBar.fillAmount = captureProgress;
        }
    }

    // Перевірка входу гравців/ворогів у зону захоплення
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Add(other.transform);
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
        }
        else if (other.CompareTag("Enemy"))
        {
            enemiesInZone.Remove(other.transform);
        }
    }
}
