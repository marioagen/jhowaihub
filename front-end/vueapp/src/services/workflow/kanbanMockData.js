const MOCK_CARD_ID_BASE = 9_000_000;
const MOCK_STEP_ID_BASE = 8_800_000;
const MOCK_DOCUMENT_ID_BASE = 8_000_000;

const DEFAULT_STATUS = {
    id: 1,
    name: "AwaitingAnalysis",
    label: "Esperando Análise",
    color: "#2b7fff",
};

const STEP_CARD_COUNTS = [20, 10, 4, 3, 2, 2, 1, 1];

function cloneSteps(steps) {
    return steps.map((step) => ({
        ...step,
        cards: [...(step.cards || [])],
        status: step.status ? { ...step.status } : { ...DEFAULT_STATUS },
        profile: step.profile ? { ...step.profile } : { id: 1, name: "Admin" },
    }));
}

function createMockCard(step, cardIndex) {
    const cardId = MOCK_CARD_ID_BASE + step.id * 100 + cardIndex;

    return {
        id: cardId,
        name: `Doc. simulado ${String(cardIndex + 1).padStart(2, "0")}`,
        description: `Card fictício para teste de scroll — ${step.name}`,
        owner: "test.admin@woopi.local",
        stepId: step.id,
        order: step.order,
        documentId: MOCK_DOCUMENT_ID_BASE + cardId,
        percentage: 100,
        assignedUser: null,
        profile: step.profile || { id: 1, name: "Admin" },
        status: { ...DEFAULT_STATUS },
        created: new Date(Date.now() - cardIndex * 3_600_000).toISOString(),
        toolName: "",
        isBatchParent: false,
    };
}

function appendMockCardsToStep(step, count) {
    for (let i = 0; i < count; i += 1) {
        step.cards.push(createMockCard(step, i));
    }
}

function appendMockSteps(steps, workflowId, targetCount) {
    const result = [...steps];
    const templateStep = result[0];

    while (result.length < targetCount) {
        const order = result.length + 1;
        result.push({
            id: MOCK_STEP_ID_BASE + order,
            name: `Etapa simulada ${order}`,
            workflowId: workflowId || templateStep?.workflowId || 0,
            order,
            profile: templateStep?.profile || { id: 1, name: "Admin" },
            status: templateStep?.status || { ...DEFAULT_STATUS },
            cards: [],
            stepTools: [],
            hasStepTools: false,
        });
    }

    return result;
}

function buildFullyMockKanban() {
    const workflowId = 1;
    const stepNames = [
        "Processando",
        "Revisão",
        "Concluído",
        "Teste",
        "Validação",
        "Aprovação",
        "Arquivamento",
        "Finalizado",
    ];

    const steps = stepNames.map((name, index) => ({
        id: MOCK_STEP_ID_BASE + index + 1,
        name,
        workflowId,
        order: index + 1,
        profile: { id: 1, name: "Admin" },
        status: { ...DEFAULT_STATUS },
        cards: [],
        stepTools: [],
        hasStepTools: false,
    }));

    steps.forEach((step, index) => {
        appendMockCardsToStep(step, STEP_CARD_COUNTS[index] ?? 1);
    });

    return steps;
}

export function applyKanbanMockData(steps, options = {}) {
    const minSteps = options.minSteps ?? 8;
    const cardCounts = options.cardCounts ?? STEP_CARD_COUNTS;

    if (!Array.isArray(steps) || steps.length === 0) {
        return buildFullyMockKanban();
    }

    const workflowId = steps[0]?.workflowId;
    let result = cloneSteps(steps);

    if (result.length < minSteps) {
        result = appendMockSteps(result, workflowId, minSteps);
    }

    result.forEach((step, index) => {
        const cardsToAdd = cardCounts[index] ?? 1;
        appendMockCardsToStep(step, cardsToAdd);
    });

    return result;
}

export function isKanbanMockQueryEnabled(routeQuery = {}) {
    const value = routeQuery?.mockKanban;
    return value === "1" || value === "true" || value === true;
}
