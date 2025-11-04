using System.Collections.Generic;
using UnityEngine;

public class SoundWordDisplay : MonoBehaviour
{
    [Tooltip("Configure different sound types with their own prefabs and settings")]
    [SerializeField] private List<SoundConfiguration> soundConfigs = new List<SoundConfiguration>();

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
        var config = soundConfigs.Find(c => c.type == type);
        if (config?.prefab == null) return;

        int instanceId = -1;
        var objectAtPosition = Physics2D.OverlapPoint(position);
        if (objectAtPosition != null)
        {
            instanceId = objectAtPosition.gameObject.GetInstanceID();
        }

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

        if (size <= 0) size = config.defaultSize;

        SoundWord sw = Object.Instantiate(config.prefab, position, Quaternion.identity);
        sw.Spawn(position, direction, size);
        Destroy(sw.gameObject, config.duration);
    }
}
