
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Diagnostics;

namespace TestMAUI
{
    public partial class MainPage : ContentPage
    {
        Grid gameGrid;
        Button[,] gridBtns;
        int availableMirrors = 5;
        GraphicsView graphicsView;
        LaserBeam laserBeam;
        Random random;

        public delegate void SizeCanged();

        public event SizeCanged SizeCangedEvent;

        public MainPage()
        {
            InitializeComponent();
            random = new Random();
            GenerateGrid(5,5);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            SizeCangedEvent?.Invoke();
        }

        void GenerateGrid(int x, int y)
        {
            gridBtns = new Button[x,y];
            gameGrid = new Grid();
            for (int i = 0; i < x; i++)
            {
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < y; i++)
            {
                gameGrid.RowDefinitions.Add(new RowDefinition());
            }
            SetStartAndEnd(x,y);
            GenerateView();
        }

        void SetStartAndEnd(int cols, int rows)
        {
            GridTile startTile = SelectCoordsOnTable(cols,rows, 10);
            GridTile endTile = SelectCoordsOnTable(cols,rows, 20);
            DataHolder.SetupGrid(cols, rows, startTile, endTile);
            SizeCangedEvent += delegate { RendererLaser(); };
        }

        void AddRestrictions(int laserWay, int cols, int rows, ref List<int> noX, ref List<int> noY)
        {
            noX.Clear();
            noY.Clear();
            switch (laserWay)
            {
                case 0:
                    noY.Add(0);
                    break;
                case 1:
                    noX.Add(cols - 1);
                    break;
                case 2:
                    noY.Add(rows - 1);
                    break;
                case 3:
                    noX.Add(0);
                    break;
                default:
                    break;
            }
        }

        GridTile SelectCoordsOnTable(int cols, int rows, int startVal)
        {
            int laserWay = random.Next(0, 4);
            List<int> noX = new List<int>();
            List<int> noY = new List<int>();
            AddRestrictions(laserWay, cols, rows, ref noX, ref noY);
            int X, Y;
            do
            {
                X = random.Next(cols);
                Y = random.Next(rows);
            } while (noX.Contains(X) || noY.Contains(Y));
            return new(X, Y, laserWay + startVal);
        }

        void GenerateView()
        {
            for (int i = 0; i < DataHolder.DataGrid.GetLength(1); i++)
            {
                for (int j = 0; j < DataHolder.DataGrid.GetLength(0); j++)
                {
                    int value = DataHolder.DataGrid[j,i];
                    Button button = new Button() { Text = $"{j + 1};{i + 1}: {value}", Margin = new Thickness(5) };
                    if (value >= 10 && value < 14)
                    {
                        button.BackgroundColor = Colors.Green;
                    }
                    else if (value >= 20 && value < 24)
                    {
                        button.BackgroundColor = Colors.Red;
                    }
                    else
                    {
                        int x = j;
                        int y = i;
                        button.Clicked += delegate 
                        { 
                            if (button.BackgroundColor == Colors.White)
                            {
                                DataHolder.PlaceMirror(x, y, 0);
                            }
                            else
                            {
                                DataHolder.PlaceMirror(x,y,random.Next(1,5)); 
                            }
                        };
                        button.Clicked += SetMirror;
                    }
                    Grid.SetColumn(button, j);
                    Grid.SetRow(button, i);
                    gridBtns[j, i] = button;
                    gameGrid.Children.Add(button);
                }
            }
            graphicsView = new GraphicsView();
            graphicsView.HorizontalOptions = LayoutOptions.FillAndExpand;
            graphicsView.VerticalOptions = LayoutOptions.FillAndExpand;
            graphicsView.InputTransparent = true;
            graphicsView.BackgroundColor = Colors.Transparent;
            laserBeam = new LaserBeam();
            graphicsView.Drawable = laserBeam;
            Grid.SetRow(graphicsView, 1);
            Grid.SetRow(gameGrid, 1);
            GridParent.Children.Add(gameGrid);
            GridParent.Children.Add(graphicsView);
            RendererLaser();
        }

