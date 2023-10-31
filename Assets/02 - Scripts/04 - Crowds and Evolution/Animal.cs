using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animal : MonoBehaviour
{
    // below config not working
    [Header("Animal parameters")]
    public float swapRate = 0.01f;
    public float mutateRate = 0.1f;
    public float swapStrength = 10.0f;
    public float mutateStrength = 0.5f;
    public float maxAngle = 10.0f;
    public float maxSpeed = 0.5f;
    private float speed;

    [Header("Energy parameters")]
    public float maxEnergy = 100.0f;
    public float lossEnergy = 0.1f;
    public float gainEnergy = 10.0f;
    public float hunger = 0.8f;
    private float energy_MAX;
    private float energy;

    [Header("Sensor - Vision")]
    public float maxVision = 20.0f;
    public float stepAngle = 10.0f;
    public int nEyes = 5;
    private float visionRange;

    [Header("Size parameters")]
    public float maxSize = 5.0f;
    private float size;

    [Header("Predator parameters")]
    // type for animal or predator
    public bool if_animal;

    // private int[] networkStruct;
    // private SimpleNeuralNet brain = null;

    private static System.Random random = new System.Random();

    // Terrain.
    private CustomTerrain terrain = null;
    private int[,] details = null;
    private Vector2 detailSize;
    private Vector2 terrainSize;

    // Animal.
    private Transform tfm;
    private float[] vision;

    // Genetic alg.
    private GeneticAlgo genetic_algo = null;

    // Renderer.
    private Material mat = null;

    // display info
    private float displayDuration = 2.0f; // Duration in seconds to display the text.
    private int fontSize = 200;

    private string displayText = "I'm eating!";
    private float eatDisplayStartTime;
    private bool isEatDisplaying;
    private Text eatLabel;

    // motion controller
    private CapsuleAutoController capsule_controller;
    private HumanoidAutoController human_controller;

    void Start()
    {
        // Initialization
        mutateRate = 0.1f;
        maxSpeed = 0.5f;
        maxEnergy = 100.0f;
        lossEnergy = 0.5f;
        gainEnergy = 20f;
        maxVision = 10.0f;
        maxSize = 5.0f;
        hunger = 0.7f;


        // Network: 1 input per receptor, 1 output per actuator.
        // vision = new float[nEyes];
        // networkStruct = new int[] { nEyes, 5, 1 };
        energy_MAX = maxEnergy;
        energy = energy_MAX;
        tfm = transform;
        visionRange = maxVision;
        size = maxSize;
        speed = maxSpeed;

        // Renderer used to update animal color.
        // It needs to be updated for more complex models.
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
            mat = renderer.material;

        //// initial display info
        //isEatDisplaying = false;
        //eatLabel.enabled = false;
        //eatLabel.fontSize = fontSize;

    }

    void Update()
    {
        // In case something is not initialized...
        //if (brain == null)
        //     brain = new SimpleNeuralNet(networkStruct);
        if (terrain == null)
            return;
        if (details == null)
        {
            UpdateSetup();
            return;
        }

        // For each frame, we lose lossEnergy
        energy -= lossEnergy;

        // If the position can eat something
        if (energy / energy_MAX <= hunger)
        {
            if (if_animal)
            {

                EatGrass();
            }
            else
            {
                EatMeat();
            }
        }

        // If the energy is below 0, the animal dies.
        if (energy < 0)
        {
            energy = 0.0f;
            genetic_algo.removeAnimal(this);
        }

        // Update the color of the animal as a function of the energy that it contains.
        if (mat != null)
            mat.color = Color.white * (energy / energy_MAX);

        //// 1. Update receptor.
        //UpdateVision();

        //// 2. Use brain.
        //float[] output = brain.getOutput(vision);

        //// 3. Act using actuators.
        //float angle = (output[0] * 2.0f - 1.0f) * maxAngle;
        //tfm.Rotate(0.0f, angle, 0.0f);

        float[] target = UpdateVisionGrass();
        Vector3 targetPosition = new Vector3(target[0], tfm.position.y, target[1]);
        tfm.LookAt(targetPosition);

    }

    // Look for grass
    private float[] UpdateVisionGrass()
    {
        // location of the animal
        Vector2 ratio = detailSize / terrainSize;
        float x = tfm.position.x;
        float y = tfm.position.z;

        float cloest_x = 0;
        float cloest_y = 0;
        float cloest_distance = visionRange + 0.1f;

        bool if_found = false;

        // transerverse its surrounding vision range
        for (float i = -visionRange; i <= visionRange; i += 0.5f)
        {
            float x_temp = (x + i) * ratio.x;

            if (x_temp < 0 || x_temp >= detailSize.x)
            {
                continue;
            }

            for (float j = -visionRange; j <= visionRange; j += 0.5f)
            {
                float y_temp = (y + j) * ratio.y;

                if (y_temp < 0 || y_temp >= detailSize.y)
                {
                    continue;
                }

                // if exist grass
                if ((int)x_temp >= 0 && (int)x_temp < details.GetLength(1) && (int)y_temp >= 0 && (int)y_temp < details.GetLength(0) && details[(int)y_temp, (int)x_temp] > 0)
                {
                    Vector2 grass_pos = new(i, j);
                    float distance = grass_pos.magnitude;

                    if (distance < cloest_distance)
                    {
                        cloest_distance = distance;
                        cloest_x = x+i;
                        cloest_y = y+j;
                        if_found = true;
                    }
                }
            }
        }

        // if nothing found
        if (!if_found)
        {
            cloest_x = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            cloest_y = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        }

        // return the cloest result
        float[] result = new float[] {cloest_x, cloest_y};
        return result;
    }

    // Eat grass
    private void EatGrass()
    {
        // Retrieve animal location in the heighmap
        int dx = (int)((tfm.position.x / terrainSize.x) * detailSize.x);
        int dy = (int)((tfm.position.z / terrainSize.y) * detailSize.y);

        // If the animal is located in the dimensions of the terrain and over a grass position (details[dy, dx] > 0), it eats it, gain energy and spawn an offspring.
        if ((dx >= 0) && dx < (details.GetLength(1)) && (dy >= 0) && (dy < details.GetLength(0)) && details[dy, dx] > 0)
        {
            // Eat (remove) the grass and gain energy.
            details[dy, dx] = 0;
            energy += gainEnergy;
            if (energy > maxEnergy)
                energy = maxEnergy;

            genetic_algo.addOffspring(this);

            // display info
            //Vector3 objectPosition = Camera.main.WorldToScreenPoint(tfm.position);

            //if (!isEatDisplaying)
            //{
            //    isEatDisplaying = true;
            //    eatDisplayStartTime = Time.time;
            //    eatLabel.text = displayText;
            //    eatLabel.rectTransform.position = objectPosition;
            //    eatLabel.enabled = true; // Enable the text.
            //}

            terrain.debug.text += "\n\nEating Grass!!!";
        }

        //// if the text should be destoryed
        //if (isEatDisplaying && Time.time - eatDisplayStartTime >= displayDuration)
        //{
        //    // The time has passed; hide the text.
        //    eatLabel.enabled = false;
        //    isEatDisplaying = false; // Reset the flag.
        //}
    }

    // Look for animal
    private float[] UpdateVisionMeat()
    {
        // location of the predator
        float x = tfm.position.x;
        float y = tfm.position.z;

        List<GameObject> animals = genetic_algo.getAnimals();

        float cloest_x = 0;
        float cloest_y = 0;
        float cloest_distance = visionRange + 0.1f;

        bool if_found = false;

        // transerverse its surrounding vision range
        // TODO: rewrite, reduce the query time for animal
        for (float i = -visionRange; i <= visionRange; i += 0.5f)
        {
            float x_temp = (x + i);

            if (x_temp < 0 || x_temp >= terrainSize.x)
            {
                continue;
            }

            for (float j = -visionRange; j <= visionRange; j += 0.5f)
            {
                float y_temp = (y + j);

                if (y_temp < 0 || y_temp >= terrainSize.y)
                {
                    continue;
                }

                // if exist meat
                for (int index = 0; index <animals.Count; index++)
                {
                    float animal_x;
                }
                if ((int)x_temp >= 0 && (int)x_temp < details.GetLength(1) && (int)y_temp >= 0 && (int)y_temp < details.GetLength(0) && details[(int)y_temp, (int)x_temp] > 0)
                {
                    Vector2 meat_pos = new(i, j);
                    float distance = meat_pos.magnitude;

                    if (distance < cloest_distance)
                    {
                        cloest_distance = distance;
                        cloest_x = x + i;
                        cloest_y = y + j;
                        if_found = true;
                    }
                }
            }
        }

        // if nothing found
        if (!if_found)
        {
            cloest_x = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            cloest_y = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        }

        // return the cloest result
        float[] result = new float[] { cloest_x, cloest_y };
        return result;
    }

    // Eat meat
    private void EatMeat()
    {

    }

    /// <summary>
    /// Calculate distance to the nearest food resource, if there is any.
    /// </summary>
    private void UpdateVision()
    {
        float startingAngle = -((float)nEyes / 2.0f) * stepAngle;
        Vector2 ratio = detailSize / terrainSize;

        for (int i = 0; i < nEyes; i++)
        {
            Quaternion rotAnimal = tfm.rotation * Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f);
            Vector3 forwardAnimal = rotAnimal * Vector3.forward;
            float sx = tfm.position.x * ratio.x;
            float sy = tfm.position.z * ratio.y;
            vision[i] = 1.0f;

            // Interate over vision length.
            for (float distance = 1.0f; distance < maxVision; distance += 0.5f)
            {
                // Position where we are looking at.
                float px = (sx + (distance * forwardAnimal.x * ratio.x));
                float py = (sy + (distance * forwardAnimal.z * ratio.y));

                if (px < 0)
                    px += detailSize.x;
                else if (px >= detailSize.x)
                    px -= detailSize.x;
                if (py < 0)
                    py += detailSize.y;
                else if (py >= detailSize.y)
                    py -= detailSize.y;

                if ((int)px >= 0 && (int)px < details.GetLength(1) && (int)py >= 0 && (int)py < details.GetLength(0) && details[(int)py, (int)px] > 0)
                {
                    vision[i] = distance / maxVision;
                    break;
                }
            }
        }
    }

    public void Setup(CustomTerrain ct, GeneticAlgo ga)
    {
        terrain = ct;
        genetic_algo = ga;
        UpdateSetup();
    }

    private void UpdateSetup()
    {
        detailSize = terrain.detailSize();
        Vector3 gsz = terrain.terrainSize();
        terrainSize = new Vector2(gsz.x, gsz.z);
        details = terrain.getDetails();
    }

    //public void InheritBrain(SimpleNeuralNet other, bool mutate)
    //{
    //    brain = new SimpleNeuralNet(other);
    //    if (mutate)
    //        brain.mutate(swapRate, mutateRate, swapStrength, mutateStrength);
    //}
    //public SimpleNeuralNet GetBrain()
    //{
    //    return brain;
    //}
    public float GetHealth()
    {
        return energy / maxEnergy;
    }

    public float GetEnergyMAX()
    {
        return energy_MAX;
    }

    public float GetVisionRange()
    {
        return visionRange;
    }

    public float GetSize()
    {
        return size;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public Transform GetTransform()
    {
        return tfm;
    }


    private static double Gaussian(double mean, double std)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double standardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + std * standardNormal;
    }

    public void Mutate(float energy_parent, float vision_parent, float size_parent, float speed_parent)
    {
        energy_MAX = (float)Gaussian(energy_parent, energy_parent * mutateRate);
        energy = energy_MAX;

        visionRange = (float)Gaussian(vision_parent, vision_parent * mutateRate);
        size = (float)Gaussian(size_parent, size_parent * mutateRate);
        speed = (float)Gaussian(speed_parent, speed_parent * mutateRate);

        // size
        // tfm.localScale = new Vector3(size, size, size);

        // speed
        //if (if_animal == true)
        //{
        //    CapsuleAutoController capsule_controller = GetComponent<CapsuleAutoController>();
        //    capsule_controller.max_speed = speed;
        //}
        //else
        //{
        //    HumanoidAutoController human_controller = GetComponent<HumanoidAutoController>();
        //    human_controller.max_speed = speed;
        //}
        CapsuleAutoController capsule_controller = GetComponent<CapsuleAutoController>();
        capsule_controller.max_speed = speed;
    }
}
