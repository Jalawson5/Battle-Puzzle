using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathStuff
{
    public static bool[] Shuffle(bool[] arr)
    {
        System.Random rand = new System.Random();
        
        for(int i = 0; i < arr.Length; i++)
        {
            int swap = rand.Next(arr.Length);
            bool temp = arr[swap];
            arr[swap] = arr[i];
            arr[i] = temp;
        }
        
        return arr;
    }
    
    public static int[] Shuffle(int[] arr)
    {
        System.Random rand = new System.Random();
        
        for(int i = 0; i < arr.Length; i++)
        {
            int swap = rand.Next(arr.Length);
            int temp = arr[swap];
            arr[swap] = arr[i];
            arr[i] = temp;
        }
        
        return arr;
    }
    
    public static GameObject[] Shuffle(GameObject[] arr)
    {
        System.Random rand = new System.Random();
        
        for(int i = 0; i < arr.Length; i++)
        {
            int swap = rand.Next(arr.Length);
            GameObject temp = arr[swap];
            arr[swap] = arr[i];
            arr[i] = temp;
        }
        
        return arr;
    }
}
