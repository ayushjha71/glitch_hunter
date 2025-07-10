using UnityEngine;

[CreateAssetMenu(fileName = "DollDataContainer", menuName = "Scriptable Objects/DollDataContainer")]
public class DollDataContainer : ScriptableObject
{
    [Header("Traverse")]
    public float moveSpeed = 10f;
    public float stopDistance = 2f;
        
    [Header("Explosion Settings")]
    public GameObject explosionEffect;
    public AudioClip explosionSound;
    public AudioClip[] footsteps;
    public GameObject dollPrefab;
    public float explosionDelay = 3f;
    public float explosionRadius = 5f;
    public float explosionDamage = 100f;
    
}
