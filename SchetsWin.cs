using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Linq;

namespace SchetsEditor
{
    public class SchetsWin : Form
    {   
        MenuStrip menuStrip;
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel bottomPanel, rightPanel;

		private SoundPlayer sfxPlayer;
		Stream beatingDevilSfx = Properties.Resources.DevilsBeating;

		bool vast;
		bool changed = false;

		Size minSize = new Size(740,550);
		ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );

        private void veranderAfmeting(object o, EventArgs ea)
        {
            schetscontrol.Size = new Size ( this.ClientSize.Width  - schetscontrol.Location.X - rightPanel.Size.Width
                                          , this.ClientSize.Height - bottomPanel.Height - schetscontrol.Location.Y);
			bottomPanel.Location = new Point(schetscontrol.Location.X, this.ClientSize.Height - bottomPanel.Height);
			rightPanel.Location = new Point(this.ClientSize.Width - rightPanel.Width, 0);
        }

		private void veranderAfmeting(Size sz) {
			ClientSize = sz;
			schetscontrol.Size = new Size(this.ClientSize.Width - schetscontrol.Location.X - rightPanel.Size.Width
										  , this.ClientSize.Height - bottomPanel.Height - schetscontrol.Location.Y);
			bottomPanel.Location = new Point(schetscontrol.Location.X, this.ClientSize.Height - bottomPanel.Height);
			rightPanel.Location = new Point(this.ClientSize.Width - rightPanel.Width, 0);
		}

		private void changeSizeFromSketchSize(Size sz) {
			Size sz2 = new Size(sz.Width + schetscontrol.Location.X + rightPanel.Size.Width,
								sz.Height + bottomPanel.Height + schetscontrol.Location.Y);
			veranderAfmeting(sz2);
		}

		private void klikToolMenu(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
        }

        private void klikToolButton(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
        }

		private void Exit(object obj, EventArgs ea)
        {
            this.Close();
        }

