using SinacorPdfParser;
using SinacorPdfParser.Models;

namespace SinacorReader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Feature flag para ler diferentes arquivos PDF
            string filePath = GetFilePath(args);
            
            Console.WriteLine($"Lendo arquivo: {filePath}");
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"ERRO: Arquivo não encontrado: {filePath}");
                Console.WriteLine("Verifique se o arquivo existe no caminho especificado.");
                return;
            }

            NotaNegociacao notaNegociacao = SinacorPdfParser.PdfParser.ExtrairNotaCorretagem(filePath);

            // Exibir dados extraídos
            Console.WriteLine("=== DADOS DA NOTA DE CORRETAGEM ===");
            Console.WriteLine($"Número da Nota: {notaNegociacao.NumeroDaNota}");
            Console.WriteLine($"Data do Pregão: {notaNegociacao.DataPregao:dd/MM/yyyy}");
            Console.WriteLine($"Data Líquido para: {notaNegociacao.DataLiquidoPara:dd/MM/yyyy}");
            Console.WriteLine($"Valor Líquido para: R$ {notaNegociacao.ValorLiquidoPara:N2}");
            
            if (notaNegociacao.Clearing != null)
            {
                Console.WriteLine("\n=== CLEARING ===");
                Console.WriteLine($"Valor Líquido das Operações: R$ {notaNegociacao.Clearing.ValorLiquidoOperacoes:N2}");
                Console.WriteLine($"Taxa de Liquidação: R$ {notaNegociacao.Clearing.TaxaLiquidacao:N2}");
                Console.WriteLine($"Taxa de Registro: R$ {notaNegociacao.Clearing.TaxaRegistro:N2}");
                Console.WriteLine($"Total CBLC: R$ {notaNegociacao.Clearing.Total:N2}");
            }

            if (notaNegociacao.Bolsa != null)
            {
                Console.WriteLine("\n=== BOLSA ===");
                Console.WriteLine($"Taxa de Termo/Opções: R$ {notaNegociacao.Bolsa.TaxaDeTermoOpções:N2}");
                Console.WriteLine($"Taxa A.N.A.: R$ {notaNegociacao.Bolsa.TaxaANA:N2}");
                Console.WriteLine($"Emolumentos: R$ {notaNegociacao.Bolsa.Emolumentos:N2}");
                Console.WriteLine($"Total Bovespa/Soma: R$ {notaNegociacao.Bolsa.Total:N2}");
            }

            if (notaNegociacao.ResumoDosNegocios != null)
            {
                Console.WriteLine("\n=== RESUMO DOS NEGÓCIOS ===");
                Console.WriteLine($"Debêntures: R$ {notaNegociacao.ResumoDosNegocios.Debentures:N2}");
                Console.WriteLine($"Vendas à Vista: R$ {notaNegociacao.ResumoDosNegocios.VendasAVista:N2}");
                Console.WriteLine($"Compras à Vista: R$ {notaNegociacao.ResumoDosNegocios.ComprasAVista:N2}");
                Console.WriteLine($"Opções - Compras: R$ {notaNegociacao.ResumoDosNegocios.OpcoesCompras:N2}");
                Console.WriteLine($"Opções - Vendas: R$ {notaNegociacao.ResumoDosNegocios.OpcoesVendas:N2}");
                Console.WriteLine($"Operações à Termo: R$ {notaNegociacao.ResumoDosNegocios.OperacoesATermo:N2}");
                Console.WriteLine($"Valor das Oper. c/ Títulos Púb.: R$ {notaNegociacao.ResumoDosNegocios.ValorOperacoesTitulosPublico:N2}");
                Console.WriteLine($"Valor das Operações: R$ {notaNegociacao.ResumoDosNegocios.ValorDasOperacoes:N2}");
            }

            if (notaNegociacao.CustosOperacionais != null)
            {
                Console.WriteLine("\n=== CUSTOS OPERACIONAIS ===");
                Console.WriteLine($"Taxa Operacional: R$ {notaNegociacao.CustosOperacionais.TaxaOperacional:N2}");
                Console.WriteLine($"Execução: R$ {notaNegociacao.CustosOperacionais.Execucao:N2}");
                Console.WriteLine($"Taxa de Custódia: R$ {notaNegociacao.CustosOperacionais.TaxaDeCustodia:N2}");
                Console.WriteLine($"Impostos: R$ {notaNegociacao.CustosOperacionais.Impostos:N2}");
                Console.WriteLine($"I.R.R.F.s/ operações, base: R$ {notaNegociacao.CustosOperacionais.IRRFsOperaçõesBase:N2}");
                Console.WriteLine($"Outros: R$ {notaNegociacao.CustosOperacionais.Ouitros:N2}");
            }

            if (notaNegociacao.Operacoes != null && notaNegociacao.Operacoes.Count > 0)
            {
                Console.WriteLine($"\n=== OPERAÇÕES ({notaNegociacao.Operacoes.Count} operações encontradas) ===");
                foreach (var operacao in notaNegociacao.Operacoes) // Mostrar apenas as primeiras 5 operações
                {
                    Console.WriteLine($"Negociação: {operacao.Negociacao} | C/V: {operacao.CV} | Título: {operacao.EspecificacaoTitulo} | Qtd: {operacao.Quantidade} | Preço: R$ {operacao.PrecoAjuste:N2} | Valor: R$ {operacao.ValorOperacaoAjuste:N2}");
                }
            }

            Console.WriteLine("\n=== EXTRAÇÃO CONCLUÍDA ===");
        }

        /// <summary>
        /// Obtém o caminho do arquivo PDF baseado em argumentos de linha de comando ou variável de ambiente
        /// </summary>
        /// <param name="args">Argumentos de linha de comando</param>
        /// <returns>Caminho do arquivo PDF</returns>
        private static string GetFilePath(string[] args)
        {
            // Prioridade 1: Argumento de linha de comando
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                return args[0];
            }

            // Prioridade 2: Variável de ambiente PDF_FILE_PATH
            string envFilePath = Environment.GetEnvironmentVariable("PDF_FILE_PATH");
            if (!string.IsNullOrWhiteSpace(envFilePath))
            {
                return envFilePath;
            }

            // Prioridade 3: Arquivo padrão
            return "20-11-2020_67988377_2024033016144001061500.pdf";
        }
    }
}