using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float horizontalSpeed = 7.5f;
    [SerializeField] private float jumpVelocity = 12.5f;
    [SerializeField] private float gravityScale = 3.3f;
    [SerializeField] private float horizontalWrapLimit = 6f;

    private readonly HashSet<Collider2D> groundColliders = new();
    private Rigidbody2D body;
    private float horizontalInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody2D>();
        }

        body.gravityScale = gravityScale;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        horizontalInput = 0f;

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalInput = 1f;
        }

        bool wantsToJump = Keyboard.current.spaceKey.wasPressedThisFrame
            || Keyboard.current.wKey.wasPressedThisFrame
            || Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (wantsToJump && groundColliders.Count > 0)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
            groundColliders.Clear();
        }
    }

    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(horizontalInput * horizontalSpeed, body.linearVelocity.y);

        Vector2 position = body.position;
        if (position.x > horizontalWrapLimit)
        {
            body.position = new Vector2(-horizontalWrapLimit, position.y);
        }
        else if (position.x < -horizontalWrapLimit)
        {
            body.position = new Vector2(horizontalWrapLimit, position.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TrackGroundContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TrackGroundContact(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundColliders.Remove(collision.collider);
    }

    private void OnDisable()
    {
        groundColliders.Clear();
    }

    public void ResetMotion(Vector3 position)
    {
        groundColliders.Clear();
        horizontalInput = 0f;
        body.position = position;
        body.linearVelocity = Vector2.zero;
        transform.position = position;
    }

    private void TrackGroundContact(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                groundColliders.Add(collision.collider);
                return;
            }
        }
    }
}
