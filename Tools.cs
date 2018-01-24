using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace SchetsEditor
{
    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
    }

    public abstract class StartpuntTool : ISchetsTool {
		protected Point startpunt;
		protected Point origPoint;
		protected SchetsControl schetscontrol;
		protected Brush kwast;
		protected int brushWidth;
		protected IDrawnShape lastDrawnShape;

		public virtual void MuisVast(SchetsControl s, Point p){
			schetscontrol = s;
			brushWidth = s.brushWidth;
			startpunt = p;
			origPoint = p;
        }
        public virtual void MuisLos(SchetsControl s, Point p){
			kwast = new SolidBrush(s.PenKleur);
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
    }

    public class TekstTool : StartpuntTool {
        public override string ToString() { return "Text"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

		public override void Letter(SchetsControl s, char c) {
			Letter(s, startpunt, (SolidBrush)kwast, c, true);
		}

        public void Letter(SchetsControl s, Point startPoint, SolidBrush b, char c, bool addToList = false) {
            if (c >= 32) {
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Open Sans", 40);
                string tekst = c.ToString();
                SizeF sz = 
                gr.MeasureString(tekst, font, startPoint, StringFormat.GenericTypographic);
                gr.DrawString   (tekst, font, b,
											  startPoint, StringFormat.GenericTypographic);
                // gr.DrawRectangle(Pens.Black, startpunt.X, startpunt.Y, sz.Width, sz.Height);
                startpunt.X += (int)sz.Width;
				if (addToList) s.drawnShapes.Add(new DrawnLetter(startPoint, b.Color, c));
				s.Invalidate();
            }
			//Add possibility of a space
			if (c.ToString() == " ") {
				startpunt.X += 20;
			}
			if (c == (char)Keys.Enter) {
				Graphics gr = s.MaakBitmapGraphics();
				Font font = new Font("Open Sans", 40);
				SizeF sz =
					gr.MeasureString("W", font, startPoint, StringFormat.GenericTypographic);
				startpunt.Y += (int) sz.Height;
				startpunt.X = origPoint.X;
			}
        }
	}

    public abstract class TweepuntTool : StartpuntTool {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2){
			return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                                , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                                );
        }
		public static Rectangle Punten2Vierkant(Point p1, Point p2) {
			int dist1 = Math.Abs(p1.X - p2.X), dist2 = Math.Abs(p1.Y - p2.Y);
			//Set longest distance as actual size (for both width and height)
			int size = dist1 > dist2 ? dist1 : dist2;
			return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y))
								, new Size(size, size)
								);
		}
		public static Pen MaakPen(Brush b, int dikte){
			Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
        public override void MuisVast(SchetsControl s, Point p){
			base.MuisVast(s, p);
            kwast = Brushes.Gray;
        }
        public override void MuisDrag(SchetsControl s, Point p){
			s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {   base.MuisLos(s, p);
            this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
			if (lastDrawnShape != null) {
				schetscontrol.drawnShapes.Add(lastDrawnShape);
			}
			s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);
        
        public virtual void Compleet(Graphics g, Point p1, Point p2){
			this.Bezig(g, p1, p2);
        }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "Frame"; }

        public override void Bezig(Graphics g, Point p1, Point p2){
			if ((Control.ModifierKeys & Keys.Shift) != 0) {
				Rectangle rect = TweepuntTool.Punten2Vierkant(p1, p2);
				g.DrawRectangle(MaakPen(kwast, brushWidth), rect);
				lastDrawnShape = new DrawnRectangle(new Point(rect.X, rect.Y), new Point(rect.Right, rect.Bottom), brushWidth,  ((SolidBrush)kwast).Color);
			} else {
				g.DrawRectangle(MaakPen(kwast, brushWidth), TweepuntTool.Punten2Rechthoek(p1, p2));
				lastDrawnShape = new DrawnRectangle(p1, p2, brushWidth, ((SolidBrush)kwast).Color);
			}
		}

		public void Draw(Graphics g, Point startPoint, Point endPoint, int width, Brush col) {
			g.DrawRectangle(MaakPen(col, width), TweepuntTool.Punten2Rechthoek(startPoint, endPoint));
		}
    }
    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "Field"; }

		public override void Bezig(Graphics g, Point p1, Point p2) {
			if ((Control.ModifierKeys & Keys.Shift) != 0) {
				Rectangle rect = TweepuntTool.Punten2Vierkant(p1, p2);
				g.FillRectangle(kwast, rect);
				lastDrawnShape = new DrawnFilledRectangle(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Bottom), ((SolidBrush)kwast).Color);
			} else {
				g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
				lastDrawnShape = new DrawnFilledRectangle(p1, p2, ((SolidBrush)kwast).Color);
			}
		}

		public void Draw(Graphics g, Point startPoint, Point endPoint, Brush col) {
			g.FillRectangle(col, TweepuntTool.Punten2Rechthoek(startPoint, endPoint));
		}
    }

	public class CircleTool : TweepuntTool {
		public override string ToString() { return "Circle"; }

		public override void Bezig(Graphics g, Point p1, Point p2) {
			if ((Control.ModifierKeys & Keys.Shift) != 0) {
				Rectangle rect = TweepuntTool.Punten2Vierkant(p1, p2);
				g.DrawEllipse(MaakPen(kwast, brushWidth), rect);
				lastDrawnShape = new DrawnEllipse(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Bottom), brushWidth, schetscontrol.PenKleur);
			} else {
				g.DrawEllipse(MaakPen(kwast, brushWidth), TweepuntTool.Punten2Rechthoek(p1, p2));
				lastDrawnShape = new DrawnEllipse(p1, p2, brushWidth, schetscontrol.PenKleur);
			}
		}

		public void Draw(Graphics g, Point startPoint, Point endPoint, int width,  Brush col) {
			g.DrawEllipse(MaakPen(col, width), TweepuntTool.Punten2Rechthoek(startPoint, endPoint));
		}
	}

	public class FilledCircleTool : TweepuntTool {
		public override string ToString() { return "Ball"; }

		public override void Bezig(Graphics g, Point p1, Point p2) {
			if ((Control.ModifierKeys & Keys.Shift) != 0) {
				Rectangle rect = TweepuntTool.Punten2Vierkant(p1, p2);
				g.FillEllipse(kwast, rect);
				lastDrawnShape = new DrawnFilledEllipse(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Bottom), schetscontrol.PenKleur);
			} else {
				g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
				lastDrawnShape = new DrawnFilledEllipse(p1, p2, schetscontrol.PenKleur);
			}
		}

		public void Draw(Graphics g, Point startPoint, Point endPoint, Brush col) {
			g.FillEllipse(col, TweepuntTool.Punten2Rechthoek(startPoint, endPoint));
		}
	}

	public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "Line"; }

        public override void Bezig(Graphics g, Point p1, Point p2){
			g.DrawLine(MaakPen(this.kwast, brushWidth), p1, p2);
			lastDrawnShape = new DrawnLine(p1, p2, brushWidth, schetscontrol.PenKleur);
		}

		public void Draw(Graphics g, Point startPoint, Point endPoint, int width, Brush col) {
			g.DrawLine(MaakPen(col, width), startPoint, endPoint);
		}
    }

    public class PenTool : LijnTool
    {
        public override string ToString() { return "Pen"; }

        public override void MuisDrag(SchetsControl s, Point p){
			this.MuisLos(s, p);
            this.MuisVast(s, p);
        }
    }
    
    public class GumTool : ISchetsTool {
		private SoundPlayer sfxPlayer;
		Stream NoMistakesSFX = Properties.Resources.NoMistakes;

		public override string ToString() { return "Eraser"; }

		public void Letter(SchetsControl s, char c) { }
		public void MuisDrag(SchetsControl s, Point p) { }
		public void MuisVast(SchetsControl s, Point p) {
			sfxPlayer = new SoundPlayer();
			Random rnd = new Random(DateTime.Now.Millisecond); //Somewhat decent seed (not really but good enough for this)
			if (rnd.Next(0, 50) == 1) {
				NoMistakesSFX.Position = 0;
				sfxPlayer.Stream = NoMistakesSFX;
				sfxPlayer.Play();
			}
		}

		public void MuisLos(SchetsControl s, Point p) {
			if (s.drawnShapes.Remove(s.drawnShapes.FindLast(shape => shape.OnClick(s, p))))
				s.drawShapes();
		}
    }
}
