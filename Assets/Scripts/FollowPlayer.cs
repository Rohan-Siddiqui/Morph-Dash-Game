using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    private void LateUpdate()
    {
        if (player != null)
        {
            this.transform.position = player.position + offset;
        }
    }
}
