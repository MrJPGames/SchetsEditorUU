using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Drawing.Imaging;

namespace SchetsEditor
{
    public class Hoofdscherm : Form
    {
        MenuStrip menuStrip;

		SoundPlayer sfxPlayer;
		Stream IntroSFX = Properties.Resources.Intro;

		public Hoofdscherm(){
			sfxPlayer = new SoundPlayer();
			this.ClientSize = new Size(800, 600);
            menuStrip = new MenuStrip();
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakHelpMenu();
            this.Text = "Bob Ross Simulator 2017";
            this.IsMdiContainer = true;
			this.MainMenuStrip = menuStrip;
			sfxPlayer.Stream = IntroSFX;
			sfxPlayer.Play();
        }
        private void maakFileMenu(){
			ToolStripDropDownItem menu = new ToolStripMenuItem("File");
            menu.DropDownItems.Add("Start Painting", null, this.nieuw);
			menu.DropDownItems.Add("Open Painting", null, this.load);
			menu.DropDownItems.Add("Exit Simulation", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }
        private void maakHelpMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("Help");
            menu.DropDownItems.Add("Over \"Bob Ross Simulator 2017\"", null, this.about);
            menuStrip.Items.Add(menu);
        }
        private void about(object o, EventArgs ea)
        {   MessageBox.Show("Bob Ross Simulator 2017 v1.0\n(c) UU Informatica 2010, Jasper peters 2017"
                           , "Over \"Bob Ross Simulator 2017\""
                           , MessageBoxButtons.OK
                           , MessageBoxIcon.Information
                           );
        }

        private void nieuw(object sender, EventArgs e){
			SchetsWin s = new SchetsWin();
            s.MdiParent = this;
            s.Show();
        }
		private void load(object sender, EventArgs e) {
			SchetsWin s = new SchetsWin();
			s.MdiParent = this;
			s.Show();
			s.LoadFile(sender, e);
		}
		private void afsluiten(object sender, EventArgs e)
        {   this.Close();
        }
    }
}
