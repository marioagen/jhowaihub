# Variáveis globais

## Objetivo

Centralizar constantes e segredos por tenant e reutilizá-los por meio da sintaxe
`{{global:nome}}`. Ferramentas armazenam apenas a referência. O valor é resolvido no
backend no momento da execução.

A opção **Disponível nas configurações das ferramentas** não cria uma variável de
ambiente do sistema operacional ou do container. Ela controla se a variável pode ser
selecionada e resolvida em campos de configuração das ferramentas.

## Comportamento do protótipo

O frontend persiste dados simulados no `localStorage`, isolados pelo tenant atual.
Cada registro possui:

- `name`: identificador usado em `{{global:nome}}`;
- `value`: fonte única do valor;
- `valueType`: `common` ou `secret`;
- `availableAsEnvironment`: disponibilidade nas configurações das ferramentas;
- `description`, `createdBy`, `createdAt` e `updatedAt`.

Variáveis comuns podem ser inseridas em URL, query, headers, body e credenciais.
Segredos podem ser inseridos somente em headers, body e credenciais. O protótipo não
oferece segredos em URL ou query string, pois esses valores costumam ser registrados
em logs, histórico e ferramentas de observabilidade.

## Modelo persistente futuro

O recurso deve ser tenant-aware e ter unicidade case-insensitive para
`(TenantId, Name)`.

| Campo | Regra |
| --- | --- |
| `Id` | Identificador imutável |
| `TenantId` | Obrigatório; obtido do contexto autenticado |
| `Name` | Começa com letra; aceita letras, números e underscore |
| `EncryptedValue` | Valor criptografado em repouso; nunca retornado em listagens |
| `ValueType` | `Common` ou `Secret` |
| `AvailableAsEnvironment` | Autoriza uso nas configurações das ferramentas |
| `Description` | Metadado opcional |
| `CreatedBy` | Criador autorizado a alterar o registro |
| `CreatedAt`, `UpdatedAt` | Auditoria temporal |

O nome pode ser alterado somente após validar referências existentes. A abordagem
preferencial é impedir a renomeação enquanto houver consumidores ou oferecer uma
operação explícita que atualize todas as referências de forma transacional.

## API futura

- `GET /GlobalVariable`: retorna metadados, placeholder e permissões; nunca o valor.
- `GET /GlobalVariable/Available?context=header`: retorna o catálogo filtrado para os seletores.
- `POST /GlobalVariable`: cria o registro e criptografa o valor.
- `PUT /GlobalVariable`: altera metadados e, quando informado, substitui o valor.
- `DELETE /GlobalVariable/{id}`: exige autoria e valida referências antes de excluir.
- `POST /GlobalVariable/{id}/copy`: operação opcional, restrita e auditada, caso a cópia do valor permaneça como requisito.

Tenant, usuário e permissões devem vir do contexto autenticado. Identificadores
enviados pelo cliente não podem alterar o escopo do tenant.

## Resolução em runtime

Um único serviço de aplicação deve resolver placeholders para templates de API,
conectores e demais ferramentas.

1. Analisar placeholders com parser dedicado para `{{global:nome}}`.
2. Coletar nomes únicos e consultar todos em uma única operação pelo tenant atual.
3. Validar `AvailableAsEnvironment` e o contexto de destino.
4. Descriptografar somente os valores necessários.
5. Substituir os tokens exatos sem resolver recursivamente o conteúdo resultante.
6. Falhar de forma fechada quando uma variável estiver ausente, desabilitada ou
   incompatível com o contexto.
7. Descartar os valores descriptografados após montar a requisição.

O resolvedor não deve usar uma sequência de `string.Replace`, pois nomes sobrepostos,
conteúdo recursivo e tokens malformados produzem resultados ambíguos. O parser deve
reconhecer apenas o formato canônico e preservar placeholders de outras origens.

## Segurança e observabilidade

- Nunca registrar valor resolvido, header de autenticação ou body contendo segredo.
- Redigir segredos em respostas de teste, exceptions, auditoria e telemetria.
- Auditar criação, alteração, cópia e exclusão sem incluir o valor.
- Não enviar o catálogo de valores ao frontend; seletores recebem somente metadados.
- Criptografar valores com um provedor de chaves externo ou serviço equivalente.
- Não armazenar valores descriptografados em cache distribuído.
- Aplicar autorização no backend mesmo que a interface desabilite ações.

## Integrações previstas

O resolvedor deve ser aplicado antes da execução em:

- `ApiTemplateRequestCheckHandler`, nos campos URL, query, headers e body;
- execução persistida de API templates;
- validação e execução de conectores;
- ferramentas futuras que declarem explicitamente um contexto compatível.

`ApiTemplateRequestCheckRequestAssembler` já substitui variáveis fornecidas pelo
chamador e serve como referência de superfície, mas não deve acessar armazenamento ou
descriptografar segredos. A resolução global pertence a um serviço anterior à montagem
da requisição.

## Testes mínimos futuros

- isolamento entre tenants e autorização por criador;
- unicidade de nome sem diferença de maiúsculas/minúsculas;
- resolução em URL, query, header, body e credencial;
- bloqueio de segredo em URL e query;
- variável ausente ou desabilitada;
- múltiplas referências e nomes sobrepostos;
- conteúdo com outro placeholder sem resolução recursiva;
- ausência de valores secretos em logs, responses e auditoria.