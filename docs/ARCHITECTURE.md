# Arquitetura da Solução

## Visão Geral

A solução adota uma arquitetura de **Microsserviços** baseada em eventos (Event-Driven Architecture), garantindo desacoplamento, escalabilidade e resiliência, conforme solicitado nos requisitos. O sistema é composto por serviços autônomos que se comunicam de forma assíncrona para processamento de negócios e síncrona para leitura de dados.

## Diagrama de Componentes (C4 Model - Container Level Simplificado)

```mermaid
graph TD
    User((Usuário))
    Client[Frontend Blazor WebAssembly]

    subgraph "Cluster Docker / Nuvem"
        IdentityAPI[Identity Service API]
        CashFlowAPI[CashFlow Service API]
        ConsolidatedAPI[Consolidated Service API]
        Worker[Consolidated Worker]

        DB_Auth[(PostgreSQL - Auth)]
        DB_CashFlow[(PostgreSQL - CashFlow)]
        DB_Consolidated[(PostgreSQL - Consolidated)]

        MessageBroker{RabbitMQ}
    end

    User -->|HTTPS| Client
    Client -->|HTTPS/REST| IdentityAPI
    Client -->|HTTPS/REST| CashFlowAPI
    Client -->|HTTPS/REST| ConsolidatedAPI

    IdentityAPI --> DB_Auth
    CashFlowAPI --> DB_CashFlow
    CashFlowAPI -->|Publishes TransactionCreated| MessageBroker

    MessageBroker -->|Consumes TransactionCreated| Worker
    Worker -->|Updates Balance| DB_Consolidated
    ConsolidatedAPI -->|Reads Balance| DB_Consolidated
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
- Código auto-explicativo, com nomes de métodos e variáveis semânticos.

### 3. Event-Driven Architecture (EDA)
- Comunicação assíncrona entre o serviço Transacional (`CashFlow`) e o Relatório (`Consolidated`) para garantir que o lançamento nunca falhe se o relatório estiver fora do ar.
- **Resiliência**: Uso de filas (RabbitMQ) garante que mensagens sejam processadas eventualmente.

## Decisões Técnicas

- **Framework**: .NET 8+.
- **ORM**: Entity Framework Core (Code First).
- **Mensageria**: MassTransit (Abstração robusta sobre RabbitMQ).
- **Frontend**: Blazor WebAssembly para SPA rica e performática em C#.
- **Banco de Dados**: PostgreSQL (containers isolados ou schemas separados).
- **Testes**: xUnit + Moq + FluentAssertions.
