using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
public static class RandomHelper
{
    private static Random rng = new Random();

    public static void Shuffle<T>(this IList<T> list, string UID)
    {
        rng = new Random(UID.GetHashCode());

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
    public static int RandInt(int minInclusive, int maxExclusive, string UID)
    {
        rng = new Random(UID.GetHashCode());
        return rng.Next(minInclusive, maxExclusive);
    }
}
