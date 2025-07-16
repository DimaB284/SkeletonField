using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptureZone : MonoBehaviour
{
    public enum Team { Neutral, Player, Enemy }

    public Team currentOwner = Team.Neutral; // Поточний власник зони
    public float captureTime = 5.0f; // Час, необхідний для захоплення
    public Slider playerCaptureProgressBar; // Слайдер для гравця (синій)
    public Slider enemyCaptureProgressBar;  // Слайдер для ворога (червоний)
    public Text captureMessageText;
    public string pointName;
    public GameObject playerFlag, enemyFlag;
    private int playerFlags = 0;
    private int enemyFlags = 0;

    private float playerProgress = 0.0f; // Прогрес гравця
    private float enemyProgress = 0.0f; // Прогрес ворога
    private bool isNeutralizingByPlayer = false, isNeutralizingByEnemy = false; // Чи йде нейтралізація
    public List<Transform> playersInZone = new List<Transform>(); // Гравці в зоні
    public List<Transform> enemiesInZone = new List<Transform>(); // Вороги в зоні

	void Start()
	{
		if (currentOwner == Team.Player)
		{
            playerCaptureProgressBar.maxValue = 100;
            playerProgress = 1.0f;
            playerFlag.SetActive(true);
        }

        if (currentOwner == Team.Enemy)
        {
            enemyCaptureProgressBar.maxValue = 100;
            enemyProgress = 1.0f;
            enemyFlag.SetActive(true);
        }
    }
	void Update()
    {
        UpdateCaptureState();
        UpdateUI();
        UpdateSlidersVisibility();
        //Debug.Log($"Player Progress: {playerProgress}");
        //Debug.Log($"Enemy Progress: {enemyProgress}");
    }

    void UpdateCaptureState()
    {
        if (playersInZone.Count > 0 && enemiesInZone.Count == 0)
        {
            if (currentOwner == Team.Enemy && enemyProgress > 0)
            {
                // Починається нейтралізація ворожого прапора
                isNeutralizingByPlayer = true;
                enemyProgress -= Time.deltaTime / captureTime;
                enemyProgress = Mathf.Clamp(enemyProgress, 0.0f, 1.0f);

                if (enemyProgress <= 0.0f)
                {
                    isNeutralizingByPlayer = false;
                    currentOwner = Team.Neutral;
                    enemyFlag.SetActive(false);
                    ShowCaptureMessage("Enemy flag neutralized at " + pointName + "!");
                }
            }
            else if (currentOwner == Team.Neutral && !isNeutralizingByPlayer && enemyProgress <= 0)
            {
                // Гравець починає захоплення нейтральної точки
                playerProgress += Time.deltaTime / captureTime;
                playerProgress = Mathf.Clamp(playerProgress, 0.0f, 1.0f);

                if (playerProgress >= 1.0f)
                {
                    currentOwner = Team.Player;
                    SetOwner(Team.Player);
                    playerFlag.SetActive(true);
                    // Перевіряємо, чи це остання точка
                    if (!IsLastZone(CaptureZone.Team.Player))
                        ShowCaptureMessage("Your team captured " + pointName + " point!");
                    GameController.Instance.CheckVictoryCondition();
                }
            }
            if (currentOwner == Team.Player && playerProgress > 0 && playerProgress < 1)
			{
                playerProgress += Time.deltaTime / captureTime;
            }
        }
        else if (enemiesInZone.Count > 0 && playersInZone.Count == 0)
        {
            if (currentOwner == Team.Player && playerProgress > 0)
            {
                // Починається нейтралізація гравецького прапора
                isNeutralizingByEnemy = true;
                playerProgress -= Time.deltaTime / captureTime;
                playerProgress = Mathf.Clamp(playerProgress, 0.0f, 1.0f);

                if (playerProgress <= 0.0f)
                {
                    isNeutralizingByEnemy = false;
                    currentOwner = Team.Neutral;
                    playerFlag.SetActive(false);
                    ShowCaptureMessage("Your flag was neutralized at " + pointName + "!");
                }
            }
            else if (currentOwner == Team.Neutral && !isNeutralizingByEnemy && playerProgress <= 0)
            {
                // Ворог починає захоплення нейтральної точки
                enemyProgress += Time.deltaTime / captureTime;
                enemyProgress = Mathf.Clamp(enemyProgress, 0.0f, 1.0f);

                if (enemyProgress >= 1.0f)
                {
                    currentOwner = Team.Enemy;
                    SetOwner(Team.Enemy);
                    enemyFlag.SetActive(true);
                    // Перевіряємо, чи це остання точка
                    if (!IsLastZone(CaptureZone.Team.Enemy))
                        ShowCaptureMessage("Skeletons' team captured " + pointName + " point!");
                    GameController.Instance.CheckVictoryCondition();
                }
            }
			/*if (currentOwner == Team.Enemy && enemyProgress > 0 && enemyProgress < 1)
			{
				enemyProgress += Time.deltaTime / captureTime;
			}*/
		}
    }

    // Додаю допоміжний метод для перевірки, чи це остання точка
    private bool IsLastZone(Team team)
    {
        var zones = GameObject.FindObjectsOfType<CaptureZone>();
        foreach (var zone in zones)
        {
            if (zone != this && zone.currentOwner != team)
                return false;
        }
        return true;
    }

    public void UpdateUI()
    {
        if (playerCaptureProgressBar != null)
        {
            playerCaptureProgressBar.value = playerProgress * 100f; // Масштабування
        }

        if (enemyCaptureProgressBar != null)
        {
            enemyCaptureProgressBar.value = enemyProgress * 100f; // Масштабування
        }
    }

    void UpdateSlidersVisibility()
    {
        if (playersInZone.Count > 0)
        {
            if (currentOwner == Team.Enemy || isNeutralizingByPlayer)
            {
                enemyCaptureProgressBar.gameObject.SetActive(true);
                playerCaptureProgressBar.gameObject.SetActive(false);
            }
            else if (currentOwner == Team.Neutral || currentOwner == Team.Player)
            {
                enemyCaptureProgressBar.gameObject.SetActive(false);
                playerCaptureProgressBar.gameObject.SetActive(true);
            }
        }
        /*else if (enemiesInZone.Count > 0)
        {
            if (currentOwner == Team.Player || isNeutralizing)
            {
                playerCaptureProgressBar.gameObject.SetActive(true);
                enemyCaptureProgressBar.gameObject.SetActive(false);
            }
            else if (currentOwner == Team.Neutral || currentOwner == Team.Enemy)
            {
                playerCaptureProgressBar.gameObject.SetActive(false);
                enemyCaptureProgressBar.gameObject.SetActive(true);
            }
        }*/
        else
        {
            // Якщо немає юнітів у зоні
            playerCaptureProgressBar.gameObject.SetActive(false);
            enemyCaptureProgressBar.gameObject.SetActive(false);
        }
    }

    void ShowCaptureMessage(string message)
    {
        if (captureMessageText != null)
        {
            captureMessageText.text = message;
            captureMessageText.gameObject.SetActive(true);
            if (isNeutralizingByPlayer || currentOwner == Team.Player)
			{
                captureMessageText.color = Color.blue;
            }
            else if (isNeutralizingByEnemy || currentOwner == Team.Enemy)
			{
                captureMessageText.color = Color.red;
            }
            StartCoroutine(HideMessageAfterDelay(3f));
        }
    }

    public IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        captureMessageText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Add(other.transform);
            if (playerProgress > 0 && playerProgress < 1f)
            {
                playerCaptureProgressBar.gameObject.SetActive(true);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            enemiesInZone.Add(other.transform);
            Debug.Log($"[CaptureZone] Enemy {other.name} увійшов у зону {pointName}, enemiesInZone.Count: {enemiesInZone.Count}");
            // Перевести ворога у стан ZoneControl
            var agent = other.GetComponent<AIAgent>();
            if (agent != null)
            {
                agent.stateMachine.ChangeState(AiStateId.ZoneControl);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Remove(other.transform);
            if (currentOwner != Team.Player)
			{
                playerProgress = 0.0f;
                if (currentOwner == Team.Enemy)
				{
                    enemyProgress = 1.0f;
				}
			}
        }
        else if (other.CompareTag("Enemy"))
        {
            enemiesInZone.Remove(other.transform);
            Debug.Log($"[CaptureZone] Enemy {other.name} вийшов із зони {pointName}");
            // Перевести ворога у стан Patrol
            var agent = other.GetComponent<AIAgent>();
            if (agent != null)
            {
                agent.stateMachine.ChangeState(AiStateId.Patrol);
            }
			if (currentOwner != Team.Enemy)
			{
				enemyProgress = 0.0f;
				/*if (currentOwner == Team.Player)
				{
					playerProgress = 1.0f;
				}*/
			}
		}
    }

    public void SetOwner(Team newOwner)
    {
        if (currentOwner != newOwner)
        {
            currentOwner = newOwner;
            GameController.Instance.CheckVictoryCondition();
        }
    }

    public void OnEntityDestroyed(Transform entity, string tag)
    {
        if (tag == "Player" && playersInZone.Contains(entity))
        {
            playersInZone.Remove(entity);
			if (currentOwner != Team.Player)
			{
				playerProgress = 0.0f;
				if (currentOwner == Team.Enemy)
				{
					enemyProgress = 1.0f;
				}
			}
		}
        else if (tag == "Enemy" && enemiesInZone.Contains(entity))
        {
            enemiesInZone.Remove(entity);
			if (currentOwner != Team.Enemy)
			{
				enemyProgress = 0.0f;
			}
		}
    }
}
