using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlgo : MonoBehaviour
{

    [Header("Genetic Algorithm parameters")]
    public int animal_popSize = 100;
    public GameObject animalPrefab;
    public int predator_popSize = 20;
    public GameObject predatorPrefab;   // predator

    [Header("Dynamic elements")]
    public float vegetationGrowthRate = 1.0f;
    public float currentGrowth;

    private List<GameObject> animals;
    private List<GameObject> predators;
    protected Terrain terrain;
    protected CustomTerrain customTerrain;
    protected float width;
    protected float height;

    void Start()
    {
        // limit the FPS
        Time.captureFramerate = 25;

        // Retrieve terrain.
        terrain = Terrain.activeTerrain;
        customTerrain = GetComponent<CustomTerrain>();
        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;

        // Initialize terrain growth.
        currentGrowth = 0.0f;

        // Initialize animals array.
        animals = new List<GameObject>();
        for (int i = 0; i < animal_popSize; i++)
        {
            GameObject animal = makeAnimal();
            animals.Add(animal);
        }

        predators = new List<GameObject>();
        for (int i = 0; i < predator_popSize; i++)
        {
            GameObject predator = makePredator();
            predators.Add(predator);
        }
    }

    void Update()
    {
        // Keeps animal to a minimum.
        while (animals.Count < animal_popSize / 2)
        {
            animals.Add(makeAnimal());
        }
        customTerrain.debug.text = "#N animals: " + animals.Count.ToString();

        while (predators.Count < predator_popSize / 2)
        {
            predators.Add(makePredator());
        }
        customTerrain.debug.text += "\n#N predators: " + predators.Count.ToString();

        // Update grass elements/food resources.
        updateResources();
    }

    /// <summary>
    /// Method to place grass or other resource in the terrain.
    /// </summary>
    public void updateResources()
    {
        Vector2 detail_sz = customTerrain.detailSize();
        int[,] details = customTerrain.getDetails();
        currentGrowth += vegetationGrowthRate;
        while (currentGrowth > 1.0f)
        {
            int x = (int)(UnityEngine.Random.value * detail_sz.x);
            int y = (int)(UnityEngine.Random.value * detail_sz.y);
            details[y, x] = 1;
            currentGrowth -= 1.0f;
        }
        customTerrain.saveDetails();
    }

    /// <summary>
    /// Method to instantiate an animal prefab. It must contain the animal.cs class attached.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject makeAnimal(Vector3 position)
    {
        GameObject animal = Instantiate(animalPrefab, transform);
        var ani = animal.GetComponent<Animal>();
        ani.Setup(customTerrain, this);
        ani.if_animal = true;
        animal.transform.position = position;
        animal.transform.Rotate(0.0f, UnityEngine.Random.value * 360.0f, 0.0f);
        
        return animal;
    }

    /// <summary>
    /// If makeAnimal() is called without position, we randomize it on the terrain.
    /// </summary>
    /// <returns></returns>
    public GameObject makeAnimal()
    {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = UnityEngine.Random.value * width;
        float z = UnityEngine.Random.value * height;
        float y = customTerrain.getInterp(x / scale.x, z / scale.z);
        return makeAnimal(new Vector3(x, y, z));
    }

    public GameObject makePredator(Vector3 position)
    {
        GameObject predator = Instantiate(predatorPrefab, transform);
        var ani = predator.GetComponent<Animal>();
        ani.Setup(customTerrain, this);
        ani.if_animal = false;
        predator.transform.position = position;
        predator.transform.Rotate(0.0f, UnityEngine.Random.value * 360.0f, 0.0f);

        return predator;
    }

    public GameObject makePredator()
    {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = UnityEngine.Random.value * width;
        float z = UnityEngine.Random.value * height;
        float y = customTerrain.getInterp(x / scale.x, z / scale.z);
        return makePredator(new Vector3(x, y, z));
    }

    /// <summary>
    /// Method to add an animal inherited from anothed. It spawns where the parent was.
    /// </summary>
    /// <param name="parent"></param>
    public void addOffspring(Animal parent)
    {
        GameObject animal = makeAnimal(parent.transform.position);
        var ani = animal.GetComponent<Animal>();
        ani.InheritBrain(parent.GetBrain(), true);
        ani.if_animal = parent.if_animal;
        if (ani.if_animal == true)
        {
            animals.Add(animal);
        }
        else
        {
            predators.Add(animal);
        }
        
    }

    /// <summary>
    /// Remove instance of an animal.
    /// </summary>
    /// <param name="animal"></param>
    public void removeAnimal(Animal animal)
    {
        if (animal.if_animal == true)
        {
            animals.Remove(animal.transform.gameObject);
        }
        else
        {
            predators.Remove(animal.transform.gameObject);
        }
        
        Destroy(animal.transform.gameObject);
    }

}
