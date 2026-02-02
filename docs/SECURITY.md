# Segurança

A aplicação utiliza o fluxo OIDC/OAuth2 através de tokens JWT (JSON Web Tokens), gerenciados pelo **Keycloak**.

## Fluxo de Autenticação

1.  **Autenticação Centralizada**: O usuário é redirecionado para o Keycloak para realizar login ou registro.
2.  **Tokens**: O Keycloak emite `id_token`, `access_token` e `refresh_token`.
3.  **Acesso Protegido**:
    - O cliente (Frontend) envia o token no header `Authorization: Bearer <token>`.
    - Os serviços validam a assinatura (RS256 via JWKS do Keycloak) e a expiração.

## Configuração

- **Identity Provider**: Keycloak rodando no container `verity_keycloak`.
- **Validation**: As APIs (`CashFlow` e `Consolidated`) validam o token JWT contra o endpoint do Keycloak.

## Decisões

- Utilização do Keycloak como padrão de indústria para gestão de identidades (IAM).
- O token contém `sub` (UserID) e `email`, permitindo que os serviços downstream identifiquem o usuário.
