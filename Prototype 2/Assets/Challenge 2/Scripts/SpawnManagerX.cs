using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerX : MonoBehaviour
{
    public GameObject[] ballPrefabs;

    private float spawnLimitXLeft = -22;
    private float spawnLimitXRight = 7;
    private float spawnPosY = 30;

    private float spawnMinTime = 0.3f;
    private float spawnMaxTime = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("SpawnRandomBall", 0);
    }

    // Spawn random ball at random x position at top of play area
    void SpawnRandomBall ()
    {
        // Generate random ball index and random spawn position
        Vector3 spawnPos = new Vector3(Random.Range(spawnLimitXLeft, spawnLimitXRight), spawnPosY, 0);

        // Get random ball
        int rand_ball = Random.Range(0, ballPrefabs.Length);

        // instantiate ball at random spawn location
        Instantiate(ballPrefabs[rand_ball].gameObject, spawnPos, ballPrefabs[rand_ball].transform.rotation);

        // Get a random time to respawn ball
        float rand_respawn = Random.Range(spawnMinTime, spawnMinTime);

        Invoke("SpawnRandomBall", rand_respawn);
    }

}
