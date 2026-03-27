export const WorkflowStatusOptions = [
    { value: "active", name: "active" },
    { value: "finalized", name: "finalized" },
    { value: "removed", name: "removed" },
];

export function mapCardsToTimelineEntries(cards) {
    if (!Array.isArray(cards)) return [];
    return cards.map((c, i) => ({
        id: `e-${c.cardId}-${i}-${c.created}`,
        userName: c.userName ?? "",
        actionName: c.actionType ?? "",
        documentName: c.cardName ?? "",
        created: c.created,
        stepName: c.stepName ?? "",
        stageName: c.stepName ?? "",
        stageId: String(c.stepId ?? ""),
    }));
}

export function mapStepsToStages(stepsCount) {
    if (!Array.isArray(stepsCount) || stepsCount.length === 0) return [];
    return stepsCount.map((s, i) => ({
        id: String(s.stepId ?? i),
        name: s.stepName ?? "",
        count: s.documentCount ?? 0,
        isTerminal: i === stepsCount.length - 1,
    }));
}
