# Estratégia de Migração para AWS

Este documento descreve como a infraestrutura atual (baseada em `docker-compose`) seria migrada para um ambiente de produção robusto na AWS (Amazon Web Services).

## 1. Computação (Containers)

Atualmente rodamos os serviços em containers Docker locais.

**AWS Target: Amazon ECS (Elastic Container Service) com Fargate**
- **Por que?**: O Fargate remove a necessidade de gerenciar instâncias EC2 (Serverless for containers).
- **Estrutura**:
    - Cada serviço (`CashFlow`, `Consolidated`, `Frontend`) seria uma **Task Definition**.
    - O `Frontend` (Blazor WASM) poderia, alternativamente, ser hospedado no **Amazon S3 + CloudFront** (se compilado estático) ou ECS se for Blazor Server (neste caso, é WASM, então S3+CloudFront é mais barato e performático).
- **Auto Scaling**: Configurado baseando-se em uso de CPU/Memória.

## 2. Banco de Dados

Atualmente rodamos PostgreSQL em containers.

**AWS Target: Amazon RDS for PostgreSQL**
- **Por que?**: Gerenciado, backups automáticos, Multi-AZ para alta disponibilidade.
- **Configuração**:
    - Uma instância RDS robusta.
    - Criação de bases lógicas separadas (`auth_db`, `cashflow_db`, `consolidated_db`) dentro da mesma instância ou instâncias separadas dependendo da carga.

## 3. Mensageria e Cache

Atualmente rodamos RabbitMQ e Redis em containers.

**Mensageria: Amazon MQ for RabbitMQ**
- **Por que?**: Serviço gerenciado compatível com RabbitMQ. Não exige mudança de código no MassTransit (apenas connection string).
- **Alternativa**: Migrar para **Amazon SQS/SNS**.

**Cache: Amazon ElastiCache for Redis**
- **Por que?**: Serviço gerenciado, alta disponibilidade, suporte a cluster mode se necessário.
- **Uso**: Cache de relatórios consolidados e verificação de idempotência (Idempotency Key).

## 4. Segurança & Networking

- **VPC**: Todos os serviços backend em subnets privadas.
- **Load Balancer**: **Application Load Balancer (ALB)** expondo os serviços publicamente (HTTPS).
- **Secrets**: Senhas de banco e chaves JWT armazenadas no **AWS Secrets Manager** ou **AWS Systems Manager Parameter Store**, injetadas como variáveis de ambiente na execução das Tasks do ECS.

## 5. Fluxo de Deploy (CI/CD)

- **CodeCommit / GitHub**: Repositório.
- **CodeBuild**: Build das imagens Docker e Push para o **Amazon ECR (Elastic Container Registry)**.
- **CodePipeline**: Orquestração do deploy para atualizar os serviços no ECS.

## Resumo do "De-Para"

| Componente Local | AWS Service | Observação |
| :--- | :--- | :--- |
| Docker Compose | AWS ECS (Fargate) | Orquestração de Containers |
| PostgreSQL Container | Amazon RDS for PostgreSQL | Banco de dados gerenciado |
| RabbitMQ Container | Amazon MQ (RabbitMQ engine) | Broker de Mensagem |
| Redis Container | Amazon ElastiCache for Redis | Cache e Store distribuído |
| Blazor Host | S3 + CloudFront (CDN) | Hospedagem estática global |
| App Settings (env) | AWS Parameter Store | Gestão de Configuração e Segredos |
