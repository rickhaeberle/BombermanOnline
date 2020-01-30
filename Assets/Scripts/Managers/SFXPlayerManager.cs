using UnityEngine;

public class SFXPlayerManager : MonoBehaviour {

    public static SFXPlayerManager Instance { get; private set; }

    private AudioSource _source;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip, float volume = 1f) {
        if (_source == null)
            return;

        if (clip == null)
            return;

        _source.PlayOneShot(clip, volume);
    }

}
