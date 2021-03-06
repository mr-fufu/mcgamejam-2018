﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public bool jump = false;
    [HideInInspector] public bool dash = false;

    private bool isDashing = false;

    public float speed = 2f;
    public float dashSpeed = 4f;
    public float moveForce = 365f;
    public float maxSpeed = 5f;
    public float jumpForce = 1000f;
    public float dashFall = 30f;
    public LayerMask groundLayer;
    public float WaitTimeToRejump = 0.3f;
    public float WaitTimeToReDash = 0.3f;
    public float DashTime = 0.2f;

    public GameObject SpriteJumpSmoke;
    public GameObject ParticleToSpawnOnJump;
    public Transform SpawnTransform;

    private bool grounded = false;
    private bool jumpable;
    private bool slowDownDash = false;
    private Animator anim;
    private Rigidbody2D rb;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        jumpable = true;
    }

    void Update()
    {
        bool groundedLastFrame = grounded;
        grounded = (Physics2D.Raycast(transform.position, Vector2.down, 1.0f, groundLayer).collider != null)
            ? true
            : false;

        if (groundedLastFrame != grounded && grounded)
            StartCoroutine(WaitBeforeJumpable());

        if (grounded && anim.GetCurrentAnimatorStateInfo(0).IsName("PlayerFalling"))
            anim.SetTrigger("TouchGround");
        else
            anim.ResetTrigger("TouchGround");

        if (Input.GetButtonDown("Jump") && grounded && jumpable && !(Input.GetAxis("Vertical") < 0))
        {
            jump = true;
        }
            
        if (Input.GetButtonDown("Jump") && (Input.GetAxis("Vertical") < 0))
        {
            dash = true;
        }
    }

    void FixedUpdate()
    {
        anim.SetFloat("Speed", Mathf.Abs(speed));
        anim.SetFloat("VelocityFalling", GetComponent<Rigidbody2D>().velocity.y);

        if (GetComponent<Rigidbody2D>().velocity.y < 0)
            jumpable = false;

        if (!isDashing)
        {
            if (speed * GetComponent<Rigidbody2D>().velocity.x < maxSpeed)
                transform.Translate(Vector3.right * Time.deltaTime * speed);

            if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }

        if (dash && !isDashing)
            StartCoroutine(Dash(DashTime));

        if(slowDownDash)
        {
            rb.AddForce(Vector2.down * dashFall * Time.deltaTime, ForceMode2D.Force);
            if(Mathf.Abs(rb.velocity.x) < speed)
            {
                slowDownDash = false;
            }
        }

        if (jump)
        {
            Instantiate(ParticleToSpawnOnJump, SpawnTransform.position, Quaternion.identity);
            Instantiate(SpriteJumpSmoke, SpawnTransform.position, Quaternion.identity);
            anim.SetTrigger("Jump");
            rb.AddForce(new Vector2(0f, jumpForce));
            jump = false;
            jumpable = false;
        }
    }

    private IEnumerator Dash(float dashDur)
    {
        float time = 0;
        isDashing = true;
        dash = false;

        while (dashDur > time)
        {
            time += Time.deltaTime;
            rb.velocity = new Vector2(dashSpeed, 0);
            yield return null;
        }

        StartCoroutine(WaitBeforeDash());
        slowDownDash = true;
    }

    private IEnumerator WaitBeforeJumpable()
    {
        yield return new WaitForSeconds(WaitTimeToRejump);
        jumpable = true;
    }

    private IEnumerator WaitBeforeDash()
    {
        yield return new WaitForSeconds(WaitTimeToReDash);
        isDashing = false;
    }

}