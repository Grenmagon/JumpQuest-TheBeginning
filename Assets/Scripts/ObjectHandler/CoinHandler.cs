using UnityEngine;
using UnityEngine.Audio;

public class CoinHandler : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 90f;
    [SerializeField]
    private AudioClip collectSound;
    [SerializeField]
    private AudioMixerGroup sfxGroup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        var collectSoundGO = new GameObject("CollectSOund");
        collectSoundGO.transform.position = transform.position;
        var audioSource = collectSoundGO.AddComponent<AudioSource>();
        audioSource.clip = collectSound;
        audioSource.outputAudioMixerGroup = sfxGroup;
        audioSource.Play();
        //collectSoundGO.AddComponent<AudioReverbFilter>();
        Destroy(collectSoundGO, collectSound.length);

        Destroy(this.gameObject);
    }
}
