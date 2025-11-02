using UnityEngine;

[System.Serializable]
public class SoundConfiguration
{
    public SoundType type;
    public SoundWord prefab;
    public float duration = 1f;
    public float defaultSize = 1f;
    [Tooltip("How often this sound can play (0 for no limit)")]
    public float minInterval = 0f;
}