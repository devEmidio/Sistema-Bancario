using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;

namespace ChatServidor
{ 
    // Trata os argumentos para o evento StatusChanged
    public class StatusChangedEventArgs : EventArgs
    {
        // Estamos interessados na mensagem descrevendo o evento
        private string EventMsg;

        // Propriedade para retornar e definir um mensagem do evento
        public string EventMessage
        {
            get { return EventMsg;}
            set { EventMsg = value;}
        }

        // Construtor para definir a mensagem do evento
        public StatusChangedEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }

    // Este delegate é necessário para especificar os parametros que estamos pasando com o nosso evento
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    class ChatServidor
    {
        // Esta hash table armazena os usuários e as conexões (acessado/consultado por usuário)
        public static Hashtable htUsuarios = new Hashtable(30); // 30 usuarios é o limite definido
        // Esta hash table armazena os usuários e as conexões (acessada/consultada por conexão)
        public static Hashtable htConexoes = new Hashtable(30); // 30 usuários é o limite definido
        // armazena o endereço IP passado
        private IPAddress enderecoIP;
        private TcpClient tcpCliente;
        // O evento e o seu argumento irá notificar o formulário quando um usuário se conecta, desconecta, envia uma mensagem,etc
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        // O construtor define o endereço IP para aquele retornado pela instanciação do objeto
        public ChatServidor(IPAddress endereco)
        {
            enderecoIP = endereco;
        }

        // A thread que ira tratar o escutador de conexões
        private Thread thrListener;

        // O objeto TCP object que escuta as conexões
        private TcpListener tlsCliente;

        // Ira dizer ao laço while para manter a monitoração das conexões
        bool ServRodando = false;


        // Inclui o usuário nas tabelas hash
        public static void IncluiUsuario(TcpClient tcpUsuario, string strUsername)
        {
            //Separar o usuário da senha
            string[] texto = strUsername.Split('/');


            // Primeiro inclui o nome e conexão associada para ambas as hash tables
            ChatServidor.htUsuarios.Add(texto[0], tcpUsuario);
            ChatServidor.htConexoes.Add(tcpUsuario, texto[0]);

            e = new StatusChangedEventArgs(Convert.ToString(htUsuarios[texto[0]]));
            OnStatusChanged(e);

            // Informa a nova conexão para todos os usuário e para o formulário do servidor
            EnviaMensagemAdmin(htConexoes[tcpUsuario] + " entrou");
        }

        // Remove o usuário das tabelas (hash tables)
        public static void RemoveUsuario(TcpClient tcpUsuario)
        {
            // Se o usuário existir
            if (htConexoes[tcpUsuario] != null)
            {
                // Primeiro mostra a informação e informa os outros usuários sobre a conexão
                EnviaMensagemAdmin(htConexoes[tcpUsuario] + " saiu");

                // Removeo usuário da hash table
                ChatServidor.htUsuarios.Remove(ChatServidor.htConexoes[tcpUsuario]);
                ChatServidor.htConexoes.Remove(tcpUsuario);
            }
        }

        // Este evento é chamado quando queremos disparar o evento StatusChanged
        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;
            if (statusHandler != null)
            {
                // invoca o  delegate
                statusHandler(null, e);
            }
        }

        //Buscar
        public static string BuscaArquivos(DirectoryInfo dir)
        {
            int count = 0;
            string teste = "";

            // lista arquivos do diretorio corrente
            foreach (FileInfo file in dir.GetFiles())
            {
                count++;
                teste += file.Name + "/";
            }

            string[] array = teste.Split('/');

            return array[count-1].Replace(".ini", "");
        }

