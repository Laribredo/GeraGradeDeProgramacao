using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using System.Data;
using System.Data.Odbc;

using Newtonsoft.Json;

using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using System.Globalization;

namespace IPTV_NovaGrade
{
    //CLASSE QUE REPRESENTA O TOKEN DE ACESSO À API DO IBMS
    public class AccessToken
    {
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string expires_on { get; set; }
        public string not_before { get; set; }
        public string resource { get; set; }
        public string access_token { get; set; }
        public DateTime expiration_date { get; set; }
    }

    class canais
    {
        public string nome { get; set; }
        public double epg { get; set; }
        public double id { get; set; }
    }


    class programasJson
    {
        public string titulo { get; set; }
        public string sinopse { get; set; }
        public string inicio { get; set; }
        public string fim { get; set; }
        public string classificacao { get; set; }
    }


    class programasIbms
    {
        public int itemGradeOrigemId { get; set; }
        public int eventoOrigemId { get; set; }
        public string data { get; set; }
        public string horario { get; set; }
        public string titulo { get; set; }
        public string tituloNomeAlternativo { get; set; }
        public string evento { get; set; }
        public string eventoSinopse { get; set; }
        public string tituloSinopse { get; set; }
        public string tituloGenero { get; set; }
        public string tituloSubGenero { get; set; }
        public string classificacaoIndicativa { get; set; }
        public string temporada { get; set; }
        public string duracao { get; set; }
        public int anoProducao { get; set; }
        public int episodio { get; set; }
        public string categoriaExibicao { get; set; }
        public string canal { get; set; }
        public string catchUp { get; set; }
        public string startOver { get; set; }
        public bool aoVivo { get; set; }
        public bool legenda { get; set; }
        public bool inedito { get; set; }
        public bool producaoIndependente { get; set; }
        public bool producaoNacional { get; set; }
        public bool eventoOlimpico { get; set; }
        public List<int> blocos { get; set; }
        public List<string> elenco { get; set; }
        public List<string> diretores { get; set; }
        public List<string> paises { get; set; }
        public string tx_elenco { get; set; }
        public string tx_diretores { get; set; }
        public string tx_paises { get; set; }
        public bool sistemaAutomacao { get; set; }
        public bool noAr { get; set; }
    }

    class Program
    {
        private static StreamReader file;

        //SOLICITA TOKEN DE ACESSO À API DO IBMS
        public static AccessToken solicitaIbmsAccessToken(string login_url, string token_url, string client_id, string client_secret, string resource)
        {
            //CONEXÃO COM A MICROSOFT PARA RECUPERARMOS O TOKEN DE ACESSO À API DO IBMS
            RestRequest request_microsoft;
            IRestResponse response_microsoft;
            AccessToken access_token = null;
            JsonDeserializer deserial = new JsonDeserializer();

            var client_microsoft = new RestClient(login_url);
            request_microsoft = new RestRequest(token_url, Method.POST);
            request_microsoft.AddParameter("grant_type", System.Web.HttpUtility.UrlPathEncode("client_credentials"));
            request_microsoft.AddParameter("client_id", System.Web.HttpUtility.UrlPathEncode(client_id));
            request_microsoft.AddParameter("client_secret", System.Web.HttpUtility.UrlPathEncode(client_secret));
            request_microsoft.AddParameter("resource", System.Web.HttpUtility.UrlPathEncode(resource));
            //request_microsoft.AddParameter("resource", System.Web.HttpUtility.UrlPathEncode("https://api-gradeprogramacao.azurewebsites.net"));



            response_microsoft = client_microsoft.Execute(request_microsoft);
            access_token = deserial.Deserialize<AccessToken>(response_microsoft);
            access_token.expiration_date = UnixTimeStampToDateTime(Double.Parse(access_token.expires_on)).AddMinutes(-10);

            return access_token;
        }

        //CONVERTOR DE DATA 
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();

            return dtDateTime;
        }

