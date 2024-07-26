using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paint_App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Set the initial size of the form
            this.Width = 1000;
            this.Height = 800;

            // Initialize the bitmap and graphics object for drawing
            bm = new Bitmap(picture.Width, picture.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White); // Clear the bitmap with a white background
            picture.Image = bm; // Set the picture box image to the bitmap
        }

        Bitmap bm; // Bitmap for drawing surface
        Graphics g; // Graphics object for drawing
        bool paint = false; // Flag to check if painting is active
        Point px, py; // Points for drawing
        Pen p = new Pen(Color.Black, 1); // Pen for drawing shapes
        Pen erase = new Pen(Color.White, 20); // Pen for erasing shapes
        int index; // Tool index (1: pencil, 2: eraser, 3: ellipse, 4: rectangle, 5: line, 7: fill)
        int x, y, sX, sY, cX, cY; // Coordinates for drawing shapes

        ColorDialog cd = new ColorDialog(); // Color dialog for color selection
        Color new_color; // Selected color

        // Event handler for mouse down event on the picture box
        private void picture_MouseDown(object sender, MouseEventArgs e)
        {
            paint = true; // Start painting
            py = e.Location; // Set previous point location

            // Set initial coordinates for shape drawing
            cX = e.X;
            cY = e.Y;
        }

        // Event handler for mouse move event on the picture box
        private void picture_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint)
            {
                if (index == 1) // Pencil tool
                {
                    px = e.Location;
                    g.DrawLine(p, px, py); // Draw line with the pen
                    py = px; // Update previous point
                }

                if (index == 2) // Eraser tool
                {
                    px = e.Location;
                    g.DrawLine(erase, px, py); // Draw line with the eraser
                    py = px; // Update previous point
                }
            }

            picture.Refresh(); // Refresh the picture box to show drawing

            // Update coordinates for shape drawing
            x = e.X;
            y = e.Y;
            sX = e.X - cX;
            sY = e.Y - cY;
        }

        // Event handler for mouse up event on the picture box
        private void picture_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false; // Stop painting

            // Calculate width and height for shape drawing
            sX = x - cX;
            sY = y - cY;

            // Draw the selected shape
            if (index == 3) // Ellipse tool
            {
                g.DrawEllipse(p, cX, cY, sX, sY);
            }

            if (index == 4) // Rectangle tool
            {
                g.DrawRectangle(p, cX, cY, sX, sY);
            }

            if (index == 5) // Line tool
            {
                g.DrawLine(p, cX, cY, x, y);
            }
        }

        // Event handler for pencil button click
        private void btn_pencil_Click(object sender, EventArgs e)
        {
            index = 1; // Set tool to pencil
        }

        // Event handler for eraser button click
        private void btn_eraser_Click(object sender, EventArgs e)
        {
            index = 2; // Set tool to eraser
        }

        // Event handler for ellipse button click
        private void btn_ellipse_Click(object sender, EventArgs e)
        {
            index = 3; // Set tool to ellipse
        }

        // Event handler for rectangle button click
        private void btn_rect_Click(object sender, EventArgs e)
        {
            index = 4; // Set tool to rectangle
        }

        // Event handler for line button click
        private void btn_line_Click(object sender, EventArgs e)
        {
            index = 5; // Set tool to line
        }

        // Event handler for color button click
        private void btn_color_Click(object sender, EventArgs e)
        {
            cd.ShowDialog(); // Show color dialog
            new_color = cd.Color; // Get selected color
            pic_color.BackColor = cd.Color; // Update color display
            p.Color = cd.Color; // Set pen color
        }

        // Event handler for painting on the color button (for drawing previews)
        private void btn_color_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (paint)
            {
                if (index == 3) // Draw ellipse preview
                {
                    g.DrawEllipse(p, cX, cY, sX, sY);
                }

                if (index == 4) // Draw rectangle preview
                {
                    g.DrawRectangle(p, cX, cY, sX, sY);
                }

                if (index == 5) // Draw line preview
                {
                    g.DrawLine(p, cX, cY, x, y);
                }
            }
        }

        // Event handler for clear button click
        private void btn_clear_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White); // Clear the drawing surface
            picture.Image = bm; // Update picture box image
            index = 0; // Reset tool index
        }

        // Helper method to adjust point location based on picture box size
        static Point set_point(PictureBox pb, Point pt)
        {
            float pX = 1f * pb.Image.Width / pb.Width;
            float pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }

        



        // Event handler for color picker click
        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = set_point(color_picker, e.Location);
            pic_color.BackColor = ((Bitmap)color_picker.Image).GetPixel(point.X, point.Y); // Get the color from the clicked point
            new_color = pic_color.BackColor; // Update new color
            p.Color = pic_color.BackColor; // Set pen color
        }

        // Method to validate and fill the area with a new color (used for flood fill)
        private void validate(Bitmap bm, Stack<Point> sp, int x, int y, Color old_color, Color new_color)
        {
            Color cx = bm.GetPixel(x, y);
            if (cx == old_color)
            {
                sp.Push(new Point(x, y));
                bm.SetPixel(x, y, new_color);
            }
        }

        // Method to fill an area with a new color
        public void Fill(Bitmap bm, int x, int y, Color new_clr)
        {
            Color old_color = bm.GetPixel(x, y);
            Stack<Point> pixel = new Stack<Point>();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, new_clr);
            if (old_color == new_clr) return;
            while (pixel.Count > 0)
            {
                Point pt = (Point)pixel.Pop();
                if (pt.X > 0 && pt.Y > 0 && pt.X < bm.Width - 1 && pt.Y < bm.Height - 1)
                {
                    validate(bm, pixel, pt.X - 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X + 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y - 1, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y + 1, old_color, new_clr);
                }
            }
        }

        // Event handler for mouse click on the picture box for fill tool
        private void picture_MouseClick(object sender, MouseEventArgs e)
        {
            if (index == 7) // Fill tool
            {
                Point point = set_point(picture, e.Location);
                Fill(bm, point.X, point.Y, new_color); // Fill the area with the selected color
            }
        }

        // Event handler for fill button click
        private void btn_fill_Click(object sender, EventArgs e)
        {
            index = 7; // Set tool to fill
        }

        // Event handler for save button click
        private void btn_save_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Image (.jpg)|*.jpg"; // Filter for saving as JPG image
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap btm = bm.Clone(new Rectangle(0, 0, picture.Width, picture.Height), bm.PixelFormat);
                btm.Save(sfd.FileName, ImageFormat.Jpeg); // Save the bitmap as a JPG file
            }
        }
    }
}
