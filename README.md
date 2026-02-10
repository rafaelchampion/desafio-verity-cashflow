# Desafio Verity: CashFlow

Solução completa para gestão de fluxo de caixa diário, demonstrando arquitetura de Microsserviços, DDD, Clean Code e Resiliência.

## Tecnologias

- **.NET 8** (LTS)
- **Blazor WebAssembly** (Frontend)
- **Entity Framework Core**
- **MassTransit** (Mensageria)
- **PostgreSQL**
- **Redis** (Cache e Idempotência)
- **RabbitMQ**
- **Docker & Docker Compose**
- **Serilog** (Logs Distribuídos)
- **Polly** (Resiliência e Circuit Breaker)

## Arquitetura

A solução é composta por 3 serviços principais e um frontend:

1.  **Keycloak**: Provedor de Identidade (IdP) responsável por autenticação OIDC/OAuth2.
2.  **CashFlow.API**: Serviço Transacional (Escrita). Recebe lançamentos de débito/crédito.
3.  **Consolidated.API**: Serviço de Relatório (Leitura). Processa eventos de lançamento e mantêm o saldo diário atualizado.
4.  **Web**: Interface do usuário amigável (Blazor WASM).

Documentação detalhada em `docs/`:
- [Arquitetura e Decisões](docs/ARCHITECTURE.md)
- [Migração para AWS](docs/AWS_MIGRATION.md)

## Como Rodar Localmente

### Pré-requisitos
- Docker e Docker Compose instalados.
- .NET 8 SDK (opcional, apenas para rodar fora do docker ou rodar testes manualmente).

    https://github.com/rafaelchampion/desafio-verity-cashflow.git
    cd verity-cashflow
    
    ```bash
    docker-compose up --build
    ```
    *Aguarde os serviços de banco de dados inicializem e as migrações serem aplicadas (pode levar alguns minutos).*

3.  **Acesse a Aplicação**
    - **Frontend (Blazor)**: http://localhost:8080
    - **Keycloak (Admin)**: http://localhost:8180
    - **Swagger CashFlow**: http://localhost:5002/swagger
    - **Swagger Consolidated**: http://localhost:5003/swagger

### Credenciais de Teste
- **Usuário da Aplicação**: `user` / `user`
- **Admin do Keycloak**: `admin` / `admin`

## Rodando os Testes

Para executar os testes automatizados (Unitários e Integração):

```bash
dotnet test
```

Para executar o teste de carga personalizado (simula 50 RPS):

```bash
cd tests/Verity.LoadTests
dotnet run
```

## Funcionalidades

- [x] Autenticação (Login/Registro).
- [x] Lançamento de Débitos e Créditos.
- [x] Relatório de Saldo Diário Consolidado (com Cache Redis).
- [x] Resiliência: O sistema de lançamentos continua funcionando mesmo se o consolidador cair. Implementação de Circuit Breaker e Retry no Frontend com Polly.
- [x] Idempotência: Verificação de idempotência na API de CashFlow (Redis + Idempotency-Key) e padrão Inbox/Outbox no consumidor.
- [x] Monitoramento: Health Checks para Banco de Dados, Redis, RabbitMQ e monitoramento de Lag de consumo de eventos.
- [x] Logs Distribuídos: Configuração de Serilog com contexto enriquecido.
