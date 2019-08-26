using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain
{
    int geneNo;
    public int[] genes { get; private set; }
    public float crossoverRate = 0.7f;
    public float mutationRate = 0.001f;
    public float fitness;
    public Brain(int noGenes)
    {
        geneNo = noGenes;
        genes = new int[noGenes];
        for (int i = 0; i < geneNo; i++) genes[i] = UnityEngine.Random.Range(0, 2);
    }

    public Brain(Brain copy) {
        geneNo = copy.geneNo;
        fitness = copy.fitness;
        genes = new int[geneNo];
        for (int i = 0; i < geneNo; i++) genes[i] = copy.genes[i];
   }

    public void Crossover(Brain other)
    {
        float rand = UnityEngine.Random.Range(0f, 1f);
        if (rand > crossoverRate) return;
        int index = Random.Range(0, geneNo);
        for (int i = index; i < geneNo; i++) genes[i] = other.genes[i];
    }

    public void Mutate()
    {
        for (int i = 0; i < geneNo; i++) if (UnityEngine.Random.Range(0f, 1f) < mutationRate) genes[i] = 1 - genes[i];
    }
}
