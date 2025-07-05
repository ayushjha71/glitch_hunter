using GlitchHunter.Constant;
using GlitchHunter.Interface;
using UnityEngine;

public class CollectableSpawnerHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject[] collectable;

    private void OnEnable()
    {
        GlitchHunterConstant.OnSpawnCollectable += Spawn;
    }

    private void OnDisable()
    {
        GlitchHunterConstant.OnSpawnCollectable -= Spawn;
    }

    public void Spawn(Transform trans, bool isCrate)
    {
        // Choose a random prefab from the array
        int randomIndex = Random.Range(0, collectable.Length);
        GameObject prefabToSpawn = collectable[randomIndex];
        if (isCrate)
        {
            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"Prefab at index {randomIndex} is null!", this);
                return;
            }
            foreach(GameObject prefab in collectable)
            {
                SpawnCollectable(prefab, trans);
            }          
        }
        else
        {
            GameObject spawnedObject = Instantiate(prefabToSpawn, trans.position, Quaternion.identity);
            spawnedObject.transform.SetParent(trans);
            Debug.Log($"Spawned {prefabToSpawn.name}");
        }

    }

    private void SpawnCollectable(GameObject prefabToSpawn, Transform trans)
    {
        GameObject spawnedObject = Instantiate(prefabToSpawn, trans.position, Quaternion.identity);
        spawnedObject.transform.SetParent(trans);
        Debug.Log($"Spawned {prefabToSpawn.name}");
    }
}
