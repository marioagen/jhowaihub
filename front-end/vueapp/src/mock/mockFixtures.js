import { applyKanbanMockData } from "@/services/workflow/kanbanMockData.js";
import { DEFAULT_LLM_MODELS, DEFAULT_MODELS } from "@/services/settings/llmModelsConstants";
import { createMockJwt } from "@/mock/mockJwt.js";
import { MOCK_TENANT, MOCK_USER_EMAIL, MOCK_USER_NAME } from "@/mock/mockConfig.js";

export const MOCK_TOKEN = createMockJwt();
export const MOCK_KEY_MONGO_ACCESS = "mock-local-prototype";

const TEAM_LEGAL = { id: 1, name: "Equipe Jurídico" };
const TEAM_FINANCE = { id: 2, name: "Equipe Financeiro" };
const TEAM_OPS = { id: 3, name: "Equipe Operações" };

const PROFILE_ADMIN = { id: 1, name: "Administrador" };
const PROFILE_ANALYST = { id: 2, name: "Analista" };
const PROFILE_AUDITOR = { id: 3, name: "Auditor" };

const MOCK_USER_REFERENCES = [
    { id: 1, name: MOCK_USER_NAME, email: MOCK_USER_EMAIL },
    { id: 2, name: "Ana Silva", email: "ana.silva@prototype.local" },
    { id: 3, name: "Bruno Costa", email: "bruno.costa@prototype.local" },
    { id: 4, name: "Carla Mendes", email: "carla.mendes@prototype.local" },
    { id: 5, name: "Diego Alves", email: "diego.alves@prototype.local" },
];

function buildMockUsers() {
    return [
        {
            id: 1,
            name: MOCK_USER_NAME,
            email: MOCK_USER_EMAIL,
            active: true,
            lastLoginAt: "2026-06-28T14:30:00.000Z",
            teams: [TEAM_LEGAL, TEAM_OPS],
            profiles: [PROFILE_ADMIN],
        },
        {
            id: 2,
            name: "Ana Silva",
            email: "ana.silva@prototype.local",
            active: true,
            lastLoginAt: "2026-06-27T09:15:00.000Z",
            teams: [TEAM_LEGAL],
            profiles: [PROFILE_ANALYST],
        },
        {
            id: 3,
            name: "Bruno Costa",
            email: "bruno.costa@prototype.local",
            active: true,
            lastLoginAt: "2026-06-26T16:45:00.000Z",
            teams: [TEAM_FINANCE],
            profiles: [PROFILE_ANALYST],
        },
        {
            id: 4,
            name: "Carla Mendes",
            email: "carla.mendes@prototype.local",
            active: true,
            lastLoginAt: "2026-06-25T11:20:00.000Z",
            teams: [TEAM_OPS],
            profiles: [PROFILE_AUDITOR],
        },
        {
            id: 5,
            name: "Diego Alves",
            email: "diego.alves@prototype.local",
            active: false,
            lastLoginAt: "2026-05-10T08:00:00.000Z",
            teams: [TEAM_FINANCE, TEAM_OPS],
            profiles: [PROFILE_ANALYST],
        },
        {
            id: 6,
            name: "Elisa Rocha",
            email: "elisa.rocha@prototype.local",
            active: true,
            lastLoginAt: "2026-06-29T10:05:00.000Z",
            teams: [TEAM_LEGAL],
            profiles: [PROFILE_ADMIN, PROFILE_AUDITOR],
        },
    ];
}

function buildMockTeams() {
    return [
        {
            id: 1,
            name: TEAM_LEGAL.name,
            description: "Revisão contratual e compliance",
            users: MOCK_USER_REFERENCES.filter((user) => [1, 2, 6].includes(user.id)),
            profile: PROFILE_ADMIN,
        },
        {
            id: 2,
            name: TEAM_FINANCE.name,
            description: "Processamento de notas e AP",
            users: MOCK_USER_REFERENCES.filter((user) => [3, 5].includes(user.id)),
            profile: PROFILE_ANALYST,
        },
        {
            id: 3,
            name: TEAM_OPS.name,
            description: "Operações e onboarding de fornecedores",
            users: MOCK_USER_REFERENCES.filter((user) => [1, 4, 5].includes(user.id)),
            profile: PROFILE_ANALYST,
        },
    ];
}

function buildPermissionCatalog() {
    return [
        {
            name: "Dashboard",
            permissions: [{ id: 101, name: "View", module: "Dashboard", action: "View" }],
        },
        {
            name: "Documents",
            permissions: [{ id: 102, name: "View", module: "Documents", action: "View" }],
        },
        {
            name: "Workflow",
            permissions: [{ id: 103, name: "View", module: "Workflow", action: "View" }],
        },
        {
            name: "WorkflowManagement",
            permissions: [{ id: 104, name: "View", module: "WorkflowManagement", action: "View" }],
        },
        {
            name: "Tools",
            permissions: [
                { id: 105, name: "Prompts", module: "Tools", action: "Prompts" },
                { id: 106, name: "Connectors", module: "Tools", action: "Connectors" },
                { id: 107, name: "APIs", module: "Tools", action: "APIs" },
                { id: 108, name: "Quizzes", module: "Tools", action: "Quizzes" },
            ],
        },
        {
            name: "Management",
            permissions: [
                { id: 109, name: "Users", module: "Management", action: "Users" },
                { id: 110, name: "Teams", module: "Management", action: "Teams" },
                { id: 111, name: "Profiles", module: "Management", action: "Profiles" },
            ],
        },
        {
            name: "Auditor",
            permissions: [{ id: 112, name: "View", module: "Auditor", action: "View" }],
        },
    ];
}

