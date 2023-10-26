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
        // Your code for Exercise 1.1 part 1.) here
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

    void GenerateEnvironment()
    {
        // Your code for Exercise 1.1 part 1.) here

        var randomPrefab = getRandomGameObjectFrom(environmentPrefabs);

        if (!randomPrefab)
        {
            throw new System.Exception("The given list of prefabs is empty.");
        }

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
