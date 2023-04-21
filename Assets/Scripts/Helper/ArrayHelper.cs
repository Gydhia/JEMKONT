using System;
using System.Collections;
using System.Collections.Generic;

public static class ArrayHelper
{
    public static void Add<T>(ref T[] array, T item)
    {
        Array.Resize(ref array, array.Length + 1);
        array[array.Length - 1] = item;
    }

    public static bool ArrayEquals<T>(T[] lhs, T[] rhs)
    {
        if (lhs == null || rhs == null)
        {
            return lhs == rhs;
        }

        if (lhs.Length != rhs.Length)
        {
            return false;
        }

        for (int i = 0; i < lhs.Length; i++)
        {
            if (!lhs[i].Equals(rhs[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static bool ArrayReferenceEquals<T>(T[] lhs, T[] rhs)
    {
        if (lhs == null || rhs == null)
        {
            return lhs == rhs;
        }

        if (lhs.Length != rhs.Length)
        {
            return false;
        }

        for (int i = 0; i < lhs.Length; i++)
        {
            if ((object)lhs[i] != (object)rhs[i])
            {
                return false;
            }
        }

        return true;
    }

    public static void AddRange<T>(ref T[] array, T[] items)
    {
        int num = array.Length;
        Array.Resize(ref array, array.Length + items.Length);
        for (int i = 0; i < items.Length; i++)
        {
            array[num + i] = items[i];
        }
    }

    public static void Insert<T>(ref T[] array, int index, T item)
    {
        ArrayList arrayList = new ArrayList();
        arrayList.AddRange(array);
        arrayList.Insert(index, item);
        array = (arrayList.ToArray(typeof(T)) as T[]);
    }

    public static void Remove<T>(ref T[] array, T item)
    {
        List<T> list = new List<T>(array);
        list.Remove(item);
        array = list.ToArray();
    }

    public static List<T> FindAll<T>(T[] array, Predicate<T> match)
    {
        List<T> list = new List<T>(array);
        return list.FindAll(match);
    }

    public static T Find<T>(T[] array, Predicate<T> match)
    {
        List<T> list = new List<T>(array);
        return list.Find(match);
    }

    public static int FindIndex<T>(T[] array, Predicate<T> match)
    {
        List<T> list = new List<T>(array);
        return list.FindIndex(match);
    }

    public static int IndexOf<T>(T[] array, T value)
    {
        List<T> list = new List<T>(array);
        return list.IndexOf(value);
    }

    public static int LastIndexOf<T>(T[] array, T value)
    {
        List<T> list = new List<T>(array);
        return list.LastIndexOf(value);
    }

    public static void RemoveAt<T>(ref T[] array, int index)
    {
        List<T> list = new List<T>(array);
        list.RemoveAt(index);
        array = list.ToArray();
    }

    public static bool Contains<T>(T[] array, T item)
    {
        List<T> list = new List<T>(array);
        return list.Contains(item);
    }

    public static void Clear<T>(ref T[] array)
    {
        Array.Clear(array, 0, array.Length);
        Array.Resize(ref array, 0);
    }

    public static T[,] ResizeArrayTwo<T>(T[,] original, int newCoNum, int newRoNum, bool resizeDown)
    {
        var newArray = new T[newCoNum, newRoNum];
        int columnCount = original.GetLength(1);
        int columnCount2 = newRoNum;
        int columns = original.GetUpperBound(0);
        if (resizeDown)
        {
            // Since 2D arrays are only one line of elements, we need to offset the difference if we go from 5 to 3 or else
            if (newRoNum < columnCount)
                for (int co = 0; co < newRoNum; co++)
                    Array.Copy(original, co * newRoNum + (co * (columnCount - newRoNum)), newArray, co * columnCount2, newRoNum);
            // For columns we shouldn't try to copy in the new array more than we have
            else
                for (int co = 0; co < newCoNum; co++)
                    Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
        }
        else
            for (int co = 0; co <= columns; co++)
                Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);

        return newArray;
    }

    public static T[,] InsertEmptyRow<T>(T[,] originalArray, int index)
    {
        int rows = originalArray.GetLength(0);
        int cols = originalArray.GetLength(1);
        int newRows = rows + 1;
        T[,] newArray = new T[newRows, cols];

        for (int i = 0; i < newRows; i++)
        {
            if (i < index)
                for (int j = 0; j < cols; j++)
                    newArray[i, j] = originalArray[i, j];
            else if (i == index)
                for (int j = 0; j < cols; j++)
                    newArray[i, j] = default(T);
            else
                for (int j = 0; j < cols; j++)
                    newArray[i, j] = originalArray[i - 1, j];
        }

        return newArray;
    }

    public static T[,] InsertEmptyColumn<T>(T[,] originalArray, int index)
    {
        int rows = originalArray.GetLength(0);
        int cols = originalArray.GetLength(1);
        int newCols = cols + 1;
        T[,] newArray = new T[rows, newCols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < newCols; j++)
            {
                if (j < index)
                    newArray[i, j] = originalArray[i, j];
                else if (j == index)
                    newArray[i, j] = default(T);
                else
                    newArray[i, j] = originalArray[i, j - 1];
            }
        }

        return newArray;
    }

    public static T[,] RemoveColumn<T>(T[,] array, int columnIndex)
    {
        int numRows = array.GetLength(0);
        int numColumns = array.GetLength(1);

        T[,] newArray = new T[numRows, numColumns - 1];

        // Copy elements from the old array to the new array,
        // skipping the column we want to remove.
        for (int row = 0; row < numRows; row++)
        {
            int sourceIndex = row * numColumns;
            int destinationIndex = row * (numColumns - 1);

            Array.Copy(array, sourceIndex, newArray, destinationIndex, columnIndex);
            Array.Copy(array, sourceIndex + columnIndex + 1, newArray, destinationIndex + columnIndex, numColumns - columnIndex - 1);
        }

        return newArray;
    }

    public static T[,] RemoveRow<T>(T[,] array, int rowIndex)
    {
        int numRows = array.GetLength(0);
        int numColumns = array.GetLength(1);

        T[,] newArray = new T[numRows - 1, numColumns];

        // Copy elements from the old array to the new array,
        // skipping the row we want to remove.
        int destinationIndex = 0;
        for (int sourceIndex = 0; sourceIndex < numRows; sourceIndex++)
        {
            if (sourceIndex != rowIndex)
            {
                Array.Copy(array, sourceIndex * numColumns, newArray, destinationIndex * numColumns, numColumns);
                destinationIndex++;
            }
        }

        return newArray;
    }
}
