using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SchetsEditor {
	class ColorConverter {
		public Color HSV2Color(double h, double s, double v, int alpha = 255) {
			//Method of translation used can be found here: http://www.rapidtables.com/convert/color/hsv-to-rgb.htm

			int R, G, B;
			double tR = 0.0, tG = 0.0, tB = 0.0, C, X, m;

			C = v * s;
			X = C * (1 - Math.Abs(((double)h / 60) % 2 - 1));
			m = v - C;

			if (h < 60) {
				tR = C;
				tG = X;
			} else if (h < 120) {
				tR = X;
				tG = C;
			} else if (h < 180) {
				tG = C;
				tB = X;
			} else if (h < 240) {
				tG = X;
				tB = C;
			} else if (h < 300) {
				tR = X;
				tB = C;
			} else {
				tR = C;
				tB = X;
			}

			R = (int)((tR + m) * 255);
			G = (int)((tG + m) * 255);
			B = (int)((tB + m) * 255);

			return Color.FromArgb(alpha, R, G, B);
		}
		private double findMax(double a, double b, double c) {
			return Math.Max(a, Math.Max(b, c));
		}
		private double findMin(double a, double b, double c) {
			return Math.Min(a, Math.Min(b, c));
		}
		public double[] Color2HSV(Color col){
			double r = col.R / 255.0;
			double g = col.G / 255.0;
			double b = col.B / 255.0;

			double h, s, v; // h:0-360.0, s:0.0-1.0, v:0.0-1.0

			double max = findMax(r, g, b);
			double min = findMin(r, g, b);

			v = max;

			if (max == 0.0f) {
				s = 0;
				h = 0;
			} else if (max - min == 0.0) {
				s = 0;
				h = 0;
			} else {
				s = (max - min) / max;

				if (max == r) {
					h = 60 * ((g - b) / (max - min)) + 0;
				} else if (max == g) {
					h = 60 * ((b - r) / (max - min)) + 120;
				} else {
					h = 60 * ((r - g) / (max - min)) + 240;
				}
			}

			if (h < 0) h += 360.0f;
			
			return new double[] {h,s,v};
		}
	}
}
