using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpiderGame
{
    public partial class fSpiderMapviewer : Form
    {
        const int TileWidth = 22;
        const int TileHeight = 22;
        const int RowSpacing = 2;

        const int MapWidth = 29;
        const int MapHeight = 16;

        private int[,] _map = new int[MapHeight, MapWidth];
        private bool _mapLoaded = false;
        private int _hoveredX = -1;
        private int _hoveredY = -1;
        private Bitmap _background;

        public fSpiderMapviewer()
        {
            InitializeComponent();

            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var files = Directory.GetFiles(Path.Combine(path, "LEVEL"), "PCSP*");
            foreach (var file in files)
            {
                listView1.Items.Add(Path.GetFileName(file));
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count < 1) return;

                _map = LoadMap("level.txt");
                _mapLoaded = true;

                // Create the final bitmap
                int imageWidth = MapWidth * TileWidth;
                int imageHeight = MapHeight * (TileHeight + RowSpacing) - RowSpacing; // No offset after last row

                _background = new Bitmap(imageWidth, imageHeight);
                using (Graphics g = Graphics.FromImage(_background))
                {
                    g.Clear(Color.Black);

                    // Draw tiles
                    for (int y = 0; y < MapHeight; y++)
                    {
                        for (int x = 0; x < MapWidth; x++)
                        {
                            int tileId = _map[y, x];
                            if (tileId == 0) continue;

                            string tilePath = Path.Combine("res", $"{tileId}.png");

                            if (File.Exists(tilePath))
                            {
                                using (Image tileImage = Image.FromFile(tilePath))
                                {
                                    int drawX = x * TileWidth;
                                    int drawY = y * (TileHeight + RowSpacing);
                                    g.DrawImage(tileImage, drawX, drawY, TileWidth, TileHeight);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Missing tile image: {tilePath}");
                            }
                        }
                    }
                }

                pictureBox1.BackgroundImage = _background;
            }
            catch { }
        }

        private int[,] LoadMap(string v)
        {
            string[] lines = File.ReadAllLines(listView1.SelectedItems[0].Text);

            // Constants for the map dimensions
            int[,] map = new int[MapHeight, MapWidth];

            // Validate input
            if (lines.Length - 1 != MapWidth * MapHeight)
            {
                throw new Exception("Invalid level file.");
            }

            int index = 0;
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    map[y, x] = int.Parse(lines[index + 1]);
                    index++;
                }
            }

            return map;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mapLoaded) return;

            int col = e.X / TileWidth;
            int row = e.Y / (TileHeight + RowSpacing);

            if (col >= 0 && col < 29 && row >= 0 && row < 16)
            {
                if (_hoveredX != col || _hoveredY != row)
                {
                    _hoveredX = col;
                    _hoveredY = row;

                    int tileValue = _map[row, col];
                    int index = col * 16 + row;
                    string tilePath = Path.Combine("res", $"{tileValue}.png");

                    if (File.Exists(tilePath) || tileValue ==0 )
                        this.Text = $"SPIDER Mapviewer - x={col}, y={row}, value={tileValue}";
                    else
                        this.Text = $"SPIDER Mapviewer - x={col}, y={row}, MISSING value={tileValue}";

                    pictureBox1.Invalidate(); // trigger repaint
                }
            }
            else
            {
                _hoveredX = _hoveredY = -1;
                this.Text = "SPIDER Mapviewer";
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (_hoveredX >= 0 && _hoveredY >= 0)
            {
                int x = _hoveredX * TileWidth;
                int y = _hoveredY * (TileHeight + RowSpacing);

                using (Brush highlightBrush = new SolidBrush(Color.FromArgb(100, Color.Yellow)))
                {
                    e.Graphics.FillRectangle(highlightBrush, x, y, TileWidth, TileHeight);
                }

                using (Pen borderPen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(borderPen, x, y, TileWidth - 1, TileHeight - 1);
                }
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            _hoveredX = _hoveredY = -1;
            pictureBox1.Invalidate();
        }
    }
}