        //Transferir Dinheiro
        public static void Transferir(string nome, string nome2, string quantia, string data)
        {
            /*
             * Para o cliente
             *  1 - usuário não existe
             *  2 - feito
             =================================
             0 - transferir
             1 - Nome de quem ta transferindo
             2 - Nome de quem ta recebendo
             3 - Quantia
             4 - Data (S/N)
             5 - Tipo
             */
            //Quebrar
            DirectoryInfo dirInfo = new DirectoryInfo(@".\TR\ ");
            string[] teste_trs1 = Conexao.ReadIni("Conta", "TR", @".\Contas\" + nome + ".ini").Split(',');
            string[] teste_trs2 = Conexao.ReadIni("Conta", "TR", @".\Contas\" + nome2 + ".ini").Split(',');
            string[] teste_data = data.Split(' '); //0 - data | 1 - horário
            string dinheiro = "";
            string salvar_cod = "";
            string salvar_cod2 = "";
            string arquivo = @".\Contas\" + nome2 + ".ini";
            int novatr = 0;
            int novodinheiro = 0;

            //SENDER-SENDER PROBLEM
            StreamWriter swSenderSender;

            // Cria um array de clientes TCPs do tamanho do numero de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
            // Copia os objetos TcpClient no array
            ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);

            // Exibe primeiro na aplicação
            e = new StatusChangedEventArgs("Transferência >> " + data);
            OnStatusChanged(e);

            //Responder ao usuário que requisitou
            ICollection MyKeys;
            int count = 0;
            MyKeys = htUsuarios.Keys;
            
            foreach (object Key in MyKeys)
            {
                count++;
                e = new StatusChangedEventArgs(Convert.ToString(count - 1) + Key.ToString());
                OnStatusChanged(e);
                if (Key.ToString() == nome)
                {
                    break;
                }
            }

            if (File.Exists(arquivo))
            {
                novatr = Convert.ToInt32(BuscaArquivos(dirInfo)) + 1;

                //Diminuir de quem ta enviando
                dinheiro = Conexao.ReadIni("Conta", "Dinheiro", @".\Contas\" + nome + ".ini");
                novodinheiro = Convert.ToInt32(dinheiro)-Convert.ToInt32(quantia);
                Conexao.WriteINI("Conta", "Dinheiro", Convert.ToString(novodinheiro), @".\Contas\" + nome + ".ini");

                //Aumenta de quem ta recebendo
                dinheiro = Conexao.ReadIni("Conta", "Dinheiro", @".\Contas\" + nome2 + ".ini");
                novodinheiro = Convert.ToInt32(dinheiro) + Convert.ToInt32(quantia);
                Conexao.WriteINI("Conta", "Dinheiro", Convert.ToString(novodinheiro), @".\Contas\" + nome2 + ".ini");

                //Registrar transação
                Conexao.WriteINI("TR", "Nome", nome, @".\TR\"+Convert.ToString(novatr)+".ini");
                Conexao.WriteINI("TR", "Para", nome2, @".\TR\" + Convert.ToString(novatr) + ".ini");
                Conexao.WriteINI("TR", "Quantia", quantia, @".\TR\" + Convert.ToString(novatr) + ".ini");
                if(data != "Sem registro")
                {
                    Conexao.WriteINI("TR", "Data", teste_data[0].Replace(",", "/")+" "+teste_data[1], @".\TR\" + Convert.ToString(novatr) + ".ini");
                }
                else
                {
                    Conexao.WriteINI("TR", "Data", data, @".\TR\" + Convert.ToString(novatr) + ".ini");
                }

                //Registrar nas contas dos envolvidos [1]
                if (Conexao.ReadIni("Conta", "TR", @".\Contas\" + nome + ".ini") == "")
                {
                    Conexao.WriteINI("Conta", "TR", Convert.ToString(novatr) + ",", @".\Contas\" + nome + ".ini");
                }
                else
                {
                    for (int c = 0; c < teste_trs1.Length - 1; c++)
                    {
                        salvar_cod += teste_trs1[c] + ",";
                    }

                    salvar_cod += Convert.ToString(novatr) + ",";
                    Conexao.WriteINI("Conta", "TR", salvar_cod, @".\Contas\" + nome + ".ini");
                }

                //Registrar nas contas dos envolvidos [2]
                if (Conexao.ReadIni("Conta", "TR", @".\Contas\" + nome2 + ".ini") == "")
                {
                    Conexao.WriteINI("Conta", "TR", Convert.ToString(novatr) + ",", @".\Contas\" + nome2 + ".ini");
                }
                else
                {
                    for (int c = 0; c < teste_trs2.Length - 1; c++)
                    {
                        salvar_cod2 += teste_trs2[c] + ",";
                    }

                    salvar_cod2 += Convert.ToString(novatr) + ",";
                    Conexao.WriteINI("Conta", "TR", salvar_cod2, @".\Contas\" + nome2 + ".ini");
                }

                //Informar tudo OK
                swSenderSender = new StreamWriter(tcpClientes[count - 1].GetStream());
                swSenderSender.WriteLine("transferir/2");
                swSenderSender.Flush();
                swSenderSender = null;

                //Autenticar
                Autenticar(nome);
            }
            else
            {
                //Enviar
                swSenderSender = new StreamWriter(tcpClientes[count - 1].GetStream());
                swSenderSender.WriteLine("transferir/1");
                swSenderSender.Flush();
                swSenderSender = null;
            }
        }

        //Ver transações
        public static void VerTR(string nome, string tr)
        {
            //SENDER-SENDER PROBLEM
            StreamWriter swSenderSender;

            // Cria um array de clientes TCPs do tamanho do numero de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
            // Copia os objetos TcpClient no array
            ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);

            // Exibe primeiro na aplicação
            e = new StatusChangedEventArgs(">> VER TR <<\n"+nome+"-"+tr);
            OnStatusChanged(e);

            ICollection MyKeys;
            int count = 0;
            MyKeys = htUsuarios.Keys;

            foreach (object Key in MyKeys)
            {
                count++;
                e = new StatusChangedEventArgs(Convert.ToString(count - 1) + Key.ToString());
                OnStatusChanged(e);
                if (Key.ToString() == nome)
                {
                    break;
                }
            }

            //Pegar os valores
            string nome_enviou = Conexao.ReadIni("TR", "Nome", @".\TR\" + tr + ".ini");
            string para = Conexao.ReadIni("TR", "Para", @".\TR\" + tr + ".ini");
            string quantia = Conexao.ReadIni("TR", "Quantia", @".\TR\" + tr + ".ini");
            string data = Conexao.ReadIni("TR", "Data", @".\TR\" + tr + ".ini");

            //Enviar
            swSenderSender = new StreamWriter(tcpClientes[count - 1].GetStream());
            swSenderSender.WriteLine("vertr/"+nome_enviou+"/"+para + "/" +quantia + "/" + data);
            swSenderSender.Flush();
            swSenderSender = null;
        }

        // Autenticar certo jogador
        public static void Autenticar(string mensagem){
            //SENDER-SENDER PROBLEM
            StreamWriter swSenderSender;

            // Cria um array de clientes TCPs do tamanho do numero de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
            // Copia os objetos TcpClient no array
            ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);

            // Exibe primeiro na aplicação
            e = new StatusChangedEventArgs("Autenticação do user: " + mensagem);
            OnStatusChanged(e);

            ICollection MyKeys;
            int count = 0;
            MyKeys = htUsuarios.Keys;

            foreach (object Key in MyKeys)
            {
                count++;
                e = new StatusChangedEventArgs(Convert.ToString(count - 1) + Key.ToString());
                OnStatusChanged(e);
                if (Key.ToString() == mensagem)
                {
                    break;
                }
            }

            //Autenticar
            string money = Conexao.ReadIni("Conta", "Dinheiro", @".\Contas\" + mensagem + ".ini");
            string transacoes = Conexao.ReadIni("Conta", "TR", @".\Contas\" + mensagem + ".ini");
            string total_tr = Conexao.ReadIni("Conta", "TotalTR", @".\Contas\" + mensagem + ".ini");
            string tipo_conta = Conexao.ReadIni("Conta", "Tipo", @".\Contas\" + mensagem + ".ini");
            string icone = Conexao.ReadIni("Conta", "Foto", @".\Contas\" + mensagem + ".ini");
            
            //Enviar
            swSenderSender = new StreamWriter(tcpClientes[count - 1].GetStream());
            swSenderSender.WriteLine("Autenticar/" + money + "/" + mensagem + "/" + transacoes + "/" + total_tr + "/" + tipo_conta + "/" + icone);
            swSenderSender.Flush();
            swSenderSender = null;

        }

        // Enviar para certo jogador
        public static void EnviarPrivado(string mensagem)
        {
            string[] quebrar = mensagem.Split('/');
            StreamWriter swSenderSender;

            // Cria um array de clientes TCPs do tamanho do numero de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
            // Copia os objetos TcpClient no array
            ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);

            // Exibe primeiro na aplicação
            e = new StatusChangedEventArgs(quebrar[1] + "[PM] para " + quebrar[2] + " a mensagem " + quebrar[3]);
            OnStatusChanged(e);

            // 0 -> COD
            // 1 -> Nome de quem enviou
            // 2 -> Nome de quem vai receber
            // 3 -> Quantidade

            ICollection MyKeys;
            int count = 0;
            MyKeys = htUsuarios.Keys;

            foreach (object Key in MyKeys)
            {
                count++;
                e = new StatusChangedEventArgs(Convert.ToString(count - 1) + Key.ToString());
                OnStatusChanged(e);
                if(Key.ToString() == quebrar[2])
                {
                    break;
                }
            }

            //Enviar
            swSenderSender = new StreamWriter(tcpClientes[count-1].GetStream());
            swSenderSender.WriteLine("privado/"+quebrar[1]+"/"+quebrar[3]);
            swSenderSender.Flush();
            swSenderSender = null;
               
        }

        // Envia mensagens administratias
        public static void EnviaMensagemAdmin(string Mensagem)
        {
            StreamWriter swSenderSender;

            // Exibe primeiro na aplicação
            e = new StatusChangedEventArgs("Administrador: " + Mensagem);
            OnStatusChanged(e);

            // Cria um array de clientes TCPs do tamanho do numero de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
            // Copia os objetos TcpClient no array
            ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);
            // Percorre a lista de clientes TCP
            for (int i = 0; i < tcpClientes.Length; i++)
            {
                // Tenta enviar uma mensagem para cada cliente
                try
                {
                    // Se a mensagem estiver em branco ou a conexão for nula sai...
                    if (Mensagem.Trim() == "" || tcpClientes[i] == null)
                    {
                        continue;
                    }
                    // Envia a mensagem para o usuário atual no laço
                    swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine("Administrador: " + Mensagem);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // Se houver um problema , o usuário não existe , então remove-o
                {
                    RemoveUsuario(tcpClientes[i]);
                }
            }
        }

        // Envia mensagens de um usuário para todos os outros
        public static void EnviaMensagem(string Origem, string Mensagem)
        {
            string[] texto = Origem.Split('/');
            string[] teste = Mensagem.Split('/');
            //CODIGO
            string dinheiro = "";
            int valor1 = 0;
            int valor2 = 0;
            string totalTR = "";
            int tr1 = 0;
            int tr2 = 0;

            StreamWriter swSenderSender;

            // Primeiro exibe a mensagem na aplicação
            e = new StatusChangedEventArgs(texto[0] + " disse : " + Mensagem);
            OnStatusChanged(e);

            if(teste[0] == "privado")
            {
                ChatServidor.EnviarPrivado(Mensagem);
            }else if (teste[0] == "chat")
            {
                // Cria um array de clientes TCPs do tamanho do numero de clientes existentes
                TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
                // Copia os objetos TcpClient no array
                ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);
                // Percorre a lista de clientes TCP
                for (int i = 0; i < tcpClientes.Length; i++)
                {
                    // Tenta enviar uma mensagem para cada cliente
                    try
                    {
                        // Se a mensagem estiver em branco ou a conexão for nula sai...
                        if (Mensagem.Trim() == "" || tcpClientes[i] == null)
                        {
                            continue;
                        }
                        // Envia a mensagem para o usuário atual no laço
                        swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                        swSenderSender.WriteLine("CHAT/" + texto[0] + "/" + teste[1]);
                        swSenderSender.Flush();
                        swSenderSender = null;
                    }
                    catch // Se houver um problema , o usuário não existe , então remove-o
                    {
                        RemoveUsuario(tcpClientes[i]);
                    }
                }
            }else if (teste[0] == "autenticar")
            {
                Autenticar(teste[1]);
            }else if (teste[0] == "TR")
            {

            }else if (teste[0] == "avatar")
            {
                Conexao.WriteINI("Conta", "Foto", teste[1], @".\Contas\" + teste[2] + ".ini");
            }else if (teste[0] == "codigo")
            {
                string[] teste_codigos = Conexao.ReadIni("Conta", "Codigos", @".\Contas\" + teste[1] + ".ini").Split(',');
                string salvar_cod = "";
                bool permitir = true;

                if (File.Exists(@".\Codigos\" + teste[2] + ".ini"))
                {
                    for (int b = 0; b < teste_codigos.Length; b++)
                    {
                        if (teste_codigos[b] == teste[2])
                        {
                            permitir = false;
                        }
                    }

                    if (permitir)
                    {
                        //Aplicar o código
                        valor1 = Convert.ToInt32(Conexao.ReadIni("Conta", "Dinheiro", @".\Contas\" + teste[1] + ".ini"));
                        valor2 = Convert.ToInt32(Conexao.ReadIni("Codigo", "Dinheiro", @".\Codigos\" + teste[2] + ".ini"));
                        dinheiro = Convert.ToString(valor1 + valor2);
                        tr1 = Convert.ToInt32(Conexao.ReadIni("Conta", "TotalTR", @".\Contas\" + teste[1] + ".ini"));
                        tr2 = Convert.ToInt32(Conexao.ReadIni("Codigo", "TotalTR", @".\Codigos\" + teste[2] + ".ini"));
                        totalTR = Convert.ToString(tr1 + tr2);

                        //Salvar a aplicação do código
                        Conexao.WriteINI("Conta", "Dinheiro", dinheiro, @".\Contas\" + teste[1] + ".ini");
                        Conexao.WriteINI("Conta", "TotalTR", totalTR, @".\Contas\" + teste[1] + ".ini");
                        if(Conexao.ReadIni("Conta", "Codigos", @".\Contas\" + teste[1] + ".ini") == "")
                        {
                            Conexao.WriteINI("Conta", "Codigos", teste[2]+",", @".\Contas\" + teste[1] + ".ini");
                        }
                        else
                        {
                            for (int c = 0; c < teste_codigos.Length-1; c++)
                            {
                                // Primeiro exibe a mensagem na aplicação
                                e = new StatusChangedEventArgs(teste_codigos[c]);
                                OnStatusChanged(e);
                                salvar_cod += teste_codigos[c] + ",";
                            }

                            salvar_cod += teste[2] + ",";
                            Conexao.WriteINI("Conta", "Codigos", salvar_cod, @".\Contas\" + teste[1] + ".ini");
                        }

                        //Autenticar
                        Autenticar(teste[1]);
                    }//SE NÃO -:> SÓ PASSA
                }
            }else if (teste[0] == "transferir")
            {
                Transferir(teste[1], teste[2], teste[3], teste[4]);
            }else if (teste[0] == "vertr")
            {
                VerTR(teste[1], teste[2]);
            }
            else
            {
                // Cria um array de clientes TCPs do tamanho do numero de clientes existentes
                TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
                // Copia os objetos TcpClient no array
                ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);
                // Percorre a lista de clientes TCP
                for (int i = 0; i < tcpClientes.Length; i++)
                {
                    // Tenta enviar uma mensagem para cada cliente
                    try
                    {
                        // Se a mensagem estiver em branco ou a conexão for nula sai...
                        if (Mensagem.Trim() == "" || tcpClientes[i] == null)
                        {
                            continue;
                        }
                        // Envia a mensagem para o usuário atual no laço
                        swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                        swSenderSender.WriteLine("Administrador: " + texto[0]);
                        swSenderSender.Flush();
                        swSenderSender = null;
                    }
                    catch // Se houver um problema , o usuário não existe , então remove-o
                    {
                        RemoveUsuario(tcpClientes[i]);
                    }
                }
            }

            /*/ Cria um array de clientes TCPs do tamanho do numero de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
            // Copia os objetos TcpClient no array
            ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);
            // Percorre a lista de clientes TCP
            for (int i = 0; i < tcpClientes.Length; i++)
            {
                // Tenta enviar uma mensagem para cada cliente
                try
                {
                    // Se a mensagem estiver em branco ou a conexão for nula sai...
                    if (Mensagem.Trim() == "" || tcpClientes[i] == null)
                    {
                        continue;
                    }
                    // Envia a mensagem para o usuário atual no laço
                    swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine("Administrador: " + texto[0]);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // Se houver um problema , o usuário não existe , então remove-o
                {
                    RemoveUsuario(tcpClientes[i]);
                }
            }*/
        }

        public void IniciaAtendimento()
        {
            try
            {

                // Pega o IP do primeiro dispostivo da rede
                IPAddress ipaLocal = enderecoIP;

                // Cria um objeto TCP listener usando o IP do servidor e porta definidas
                tlsCliente = new TcpListener(ipaLocal, 3333);

                // Inicia o TCP listener e escuta as conexões
                tlsCliente.Start();

                // O laço While verifica se o servidor esta rodando antes de checar as conexões
                ServRodando = true;

                // Inicia uma nova tread que hospeda o listener
                thrListener = new Thread(MantemAtendimento);
                thrListener.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void MantemAtendimento()
        {
            // Enquanto o servidor estiver rodando
            while (ServRodando == true)
            {
                // Aceita uma conexão pendente
                tcpCliente = tlsCliente.AcceptTcpClient();
                // Cria uma nova instância da conexão
                Conexao newConnection = new Conexao(tcpCliente);
            }
        }
    }

    // Esta classe trata as conexões, serão tantas quanto as instâncias do usuários conectados
    class Conexao
    {
        TcpClient tcpCliente;
        // A thread que ira enviar a informação para o cliente
        private Thread thrSender;
        private StreamReader srReceptor;
        private StreamWriter swEnviador;
        private string usuarioAtual;
        private string strResposta;

        //Importar dll
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string re, StringBuilder retval, int size, string filePath);

        //Ler arquivo
        public static string ReadIni(string section, string key, string path)
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, String.Empty, sb, 255, path);
            return sb.ToString();
        }

        //Escrever arquivo
        public static void WriteINI(string Section, string Key, string Value, string path)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }

        // O construtor da classe que que toma a conexão TCP
        public Conexao(TcpClient tcpCon)
        {
            tcpCliente = tcpCon;
            // A thread que aceita o cliente e espera a mensagem
            thrSender = new Thread(AceitaCliente);
            // A thread chama o método AceitaCliente()
            thrSender.Start();
        }

        private void FechaConexao()
        {
            // Fecha os objetos abertos
            tcpCliente.Close();
            srReceptor.Close();
            swEnviador.Close();
        }

        // Ocorre quando um novo cliente é aceito
        private void AceitaCliente()
        {
            srReceptor = new System.IO.StreamReader(tcpCliente.GetStream());
            swEnviador = new System.IO.StreamWriter(tcpCliente.GetStream());
            string[] nome = new string[2];
            string lugar;

            // Lê a informação da conta do cliente
            usuarioAtual = srReceptor.ReadLine();
            nome = usuarioAtual.Split('/');
            lugar = @".\Contas\" + nome[0] + ".ini";

            //Cadastrar 'IMPORTANTE'
            if(nome[2] == "cadastrar")
            {
                if (File.Exists(lugar))
                {
                    // 5 => conta já cadastrada
                    swEnviador.WriteLine("5");
                    swEnviador.Flush();
                    FechaConexao();
                    return;
                }
                else
                {
                    
                    //Cadastrar .ini
                    WriteINI("Conta", "Senha", nome[1], lugar);
                    WriteINI("Conta", "Dinheiro", "0", lugar);
                    WriteINI("Conta", "Tipo", nome[3], lugar);
                    WriteINI("Conta", "Foto", "icon1", lugar);
                    WriteINI("Conta", "TR", "", lugar);
                    WriteINI("Conta", "Codigos", "", lugar);
                    if (nome[3] == "Pessoal")
                    {
                        WriteINI("Conta", "TotalTR", "3", lugar);
                    }
                    else
                    {
                        WriteINI("Conta", "TotalTR", "5", lugar);
                    }
                    WriteINI("Conta", "Acesso", "1", lugar);

                    // 6 => cadastro concluído
                    swEnviador.WriteLine("6");
                    swEnviador.Flush();
                    FechaConexao();
                    return;
                }
            }

            // temos uma resposta do cliente
            if (usuarioAtual != "")
            {
                if (File.Exists(lugar))
                {
                    if (ReadIni("Conta", "Acesso", lugar) == "0")
                    {
                        // 2 => acesso negado
                        swEnviador.WriteLine("2");
                        swEnviador.Flush();
                        FechaConexao();
                        return;
                    }
                    else
                    {
                        if (nome[1] != ReadIni("Conta", "Senha", @".\Contas\"+nome[0]+".ini"))
                        {
                            // 3 => senha errada
                            swEnviador.WriteLine("3");
                            swEnviador.Flush();
                            FechaConexao();
                            return;
                        }
                        else
                        {
                            if (ChatServidor.htUsuarios.Contains(nome[0]) == true)
                            {
                                // 4 => usuário já está conectado
                                swEnviador.WriteLine("4");
                                swEnviador.Flush();
                                FechaConexao();
                                return;
                            }
                            else
                            {
                                //Autenticar
                                string money       = ReadIni("Conta", "Dinheiro", @".\Contas\" + nome[0] + ".ini");
                                string transacoes  = ReadIni("Conta", "TR", @".\Contas\" + nome[0] + ".ini");
                                string total_tr    = ReadIni("Conta", "TotalTR", @".\Contas\" + nome[0] + ".ini");
                                string tipo_conta  = ReadIni("Conta", "Tipo", @".\Contas\" + nome[0] + ".ini");
                                string icone       = ReadIni("Conta", "Foto", @".\Contas\" + nome[0] + ".ini");

                                // 1 => conectou com sucesso
                                swEnviador.WriteLine("1");
                                swEnviador.Flush();

                                // First Authentication
                                swEnviador.WriteLine("Autenticar/" + money + "/" + nome[0] + "/" + transacoes + "/" + total_tr + "/" + tipo_conta + "/" + icone);
                                swEnviador.Flush();

                                // Inclui o usuário na hash table e inicia a escuta de suas mensagens
                                ChatServidor.IncluiUsuario(tcpCliente, usuarioAtual);
                            }
                        }
                    }
                }
                else
                {
                    // 0 => significa que a conta não existe
                    swEnviador.WriteLine("0");
                    swEnviador.Flush();
                    FechaConexao();
                    return;
                }
            }
            else
            {
                FechaConexao();
                return;
            }
            //
            try
            {
                // Continua aguardando por uma mensagem do usuário
                while ((strResposta = srReceptor.ReadLine()) != "")
                {
                    // Se for inválido remove-o
                    if (strResposta == null)
                    {
                        ChatServidor.RemoveUsuario(tcpCliente);
                    }
                    else
                    {
                        // envia a mensagem para todos os outros usuários
                        ChatServidor.EnviaMensagem(nome[0], strResposta);
                        // envia a mensagem para todos os outros usuários
                        //ChatServidor.EnviaMensagem(usuarioAtual, strResposta);
                    }
                }
            }
            catch
            {
                // Se houve um problema com este usuário desconecta-o
                ChatServidor.RemoveUsuario(tcpCliente);
            }
        }
    }
}