function buildMockProfiles() {
    const allPermissionIds = buildPermissionCatalog().flatMap((group) =>
        group.permissions.map((permission) => ({ id: permission.id, name: permission.name })),
    );

    return [
        {
            id: 1,
            name: "Administrador",
            description: "Acesso total simulado",
            users: MOCK_USER_REFERENCES.filter((user) => [1, 6].includes(user.id)),
            permissions: allPermissionIds,
            workflowPermission: [{ stepId: 1, permissionId: 201 }],
        },
        {
            id: 2,
            name: "Analista",
            description: "Análise de documentos e workflows",
            users: MOCK_USER_REFERENCES.filter((user) => [2, 3, 5].includes(user.id)),
            permissions: [
                { id: 102, name: "View" },
                { id: 103, name: "View" },
                { id: 105, name: "Prompts" },
            ],
            workflowPermission: [{ stepId: 2, permissionId: 202 }],
        },
        {
            id: 3,
            name: "Auditor",
            description: "Consulta de trilhas de auditoria",
            users: [{ id: 4, name: "Carla Mendes" }],
            permissions: [
                { id: 102, name: "View" },
                { id: 112, name: "View" },
            ],
            workflowPermission: [],
        },
        {
            id: 99,
            name: "Avanço automático",
            description: "Perfil reservado para etapas sem revisão humana — avança automaticamente via IA",
            users: [],
            permissions: [],
            workflowPermission: [],
        },
    ];
}

function buildMockWorkflows() {
    return [
        {
            id: 1,
            name: "Análise de Contratos",
            description: "Workflow demo para revisão contratual e extração de cláusulas",
            active: true,
            teamId: 1,
            teams: [TEAM_LEGAL],
            created: "2026-01-15T10:00:00.000Z",
            updated: "2026-06-01T14:30:00.000Z",
        },
        {
            id: 2,
            name: "Onboarding de Fornecedores",
            description: "Homologação cadastral e documental de fornecedores",
            active: true,
            teamId: 3,
            teams: [TEAM_OPS],
            created: "2026-02-20T09:00:00.000Z",
            updated: "2026-05-10T11:00:00.000Z",
        },
        {
            id: 3,
            name: "Processamento de Notas Fiscais",
            description: "Validação fiscal e lançamento contábil simulado",
            active: true,
            teamId: 2,
            teams: [TEAM_FINANCE],
            created: "2026-03-05T08:30:00.000Z",
            updated: "2026-06-12T09:00:00.000Z",
        },
        {
            id: 4,
            name: "Due Diligence M&A",
            description: "Análise documental para fusões e aquisições",
            active: true,
            teamId: 1,
            teams: [TEAM_LEGAL, TEAM_FINANCE],
            created: "2026-04-18T15:00:00.000Z",
            updated: "2026-06-20T16:45:00.000Z",
        },
        {
            id: 5,
            name: "Compliance LGPD",
            description: "Validação de conformidade com a Lei Geral de Proteção de Dados",
            active: true,
            teamId: 2,
            teams: [TEAM_FINANCE],
            created: "2026-05-02T10:00:00.000Z",
            updated: "2026-07-01T09:00:00.000Z",
        },
        {
            id: 6,
            name: "Auditoria de Fornecedores",
            description: "Revisão e auditoria de documentação de fornecedores homologados",
            active: true,
            teamId: 3,
            teams: [TEAM_OPS],
            created: "2026-05-20T08:00:00.000Z",
            updated: "2026-07-10T11:30:00.000Z",
        },
    ];
}

