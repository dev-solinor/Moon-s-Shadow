using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ForestGenerator : MonoBehaviour
{
    [Header("Terrain")]
    [SerializeField] private Terrain terrain;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] rockPrefabs;

    [Header("Placement")]
    [SerializeField] private int treeCount = 200;
    [SerializeField] private int rockCount = 40;
    [SerializeField] private float noiseScale = 0.05f;
    [SerializeField][Range(0f, 1f)] private float noiseThreshold = 0.4f;
    [SerializeField] private int seed = 42;

    [Header("Scale aléatoire")]
    [SerializeField] private float treeScaleMin = 0.8f;
    [SerializeField] private float treeScaleMax = 1.4f;
    [SerializeField] private float rockScaleMin = 0.5f;
    [SerializeField] private float rockScaleMax = 1.2f;

    [Header("Zones d'exclusion (centre + rayon)")]
    [SerializeField] private List<ExclusionZone> exclusionZones = new();

    private Transform _container;

    // Public API

    [ContextMenu("Generate Forest")]
    public void GenerateForest()
    {
        ClearForest();
        Random.InitState(seed);

        _container = new GameObject("_ForestContainer").transform;
        _container.parent = transform;

        if (terrain == null)
        {
            Debug.LogError("[ForestGenerator] Aucun Terrain assigné !");
            return;
        }

        PlaceObjects(treePrefabs, treeCount, treeScaleMin, treeScaleMax, "Tree");
        PlaceObjects(rockPrefabs, rockCount, rockScaleMin, rockScaleMax, "Rock");

        Debug.Log($"[ForestGenerator] Forêt générée — {treeCount} arbres, {rockCount} rochers.");
    }

    [ContextMenu("Clear Forest")]
    public void ClearForest()
    {
        var existing = transform.Find("_ForestContainer");
        if (existing != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(existing.gameObject);
#else
            Destroy(existing.gameObject);
#endif
        }
    }

    // Placement

    private void PlaceObjects(
        GameObject[] prefabs,
        int count,
        float scaleMin, float scaleMax,
        string label)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        TerrainData td = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        float width = td.size.x;
        float depth = td.size.z;

        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 10;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;

            float nx = Random.Range(0f, 1f);
            float nz = Random.Range(0f, 1f);

            float noiseValue = Mathf.PerlinNoise(
                nx * width * noiseScale + seed,
                nz * depth * noiseScale + seed
            );

            if (noiseValue < noiseThreshold) continue;

            float worldX = origin.x + nx * width;
            float worldZ = origin.z + nz * depth;
            float worldY = terrain.SampleHeight(new Vector3(worldX, 0, worldZ)) + origin.y;

            Vector3 position = new(worldX, worldY, worldZ);

            if (IsExcluded(position)) continue;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            float scale = Random.Range(scaleMin, scaleMax);
            float rotY = Random.Range(0f, 360f);

            GameObject instance = Instantiate(
                prefab,
                position,
                Quaternion.Euler(0, rotY, 0),
                _container
            );

            instance.transform.localScale = Vector3.one * scale;
            instance.name = $"{label}_{placed}";
            placed++;
        }

        if (placed < count)
            Debug.LogWarning($"[ForestGenerator] Seulement {placed}/{count} {label}s placés. " +
                             "Réduis noiseThreshold ou les zones d'exclusion.");
    }

    // Exclusion

    private bool IsExcluded(Vector3 position)
    {
        foreach (var zone in exclusionZones)
        {
            float dist = Vector2.Distance(
                new Vector2(position.x, position.z),
                new Vector2(zone.center.x, zone.center.z)
            );
            if (dist < zone.radius) return true;
        }
        return false;
    }

    // Gizmos (Scene visualization)

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        foreach (var zone in exclusionZones)
            Gizmos.DrawWireSphere(zone.center, zone.radius);
    }
}

// Struct of exclusion zones

[System.Serializable]
public struct ExclusionZone
{
    public Vector3 center;
    public float radius;
}

// Generate button in the Inspector (Editor only)

#if UNITY_EDITOR
[CustomEditor(typeof(ForestGenerator))]
public class ForestGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(8);

        ForestGenerator gen = (ForestGenerator)target;

        if (GUILayout.Button("Generate Forest", GUILayout.Height(32)))
            gen.GenerateForest();

        if (GUILayout.Button("Clear Forest", GUILayout.Height(28)))
            gen.ClearForest();
    }
}
#endif