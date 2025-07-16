using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AiStateId
{
    ChasePlayer, 
    Destroy,
    Idle,
    AttackPlayer,
    Shooting,
    Patrol,      // новий стан
    Capture,     // новий стан
    ZoneControl  // стан перебування у зоні
}
public interface AIState
{
    AiStateId GetId();
    void Enter(AIAgent agent);
    void Update(AIAgent agent);
    void Exit(AIAgent agent);
}

// Видалено класи AIPatrolState і AICaptureState, вони будуть у окремих файлах
