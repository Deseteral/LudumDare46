﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    public float Speed = 1000f;
    public float DamageAmount = 0.01f;

    private GameObject torchlightCollider;
    private MeshCollider torchlightColliderActualColliderThatCollides;
    private Light torchlightLight;

    public bool IsTorchlightActive = false;
    public float TorchlightLengthActive = 10f;
    public float TorchlightLength = 4f;

    public float Health = 1.0f;

    private new Rigidbody rigidbody;
    private Volume globalVolume;

    public int AttackTime = 1;
    public int AttackRechargeTime = 3;
    private float canAttackAfter = float.MinValue;
    private float noLongerAttackingAfter = float.MinValue;

    private Image attackTimerProgress;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        torchlightLight = GameObject.Find("Player/Torchlight").GetComponent<Light>();
        attackTimerProgress = GameObject.Find("Player/Canvas/AttackTimerProgress").GetComponent<Image>();
        globalVolume = GameObject.Find("Global Volume").GetComponent<Volume>();
        torchlightCollider = GameObject.Find("Player/TorchlightCollider");
        torchlightColliderActualColliderThatCollides = torchlightCollider.GetComponent<MeshCollider>();
    }

    void Update()
    {
        // Atack logic
        if (Time.timeSinceLevelLoad >= canAttackAfter) // Can attack
        {
            attackTimerProgress.fillAmount = 0f;

            if (Input.GetMouseButton(0))
            {
                IsTorchlightActive = true;
                canAttackAfter = (Time.timeSinceLevelLoad + AttackTime + AttackRechargeTime);
                noLongerAttackingAfter = (Time.timeSinceLevelLoad + AttackTime);
            }
        }
        else
        {
            // Display attack timer progress
            float v = (canAttackAfter - Time.timeSinceLevelLoad) / (AttackTime + AttackRechargeTime);
            attackTimerProgress.fillAmount = Mathf.Clamp(v, 0f, 1f);
        }

        if (IsTorchlightActive && (Time.timeSinceLevelLoad >= noLongerAttackingAfter)) // Is no longer attacking?
        {
            IsTorchlightActive = false;
        }

        // Activate collider
        torchlightColliderActualColliderThatCollides.enabled = IsTorchlightActive;

        // Rotation
        // TODO: Controller support
        Vector3 diff = transform.position - GetMouseOnTerrain();
        float direction = Mathf.Atan2(diff.z, -diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, direction, 0f);

        // Torchlight
        Vector3 torchColliderScale = torchlightCollider.transform.localScale;
        Vector3 torchColliderPosition = torchlightCollider.transform.localPosition;
        {
            float actualLightLength = IsTorchlightActive ? TorchlightLengthActive : TorchlightLength;

            torchColliderScale.y = actualLightLength;
            torchColliderPosition.x = (actualLightLength / 2) + 2;
            torchlightLight.range = (actualLightLength * 4);

            torchlightLight.intensity += ((IsTorchlightActive ? 80f : 15f) - torchlightLight.intensity) * 0.1f;
        }
        torchlightCollider.transform.localScale = torchColliderScale;
        torchlightCollider.transform.localPosition = torchColliderPosition;

        // Set vignette to health value
        SetVignetteIntensity(1f - Health);

        // TODO: Check health and die
        if (Health <= 0f)
        {
            Debug.Log("YOU DIED!");
        }

        // Restore health
        ChangeHealth(DamageAmount * 0.5f);
    }

    void FixedUpdate()
    {
        Vector3 delta = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 forceVec = (Quaternion.Euler(0, 45, 0) * (delta * Speed));
        rigidbody.AddForce(forceVec, ForceMode.Force);
    }

    void OnTriggerStay(Collider collider)
    {
        if (collider.name == "DamageCollider")
        {
            ChangeHealth(-DamageAmount);
        }
    }

    private Vector3 GetMouseOnTerrain()
    {
        const int terrainLayerMask = (1 << 8);
        RaycastHit hit;
        return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000, terrainLayerMask)
            ? hit.point
            : Vector3.zero;
    }

    private void SetVignetteIntensity(float value)
    {
        Vignette vignette;
        if (globalVolume.profile.TryGet(out vignette)) vignette.intensity.value = value;
    }

    private void ChangeHealth(float diff)
    {
        Health += diff;
        if (Health > 1f) Health = 1f;
    }
}
