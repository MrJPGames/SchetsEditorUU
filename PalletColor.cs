using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Resources;

namespace SchetsEditor {
	public class PalletColor {
		string name;
		Color col;
		public Stream sfx;

		//Constructor
		public PalletColor(string n, Color c, Stream s) {
			name = n;
			col = c;
			sfx = s;
		}

		//Accessor
		public Color color {
			get {
				return col;
			}
		}

		//Override ToString method
		public override string ToString() {
			return name;
		}
	}
}
