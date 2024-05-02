using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMAUI
{
    public static class DataHolder
    {
        public static int[,] DataGrid { get; set; }

        public static void SetupGrid(int cols, int rows, GridTile start, GridTile end)
        {
            DataGrid = new int[cols,rows];
            DataGrid[start.X,start.Y] = start.Value;
            DataGrid[end.X,end.Y] = end.Value;
        }
    }

    public class GridTile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Value { get; set; }

        public GridTile(int x, int y, int value)
        {
            X = x;
            Y = y;
            Value = value;
        }
    }
}
