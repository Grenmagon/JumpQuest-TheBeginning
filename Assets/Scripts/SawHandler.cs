using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent (typeof(AudioSource))]
public class SawHandler : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField]
    private float rotationSpeed = 400f;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioMixerGroup sfxMixerGroup;
    [SerializeField]
    private AudioClip idleSound;
    [SerializeField]
    private AudioClip cuttingSound;

    [Header("Particles")]
    [SerializeField]
    private ParticleSystem cuttingParticles;

    private bool isCutting ;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.loop = true;
        audioSource.playOnAwake = true;

        isCutting = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource.clip = idleSound;
        audioSource.Play();
        isCutting = false;
        if (cuttingParticles != null)
        {
            cuttingParticles.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetState(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetState(false);
        }
    }

    private void SetState(bool cutting)
    {
        if (isCutting == cutting)
            return;
        if (cutting)
        {
            isCutting = true;
            audioSource.clip = cuttingSound;
            if (cuttingParticles != null)
            {
                cuttingParticles.Play();
            }
        }
        else
        {
            isCutting = false;
            audioSource.clip = idleSound;
            if (cuttingParticles != null)
            {
                cuttingParticles.Stop();
            }
        }
        audioSource.Play();
    }
}
