
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Diagnostics;

namespace TestMAUI
{
    public partial class MainPage : ContentPage
    {
        Grid gameGrid;
        Button[,] gridBtns;
        int usedMirrors = 0;
        int selectedWay = 1;
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
            SetBarriers();
            GenerateView();
        }

        void SetStartAndEnd(int cols, int rows)
        {
            GridTile startTile = SelectCoordsOnTable(cols,rows, 10);
            GridTile endTile = SelectCoordsOnTable(cols,rows, 20);
            DataHolder.SetupGrid(cols, rows, startTile, endTile);
            SizeCangedEvent += delegate { RendererLaser(); };
        }

        void SetBarriers()
        {
            int barrierNum = random.Next(0, Convert.ToInt32(Math.Round((decimal)(DataHolder.DataGrid.Length / 3))));
            int xLength = DataHolder.DataGrid.GetLength(0);
            int yLength = DataHolder.DataGrid.GetLength(1);
            for (int i = 0;i < barrierNum;i++)
            {
                int x, y;
                GridTile firstL = FirstLaserTile(), lastL = LastLaserTile();
                int[] start = FindStart(), end = FindEnd();
                Debug.Write($"{end[0]}, {end[1]} \n");
                Debug.Write($"{start[0]}, {start[1]}\n");
                Debug.Write($"{firstL.X}, {firstL.Y}\n");
                Debug.Write($"{lastL.X}, {lastL.Y}\n");
                do
                {
                    x = random.Next(0, xLength);
                    y = random.Next(0, yLength);
                } while (DataHolder.DataGrid[x,y] != 0 && ((x == firstL.X && y == firstL.Y) || ((x == lastL.X && y == lastL.Y))) && ((x == start[0] && y == start[1]) || ((x == end[0] && y == end[1]))));
                DataHolder.DataGrid[x,y] = -1;
            }
        }

        GridTile FirstLaserTile()
        {
            int[] first = FindStart();
            int way = DataHolder.DataGrid[first[0], first[1]];
            switch (way)
            {
                case 10:
                    return new GridTile(first[0], first[1] - 1, 0);
                case 11:
                    return new GridTile(first[0] + 1, first[1], 0);
                case 12:
                    return new GridTile(first[0], first[1] + 1, 0);
                case 13:
                    return new GridTile(first[0] - 1, first[1], 0);
                default:
                    return new (0,0,0);
            }
        }

