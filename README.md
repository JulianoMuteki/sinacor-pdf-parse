# Sinacor PDF Reader

Leitor de notas de corretagem em PDF que extrai dados estruturados de arquivos de corretoras.
Corretoras suportadas:
- XP
- Banco Inter

## Estrutura do Projeto

O projeto foi refatorado para separar a lógica de extração em uma biblioteca reutilizável:

- **SinacorPdfParser**: Biblioteca de classes contendo toda a lógica de extração de dados
- **SinacorReader**: Aplicação console que utiliza a biblioteca para exibir os dados

### Biblioteca SinacorPdfParser

A biblioteca `SinacorPdfParser` pode ser usada independentemente em outros projetos .NET. Veja a documentação completa em [SinacorPdfParser/README.md](SinacorPdfParser/README.md).

#### Uso da Biblioteca

```csharp
using SinacorPdfParser;
using SinacorPdfParser.Models;

// Extrair dados de uma nota de corretagem
var notaNegociacao = SinacorPdfParser.PdfParser.ExtrairNotaCorretagem("caminho/para/nota.pdf");

// Acessar os dados extraídos
Console.WriteLine($"Número da Nota: {notaNegociacao.NumeroDaNota}");
Console.WriteLine($"Data do Pregão: {notaNegociacao.DataPregao:dd/MM/yyyy}");
Console.WriteLine($"Valor Líquido: R$ {notaNegociacao.ValorLiquidoPara:N2}");
```

## Como usar

### 1. Arquivo padrão
Para ler o arquivo padrão `20-11-2020_67988377_2024033016144001061500.pdf`:

```bash
cd SinacorReader
dotnet run
```

### 2. Arquivo específico via argumento de linha de comando
Para ler um arquivo PDF específico:

```bash
cd SinacorReader
dotnet run "caminho/para/seu/arquivo.pdf"
```

### 3. Arquivo via variável de ambiente
Defina a variável de ambiente `PDF_FILE_PATH`:

**Windows:**
```cmd
set PDF_FILE_PATH=caminho/para/seu/arquivo.pdf
dotnet run
```

**Linux/Mac:**
```bash
export PDF_FILE_PATH=caminho/para/seu/arquivo.pdf
dotnet run
```

## Prioridade de resolução do arquivo

1. **Argumento de linha de comando** (maior prioridade)
2. **Variável de ambiente** `PDF_FILE_PATH`
3. **Arquivo padrão** `20-11-2020_67988377_2024033016144001061500.pdf`

## Exemplos de uso

### Ler arquivo específico
```bash
dotnet run "C:\Users\Usuario\Documents\nota_corretagem.pdf"
```

### Ler arquivo relativo
```bash
dotnet run "..\outros_pdfs\nota_2024.pdf"
```

### Usar variável de ambiente
```bash
# Windows
set PDF_FILE_PATH=C:\Financeiro\notas\nota_marco.pdf
dotnet run

# Linux/Mac
export PDF_FILE_PATH=/home/user/financeiro/notas/nota_marco.pdf
dotnet run
```

## Dados extraídos

O programa extrai e exibe:

- **Dados básicos**: Número da nota, data do pregão, data líquido para, valor líquido
- **Clearing**: Valor líquido das operações, taxas de liquidação e registro
- **Bolsa**: Taxas de termo/opções, A.N.A., emolumentos
- **Resumo dos negócios**: Debêntures, vendas/compras à vista, opções, operações à termo
- **Custos operacionais**: Taxa operacional, execução, custódia, impostos
- **Operações**: Lista detalhada de todas as operações realizadas

## Desenvolvimento

### Compilar o projeto completo
```bash
dotnet build SinacorPdfParser.sln
```

### Compilar apenas a biblioteca
```bash
cd SinacorPdfParser
dotnet build
```

### Compilar apenas a aplicação
```bash
cd SinacorReader
dotnet build
```

### Executar testes
```bash
cd SinacorReader
dotnet run
```

## Requisitos

- .NET 8.0 ou superior
- iText7 (versão 8.0.2)
- Arquivo PDF válido de nota de corretagem

## Estrutura de Arquivos

```
sinacor-pdf-parse/
├── SinacorPdfParser/           # Biblioteca de classes
│   ├── Models/                 # Modelos de dados
│   │   ├── NotaNegociacao.cs
│   │   ├── Operacao.cs
│   │   ├── Clearing.cs
│   │   ├── Bolsa.cs
│   │   ├── CustosOperacionais.cs
│   │   └── ResumoDosNegocios.cs
│   ├── PdfParser.cs           # Classe principal do parser
│   ├── SinacorPdfParser.csproj
│   ├── README.md              # Documentação da biblioteca
│   └── ExemploUso.cs          # Exemplo de uso
├── SinacorReader/             # Aplicação console
│   ├── Program.cs             # Interface de usuário
│   └── SinacorReader.csproj
├── SinacorPdfParser.sln       # Solução completa
└── README.md                  # Este arquivo
``` 