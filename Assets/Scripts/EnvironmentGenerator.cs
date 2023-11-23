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
        GenerateEnvironment();
    }

    // Update is called once per frame
    void Update()
    {
        if(reset)
        {
            reset = false;
            ClearEnvironment();
            GenerateEnvironment();
        }

    }

    void ClearEnvironment()
    {
        foreach (var instance in instances)
            Destroy(instance);
        instances.Clear();
    }

    void GenerateEnvironment()
    {
        while(instances.Count < numObjects)
        {
            var instance = Instantiate(environmentPrefabs[Random.Range(0, environmentPrefabs.Count)]);
            instance.transform.SetParent(gameObject.transform);
            ApplyRandomInstanceTransform(instance);
            instances.Add(instance);
        }
        StartCoroutine(ResolveCollisions());
    }

    IEnumerator ResolveCollisions()
    {
        yield return new WaitForSeconds(2);
        bool resolveAgain = false;
        foreach(var instance in instances)
        {
            if(IsInRestrictedBounds(instance.GetComponent<Collider>()))
            {
                ApplyRandomInstanceTransform(instance);
                resolveAgain = true;
            }
        }
        if (resolveAgain)
            StartCoroutine(ResolveCollisions());
    }

    bool IsInRestrictedBounds(Collider co)
    {
        if (co == null)
            return false;
        foreach(var c in restrictedBounds)
        {
            if (c.bounds.Intersects(co.bounds))
                return true;
        }
        return false;
    }

    void ApplyRandomInstanceTransform(GameObject instance)
    {
        instance.transform.position = CalculateRandomInstancePosition();
        instance.transform.rotation = CalculateRandomInstanceRotation();
    }

    Vector3 CalculateRandomInstancePosition()
    {
        return new Vector3(
            Random.Range(generatorBoundsMin.x, generatorBoundsMax.x),
            Random.Range(generatorBoundsMin.y, generatorBoundsMax.y),
            Random.Range(generatorBoundsMin.z, generatorBoundsMax.z)
        );
    }

    Quaternion CalculateRandomInstanceRotation()
    {
        return Quaternion.Euler(0, Random.Range(-90f, 90f), 0);
    }
}
