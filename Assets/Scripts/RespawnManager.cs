using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [System.Serializable]
    public class RespawnQueueEntry
    {
        public CaptureZone.Team team;
        public float respawnTime;
    }

    private List<RespawnQueueEntry> respawnQueue = new List<RespawnQueueEntry>();
    [SerializeField] private float retryDelay = 1.0f; // повторна спроба, якщо поки неможливо заспавнитись

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        for (int i = respawnQueue.Count - 1; i >= 0; i--)
        {
            respawnQueue[i].respawnTime -= Time.deltaTime;
            if (respawnQueue[i].respawnTime <= 0f)
            {
                bool spawned = SpawnBot(respawnQueue[i].team);
                if (spawned)
                {
                    respawnQueue.RemoveAt(i);
                }
                else
                {
                    // Відкласти повторну спробу
                    respawnQueue[i].respawnTime = retryDelay;
                }
            }
        }
    }

    public void AddToRespawnQueue(CaptureZone.Team team, float delay)
    {
        respawnQueue.Add(new RespawnQueueEntry
        {
            team = team,
            respawnTime = delay
        });
    }

    /*private void SpawnBot(Team team)
    {
        // отримуємо всі точки, які належать цій команді
        List<CaptureZone> zones = CaptureZoneManager.Instance.GetZonesControlledBy(team);

        if (zones.Count == 0)
        {
            Debug.Log($"[{team}] не має контрольних точок — респавн відмінено.");
            return;
        }

        // випадковий спавнпоінт серед контрольованих
        CaptureZone chosenZone = zones[Random.Range(0, zones.Count)];
        Transform spawnPoint = chosenZone.GetRandomSpawnPoint();

        if (spawnPoint != null)
        {
            GameObject prefab = (team == Team.Player) ? UnitManager.Instance.playerAllyPrefab : UnitManager.Instance.enemyPrefab;
            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        }
    }*/
    private bool SpawnBot(CaptureZone.Team team)
    {
        if (UnitManager.Instance == null) return false;

        // Перевіряємо наявність контрольованих зон
        var zones = CaptureZoneManager.Instance.GetZonesControlledBy(team);
        if (zones == null || zones.Count == 0)
            return false;

        // Перевіряємо ліміти команди (щоб зберігати однакову кількість учасників)
        if (team == CaptureZone.Team.Player)
        {
            int aliveAllies = UnitManager.Instance.GetAliveAllies();
            int allowedAllies = UnitManager.Instance.GetAllowedAllies();
            if (aliveAllies >= allowedAllies)
                return false;
            UnitManager.Instance.SpawnAlly();
            return true;
        }
        else if (team == CaptureZone.Team.Enemy)
        {
            int aliveEnemies = UnitManager.Instance.GetAliveEnemies();
            int allowedEnemies = UnitManager.Instance.GetAllowedEnemies();
            if (aliveEnemies >= allowedEnemies)
                return false;
            UnitManager.Instance.SpawnEnemy();
            return true;
        }
        return false;
    }
}