        //RECUPERA A CREDENCIAL IBMS
        public static string credenciaisIBMS()
        {
            System.IO.StreamReader file_ibms = new System.IO.StreamReader("credenciais_ibms.txt");
            string tx_ibms = file_ibms.ReadLine();
            file_ibms.Close();

            return tx_ibms;
        }

        //RECUPERA O ACESSO AO BANCO DE DADOS
        public static string conexaoBD()
        {
            System.IO.StreamReader file = new System.IO.StreamReader("conexao_bd.txt");
            string tx_conexao = file.ReadLine();
            file.Close();
            return tx_conexao;
        }


        //SUBSTITUI ALGUNS CARACTERES DOS JSON'S RETORNADOS PELO IBMS OU DO XML DO MARINA
        static string limpaCaracteres(string texto)
        {
            texto = texto.Replace("\\u0013", "-");
            texto = texto.Replace("\\u0014", "");
            texto = texto.Replace("\\u0018", "\'");
            texto = texto.Replace("\\u0019", "\'");
            texto = texto.Replace("\\u001f", "g");
            texto = texto.Replace("\\u000b", "");
            texto = texto.Replace("\\u0010", "");
            texto = texto.Replace("¿", "-");
            texto = texto.Replace("\\u001c", "\\\"");
            texto = texto.Replace("\\u001d", "\\\"");
            texto = texto.Replace("a\\u0001", "á");
            texto = texto.Replace("a\\u0002", "â");
            texto = texto.Replace("a\\u0003", "ã");
            texto = texto.Replace("e\\u0001", "é");
            texto = texto.Replace("e\\u0002", "ê");
            texto = texto.Replace("i\\u0001", "í");
            texto = texto.Replace("i\\u0002", "î");
            texto = texto.Replace("o\\u0001", "ó");
            texto = texto.Replace("o\\u0002", "ô");
            texto = texto.Replace("o\\u0003", "õ");
            texto = texto.Replace("u\\u0001", "ú");
            texto = texto.Replace("u\\u0002", "û");
            texto = texto.Replace("n\\u0003", "ñ");
            texto = texto.Replace("c'", "ç");

            return texto;
        }


        //ACESSA O BANCO DE DADOS E ARMAZENA OS DADOS DOS CANAIS  EM UMA LISTA
        public static List<canais> acessoBD()
        {

            //CONEXÃO COM O BANCO DE DADOS
            OdbcConnection Conexao = new OdbcConnection(conexaoBD());
            Console.WriteLine("* Abrindo conexão com o banco de dados\n");

            Conexao.Open();

            OdbcCommand cmd_canal = Conexao.CreateCommand();
            OdbcDataReader canal;

            List<canais> canal_c = new List<canais>();

            cmd_canal.CommandText = "SELECT C.nm_nome, C.id_canal, CFE.id_canal_epg FROM iptv2_canal C INNER JOIN iptv2_canal_fonte_epg CFE ON C.id_canal = CFE.id_canal WHERE id_fonte_epg = 1 AND CFE.lg_ativo = 'S'";
            canal = cmd_canal.ExecuteReader();

            if (canal.HasRows)
            {
                while (canal.Read())
                {
                    if (!canal.IsDBNull(2))
                    {
                        canal_c.Add(new canais { nome = canal.GetString(0), id = canal.GetDouble(1), epg = canal.GetDouble(2) });
                    }

                }
            }

            return canal_c;
        }

        //TRATA OS TITULOS CASO ESTEJAM NULL
        public static string trataTitulo(string tituloAlternativo, string evento)
        {

            if(tituloAlternativo == null)
            {
                tituloAlternativo = evento; 
            }

            return tituloAlternativo;
        }

        public static string trataSinopse(string sinopse,string sinopseAlternativa,int tamanhoMax)
        {
            if(sinopse == null)
            {
                if(sinopseAlternativa == null)
                {
                    sinopse = "";
                }
                else
                {
                    sinopse = sinopseAlternativa;
                }
            }


            //CORTANDO A SINOPSE NO PRIMEIRO PONTO
            if(sinopse.Count() > tamanhoMax)
            {
                sinopse = sinopse.Substring(0, sinopse.IndexOf(".")+1);
            }

          
            return sinopse;
        }

