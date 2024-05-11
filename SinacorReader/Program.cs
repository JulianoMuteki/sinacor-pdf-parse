using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;
using System.Globalization;

namespace LeitorNotaCorretagem
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "E:\\OneDrive\\Juliano\\Financeiro\\Investimentos\\XP - Notas\\XPINC_NOTA_NEGOCIACAO_B3_1_2023.pdf"; // Insira o caminho do seu arquivo PDF aqui

            List<Operacao> operacoes = ExtrairOperacoes(filePath);

            // Exemplo de como usar os dados extraídos

        }

        static List<Operacao> ExtrairOperacoes(string filePath)
        {
            List<Operacao> operacoes = new List<Operacao>();

            using (PdfReader reader = new PdfReader(filePath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {

                for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
                {
                    PdfPage page = pdfDoc.GetPage(pageNum);
                    string text = PdfTextExtractor.GetTextFromPage(page);
                    string paginaTexto = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum));

                    decimal taxaLiquidacao = ExtrairTaxaLiquidacao(text);
                    // decimal valorLiquidoOperacao = ExtrairValorPorFiltro(text, "Valor líquido das operações");

                    decimal valorDasOperacoes = ExtrairValorPorFiltro(text, "Valor das operações");


                    var tabela = ExtrairTabelaNegociacao(text);

                    ParseStringToOperacoes(tabela);
                    // Depois de extrair os dados, adicione o objeto Operacao à lista de operações
                    operacoes.Add(new Operacao
                    {
                        // Atribua os valores extraídos aos campos do objeto Operacao
                    });
                }

            }

            return operacoes;
        }

        public static decimal ExtrairTaxaLiquidacao(string text)
        {
            decimal taxaLiquidacao = 0M;

            // Expressão regular para encontrar o valor da Taxa de Liquidação
            Regex regex = new Regex(@"Taxa de liquidação\s+(\d{1,3}(?:\.\d{3})*(?:,\d{1,2})?)");

            Match match = regex.Match(text);
            if (match.Success)
            {
                string valorTaxaLiquidaçãoStr = match.Groups[1].Value;
                // Substituir a vírgula por ponto para garantir que o valor seja interpretado corretamente como decimal
                valorTaxaLiquidaçãoStr = valorTaxaLiquidaçãoStr.Replace(",", ".");
                taxaLiquidacao = decimal.Parse(valorTaxaLiquidaçãoStr, CultureInfo.InvariantCulture);
            }

            return taxaLiquidacao;
        }

        public static decimal ExtrairValorPorFiltro(string notaCorretagem, string termoBusca)
        {
            decimal valorFiltrado = 0M;

            // Use o termo de busca na expressão regular
            // Regex regex = new Regex($@"{termoBusca}\s+(\d{{1,3}}(?:\.\d{{3}})*(?:,\d{{1,2}})?)");
            Regex regex = new Regex($@"{termoBusca}\s+(\d{{1,3}}(?:\.\d{{3}})*(?:,\d{{2}}))");

            Match match = regex.Match(notaCorretagem);
            if (match.Success)
            {
                string valorStr = match.Groups[1].Value;
                // Substituir a vírgula por ponto para garantir que o valor seja interpretado corretamente como decimal

                valorFiltrado = ConverterParaDecimal(valorStr);
            }

            return valorFiltrado;
        }
        public static decimal ConverterParaDecimal(string valorString)
        {
            // Remover o ponto de milhar e substituir a vírgula pelo ponto decimal
            string valorFormatado = valorString.Replace(".", "").Replace(",", ".");

            // Converter para decimal
            decimal valorDecimal = decimal.Parse(valorFormatado, CultureInfo.InvariantCulture);

            return valorDecimal;
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
                if (linha.Contains("Negócios realizados"))
                {
                    dentroDaSeção = true;
                    continue; // Ignorar esta linha
                }
                else if (linha.Contains("Resumo dos Negócios Resumo Financeiro"))
                {
                    dentroDaSeção = false;
                    break; // Parar a busca, pois alcançamos o fim da seção desejada
                }

                if (dentroDaSeção)
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
    }

    public class Bolsa
    {
        public double TaxaDeTermoOpções { get; set; }
        public double TaxaANA { get; set; }
        public double Emolumentos { get; set; }
        public double Total { get; set; }
    }

    public class ResumoDosNegocios
    {

    }
}
