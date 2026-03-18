const I18N_ACTION_TYPES_PREFIX = "auditor.documents.detail.actionTypes.";
const I18N_ACTION_SENTENCES_PREFIX = "auditor.documents.detail.actionSentences.";

export const AUDIT_ACTION_TYPE_NAMES = [
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
];

export function getAuditActionDisplay(actionTypeName, options = {}) {
    const { t, stepName } = options;
    if (!actionTypeName || typeof actionTypeName !== "string") {
        return { title: "", action: "" };
    }
    const titleKey = I18N_ACTION_TYPES_PREFIX + actionTypeName;
    const actionKey = I18N_ACTION_SENTENCES_PREFIX + actionTypeName;
    const title = t ? (t(titleKey) !== titleKey ? t(titleKey) : actionTypeName) : actionTypeName;
    const stepParam = stepName ?? "—";
    const action = t
        ? t(actionKey, { stepName: stepParam }) !== actionKey
            ? t(actionKey, { stepName: stepParam })
            : actionTypeName
        : actionTypeName;
    return { title, action };
}

export default {
    getAuditActionDisplay,
    AUDIT_ACTION_TYPE_NAMES,
};
