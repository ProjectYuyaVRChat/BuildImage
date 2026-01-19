using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TouchSoundTrigger : UdonSharpBehaviour
{
    [Header("反応するオブジェクト")]
    public GameObject targetObject;

    [Header("再生する音")]
    public AudioClip sound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetObject)
        {
            if (sound)
            {
                audioSource.PlayOneShot(sound);
            }
        }
    }
}
