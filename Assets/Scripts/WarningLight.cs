using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningLight : MonoBehaviour
{
	public Light warningLight; // Признач цей компонент у Inspector
	public float blinkSpeed = 0.5f; // Час між миготінням

	void Start()
	{
		StartCoroutine(BlinkLight());
	}

	IEnumerator BlinkLight()
	{
		while (true)
		{
			warningLight.enabled = !warningLight.enabled; // Перемикаємо світло
			yield return new WaitForSeconds(blinkSpeed);
		}
	}
}
