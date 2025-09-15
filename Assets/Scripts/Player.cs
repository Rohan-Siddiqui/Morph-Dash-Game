using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private float speed = 5f;
    [SerializeField] private FixedJoystick joystick;
    [SerializeField] private float interactionRadius = 5f;
    [SerializeField] private Color gizmozColor = Color.green;

    private new Rigidbody2D rigidbody;
    private void Awake()
    {
        Instance = this;
        rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        CheckMobilePlatform();
    }
    private void CheckMobilePlatform()
    {
        if(Application.isMobilePlatform)
        {
            joystick.gameObject.SetActive(true);
        }
        else
        {
            joystick.gameObject.SetActive(false);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmozColor;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
    private void FixedUpdate()
    {
        PlayerMovement();
    }
    public void PlayerMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        if(Mathf.Abs(moveHorizontal) < 0.1f && Mathf.Abs(moveVertical) < 0.1f)
        {
            moveHorizontal = joystick.Horizontal;
            moveVertical = joystick.Vertical;
        }
        Vector3 move = new Vector2 (moveHorizontal, moveVertical);
        bool moving = move.magnitude > 0.1f;
        rigidbody.velocity = move.normalized * speed;
        if (moving)
        {
            float angle = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
            rigidbody.rotation = angle;
        }
    }
}
