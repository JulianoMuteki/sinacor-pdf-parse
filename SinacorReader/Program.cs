using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SinacorReader
{
    internal class Program
    {
        static void Main(string[] args)
        {
             string filePath = "E:\\OneDrive\\Juliano\\Financeiro\\Investimentos\\XP - Notas\\XPINC_NOTA_NEGOCIACAO_B3_1_2023.pdf"; // Insira o caminho do seu arquivo PDF aqui
            NotaNegociacao notaNegociacao = ExtrairNotaCorretagem(filePath);

        }

        static NotaNegociacao ExtrairNotaCorretagem(string filePath)
        {
            List<Operacao> operacoes = new List<Operacao>();

            using (PdfReader reader = new PdfReader(filePath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {

                for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
                {
                   // PdfPage page = pdfDoc.GetPage(pageNum);
                   // string text = PdfTextExtractor.GetTextFromPage(page);
                    string paginaTexto = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum));
                    paginaTexto = CorrigirTextosPorCorretora(paginaTexto);

                    //regex que capture essa frase específica com cada palavra podendo começar com letra maiúscula ou minúscula
                    if (Regex.IsMatch(paginaTexto, @"(?i)Resumo dos Negócios"))
                    {
                        CarregarDadosDoClearing(paginaTexto);
                        CarregarDadosDaBolsa(paginaTexto);
                        CarregarDadosDoResumoDosNegcios(paginaTexto);
                    }

                    var tabela = ExtrairTabelaNegociacao(paginaTexto);

                    // Depois de extrair os dados, adicione o objeto Operacao à lista de operações
                    operacoes.AddRange(ParseStringToOperacoes(tabela));
                }

            }

            return new NotaNegociacao();
        }

        private static string CorrigirTextosPorCorretora(string paginaTexto)
        {
            string colunasXP = "Q Negociação C/V Tipo mercado Prazo Especificação do título Obs. (*) Quantidade Preço / Ajuste Valor Operação / Ajuste D/C";
            string colunasInter = "Obs (*) Quantidade Preço/Ajuste Valor D/C\nQ Negociação C/V Tipo mercado Prazo Especificação do titulo";

            paginaTexto = paginaTexto.Replace(colunasInter, colunasXP);
            return paginaTexto;
        }

        private static ResumoDosNegocios CarregarDadosDoResumoDosNegcios(string text)
        {
            ResumoDosNegocios resumoDosNegocios = new();
            double debentures = ExtrairValorPorFiltro(text, "Debêntures");
            double vendasAVista = ExtrairValorPorFiltro(text, "Vendas à vista");
            double comprasAVista = ExtrairValorPorFiltro(text, "Compras à vista");
            double opcoesCompras = ExtrairValorPorFiltro(text, "Opções - compras");
            double opcoesVendas = ExtrairValorPorFiltro(text, "Opções - vendas");
            double operacoesATermo = ExtrairValorPorFiltro(text, "Operações à termo");
            double valorOperacoesComTitulosPublicos = ExtrairValorPorFiltro(text, "Valor das oper. c/ títulos púb");
            double valorDasOperacoes = ExtrairValorPorFiltro(text, "Valor das operações");

            resumoDosNegocios.Debentures = debentures;
            resumoDosNegocios.VendasAVista = vendasAVista;
            resumoDosNegocios.ComprasAVista = comprasAVista;
            resumoDosNegocios.OpcoesCompras = opcoesCompras;
            resumoDosNegocios.OpcoesVendas = opcoesVendas;
            resumoDosNegocios.OperacoesATermo = operacoesATermo;
            resumoDosNegocios.ValorOperacoesTitulosPublico = valorOperacoesComTitulosPublicos;
            resumoDosNegocios.ValorDasOperacoes = valorDasOperacoes;

            return resumoDosNegocios;
        }

        private static Bolsa CarregarDadosDaBolsa(string text)
        {
            Bolsa bolsa = new();
            double taxaTermoOpcoes = ExtrairValorPorFiltro(text, "Taxa de termo/opções");
            double taxaANA = ExtrairValorPorFiltro(text, "Taxa A.N.A.");
            double emolumentos = ExtrairValorPorFiltro(text, "Emolumentos");
            double totalBovespaSoma = ExtrairValorPorFiltro(text, "Total Bovespa / Soma");

            bolsa.TaxaANA = taxaANA;
            bolsa.TaxaDeTermoOpções = taxaTermoOpcoes;
            bolsa.Emolumentos = emolumentos;
            bolsa.Total = totalBovespaSoma;

            return bolsa;
        }

        private static Clearing CarregarDadosDoClearing(string text)
        {
            Clearing clearing = new();
            double valorLiquidoOperacao = ExtrairValorPorFiltro(text, "Valor líquido das operações");
            double taxaLiquidacao = ExtrairValorPorFiltro(text, "Taxa de liquidação");
            double taxaRegistro = ExtrairValorPorFiltro(text, "Taxa de Registro");
            double total = ExtrairValorPorFiltro(text, "Total CBLC");

            clearing.ValorLiquidoOperacoes = valorLiquidoOperacao;
            clearing.TaxaLiquidacao = taxaLiquidacao;
            clearing.TaxaRegistro = taxaRegistro;
            clearing.Total = total;

            return clearing;
        }

        public static double ExtrairValorPorFiltro(string notaCorretagem, string termoBusca)
        {
            double valorFiltrado = 0;

            // Use o termo de busca na expressão regular
            Regex regex = new Regex($@"{termoBusca}\s+(\d{{1,3}}(?:\.\d{{3}})*(?:,\d{{2}}))");

            Match match = regex.Match(notaCorretagem);
            if (match.Success)
            {
                string valorStr = match.Groups[1].Value;
                valorFiltrado = ConverterParaDecimal(valorStr);
            }

            return valorFiltrado;
        }
        public static double ConverterParaDecimal(string valorString)
        {
            // Remover o ponto de milhar e substituir a vírgula pelo ponto decimal
            string valorFormatado = valorString.Replace(".", "").Replace(",", ".");

            double valor = double.Parse(valorFormatado, CultureInfo.InvariantCulture);

            return valor;
        }

        public static string ExtrairTabelaNegociacao(string text)
        {
            // Dividir o texto em linhas
            string[] linhas = text.Split('\n');

            bool dentroDaSeção = false;
            List<string> linhasNegocios = new List<string>();

            // Iterar pelas linhas para encontrar a seção desejada
            foreach (string linha in linhas)
            {
                if (Regex.IsMatch(linha, @"(?i)Negócios realizados"))
                {
                    dentroDaSeção = true;
                    continue; // Ignorar esta linha
                }
                else if (Regex.IsMatch(linha, @"(?i)Resumo dos negócios"))
                {
                    dentroDaSeção = false;
                    break; // Parar a busca, pois alcançamos o fim da seção desejada
                }

                if (dentroDaSeção && linha.Length > 15)
                {
                    linhasNegocios.Add(linha);
                }
            }

            // Concatenar todas as linhas encontradas em uma única string
            string resultado = string.Join("\n", linhasNegocios);
            return resultado;
        }

        public static List<Operacao> ParseStringToOperacoes(string tabela)
        {
            var linhas = tabela.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var operacoes = new List<Operacao>();

            foreach (var linha in linhas.Skip(1)) // Ignora o cabeçalho
            {
                var colunas = linha.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                bool cvIsIndex2 = false;
                int startSkip = 0;

                int totalEspecificacaoTitulo = 0;
                for (int i = colunas.Length - 5; i >= 0; i--)
                {
                    if (colunas[i].ToUpper().Equals("C") || colunas[i].ToUpper().Equals("V"))
                    {
                        cvIsIndex2 = (i == 1);
                        totalEspecificacaoTitulo--;
                        startSkip = i + 2;
                        break;
                    }
                    totalEspecificacaoTitulo++;
                }

                int valueStartIndex = cvIsIndex2 ? 0 : 1;

                var operacao = new Operacao
                {
                    Q = valueStartIndex == 1 ? colunas[0] : "",
                    Negociacao = colunas[valueStartIndex],
                    CV = colunas[valueStartIndex + 1],
                    TipoMercado = colunas[valueStartIndex + 2],
                    Prazo = "", //colunas[4],
                    EspecificacaoTitulo = string.Join(" ", colunas.Skip(startSkip).Take(totalEspecificacaoTitulo)), // Juntando as palavras até encontrar o primeiro número
                    Obs = "", //colunas[colunas.Length - 4],
                    Quantidade = int.Parse(colunas[colunas.Length - 4]),
                    PrecoAjuste = double.Parse(colunas[colunas.Length - 3]),
                    ValorOperacaoAjuste = double.Parse(colunas[colunas.Length - 2]),
                    DC = colunas[colunas.Length - 1]
                };

                operacoes.Add(operacao);
            }

            return operacoes;
        }
    }

    public class NotaNegociacao
    {
        public int NumeroDaNota { get; set; }
        public DateTime DataPregao { get; set; }

        public DateTime DataLiquidoPara { get; set; }
        public double ValorLiquidoPara { get; set; }
    }

    public class Operacao
    {
        public string Q { get; set; }
        public string Negociacao { get; set; }
        public string CV { get; set; }
        public string TipoMercado { get; set; }
        public string Prazo { get; set; }
        public string EspecificacaoTitulo { get; set; }
        public string Obs { get; set; }
        public int Quantidade { get; set; }
        public double PrecoAjuste { get; set; }
        public double ValorOperacaoAjuste { get; set; }
        public string DC { get; set; }
    }

    public class Clearing
    {
        public double ValorLiquidoOperacoes { get; set; }
        public double TaxaLiquidacao { get; set; }
        public double TaxaRegistro { get; set; }
        public double Total { get; set; }
        //Valor líquido das operações D
        //Taxa de liquidação D
        //Taxa de Registro
    }

    public class Bolsa
    {
        public double TaxaDeTermoOpções { get; set; }
        public double TaxaANA { get; set; }
        public double Emolumentos { get; set; }
        public double Total { get; set; }
        //Taxa de termo/opções
        //Taxa A.N.A.
        //Emolumento
    }

    public class CustosOperacionais
    {
        public double TaxaOperacional { get; set; }
        public double Execucao { get; set; }
        public double TaxaDeCustodia { get; set; }
        public double Impostos { get; set; }
        public double IRRFsOperaçõesBase { get; set; }
        public double Ouitros { get; set; }
        //Taxa Operacional
        //Execução
        //Taxa de Custódia
        //Impostos
        //I.R.R.F.s/ operações, base R$0,00
        //Outros
    }

    public class ResumoDosNegocios
    {
        public double Debentures { get; set; }
        public double VendasAVista { get; set; }
        public double ComprasAVista { get; set; }
        public double OpcoesCompras { get; set; }
        public double OpcoesVendas { get; set; }
        public double OperacoesATermo { get; set; }
        public double ValorOperacoesTitulosPublico { get; set; }
        public double ValorDasOperacoes { get; set; }
        //Debêntures
        //Vendas à vista
        //Compras à vista
        //Opções - compras
        //Opções - vendas
        //Operações à termo
        //Valor das oper. c/ títulos públ. (v. nom.)
        //Valor das operações
    }
}