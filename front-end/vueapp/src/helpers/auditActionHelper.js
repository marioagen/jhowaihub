const I18nActionTypesPrefix = "auditor.documents.detail.actionTypes.";
const I18nActionSentencesPrefix = "auditor.documents.detail.actionSentences.";

export const AuditActionTypeNames = [
    "Upload",
    "Assign",
    "Unassign",
    "Advancement",
    "EditAnswer",
    "AnalyzeApproval",
    "AnalyzeRejection",
    "Finalize",
    "Removed",
    "DocumentCreated",
    "DocumentDeleted",
    "Rejection",
    "InputQuestionnaire",
    "InputDocument",
    "Failed",
    "AnonymizationRequest",
    "ToolUpdated",
];

export const AuditActionTypeOptions = [
    { value: 0, name: "Upload" },
    { value: 1, name: "Assign" },
    { value: 2, name: "Unassign" },
    { value: 3, name: "Advancement" },
    { value: 4, name: "EditAnswer" },
    { value: 5, name: "AnalyzeApproval" },
    { value: 6, name: "AnalyzeRejection" },
    { value: 7, name: "Finalize" },
    { value: 8, name: "Removed" },
    { value: 9, name: "DocumentCreated" },
    { value: 10, name: "DocumentDeleted" },
    { value: 11, name: "Rejection" },
    { value: 12, name: "InputQuestionnaire" },
    { value: 13, name: "InputDocument" },
    { value: 14, name: "Failed" },
    { value: 15, name: "AnonymizationRequest" },
    { value: 16, name: "ToolUpdated" },
];

export function getAuditActionDisplay(actionTypeName, options = {}) {
    const { t, stepName, toolName } = options;
    if (!actionTypeName || typeof actionTypeName !== "string") {
        return { title: "", action: "" };
    }
    const titleKey = I18nActionTypesPrefix + actionTypeName;
    const actionKey = I18nActionSentencesPrefix + actionTypeName;
    const title = t ? (t(titleKey) !== titleKey ? t(titleKey) : actionTypeName) : actionTypeName;
    const params = { stepName: stepName ?? "—", toolName: toolName ?? "—" };
    const action = t
        ? t(actionKey, params) !== actionKey
            ? t(actionKey, params)
            : actionTypeName
        : actionTypeName;
    return { title, action };
}

export default {
    getAuditActionDisplay,
    AuditActionTypeNames,
    AuditActionTypeOptions,
};
