namespace SinacorPdfParser.Models
{
    public class NotaNegociacao
    {
        public int NumeroDaNota { get; set; }
        public DateTime DataPregao { get; set; }
        public DateTime DataLiquidoPara { get; set; }
        public double ValorLiquidoPara { get; set; }
        public Clearing? Clearing { get; set; }
        public Bolsa? Bolsa { get; set; }
        public ResumoDosNegocios? ResumoDosNegocios { get; set; }
        public CustosOperacionais? CustosOperacionais { get; set; }
        public List<Operacao>? Operacoes { get; set; }
    }
} 