        public static string trataClassificacao(string classificao)
        {
            if(classificao == null || classificao == "isento")
            {
                classificao = "ND";
            }

            return classificao;
        }

        //FUNCAO PARA TRATAR A HORA DO JSON
        public static string trataHora(string hora,string data)
        {

            string dia = data.Substring(0, 10).Replace("-", "/");
            string hora_n = dia + " "+ hora;
            string horaRecebida = DateTime.Parse(hora_n, System.Globalization.CultureInfo.InvariantCulture).AddMonths(-1).ToString("yyyyMMddHHmmss");


            return horaRecebida;
        }

        public static void acessoIBMS(string caminho, int tamanhoMaxSinopse)
        {

            RestRequest request_grade;
            IRestResponse response_grade;
            List<programasIbms> grade;
            List<programasJson> js = new List<programasJson>();
            //LISTA DE DETALHES DOS CANAIS
            List<canais> canais_armazenados = acessoBD();

            //TRATANDO CREDENCIAL IBMS 
            string[] vet_credenciais = credenciaisIBMS().Split('\t');

            //CONEXÃO COM O IBMS
            var client_ibms = new RestClient(vet_credenciais[0]);

            AccessToken access_token = null;
            access_token = solicitaIbmsAccessToken(vet_credenciais[1], vet_credenciais[2], vet_credenciais[3], vet_credenciais[4], vet_credenciais[5]);


            foreach (canais valor in canais_armazenados)
            {

                Console.WriteLine("* Recuperando as informações de " + valor.nome);
                Console.WriteLine(valor.id);

                request_grade = new RestRequest("v1/canais/" + valor.epg + "/grades/" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), Method.GET);
                request_grade.AddHeader("Authorization", "Bearer " + System.Web.HttpUtility.UrlPathEncode(access_token.access_token));
                response_grade = client_ibms.Execute(request_grade);

                //VERIFICANDO SE TEMOS UM JSON VÁLIDO
                if (response_grade.Content != "" && response_grade.Content != "[\"Lista vazia\"]")
                {
                    Console.WriteLine("Informação chegou");

                    //CRIANDO ARQUIVO COM A PROGRAMAÇÃO
                    StreamWriter sw = new StreamWriter(File.Open(caminho + "\\programacao" + valor.id + ".json", FileMode.Create), Encoding.UTF8);
                    sw.AutoFlush = true;

                    //LIMPANDO O JSON RETORNADO PELO IBMS
                    response_grade.Content = limpaCaracteres(response_grade.Content);

                    //PEGA O JSON QUE VEM DO IBMS E FAZ UM TRATAMENTO E JÁ ADICIONA NA LISTA DE CLASSE PARA FORMAR O OBJETO JSON
                    grade = JsonConvert.DeserializeObject<List<programasIbms>>(response_grade.Content);

                    for(int i = 0; i < grade.Count; i++)
                    {
                        try
                        {

                            js.Add(new programasJson { titulo = trataTitulo(grade[i].tituloNomeAlternativo, grade[i].evento), classificacao = trataClassificacao(grade[i].classificacaoIndicativa), inicio = trataHora(grade[i].horario, grade[i].data), sinopse = trataSinopse(grade[i].tituloSinopse, grade[i].eventoSinopse,tamanhoMaxSinopse), fim = trataHora(grade[i+1].horario, grade[i+1].data) });
                        }catch(System.ArgumentOutOfRangeException)
                        {

                        }
                       

                    }

                    //sw.WriteLine(response_grade.Content);
                    sw.WriteLine("{\"programas\":" + JsonConvert.SerializeObject(js) + "}");
                    js.Clear();
                }

               
            }

        }


        static void Main(string[] args)
        {
            string caminho = args[0];
            int tamanhoSinopse = Convert.ToInt32(args[1]);
          

            acessoIBMS(caminho,tamanhoSinopse);

        }
    }
}
