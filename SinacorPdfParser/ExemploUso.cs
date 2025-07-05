using SinacorPdfParser;
using SinacorPdfParser.Models;

namespace ExemploUso
{
    /// <summary>
    /// Exemplo de como usar a biblioteca SinacorPdfParser
    /// </summary>
    public class ExemploUso
    {
        public static void Main(string[] args)
        {
            try
            {
                // Exemplo 1: Uso básico
                var notaNegociacao = SinacorPdfParser.PdfParser.ExtrairNotaCorretagem("caminho/para/nota.pdf");
                
                Console.WriteLine("=== DADOS EXTRAÍDOS ===");
                Console.WriteLine($"Número da Nota: {notaNegociacao.NumeroDaNota}");
                Console.WriteLine($"Data do Pregão: {notaNegociacao.DataPregao:dd/MM/yyyy}");
                Console.WriteLine($"Valor Líquido: R$ {notaNegociacao.ValorLiquidoPara:N2}");

                // Exemplo 2: Acessar dados de clearing
                if (notaNegociacao.Clearing != null)
                {
                    Console.WriteLine("\n=== CLEARING ===");
                    Console.WriteLine($"Valor Líquido das Operações: R$ {notaNegociacao.Clearing.ValorLiquidoOperacoes:N2}");
                    Console.WriteLine($"Taxa de Liquidação: R$ {notaNegociacao.Clearing.TaxaLiquidacao:N2}");
                    Console.WriteLine($"Taxa de Registro: R$ {notaNegociacao.Clearing.TaxaRegistro:N2}");
                    Console.WriteLine($"Total CBLC: R$ {notaNegociacao.Clearing.Total:N2}");
                }

                // Exemplo 3: Acessar operações
                if (notaNegociacao.Operacoes != null && notaNegociacao.Operacoes.Count > 0)
                {
                    Console.WriteLine($"\n=== OPERAÇÕES ({notaNegociacao.Operacoes.Count} encontradas) ===");
                    
                    // Mostrar apenas as primeiras 5 operações
                    foreach (var operacao in notaNegociacao.Operacoes.Take(5))
                    {
                        Console.WriteLine($"Negociação: {operacao.Negociacao} | C/V: {operacao.CV} | " +
                                        $"Título: {operacao.EspecificacaoTitulo} | Qtd: {operacao.Quantidade} | " +
                                        $"Preço: R$ {operacao.PrecoAjuste:N2} | Valor: R$ {operacao.ValorOperacaoAjuste:N2}");
                    }
                }

                // Exemplo 4: Calcular totais
                if (notaNegociacao.Operacoes != null)
                {
                    var totalCompras = notaNegociacao.Operacoes
                        .Where(o => o.CV.ToUpper() == "C")
                        .Sum(o => o.ValorOperacaoAjuste);
                    
                    var totalVendas = notaNegociacao.Operacoes
                        .Where(o => o.CV.ToUpper() == "V")
                        .Sum(o => o.ValorOperacaoAjuste);

                    Console.WriteLine($"\n=== RESUMO ===");
                    Console.WriteLine($"Total Compras: R$ {totalCompras:N2}");
                    Console.WriteLine($"Total Vendas: R$ {totalVendas:N2}");
                    Console.WriteLine($"Saldo: R$ {(totalVendas - totalCompras):N2}");
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"ERRO: Arquivo não encontrado: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
            }
        }
    }
} 