function buildMockDocuments() {
    return [
        {
            id: 101,
            name: "Contrato_Prestacao_Servicos_Alpha.pdf",
            type: "PDF",
            pages: 12,
            status: "Processed",
            created: "2026-06-10T08:00:00.000Z",
            owner: MOCK_USER_EMAIL,
            hasBatch: false,
            anonymizationAmount: 2,
            workflowProgress: [
                { workflowName: "Análise de Contratos", currentStep: 3, totalSteps: 5 },
            ],
        },
        {
            id: 102,
            name: "Proposta_Comercial_Beta.docx",
            type: "DOCX",
            pages: 5,
            status: "AwaitingAnalysis",
            created: "2026-06-12T13:20:00.000Z",
            owner: "ana.silva@prototype.local",
            hasBatch: false,
            anonymizationAmount: 0,
            workflowProgress: [
                { workflowName: "Onboarding de Fornecedores", currentStep: 1, totalSteps: 4 },
            ],
        },
        {
            id: 103,
            name: "Nota_Fiscal_Junho_2026.pdf",
            type: "PDF",
            pages: 2,
            status: "InProgress",
            created: "2026-06-15T09:40:00.000Z",
            owner: "bruno.costa@prototype.local",
            hasBatch: false,
            anonymizationAmount: 1,
            workflowProgress: [
                { workflowName: "Processamento de Notas Fiscais", currentStep: 2, totalSteps: 3 },
            ],
        },
        {
            id: 104,
            name: "Pack_Due_Diligence_Gamma.zip",
            type: "ZIP",
            pages: 48,
            status: "Processed",
            created: "2026-06-18T17:00:00.000Z",
            owner: MOCK_USER_EMAIL,
            hasBatch: true,
            anonymizationAmount: 3,
            workflowProgress: [
                { workflowName: "Due Diligence M&A", currentStep: 4, totalSteps: 6 },
                { workflowName: "Análise de Contratos", currentStep: 2, totalSteps: 5 },
            ],
        },
        {
            id: 105,
            name: "Politica_Seguranca_Interna.pdf",
            type: "PDF",
            pages: 8,
            status: "Completed",
            created: "2026-06-22T11:10:00.000Z",
            owner: "carla.mendes@prototype.local",
            hasBatch: false,
            anonymizationAmount: 0,
            workflowProgress: [
                { workflowName: "Análise de Contratos", currentStep: 5, totalSteps: 5 },
            ],
        },
        {
            id: 106,
            name: "Aditivo_Contratual_v2.pdf",
            type: "PDF",
            pages: 4,
            status: "Rejected",
            created: "2026-06-25T14:55:00.000Z",
            owner: "elisa.rocha@prototype.local",
            hasBatch: false,
            anonymizationAmount: 1,
            workflowProgress: [
                { workflowName: "Análise de Contratos", currentStep: 2, totalSteps: 5 },
            ],
        },
    ];
}

function buildMockTools() {
    return [
        {
            id: 1,
            name: "Consulta CNPJ Receita",
            toolType: { id: 1, name: "ApiConnector", apiName: "ApiConnector" },
            inputData: "cnpj: string",
            outputData: "razaoSocial, situacao",
        },
        {
            id: 2,
            name: "Webhook Homologação N8N",
            toolType: { id: 2, name: "N8NConnector", apiName: "N8NConnector" },
            inputData: "fornecedorId, documentoUrl",
            outputData: "statusHomologacao",
        },
        {
            id: 3,
            name: "Parser PDF Nativo",
            toolType: { id: 3, name: "ParserConnector", apiName: "ParserConnector" },
            inputData: "documentId",
            outputData: "textoExtraido",
        },
        {
            id: 4,
            name: "Classificador de Cláusulas",
            toolType: { id: 4, name: "PromptConnector", apiName: "PromptConnector" },
            inputData: "textoContrato",
            outputData: "clausulasCriticas",
        },
        {
            id: 5,
            name: "Integração ERP SAP",
            toolType: { id: 1, name: "ApiConnector", apiName: "ApiConnector" },
            inputData: "notaFiscalPayload",
            outputData: "numeroLancamento",
        },
    ];
}

function buildMockPrompts() {
    return [
        {
            id: 1,
            name: "Resumo executivo",
            description: "Gera resumo executivo de documentos longos com estrutura clara em Markdown",
            content: "Analise o documento e produza um resumo executivo em Markdown.",
            created: "2026-03-01T10:00:00.000Z",
            owner: MOCK_USER_EMAIL,
            ownerName: MOCK_USER_NAME,
            ownerEmail: MOCK_USER_EMAIL,
            isOwner: true,
            active: true,
        },
        {
            id: 2,
            name: "Extração de cláusulas críticas",
            description: "Identifica e classifica riscos contratuais, prazos e penalidades",
            content: "Liste cláusulas críticas, riscos, prazos e recomendações.",
            created: "2026-04-05T10:00:00.000Z",
            owner: MOCK_USER_EMAIL,
            ownerName: MOCK_USER_NAME,
            ownerEmail: MOCK_USER_EMAIL,
            isOwner: true,
            active: true,
        },
        {
            id: 3,
            name: "Validação fiscal NF-e",
            description: "Checa campos obrigatórios, CNPJ, impostos e CFOP em notas fiscais eletrônicas",
            content: "Valide CNPJ, valores, CFOP e impostos da nota fiscal.",
            created: "2026-05-12T10:00:00.000Z",
            owner: "bruno.costa@prototype.local",
            ownerName: "Bruno Costa",
            ownerEmail: "bruno.costa@prototype.local",
            isOwner: false,
            active: true,
        },
        {
            id: 4,
            name: "Checklist LGPD",
            description: "Verifica conformidade de cláusulas de privacidade e base legal LGPD",
            content: "Avalie cláusulas de privacidade e base legal LGPD.",
            created: "2026-06-01T10:00:00.000Z",
            owner: "ana.silva@prototype.local",
            ownerName: "Ana Silva",
            ownerEmail: "ana.silva@prototype.local",
            isOwner: false,
            active: true,
        },
        {
            id: 5,
            name: "Classificação de documentos",
            description: "Categoriza automaticamente documentos por tipo, urgência e área responsável",
            content: "Classifique o documento em: tipo, urgência (alta/média/baixa) e área responsável.",
            created: "2026-06-15T09:30:00.000Z",
            owner: MOCK_USER_EMAIL,
            ownerName: MOCK_USER_NAME,
            ownerEmail: MOCK_USER_EMAIL,
            isOwner: true,
            active: true,
        },
        {
            id: 6,
            name: "Análise de compliance trabalhista",
            description: "Avalia contratos e acordos contra a legislação trabalhista vigente",
            content: "Analise o documento quanto à conformidade com a CLT e regulamentações trabalhistas.",
            created: "2026-07-01T11:00:00.000Z",
            owner: "carlos.mendes@prototype.local",
            ownerName: "Carlos Mendes",
            ownerEmail: "carlos.mendes@prototype.local",
            isOwner: false,
            active: true,
        },
    ];
}

