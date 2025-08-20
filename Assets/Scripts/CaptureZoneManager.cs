using System.Collections.Generic;
using UnityEngine;

public class CaptureZoneManager : MonoBehaviour
{
    public static CaptureZoneManager Instance;

    private List<CaptureZone> allZones = new List<CaptureZone>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        allZones.AddRange(FindObjectsOfType<CaptureZone>());
    }

    public List<CaptureZone> GetZonesControlledBy(CaptureZone.Team team)
    {
        List<CaptureZone> result = new List<CaptureZone>();
        foreach (var zone in allZones)
        {
            if (zone.currentOwner == team)
                result.Add(zone);
        }
        return result;
    }
}
