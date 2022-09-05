using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aplicativo
{
    public partial class Form4 : Form
    {
        //Tirar o X-Button
        private const int CP_NOCLOSE_BUTTON = 0x200;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        //Variáveis
        public string icone = "";
        
        public Form4()
        {
            InitializeComponent();
            button1.Enabled = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Ativar botão
            button1.Enabled = true;

            switch (listBox1.SelectedIndex)
            {
                case 0:
                    pictureBox1.Image = Properties.Resources.black_head_horse_side_view_with_horsehair;
                    icone = "icon1";
                    break;
                case 1:
                    pictureBox1.Image = Properties.Resources.gull_bird_flying_shape;
                    icone = "icon2";
                    break;
                case 2:
                    pictureBox1.Image = Properties.Resources.dog;
                    icone = "icon3";
                    break;
                case 3:
                    pictureBox1.Image = Properties.Resources.rabbit_shape;
                    icone = "icon4";
                    break;
                case 4:
                    pictureBox1.Image = Properties.Resources.deer_silhouette;
                    icone = "icon5";
                    break;
                case 5:
                    pictureBox1.Image = Properties.Resources.gorilla_facing_right;
                    icone = "icon6";
                    break;
                case 6:
                    pictureBox1.Image = Properties.Resources.icon;
                    icone = "icon7";
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.loginForm.Send("avatar/"+ icone + "/" + label1.Text);
            Program.loginForm.Send("autenticar/" + label1.Text); // Autenticar SERVER-FOTO
            this.Visible = false;
        }
    }
}