function buildMockQuestions() {
    return [
        { id: 1, description: "O contrato possui cláusula de confidencialidade?", created: "2026-03-10T09:00:00.000Z", emailCreator: MOCK_USER_EMAIL },
        { id: 2, description: "Há multa rescisória definida?", created: "2026-03-11T10:30:00.000Z", emailCreator: MOCK_USER_EMAIL },
        { id: 3, description: "Existe cláusula de SLA com penalidade?", created: "2026-04-01T08:15:00.000Z", emailCreator: "ana.silva@prototype.local" },
        { id: 4, description: "O documento menciona tratamento de dados pessoais?", created: "2026-04-15T14:00:00.000Z", emailCreator: "ana.silva@prototype.local" },
        { id: 5, description: "Há prazo de vigência explícito?", created: "2026-05-02T11:20:00.000Z", emailCreator: "bruno.costa@prototype.local" },
        { id: 6, description: "Existem anexos técnicos referenciados?", created: "2026-05-20T16:45:00.000Z", emailCreator: "bruno.costa@prototype.local" },
    ];
}

function buildMockQuestionnaires() {
    return [
        {
            id: 1,
            title: "Questionário de conformidade LGPD",
            name: "Questionário de conformidade LGPD",
            description: "Checklist para documentos com dados pessoais",
            typeDocName: "Contrato",
            active: true,
            created: "2026-03-15T10:00:00.000Z",
            emailCreator: MOCK_USER_EMAIL,
            questions: [
                { id: 4, description: "O documento menciona tratamento de dados pessoais?" },
                { id: 1, description: "O contrato possui cláusula de confidencialidade?" },
                { id: 5, description: "Há prazo de vigência explícito?" },
            ],
        },
        {
            id: 2,
            title: "Homologação de fornecedor",
            name: "Homologação de fornecedor",
            description: "Perguntas cadastrais e documentais",
            typeDocName: "Proposta",
            active: true,
            created: "2026-04-20T08:30:00.000Z",
            emailCreator: "ana.silva@prototype.local",
            questions: [
                { id: 2, description: "Há multa rescisória definida?" },
                { id: 6, description: "Existem anexos técnicos referenciados?" },
            ],
        },
        {
            id: 3,
            title: "Revisão contratual padrão",
            name: "Revisão contratual padrão",
            description: "Questionário jurídico base",
            typeDocName: "Contrato",
            active: true,
            created: "2026-05-10T14:00:00.000Z",
            emailCreator: "ana.silva@prototype.local",
            questions: [
                { id: 1, description: "O contrato possui cláusula de confidencialidade?" },
                { id: 2, description: "Há multa rescisória definida?" },
                { id: 3, description: "Existe cláusula de SLA com penalidade?" },
                { id: 5, description: "Há prazo de vigência explícito?" },
            ],
        },
    ];
}

function buildMockTemplates() {
    return [
        {
            id: 1,
            name: "API Validação CNPJ",
            description: "Consulta cadastral simulada",
            method: "GET",
            active: true,
            url: "https://api.mock/cnpj/{cnpj}",
        },
        {
            id: 2,
            name: "API Cotação Dólar",
            description: "Retorna cotação do dia",
            method: "GET",
            active: true,
            url: "https://api.mock/fx/usd",
        },
        {
            id: 3,
            name: "API Envio ERP",
            description: "Post de lançamento contábil",
            method: "POST",
            active: true,
            url: "https://api.mock/erp/postings",
        },
    ];
}

function buildAuditorDocumentItems() {
    return buildMockDocuments().map((doc, index) => ({
        documentId: doc.id,
        documentName: doc.name,
        owner: doc.owner,
        isRemoved: index === 5,
        isFinalized: index === 4,
        actionsCount: 8 + index * 3,
        workflows: doc.workflowProgress.map((progress, workflowIndex) => ({
            id: workflowIndex + 1,
            name: progress.workflowName,
        })),
    }));
}

function buildAuditorWorkflowItems() {
    return buildMockWorkflows().map((workflow, index) => ({
        workflowId: workflow.id,
        workflowName: workflow.name,
        teamName: workflow.teams[0]?.name || "—",
        documentCount: 3 + index * 2,
        logsCount: 15 + index * 7,
    }));
}

function buildAuditorUserItems() {
    return buildMockUsers().map((user, index) => ({
        userId: user.id,
        userName: user.name,
        logCount: 10 + index * 4,
        workflowCount: 1 + (index % 3),
        teams: user.teams.map((team) => ({
            teamId: team.id,
            teamName: team.name,
        })),
    }));
}

