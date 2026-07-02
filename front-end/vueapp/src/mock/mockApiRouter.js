import {
    buildAnonymizationList,
    buildAuditorDocumentDetail,
    buildAuditorDocumentSummary,
    buildAuditorUserDetail,
    buildAuditorUserSummary,
    buildAuditorWorkflowDetail,
    buildAuditorWorkflowSummary,
    buildCardAnalyzeSteps,
    buildCardHeaderInfo,
    buildDashboardResponse,
    buildEmptyBlob,
    buildLlmModelsSettingsResponse,
    buildLoginResponse,
    buildPagedResponse,
    buildPermissionGroups,
    buildStatusList,
    buildSuccessBody,
    buildTenantPlan,
    buildToolPagedResponse,
    buildToolTypes,
    buildTypeDocList,
    buildUsageMonthResponse,
    buildWorkflowPermissionGroups,
    buildWorkflowPhase,
    findWorkflowSteps,
    mockState,
} from "@/mock/mockFixtures.js";

function normalizePath(url = "") {
    let path = url;
    if (path.includes("://")) {
        try {
            path = new URL(url).pathname;
        } catch {
            path = url;
        }
    }
    path = path.replace(/^\/api/, "").split("?")[0];
    if (!path.startsWith("/")) {
        path = `/${path}`;
    }
    return path.replace(/\/+$/, "") || "/";
}

function matchPath(path, pattern) {
    const regex = new RegExp(`^${pattern.replace(/\//g, "\\/").replace(/:\w+/g, "[^/]+")}$`);
    return regex.test(path);
}

function parseIdFromPath(path, segmentIndex) {
    const parts = path.split("/").filter(Boolean);
    const value = parts[segmentIndex];
    const numeric = Number(value);
    return Number.isNaN(numeric) ? value : numeric;
}

