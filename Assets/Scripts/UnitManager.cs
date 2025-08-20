using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    [Header("Prefabs")]
    public GameObject allyPrefab;
    public GameObject enemyPrefab;

    [Header("Team Settings")]
    public int teamSize = 2; // приклад: 5 на 5

    private List<GameObject> allies = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SpawnInitialTeams();
    }

    private void SpawnInitialTeams()
    {
        // Спавнимо союзників (teamSize - 1 бо є гравець)
        for (int i = 0; i < GetAllowedAllies(); i++)
        {
            SpawnAlly();
        }

        // Спавнимо ворогів
        for (int i = 0; i < GetAllowedEnemies(); i++)
        {
            SpawnEnemy();
        }
    }

    public void SpawnAlly()
    {
        if (allies.Count >= GetAllowedAllies()) return;
        var zones = CaptureZoneManager.Instance.GetZonesControlledBy(CaptureZone.Team.Player);
        if (zones.Count == 0) return;

        CaptureZone zone = zones[Random.Range(0, zones.Count)];
        Transform spawn = zone.GetRandomSpawnPoint();

        GameObject ally = Instantiate(allyPrefab, spawn.position, spawn.rotation);
        // Гарантуємо, що агент на NavMesh
        var allyNav = ally.GetComponent<NavMeshAgent>();
        if (allyNav != null && !allyNav.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawn.position, out hit, 2f, NavMesh.AllAreas))
            {
                allyNav.Warp(hit.position);
            }
            else
            {
                ally.transform.position = spawn.position;
            }
        }
        allies.Add(ally);
    }

    public void SpawnEnemy()
    {
        if (enemies.Count >= GetAllowedEnemies()) return;
        var zones = CaptureZoneManager.Instance.GetZonesControlledBy(CaptureZone.Team.Enemy);
        if (zones.Count == 0) return;

        CaptureZone zone = zones[Random.Range(0, zones.Count)];
        Transform spawn = zone.GetRandomSpawnPoint();

        GameObject enemy = Instantiate(enemyPrefab, spawn.position, spawn.rotation);
        // Гарантуємо, що агент на NavMesh
        var enemyNav = enemy.GetComponent<NavMeshAgent>();
        if (enemyNav != null && !enemyNav.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawn.position, out hit, 2f, NavMesh.AllAreas))
            {
                enemyNav.Warp(hit.position);
            }
            else
            {
                enemy.transform.position = spawn.position;
            }
        }
        enemies.Add(enemy);
    }

    public void OnUnitDeath(CaptureZone.Team team)
    {
        // додаємо в чергу на респавн
        if (team == CaptureZone.Team.Player)
        {
            RespawnManager.Instance.AddToRespawnQueue(CaptureZone.Team.Player, 5f);
        }
        else if (team == CaptureZone.Team.Enemy)
        {
            RespawnManager.Instance.AddToRespawnQueue(CaptureZone.Team.Enemy, 5f);
        }
    }

    public int GetAliveAllies() => allies.Count;
    public int GetAliveEnemies() => enemies.Count;

    // Дозволені ліміти: у команді гравця один слот займає сам гравець
    public int GetAllowedAllies() => Mathf.Max(0, teamSize - 1);
    public int GetAllowedEnemies() => teamSize;

    public void UnregisterAgent(AIAgent agent)
    {
        if (agent == null) return;
        if (agent.faction == AIAgent.Faction.Ally)
        {
            allies.Remove(agent.gameObject);
        }
        else if (agent.faction == AIAgent.Faction.Enemy)
        {
            enemies.Remove(agent.gameObject);
        }
    }
}