function buildAuditHistoryEntries(prefix) {
    return [
        {
            userName: MOCK_USER_NAME,
            actionName: "Upload",
            stepName: `${prefix} — Upload`,
            createdAt: "2026-06-10T08:05:00.000Z",
        },
        {
            userName: "Ana Silva",
            actionName: "Analyze",
            stepName: `${prefix} — OCR`,
            createdAt: "2026-06-10T08:20:00.000Z",
        },
        {
            userName: "Bruno Costa",
            actionName: "Approve",
            stepName: `${prefix} — Revisão`,
            createdAt: "2026-06-10T09:00:00.000Z",
        },
        {
            userName: "Carla Mendes",
            actionName: "Comment",
            stepName: `${prefix} — Validação`,
            createdAt: "2026-06-10T10:15:00.000Z",
        },
    ];
}

export const mockState = {
    users: buildMockUsers(),
    teams: buildMockTeams(),
    profiles: buildMockProfiles(),
    workflows: buildMockWorkflows(),
    wizardWorkflows: {},
    documents: buildMockDocuments(),
    prompts: buildMockPrompts(),
    tools: buildMockTools(),
    questionnaires: buildMockQuestionnaires(),
    questions: buildMockQuestions(),
    templates: buildMockTemplates(),
    workflowTemplates: [
        {
            id: "tpl-contract-review",
            name: "Revisão Contratual",
            description: "Template de workflow para análise jurídica",
            category: "Jurídico",
            version: "1.0",
            stepCount: 4,
            toolCount: 2,
            created: "2026-05-01T10:00:00.000Z",
        },
        {
            id: "tpl-invoice-flow",
            name: "Processamento de Notas",
            description: "Template para AP/Financeiro",
            category: "Financeiro",
            version: "1.1",
            stepCount: 5,
            toolCount: 3,
            created: "2026-04-15T10:00:00.000Z",
        },
        {
            id: "tpl-vendor-onboarding",
            name: "Onboarding Fornecedor",
            description: "Homologação documental completa",
            category: "Operações",
            version: "2.0",
            stepCount: 6,
            toolCount: 4,
            created: "2026-03-20T10:00:00.000Z",
        },
    ],
    llmModels: { ...DEFAULT_LLM_MODELS },
};

export function buildPagedResponse(items, params = {}) {
    const search = (params.search || "").trim().toLowerCase();
    let filtered = items;

    if (search) {
        filtered = items.filter((item) =>
            JSON.stringify(item).toLowerCase().includes(search),
        );
    }

    const page = Number(params.page || params.Page || 1);
    const pageSize = Number(params.pageSize || params.PageSize || 10);
    const start = (page - 1) * pageSize;
    const content = filtered.slice(start, start + pageSize);
    const rowCount = filtered.length;
    const pageCount = Math.max(1, Math.ceil(rowCount / pageSize));

    return {
        content,
        items: content,
        currentPage: page,
        pageCount,
        totalPages: pageCount,
        rowCount,
        count: rowCount,
    };
}

export function buildAuditorPagedResponse(items, params = {}) {
    const take = Number(params.take || 10);
    const skip = Number(params.skip || 0);
    const slice = items.slice(skip, skip + take);

    return {
        items: slice,
        hasMore: skip + take < items.length,
    };
}

export function buildToolPagedResponse(items, params = {}) {
    const search = (params.search || "").trim().toLowerCase();
    let filtered = items;

    if (search) {
        filtered = items.filter((item) =>
            JSON.stringify(item).toLowerCase().includes(search),
        );
    }

    const page = Number(params.page || 1);
    const pageSize = Number(params.pageSize || 10);
    const start = (page - 1) * pageSize;

    return {
        items: filtered.slice(start, start + pageSize),
        currentPage: page,
        totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
        totalCount: filtered.length,
    };
}

export function findWorkflowSteps(workflowId) {
    const steps = applyKanbanMockData([], { minSteps: 8 });
    return steps.map((step) => ({ ...step, workflowId: Number(workflowId) || 1 }));
}

export function buildLoginResponse(email = MOCK_USER_EMAIL) {
    return {
        name: MOCK_USER_NAME,
        email,
        tenant: MOCK_TENANT,
        token: MOCK_TOKEN,
        tenants: [],
    };
}

export function buildLlmModelsSettingsResponse() {
    return {
        models: { ...mockState.llmModels },
        availableModels: DEFAULT_MODELS.map((model) => ({ ...model })),
        canEdit: true,
    };
}

export function buildDashboardResponse() {
    return {
        totalTokens: 128450,
        totalPages: 842,
        workflowsAutomatic: 68,
        workflowsManual: 32,
    };
}

export function buildUsageMonthResponse() {
    return [
        { month: "2026-01", tokens: 12000, pages: 80 },
        { month: "2026-02", tokens: 14500, pages: 95 },
        { month: "2026-03", tokens: 16200, pages: 110 },
        { month: "2026-04", tokens: 17800, pages: 120 },
        { month: "2026-05", tokens: 19500, pages: 135 },
        { month: "2026-06", tokens: 21000, pages: 150 },
    ];
}

