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
    public partial class Form3 : Form
    {
        //Instanciar Form's
        private Form4 avatarForm = new Form4();

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

        //Diretório
        private string Path = Environment.CurrentDirectory + @"\Lembrar.ini";

        //Autenticar dados (AUTH)
        private void Auth()
        {
            Program.loginForm.Send("autenticar/" + label9.Text);
        }

        public Form3()
        {
            //Exit
            InitializeComponent();
            //Valor
            List<TextBox> tList = new List<TextBox>();
            List<string> sList = new List<string>();
            tList.Add(textBox2);
            sList.Add("Ex: 900");
            SetCueBanner(ref tList, sList);
            //Nome [PM]
            List<TextBox> dList = new List<TextBox>();
            List<string> fList = new List<string>();
            dList.Add(textBox5);
            fList.Add("Nome do usuário");
            SetCueBanner(ref dList, fList);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = listBox1.Text;
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void AdicionarOnline(string nome, string tipo)
        {
            int index = listBox1.FindString(nome);

            if(tipo == "add")
            {
                if(index != -1)
                {
                    //listBox1.SetSelected(index, true);
                }
                else
                {
                    listBox1.Items.Add(nome);
                    Verificar();
                }
            }
            else
            {
                listBox1.Items.Remove(nome);
            }
        }

        public void Autenticar(string dinheiro, string nome, string transacoes, string total_de_transacoes, string tipo_de_conta, string icone)
        {
            /*
             label2 - Dinheiro
             label9 - Nome
             listbox2 - suas transações
             label12 - total de transações
             label10 - tipo de conta
             PictureBox1 - ICONE
            */
            //AppendListBox2
            string[] suas_transacoes = transacoes.Split(',');

            label2.Text = dinheiro;
            if(nome == "")
            {
                //NADA -:> Recebe só 1 e para
            }
            else
            {
                label9.Text = nome;
            }
            for(int a = 0; a < suas_transacoes.Length; a++)
            {
                if (!listBox2.Items.Contains(suas_transacoes[a]))
                {//Não repetir items
                    if(suas_transacoes[a] != "")
                    {
                        listBox2.Items.Add(suas_transacoes[a]);
                    }
                }
            }
            if (icone == "icon1")
            {//Cavalo
                pictureBox1.Image = Properties.Resources.black_head_horse_side_view_with_horsehair;
            }
            else if (icone == "icon2")
            {//Gaivota
                pictureBox1.Image = Properties.Resources.gull_bird_flying_shape;
            }
            else if (icone == "icon3")
            {//Cachorro
                pictureBox1.Image = Properties.Resources.dog;
            }
            else if (icone == "icon4")
            {//Coelho
                pictureBox1.Image = Properties.Resources.rabbit_shape;
            }
            else if (icone == "icon5")
            {//Veado
                pictureBox1.Image = Properties.Resources.deer_silhouette;
            }
            else if (icone == "icon6")
            {//Gorila
                pictureBox1.Image = Properties.Resources.gorilla_facing_right;
            }
            else if (icone == "icon7")
            {//Urso
                pictureBox1.Image = Properties.Resources.icon;
            }
            label12.Text = total_de_transacoes;
            label10.Text = tipo_de_conta;
        }

        public void EscreverText(string mensagem)
        {
            //Tratamento 2 - PROBLEM
            string[] conectados = mensagem.Split(' ');
            string[] chat = mensagem.Split('/');

            //Escrever no painel
            textBox3.AppendText(mensagem+"\n");

            //Chat
            if (chat[0] == "CHAT")
            {
                richTextBox1.AppendText(chat[1]+": "+chat[2]+"\n");
            }
            else
            {
                //Autenticar dados
                if (chat[0] == "Autenticar")
                {
                    // 1 - dinheiro
                    // 2 - nome
                    // 3 - transações
                    // 4 - total de transações
                    // 5 - tipo de conta
                    // 6 - icone

                    Autenticar(chat[1], chat[2], chat[3], chat[4], chat[5], chat[6]);
                }
                else
                {
                    //Mensagem privada
                    if (chat[0] == "privado")
                    {
                        richTextBox1.AppendText("[PM]"+chat[1]+": "+chat[2]+"\n");
                    }
                    else
                    {
                        if(chat[0] == "transferir")
                        {
                            if(chat[1] == "1")
                            {
                                toolStripStatusLabel1.Text = "Usuário não existe";
                                toolStripStatusLabel1.ForeColor = Color.Red;
                            }
                            else
                            {
                                toolStripStatusLabel1.Text = "Transfência bem-sucedida";
                                toolStripStatusLabel1.ForeColor = Color.Green;
                            }
                        }
                        else
                        {
                            if (chat[0] == "vertr")
                            {
                                pictureBox2.Image = Properties.Resources.arrow_right_15604;
                                label13.Text = "[ID]" + listBox2.Text;
                                label15.Text = chat[1];
                                label16.Text = chat[2];
                                label17.Text = "-" + chat[3];
                                label18.Text = "+" + chat[3];
                                if (chat[4] != "Sem registro")
                                {
                                    label14.Text = "Transação realizada em "+chat[4] + "/" + chat[5] + "/" + chat[6].Split(' ')[0] + " as " + chat[6].Split(' ')[1];
                                }
                                else
                                {
                                    label14.Text = chat[4];
                                }
                            }
                            else
                            {
                                //Percorrer index
                                for (int a = 0; a < conectados.Length; a++)
                                {
                                    if (conectados[a] == "saiu")
                                    {
                                        AdicionarOnline(conectados[1] + " " + conectados[2], "saiu");
                                    }
                                    else
                                    {
                                        AdicionarOnline(conectados[1] + " " + conectados[2], "add");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //CHECKED
            string[] testar = Convert.ToString(DateTime.Now).Split(' ');
            string data = Convert.ToString(testar[0].Replace('/', ','));

            //TRANSFERENCIA
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                toolStripStatusLabel1.Text = "Informe o nome e o valor para transferência";
                toolStripStatusLabel1.ForeColor = Color.Black;
            }
            else
            {
                if(checkBox1.Checked == true)
                {
                    Program.loginForm.Send("transferir/"+label9.Text+"/"+textBox1.Text+"/"+textBox2.Text+"/"+ data + " " + testar[1]);  
                }
                else
                {
                    Program.loginForm.Send("transferir/" + label9.Text + "/" + textBox1.Text + "/" + textBox2.Text + "/Sem registro");
                }
            }
        }

        void Sair()
        {
            Program.loginForm.FecharConexao();
            /*for (int intIndex = Application.OpenForms.Count - 1; intIndex >= 0; intIndex--)
            {
                if (Application.OpenForms[intIndex] != this)
                    Application.OpenForms[intIndex].Close();
            }*/
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Sair();
        }

        private void menuToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void sairToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Sair();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndex = 0;
        }

        private void Verificar()
        {
            Program.loginForm.Send("conectados");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            avatarForm.label1.Text = label9.Text;

            if(avatarForm.Visible == false)
            {
                avatarForm.Visible = true;
            }
            else
            {
                avatarForm.Visible = false;
            }
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                this.button4.PerformClick();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool existe = listBox1.Items.Contains(textBox5.Text);

            if (textBox4.Text == "")
            {
                toolStripStatusLabel1.Text = "Você precisa escrever algo";
                toolStripStatusLabel1.ForeColor = Color.Red;
            }
            else
            {
                if (comboBox1.SelectedIndex == 0)
                {
                    Program.loginForm.Send("chat/" + textBox4.Text);
                    richTextBox1.ScrollToCaret();
                    textBox4.Text = "";
                    toolStripStatusLabel1.Text = "Painel do banco";
                    toolStripStatusLabel1.ForeColor = Color.Black;
                }
                else
                {
                    if (textBox5.Text == label9.Text)
                    {
                        toolStripStatusLabel1.Text = "Você não pode se enviar um PM";
                        toolStripStatusLabel1.ForeColor = Color.Black;
                    }
                    else
                    {
                        if (!existe)
                        {
                            toolStripStatusLabel1.Text = "Usuário não está conectado";
                            toolStripStatusLabel1.ForeColor = Color.Red;
                        }
                        else
                        {
                            richTextBox1.AppendText("[PM]Você(Para:" + textBox5.Text + ")" + ": " + textBox4.Text + "\n");
                            Program.loginForm.Send("privado" + "/" + label9.Text + "/" + textBox5.Text + "/" + textBox4.Text);
                            richTextBox1.ScrollToCaret();
                            textBox4.Text = "";
                        }
                    }
                }
            }
            
        }

        private void Form3_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsLetter(e.KeyChar) || Char.IsWhiteSpace(e.KeyChar) || Char.IsNumber(e.KeyChar) || Char.IsSymbol(e.KeyChar) || Char.IsPunctuation(e.KeyChar))
                e.Handled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1)
            {
                //Privado
                toolStripStatusLabel1.Text = "Informe o nome do usuário e manda a mensagem";
                toolStripStatusLabel1.ForeColor = Color.Black;
                textBox5.Visible = true;
            }
            else
            {
                //Global
                textBox5.Text = "";
                textBox5.Visible = false;
                toolStripStatusLabel1.Text = "Painel do banco";
                toolStripStatusLabel1.ForeColor = Color.Black;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            label13.Visible = true;
            label16.Visible = true;
            label17.Visible = true;
            label18.Visible = true;
            label14.Visible = true;
            label15.Visible = true;

            Program.loginForm.Send("vertr/"+label9.Text+"/"+listBox2.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
        }

        private void códigoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Novo
            Form5 codigoForm = new Form5();

            //Informar nome
            codigoForm.label1.Text = label9.Text;

            //Mostrar
            if (codigoForm.Visible == false)
            {
                codigoForm.Visible = true;
            }
            else
            {
                codigoForm.Visible = false;
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsLetter(e.KeyChar) || Char.IsWhiteSpace(e.KeyChar) || Char.IsSymbol(e.KeyChar) || Char.IsPunctuation(e.KeyChar))
                e.Handled = true;
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show(Convert.ToString(DateTime.Now));
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.loginForm.FecharConexao();
        }
    }
}