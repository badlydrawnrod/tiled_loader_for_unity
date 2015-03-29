using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public float horizontalSpeed = 120;
    public float jumpSpeed = 180;
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode jumpKey = KeyCode.UpArrow;

    private float desiredSpeed;

    void Update()
    {
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (Input.GetKeyDown(jumpKey))
        {
            body.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        }

        desiredSpeed = 0;
        if (Input.GetKey(leftKey))
        {
            desiredSpeed = -horizontalSpeed;
        }
        if (Input.GetKey(rightKey))
        {
            desiredSpeed = horizontalSpeed;
        }
    }

    void FixedUpdate()
    {
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        var velocity = body.velocity;
        var dx = desiredSpeed - velocity.x;
        body.AddForce(Vector2.right * dx, ForceMode2D.Impulse);
    }
}