export function buildStatusList() {
    return [
        { id: 1, name: "AwaitingAnalysis", label: "Esperando Análise", color: "#2b7fff" },
        { id: 2, name: "InProgress", label: "Em Progresso", color: "#f59e0b" },
        { id: 3, name: "Completed", label: "Concluído", color: "#22c55e" },
        { id: 4, name: "Rejected", label: "Rejeitado", color: "#ef4444" },
    ];
}

export function buildPermissions() {
    return [
        { Dashboard: "View" },
        { Documents: "View" },
        { Workflow: "View" },
        { WorkflowManagement: "View" },
        { Tools: "Prompts" },
        { Tools: "Connectors" },
        { Tools: "APIs" },
        { Tools: "Quizzes" },
        { Management: "Users" },
        { Management: "Teams" },
        { Management: "Profiles" },
        { Auditor: "View" },
    ];
}

export function buildPermissionGroups() {
    return buildPermissionCatalog();
}

export function buildWorkflowPermissionGroups() {
    return [
        {
            name: "WorkflowSteps",
            permissions: [
                { id: 201, name: "Editar etapa", stepId: 1 },
                { id: 202, name: "Executar ferramenta", stepId: 2 },
                { id: 203, name: "Aprovar documento", stepId: 3 },
            ],
        },
    ];
}

export function buildCardAnalyzeSteps(cardId) {
    return {
        data: {
            cardId: Number(cardId) || 9000001,
            documentName: mockState.documents[0].name,
            workflowName: mockState.workflows[0].name,
            steps: [
                {
                    id: 1,
                    name: "Upload",
                    status: "Completed",
                    order: 1,
                    output: "Documento recebido com sucesso.",
                },
                {
                    id: 2,
                    name: "OCR",
                    status: "Completed",
                    order: 2,
                    output: "Texto extraído (simulado).",
                },
                {
                    id: 3,
                    name: "Análise IA",
                    status: "InProgress",
                    order: 3,
                    output: null,
                },
            ],
        },
    };
}

export function buildCardHeaderInfo(cardId) {
    return {
        cardId: Number(cardId) || 9000001,
        cardName: mockState.documents[0]?.name ?? "Contrato de Prestação de Serviços.pdf",
        documentName: mockState.documents[0]?.name ?? "Contrato de Prestação de Serviços.pdf",
        workflowName: mockState.workflows[0]?.name ?? "Esteira de Análise Documental",
        workflowId: mockState.workflows[0]?.id ?? 1,
        currentStepOrder: 2,
        statusName: "InProgress",
        status: buildStatusList()[0],
        owner: MOCK_USER_EMAIL,
        percentage: 65,
        documentBatchId: null,
    };
}

export function buildAuditorDocumentSummary(params = {}) {
    return buildAuditorPagedResponse(buildAuditorDocumentItems(), params);
}

export function buildAuditorWorkflowSummary(params = {}) {
    return buildAuditorPagedResponse(buildAuditorWorkflowItems(), params);
}

export function buildAuditorUserSummary(params = {}) {
    return buildAuditorPagedResponse(buildAuditorUserItems(), params);
}

export function buildAuditorDocumentDetail(documentId, workflowId) {
    const document =
        mockState.documents.find((item) => item.id === Number(documentId)) || mockState.documents[0];
    const workflow =
        mockState.workflows.find((item) => item.id === Number(workflowId)) || mockState.workflows[0];

    return {
        documentId: document.id,
        documentName: document.name,
        workflowId: workflow.id,
        workflowName: workflow.name,
        documentHistory: buildAuditHistoryEntries(workflow.name),
    };
}

export function buildAuditorWorkflowDetail(workflowId) {
    const workflow =
        mockState.workflows.find((item) => item.id === Number(workflowId)) || mockState.workflows[0];

    return {
        workflowId: workflow.id,
        workflowName: workflow.name,
        teamName: workflow.teams[0]?.name,
        documentCount: 6,
        logsCount: 24,
        timeline: buildAuditHistoryEntries(workflow.name),
        events: buildAuditHistoryEntries(workflow.name),
    };
}

export function buildAuditorUserDetail(userId) {
    const user = mockState.users.find((item) => item.id === Number(userId)) || mockState.users[0];

    return {
        userId: user.id,
        userName: user.name,
        email: user.email,
        teams: user.teams.map((team) => ({ teamId: team.id, teamName: team.name })),
        actions: buildAuditHistoryEntries(user.name).map((entry, index) => ({
            ...entry,
            id: index + 1,
            workflowName: mockState.workflows[index % mockState.workflows.length].name,
        })),
    };
}

// ── Tools audit mock ─────────────────────────────────────────────────────────

const TOOL_CATEGORIES = ["agent", "connector", "apiTemplate", "questionnaire"];
const TOOL_NAMES = {
    agent: ["Agente de Extração", "Agente de Classificação", "Agente de Resumo"],
    connector: ["Conector HTTP", "Conector N8N", "Conector Parser"],
    apiTemplate: ["Template de Consulta", "Template de Envio", "Template de Validação"],
    questionnaire: ["Questionário de Triagem", "Questionário de Compliance", "Questionário de KYC"],
};
const TOOL_USER_NAMES = MOCK_USER_REFERENCES.map((user) => user.name);

