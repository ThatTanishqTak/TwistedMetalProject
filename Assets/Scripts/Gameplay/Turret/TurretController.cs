using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TurretController : NetworkBehaviour
{
    [Header("Turret Parts")]
    [SerializeField] private Transform cannonBase;
    [SerializeField] private Transform cannonHead;
    [SerializeField] private Transform shootPoint;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 45f;

    [Header("Shooter Reference")]
    [SerializeField] private Shooter shooter;
    [SerializeField] private GunStats gunStats;              

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlash;    

    [Header("Rocket Settings")]
    [SerializeField] private GunStats rocketStats;           
    [SerializeField] private float rocketCooldown = 45f;    
    [SerializeField] private bool rocketReady = true;

    [Header("Audio System for weapos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rifleSound;
    [SerializeField] private AudioClip rocketSound;
    [SerializeField] private const string RIFLE_PATH = "Audio/Weapons/RifleFire";
    [SerializeField] private const string ROCKET_PATH = "Audio/Weapons/RocketLaunch";

    [SerializeField] private float lastFireTime;

    private NetworkVariable<Quaternion> baseRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<float> headPitch = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[TurretController] Added AudioSource at runtime");
        }
        else
        {
            Debug.Log("[TurretController] Found existing AudioSource");
        }

        rifleSound = Resources.Load<AudioClip>(RIFLE_PATH);
        rocketSound = Resources.Load<AudioClip>(ROCKET_PATH);

        if (rifleSound == null)
            Debug.LogWarning($"[TurretController] Failed to load Rifle clip at '{RIFLE_PATH}'");
        else
            Debug.Log("[TurretController] Loaded rifleSound successfully");

        if (rocketSound == null)
            Debug.LogWarning($"[TurretController] Failed to load Rocket clip at '{ROCKET_PATH}'");
        else
            Debug.Log("[TurretController] Loaded rocketSound successfully");
    }

    private void Update()
    {
        if (!shooter.IsShooterControlled ||
            NetworkManager.Singleton.LocalClientId != shooter.ShooterClientId)
            return;

        float mx = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        UpdateRotationServerRpc(mx, my);

        if (Input.GetMouseButton(0) &&
            Time.time >= lastFireTime + gunStats.fireRate)
        {
            lastFireTime = Time.time;
            FireServerRpc();
        }

        if (Input.GetMouseButtonDown(1) && rocketReady)
        {
            rocketReady = false;
            FireRocketServerRpc();
            Debug.Log("[TurretController] Rocket launched");
            StartCoroutine(RocketReloadCoroutine());
        }
    }

    private void LateUpdate()
    {
        cannonBase.localRotation = baseRotation.Value;
        cannonHead.localEulerAngles = new Vector3(headPitch.Value, 0f, 0f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateRotationServerRpc(float yawDelta, float pitchDelta)
    {
        float newYaw = (baseRotation.Value.eulerAngles.y + yawDelta) % 360f;
        float newPitch = Mathf.Clamp(headPitch.Value - pitchDelta, minPitch, maxPitch);

        baseRotation.Value = Quaternion.Euler(0f, newYaw, 0f);
        headPitch.Value = newPitch;
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward,
                            out var hit, gunStats.bulletRange))
        {
            if (hit.transform.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(gunStats.damage);
                Debug.Log($"[Server] Rifle hit {hit.transform.name} for {gunStats.damage}");
            }
            else
            {
                Debug.Log($"[Server] Rifle hit {hit.transform.name}, not damageable");
            }
        }
        else
        {
            Debug.Log("[Server] Rifle missed");
        }

        MuzzleFlashAndSoundClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireRocketServerRpc()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward,
                            out var hit, rocketStats.bulletRange))
        {
            if (hit.transform.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(rocketStats.damage);
                Debug.Log($"[Server] Rocket hit {hit.transform.name} for {rocketStats.damage}");
            }
            else
            {
                Debug.Log($"[Server] Rocket hit {hit.transform.name}, not damageable");
            }
            RocketExplosionClientRpc(hit.point);

        }
        else
        {
            Debug.Log("[Server] Rocket missed");
        }

        RocketLaunchSoundClientRpc();
    }

    [ClientRpc]
    private void MuzzleFlashAndSoundClientRpc()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.transform.position = shootPoint.position;
            muzzleFlash.transform.rotation = shootPoint.rotation;
            muzzleFlash.Clear();
            muzzleFlash.Emit(1);
            Debug.Log("[TurretController] Emitted muzzle flash");
        }
        else
        {
            Debug.LogWarning("[TurretController] muzzleFlash not assigned");
        }

        if (rifleSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(rifleSound);
            Debug.Log("[TurretController] Played rifleSound");
        }
        else
        {
            Debug.LogWarning("[TurretController] rifleSound is null");
        }
    }

    [ClientRpc]
    private void RocketLaunchSoundClientRpc()
    {
        if (rocketSound != null)
        {
            audioSource.PlayOneShot(rocketSound);
            Debug.Log("[TurretController] Played rocketSound");
        }
        else
        {
            Debug.LogWarning("[TurretController] rocketSound is null");
        }
    }

    private IEnumerator RocketReloadCoroutine()
    {
        yield return new WaitForSeconds(rocketCooldown);
        rocketReady = true;
        Debug.Log("[TurretController] Rocket loaded");
    }
    [ClientRpc]
   private void RocketExplosionClientRpc(Vector3 position)
   {
       ExplosionSoundHelper.PlayExplosion(position);
   }
}