function resolveMockRequest(config) {
    const method = (config.method || "get").toUpperCase();
    const path = normalizePath(config.url);
    const params = config.params || {};
    const body = config.data;

    if (method === "POST" && path === "/Account/Login") {
        const credentials = typeof body === "string" ? JSON.parse(body) : body;
        return buildLoginResponse(credentials?.email);
    }
    if (method === "POST" && path === "/Account/Login-sso") {
        return buildLoginResponse();
    }
    if (method === "POST" && path === "/Account/refresh-token") {
        return { token: buildLoginResponse().token };
    }
    if (method === "POST" && path === "/Account/logout") {
        return buildSuccessBody(true);
    }
    if (method === "GET" && path === "/Account/clientId") {
        return "mock-client-id";
    }

    if (method === "GET" && path === "/Settings/llm-models") {
        return buildLlmModelsSettingsResponse();
    }
    if (method === "PUT" && path === "/Settings/llm-models") {
        const payload = typeof body === "string" ? JSON.parse(body) : body;
        mockState.llmModels = { ...mockState.llmModels, ...payload?.models };
        return buildLlmModelsSettingsResponse();
    }

    if (method === "GET" && matchPath(path, "/Tenant/InitializeTenant/:tenant")) {
        return { keyMongoAccess: "mock-local-prototype", success: true };
    }
    if (method === "GET" && matchPath(path, "/Tenant/FindPlanByName/:tenant")) {
        return buildTenantPlan();
    }

    if (method === "GET" && (path === "/User/Paged" || path === "/User/Paged/")) {
        return buildPagedResponse(mockState.users, params);
    }
    if (method === "GET" && path === "/User") {
        return mockState.users;
    }
    if (method === "GET" && matchPath(path, "/User/:email")) {
        const email = decodeURIComponent(path.split("/").pop());
        return (
            mockState.users.find((user) => user.email === email) || {
                ...mockState.users[0],
                email,
                password: "",
                confirmedPassword: "",
            }
        );
    }
    if (method === "GET" && matchPath(path, "/User/Team/:teamId")) {
        return mockState.users;
    }
    if (method === "POST" && path === "/User/Team/Query") {
        return mockState.users;
    }
    if (method === "POST" && path === "/User/IsEmailInUse") {
        return false;
    }
    if (method === "POST" && path === "/User") {
        return buildSuccessBody(true);
    }
    if (method === "PUT" && path === "/User") {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && path === "/User/DeactivateByIds") {
        return buildSuccessBody(true);
    }

    if (method === "GET" && (path === "/Team/Paged" || path === "/Team/Paged/")) {
        return buildPagedResponse(mockState.teams, params);
    }
    if (method === "GET" && path === "/Team") {
        return mockState.teams;
    }
    if (method === "GET" && path === "/Team/Simple") {
        return mockState.teams.map(({ id, name }) => ({ id, name }));
    }
    if (method === "GET" && matchPath(path, "/Team/:teamId")) {
        const teamId = parseIdFromPath(path, 1);
        return mockState.teams.find((team) => team.id === teamId) || mockState.teams[0];
    }
    if (method === "POST" && path === "/Team") {
        return buildSuccessBody(true);
    }
    if (method === "PUT" && path === "/Team") {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && path === "/Team/DeleteByIds") {
        return buildSuccessBody(true);
    }

    if (method === "GET" && (path === "/Profile/Paged" || path === "/Profile/Paged/")) {
        return buildPagedResponse(mockState.profiles, params);
    }
    if (method === "GET" && path === "/Profile") {
        return mockState.profiles;
    }
    if (method === "GET" && matchPath(path, "/Profile/:id")) {
        const profileId = parseIdFromPath(path, 1);
        return mockState.profiles.find((profile) => profile.id === profileId) || mockState.profiles[0];
    }
    if (method === "POST" && path === "/Profile") {
        return { success: true, data: mockState.profiles[0] };
    }
    if (method === "PUT" && path === "/Profile") {
        return { success: true, data: mockState.profiles[0] };
    }
    if (method === "DELETE" && path === "/Profile") {
        return buildSuccessBody(true);
    }

    if (method === "GET" && (path === "/Permission/FindAll" || path === "/Permission/FindAll/")) {
        return buildPermissionGroups();
    }
    if (method === "GET" && (path === "/Permission/Workflow" || path === "/Permission/Workflow/")) {
        return buildWorkflowPermissionGroups();
    }

    if (method === "GET" && path === "/Workflow/List") {
        return buildPagedResponse(mockState.workflows, params);
    }
    if (method === "GET" && path === "/Workflow") {
        return mockState.workflows;
    }
    if (method === "GET" && matchPath(path, "/Workflow/Users/:email")) {
        return mockState.workflows;
    }
    if (method === "GET" && matchPath(path, "/Workflow/Teams/:teamId")) {
        return mockState.workflows;
    }
    if (method === "GET" && matchPath(path, "/Workflow/:workflowId/Steps")) {
        const workflowId = parseIdFromPath(path, 1);
        return findWorkflowSteps(workflowId);
    }
    if (method === "GET" && matchPath(path, "/Workflow/Phase1/:workflowId")) {
        return buildWorkflowPhase(parseIdFromPath(path, 2), 1);
    }
    if (method === "GET" && matchPath(path, "/Workflow/Phase2/:workflowId")) {
        return buildWorkflowPhase(parseIdFromPath(path, 2), 2);
    }
    if (method === "GET" && matchPath(path, "/Workflow/Phase3/:workflowId")) {
        return buildWorkflowPhase(parseIdFromPath(path, 2), 3);
    }
    if (method === "GET" && matchPath(path, "/Workflow/Step/:stepId")) {
        const steps = findWorkflowSteps(1);
        const stepId = parseIdFromPath(path, 2);
        return steps.find((step) => step.id === stepId) || steps[0];
    }
    if (method === "GET" && matchPath(path, "/Workflow/Step/:stepId/HasConstraints")) {
        return false;
    }
    if (method === "GET" && matchPath(path, "/Workflow/CountDocuments/:workflowId")) {
        return mockState.documents.length;
    }
    if (method === "GET" && path === "/Workflow/Templates") {
        return mockState.workflowTemplates;
    }
    if (method === "GET" && matchPath(path, "/Workflow/:workflowId/Export")) {
        return { packageJson: "{}", workflowName: "Export Demo" };
    }
    if (method === "GET" && matchPath(path, "/Workflow/:workflowId")) {
        const workflowId = parseIdFromPath(path, 1);
        return mockState.workflows.find((workflow) => workflow.id === workflowId) || mockState.workflows[0];
    }
    if (method === "POST" && path.startsWith("/Workflow")) {
        return buildSuccessBody({ id: 99, name: "Workflow Simulado" });
    }
    if (method === "PUT" && path.startsWith("/Workflow")) {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && matchPath(path, "/Workflow/:workflowId")) {
        return buildSuccessBody(true);
    }

    if (method === "GET" && path === "/Document") {
        return buildPagedResponse(mockState.documents, params);
    }
    if (method === "GET" && path === "/Document/CheckExceededPages") {
        return false;
    }
    if (method === "GET" && matchPath(path, "/Document/Status/:id")) {
        return "Processed";
    }
    if (method === "GET" && matchPath(path, "/Document/FindDocument/:docId")) {
        return buildEmptyBlob();
    }
    if (method === "GET" && matchPath(path, "/Document/Analyze/:id")) {
        return { summary: "Análise simulada concluída." };
    }
    if (method === "POST" && path.startsWith("/Document")) {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && path === "/Document/Delete") {
        return buildSuccessBody(true);
    }

    if (method === "GET" && matchPath(path, "/DocumentMetadata/Analyze/:docId")) {
        return { content: "Metadados simulados do documento." };
    }
    if (method === "GET" && matchPath(path, "/DocumentMetadata/Normalized/:docId")) {
        return { content: "Texto normalizado simulado." };
    }
    if (method === "GET" && matchPath(path, "/DocumentMetadata/OcrText/:docId")) {
        return { content: "Texto OCR simulado." };
    }
    if (method === "POST" && path.startsWith("/DocumentMetadata")) {
        return buildSuccessBody(true);
    }

    if (method === "GET" && matchPath(path, "/DocumentHistory/:id")) {
        return [{ action: "Upload", date: "2026-06-10T08:00:00.000Z", user: "demo@prototype.local" }];
    }
    if (method === "GET" && matchPath(path, "/DocumentHistory/:id/batch")) {
        return [];
    }

    if (method === "POST" && path.startsWith("/DocumentQuestionnarire")) {
        return buildSuccessBody(true);
    }
    if (method === "POST" && path.startsWith("/DocumentAnalysisRejection")) {
        return buildSuccessBody(true);
    }
    if (method === "GET" && path.startsWith("/DocumentAnalysisRejection")) {
        return [];
    }

    if (method === "GET" && matchPath(path, "/Card/AnalyzeSteps/:id")) {
        return buildCardAnalyzeSteps(parseIdFromPath(path, 2));
    }
    if (method === "GET" && matchPath(path, "/Card/HeaderInfo/:id")) {
        return buildCardHeaderInfo(parseIdFromPath(path, 2));
    }
    if (method === "GET" && matchPath(path, "/Card/Batch/:batchId")) {
        return [];
    }
    if (method === "PUT" && path.startsWith("/Card")) {
        return buildSuccessBody(true);
    }

    if (method === "POST" && path === "/Anonymization") {
        return buildSuccessBody(true);
    }
    if (method === "GET" && matchPath(path, "/Anonymization/document/:documentId")) {
        return buildAnonymizationList(parseIdFromPath(path, 2));
    }

    if (method === "GET" && (path === "/TypeDoc/Paged" || path === "/TypeDoc/Paged/")) {
        return buildPagedResponse(buildTypeDocList(), params);
    }
    if (method === "GET" && path === "/TypeDoc/FindAll") {
        return buildTypeDocList();
    }
    if (method === "POST" && path.startsWith("/TypeDoc")) {
        return buildSuccessBody(true);
    }
    if (method === "PUT" && path === "/TypeDoc") {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && path === "/TypeDoc/DeleteByIds") {
        return buildSuccessBody(true);
    }

    if (method === "GET" && (path === "/Prompt/Paged" || path === "/Prompt/Paged/")) {
        return buildPagedResponse(mockState.prompts, params);
    }
    if (method === "GET" && (path === "/Prompt/PagedByUser" || path === "/Prompt/PagedByUser/")) {
        return buildPagedResponse(mockState.prompts, params);
    }
    if (method === "GET" && path === "/Prompt/Templates") {
        return [];
    }
    if (method === "GET" && path === "/Prompt") {
        return mockState.prompts;
    }
    if (method === "GET" && matchPath(path, "/Prompt/:id")) {
        const promptId = parseIdFromPath(path, 1);
        return mockState.prompts.find((prompt) => prompt.id === promptId) || mockState.prompts[0];
    }
    if (method === "GET" && matchPath(path, "/Prompt/:id/validate-ownership")) {
        return true;
    }
    if (method === "POST" && path.startsWith("/Prompt")) {
        return buildSuccessBody(true);
    }
    if (method === "PUT" && path === "/Prompt") {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && path === "/Prompt/DeleteByIds") {
        return buildSuccessBody(true);
    }
    if (method === "POST" && path === "/PlayGroundPrompts/test") {
        return { output: "Resposta simulada do playground." };
    }

    if (method === "GET" && path === "/Question/Paged") {
        return buildPagedResponse(mockState.questions, params);
    }
    if (method === "GET" && path === "/Question/FindAll") {
        return mockState.questions;
    }
    if (method === "POST" && path === "/Question") {
        return buildSuccessBody(true);
    }
    if (method === "PUT" && path === "/Question") {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && path === "/Question/DeleteByIds") {
        return buildSuccessBody(true);
    }

    if (method === "GET" && path === "/Questionnaire/Paged") {
        return buildPagedResponse(mockState.questionnaires, params);
    }
    if (method === "GET" && path === "/Questionnaire/FindAll") {
        return mockState.questionnaires;
    }
    if (method === "GET" && matchPath(path, "/Questionnaire/:id")) {
        const quizId = parseIdFromPath(path, 1);
        return mockState.questionnaires.find((quiz) => quiz.id === quizId) || mockState.questionnaires[0];
    }
    if (method === "POST" && path === "/Questionnaire") {
        return buildSuccessBody(true);
    }
    if (method === "PUT" && path === "/Questionnaire") {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && path === "/Questionnaire/DeleteByIds") {
        return buildSuccessBody(true);
    }

    if (method === "GET" && (path === "/Tool/Paged" || path === "/Tool/Paged/")) {
        return buildToolPagedResponse(mockState.tools, params);
    }
    if (method === "GET" && path === "/Tool") {
        return mockState.tools;
    }
    if (method === "POST" && path.startsWith("/Tool")) {
        return buildSuccessBody(true);
    }
    if (method === "PUT" && path === "/Tool") {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && (path === "/Tool" || path === "/Tool/")) {
        return buildSuccessBody(true);
    }

    if (method === "GET" && path === "/ToolType") {
        return buildToolTypes();
    }
    if (method === "GET" && path === "/ToolData") {
        return [];
    }

    if (method === "GET" && path.startsWith("/Automation")) {
        return [];
    }

    if (method === "GET" && path === "/StepId") {
        return findWorkflowSteps(1)[0];
    }

    if (method === "GET" && path === "/ApiTemplate/Paged") {
        return buildPagedResponse(mockState.templates, params);
    }
    if (method === "GET" && path === "/ApiTemplate") {
        return mockState.templates;
    }
    if (method === "GET" && matchPath(path, "/ApiTemplate/:id")) {
        const templateId = parseIdFromPath(path, 1);
        return mockState.templates.find((template) => template.id === templateId) || mockState.templates[0];
    }
    if (method === "POST" && path.startsWith("/ApiTemplate")) {
        return buildSuccessBody(true);
    }
    if (method === "PUT" && path === "/ApiTemplate") {
        return buildSuccessBody(true);
    }
    if (method === "DELETE" && matchPath(path, "/ApiTemplate/:id")) {
        return buildSuccessBody(true);
    }
    if (method === "POST" && path === "/ApiTemplateRequestCheck/execute") {
        return { statusCode: 200, body: '{"ok":true}' };
    }

    if (method === "GET" && path === "/Status") {
        return buildStatusList();
    }
    if (method === "GET" && path === "/Status/Steps") {
        return buildStatusList();
    }

    if (method === "GET" && path === "/Dashboard") {
        return buildDashboardResponse();
    }
    if (method === "GET" && path === "/Dashboard/UsageUnits") {
        return { tokens: "WTC", pages: "Pages" };
    }
    if (method === "PUT" && path === "/Dashboard/ProcessMetricsByTenant") {
        return buildSuccessBody(true);
    }
    if (method === "GET" && path === "/UsageMonth/FindUsedModels") {
        return ["gpt-4o", "gemini-2.5-pro"];
    }
    if (method === "GET" && path === "/UsageMonth") {
        return buildUsageMonthResponse();
    }
    if (method === "GET" && path.startsWith("/UsageMonth/")) {
        return buildUsageMonthResponse();
    }

    if (method === "GET" && path === "/Auditor/ActionTypes") {
        return ["Upload", "Analyze", "Approve", "Reject", "Comment"];
    }
    if (method === "GET" && path === "/Auditor/Documents") {
        return buildAuditorDocumentSummary(params);
    }
    if (method === "GET" && path === "/Auditor/Workflows") {
        return buildAuditorWorkflowSummary(params);
    }
    if (method === "GET" && matchPath(path, "/Auditor/Workflow/:id")) {
        return buildAuditorWorkflowDetail(parseIdFromPath(path, 2));
    }
    if (method === "GET" && path === "/Auditor/Users") {
        return buildAuditorUserSummary(params);
    }
    if (method === "GET" && matchPath(path, "/Auditor/User/:userId")) {
        return buildAuditorUserDetail(parseIdFromPath(path, 2));
    }
    if (method === "GET" && matchPath(path, "/Auditor/Documents/:documentId/Workflows/:workflowId")) {
        const documentId = parseIdFromPath(path, 2);
        const workflowId = parseIdFromPath(path, 4);
        return buildAuditorDocumentDetail(documentId, workflowId);
    }

    if (method === "GET") {
        return {};
    }
    if (method === "DELETE") {
        return buildSuccessBody(true);
    }
    return buildSuccessBody(true);
}

export function resolveMockResponse(config) {
    const data = resolveMockRequest(config);
    const isBlob = config.responseType === "blob";
    return {
        data: isBlob && !(data instanceof Blob) ? buildEmptyBlob() : data,
        status: 200,
        statusText: "OK",
        headers: {},
        config,
    };
}
