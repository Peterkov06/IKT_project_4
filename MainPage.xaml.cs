
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

        public MainPage()
        {
            InitializeComponent();
            GenerateGrid(5,5);
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
            gameGrid.Children.Add(graphicsView);
            GridParent.Children.Add(gameGrid);
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
