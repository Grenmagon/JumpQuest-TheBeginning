using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
public class Character : MonoBehaviour
{
    private bool isJumping = false;
    private float jumpCooldownTimer;
    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;

    [SerializeField] private float jumpCooldown;
    //We set gravity lower than in real live as it is more fun!
    [SerializeField] private float gravity;
    [SerializeField] private float characterSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float dampening;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask platformMask;
    [SerializeField] private float platformCheckSize;

    [SerializeField] private Transform respawnPosition;


    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource jumpAudioSource;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip spawnSound;
    private bool isRunningSound = false;
    private bool doJumpSound = false;


    [Header("Particle")]
    [SerializeField] private ParticleSystem walkingParticles;

    [Header("DEBUG")]
    public Vector3 characterMovement;
    public float characterMovementSpeed;
    private Vector3 jumpVelocity;
    private Vector3 characterGravity;
    public Vector3 platformVelocity;

    private Animator animator;

    void Start()
    {
        this.animator = GetComponent<Animator>();
        this.controller = this.GetComponent<CharacterController>();
        this.moveAction = InputSystem.actions.FindAction("Move");
        this.jumpAction = InputSystem.actions.FindAction("Jump");
        this.jumpCooldownTimer = 0.0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        jumpAudioSource.outputAudioMixerGroup = sfxMixerGroup;
        jumpAudioSource.loop = false;
        jumpAudioSource.playOnAwake = false;


        if (walkingParticles != null)
        {
            walkingParticles.Stop();
        }
    }



    public void KatapultJumpPlayer(float katapultJumpSpeed)
    {
        this.isJumping = true;
        this.characterGravity = Vector3.zero;
        this.jumpVelocity = Vector3.zero;
        this.jumpVelocity.y = katapultJumpSpeed;
        this.jumpCooldownTimer = this.jumpCooldown;
    }

    public void Die()
    {
        StartCoroutine(DieAndRespawnRoutine());
    }

    IEnumerator DieAndRespawnRoutine()
    {
        controller.enabled = false;

        yield return StartCoroutine(DieRoutine());

        this.transform.position = respawnPosition.position;

        yield return StartCoroutine(RespawnRoutine());

        controller.enabled = true;
    }

