using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SchetsEditor {
	class ColorPicker : Control {
		private Bitmap wheelBitmap, wheelBitmapReal;
		private ColorConverter cc;
		Point colPoint;

		double value = 1.0;

		public ColorPicker(Size size){
			wheelBitmap = new Bitmap(size.Width, size.Height);
			colPoint = new Point(size.Width / 2, size.Height / 2);
			cc = new ColorConverter();
			drawWheelBitmap();

			this.Size = size;
			this.Paint += this.draw;
			this.Update();
		}

		private void draw(object sender, PaintEventArgs pea) {
			Graphics g = pea.Graphics;
			g.DrawImage(wheelBitmapReal, 0,0);
		}

		private void drawWheelBitmap() {
			for (int x=0; x < wheelBitmap.Width; x++) {
				for (int y=0; y < wheelBitmap.Height; y++) {
					wheelBitmap.SetPixel(x, y, getWheelPosColor(x, y));
				}
			}
			wheelBitmapReal = (Bitmap)wheelBitmap.Clone();
		}

		private void sneakyUpdateWheelBitmap() {
			//This is much faster than recalculating the entire bitmap every time
			//This gives a close enough estimation of the color that any user would not 
			//notice the difrence (if there is any)
			wheelBitmapReal = (Bitmap)wheelBitmap.Clone();
			Graphics g = Graphics.FromImage(wheelBitmapReal);
			SolidBrush kleur = new SolidBrush(Color.FromArgb((int)(255 - value * 255), 0, 0, 0));
			g.FillEllipse(kleur, 0, 0, 200, 200); //Draws a black cirlce over the bitmap with transperancy based on the Value
			Color curCol = wheelBitmap.GetPixel(colPoint.X, colPoint.Y);
			double[] hsv = cc.Color2HSV(curCol);
			curCol = cc.HSV2Color(hsv[0], hsv[1], value);
			g.FillEllipse(new SolidBrush(curCol), colPoint.X - 2, colPoint.Y - 2, 4, 4); //Draws current color circle
			if (value > 0.5)
				g.DrawEllipse(new Pen(new SolidBrush(Color.Black)), new Rectangle(colPoint.X - 2, colPoint.Y - 2, 4, 4)); //Draws a black cirlce around current color circle
			else
				g.DrawEllipse(new Pen(new SolidBrush(Color.White)), new Rectangle(colPoint.X - 2, colPoint.Y - 2, 4, 4)); //Draws a black cirlce around current color circle
		}

		public void SetValue(double val) {
			value = val;
			sneakyUpdateWheelBitmap();
			this.Invalidate();
		}

		public void SetColor(Color col) {
			double[] hsv = cc.Color2HSV(col);
			double angle = hsv[0]; //Hue = angle from center of circle
			double distance = wheelBitmap.Width/2 * hsv[1] - 2 * hsv[1]; //Saturation = distance from center (0-1 scaled to 0-radius)

			//Basic maths
			int y = (int)Math.Round(wheelBitmap.Height / 2 - distance * Math.Cos(angle/180*Math.PI));
			int x = (int)Math.Round(wheelBitmap.Width / 2 - distance * Math.Sin(angle / 180 * Math.PI));
			colPoint = new Point(x, y);

			sneakyUpdateWheelBitmap();
			this.Invalidate();
		}

		public Color getWheelPosColor(int x, int y) {
			Point center = new Point(wheelBitmap.Width / 2, wheelBitmap.Height / 2);
			double radius = wheelBitmap.Width / 2;

			int dX = center.X - x, dY = center.Y - y;

			double dist = Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY,2));

			if (dist > radius)
				return Color.FromArgb(0,0,0,0); //"null"

			double angle = Math.Atan2(dX, dY) * 180 / Math.PI;
			if (angle < 0)
				angle += 360;
			
			if (angle > 360 || angle < 0) {
				return Color.FromArgb(255, 0, 0, 0);
			}
			return cc.HSV2Color(angle, dist/radius, value);
		}
	}
}
