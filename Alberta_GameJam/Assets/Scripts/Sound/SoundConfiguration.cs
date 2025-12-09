using UnityEngine;

[System.Serializable]
public class SoundConfiguration
{
    public SoundType type;
    public SoundWord prefab;
    public float duration = 1f;
    public float defaultSize = 1f;
    public float minInterval = 0f;
}