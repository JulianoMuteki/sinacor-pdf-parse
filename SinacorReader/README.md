# Sinacor PDF Reader

Leitor de notas de corretagem em PDF que extrai dados estruturados de arquivos de corretoras.

## Como usar

### 1. Arquivo padrão
Para ler o arquivo padrão `XPINC_NOTA_NEGOCIACAO_B3_1_2023.pdf`:

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
3. **Arquivo padrão** `XPINC_NOTA_NEGOCIACAO_B3_1_2023.pdf`

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

## Requisitos

- .NET 7.0 ou superior
- iText7 (versão 8.0.4)
- Arquivo PDF válido de nota de corretagem 