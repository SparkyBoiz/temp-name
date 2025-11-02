using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Presentation listener that spawns different SoundWord prefabs based on the requested type.
/// Configure different sound types with their own prefabs, durations, and intervals.
/// </summary>
public class SoundWordDisplay : MonoBehaviour
{
    [Tooltip("Configure different sound types with their own prefabs and settings")]
    [SerializeField] private List<SoundConfiguration> soundConfigs = new List<SoundConfiguration>();

    // Track last play time for sounds with intervals, per-instance and per-type
    private Dictionary<int, Dictionary<SoundType, float>> lastPlayTimes = new Dictionary<int, Dictionary<SoundType, float>>();

    private void OnEnable()
    {
        GameEvents.OnSoundWordRequested += HandleSoundWordRequested;
    }

    private void OnDisable()
    {
        GameEvents.OnSoundWordRequested -= HandleSoundWordRequested;
        lastPlayTimes.Clear();
    }

    private void HandleSoundWordRequested(SoundType type, Vector3 position, Vector3 direction, float size)
    {
        // Find the configuration for this sound type
        var config = soundConfigs.Find(c => c.type == type);
        if (config?.prefab == null) return;

        // Get instance ID from the GameObject at this position
        int instanceId = -1;
        var objectAtPosition = Physics2D.OverlapPoint(position);
        if (objectAtPosition != null)
        {
            instanceId = objectAtPosition.gameObject.GetInstanceID();
        }

        // Check if enough time has passed since last play for this instance
        if (config.minInterval > 0 && instanceId != -1)
        {
            if (!lastPlayTimes.ContainsKey(instanceId))
            {
                lastPlayTimes[instanceId] = new Dictionary<SoundType, float>();
            }

            var instanceTimes = lastPlayTimes[instanceId];
            float lastPlayTime = instanceTimes.ContainsKey(type) ? instanceTimes[type] : -1000f;
            if (Time.time < lastPlayTime + config.minInterval) return;
            instanceTimes[type] = Time.time;
        }

        // Use the configured size if no specific size was requested
        if (size <= 0) size = config.defaultSize;

        // Instantiate and call Spawn on the component
        SoundWord sw = Object.Instantiate(config.prefab, position, Quaternion.identity);
        sw.Spawn(position, direction, size);
        Destroy(sw.gameObject, config.duration);
    }
}
