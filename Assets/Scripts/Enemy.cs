using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //[SerializeField] private float enemySpeed = 2f;

    private GameObject currentTarget;
    private float arrivalThreshold = 0.2f;
    private bool waitingForNewTarget = false;
    private Transform playerTarget;

    private float enemySpeed = 2f;
    private void Start()
    {
        PickNewTarget();
    }
    public void SetSpeed(float newSpeed)
    {
        enemySpeed = newSpeed;
    }
    private void FixedUpdate()
    {
        if (waitingForNewTarget)
            return;

        Vector3 targetPosition = (playerTarget != null)
            ? playerTarget.position
            : (currentTarget != null ? currentTarget.transform.position : transform.position);

        // Move toward current target
        Vector3 direction = (targetPosition - transform.position).normalized;
        if(direction != Vector3.zero )
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        transform.position += direction * enemySpeed * Time.deltaTime;

        // Check if we’ve arrived
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance < arrivalThreshold)
        {
            GameManager.Instance.ReleasePoint(currentTarget);
            currentTarget = null;
            StartCoroutine(WaitAndPickNewTarget());
        }
    }

    private IEnumerator WaitAndPickNewTarget()
    {
        waitingForNewTarget = true;

        // Wait a small amount of time to avoid double-picking in the same frame
        yield return new WaitForSeconds(0.1f);

        PickNewTarget();
        waitingForNewTarget = false;
    }

    void PickNewTarget()
    {
        GameObject newTarget = GameManager.Instance.GetAvailablePoint();
        if (newTarget != null)
        {
            currentTarget = newTarget;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player") // when enemy in the range of player
        {
            if(currentTarget != null)
            {
                GameManager.Instance.ReleasePoint(currentTarget);
                currentTarget = null;
            }
            playerTarget = collision.transform;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(playerTarget != null)
            {
                PickNewTarget();
                playerTarget = null;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            GameManager.Instance.CheckId(this.gameObject);
            print("Collide");
        }
    }
}
