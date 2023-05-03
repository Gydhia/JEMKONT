using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
public static class ListHelper
{
    private static Random rng = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int count = list.Count;
        while (count > 1)
        {
            count--;

            int newIndex = rng.Next(count + 1);
            T value = list[newIndex];
            list[newIndex] = list[count];
            list[count] = value;
        }
    }
}