        GridTile LastLaserTile()
        {
            int[] first = FindEnd();
            int way = DataHolder.DataGrid[first[0], first[1]];
            switch (way)
            {
                case 20:
                    return new GridTile(first[0], first[1] + 1, 0);
                case 21:
                    return new GridTile(first[0] + 1, first[1], 0);
                case 22:
                    return new GridTile(first[0], first[1] - 1, 0);
                case 23:
                    return new GridTile(first[0] - 1, first[1], 0);
                default:
                    return new(0, 0, 0);
            }
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
                    if (value == 0)
                    {
                        int x = j;
                        int y = i;
                        button.Clicked += delegate 
                        { 
                            if (button.BackgroundColor == Colors.White)
                            {
                                DataHolder.PlaceMirror(x, y, 0);
                                selectedWay++;
                            }
                            else
                            {
                                DataHolder.PlaceMirror(x,y,selectedWay); 
                            }
                            if (selectedWay > 4)
                            {
                                selectedWay = 1;
                            }
                        };
                        button.Clicked += SetMirror;
                    }
                    else if (value >= 10 && value < 14)
                    {
                        button.BackgroundColor = Colors.Green;
                    }
                    else if (value >= 20 && value < 24)
                    {
                        button.BackgroundColor = Colors.Red;
                    }
                    else if (value == -1)
                    {
                        button.BackgroundColor = Colors.Gray;
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
                    usedMirrors--;
                    btn.BackgroundColor = Color.FromArgb("#ac99ea");
                }
                else
                {
                    usedMirrors++;
                    btn.BackgroundColor = Colors.White;
                }
                RendererLaser();
                UpdateMirrorLabel();
            }
        }

        void Restart(object? sender, EventArgs e)
        {
            graphicsView.Drawable = null;
            usedMirrors = 0;
            UpdateMirrorLabel();
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
                            if (DataHolder.DataGrid[startCoord[0], y] > 0 && DataHolder.DataGrid[startCoord[0], y] < 4)
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
                                if (way == finalWay)
                                {
                                    endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                    EndDialogue();
                                }
                                break;
                            }
                            else if (DataHolder.DataGrid[startCoord[0], y] != 0)
                            {
                                endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                startCoord = null;
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
                            if (DataHolder.DataGrid[x, startCoord[1]] > 1 && DataHolder.DataGrid[x, startCoord[1]] < 5 )
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
                                if (way == finalWay)
                                {
                                    endX = gridBtns[x, startCoord[1]].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[x, startCoord[1]].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                    EndDialogue();
                                }
                                break;
                            }
                            else if (DataHolder.DataGrid[x, startCoord[1]] != 0)
                            {
                                endX = gridBtns[x, startCoord[1]].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[x, startCoord[1]].Bounds.Y + (startButton.Bounds.Height / 2);
                                startCoord = null;
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
                            if (DataHolder.DataGrid[startCoord[0], y] > 0 && DataHolder.DataGrid[startCoord[0], y] < 5 && DataHolder.DataGrid[startCoord[0], y] != 2)
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
                                if (way == finalWay)
                                {
                                    endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                    EndDialogue();
                                }
                                break;
                            }
                            else if (DataHolder.DataGrid[startCoord[0], y] != 0)
                            {
                                endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                startCoord = null;
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
                            if (DataHolder.DataGrid[x, startCoord[1]] > 0 && DataHolder.DataGrid[x, startCoord[1]] < 5 && DataHolder.DataGrid[x, startCoord[1]] != 3)
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
                                if (way == finalWay)
                                {
                                    endX = gridBtns[x, startCoord[1]].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[x, startCoord[1]].Bounds.Y + (startButton.Bounds.Height / 2);
                                    startCoord = null;
                                    EndDialogue();
                                }
                                break;
                            }
                            else if (DataHolder.DataGrid[x, startCoord[1]] != 0)
                            {
                                endX = gridBtns[x, startCoord[1]].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[x, startCoord[1]].Bounds.Y + (startButton.Bounds.Height / 2);
                                startCoord = null;
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
                            if (DataHolder.DataGrid[startCoord[0], y] > 0 && DataHolder.DataGrid[startCoord[0], y] < 4)
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
                                if (way == finalWay)
                                {
                                    endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                    endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                    EndDialogue();
                                    startCoord = null;
                                }
                                break;
                            }
                            else if (DataHolder.DataGrid[startCoord[0], y] != 0)
                            {
                                endX = gridBtns[startCoord[0], y].Bounds.X + (startButton.Bounds.Width / 2);
                                endY = gridBtns[startCoord[0], y].Bounds.Y + (startButton.Bounds.Height / 2);
                                startCoord = null;
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

        int[] FindStart()
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
            throw new Exception($"NO START y: {DataHolder.DataGrid.GetLength(1)} x: {DataHolder.DataGrid.GetLength(0)}");
        }
        int[] FindEnd()
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
            throw new Exception($"NO End y: {DataHolder.DataGrid.GetLength(1)} x: {DataHolder.DataGrid.GetLength(0)}");
        }

        async void EndDialogue()
        {
            bool answer = await DisplayAlert("Challange Complete!", "Would you like to play a new one?", "Yes", "No" );
            if (answer)
            {
                Restart(this, null);
            }
        }

        void UpdateMirrorLabel()
        {
            MirrorNumLbl.Text = $"Used mirrors: {usedMirrors}";
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
