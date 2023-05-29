using UnityEngine;

public class AudioHolder : MonoBehaviour
{
    public static AudioHolder Instance;

    public static bool HasMusicStarted = false;
    public uint? AmbienceBedGenericID = null;
    public uint? AmbienceCrowdGenericID = null;
    public uint? AmbienceMenuSoundID = null;

    private void Awake()
    {
        if (AudioHolder.Instance != null)
        {
            if (AudioHolder.Instance != this)
            {
#if UNITY_EDITOR
                    DestroyImmediate(this.gameObject, false);
#else
                Destroy(this.gameObject);
#endif
                return;
            }
        }

        AudioHolder.Instance = this;

        // Set it to root 
        this.transform.parent = null;

        Object.DontDestroyOnLoad(this.gameObject);
    }
}