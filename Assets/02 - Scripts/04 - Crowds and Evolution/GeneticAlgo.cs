using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GeneticAlgo : MonoBehaviour
{

    [Header("Genetic Algorithm parameters")]
    public int animal_popSize = 100;
    public GameObject animalPrefab;
    public int predator_popSize = 20;
    public GameObject predatorPrefab;   // predator

    [Header("Dynamic elements")]
    public float vegetationGrowthRate = 0.1f;
    public float currentGrowth;

    private List<GameObject> animals;
    private List<GameObject> predators;
    protected Terrain terrain;
    protected CustomTerrain customTerrain;
    protected float width;
    protected float height;

    private System.Random random;

    // Record
    private int frame = 0;
    private int recordFreq = 6;
    private List<int> animalNum;
    private List<int> predatorNum;
    private List<float> energyAnimal;
    private List<float> energyPredator;
    private List<float> visionAnimal;
    private List<float> visionPredator;
    private List<float> speedAnimal;
    private List<float> speedPredator;
    private List<double> reproduceProbAnimal;
    private List<double> reproduceProbPredator;

    void Start()
    {
        // limit the FPS
        Time.captureFramerate = 10;

        // Initialization
        animal_popSize = 250;
        predator_popSize = 80;
        vegetationGrowthRate = 1.0f;

        random = new System.Random();

        // Init record
        frame = 0;
        animalNum = new List<int>();
        predatorNum = new List<int>();
        energyAnimal = new List<float>();
        energyPredator = new List<float>();
        visionAnimal = new List<float>();
        visionPredator = new List<float>();
        speedAnimal = new List<float>();
        speedPredator = new List<float>();
        reproduceProbAnimal = new List<double>();
        reproduceProbPredator = new List<double>();

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
        frame += 1;

        // Keeps animal to a minimum.
        //while (animals.Count < animal_popSize / 4)
        //{
        //    animals.Add(makeAnimal());
        //}
        customTerrain.debug.text = "#N herbivore: " + animals.Count.ToString();

        //while (predators.Count < predator_popSize / 4)
        //{
        //    predators.Add(makePredator());
        //}
        customTerrain.debug.text += "\n#N carnivore: " + predators.Count.ToString();

        // Update grass elements/food resources.
        updateResources();

        // record the info
        if (frame % recordFreq == 0)
            Record();

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveResult();
        }
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
        ani.SetAnimalType(true);
        ani.Setup(customTerrain, this);
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
        ani.SetAnimalType(false);
        ani.Setup(customTerrain, this);
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
        GameObject animal;
        if (parent.GetAnimalType())
        {
            animal = makeAnimal(parent.transform.position);
            animals.Add(animal);
        }
        else
        {
            animal = makePredator(parent.transform.position);
            predators.Add(animal);
        }

        // var ani = animal.GetComponent<Animal>();
        // ani.InheritBrain(parent.GetBrain(), true);

        // animal.GetComponent<Animal>().Mutate(parent.GetEnergyMAX(), parent.GetVisionRange(), parent.GetSize(), parent.GetSpeed(), parent.GetReproduceProba());
        // Mutate(animal.GetComponent<Animal>(), parent.GetMutateRate(), parent.GetEnergyMAX(), parent.GetVisionRange(), parent.GetSize(), parent.GetSpeed(), parent.GetReproduceProba());
    }

    /// <summary>
    /// Remove instance of an animal.
    /// </summary>
    /// <param name="animal"></param>
    public void removeAnimal(Animal animal)
    {
        if (animal.GetAnimalType())
            animals.Remove(animal.transform.gameObject);
        else
            predators.Remove(animal.transform.gameObject);
        
        Destroy(animal.transform.gameObject);
    }

    public List<GameObject> getAnimals()
    {
        return animals;
    }

    public List<GameObject> getPredators()
    {
        return predators;
    }

    private void Mutate(Animal animal, float mutateRate, float energy_parent, float vision_parent, float size_parent, float speed_parent, double reproduce_prob_parent)
    {
        //energy_MAX = (float)Gaussian(energy_parent, energy_parent * mutateRate);
        //energy = energy_MAX;

        //reproduceProb = Gaussian(reproduce_prob_parent, reproduce_prob_parent * mutateRate);
        //visionRange = (float)Gaussian(vision_parent, vision_parent * mutateRate);
        //size = (float)Gaussian(size_parent, size_parent * mutateRate);
        //speed = (float)Gaussian(speed_parent, speed_parent * mutateRate);

        animal.SetEnergyMAX(energy_parent * (float)(1 + mutateRate * Uniform()));
        animal.SetReproduceProba(reproduce_prob_parent * (float)(1 + mutateRate * Uniform()));
        animal.SetVisionRange(vision_parent * (float)(1 + mutateRate * Uniform()));
        animal.SetSize(size_parent * (float)(1 + mutateRate * Uniform()));
        animal.SetSpeed(speed_parent * (float)(1 + mutateRate * Uniform()));
    }

    private double Gaussian(double mean, double std)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double standardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + std * standardNormal;
    }

    private double Uniform()
    {
        return (random.NextDouble() * 2) - 1;   // [-1, 1]
    }

    private void Record()
    {
        animalNum.Add(animals.Count);
        predatorNum.Add(predators.Count);

        float energy_animal_sum = 0.0f;
        float vision_animal_sum = 0.0f;
        float speed_animal_sum = 0.0f;
        double reproduce_pro_animal_sum = 0.0;

        for (int i=0; i<animals.Count; i++)
        {
            var animal = animals[i].GetComponent<Animal>();
            energy_animal_sum += animal.GetEnergyMAX();
            vision_animal_sum += animal.GetVisionRange();
            speed_animal_sum += animal.GetSpeed();
            reproduce_pro_animal_sum += animal.GetReproduceProba();

            //Debug.Log("     Energy: " + animal.GetEnergyMAX());
            //Debug.Log("     Vision: " + animal.GetVisionRange());
        }

        if (animals.Count > 0)
        {
            energyAnimal.Add(energy_animal_sum / animals.Count);
            visionAnimal.Add(vision_animal_sum / animals.Count);
            speedAnimal.Add(speed_animal_sum / animals.Count);
            reproduceProbAnimal.Add(reproduce_pro_animal_sum / animals.Count);
        }
        else
        {
            energyAnimal.Add(0);
            visionAnimal.Add(0);
            speedAnimal.Add(0);
            reproduceProbAnimal.Add(0);
        }

        Debug.Log("Energy Max: " + energy_animal_sum / animals.Count);
        Debug.Log("Vision: " + vision_animal_sum / animals.Count);

        float energy_predator_sum = 0.0f;
        float vision_predator_sum = 0.0f;
        float speed_predator_sum = 0.0f;
        double reproduce_pro_predator_sum = 0.0;

        for (int i=0; i<predators.Count; i++)
        {
            var predator = predators[i].GetComponent<Animal>();
            energy_predator_sum += predator.GetEnergyMAX();
            vision_predator_sum += predator.GetVisionRange();
            speed_predator_sum += predator.GetSpeed();
            reproduce_pro_predator_sum += predator.GetReproduceProba();
        }

        if (predators.Count > 0)
        {
            energyPredator.Add(energy_predator_sum / predators.Count);
            visionPredator.Add(vision_predator_sum / predators.Count);
            speedPredator.Add(speed_predator_sum / predators.Count);
            reproduceProbPredator.Add(reproduce_pro_predator_sum / predators.Count);
        }
        else
        {
            energyPredator.Add(0);
            visionPredator.Add(0);
            speedPredator.Add(0);
            reproduceProbPredator.Add(0);
        }
        
    }

    private void SaveResult()
    {
        string path = Path.Combine(Application.dataPath, "OUTPUT", "AnimalNumber.txt"); // Define the file path.
        // Write the data to the file.
        string dataToSave = string.Join("\n", animalNum.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "PredatorNumber.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", predatorNum.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "AnimalEnergy.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", energyAnimal.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "PredatorEnergy.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", energyPredator.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "AnimalVision.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", visionAnimal.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "PredatorVision.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", visionPredator.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "AnimalSpeed.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", speedAnimal.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "PredatorSpeed.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", speedPredator.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "AnimalReproduceProb.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", reproduceProbAnimal.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);

        path = Path.Combine(Application.dataPath, "OUTPUT", "PredatorReproduceProb.txt"); // Define the file path.
        // Write the data to the file.
        dataToSave = string.Join("\n", reproduceProbPredator.ToArray());
        File.WriteAllText(path, dataToSave);
        Debug.Log("Data saved to: " + path);
    }

}
