using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))] // Asegura que haya un NetworkObject
public class BombSpawner : SimulationBehaviour
{
    [Header("Prefab de Bomba")]
    [Tooltip("Prefab networkeable de la bomba. Debe estar registrado en NetworkRunner > Spawnable Prefabs")]
    [SerializeField] private NetworkPrefabRef bombPrefab;

    [Header("Puntos de Spawn")]
    [Tooltip("Transforms en los que pueden aparecer las bombas")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Configuración de Spawn")]
    [Tooltip("Intervalo, en segundos, entre cada intento de spawn")]
    [SerializeField] private float spawnInterval = 5f;
    [Tooltip("Máximo número de bombas simultáneas en escena")]
    [SerializeField] private int maxBombs = 2;

    private float _lastSpawnTime;

    public override void FixedUpdateNetwork()
    {
        Debug.Log($"[BombSpawner] FixedUpdateNetwork – HasStateAuthority={Object.HasStateAuthority}, simTime={Runner.SimulationTime}");
        if (!Object.HasStateAuthority)
            return;

        if (Runner.SimulationTime - _lastSpawnTime < spawnInterval)
            return;

        int activeBombs = FindObjectsOfType<Bomb>().Length;
        if (activeBombs >= maxBombs)
            return;

        _lastSpawnTime = Runner.SimulationTime;
        SpawnBomb();
    }

    private void SpawnBomb()
    {
        Debug.Log("[BombSpawner] SpawnBomb() llamado");
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        int idx = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPos = spawnPoints[idx].position;
        Quaternion spawnRot = spawnPoints[idx].rotation;

        Runner.Spawn(bombPrefab, spawnPos, spawnRot);
    }
}
