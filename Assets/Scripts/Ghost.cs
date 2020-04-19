﻿using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float Speed = 1000f;
    public float TargettingDistance = 20f;
    public float ReceivedAttackForce = 550f;

    private GameObject player;
    private GameObject torchlightCollider;
    private GameObject campfire;
    private new Rigidbody rigidbody;

    private Vector3 direction;
    private Vector3 target;
    private Vector3 spawnPoint;

    private float confiusionTargetTime = float.MinValue;
    private bool trackingPlayer = false;

    private Vector3? runAwayDirection;
    private TimeProgress runAwayTimeProgress = new TimeProgress();

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        spawnPoint = transform.position;

        player = GameObject.FindWithTag("Player");
        torchlightCollider = GameObject.Find("Player/TorchlightCollider");
        campfire = GameObject.Find("Campfire");
    }

    void Update()
    {
        // AI, set target
        if (runAwayDirection == null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            bool canSeePlayer = (distanceToPlayer <= TargettingDistance);

            if (!canSeePlayer && (confiusionTargetTime > Time.timeSinceLevelLoad))
            {
                target = transform.position;
            }
            else
            {
                bool prevFrameTrackingPlayer = trackingPlayer;

                if (canSeePlayer)
                {
                    target = player.transform.position;
                    trackingPlayer = true;
                }
                else
                {
                    target = spawnPoint;
                    trackingPlayer = false;
                }

                if (prevFrameTrackingPlayer && !trackingPlayer)
                {
                    confiusionTargetTime = (Time.timeSinceLevelLoad + Random.Range(2f, 6f));
                }
            }
        }

        // Calc direction
        direction = (runAwayDirection != null)
            ? (runAwayDirection ?? Vector3.zero)
            : (transform.position - target);
        direction.y = 0f;
        direction *= -1f;

        // Clear run away dir
        if (runAwayTimeProgress.IsDone())
        {
            runAwayDirection = null;
        }

        // Rotate towards movement direction
        float directionAngle = Mathf.Atan2(-direction.z, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, directionAngle, 0f);
    }

    void FixedUpdate()
    {
        // Actually move
        float distanceToTarget = Vector3.Distance(transform.position, target);
        if (distanceToTarget > 1f)
        {
            rigidbody.AddForce((direction.normalized * Speed), ForceMode.Force);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "TorchlightCollider")
        {
            rigidbody.AddForce((-direction * ReceivedAttackForce), ForceMode.Impulse);
        }
        else if (collider.name == "CampfireTrigger")
        {
            Vector3 cv = campfire.transform.position;
            Vector3 dir = cv - transform.position;
            dir.Normalize();

            runAwayDirection = dir;
            runAwayTimeProgress.Start(5f);
            trackingPlayer = false;
        }
    }
}