        public SchetsWin()
        {
            ISchetsTool[] deTools = { new PenTool()         
                                    , new LijnTool()
                                    , new RechthoekTool()
									, new VolRechthoekTool()
									, new CircleTool()
									, new FilledCircleTool()
									, new TekstTool()
                                    , new GumTool()
                                    };

			//Color names linked with the RGB color values (Bob Ross Colors)
			PalletColor[] deKleuren =
			{
				new PalletColor("Titanium White", Color.FromArgb(255,255,255), Properties.Resources.TitaniumWhite),
				new PalletColor("Bright Red", Color.FromArgb(219,0,0), Properties.Resources.BrightRed),
				new PalletColor("Indian Yellow", Color.FromArgb(255,184,0), Properties.Resources.IndianYellow),
				new PalletColor("Yellow Ochre", Color.FromArgb(199,155,0), Properties.Resources.YellowOchre),
				new PalletColor("Cadmium Yellow", Color.FromArgb(255,236,0), Properties.Resources.CadmiumYellow),
				new PalletColor("Phthalo Green", Color.FromArgb(16,46,60), Properties.Resources.PhthaloGreen),
				new PalletColor("Phthalo Blue", Color.FromArgb(12,0,64), Properties.Resources.PhthaloBlue),
				new PalletColor("Prussian Blue", Color.FromArgb(2,30,68), Properties.Resources.PrussianBlue),
				new PalletColor("Midnight Black", Color.FromArgb(0,0,0), Properties.Resources.MidnightBlack),
				new PalletColor("Dark Sienna", Color.FromArgb(95,46,31), Properties.Resources.DarkSienna),
				new PalletColor("Van Dyke Brown", Color.FromArgb(34,27,21), Properties.Resources.VanDykeBrown),
				new PalletColor("Alizarin Crimson", Color.FromArgb(78,21,0), Properties.Resources.AlizarinCrimson),
				new PalletColor("Sap Green", Color.FromArgb(10,52,16), Properties.Resources.SapGreen)
			};
				

            this.ClientSize = minSize;
            huidigeTool = deTools[0];

            schetscontrol = new SchetsControl();
            schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {   vast=true;  
                                           huidigeTool.MuisVast(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisDrag(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisLos (schetscontrol, mea.Location);
                                           vast = false;
										   Controls.Find("UndoButton", true).FirstOrDefault().Enabled = true;
										   changed = true;
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {	huidigeTool.Letter  (schetscontrol, kpea.KeyChar);
											this.Shake(kpea, this);
										   changed = true;
                                       };
			this.KeyPress += (object o, KeyPressEventArgs kpea) => {
											this.Shake(kpea, this);
										};
            this.Controls.Add(schetscontrol);

			this.Closing += Window_Closing;

            menuStrip = new MenuStrip();
            menuStrip.Visible = false;
			sfxPlayer = new SoundPlayer();
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakToolMenu(deTools);
            this.maakAktieMenu();
            this.maakToolButtons(deTools);
            this.maakAktieButtons(deKleuren);
			this.makeRightPanel();
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        }

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (changed == true) {
				if (MessageBox.Show(
									"You have made changes to this painting since last saving it!\n" +
									"Are you sure you want to exit?\n" +
									"Remember there are no mistakes just happy little accidents.", "Warning", MessageBoxButtons.OKCancel)
									== DialogResult.Cancel) 
				{
					e.Cancel = true;
				}
			}
		}

		private void Shake(KeyPressEventArgs kpea, Form form) {
			if (kpea.KeyChar == (char)Keys.Enter) {
				var original = form.Location;
				var rnd = new Random(1337);
				const int shake_amplitude = 10;
				for (int i = 0; i < 10; i++) {
					form.Location = new Point(original.X + rnd.Next(-shake_amplitude, shake_amplitude), original.Y + rnd.Next(-shake_amplitude, shake_amplitude));
					System.Threading.Thread.Sleep(20);
				}
				form.Location = original;

				beatingDevilSfx.Position = 0;
				sfxPlayer.Stream = beatingDevilSfx;
				sfxPlayer.Play();
			}
		}

		private void maakFileMenu()
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
			menu.DropDownItems.Add("Import Painting", null, this.LoadFile);
			menu.DropDownItems.Add("Save Painting", null, this.Save);
			menu.DropDownItems.Add("Close Painting", null, this.Exit);
            menuStrip.Items.Add(menu);
        }
		private void Save(object sender, EventArgs e) {
			SaveFileDialog dialog = new SaveFileDialog();
			//Jif is not a type it's a 'joke'
			dialog.Filter = "Bob Ross Simulator Format|*.brs|PNG image|*.png|JPG/JPEG image|*.jpeg|Bitmap image|*.BMP|Jif image|*.gif|Tiff image|*.tiff|Icon File|*.ico|Windows Meta File|*.wmf";
			if (dialog.ShowDialog() == DialogResult.OK) {
				//Haal extensie uit de bestandnaam zodat we weten als welk type bestand er opgeslagen zal moeten worden
				var extension = Path.GetExtension(dialog.FileName);
				Bitmap bmp = schetscontrol.Schets.bitmap;
				switch (extension.ToLower()) {
					//Save both versions of the JPEG extension the same way
					case ".jpg":
					case ".jpeg":
						bmp.Save(dialog.FileName, ImageFormat.Jpeg);
						break;
					case ".png":
						bmp.Save(dialog.FileName, ImageFormat.Png);
						break;
					case ".bmp":
						bmp.Save(dialog.FileName, ImageFormat.Bmp);
						break;
					case ".gif":
						bmp.Save(dialog.FileName, ImageFormat.Gif);
						break;
					case ".tiff":
						bmp.Save(dialog.FileName, ImageFormat.Tiff);
						break;
					case ".ico":
						bmp.Save(dialog.FileName, ImageFormat.Icon);
						break;
					case ".wmf":
						bmp.Save(dialog.FileName, ImageFormat.Wmf);
						break;
					case ".brs":
						SaveBRSFile(dialog.FileName, schetscontrol.drawnShapes);
						break;
				}
				changed = false;
			}
		}
		public void SaveBRSFile(string fileName, List<IDrawnShape> drawnShapes) {
			string text = "▲ " + schetscontrol.Schets.bitmap.Width + " " + schetscontrol.Schets.bitmap.Height + "\n";
			foreach (IDrawnShape drawnShape in drawnShapes){
				text += drawnShape;
			}
			File.WriteAllText(fileName, text);
		}
		public void LoadFile(object sender, EventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Bob Ros Simulator Format|*.brs|Image file|*.png;*.jpg;*.jpeg;*.gif;*.ico;*.bmp;*.tiff;*.wmf|PNG image|*.png|JPG/JPEG image|*.jpeg|Bitmap image|*.BMP|Jif image|*.gif|Tiff image|*.tiff|Icon File|*.ico";
			if (dialog.ShowDialog() == DialogResult.OK) {
				if (Path.GetExtension(dialog.FileName).ToLower() == ".brs") {
					string[] text = File.ReadAllLines(dialog.FileName);
					schetscontrol.SetDrawnShapes(DecodeBRSText(text));
					schetscontrol.drawShapes();
				} else {
					//Read image file to buffer
					schetscontrol.Schoon(this, null);
					schetscontrol.Schets.bitmap = (Bitmap)Image.FromFile(dialog.FileName);
					//Padding for controls (+70)
					Size sz = new Size(schetscontrol.Schets.bitmap.Width + 70 + rightPanel.Size.Width, schetscontrol.Schets.bitmap.Height + 50);
					if (sz.Width < minSize.Width) {
						sz.Width = minSize.Width;
					}
					if (sz.Height < minSize.Height) {
						sz.Height = minSize.Height;
					}
					this.veranderAfmeting(sz);
					schetscontrol.Invalidate();
				}
				changed = false;
			}
		}
		private List<IDrawnShape> DecodeBRSText(string[] text) {
			List<IDrawnShape> drawnShapes = new List<IDrawnShape>();
			try {
				foreach (string dataPoint in text) {
					//Get individual arguments from line
					string[] arguments = dataPoint.Split(' ');
					//2nd and 3rd arguments are always cords of the startPoint
					Point startPoint = new Point(int.Parse(arguments[1]), int.Parse(arguments[2]));
					if (arguments[0] == "SchetsEditor.DrawnLetter") {
						//Letter only has startpoint
						Color col = Color.FromArgb(int.Parse(arguments[3]), int.Parse(arguments[4]), int.Parse(arguments[5]), int.Parse(arguments[6]));
						char letter = arguments[7].ToCharArray()[0];
						drawnShapes.Add(new DrawnLetter(startPoint, col, letter));
					} else if (arguments[0] == "▲") {
						Size sz = new Size(int.Parse(arguments[1]), int.Parse(arguments[2]));
						schetscontrol.veranderAfmeting(sz);
						this.changeSizeFromSketchSize(sz);
					} else {
						//Other options also have a end point
						Point endPoint = new Point(int.Parse(arguments[3]), int.Parse(arguments[4]));
						Color col = Color.FromArgb(int.Parse(arguments[5]), int.Parse(arguments[6]), int.Parse(arguments[7]), int.Parse(arguments[8]));
						switch (arguments[0]) {
							case "SchetsEditor.DrawnLine":
								drawnShapes.Add(new DrawnLine(startPoint, endPoint, int.Parse(arguments[9]), col));
								break;
							case "SchetsEditor.DrawnRectangle":
								drawnShapes.Add(new DrawnRectangle(startPoint, endPoint, int.Parse(arguments[9]), col));
								break;
							case "SchetsEditor.DrawnFilledRectangle":
								drawnShapes.Add(new DrawnFilledRectangle(startPoint, endPoint, col));
								break;
							case "SchetsEditor.DrawnEllipse":
								drawnShapes.Add(new DrawnEllipse(startPoint, endPoint, int.Parse(arguments[9]), col));
								break;
							case "SchetsEditor.DrawnFilledEllipse":
								drawnShapes.Add(new DrawnFilledEllipse(startPoint, endPoint, col));
								break;
							default:
								MessageBox.Show("Tried to load invalid shape type (" + arguments[0] + ")");
								break;
						}
					}
				}
				return drawnShapes;
			} catch {
				MessageBox.Show("This File is Corrupted or not in Bob Ross Format!");
				return drawnShapes;
			}
		}

		private void import(object sender, EventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "PNG image|*.png|JPG/JPEG image|*.jpeg|Bitmap image|*.BMP|Jif image|*.gif|Tiff image|*.tiff";
			if (dialog.ShowDialog() == DialogResult.OK) {
				//Read image file to buffer
				//TODO check if file is actually an image!
				schetscontrol.Schets.bitmap = (Bitmap)Image.FromFile(dialog.FileName);
				schetscontrol.Invalidate();
			}
		}


		private void maakToolMenu(ICollection<ISchetsTool> tools)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
            foreach (ISchetsTool tool in tools)
            {   ToolStripItem item = new ToolStripMenuItem();
                item.Tag = tool;
                item.Text = tool.ToString();
                item.Image = (Image)resourcemanager.GetObject(tool.ToString());
                item.Click += this.klikToolMenu;
                menu.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menu);
        }

