
using Microsoft.Maui.Graphics;
using System.Diagnostics;

namespace TestMAUI
{
    public partial class MainPage : ContentPage
    {
        Grid gameGrid;
        Button[,] gridBtns;
        int availableMirrors = 5;
        GraphicsView graphicsView = new();
        LaserBeam laserBeam = new LaserBeam();

        public MainPage()
        {
            InitializeComponent();
            GenerateGrid(5,5);
            gridBtns[0, 0].Loaded += (s, e) => { RendererLaser(); };
            gridBtns[0,0].SizeChanged += (s, e) => { RendererLaser(); };
            graphicsView.Drawable = laserBeam;
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
            Random rnd = new Random();
            GridTile startTile = new(rnd.Next(cols), rnd.Next(rows),10 + rnd.Next(4));
            GridTile endTile = new(rnd.Next(cols), rnd.Next(rows),20 + rnd.Next(4));
            DataHolder.SetupGrid(cols, rows, startTile, endTile);
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
                        button.Clicked += SetMirror;
                    }
                    Grid.SetColumn(button, j);
                    Grid.SetRow(button, i);
                    gridBtns[j, i] = button;
                    gameGrid.Children.Add(button);
                }
            }
            graphicsView.HorizontalOptions = LayoutOptions.FillAndExpand;
            graphicsView.VerticalOptions = LayoutOptions.FillAndExpand;
            graphicsView.InputTransparent = true;
            graphicsView.BackgroundColor = Colors.Transparent;
            GridParent.Children.Add(gameGrid);
            GridParent.Children.Add(graphicsView);
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
            }
        }

        void RendererLaser()
        {
            var startCoord = FindStart();
            Button button = gridBtns[startCoord[0], startCoord[1]];
            laserBeam.StartPoint = new Point(button.Bounds.X + (button.Bounds.Width / 2), button.Bounds.Y + (button.Bounds.Height / 2));
            int way = DataHolder.DataGrid[startCoord[0], startCoord[1]];
            if (way >= 20)
            {
                way -= 20;
            }
            else if (way >= 10)
            {
                way -= 10;
            }
            int endX = 0;
            int endY = 0;
            switch (way)
            {
                case 0:
                    endY = startCoord[1];
                    break;
                case 1:
                    endY = startCoord[1];
                    break;
                default:
                    break;
            }
            laserBeam.EndPoint = new Point(endX, endY);
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
    }
    public class LaserBeam: IDrawable
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.OrangeRed;
            canvas.StrokeSize = 5f;
            canvas.Antialias = true;

            canvas.DrawLine((float)StartPoint.X, (float)StartPoint.Y, (float)EndPoint.X, (float)EndPoint.Y);
        }
    }

}
