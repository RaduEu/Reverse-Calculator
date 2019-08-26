using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ManagerScript : MonoBehaviour
{
    public int populationSize = 1000;
    char[] decodingTable = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '-', '*', '/', 'N', 'N' }; // how to interpret a gene
    Brain[] brains;
    public int generationNo = 1;
    public float answer; // what to look for
    public int noChunks; // number of genes
    const int ChunkSize = 4; // how many bits in each gene
    public string[] stats;
    public float additionalFitness; // How much we reward an individual for doing well. The higher this is, the quicker the population converges, but also the more likely it is to fnd a suboptimal solution 
    public Text generationNumberText, bestOfGenerationText, averageFitnessText;
    public InputField input;
    Brain best; // best individual of this generation
    public void Start()
    {
        brains = new Brain[populationSize];
        stats = new string[populationSize];
        for (int i = 0; i < populationSize; i++) brains[i] = new Brain(noChunks * ChunkSize);
        NextGen();
    }

    public char[] Decode(Brain brain)
    {
        char[] characters = new char[noChunks];
        for (int i = 0; i < noChunks; i++)
        {
            int character = 0;
            for (int j = 0; j < ChunkSize; j++) character = character * 2 + brain.genes[i * ChunkSize + j];
            characters[i] = decodingTable[character];
        }
        return characters;
    }

    public char[] Purge(char[] expr)
    {
        bool wantNumber = true;
        char[] ret = new char[noChunks + 1];
        int k = 0;
        int i = 0;
        for (i = 0; i < noChunks; i++)
        {
            char curr = expr[i];
            if (wantNumber && '0' <= curr && curr <= '9')
            {
                ret[k] = curr; k++; wantNumber = !wantNumber;
            }
            else if (!wantNumber && (curr == '+' || curr == '-' || curr == '/' || curr == '*'))
            {
                ret[k] = curr; k++; wantNumber = !wantNumber;
            }
        }
        if (k > 0) if (ret[k - 1] < '0' || ret[k - 1] > '9') { ret[k - 1] = 'N'; ret[k] = 'N'; }
            else { ret[k] = 'N'; ret[k + 1] = 'N'; }
        return ret;
    }

    public float Evaluate(char[] expr)
    {
        if (expr[0] == 'N' || expr[1] == 'N') return Mathf.Infinity;
        int len = expr.Length;
        float ret = (float)expr[0] - '0';
        int index = 1;
        char number = ' ';
        char op = ' ';
        while (index + 1 < len && expr[index] != 'N' && expr[index + 1] != 'N')
        {
            number = expr[index + 1];
            op = expr[index];
            index += 2;

            int num = number - '0';
            if (op == '+') ret += num;
            else if (op == '-') ret -= num;
            else if (op == '*') ret *= num;
            else if (op == '/' && num != 0) ret /= num;
            else if (op == '/' && num == 0) return Mathf.Infinity;
            else Debug.Log(new string(expr));
        }
        return ret;
    }

    // A value from 1/(additional+1) to 1. Higher is better 
    public float Fitness(Brain brain)
    {
        float sol = Evaluate(Purge(Decode(brain)));
        float fit = (1f + additionalFitness / (1 + Mathf.Abs(sol - answer))) / (1f + additionalFitness);
        return fit;
    }

    public float avgFitness()
    {
        float sum = 0f;
        for (int i = 0; i < populationSize; i++) sum += Fitness(brains[i]);
        return sum / populationSize;
    }

    public void SkipGenerations(int number)
    {
        for (int i = 0; i < number; i++) NextGen();
    }
    void NextGen()
    {
        generationNo++;

        Brain[] nextGen = new Brain[populationSize];
        float[] partialSums = new float[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            brains[i].fitness = Fitness(brains[i]);
        }
        partialSums[0] = brains[0].fitness;
        for (int i = 1; i < populationSize; i++) partialSums[i] = partialSums[i - 1] + brains[i].fitness;

        best = brains[0];
        for (int i = 0; i < populationSize; i++) if (best.fitness < brains[i].fitness) best = brains[i];

        nextGen[0] = new Brain(best);

        for (int i = 1; i < populationSize; i++)
        {
            int choice = 0;
            float rnd = UnityEngine.Random.Range(0f, partialSums[populationSize - 1]);
            for (int j = 1; j < populationSize; j++)
            {
                if (rnd < partialSums[0]) choice = 0;
                else if (rnd > partialSums[j - 1] && rnd < partialSums[j]) choice = j;
            }
            nextGen[i] = new Brain(brains[choice]);
            if (i > 0) nextGen[i].Crossover(nextGen[i - 1]);
            nextGen[i].Mutate();
        }

        Show();

        brains = nextGen;
        for (int i = 0; i < populationSize; i++) stats[i] = new string(Decode(brains[i])) + " " + Fitness(brains[i]);
    }

    void Show()
    {
        generationNumberText.text = "Generation Number " + generationNo;
        
        string txt = "Best of this Generation: " + " (" + Evaluate(Purge(Decode(best))).ToString() + ") " + new string((Purge(Decode(best))));
        txt = txt.Split(new char[] { 'N' })[0];
        bestOfGenerationText.text = txt;
        averageFitnessText.text = "Average Fitness: " + avgFitness().ToString();
    }

    public void SetAnswer()
    {
        float ans = float.Parse(input.text);
        answer = ans;
    }
}