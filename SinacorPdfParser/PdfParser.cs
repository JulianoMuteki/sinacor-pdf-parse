using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using SinacorPdfParser.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SinacorPdfParser
{
    public class PdfParser
    {
        /// <summary>
        /// Extrai dados de uma nota de corretagem a partir de um arquivo PDF
        /// </summary>
        /// <param name="filePath">Caminho para o arquivo PDF</param>
        /// <returns>Objeto NotaNegociacao com os dados extraídos</returns>
        /// <exception cref="FileNotFoundException">Quando o arquivo não é encontrado</exception>
        public static NotaNegociacao ExtrairNotaCorretagem(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");
            }

            List<Operacao> operacoes = new List<Operacao>();
            NotaNegociacao notaNegociacao = new NotaNegociacao();

            using (PdfReader reader = new PdfReader(filePath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
                {
                    string paginaTexto = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum));
                    paginaTexto = CorrigirTextosPorCorretora(paginaTexto);

                    // Buscar número da nota e data do pregão por regex
                    var matchNota = Regex.Match(paginaTexto, @"Nr\.?\s*Nota:?\s*(\d+)", RegexOptions.IgnoreCase);
                    var matchData = Regex.Match(paginaTexto, @"Data pregão:?\s*(\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase);

                    if (matchNota.Success)
                        notaNegociacao.NumeroDaNota = int.Parse(matchNota.Groups[1].Value);
                    if (matchData.Success)
                        notaNegociacao.DataPregao = DateTime.ParseExact(matchData.Groups[1].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    // Data e valor líquido para
                    var dataLiquidoPara = ExtrairDataPorFiltro(paginaTexto, "Líquido para");
                    var valorLiquidoPara = ExtrairValorPorFiltro(paginaTexto, $"Líquido para {dataLiquidoPara}");
                    if (DateTime.TryParseExact(dataLiquidoPara, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataLiq))
                        notaNegociacao.DataLiquidoPara = dataLiq;
                    notaNegociacao.ValorLiquidoPara = valorLiquidoPara;

                    if (Regex.IsMatch(paginaTexto, @"(?i)Resumo dos Negócios"))
                    {
                        notaNegociacao.Clearing = CarregarDadosDoClearing(paginaTexto);
                        notaNegociacao.Bolsa = CarregarDadosDaBolsa(paginaTexto);
                        notaNegociacao.ResumoDosNegocios = CarregarDadosDoResumoDosNegcios(paginaTexto);
                        notaNegociacao.CustosOperacionais = CarregarDadosDosCustosOperacionais(paginaTexto);
                    }

                    var tabela = ExtrairTabelaNegociacao(paginaTexto);
                    operacoes.AddRange(ParseStringToOperacoes(tabela));
                }
            }
            notaNegociacao.Operacoes = operacoes;
            return notaNegociacao;
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

        private static CustosOperacionais CarregarDadosDosCustosOperacionais(string text)
        {
            CustosOperacionais custosOperacionais = new();
            double taxaOperacional = ExtrairValorPorFiltro(text, "Taxa Operacional");
            double execucao = ExtrairValorPorFiltro(text, "Execução");
            double taxaDeCustodia = ExtrairValorPorFiltro(text, "Taxa de Custódia");
            double impostos = ExtrairValorPorFiltro(text, "Impostos");
            double irrfOperacoesBase = ExtrairValorPorFiltro(text, "I.R.R.F.s/ operações, base");
            double outros = ExtrairValorPorFiltro(text, "Outros");

            custosOperacionais.TaxaOperacional = taxaOperacional;
            custosOperacionais.Execucao = execucao;
            custosOperacionais.TaxaDeCustodia = taxaDeCustodia;
            custosOperacionais.Impostos = impostos;
            custosOperacionais.IRRFsOperaçõesBase = irrfOperacoesBase;
            custosOperacionais.Ouitros = outros;

            return custosOperacionais;
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

        private static string CorrigirTextosPorCorretora(string paginaTexto)
        {
            string colunasXP = "Q Negociação C/V Tipo mercado Prazo Especificação do título Obs. (*) Quantidade Preço / Ajuste Valor Operação / Ajuste D/C";
            string colunasInter = "Obs (*) Quantidade Preço/Ajuste Valor D/C\nQ Negociação C/V Tipo mercado Prazo Especificação do titulo";

            paginaTexto = paginaTexto.Replace(colunasInter, colunasXP);
            return paginaTexto;
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

        public static string ExtrairDataPorFiltro(string notaCorretagem, string termoBusca)
        {
            string valorStr = "N/F";

            // Define the regex pattern to match the phrase "Líquido para" followed by a date in the format dd/MM/yyyy
            string frase = termoBusca;
            string pattern = $@"{Regex.Escape(frase)} (\d{{2}}/\d{{2}}/\d{{4}})";

            // Find matches in the input text
            Match match = Regex.Match(notaCorretagem, pattern);

            // If a match is found, print it
            if (match.Success)
            {
                valorStr = match.Groups[1].Value;
            }

            return valorStr;
        }

        public static double ConverterParaDecimal(string valorString)
        {
            // Remover o ponto de milhar e substituir a vírgula pelo ponto decimal
            string valorFormatado = valorString.Replace(".", "").Replace(",", ".");

            double valor = double.Parse(valorFormatado, CultureInfo.InvariantCulture);

            return valor;
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
} 