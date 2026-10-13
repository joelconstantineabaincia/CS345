using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIP_Activity
{
    public partial class Form1 : Form
    {
        private Bitmap loadedImage;
        private Bitmap processedImage;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                loadedImage = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = loadedImage;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loadedImage == null)
            {
                MessageBox.Show("Please load an image first via File -> Open.");
                return;
            }

            int width = loadedImage.Width;
            int height = loadedImage.Height;

            // Step A: Grayscale & Global Thresholding
            // Background is white/light (> 180), coins are dark (< 180)
            bool[,] binary = new bool[width, height];
            processedImage = new Bitmap(width, height);
            int threshold = 180;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = loadedImage.GetPixel(x, y);
                    int gray = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);

                    if (gray < threshold)
                    {
                        binary[x, y] = true; // Coin pixel
                        processedImage.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        binary[x, y] = false; // Background
                        processedImage.SetPixel(x, y, Color.White);
                    }
                }
            }

            pictureBox2.Image = processedImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            // Step B: Connected-Component Labeling (BFS Flood-fill)
            bool[,] visited = new bool[width, height];
            List<CoinBlob> detectedCoins = new List<CoinBlob>();

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (binary[x, y] && !visited[x, y])
                    {
                        int area = 0;
                        int minX = x, maxX = x;
                        int minY = y, maxY = y;

                        Queue<Point> q = new Queue<Point>();
                        visited[x, y] = true;
                        q.Enqueue(new Point(x, y));

                        while (q.Count > 0)
                        {
                            Point p = q.Dequeue();
                            area++;

                            if (p.X < minX) minX = p.X;
                            if (p.X > maxX) maxX = p.X;
                            if (p.Y < minY) minY = p.Y;
                            if (p.Y > maxY) maxY = p.Y;

                            for (int i = 0; i < 4; i++)
                            {
                                int nx = p.X + dx[i];
                                int ny = p.Y + dy[i];

                                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                {
                                    if (binary[nx, ny] && !visited[nx, ny])
                                    {
                                        visited[nx, ny] = true;
                                        q.Enqueue(new Point(nx, ny));
                                    }
                                }
                            }
                        }

                        // Filter out small noise specks / dust (< 100 pixels)
                        if (area > 100)
                        {
                            // Step C: Hole Detection (identifies 5-centavo coins)


                            detectedCoins.Add(new CoinBlob
                            {
                                Area = area,
                            });
                        }
                    }
                }
            }

            // Step D: Print areas to Output window for calibration
            var solidAreas = detectedCoins.Where(c => !c.HasHole).Select(c => c.Area).OrderBy(a => a).ToList();
            System.Diagnostics.Debug.WriteLine("Solid coin pixel areas: " + string.Join(", ", solidAreas));

            // Step E: Classify Coins & Count Denominations
            int c5c = 0;
            int c10c = 0;
            int c25c = 0;
            int c1p = 0;
            int c5p = 0;
            const int T0 = 2900; // 5c  | 10c (gap 2701 -> 3189)
            const int T1 = 3800; // 10c | 25c (gap 3327 -> 4380)
            const int T2 = 5400; // 25c | 1P  (gap 4613 -> 6224)
            const int T3 = 7100; // 1P  | 5P  (gap 6392 -> 7780)

            foreach (var coin in detectedCoins)
            {
                if (coin.Area < T0) c5c++;
                else if (coin.Area < T1) c10c++;
                else if (coin.Area < T2) c25c++;
                else if (coin.Area < T3) c1p++;
                else c5p++;
            }

            double totalAmount = (c5c * 0.05) + (c10c * 0.10) + (c25c * 0.25) + (c1p * 1.00) + (c5p * 5.00);

            // Step F: Display Breakdown
            label1.Text =
                $"--- COIN COUNT BREAKDOWN ---\n" +
                $"5 Centavos  (₱0.05) : {c5c}\n" +
                $"10 Centavos (₱0.10) : {c10c}\n" +
                $"25 Centavos (₱0.25) : {c25c}\n" +
                $"1 Peso      (₱1.00) : {c1p}\n" +
                $"5 Pesos     (₱5.00) : {c5p}\n" +
                $"----------------------------\n" +
                $"Total Coins Detected : {detectedCoins.Count}\n" +
                $"Total Denomination   : ₱{totalAmount:F2}";
        }

        private class CoinBlob
        {
            public int Area { get; set; }
            public bool HasHole { get; set; }
        }
    }
}

