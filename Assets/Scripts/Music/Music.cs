using UnityEngine;

public class Music : MonoBehaviour
{
    private AudioSource _audioSource;
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        //DontDestroyOnLoad(transform.gameObject);
        if (_audioSource.isPlaying)
            return;
        _audioSource.Play();

    }

    public void ToggleMusic()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
        else
        {
            _audioSource.Play();
        }
    }


}