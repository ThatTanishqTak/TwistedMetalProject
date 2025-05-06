using UnityEngine;

public static class ExplosionSoundHelper
{
    private const string EXPLOSION_PATH = "Audio/Weapons/Effects/Explosion";

    private static AudioClip _explosionClip;
    private static bool _loaded;

    public static void PlayExplosion(Vector3 position)
    {
        if (!_loaded)
        {
            _explosionClip = Resources.Load<AudioClip>(EXPLOSION_PATH);
            if (_explosionClip == null)
                Debug.LogWarning($"[ExplosionSoundHelper] Failed to load clip at '{EXPLOSION_PATH}'");
            else
                Debug.Log("[ExplosionSoundHelper] Loaded explosion clip");
            _loaded = true;
        }

        if (_explosionClip != null)
        {
            AudioSource.PlayClipAtPoint(_explosionClip, position);
            Debug.Log($"[ExplosionSoundHelper] Played explosion at {position}");
        }
    }
}
