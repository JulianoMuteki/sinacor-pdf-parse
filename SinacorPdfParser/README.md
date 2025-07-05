# SinacorPdfParser

Biblioteca .NET para extrair dados de notas de corretagem em formato PDF.

## Descrição

Esta biblioteca permite extrair informações estruturadas de notas de corretagem da Sinacor e outras corretoras que utilizam formatos similares. A biblioteca utiliza iText7 para processar arquivos PDF e extrair dados como:

- Número da nota
- Data do pregão
- Dados de clearing
- Dados da bolsa
- Resumo dos negócios
- Custos operacionais
- Lista de operações

## Instalação

### Via NuGet (recomendado)
```bash
dotnet add package SinacorPdfParser
```

### Via referência de projeto
```xml
<ItemGroup>
  <ProjectReference Include="path/to/SinacorPdfParser.csproj" />
</ItemGroup>
```

## Uso Básico

```csharp
using SinacorPdfParser;
using SinacorPdfParser.Models;

// Extrair dados de uma nota de corretagem
var notaNegociacao = SinacorPdfParser.PdfParser.ExtrairNotaCorretagem("caminho/para/nota.pdf");

// Acessar os dados extraídos
Console.WriteLine($"Número da Nota: {notaNegociacao.NumeroDaNota}");
Console.WriteLine($"Data do Pregão: {notaNegociacao.DataPregao:dd/MM/yyyy}");
Console.WriteLine($"Valor Líquido: R$ {notaNegociacao.ValorLiquidoPara:N2}");

// Acessar dados de clearing
if (notaNegociacao.Clearing != null)
{
    Console.WriteLine($"Taxa de Liquidação: R$ {notaNegociacao.Clearing.TaxaLiquidacao:N2}");
}

// Acessar operações
if (notaNegociacao.Operacoes != null)
{
    foreach (var operacao in notaNegociacao.Operacoes)
    {
        Console.WriteLine($"Operação: {operacao.EspecificacaoTitulo} - {operacao.CV} - Qtd: {operacao.Quantidade}");
    }
}
```

## Modelos de Dados

### NotaNegociacao
Classe principal que contém todos os dados extraídos:

- `NumeroDaNota`: Número da nota de corretagem
- `DataPregao`: Data do pregão
- `DataLiquidoPara`: Data para liquidação
- `ValorLiquidoPara`: Valor líquido para liquidação
- `Clearing`: Dados de clearing (pode ser null)
- `Bolsa`: Dados da bolsa (pode ser null)
- `ResumoDosNegocios`: Resumo dos negócios (pode ser null)
- `CustosOperacionais`: Custos operacionais (pode ser null)
- `Operacoes`: Lista de operações (pode ser null)

### Operacao
Representa uma operação individual:

- `Q`: Qualificador
- `Negociacao`: Número da negociação
- `CV`: Compra/Venda
- `TipoMercado`: Tipo de mercado
- `Prazo`: Prazo da operação
- `EspecificacaoTitulo`: Especificação do título
- `Obs`: Observações
- `Quantidade`: Quantidade negociada
- `PrecoAjuste`: Preço/ajuste
- `ValorOperacaoAjuste`: Valor da operação/ajuste
- `DC`: Débito/Crédito

## Tratamento de Erros

A biblioteca lança `FileNotFoundException` quando o arquivo PDF não é encontrado:

```csharp
try
{
    var nota = SinacorPdfParser.PdfParser.ExtrairNotaCorretagem("arquivo.pdf");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Arquivo não encontrado: {ex.Message}");
}
```

## Dependências

- .NET 8.0 ou superior
- iText7 (versão 8.0.2)

## Licença

Este projeto está sob a mesma licença do projeto principal. 