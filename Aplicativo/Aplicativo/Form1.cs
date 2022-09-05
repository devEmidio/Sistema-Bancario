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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Aplicativo
{
    public partial class Form1 : Form
    {
        //Instacia as classes
        private Form2 cadastro = new Form2();
        private Form3 painel = new Form3();

        //Dados da Conexão
        private StreamWriter stwEnviador;
        private StreamReader strReceptor;
        private TcpClient tcpServidor;
        private Thread mensagemThread;
        private IPAddress enderecoIP = IPAddress.Parse("127.0.0.1");
        private bool Conectado = false;

        //Delegate
        private delegate void AttStatus(string mensagem, string cor);
        private delegate void AttIrPainel();
        private delegate void AttEscreverPainel(string mensagem);
        private delegate void AttAdicionarOnline(string user, string tipo);
        private delegate void AttStatusCadastro(string mensagem, string cor);

        //Variables
        public int count = 0;

        //Diretório
        private string Path = Environment.CurrentDirectory + @"\Lembrar.ini";

        //Importar dll
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string re, StringBuilder retval, int size, string filePath);

        //Escrever no arquivo
        public void WriteINI(string Section, string Key, string Value, string path)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }
        
        //Ler o arquivo
        public string ReadIni(string section, string key, string path)
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, String.Empty, sb, 255, path);
            return sb.ToString();
        }

        public Form1()
        {
            //Exit
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
            //Lembrar
            if(ReadIni("Lembrar", "Valor", Path) == "True")
            {
                checkBox1.Checked = true;
                textBox1.Text = ReadIni("Lembrar","Login", Path);
                textBox2.Text = ReadIni("Lembrar","Senha", Path);
            }
        }

        //Fechar programa
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Conectado == true)
            {
                FecharConexao();
            }
        }

        //Fechar Conexão
        public void FecharConexao()
        {
            //Fecha os objetos
            Conectado = false;
            stwEnviador.Close();
            strReceptor.Close();
            tcpServidor.Close();
            mensagemThread.Abort(); //Terminar Thread

            //Exit  
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        //Status - delegate 'AttStatus'
        private void Status(string mensagem, string cor)
        {
            if(cor == "Red")
            {
                toolStripStatusLabel1.Text = mensagem;
                toolStripStatusLabel1.ForeColor = Color.Red;
            }
            else if(cor == "Green")
            {
                toolStripStatusLabel1.Text = mensagem;
                toolStripStatusLabel1.ForeColor = Color.Green;
            }
            else
            {
                toolStripStatusLabel1.Text = mensagem;
            }
        }

        //Iniciar conexão
        public void IniciarConexao(string nome, string senha, string tipo)
        {
            try
            {
                //Iniciar conexão
                tcpServidor = new TcpClient();
                tcpServidor.Connect(enderecoIP, 3333);

                //Definir que estamos conectados
                Conectado = true;

                //Criar StreamWriter e enviar a primeira mensagem
                stwEnviador = new StreamWriter(tcpServidor.GetStream());
                stwEnviador.WriteLine(nome + "/" + senha + "/" + tipo);
                stwEnviador.Flush();

                //Iniciar Thread
                mensagemThread = new Thread(new ThreadStart(ReceberDados));
                mensagemThread.Start();
            }
            catch
            {
                count++;
                if(cadastro.Visible == true)
                {
                    this.Invoke(new AttStatusCadastro(this.StatusCadastro), new object[] { "Offline - Servidor não está ligado... ("+count+")", "Red" });
                }
                else
                {
                    this.Invoke(new AttStatus(this.Status), new object[] { "Offline - Servidor não está ligado... ("+count+")", "Red" });
                }
            }
        }

        //irPainel - delegate 'AttIrPainel'
        private void IrPainel()
        {
            this.Visible = false;
            this.Hide();
            painel.Show();
        }

        //EscreverPainel - delegate 'AttEscreverPainel'
        private void EscreverPainel(string mensagem)
        {
            painel.EscreverText(mensagem);
        }
        
        //AdicionarOnline - delegate 'AttAdicionarOnline'
        private void AdicionarOnline(string user, string tipo)
        {
            painel.AdicionarOnline(user, tipo);
        }
        
        //StatusCadastro - delegate 'AttStatusCadastro'
        private void StatusCadastro(string mensagem, string cor)
        {
            cadastro.Status(mensagem, cor);
        }

        private void ReceberDados()
        {
            strReceptor = new StreamReader(tcpServidor.GetStream());
            string ConResposta = strReceptor.ReadLine();
            Form1 form1 = this;

            //Retornar dinheiro
            string[] dinheiro = ConResposta.Split('/');

            if(ConResposta[0] == '0')
            {
                //Conta não existe -:> 0
                form1.Invoke(new AttStatus(form1.Status), new object[] { "Conta não existe", "d" });
                FecharConexao();
            }else if(ConResposta[0] == '2')
            {
                //Acesso negado -:> 2
                form1.Invoke(new AttStatus(form1.Status), new object[] { "Esta conta não tem mais acesso", "Red" });
                FecharConexao();
            }else if(ConResposta[0] == '3')
            {
                //Senha errada -:> 3
                form1.Invoke(new AttStatus(form1.Status), new object[] { "Senha incorreta", "Red" });
                FecharConexao();
            }
            else if(ConResposta[0] == '4')
            {
                //Usuário conectado -:> 4
                form1.Invoke(new AttStatus(form1.Status), new object[] { "Usuário conectado", "d" });
                FecharConexao();
            }
            else if(ConResposta[0] == '5')
            {
                //Conta já existe 'Cadastro' -:> 5
                form1.Invoke(new AttStatusCadastro(form1.StatusCadastro), new object[] { "Conta já existe", "red" });
                FecharConexao();
            }
            else if(ConResposta[0] == '6')
            {
                //Conta cadastrada 'Cadastro' -:> 6
                form1.Invoke(new AttStatusCadastro(form1.StatusCadastro), new object[] { "Cadastrado com sucesso", "Green" });
                FecharConexao();
            }
            else
            {
                //Lembrar.ini
                if(checkBox1.Checked == false)
                {
                    WriteINI("Lembrar", "Valor", "False", Path);
                }
                else
                {
                    WriteINI("Lembrar", "Login", textBox1.Text, Path);
                    WriteINI("Lembrar", "Senha", textBox2.Text, Path);
                    WriteINI("Lembrar", "Valor", "True", Path);
                }

                //Login realizado
                form1.Invoke(new AttIrPainel(form1.IrPainel));
            }
            
            //Packet's
            while (Conectado)
            {
                try
                {
                    form1.Invoke(new AttEscreverPainel(form1.EscreverPainel), new object[] { form1.strReceptor.ReadLine() });
                }catch (IOException i)
                {
                    //MessageBox.Show(Convert.ToString(i));
                }
                catch (Exception e)
                {
                    //MessageBox.Show(Convert.ToString(e));
                }
                
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(cadastro.Visible == false)
            {
                cadastro.Visible = true;
            }
            else
            {
                cadastro.Visible = false;
            }
        }

        //Enviar pacote para o servidor
        public void Send(string mensagem)
        {
            if(Conectado == true)
            {
                stwEnviador.WriteLine(mensagem);
                stwEnviador.Flush();
            }
        }

        //Entrar
        private void Entrar()
        {
            string[] teste = textBox1.Text.Split(' ');

            if (Conectado == false)
            {
                if (teste.Length == 1)
                {
                    Status("O login precisa ter duas palavras", "Red");
                }
                else
                {
                    IniciarConexao(textBox1.Text, textBox2.Text, "entrar");
                }
               
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Entrar();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void button1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Entrar();
            }
        }

        private void textBox1_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Entrar();
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }
    }
}
