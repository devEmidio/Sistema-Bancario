using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Aplicativo
{
    public partial class Form2 : Form
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

        //Marca d'agua
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr i, string str);

        void SetCueBanner(ref List<TextBox> textBox, List<string> CueText)
        {
            for (int x = 0; x < textBox.Count; x++)
            {
                SendMessage(textBox[x].Handle, 0x1501, (IntPtr)1, CueText[x]);
            }
        }

        public Form2()
        {
            InitializeComponent();
            List<TextBox> tList = new List<TextBox>();
            List<string> sList = new List<string>();
            tList.Add(textBox1);
            sList.Add("Ex: Richardson Emidio");
            SetCueBanner(ref tList, sList);
        }

        public void Status(string mensagem, string cor)
        {
            toolStripStatusLabel1.Text = mensagem;
            if (cor == "Green")
            {
                toolStripStatusLabel1.ForeColor = Color.Green;
            }
            else
            {
                toolStripStatusLabel1.ForeColor = Color.Red;
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripStatusLabel1.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            toolStripStatusLabel1.Text = "";
            comboBox1.SelectedIndex = -1;
            this.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] texto = textBox1.Text.Split(' ');

            if (textBox1.Text.Length > 16)
            {
                Status("Login muito grande", "Red");
            }
            else
            {
                if (textBox1.Text.Length < 2)
                {
                    Status("Login muito pequeno", "Red");
                }
                else
                {
                    if ((texto.Length == 1))
                    {
                        Status("O login precisa 2 palavras", "red");
                    }
                    else
                    {
                        if (textBox2.Text != textBox3.Text)
                        {
                            Status("Senhas não conferem", "red");
                        }
                        else
                        {
                            if (texto.Length - 1 > 1)
                            {
                                Status("O login não pode ter mais que 2 palavras", "red");
                            }
                            else
                            {
                                if (comboBox1.Text == "")
                                {
                                    Status("Escolha o tipo de conta", "red");
                                }
                                else
                                {
                                    Program.loginForm.IniciarConexao(textBox1.Text, textBox2.Text, "cadastrar/" + comboBox1.Text);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                this.button1.PerformClick();
            }
        }
    }
}
