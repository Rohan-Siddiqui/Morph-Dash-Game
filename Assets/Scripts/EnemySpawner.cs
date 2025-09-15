using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    //[SerializeField] private Transform[] points;
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private float spawnInterval = 5f;
    public int count = 0;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StartCoroutine(Spawn());
    }
    private void LateUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.isMainMenuActive) return;

        if (count < 18)
        {
            StartCoroutine(Spawn());
        }
    }
    private IEnumerator Spawn()
    {
        SpawnEnemy();
        yield return new WaitForSeconds(spawnInterval);
    }
    private void SpawnEnemy()
    {
        List<int> availableEnemy = new List<int>(); // game manager sa select quantity la kar enemy spawn

        for (int i = 0; i < enemies.Length; i++)
        {
            if (i < GameManager.Instance.selectShapesQuantity)
            {
                availableEnemy.Add(i);
            }
        }
        if (availableEnemy.Count == 0)
            return;

        Vector2 spawnPosition = GetRandomSpawnPosition();
        int randomEnemy = Random.Range(0, availableEnemy.Count);
        GameObject enemy = Instantiate(enemies[randomEnemy], spawnPosition, Quaternion.identity);
        float currentSpeed = GameManager.Instance.enemySpeed;
        enemy.GetComponent<Enemy>().SetSpeed(currentSpeed);
        count++;
    }
    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 playerPos = Player.Instance.transform.position;
        int rand = Random.Range(0, 4);
        float x = 0f;
        float y = 0f;

        switch(rand)
        {
            case 0: // left
                x = Random.Range(playerPos.x - 10, playerPos.x - 30);
                y = Random.Range(playerPos.y - 30, playerPos.y + 30);
                break;
            case 1: // Right
                x = Random.Range(playerPos.x + 10, playerPos.x + 30);
                y = Random.Range(playerPos.y - 30, playerPos.y + 30);
                break;
            case 2: // Up
                x = Random.Range(playerPos.x - 30, playerPos.x + 30);
                y = Random.Range(playerPos.y + 10, playerPos.y + 30);
                break;
            case 3:
                x = Random.Range(playerPos.x - 30, playerPos.x + 30);
                y = Random.Range(playerPos.y - 30, playerPos.y - 10);
                break;
        }
        return new Vector2(x, y);
    }
}
