namespace SinacorPdfParser.Models
{
    public class Operacao
    {
        public string Q { get; set; } = string.Empty;
        public string Negociacao { get; set; } = string.Empty;
        public string CV { get; set; } = string.Empty;
        public string TipoMercado { get; set; } = string.Empty;
        public string Prazo { get; set; } = string.Empty;
        public string EspecificacaoTitulo { get; set; } = string.Empty;
        public string Obs { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public double PrecoAjuste { get; set; }
        public double ValorOperacaoAjuste { get; set; }
        public string DC { get; set; } = string.Empty;
    }
} 