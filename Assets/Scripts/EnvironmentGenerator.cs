using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnvironmentGenerator : MonoBehaviour
{
    public List<GameObject> environmentPrefabs = new List<GameObject>();

    private List<GameObject> instances = new List<GameObject>();

    public List<Collider> restrictedBounds = new List<Collider>();

    public int numObjects = 30;

    public Vector3 generatorBoundsMin = new Vector3(-30, 0, -30);

    public Vector3 generatorBoundsMax = new Vector3(30, 0, 30);

    public bool reset = false;

    // Start is called before the first frame update
    void Start()
    {
        // 1.1.6
        GenerateEnvironment();
    }

    // Update is called once per frame
    void Update()
    {
        // Your code for Exercise 1.1 part 3.) here
    }

    void ClearEnvironment()
    {
        // Your code for Exercise 1.1 part 3.) here
    }

    bool isEmpty<T>(List<T> list)
    {
        return list.Count == 0;
    }

#nullable enable
    GameObject? getRandomGameObjectFrom(List<GameObject> list)
    {
        if (isEmpty(list))
        {
            return null;
        }

        var random = new System.Random();

        int randomIndex = random.Next(0, list.Count);

        GameObject randomGameObject = list[randomIndex];

        return randomGameObject;
    }

    float getRandomFloatBetween(float firstFloat, float secondFloat)
    {
        float smallerFloat = Mathf.Min(firstFloat, secondFloat);
        float largerFloat = Mathf.Max(firstFloat, secondFloat);

        float randomFloat = Random.Range(smallerFloat, largerFloat);

        return randomFloat;
    }

    Vector3 getRandomVectorBetween(Vector3 firstVector, Vector3 secondVector)
    {
        float randomX = getRandomFloatBetween(firstVector.x, secondVector.x);
        float randomY = getRandomFloatBetween(firstVector.y, secondVector.y);
        float randomZ = getRandomFloatBetween(firstVector.z, secondVector.z);

        Vector3 randomVector = new Vector3(randomX, randomY, randomZ);

        return randomVector;
    }

    void moveObjectByVector(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.position += vector;
    }

    void rotateObjectByVector(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.Rotate(vector);
    }

    void rotateObjectsYAxisRandomly(GameObject gameObject)
    {
        /**
         * @TODO: Find out what they mean by "[...] instances’ up-Vector".
         * 
         * Sounds like the game object's Y-axis, so that's what I'll do for now.
         */
        Vector3 rotationVector = new Vector3(
            0,
            getRandomFloatBetween(1, 360),
            0);

        rotateObjectByVector(gameObject, rotationVector);
    }

    List<GameObject> addNRandomGameObjectsTo(
        List<GameObject> list,
        int times)
    {
        // @TODO: There's "Enumerable.Repeat" we could use instead of pesky for-loops.
        for (int index = 0; index < times; index++)
        {
            //  1.1.1
            GameObject? randomPrefab = getRandomGameObjectFrom(environmentPrefabs);

            // In case we act stupid.
            if (!randomPrefab)
            {
                throw new System.Exception("The given list of prefabs is empty.");
            }

            /**
             * 1.1.2
             * 
             * @TODO: Where is the "Prop Generator.gameObject" they talk about under arrow 2?
             */
            GameObject instantiatedGameObject = Instantiate(randomPrefab!);

            // 1.2.3
            moveObjectByVector(
                instantiatedGameObject,
                getRandomVectorBetween(generatorBoundsMin, generatorBoundsMax));

            rotateObjectsYAxisRandomly(instantiatedGameObject);

            // 1.2.4
            list.Add(instantiatedGameObject);
        }

        return list;
    }

    void GenerateEnvironment()
    {
        addNRandomGameObjectsTo(instances, numObjects);

        StartCoroutine(ResolveCollisions());
    }

    IEnumerator ResolveCollisions()
    {
        yield return new WaitForSeconds(2);
        bool resolveAgain = false;
        // Your code for Exercise 1.1 part 2.) here
        if (resolveAgain)
            StartCoroutine(ResolveCollisions());
    }
}