        private void maakAktieMenu()
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Aktie");
            menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
            menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
            menuStrip.Items.Add(menu);
        }

        private void maakToolButtons(ICollection<ISchetsTool> tools)
        {
            int t = 0;
            foreach (ISchetsTool tool in tools)
            {
                RadioButton b = new RadioButton();
                b.Appearance = Appearance.Button;
                b.Size = new Size(45, 62);
                b.Location = new Point(10, 10 + t * 62);
                b.Tag = tool;
                b.Text = tool.ToString();
				b.Image = (Image)resourcemanager.GetObject(tool.ToString());
				b.TextAlign = ContentAlignment.TopCenter;
                b.ImageAlign = ContentAlignment.BottomCenter;
                b.Click += this.klikToolButton;
                this.Controls.Add(b);
                if (t == 0) b.Select();
                t++;
            }
        }

		private void makeRightPanel() {
			rightPanel = new Panel();
			rightPanel.Size = new Size(210, 600);

			this.Controls.Add(rightPanel);

			Label l = new Label();
			l.Location = new Point(10, 220);
			l.Width = 100;
			l.Name = "TypeDescriptorLabel";
			rightPanel.Controls.Add(l);

			l = new Label();
			l.Location = new Point(120, 220);
			l.Name = "alphaLabel";
			rightPanel.Controls.Add(l);

			//Alpha
			TrackBar tb;
			tb = new TrackBar(); 
			tb.Location = new Point(10, 235);
			tb.Maximum = 255;
			tb.Width = 200;
			tb.Value = 255;
			tb.Name = "AlphaTrackBar";
			tb.TickFrequency = 26;
			tb.MouseUp += schetscontrol.ChangeColorAlpha;
			tb.MouseMove += this.updateLabels;
			tb.MouseUp += this.updateLabels;
			rightPanel.Controls.Add(tb);
			tb.BringToFront();

			//Hue
			tb = new TrackBar();
			tb.Location = new Point(10, 270);
			tb.Maximum = 360;
			tb.Width = 200;
			tb.Value = 0;
			tb.Name = "HueTrackBar";
			tb.TickFrequency = 26;
			tb.MouseUp += schetscontrol.ChangeColorHue;
			tb.MouseMove += this.UpdateWheelColorsFromTrackBars;
			tb.MouseMove += this.updateLabels;
			tb.MouseUp += this.updateLabels;
			rightPanel.Controls.Add(tb);
			tb.BringToFront();
			//Saturation
			tb = new TrackBar();
			tb.Location = new Point(10, 305);
			tb.Maximum = 100;
			tb.Width = 200;
			tb.Value = 100;
			tb.Name = "SatTrackBar";
			tb.TickFrequency = 26;
			tb.MouseUp += schetscontrol.ChangeColorSaturation;
			tb.MouseMove += this.UpdateWheelColorsFromTrackBars;
			tb.MouseMove += this.updateLabels;
			tb.MouseUp += this.updateLabels;
			rightPanel.Controls.Add(tb);
			tb.BringToFront();
			//Value
			tb = new TrackBar();
			tb.Location = new Point(10, 340);
			tb.Maximum = 100;
			tb.Width = 200;
			tb.Value = 100;
			tb.Name = "ValueTrackBar";
			tb.TickFrequency = 26;
			tb.MouseUp += schetscontrol.ChangeColorVal;
			tb.MouseMove += this.UpdateWheelColorsFromTrackBars;
			tb.MouseMove += this.updateColorWheelVal;
			tb.MouseUp += this.updateColorWheelVal;
			tb.MouseMove += this.updateLabels;
			tb.MouseUp += this.updateLabels;
			rightPanel.Controls.Add(tb);
			tb.BringToFront();

			//Brush Width
			tb = new TrackBar();
			tb.Location = new Point(10, 375);
			tb.Maximum = 20;
			tb.Width = 200;
			tb.Value = 3;
			tb.TickFrequency = 2;
			tb.Name = "BrushWidthTrackBar";
			tb.MouseUp += schetscontrol.ChangeBrushWidth;
			tb.MouseMove += this.updateLabels;
			tb.MouseUp += this.updateLabels;
			rightPanel.Controls.Add(tb);
			tb.BringToFront();

			//Color wheel
			ColorPicker wheelControl = new ColorPicker(new Size(200,200));
			wheelControl.Location = new Point(0, 10);
			wheelControl.Name = "colorPicker";
			rightPanel.Controls.Add(wheelControl);
			wheelControl.MouseDown += this.WheelCapture;
			wheelControl.MouseDown += this.WheelUpdateColor;
			wheelControl.MouseUp += this.WheelUpdateColor;
			wheelControl.MouseUp += this.WheelUncapture;
			wheelControl.MouseMove += this.WheelUpdateColor;
		}

		private void updateColorWheelVal(object sender, EventArgs ea) {
			double val = ((TrackBar)sender).Value / 100.0;
			ColorPicker wheelControl = this.Controls.Find("colorPicker", true).FirstOrDefault() as ColorPicker;
			wheelControl.SetValue(val);
		}
		private void WheelCapture(object sender, EventArgs mea) {
			ColorPicker cp = (ColorPicker)sender;
			cp.Capture = true;
		}
		private void WheelUncapture(object sender, EventArgs mea) {
			ColorPicker cp = (ColorPicker)sender;
			cp.Capture = false;
		}
		private void WheelUpdateColor(object sender, MouseEventArgs mea) {
			ColorPicker cp = (ColorPicker)sender;
			ColorConverter cc = new ColorConverter();
			if (cp.Capture) { //Is mouse being held?
				Color col = cp.getWheelPosColor(mea.X, mea.Y);
				if (col != Color.FromArgb(0, 0, 0, 0)) {
					schetscontrol.ChangeColor(col);
					cp.SetColor(col);

					double[] hsv = cc.Color2HSV(col);
					TrackBar hueTB = this.Controls.Find("HueTrackBar", true).FirstOrDefault() as TrackBar;
					TrackBar satTB = this.Controls.Find("SatTrackBar", true).FirstOrDefault() as TrackBar;
					hueTB.Value = (int)hsv[0];
					satTB.Value = (int)(hsv[1] * 100);
				}
			}
		}
		private void UpdateWheelColorsFromTrackBars(object sender, MouseEventArgs mea) {
			if (mea.Button == MouseButtons.Left) {
				ColorPicker cp = this.Controls.Find("colorPicker", true).FirstOrDefault() as ColorPicker;
				TrackBar hueTB = this.Controls.Find("HueTrackBar", true).FirstOrDefault() as TrackBar;
				TrackBar satTB = this.Controls.Find("SatTrackBar", true).FirstOrDefault() as TrackBar;
				TrackBar valTB = this.Controls.Find("ValueTrackBar", true).FirstOrDefault() as TrackBar;
				ColorConverter cc = new ColorConverter();
				Color col = cc.HSV2Color(hueTB.Value, satTB.Value / 100.0, valTB.Value / 100.0);
				cp.SetColor(col);
				cp.SetValue(valTB.Value / 100.0);
			}
		}
		//When you choose one of the default colors this function is called to make sure the correct spot on the
		//Color wheel for that color is highlighted!
		private void UpdateWheelColorFromDefaults(object sender, EventArgs ea) {
			PalletColor pc = (PalletColor)((RadioButton)sender).Tag; ;
			ColorPicker cp = this.Controls.Find("colorPicker", true).FirstOrDefault() as ColorPicker;
			cp.SetColor(pc.color);
			ColorConverter cc = new ColorConverter();
			double[] hsv = cc.Color2HSV(pc.color);
			cp.SetValue(hsv[2]);

			TrackBar tb = this.Controls.Find("ValueTrackBar", true).FirstOrDefault() as TrackBar;
			tb.Value = (int)Math.Round(hsv[2] * 100);
			tb.Invalidate();

			tb = this.Controls.Find("SatTrackBar", true).FirstOrDefault() as TrackBar;
			tb.Value = (int)Math.Round(hsv[1] * 100);
			tb.Invalidate();

			tb = this.Controls.Find("HueTrackBar", true).FirstOrDefault() as TrackBar;
			tb.Value = (int)Math.Round(hsv[0]);
			tb.Invalidate();
		}

		private void updateLabels(object o, MouseEventArgs mea) {
			Label lb = this.Controls.Find("alphaLabel", true).FirstOrDefault() as Label;
			lb.Text = "(" + ((TrackBar)o).Value + ")";
			lb = this.Controls.Find("TypeDescriptorLabel", true).FirstOrDefault() as Label;
			string typeName = "";
			switch (((TrackBar)o).Name) {
				case "HueTrackBar":
					typeName = "Hue";
					break;
				case "ValueTrackBar":
					typeName = "Value";
					break;
				case "SatTrackBar":
					typeName = "Saturation";
					break;
				case "BrushWidthTrackBar":
					typeName = "Brush Thiccness"; //Intentional misspelling!
					break;
				case "AlphaTrackBar":
					typeName = "Alpha";
					break;
			}
			lb.Text = typeName+":";
		}

		private void InitializeComponent() {
			this.SuspendLayout();
			// 
			// SchetsWin
			// 
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Name = "SchetsWin";
			this.ResumeLayout(false);

		}

		private void maakAktieButtons(PalletColor[] kleuren)
        {
			bottomPanel = new Panel();
			bottomPanel.Size = new Size(650, 24);
            this.Controls.Add(bottomPanel);
            
            Button b; Label l; ComboBox cbb;
            b = new Button(); 
            b.Text = "Clear";  
            b.Location = new Point(  0, 0); 
            b.Click += schetscontrol.Schoon;
			bottomPanel.Controls.Add(b);
            
            b = new Button(); 
            b.Text = "Rotate"; 
            b.Location = new Point( 80, 0); 
            b.Click += schetscontrol.Roteer;
			bottomPanel.Controls.Add(b);

			b = new Button();
			b.Text = "Undo";
			b.Name = "UndoButton";
			b.Enabled = false;
			b.Location = new Point(160, 0);
			b.Click += schetscontrol.Undo;
			bottomPanel.Controls.Add(b);

			l = new Label();  
            l.Text = "Penkleur:"; 
            l.Location = new Point(260, 3); 
            l.AutoSize = true;
			bottomPanel.Controls.Add(l);

			int t = 0;
			foreach (PalletColor kleur in kleuren) {
				RadioButton br = new RadioButton();
				br.Appearance = Appearance.Button;
				br.Size = new Size(20, 20);
				br.Location = new Point(320 + t*24, 0);
				br.Tag = kleur;
				br.BackColor = kleur.color;
				br.Click += schetscontrol.ChangeColor;
				br.Click += this.UpdateWheelColorFromDefaults;
				bottomPanel.Controls.Add(br);
				if (t == 0) b.Select();
				t++;
			}
        }
    }
}
