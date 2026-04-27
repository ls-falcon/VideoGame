using System.Collections.Generic;
using UnityEngine;

public class enemyMovement : MonoBehaviour
{
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float enemySpeed = 2f;

    void Start()
    {

    }

    void Update()
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, playerPosition.position, enemySpeed * Time.deltaTime);
    }
}
