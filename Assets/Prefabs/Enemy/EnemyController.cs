using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class EnemyController : MonoBehaviour
{
    enum Animation
    {
        idle = 0,
        walking = 1
    }

    [Header("Bewegung")]
    [SerializeField] private List<Transform> waypoints; // Ziehe hier leere GameObjects rein
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float waitTimeAtWaypoint = 1f;


    [Header("Squash Einstellungen")]
    [SerializeField] private Vector3 squashFactor = new Vector3(1.5f, 0.2f, 1.5f);
    [SerializeField] private float squashDuration = 0.1f;
    [SerializeField] private float revertDuration = 0.3f;
    [SerializeField] private float recoverDuration = 1f;
    [SerializeField] private float headHeight = 0.5f;

    [Header("Player Einstellungen")]
    [SerializeField] private float katapultForce = 12f;

    [Header("Player Einstellungen")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioClip squashSound;



    private Vector3 originalScale;
    private bool isSquashing = false;


    private int currentWaypointIndex = 0;
    private bool isDead = false;
    private bool isWaiting = false;

    private Animator anim;
    //private Rigidbody rb;

    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        originalScale = transform.localScale;

        if (waypoints.Count > 0)
        {
            //transform.position = waypoints[0].position;
            anim.SetInteger("animation", (int)Animation.walking);
        }
        else
            anim.SetInteger("animation", (int)Animation.idle);

        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.playOnAwake = false;
        audioSource.clip = squashSound;
        audioSource.loop = false;
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (waypoints.Count == 0) return;

        Vector3 targetPos = waypoints[currentWaypointIndex].position;
        Vector3 moveDirection = (targetPos - transform.position);
        moveDirection.y = 0;

        TurnTowardsWaypoint(moveDirection);

        if (isWaiting) return;

        MoveTowardsWaypoint(targetPos, moveDirection);

    }

    void TurnTowardsWaypoint(Vector3 moveDirection)
    {
        // 1. Drehung zum Ziel
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void MoveTowardsWaypoint(Vector3 targetPos, Vector3 moveDirection)
    {
        // 2. Bewegung per Rigidbody
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);
        //rb.MovePosition(newPos);
        transform.position = newPos;

        // 3. Prüfen, ob Wegpunkt erreicht
        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        //anim.SetFloat("Speed", 0); // Falls dein Animator einen Speed-Parameter hat

        // Nächster Wegpunkt (Ping-Pong Logik oder Loop)
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;

        yield return new WaitForSeconds(waitTimeAtWaypoint);


        //anim.SetFloat("Speed", 1);
        isWaiting = false;
    }

    // Trigger für das Draufspringen (sollte am Kopf-Objekt hängen oder hier gefiltert werden)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDead && !isSquashing)
        {
            Debug.Log("I wana squash other:" + (other.transform.position.y - transform.position.y ));
            // Prüfen ob der Spieler von oben kommt (einfacher Check)
            if ((other.transform.position.y - transform.position.y) > headHeight)
            {
                ApplySquash(other.gameObject);
            }
            else
            {
                Character c = other.GetComponent<Character>();
                if (c != null)
                {
                    c.Die();
                }
            }
        }
    }

    void ApplySquash(GameObject player)
    {
        if (isDead) return;

        Character c = player.GetComponent<Character>();
        if (c != null)
        {
            c.KatapultJumpPlayer(katapultForce);
        }
        PlayJumpSound();
        StartCoroutine(SquashSequence());
    }

    IEnumerator SquashSequence()
    {
        isWaiting = true;
        isSquashing = true;

        // Physik ausschalten, damit er nicht wegkatapultiert wird
        //if (rb) rb.isKinematic = true;

        // Squash hin
        Vector3 targetScale = Vector3.Scale(originalScale, squashFactor);
        yield return StartCoroutine(ScaleOverTime(originalScale, targetScale, squashDuration));

        // Kurz warten
        yield return new WaitForSeconds(recoverDuration);

        // Squash zurück (Recover)
        yield return StartCoroutine(ScaleOverTime(targetScale, originalScale, revertDuration));

        // Alles wieder auf normal
        //if (rb) rb.isKinematic = false;
        isSquashing = false;
        isWaiting = false;
    }

    IEnumerator ScaleOverTime(Vector3 start, Vector3 end, float time)
    {
        float elapsed = 0;
        while (elapsed < time)
        {
            transform.localScale = Vector3.Lerp(start, end, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = end;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // NUR wenn der Gegner NICHT gerade gesquashed wird, stirbt der Spieler
            if (!isSquashing)
            {
                RespawnPlayer(collision.gameObject);
            }
        }
    }

    void RespawnPlayer(GameObject player)
    {
        Debug.Log("Spieler getroffen! Respawn...");

        Character c = player.GetComponent<Character>();
        if (c != null)
        {
            c.Die();
        }
    }

    private void PlayJumpSound()
    {
        audioSource.Play();
    }
}
