# Arquitetura da Solução

## Visão Geral

A solução adota uma arquitetura de **Microsserviços** baseada em eventos (Event-Driven Architecture), garantindo desacoplamento, escalabilidade e resiliência, conforme solicitado nos requisitos. O sistema é composto por serviços autônomos que se comunicam de forma assíncrona para processamento de negócios e síncrona para leitura de dados.

## Diagrama de Componentes (C4 Model - Container Level Simplificado)

```mermaid
graph TD
    User((Usuário))
    Client[Frontend Blazor WebAssembly]

    subgraph "Cluster Docker / Nuvem"
        Keycloak[Keycloak - Servidor de Identidade]
        CashFlowAPI[API do Serviço de Fluxo de Caixa]
        ConsolidatedAPI[API do Serviço Consolidado]
        Worker[Worker do Serviço Consolidado]

        DB_CashFlow[(PostgreSQL - Fluxo de Caixa)]
        DB_Consolidated[(PostgreSQL - Consolidado)]

        Redis[(Redis - Cache/Idempotência)]
        MessageBroker{RabbitMQ}
    end

    User -->|HTTPS| Client
    Client -->|OIDC/OAuth2| Keycloak
    Client -->|HTTPS/REST| CashFlowAPI
    Client -->|HTTPS/REST| ConsolidatedAPI

    Keycloak -->|Valida Token JWT| CashFlowAPI
    Keycloak -->|Valida Token JWT| ConsolidatedAPI
    CashFlowAPI --> DB_CashFlow
    CashFlowAPI -->|Verifica Idempotência| Redis
    CashFlowAPI -->|Publica TransactionCreated| MessageBroker

    MessageBroker -->|Consome TransactionCreated| Worker
    Worker -->|Atualiza Saldo| DB_Consolidated
    ConsolidatedAPI -->|Lê Saldo (Cache Miss)| DB_Consolidated
    ConsolidatedAPI -->|Lê Cache| Redis
```

## Padrões Adotados

### 1. Domain-Driven Design (DDD)
Cada serviço possui sua própria camada de Domínio, isolada e pura.
- **Camadas**:
    - `Domain`: Entidades, Value Objects, Interfaces de Repositório.
    - `Application`: Casos de uso (Services/Handlers), DTOs.
    - `Infrastructure`: Implementação de persistência (EF Core), Mensageria (MassTransit).
    - `API`: Controllers, Middlewares, Configuração.

### 2. SOLID & Clean Code
- Injeção de Dependência.
- Single Responsibility Principle em todas as classes.
- Validação de DTOs com `EnumDataType`.

### 3. Event-Driven Architecture (EDA)
- Comunicação assíncrona entre o serviço Transacional (`CashFlow`) e o Relatório (`Consolidated`) para garantir que o lançamento nunca falhe se o relatório estiver fora do ar.
- **Resiliência**: Uso de filas (RabbitMQ) garante que mensagens sejam processadas eventualmente.

## Decisões Técnicas

- **Framework**: .NET 8+.
- **ORM**: Entity Framework Core (Code First).
- **Mensageria**: MassTransit (Abstração robusta sobre RabbitMQ).
- **Frontend**: Blazor WebAssembly para SPA rica e performática em C#.
- **Banco de Dados**: PostgreSQL.
- **Cache Distribuído**: Redis (Cache-Aside em Reports e Idempotency Key Store).
- **Resiliência HTTP**: Polly (Retry e Circuit Breaker) no Frontend.
- **Observabilidade**: Serilog (Logs Estruturados) e Health Checks (DB, Redis, RabbitMQ, Lag).
- **Testes**: xUnit + Moq + FluentAssertions + Teste de Carga Customizado.