    IEnumerator DieRoutine()
    {
        StopRunningSound();
        PlayDeathSound();
        animator.SetTrigger("Dieing");
        yield return null;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length + 0.5f);
    }

    IEnumerator RespawnRoutine()
    {
        PlaySpawnSound();
        animator.SetTrigger("Spawn");
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length + 0.3f);
    }

    private void setAnimation()
    {
        Vector2 movement = new Vector2(characterMovement.x, characterMovement.z);
        characterMovementSpeed = movement.magnitude / Time.fixedDeltaTime;
        animator.SetFloat("RunningSpeed", characterMovementSpeed);
        //animator.SetBool("isJumping", isJumping);
        animator.SetBool("isJumping", !controller.isGrounded);

        if (doJumpSound)
            PlayJumpSound();
        else
        {
            if (characterMovementSpeed >= 1.0f && controller.isGrounded)
                doRunningSound();
            if (characterMovementSpeed < 1f || !controller.isGrounded)
                StopRunningSound();
        }
    }

    private void StopRunningSound()
    {
        if (!isRunningSound) return;

        Debug.Log("End WalkingSound");
        audioSource.Stop();
        isRunningSound = false;

        if (walkingParticles != null)
        {
            walkingParticles.Stop();
        }

    }

    private void doRunningSound()
    {
        if (isRunningSound) return;

        Debug.Log("Start WalkingSound");
        audioSource.clip = walkingSound;
        audioSource.loop = true;
        audioSource.Play();
        isRunningSound = true;

        if (walkingParticles != null)
        {
            walkingParticles.Play();
        }

    }

    private void PlayJumpSound()
    {
        if (!doJumpSound) return;

        Debug.Log("PlayJumpSource");
        StopRunningSound();
        jumpAudioSource.clip = jumpSound;
        jumpAudioSource.loop = false;
        jumpAudioSource.Play();
    }
    private void PlayLandingSound()
    {
        Debug.Log("PlayLandingSource");
        StopRunningSound();
        jumpAudioSource.clip = landingSound;
        jumpAudioSource.loop = false;
        jumpAudioSource.Play();
    }

    private void PlayDeathSound()
    {
        audioSource.clip = deathSound;
        audioSource.loop = false;
        audioSource.Play();
    }

    private void PlaySpawnSound()
    {
        audioSource.clip = spawnSound;
        audioSource.loop = false;
        audioSource.Play();
    }



    void HandleJumping()
    {
        doJumpSound = false;

        if (this.controller.isGrounded && this.isJumping && this.jumpCooldownTimer <= 0.0f)
        {
            this.jumpVelocity = Vector3.zero;
            this.isJumping = false;
            PlayLandingSound();
        }
        if (this.controller.isGrounded && !this.isJumping && this.jumpAction.WasPressedThisFrame())
        {
            this.characterGravity = Vector3.zero;
            this.jumpVelocity = Vector3.zero;
            this.jumpVelocity.y = this.jumpSpeed;
            this.jumpCooldownTimer = this.jumpCooldown;
            this.isJumping = true;
            doJumpSound = true;


        }
        if (this.jumpVelocity.y > 0.0f)
        {
            this.jumpVelocity.y -= Time.fixedDeltaTime;
        }
        else
        {
            this.jumpVelocity = Vector3.zero;
        }
        this.jumpCooldownTimer -= Time.fixedDeltaTime;
    }

    void FixedUpdate()
    {
        if (!controller.enabled) return;

        this.HandleJumping();
        var inputMovement = this.moveAction.ReadValue<Vector2>();

        var inputRightDirection = this.cameraTransform.right;
        var inputForwardDirection = this.cameraTransform.forward;
        inputRightDirection.y = 0.0f;
        inputForwardDirection.y = 0.0f;
        inputRightDirection.Normalize();
        inputForwardDirection.Normalize();
        //Since we do not use the physics system, we have to simulate gravity ourselves
        if (this.controller.isGrounded)
        {
            this.characterGravity.y = 0.0f;
        }
        this.characterGravity.y += this.gravity * Time.fixedDeltaTime;
        this.characterMovement += this.characterGravity * Time.fixedDeltaTime;
        this.characterMovement += this.jumpVelocity * Time.fixedDeltaTime;
        this.characterMovement += inputRightDirection * inputMovement.x * this.characterSpeed * Time.fixedDeltaTime;
        this.characterMovement += inputForwardDirection * inputMovement.y * this.characterSpeed * Time.fixedDeltaTime;
        this.characterMovement *= (1 - this.dampening);
        Vector3 characterForward = this.characterMovement;
        characterForward.y = 0.0f;
        if (characterForward.sqrMagnitude > 0.0f && characterForward != Vector3.zero)
        {
            this.transform.forward = characterForward.normalized;
        }



        setAnimation();

        GetPlatformVelocity();
        var combinedMovement = this.characterMovement + this.platformVelocity * Time.fixedDeltaTime;
        this.controller.Move(combinedMovement);
    }

    private void GetPlatformVelocity()
    {
        if (!this.controller.isGrounded)
            return;

        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, Vector3.down, out hit, platformCheckSize, platformMask))
        {
            Debug.Log("OnPlatform");
            MovingPlatform platform = hit.collider.GetComponent<MovingPlatform>();

            if (platform != null)
            {
                Debug.Log("GetVelocity");
                // Gib die Velocity der Plattform zurück
                platformVelocity = platform.Velocity;
            }
            else
                platformVelocity = Vector3.zero;
        }
        else
            platformVelocity = Vector3.zero;
    }
}

