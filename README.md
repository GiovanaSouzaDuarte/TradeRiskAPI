# Trade Risk Classification API

API REST desenvolvida em ASP.NET Core 8 para classificação automática de operações financeiras (trades) de acordo com o nível de risco. A solução foi projetada seguindo os princípios de Clean Architecture e padrões de design que facilitam a manutenção e extensibilidade do código.

## Índice

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Regras de Classificação](#regras-de-classificação)
- [Endpoints da API](#endpoints-da-api)
- [Como Executar](#como-executar)
- [Executando os Testes](#executando-os-testes)
- [Decisões Técnicas](#decisões-técnicas)
- [Extensibilidade](#extensibilidade)

## Visão Geral

Esta API foi desenvolvida para atender à necessidade de classificar automaticamente milhares de operações financeiras de acordo com o nível de risco. Cada trade possui um valor monetário e o setor do cliente (Público ou Privado), e a API retorna a classificação de risco adequada baseada em regras de negócio configuráveis.

A solução suporta processamento eficiente de até 100.000 trades em uma única requisição, incluindo análise estatística com resumo por categoria de risco.

## Arquitetura

O projeto segue os princípios da **Clean Architecture**, organizando o código em camadas com responsabilidades bem definidas:

```
┌─────────────────────────────────────────────────────────────┐
│                        WebAPI                                │
│  (Controllers, Middleware, DTOs de Request/Response)        │
├─────────────────────────────────────────────────────────────┤
│                      Application                             │
│  (Services, Rules, Validators, DTOs)                        │
├─────────────────────────────────────────────────────────────┤
│                        Domain                                │
│  (Entities, Enums, Interfaces)                              │
└─────────────────────────────────────────────────────────────┘
```

| Camada | Responsabilidade |
|--------|------------------|
| **Domain** | Contém as entidades de negócio, enums e interfaces. Não possui dependências externas. |
| **Application** | Implementa a lógica de negócio, regras de classificação e validações. Depende apenas do Domain. |
| **WebAPI** | Expõe os endpoints REST, configura o middleware e gerencia as requisições HTTP. |

## Tecnologias Utilizadas

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| .NET | 8.0 | Framework base |
| ASP.NET Core | 8.0 | Framework web para APIs REST |
| FluentValidation | 11.x | Validação de entrada de dados |
| Serilog | 4.x | Logging estruturado |
| xUnit | 2.5.x | Framework de testes |
| FluentAssertions | 8.x | Asserções fluentes para testes |
| Moq | 4.x | Mocking para testes unitários |

## Estrutura do Projeto

```
TradeRiskAPI/
├── src/
│   ├── TradeRiskAPI.Domain/
│   │   ├── Entities/
│   │   │   ├── Trade.cs
│   │   │   └── CategorySummary.cs
│   │   ├── Enums/
│   │   │   └── RiskCategory.cs
│   │   └── Interfaces/
│   │       ├── IClassificationRule.cs
│   │       ├── ITradeClassificationService.cs
│   │       └── ITradeAnalysisService.cs
│   │
│   ├── TradeRiskAPI.Application/
│   │   ├── DTOs/
│   │   │   └── TradeDto.cs
│   │   ├── Rules/
│   │   │   ├── LowRiskRule.cs
│   │   │   ├── MediumRiskRule.cs
│   │   │   └── HighRiskRule.cs
│   │   ├── Services/
│   │   │   ├── TradeClassificationService.cs
│   │   │   └── TradeAnalysisService.cs
│   │   ├── Validators/
│   │   │   ├── ClassifyRequestValidator.cs
│   │   │   └── AnalyzeRequestValidator.cs
│   │   └── DependencyInjection.cs
│   │
│   └── TradeRiskAPI.WebAPI/
│       ├── Controllers/
│       │   └── TradesController.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       └── Program.cs
│
└── tests/
    ├── TradeRiskAPI.UnitTests/
    │   ├── Rules/
    │   ├── Services/
    │   └── Validators/
    │
    └── TradeRiskAPI.IntegrationTests/
        └── TradesControllerTests.cs
```

## Regras de Classificação

As regras de classificação são aplicadas na seguinte ordem de prioridade:

| Categoria | Regra | Prioridade |
|-----------|-------|------------|
| **LOWRISK** | Trades com valor menor que 1.000.000 | 1 |
| **MEDIUMRISK** | Trades com valor ≥ 1.000.000 e cliente do setor Público | 2 |
| **HIGHRISK** | Trades com valor ≥ 1.000.000 e cliente do setor Privado | 3 |

## Endpoints da API

### POST /api/trades/classify

Classifica uma lista de trades e retorna as categorias de risco correspondentes.

**Request Body:**
```json
{
  "trades": [
    { "value": 2000000, "clientSector": "Private" },
    { "value": 400000, "clientSector": "Public" },
    { "value": 500000, "clientSector": "Public" },
    { "value": 3000000, "clientSector": "Public" }
  ]
}
```

**Response (200 OK):**
```json
{
  "categories": ["HIGHRISK", "LOWRISK", "LOWRISK", "MEDIUMRISK"]
}
```

### POST /api/trades/analyze

Classifica trades e retorna um resumo estatístico completo da carteira.

**Request Body:**
```json
{
  "trades": [
    { "value": 2000000, "clientSector": "Private", "clientId": "CLI001" },
    { "value": 400000, "clientSector": "Public", "clientId": "CLI002" },
    { "value": 500000, "clientSector": "Public", "clientId": "CLI003" },
    { "value": 3000000, "clientSector": "Public", "clientId": "CLI004" }
  ]
}
```

**Response (200 OK):**
```json
{
  "categories": ["HIGHRISK", "LOWRISK", "LOWRISK", "MEDIUMRISK"],
  "summary": {
    "LOWRISK": {
      "count": 2,
      "totalValue": 900000,
      "topClient": "CLI003"
    },
    "MEDIUMRISK": {
      "count": 1,
      "totalValue": 3000000,
      "topClient": "CLI004"
    },
    "HIGHRISK": {
      "count": 1,
      "totalValue": 2000000,
      "topClient": "CLI001"
    }
  },
  "processingTimeMs": 12
}
```

### Respostas de Erro

**400 Bad Request - Erro de Validação:**
```json
{
  "type": "ValidationError",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": {
    "Trades[0].ClientSector": ["Client sector must be 'Public' or 'Private'"]
  }
}
```

## Como Executar

### Pré-requisitos

- .NET SDK 8.0 ou superior

### Passos para Execução

1. Clone o repositório ou extraia os arquivos do projeto

2. Navegue até a pasta raiz do projeto:
```bash
cd TradeRiskAPI
```

3. Restaure as dependências:
```bash
dotnet restore
```

4. Execute a aplicação:
```bash
dotnet run --project src/TradeRiskAPI.WebAPI
```

5. A API estará disponível em:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`

### Testando a API

Você pode testar os endpoints usando curl:

```bash
# Classificar trades
curl -X POST http://localhost:5000/api/trades/classify \
  -H "Content-Type: application/json" \
  -d '{"trades":[{"value":2000000,"clientSector":"Private"},{"value":400000,"clientSector":"Public"}]}'

# Analisar trades com resumo estatístico
curl -X POST http://localhost:5000/api/trades/analyze \
  -H "Content-Type: application/json" \
  -d '{"trades":[{"value":2000000,"clientSector":"Private","clientId":"CLI001"},{"value":400000,"clientSector":"Public","clientId":"CLI002"}]}'
```

## Executando os Testes

### Todos os Testes
```bash
dotnet test
```

### Apenas Testes Unitários
```bash
dotnet test tests/TradeRiskAPI.UnitTests
```

### Apenas Testes de Integração
```bash
dotnet test tests/TradeRiskAPI.IntegrationTests
```

### Com Cobertura de Código
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Decisões Técnicas

### Clean Architecture

A escolha pela Clean Architecture permite uma clara separação de responsabilidades, facilitando a manutenção e evolução do código. As dependências fluem de fora para dentro, garantindo que a camada de domínio permaneça isolada de detalhes de infraestrutura.

### Strategy Pattern para Regras de Classificação

O padrão Strategy foi implementado através da interface `IClassificationRule`, permitindo que novas regras de classificação sejam adicionadas sem modificar o código existente. Cada regra é uma classe independente que pode ser facilmente testada e mantida.

### FluentValidation

A validação de entrada foi implementada com FluentValidation, proporcionando validações expressivas e facilmente testáveis. As regras de validação são declarativas e centralizadas em classes específicas.

### Serilog para Logging

O Serilog foi escolhido por sua capacidade de logging estruturado, facilitando a análise de logs em ambientes de produção. Os logs incluem informações sobre tempo de processamento e quantidade de trades processados.

### Tratamento Global de Erros

Um middleware customizado (`ExceptionHandlingMiddleware`) captura todas as exceções não tratadas, garantindo respostas consistentes e informativas para o cliente da API.

### Performance

O endpoint de análise foi otimizado para processar grandes volumes de dados de forma eficiente, utilizando estruturas de dados apropriadas e evitando alocações desnecessárias. O tempo de processamento é medido e retornado na resposta.

## Extensibilidade

### Adicionando Novas Regras de Classificação

Para adicionar uma nova regra de classificação:

1. Crie uma nova classe que implemente `IClassificationRule`:

```csharp
public class CriticalRiskRule : IClassificationRule
{
    public int Priority => 0; // Maior prioridade
    public RiskCategory Category => RiskCategory.CRITICALRISK;

    public bool Matches(Trade trade)
    {
        return trade.Value >= 10_000_000m;
    }
}
```

2. Adicione o novo enum em `RiskCategory`:

```csharp
public enum RiskCategory
{
    LOWRISK,
    MEDIUMRISK,
    HIGHRISK,
    CRITICALRISK // Nova categoria
}
```

3. Registre a nova regra no container de DI em `DependencyInjection.cs`:

```csharp
services.AddSingleton<IClassificationRule, CriticalRiskRule>();
```

### Modificando Regras Existentes

As regras existentes podem ser modificadas alterando os valores de threshold ou as condições de matching nas classes correspondentes em `Application/Rules/`.

---

**Desenvolvido como parte do Desafio Técnico .NET - API de Classificação de Risco de Trades**
