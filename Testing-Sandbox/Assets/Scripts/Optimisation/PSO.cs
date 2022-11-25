using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using System;

public static class PSO 
{
    public delegate float FitnessFunction(float[] val);

    [System.Serializable]
    public struct Parameters
    {
        [Range(5, 100)]
        public int   nOfParticles;
        [Range(10, 100)]
        public int iterations;
        public int dim;
        public float minX;
        public float maxX;
        [Range(0f, 1f)]
        public float w;
        [Range(0f, 2f)]
        public float c1;
        [Range(0f, 2f)]
        public float c2;
        public Parameters(int numberOfParticles, int numberOfIterations, int dimensions, float minimumVal, float maximumVal, float inertiaCoefficient, float cognitiveCoefficient, float socialCoefficient)
        {
            nOfParticles = numberOfParticles;
            iterations = numberOfIterations;
            dim = dimensions;
            minX = minimumVal;
            maxX = maximumVal;
            w = inertiaCoefficient;
            c1 = cognitiveCoefficient;
            c2 = socialCoefficient;
        }
    }

    private class Particle 
    {
        float[] current, velocity;
        float minX, maxX;
        KeyValuePair<float, float[]> lBest;
        FitnessFunction f;

        public Particle(float[] startingValue, float[] startingVelocity, float minX, float maxX, FitnessFunction function)
        {
            current = startingValue;
            lBest = new KeyValuePair<float, float[]>(function(startingValue), startingValue);
            velocity = startingVelocity;
            f = function;
            this.minX = minX;
            this.maxX = maxX;
        }

        public KeyValuePair<float, float[]> Update(KeyValuePair<float, float[]> gBest, float w, float c1, float c2, int dim)
        {
            //Set velocity
            for (int i = 0; i < dim; i++)
            {
                float r1 = UnityEngine.Random.value;
                float r2 = UnityEngine.Random.value;
                velocity[i] = (w * velocity[i]) + (c1 * r1 * (lBest.Value[i] - current[i])) + (c2 * r2 * (gBest.Value[i] - current[i]));
                velocity[i] = Mathf.Clamp(velocity[i], minX, maxX);
            }
            //Set new position
            for (int i = 0; i < dim; i++)
            {
                current[i] += velocity[i];
                //Wrapping value
                if (current[i] > maxX)
                    current[i] = current[i] - maxX;
                else if (current[i] < minX)
                    current[i] = maxX + current[i];

                Debug.Log($"{velocity[0]}, {velocity[1]}, {velocity[2]}");
            }
            //Evaluate
            float score = f(current);
            if(score > lBest.Key)
            {
                lBest = new KeyValuePair<float, float[]>(score, current);
            }
            return lBest;
        }
    }

    public static void Optimise(
        MonoBehaviour owner,
        FitnessFunction function,
        Action<float[], int> renderAction,
        Parameters parameters
        )
    {
        IEnumerator OptimisationRoutine()
        {
            KeyValuePair<float, float[]> gBest = new KeyValuePair<float, float[]>(0, new float[parameters.dim]);
            Particle[] particles = new Particle[parameters.nOfParticles];
            var wait = new EditorWaitForSeconds(0.1f);

            //Initialise
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = new Particle(
                    GetRandomVector(parameters.minX, parameters.maxX, parameters.dim), 
                    GetRandomVector(parameters.minX, parameters.maxX, parameters.dim), 
                    parameters.minX, 
                    parameters.maxX, 
                    function
                    );
            }

            //Optimise
            for (int i = 0; i < parameters.iterations; i++)
            {
                for (int j = 0; j < particles.Length; j++)
                {
                    KeyValuePair<float, float[]> currentPBest = particles[j].Update(gBest, parameters.w, parameters.c1, parameters.c2, parameters.dim);
                    if (currentPBest.Key > gBest.Key)
                    {
                        gBest = currentPBest;
                    }
                }
                renderAction.Invoke(gBest.Value, i);
                Debug.Log(gBest.Key);
                yield return wait;
            }
        }
        EditorCoroutineUtility.StartCoroutine(OptimisationRoutine(), owner);
    }


    static float[] GetRandomVector(float min, float max, int dim)
    {
        float[] outVector = new float[dim];
        for (int i = 0; i < dim; i++)
        {
            outVector[i] = UnityEngine.Random.Range(min, max);
        }
        return outVector;
    }

}
