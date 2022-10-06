using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Jemkont.GridSystem
{
    public class GridUtility
    {
        [Serializable]
        public class Array2D<Cell>
        {
            public int x, y;

            /// <summary>2D array stored in 1D array.</summary>
            public Cell[] SingleArray;

            public Cell this[int x, int y]
            {
                get => SingleArray[y * this.x + x];
                set => SingleArray[y * this.x + x] = value;
            }

            public Array2D(int x, int y)
            {
                this.x = x;
                this.y = y;
                SingleArray = new Cell[x * y];
            }

            /// <summary>Gets the total number of elements in X dimension (1st dimension). </summary>
            public int Get_X_Length => x;

            /// <summary>Gets the total number of elements in Y dimension. (2nd dimension).</summary>
            public int Get_Y_Length => y;

            /// <summary>Gets the total number of elements all dimensions.</summary>
            public int Length => SingleArray.Length;

        }
    }
}