function findMockUserDisplayName(emailOrName) {
    const byEmail = MOCK_USER_REFERENCES.find((user) => user.email === emailOrName);
    if (byEmail) return byEmail.name;
    const byName = MOCK_USER_REFERENCES.find((user) => user.name === emailOrName);
    if (byName) return byName.name;
    if (typeof emailOrName === "string" && emailOrName.includes("@")) {
        const fromState = mockState.users.find((user) => user.email === emailOrName);
        return fromState?.name ?? emailOrName.split("@")[0];
    }
    return emailOrName;
}

function findToolAuditItem(toolId) {
    return buildToolAuditItems().find((item) => item.toolId === Number(toolId));
}

function buildToolAuditItems() {
    return TOOL_CATEGORIES.flatMap((cat, ci) =>
        TOOL_NAMES[cat].map((name, ni) => ({
            toolId: ci * 10 + ni + 1,
            toolName: name,
            category: cat,
            eventCount: cat === "connector" || cat === "apiTemplate" ? 6 + ni : 3 + ni,
            lastEvent: new Date(Date.now() - (ci * 3 + ni) * 3_600_000).toISOString(),
        }))
    );
}

function buildToolAuditEvents(toolId) {
    const actions = ["updated", "created", "updated"];
    const details = [
        "Configuração de parâmetros atualizada",
        "Ferramenta criada no sistema",
        "Prompt principal editado",
    ];
    return actions.map((action, i) => ({
        eventId: toolId * 100 + i,
        action,
        userName: TOOL_USER_NAMES[i % TOOL_USER_NAMES.length],
        detail: details[i],
        createdAt: new Date(Date.now() - i * 86_400_000).toISOString(),
    }));
}

function buildToolApiCallEvents(toolId) {
    const methods = ["GET", "POST", "PUT"];
    const endpoints = ["/api/Tool", "/api/Connector/Execute", "/api/Template/Run"];
    return methods.map((method, i) => ({
        eventId: toolId * 1000 + i,
        action: "apiCall",
        userName: TOOL_USER_NAMES[i % TOOL_USER_NAMES.length],
        detail: "Execução de API do Woopi AI registrada",
        method,
        endpoint: endpoints[i],
        statusCode: i === 2 ? 400 : 200,
        durationMs: 95 + i * 48,
        createdAt: new Date(Date.now() - i * 3_600_000).toISOString(),
    }));
}

export function buildAuditorToolsSummary(params = {}) {
    return buildAuditorPagedResponse(buildToolAuditItems(), params);
}

export function buildAuditorToolsDetail(toolId) {
    const id = Number(toolId) || 1;
    const meta = findToolAuditItem(id);
    const baseEvents = buildToolAuditEvents(id);
    if (meta?.category === "connector" || meta?.category === "apiTemplate") {
        return [...baseEvents, ...buildToolApiCallEvents(id)];
    }
    return baseEvents;
}

// ── System audit mock ─────────────────────────────────────────────────────────

const SYSTEM_EVENT_TEMPLATES = [
    { eventType: "accessLogin", buildDetail: (actor) => `${actor} entrou no Woopi AI` },
    { eventType: "accessLogout", buildDetail: (actor) => `${actor} encerrou a sessão` },
    {
        eventType: "userCreated",
        buildDetail: (actor) => `${actor} criou o usuário "Carla Mendes"`,
    },
    {
        eventType: "userUpdated",
        buildDetail: (actor) => `${actor} atualizou dados do usuário "Bruno Costa"`,
    },
    {
        eventType: "userDeleted",
        buildDetail: (actor) => `${actor} excluiu o usuário "Diego Alves"`,
    },
    {
        eventType: "teamCreated",
        buildDetail: (actor) => `${actor} criou o time "Equipe Financeiro"`,
    },
    {
        eventType: "teamUpdated",
        buildDetail: (actor) => `${actor} alterou membros do time "Equipe Jurídico"`,
    },
    {
        eventType: "teamDeleted",
        buildDetail: (actor) => `${actor} removeu o time "Equipe Operações"`,
    },
    {
        eventType: "permissionCreated",
        buildDetail: (actor) => `${actor} criou o perfil de permissão "Auditor"`,
    },
    {
        eventType: "permissionUpdated",
        buildDetail: (actor) => `${actor} alterou permissões do perfil "Analista"`,
    },
    {
        eventType: "permissionDeleted",
        buildDetail: (actor) => `${actor} excluiu o perfil de permissão "Convidado"`,
    },
    {
        eventType: "apiKeyCreated",
        buildDetail: (actor) => `${actor} criou a chave de API "Integração ERP"`,
    },
    {
        eventType: "apiKeyCreated",
        buildDetail: (actor) => `${actor} criou a chave de API "Webhook Parceiro"`,
    },
    {
        eventType: "apiKeyDeleted",
        buildDetail: (actor) => `${actor} excluiu a chave de API "Token Legado"`,
    },
];

