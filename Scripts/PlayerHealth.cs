using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health = 10f;
	private float currentHealth;
	public Transform[] respawnPoints; // ����� ���� ��� ��������
	public CharacterController characterController;
    private bool isRespawning = false;
    private bool isDead = false;
	private void Start()
	{
		currentHealth = health;
	}

	public void TakeDamage(float damage)
    {
        if (isDead) return;
		Debug.Log("Took damage!");
		currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        CaptureZone captureZone = FindCaptureZone();

        if (captureZone != null)
        {
            captureZone.OnEntityDestroyed(transform, "Player");
        }

		StartCoroutine(Respawn());
	}

    CaptureZone FindCaptureZone()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f); // ����� ��������
        foreach (Collider collider in colliders)
        {
            CaptureZone zone = collider.GetComponent<CaptureZone>();
            if (zone != null)
            {
                return zone;
            }
        }
        return null; 
    }

	IEnumerator Respawn()
	{
        if (isRespawning) yield break;
        isRespawning = true;

		yield return new WaitForSeconds(1f); //   

		int randomIndex = Random.Range(0, respawnPoints.Length);
		Transform respawnLocation = respawnPoints[randomIndex];

		characterController.enabled = false; // Вимкнути CharacterController перед переміщенням
		transform.position = respawnLocation.position; // Перемістити гравця
		yield return null; // Дочекатися одного кадру
		characterController.enabled = true; // Увімкнути назад

		currentHealth = health; // ³ '
		isDead = false;
        isRespawning = false;
		Debug.Log("Гравець респавнено!");
	}
}

