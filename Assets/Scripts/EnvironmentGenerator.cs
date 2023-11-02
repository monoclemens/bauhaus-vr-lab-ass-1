using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGenerationHelper : MonoBehaviour
{
    private readonly System.Random random = new();

    public void DestroyAll(List<GameObject> gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            Destroy(gameObject);
        }
    }

    private bool IsEmpty<T>(List<T> list)
    {
        return list.Count == 0;
    }

#nullable enable
    private GameObject? GetRandomGameObjectFrom(List<GameObject> list)
    {
        if (IsEmpty(list)) return null;

        int randomIndex = random.Next(0, list.Count);

        GameObject randomGameObject = list[randomIndex];

        return randomGameObject;
    }

    private float GetRandomFloatBetween(float firstFloat, float secondFloat)
    {
        float smallerFloat = Mathf.Min(firstFloat, secondFloat);
        float largerFloat = Mathf.Max(firstFloat, secondFloat);

        float randomFloat = Random.Range(smallerFloat, largerFloat);

        return randomFloat;
    }

    public Vector3 GetRandomVectorBetween(Vector3 firstVector, Vector3 secondVector)
    {
        float randomX = GetRandomFloatBetween(firstVector.x, secondVector.x);
        float randomY = GetRandomFloatBetween(firstVector.y, secondVector.y);
        float randomZ = GetRandomFloatBetween(firstVector.z, secondVector.z);

        Vector3 randomVector = new Vector3(randomX, randomY, randomZ);

        return randomVector;
    }

    public void MoveObjectByVector(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.position += vector;
    }

    private void RotateObjectByVector(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.Rotate(vector);
    }

    private void RotateObjectsYAxisRandomly(GameObject gameObject)
    {
        Vector3 rotationVector = new(
             0,
             GetRandomFloatBetween(0, 360),
             0);

        RotateObjectByVector(gameObject, rotationVector);
    }

    public List<GameObject> PopulateWorld(
        List<GameObject> prefabs,
        List<GameObject> instances,
        Vector3 boundsMin,
        Vector3 boundsMax,
        int times)
    {
        for (int index = 0; index < times; index++)
        {
            //  1.1.1
            GameObject? randomPrefab = GetRandomGameObjectFrom(prefabs);

            if (randomPrefab == null)
            {
                throw new System.Exception("The given list of prefabs is empty.");
            }

            GameObject instantiatedGameObject = Instantiate(randomPrefab, gameObject.transform);

            // 1.2.3
            MoveObjectByVector(
                instantiatedGameObject,
                GetRandomVectorBetween(boundsMin, boundsMax));

            RotateObjectsYAxisRandomly(instantiatedGameObject);

            // 1.2.4
            instances.Add(instantiatedGameObject);
        }

        return instances;
    }

}

public class EnvironmentGenerator : MonoBehaviour
{
    public List<GameObject> environmentPrefabs = new();

    private readonly List<GameObject> instances = new();

    public List<Collider> restrictedBounds = new();

    public int numObjects = 30;

    public Vector3 generatorBoundsMin = new(-30, 0, -30);

    public Vector3 generatorBoundsMax = new(30, 0, 30);

    public bool reset = false;

    private readonly EnvironmentGenerationHelper helper = new();

    // Start is called before the first frame update
    void Start()
    {
        // 1.1.6
        GenerateEnvironment();
    }

    // Update is called once per frame
    void Update()
    {
        if (reset)
        {
            ClearEnvironment();

            GenerateEnvironment();

            reset = false;
        }
    }

    void ClearEnvironment()
    {
        helper.DestroyAll(instances);

        instances.Clear();
    }

    void GenerateEnvironment()
    {
        helper.PopulateWorld(
            environmentPrefabs,
            instances,
            generatorBoundsMin,
            generatorBoundsMax,
            numObjects
        );

        StartCoroutine(ResolveCollisions());
    }

    /**
     * This method resolves collisions between newly created game objects and the central house.
     * 
     * If there is a collision, it will simply move the colliding game object and check for collisions again.
     */
    IEnumerator ResolveCollisions()
    {
        yield return new WaitForSeconds(2);

        bool resolveAgain = false;

        // Your code for Exercise 1.1 part 2.) here
        foreach (var gameObject in instances)
        {
            Collider gameObjectCollider = gameObject.GetComponent<Collider>();

            foreach (var restrictedCollider in restrictedBounds)
            {
                if (gameObjectCollider.bounds.Intersects(restrictedCollider.bounds))
                {
                    helper.MoveObjectByVector(
                        gameObject,
                        helper.GetRandomVectorBetween(generatorBoundsMin, generatorBoundsMax));

                    resolveAgain = true;
                }
            }
        }

        if (resolveAgain)
            StartCoroutine(ResolveCollisions());
    }
}