function buildSystemEvents() {
    const userEmails = MOCK_USER_REFERENCES.map((user) => user.email);
    const events = [];
    let id = 1;
    for (let i = 0; i < 33; i++) {
        const template = SYSTEM_EVENT_TEMPLATES[i % SYSTEM_EVENT_TEMPLATES.length];
        const email = userEmails[i % userEmails.length];
        const displayName = findMockUserDisplayName(email);
        const isAccess = template.eventType.startsWith("access");
        events.push({
            eventId: id++,
            eventType: template.eventType,
            userName: displayName,
            detail: template.buildDetail(displayName),
            endpoint: null,
            method: null,
            statusCode: null,
            durationMs: null,
            ipAddress: isAccess ? `192.168.1.${(i * 7 + 10) % 254}` : null,
            createdAt: new Date(Date.now() - i * 1_800_000).toISOString(),
        });
    }
    return events;
}

export function buildAuditorSystemEvents(params = {}) {
    const all = buildSystemEvents();
    return buildAuditorPagedResponse(all, params);
}

// ─────────────────────────────────────────────────────────────────────────────
export function buildTypeDocList() {
    return [
        { id: 1, name: "Contrato", created: "2026-01-10T08:00:00.000Z", emailCreator: MOCK_USER_EMAIL },
        { id: 2, name: "Proposta", created: "2026-01-15T09:00:00.000Z", emailCreator: MOCK_USER_EMAIL },
        { id: 3, name: "Nota Fiscal", created: "2026-02-01T10:00:00.000Z", emailCreator: "ana.silva@prototype.local" },
        { id: 4, name: "Política Interna", created: "2026-02-20T11:00:00.000Z", emailCreator: "ana.silva@prototype.local" },
        { id: 5, name: "Aditivo", created: "2026-03-05T12:00:00.000Z", emailCreator: "bruno.costa@prototype.local" },
    ];
}

export function buildToolTypes() {
    return [
        { id: 1, name: "API", label: "API Connector", apiName: "ApiConnector" },
        { id: 2, name: "N8N", label: "N8N", apiName: "N8NConnector" },
        { id: 3, name: "Parser", label: "Parser", apiName: "ParserConnector" },
        { id: 4, name: "Prompt", label: "Prompt", apiName: "PromptConnector" },
    ];
}

export function buildTenantPlan() {
    return {
        name: "Prototype",
        wtcIncluded: 100000,
        description: "Plano simulado para protótipo local",
    };
}

export function buildWorkflowPhase(workflowId, phase) {
    const workflow =
        mockState.workflows.find((item) => item.id === Number(workflowId)) || mockState.workflows[0];

    return {
        workflowId: workflow.id,
        name: workflow.name,
        description: workflow.description,
        phase,
        steps: findWorkflowSteps(workflow.id),
        teams: workflow.teams,
    };
}

export function buildEmptyBlob() {
    return new Blob(["Conteúdo simulado do documento."], { type: "application/pdf" });
}

export function buildSuccessBody(data = true) {
    return data;
}

export function buildAnonymizationList(documentId) {
    return {
        data: [
            {
                id: 1,
                documentId: Number(documentId) || 101,
                field: "CPF",
                status: "Completed",
                createdAt: "2026-06-10T08:30:00.000Z",
            },
            {
                id: 2,
                documentId: Number(documentId) || 101,
                field: "E-mail",
                status: "Completed",
                createdAt: "2026-06-10T08:35:00.000Z",
            },
        ],
    };
}

// ── Card tool outputs export mock ─────────────────────────────────────────────

export function buildCardToolOutputsExport(cardId) {
    const id = Number(cardId) || 9000001;
    const documentName = mockState.documents[0]?.name ?? "Contrato de Prestação de Serviços.pdf";

    const rows = [
        {
            cardId: id,
            documentName,
            stepName: "Extração de Dados",
            toolName: "Agente OCR",
            executionDate: "2026-06-15T08:10:00.000Z",
            output: JSON.stringify({ paginas: 12, texto: "Conteúdo extraído do documento via OCR..." }),
        },
        {
            cardId: id,
            documentName,
            stepName: "Extração de Dados",
            toolName: "Prompt Extrator",
            executionDate: "2026-06-15T08:12:30.000Z",
            output: JSON.stringify({ cnpj: "12.345.678/0001-90", razaoSocial: "Empresa Exemplo LTDA", valor: "R$ 150.000,00" }),
        },
        {
            cardId: id,
            documentName,
            stepName: "Análise de Conformidade",
            toolName: "Prompt Validador",
            executionDate: "2026-06-15T08:15:00.000Z",
            output: JSON.stringify({ conformidade: true, observacoes: "Documento dentro dos padrões esperados.", risco: "Baixo" }),
        },
        {
            cardId: id,
            documentName,
            stepName: "Análise de Conformidade",
            toolName: "API de Validação Externa",
            executionDate: "2026-06-15T08:16:45.000Z",
            output: JSON.stringify({ status: "OK", score: 98, detalhe: "Validação via API concluída com sucesso." }),
        },
        {
            cardId: id,
            documentName,
            stepName: "Geração de Relatório",
            toolName: "Agente N8N",
            executionDate: "2026-06-15T08:20:00.000Z",
            output: "Relatório gerado e enviado para o repositório de saída.",
        },
    ];

    return rows;
}
