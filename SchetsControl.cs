using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Collections.Generic;

namespace SchetsEditor{
	public class SchetsControl : UserControl{
		public List<IDrawnShape> drawnShapes = new List<IDrawnShape>();
		private Schets schets;
        private Color penkleur;
		private int brushwidth = 3;

		private SoundPlayer sfxPlayer;

		private ColorConverter cc;

        public Color PenKleur {
			get {
				return penkleur;
			}
        }
		public int brushWidth {
			get {
				return brushwidth;
			}
			set {
				brushwidth = value;
			}
		}
        public Schets Schets {
			get {
				return schets;
			}
        }
        public SchetsControl(List<IDrawnShape> importShapes = null) {
			this.BorderStyle = BorderStyle.Fixed3D;
            this.schets = new Schets();
            this.Paint += this.teken;
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
			this.penkleur = Color.FromArgb(255, 0, 0, 0);
			sfxPlayer = new SoundPlayer();
			cc = new ColorConverter();
			if (importShapes != null) {
				drawnShapes = importShapes;
				drawShapes();
			}
        }
		public void SetDrawnShapes(List<IDrawnShape> importShapes) {
			drawnShapes = importShapes;
		}
        protected override void OnPaintBackground(PaintEventArgs e){

        }
        private void teken(object o, PaintEventArgs pea) {
			schets.Teken(pea.Graphics);
        }
		public void drawShapes() {
			schets.Schoon();
			foreach (IDrawnShape shape in drawnShapes) {
				shape.Draw(this);
			}
			this.Invalidate();
		}
        private void veranderAfmeting(object o, EventArgs ea){
			schets.VeranderAfmeting(this.ClientSize);
            this.Update();
        }
		public void veranderAfmeting(Size customSize) {
			schets.VeranderAfmeting(customSize);
			this.ClientSize = customSize;
			drawShapes();
		}
		public Graphics MaakBitmapGraphics() {
			Graphics g = schets.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }
        public void Schoon(object o, EventArgs ea) {
			schets.Schoon();
			for (int i = drawnShapes.Count - 1; i >= 0; i--) {
				drawnShapes.RemoveAt(i);
			}
			drawShapes();
        }
        public void Roteer(object o, EventArgs ea) {
			schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
            schets.Roteer();
            this.Update();
        }
		public void Undo(object sender, EventArgs ea) {
			if (drawnShapes.Count > 0) {
				drawnShapes.RemoveAt(drawnShapes.Count - 1);
				if (drawnShapes.Count == 0) { //Nothing to undo anymore
					((Button)sender).Enabled = false;
				}
			}
			this.drawShapes();
		}
        public void ChangeColor(object obj, EventArgs ea) { 
			PalletColor cb = (PalletColor)((RadioButton)obj).Tag;
			//Play bob saying the name of the color!
			if (cb.sfx != null) {
				cb.sfx.Position = 0;
				sfxPlayer.Stream = cb.sfx;
				sfxPlayer.Play();
			}
			int alpha = penkleur.A; //Save current alpha level
			penkleur = cb.color; //Set RGB values from pallet
			penkleur = Color.FromArgb(alpha, penkleur.R, penkleur.G, penkleur.B); //Apply saved alpha level to new pen color
        }
		public void ChangeColorAlpha(object obj, EventArgs ea) {
			int alpha = ((TrackBar)obj).Value;
			penkleur = Color.FromArgb(alpha, penkleur.R, penkleur.G, penkleur.B);
		}
		public void ChangeColorRGB(int R, int G, int B) {
			penkleur = Color.FromArgb(penkleur.A, R, G, B);
		}
		public void ChangeColor(Color col) {
			Color col2 = Color.FromArgb(penkleur.A, col.R, col.G, col.B);
			penkleur = col2;
		}
		public void ChangeColorVal(object sender, EventArgs ea) {
			double val = ((TrackBar)sender).Value / 100.0;
			double[] hsv = cc.Color2HSV(penkleur);
			penkleur = cc.HSV2Color(hsv[0], hsv[1], val, penkleur.A);
		}
		public void ChangeColorHue(object sender, EventArgs ea) {
			double val = ((TrackBar)sender).Value;
			double[] hsv = cc.Color2HSV(penkleur);
			penkleur = cc.HSV2Color(val, hsv[1], hsv[2], penkleur.A);
		}
		public void ChangeColorSaturation(object sender, EventArgs ea) {
			double val = ((TrackBar)sender).Value / 100.0;
			double[] hsv = cc.Color2HSV(penkleur);
			penkleur = cc.HSV2Color(hsv[0], val, hsv[2], penkleur.A);
		}
		public void ChangeBrushWidth(object sender, EventArgs ea) {
			double val = ((TrackBar)sender).Value;
			brushwidth = (int)val;
		}
	}
}
