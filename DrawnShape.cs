using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SchetsEditor {
	public interface IDrawnShape {
		void Draw(SchetsControl s);
		bool OnClick(SchetsControl s, Point p);
	}

	public abstract class DrawnShape : IDrawnShape {
		protected Point startPoint, endPoint;
		protected Color col;

		public DrawnShape(Point startPoint, Point endPoint, Color col) {
			this.startPoint = startPoint;
			this.endPoint = endPoint;
			this.col = col;
		}

		public override string ToString() {
			return this.GetType() + " " + startPoint.X + " " + startPoint.Y + " " + endPoint.X + " " + endPoint.Y + " " + col.A + " " + col.R + " " + col.G + " " + col.B + "\n";
		}
		public abstract void Draw(SchetsControl s);
		public abstract bool OnClick(SchetsControl s, Point p);
	}

	public abstract class DrawnText : IDrawnShape {
		protected Point startPoint;
		protected Color col;
		protected char letter;

		public DrawnText(Point startPoint, Color col, char letter) {
			this.startPoint = startPoint;
			this.col = col;
			this.letter = letter;
		}

		public override string ToString() {
			return this.GetType() + " " + startPoint.X + " " + startPoint.Y + " " + col.A + " " + col.R + " " + col.G + " " + col.B + " " + letter + "\n";
		}

		public abstract void Draw(SchetsControl s);
		public abstract bool OnClick(SchetsControl s, Point p);
	}

	public class DrawnLetter : DrawnText {
		public DrawnLetter(Point startPunt, Color kleur, char letter) : base(startPunt, kleur, letter) { }

		public override void Draw(SchetsControl s) {
			new TekstTool().Letter(s, this.startPoint, new SolidBrush(this.col), this.letter);
		}
		
		//Was there a click within the collision bounds of the letter?
		public override bool OnClick(SchetsControl s, Point p) {
			Graphics gr = s.MaakBitmapGraphics();
			Font font = new Font("Comic Sans", 40);
			SizeF sz = gr.MeasureString(letter.ToString(), font, startPoint, StringFormat.GenericTypographic);
			return (p.X >= startPoint.X && p.X <= startPoint.X + sz.Width && p.Y >= startPoint.Y && p.Y <= startPoint.Y + sz.Height);
		}
	}

	public class DrawnRectangle : DrawnShape {
		int width;
		public DrawnRectangle(Point startPoint, Point endPoint, int inWidth, Color col) : base(startPoint, endPoint, col) {
			width = inWidth;
		}

		public override string ToString() {
			return this.GetType() + " " + startPoint.X + " " + startPoint.Y + " " + endPoint.X + " " + endPoint.Y + " " + col.A + " " + col.R + " " + col.G + " " + col.B + " " + width + " \n";
		}

		public override void Draw(SchetsControl s) {
			new RechthoekTool().Draw(s.MaakBitmapGraphics(), startPoint, endPoint, width, new SolidBrush(col));
		}

		//Was there a click on the edge of the rectangle
		public override bool OnClick(SchetsControl s, Point p) {
			int offset = width + 5; //Amount of pixels next to actual line that will be counted as a hit
			int upperBorder = Math.Min(startPoint.Y, endPoint.Y);
			int underBorder = Math.Max(startPoint.Y, endPoint.Y);
			int leftBorder = Math.Min(startPoint.X, endPoint.X);
			int rightBorder = Math.Max(startPoint.X, endPoint.X);
			//Left, right, up, down (in that order)		
			return (
				(p.X >= leftBorder - offset && p.X <= leftBorder + offset && p.Y >= upperBorder - offset && p.Y <= underBorder + offset) ||
				(p.X >= rightBorder - offset && p.X <= rightBorder + offset && p.Y >= upperBorder - offset && p.Y <= underBorder + offset) ||
				(p.X >= leftBorder - offset && p.X <= rightBorder + offset && p.Y >= upperBorder - offset && p.Y <= upperBorder + offset) ||
				(p.X >= leftBorder - offset && p.X <= rightBorder + offset && p.Y >= underBorder - offset && p.Y <= underBorder + offset)
			);
		}
	}

	public class DrawnFilledRectangle : DrawnRectangle {
		public DrawnFilledRectangle(Point startPoint, Point endPoint, Color col) : base(startPoint, endPoint, 0,  col) { }

		public override void Draw(SchetsControl s) {
			new VolRechthoekTool().Draw(s.MaakBitmapGraphics(), startPoint, endPoint, new SolidBrush(col));
		}

		public override bool OnClick(SchetsControl s, Point p) {
			Rectangle rect = new Rectangle(new Point(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y))
							   , new Size(Math.Abs(startPoint.X - endPoint.X), Math.Abs(startPoint.Y - endPoint.Y))
							   );
			Point upLeftCorner = new Point(rect.X, rect.Y);
			return (p.X >= upLeftCorner.X && p.X <= upLeftCorner.X + rect.Width && p.Y >= upLeftCorner.Y && p.Y <= upLeftCorner.Y + rect.Height);
		}
	}

	public class DrawnEllipse : DrawnShape {
		int width;
		public DrawnEllipse(Point startPoint, Point endPoint, int inWidth, Color col) : base(startPoint, endPoint, col) {
			width = inWidth;
		}

		public override void Draw(SchetsControl s) {
			new CircleTool().Draw(s.MaakBitmapGraphics(), startPoint, endPoint, width, new SolidBrush(col));
		}

		public override string ToString() {
			return this.GetType() + " " + startPoint.X + " " + startPoint.Y + " " + endPoint.X + " " + endPoint.Y + " " + col.A + " " + col.R + " " + col.G + " " + col.B + " " + width + " \n";
		}

		//Was there clicked within the ellipse?
		//Source: http://stackoverflow.com/questions/13285007/how-to-determine-if-a-point-is-within-an-ellipse
		public override bool OnClick(SchetsControl s, Point p) {
			int width = Math.Abs(endPoint.X - startPoint.X);
			int height = Math.Abs(endPoint.Y - startPoint.Y);
			int left = Math.Min(startPoint.X, endPoint.X);
			int upper = Math.Min(startPoint.Y, endPoint.Y);

			Point middelpunt = new Point(left + (width / 2), upper + (height / 2));

			double radiusX = (double)(width / 2);
			double radiusY = (double)(height / 2);

			if (radiusX <= 0.0 || radiusY <= 0.0)
				return false;

			Point normalisedPoint = new Point(p.X - middelpunt.X, p.Y - middelpunt.Y);

			return ((double)(normalisedPoint.X * normalisedPoint.X)
					 / (radiusX * radiusX)) + ((double)(normalisedPoint.Y * normalisedPoint.Y) / (radiusY * radiusY))
				<= 1.0;
		}
	}

	public class DrawnFilledEllipse : DrawnEllipse {
		public DrawnFilledEllipse(Point startPoint, Point endPoint, Color kleur) : base(startPoint, endPoint, 0, kleur) { }

		public override void Draw(SchetsControl s) {
			new FilledCircleTool().Draw(s.MaakBitmapGraphics(), startPoint, endPoint, new SolidBrush(col));
		}
	}

	public class DrawnLine : DrawnShape {
		int width;
		public DrawnLine(Point startPoint, Point endPoint, int inWidth, Color col) : base(startPoint, endPoint, col) {
			width = inWidth;
		}

		public override string ToString() {
			return this.GetType() + " " + startPoint.X + " " + startPoint.Y + " " + endPoint.X + " " + endPoint.Y + " " + col.A + " " + col.R + " " + col.G + " " + col.B + " " + width + " \n";
		}

		public override void Draw(SchetsControl s) {
			new LijnTool().Draw(s.MaakBitmapGraphics(), startPoint, endPoint, width, new SolidBrush(col));
		}
		
		/// Gebaseerd op: http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
		public override bool OnClick(SchetsControl s, Point p) {
			int maxDistance = width + 5; //5 was chosen by fair dice roll
			float px = endPoint.X - startPoint.X;
			float py = endPoint.Y - startPoint.Y;
			float temp = (px * px) + (py * py);
			float u = ((p.X - startPoint.X) * px + (p.Y - startPoint.Y) * py) / (temp);

			if (u > 1)
				u = 1;
			else if (u < 0)
				u = 0;

			float x = startPoint.X + u * px;
			float y = startPoint.Y + u * py;

			float dx = x - p.X;
			float dy = y - p.Y;
			double distance = Math.Sqrt(dx * dx + dy * dy);
			return distance <= maxDistance;
		}
	}
}