        void SetMirror(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.BackgroundColor == Colors.White)
                {
                    availableMirrors++;
                    btn.BackgroundColor = Color.FromArgb("#ac99ea");
                }
                else if (availableMirrors > 0)
                {
                    availableMirrors--;
                    btn.BackgroundColor = Colors.White;
                }
                RendererLaser();
            }
        }

        void Restart(object? sender, EventArgs e)
        {
            graphicsView.Drawable = null;
            availableMirrors = 5;
            GenerateGrid(5, 5);
        }

        void RendererLaser()
        {
            List<Point[]> points = new();
            List<int[]> prevStartCoords = new();
            int[]? startCoord = FindStart();
            int[]? endCoord = FindEnd();
            do
            {
                List<Point> start = new List<Point>();
                Button startButton = gridBtns[startCoord[0], startCoord[1]];
                start.Add(new Point(startButton.Bounds.X + (startButton.Bounds.Width / 2), startButton.Bounds.Y + (startButton.Bounds.Height / 2)));
                int way = DataHolder.DataGrid[startCoord[0], startCoord[1]];
                prevStartCoords.Add([startCoord[0], startCoord[1]]);
                Debug.WriteLine($"Prev starts: {prevStartCoords.Count}");
                Debug.WriteLine($"StartCoord: {startCoord[0]} {startCoord[1]}, value: {way}");
                if (way >= 20)
                {
                    way -= 20;
                }
                else if (way >= 10)
                {
                    way -= 10;
                }
                double? endX = null;
                double? endY = null;
                switch (way)
                {
                    case 0:
                        for (int y = startCoord[1] - 1; y >= 0; y--)
                        {
                            Debug.WriteLine($"Value: {DataHolder.DataGrid[startCoord[0], y]}");
                            if (DataHolder.DataGrid[startCoord[0], y] > 0 && DataHolder.DataGrid[startCoord[0], y] < 5)
                            { 
                                endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                int prevX = startCoord[0];
                                startCoord = new int[] { prevX, y };
                                break;
                            }
                            else if (DataHolder.DataGrid[startCoord[0], y] >= 20 && DataHolder.DataGrid[startCoord[0], y] < 25)
                            {
                                int finalWay = DataHolder.DataGrid[startCoord[0], y];
                                switch (finalWay)
                                {
                                    case >= 20:
                                        finalWay -= 20;
                                        break;
                                    case >= 10:
                                        finalWay -= 10;
                                        break;
                                    default:
                                        break;
                                }
                                switch (finalWay)
                                {
                                    case 0:
                                        finalWay = 2;
                                        break;
                                    case 1:
                                        finalWay = 3;
                                        break;
                                    case 2:
                                        finalWay = 0;
                                        break;
                                    case 3:
                                        finalWay = 1;
                                        break;
                                    case 4:
                                        finalWay = 2;
                                        break;
                                }
                                Debug.WriteLine($"Finaly way: {finalWay}, way: {way}");
                                if (way == finalWay)
                                {
                                    endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                }
                                break;
                            }
                        }
                        if (endX == null ||  endY == null)
                        { 
                            endY = 0;
                            endX = startButton.Bounds.X + (startButton.Bounds.Width / 2);
                            startCoord = null;
                        }
                        break;
                    case 1:
                        for (int x = startCoord[0] + 1; x < DataHolder.DataGrid.GetLength(0); x++)
                        {
                            Debug.WriteLine($"Value: {DataHolder.DataGrid[x, startCoord[1]]}");
                            if (DataHolder.DataGrid[x, startCoord[1]] > 0 && DataHolder.DataGrid[x, startCoord[1]] < 5)
                            {
                                endX = gridBtns[x, startCoord[1]].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[x, startCoord[1]].Bounds.Y + (startButton.Bounds.Height / 2);
                                int prevY = startCoord[1];
                                startCoord = new int[] { x, prevY };
                                break;
                            }
                            else if (DataHolder.DataGrid[x, startCoord[1]] >= 20 && DataHolder.DataGrid[x, startCoord[1]] < 25)
                            {
                                int finalWay = DataHolder.DataGrid[x, startCoord[1]];
                                switch (finalWay)
                                {
                                    case >= 20:
                                        finalWay -= 20;
                                        break;
                                    case >= 10:
                                        finalWay -= 10;
                                        break;
                                    default:
                                        break;
                                }
                                switch (finalWay)
                                {
                                    case 0:
                                        finalWay = 2;
                                        break;
                                    case 1:
                                        finalWay = 3;
                                        break;
                                    case 2:
                                        finalWay = 4;
                                        break;
                                    case 3:
                                        finalWay = 1;
                                        break;
                                    case 4:
                                        finalWay = 2;
                                        break;
                                }
                                Debug.WriteLine($"Finaly way: {finalWay}, way: {way}");
                                if (way == finalWay)
                                {
                                    endX = gridBtns[x, startCoord[1]].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[x, startCoord[1]].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                }
                                break;
                            }
                        }
                        if (endX == null || endY == null)
                        {
                            endX = gridBtns[gridBtns.GetLength(0) - 1, startCoord[1]].Bounds.X + (startButton.Bounds.Width);
                            endY = startButton.Bounds.Y + (startButton.Bounds.Height / 2);
                            startCoord = null;
                        }
                        break;
                    case 2:
                        for (int y = startCoord[1] + 1; y < DataHolder.DataGrid.GetLength(1); y++)
                        {
                            Debug.WriteLine($"Value: {DataHolder.DataGrid[startCoord[0], y]}");
                            if (DataHolder.DataGrid[startCoord[0], y] > 0 && DataHolder.DataGrid[startCoord[0], y] < 5)
                            {
                                endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                int prevX = startCoord[0];
                                startCoord = new int[] { prevX, y };
                                break;
                            }
                            else if (DataHolder.DataGrid[startCoord[0], y] >= 20 && DataHolder.DataGrid[startCoord[0], y] < 25)
                            {
                                int finalWay = DataHolder.DataGrid[startCoord[0], y];
                                switch (finalWay)
                                {
                                    case >= 20:
                                        finalWay -= 20;
                                        break;
                                    case >= 10:
                                        finalWay -= 10;
                                        break;
                                    default:
                                        break;
                                }
                                switch (finalWay)
                                {
                                    case 0:
                                        finalWay = 2;
                                        break;
                                    case 1:
                                        finalWay = 3;
                                        break;
                                    case 2:
                                        finalWay = 4;
                                        break;
                                    case 3:
                                        finalWay = 1;
                                        break;
                                    case 4:
                                        finalWay = 2;
                                        break;
                                }
                                Debug.WriteLine($"Finaly way: {finalWay}, way: {way}");
                                if (way == finalWay)
                                {
                                    endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                }
                                break;
                            }
                        }
                        if (endX == null || endY == null)
                        {
                            endY = gridBtns[startCoord[0], gridBtns.GetLength(1) - 1].Bounds.Y + gridBtns[startCoord[0], gridBtns.GetLength(1) - 1].Bounds.Height;
                            endX = startButton.Bounds.X + (startButton.Bounds.Width / 2);
                            startCoord = null;
                        }
                        break;
                    case 3:
                        for (int x = startCoord[0] - 1; x >= 0; x--)
                        {
                            Debug.WriteLine($"Value: {DataHolder.DataGrid[x, startCoord[1]]}");
                            if (DataHolder.DataGrid[x, startCoord[1]] > 0 && DataHolder.DataGrid[x, startCoord[1]] < 5)
                            {
                                endX = gridBtns[x, startCoord[1]].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[x, startCoord[1]].Bounds.Y + (startButton.Bounds.Height / 2);
                                int prevY = startCoord[1];
                                startCoord = new int[] { x, prevY };
                                break;
                            }
                            else if (DataHolder.DataGrid[x, startCoord[1]] >= 20 && DataHolder.DataGrid[x, startCoord[1]] < 25)
                            {
                                int finalWay = DataHolder.DataGrid[x, startCoord[1]];
                                switch (finalWay)
                                {
                                    case >= 20:
                                        finalWay -= 20;
                                        break;
                                    case >= 10:
                                        finalWay -= 10;
                                        break;
                                    default:
                                        break;
                                }
                                switch (finalWay)
                                {
                                    case 0:
                                        finalWay = 2;
                                        break;
                                    case 1:
                                        finalWay = 3;
                                        break;
                                    case 2:
                                        finalWay = 4;
                                        break;
                                    case 3:
                                        finalWay = 1;
                                        break;
                                    case 4:
                                        finalWay = 2;
                                        break;
                                }
                                Debug.WriteLine($"Finaly way: {finalWay}, way: {way}");
                                if (way == finalWay)
                                {
                                    endX = gridBtns[x, startCoord[1]].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[x, startCoord[1]].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                }
                                break;
                            }
                        }
                        if (endX == null || endY == null)
                        {
                            endX = 0;
                            endY = startButton.Bounds.Y + (startButton.Bounds.Height / 2);
                            startCoord = null;
                        }
                        break;
                    case 4:
                        for (int y = startCoord[1] - 1; y >= 0; y--)
                        {
                            Debug.WriteLine($"Value: {DataHolder.DataGrid[startCoord[0], y]}");
                            if (DataHolder.DataGrid[startCoord[0], y] > 0 && DataHolder.DataGrid[startCoord[0], y] < 5)
                            {
                                endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                int prevX = startCoord[0];
                                startCoord = new int[] { prevX, y };
                                break;
                            }
                            else if (DataHolder.DataGrid[startCoord[0], y] >= 20 && DataHolder.DataGrid[startCoord[0], y] < 25)
                            {
                                int finalWay = DataHolder.DataGrid[startCoord[0], y];
                                switch (finalWay)
                                {
                                    case >= 20:
                                        finalWay -= 20;
                                        break;
                                    case >= 10:
                                        finalWay -= 10;
                                        break;
                                    default:
                                        break;
                                }
                                switch (finalWay)
                                {
                                    case 0:
                                        finalWay = 2;
                                        break;
                                    case 1:
                                        finalWay = 3;
                                        break;
                                    case 2:
                                        finalWay = 4;
                                        break;
                                    case 3:
                                        finalWay = 1;
                                        break;
                                    case 4:
                                        finalWay = 2;
                                        break;
                                }
                                Debug.WriteLine($"Finaly way: {finalWay}, way: {way}");
                                if (way == finalWay)
                                {
                                    endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                }
                                break;
                            }
                        }
                        if (endX == null || endY == null)
                        {
                            endY = 0;
                            endX = startButton.Bounds.X + (startButton.Bounds.Width / 2);
                            startCoord = null;
                        }
                        break;
                    default:
                        startCoord = null;
                        break;
                }
                Debug.WriteLine($"EndCoord: {endX} {endY}");
                start.Add(new Point(endX??0, endY??0));
                points.Add(start.ToArray());
            } while (startCoord != null && !ContainsStart(startCoord[0], startCoord[1], prevStartCoords));
            laserBeam.Points = points;
            graphicsView.Invalidate();
        }

        bool ContainsStart(int x, int y, List<int[]> points)
        {
            foreach (int[] pointPair in points)
            {
                if (pointPair[0] == x && pointPair[1] == y) return true;
            }
            return false;
        }

        int[]? FindStart()
        {
            for (int i = 0; i < DataHolder.DataGrid.GetLength(1); i++)
            {
                for (int j = 0; j < DataHolder.DataGrid.GetLength(0); j++)
                {
                    if (DataHolder.DataGrid[j,i] >= 10 && DataHolder.DataGrid[j, i] < 14)
                    {
                        return [j,i];
                    }
                }
            }
            return null;
        }
        int[]? FindEnd()
        {
            for (int i = 0; i < DataHolder.DataGrid.GetLength(1); i++)
            {
                for (int j = 0; j < DataHolder.DataGrid.GetLength(0); j++)
                {
                    if (DataHolder.DataGrid[j, i] >= 20 && DataHolder.DataGrid[j, i] < 24)
                    {
                        return [j, i];
                    }
                }
            }
            return null;
        }
    }
    public class LaserBeam: IDrawable
    {
        public List<Point[]> Points { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                canvas.StrokeColor = Colors.Red;
                canvas.StrokeSize = 5f;
                canvas.Antialias = true;

                canvas.DrawLine((float)Points[i][0].X, (float)Points[i][0].Y, (float)Points[i][1].X, (float)Points[i][1].Y);
            }
        }
    }

}
