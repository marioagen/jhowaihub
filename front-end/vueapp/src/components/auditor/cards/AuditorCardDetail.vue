<template>
    <div class="card-body d-flex flex-column p-0">
        <!-- Prop not null: branch on workflow count -->
        <template v-if="selectedDocument != null">
            <!-- More than 1 workflow: show workflow selection flow -->
            <template v-if="hasMultipleWorkflows">
                <!-- Workflow selection screen -->
                <template v-if="mustSelectWorkflow && !isLoading">
                    <div class="p-3">
                        <h6
                            class="workflow-select-heading mb-2 fw-bold d-flex align-items-center gap-2"
                        >
                            <LucideIcon
                                icon="GitBranch"
                                :size="20"
                                class="text-warning"
                            />
                            Selecionar Esteira
                        </h6>
                        <p class="small text-muted mb-3">
                            Este documento participa de
                            <strong>{{ selectedDocumentWorkflows.length }} esteiras</strong>
                            diferentes. Selecione qual deseja visualizar.
                        </p>
                        <!-- Document info card (not clickable) -->
                        <div class="workflow-select-card rounded-2 p-2 mb-2 border">
                            <div class="d-flex align-items-start gap-2">
                                <span
                                    class="workflow-doc-icon d-inline-flex align-items-center justify-content-center flex-shrink-0"
                                >
                                    <LucideIcon
                                        icon="FileText"
                                        :size="20"
                                    />
                                </span>
                                <div class="min-w-0 flex-grow-1">
                                    <div class="workflow-select-card-title fw-semibold">
                                        {{ selectedDocument.cardName }}
                                    </div>
                                    <div class="small text-muted">
                                        {{
                                            selectedDocument.actionsCount != null
                                                ? selectedDocument.actionsCount + " ação(ões)"
                                                : selectedDocumentWorkflows.length + " esteira(s)"
                                        }}
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- Workflow cards (clickable) -->
                        <div
                            v-for="wf in selectedDocumentWorkflows"
                            :key="wf.id"
                            class="workflow-select-card workflow-select-card-clickable rounded-2 p-2 mb-2 border cursor-pointer d-flex align-items-center gap-2"
                            @click="onSelectWorkflow(wf)"
                        >
                            <span
                                class="workflow-wf-icon d-inline-flex align-items-center justify-content-center flex-shrink-0"
                            >
                                <LucideIcon
                                    icon="Workflow"
                                    :size="20"
                                />
                            </span>
                            <div class="min-w-0 flex-grow-1">
                                <div class="workflow-select-card-title fw-semibold">
                                    {{ wf.name }}
                                </div>
                                <div class="small text-muted">
                                    <template v-if="wf.stage != null || wf.eventsCount != null">
                                        Etapa: {{ wf.stage ?? "—" }} ·
                                        {{ wf.eventsCount ?? "—" }} eventos
                                    </template>
                                    <template v-else>Workflow ID: {{ wf.id }}</template>
                                </div>
                                <div
                                    v-if="wf.lastAction || wf.lastActionTimestamp"
                                    class="small text-muted"
                                >
                                    Última ação: {{ wf.lastAction ?? "—" }}
                                    <span class="ms-1">{{ wf.lastActionTimestamp ?? "" }}</span>
                                </div>
                            </div>
                            <LucideIcon
                                icon="ChevronRight"
                                :size="16"
                                class="text-muted flex-shrink-0"
                            />
                        </div>
                    </div>
                </template>
                <!-- Loading after workflow selected -->
                <div
                    v-else-if="isLoading"
                    class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-5"
                >
                    <LoadingComponent />
                </div>
                <!-- Detail view with return button -->
                <template v-else>
                    <div class="p-3 border-bottom">
                        <div
                            v-if="showReturnInDetail"
                            class="d-flex align-items-center gap-2 mb-2"
                        >
                            <button
                                type="button"
                                class="btn btn-light btn-sm border d-flex align-items-center gap-1 py-1 px-2"
                                aria-label="Voltar para lista de esteiras"
                                @click="onReturnToWorkflowList"
                            >
                                <LucideIcon
                                    icon="ArrowLeft"
                                    :size="14"
                                />
                                Voltar
                            </button>
                        </div>
                        <div class="mb-2">
                            <h6 class="mb-0 fw-bold d-flex align-items-center gap-1">
                                <LucideIcon
                                    icon="History"
                                    :size="18"
                                />
                                Histórico - {{ selectedDocument.cardName }}
                                <BadgeComponent
                                    :text="(auditCardDetails || []).length"
                                    variant="secondary"
                                    size="sm"
                                    :clickable="false"
                                />
                            </h6>
                        </div>
                        <div
                            class="d-flex align-items-center justify-content-between flex-wrap gap-2 mb-2"
                        >
                            <div class="d-flex align-items-center gap-2 flex-wrap">
                                <button
                                    v-for="stage in stageFilterOptions"
                                    :key="stage.value"
                                    type="button"
                                    class="btn btn-sm rounded-pill border py-1 px-2 auditor-filter-sm"
                                    :class="
                                        selectedStageId === stage.value
                                            ? 'btn-primary'
                                            : 'btn-light'
                                    "
                                    @click="$emit('update:selectedStageId', stage.value)"
                                >
                                    {{ stage.label }}
                                </button>
                            </div>
                            <div class="d-flex align-items-center gap-2 flex-wrap">
                                <button
                                    type="button"
                                    class="btn btn-light btn-sm border py-1 px-2 auditor-filter-sm d-flex align-items-center gap-1"
                                >
                                    <LucideIcon
                                        icon="ArrowUpDown"
                                        :size="12"
                                    />
                                    Mais recentes
                                </button>
                                <div class="dropdown">
                                    <button
                                        class="btn btn-light btn-sm border py-1 px-2 auditor-filter-sm d-flex align-items-center gap-1 dropdown-toggle"
                                        type="button"
                                        data-bs-toggle="dropdown"
                                        aria-expanded="false"
                                    >
                                        <LucideIcon
                                            icon="Filter"
                                            :size="12"
                                        />
                                        Todas as ações
                                        <LucideIcon
                                            icon="ChevronDown"
                                            :size="12"
                                        />
                                    </button>
                                    <ul class="dropdown-menu dropdown-menu-start">
                                        <li>
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                            >
                                                Todas as ações
                                            </a>
                                        </li>
                                        <li>
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                            >
                                                Upload
                                            </a>
                                        </li>
                                        <li>
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                            >
                                                Deletar
                                            </a>
                                        </li>
                                        <li>
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                            >
                                                Protocolo
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                        <div class="input-group input-group-sm auditor-filter-sm">
                            <span class="input-group-text border-end-0 py-1">
                                <LucideIcon
                                    icon="Search"
                                    :size="14"
                                />
                            </span>
                            <input
                                type="text"
                                class="form-control form-control-sm border-start-0 py-1"
                                placeholder="Buscar por usuário, detalhes, ação, etapa..."
                                aria-label="Buscar no histórico"
                                :value="historySearchInput"
                                @input="$emit('update:historySearchInput', $event.target.value)"
                            />
                        </div>
                    </div>
                    <div
                        class="audit-history-list overflow-auto flex-grow-1 px-3 pb-3 d-flex flex-column min-h-0"
                    >
                        <div
                            v-for="(entry, index) in displayedDetailsEntries"
                            :key="index"
                            class="audit-history-card rounded-2 p-2 mt-2 mb-2 border"
                        >
                            <div class="d-flex align-items-start gap-2 flex-wrap">
                                <span
                                    class="audit-user-badge d-inline-flex align-items-center justify-content-center flex-shrink-0"
                                >
                                    <LucideIcon
                                        icon="User"
                                        :size="12"
                                    />
                                </span>
                                <span class="small fw-semibold align-self-center">
                                    {{ entry.userName }}
                                </span>
                                <div class="min-w-0 flex-grow-1 audit-history-card-content">
                                    <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                        <BadgeComponent
                                            v-if="entry.actionName"
                                            :text="entry.actionName"
                                            variant="primary"
                                            size="sm"
                                            :clickable="false"
                                        />
                                        <BadgeComponent
                                            v-if="entry.stepName"
                                            :text="entry.stepName"
                                            variant="secondary"
                                            size="sm"
                                            :clickable="false"
                                        />
                                    </div>
                                    <p
                                        v-if="entry.actionName || entry.stepName"
                                        class="small text-muted mb-1"
                                    >
                                        {{
                                            entry.stepName
                                                ? entry.actionName + " · " + entry.stepName
                                                : entry.actionName
                                        }}
                                    </p>
                                    <div class="small text-muted d-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Clock"
                                            :size="12"
                                        />
                                        {{ formatDate(entry.created) }}
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div
                            v-if="showHistoryLoadMore"
                            class="mt-2 mb-3 text-center"
                        >
                            <button
                                type="button"
                                class="btn btn-outline-primary btn-sm"
                                @click="loadMoreHistory"
                            >
                                Carregar mais
                            </button>
                        </div>
                    </div>
                </template>
            </template>

            <!-- 1 or 0 workflows: render details part only (or loading) -->
            <template v-else>
                <div
                    v-if="isLoading"
                    class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-5"
                >
                    <LoadingComponent />
                </div>
                <template v-else>
                    <div class="p-3 border-bottom">
                        <div class="mb-2">
                            <h6 class="mb-0 fw-bold d-flex align-items-center gap-1">
                                <LucideIcon
                                    icon="History"
                                    :size="18"
                                />
                                Histórico - {{ selectedDocument.cardName }}
                                <BadgeComponent
                                    :text="(auditCardDetails || []).length"
                                    variant="secondary"
                                    size="sm"
                                    :clickable="false"
                                />
                            </h6>
                        </div>
                        <div
                            class="d-flex align-items-center justify-content-between flex-wrap gap-2 mb-2"
                        >
                            <div class="d-flex align-items-center gap-2 flex-wrap">
                                <button
                                    v-for="stage in stageFilterOptions"
                                    :key="stage.value"
                                    type="button"
                                    class="btn btn-sm rounded-pill border py-1 px-2 auditor-filter-sm"
                                    :class="
                                        selectedStageId === stage.value
                                            ? 'btn-primary'
                                            : 'btn-light'
                                    "
                                    @click="$emit('update:selectedStageId', stage.value)"
                                >
                                    {{ stage.label }}
                                </button>
                            </div>
                            <div class="d-flex align-items-center gap-2 flex-wrap">
                                <button
                                    type="button"
                                    class="btn btn-light btn-sm border py-1 px-2 auditor-filter-sm d-flex align-items-center gap-1"
                                >
                                    <LucideIcon
                                        icon="ArrowUpDown"
                                        :size="12"
                                    />
                                    Mais recentes
                                </button>
                                <div class="dropdown">
                                    <button
                                        class="btn btn-light btn-sm border py-1 px-2 auditor-filter-sm d-flex align-items-center gap-1 dropdown-toggle"
                                        type="button"
                                        data-bs-toggle="dropdown"
                                        aria-expanded="false"
                                    >
                                        <LucideIcon
                                            icon="Filter"
                                            :size="12"
                                        />
                                        Todas as ações
                                        <LucideIcon
                                            icon="ChevronDown"
                                            :size="12"
                                        />
                                    </button>
                                    <ul class="dropdown-menu dropdown-menu-start">
                                        <li>
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                            >
                                                Todas as ações
                                            </a>
                                        </li>
                                        <li>
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                            >
                                                Upload
                                            </a>
                                        </li>
                                        <li>
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                            >
                                                Deletar
                                            </a>
                                        </li>
                                        <li>
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                            >
                                                Protocolo
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                        <div class="input-group input-group-sm auditor-filter-sm">
                            <span class="input-group-text border-end-0 py-1">
                                <LucideIcon
                                    icon="Search"
                                    :size="14"
                                />
                            </span>
                            <input
                                type="text"
                                class="form-control form-control-sm border-start-0 py-1"
                                placeholder="Buscar por usuário, detalhes, ação, etapa..."
                                aria-label="Buscar no histórico"
                                :value="historySearchInput"
                                @input="$emit('update:historySearchInput', $event.target.value)"
                            />
                        </div>
                    </div>
                    <div
                        class="audit-history-list overflow-auto flex-grow-1 px-3 pb-3 d-flex flex-column min-h-0"
                    >
                        <div
                            v-for="(entry, index) in displayedDetailsEntries"
                            :key="index"
                            class="audit-history-card rounded-2 p-2 mt-2 mb-2 border"
                        >
                            <div class="d-flex align-items-start gap-2 flex-wrap">
                                <span
                                    class="audit-user-badge d-inline-flex align-items-center justify-content-center flex-shrink-0"
                                >
                                    <LucideIcon
                                        icon="User"
                                        :size="12"
                                    />
                                </span>
                                <span class="small fw-semibold align-self-center">
                                    {{ entry.userName }}
                                </span>
                                <div class="min-w-0 flex-grow-1 audit-history-card-content">
                                    <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                        <BadgeComponent
                                            v-if="entry.actionName"
                                            :text="entry.actionName"
                                            variant="primary"
                                            size="sm"
                                            :clickable="false"
                                        />
                                        <BadgeComponent
                                            v-if="entry.stepName"
                                            :text="entry.stepName"
                                            variant="secondary"
                                            size="sm"
                                            :clickable="false"
                                        />
                                    </div>
                                    <p
                                        v-if="entry.actionName || entry.stepName"
                                        class="small text-muted mb-1"
                                    >
                                        {{
                                            entry.stepName
                                                ? entry.actionName + " · " + entry.stepName
                                                : entry.actionName
                                        }}
                                    </p>
                                    <div class="small text-muted d-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Clock"
                                            :size="12"
                                        />
                                        {{ formatDate(entry.created) }}
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div
                            v-if="showHistoryLoadMore"
                            class="mt-2 mb-3 text-center"
                        >
                            <button
                                type="button"
                                class="btn btn-outline-primary btn-sm"
                                @click="loadMoreHistory"
                            >
                                Carregar mais
                            </button>
                        </div>
                    </div>
                </template>
            </template>
        </template>

        <!-- Prop null: empty state -->
        <template v-else>
            <div class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-4">
                <div class="text-center text-muted py-5">
                    <div class="rounded-circle d-inline-flex p-4 mb-3 workflow-empty-icon-wrap">
                        <LucideIcon
                            icon="History"
                            :size="48"
                        />
                    </div>
                    <p class="mb-0">Selecione um documento para ver seu histórico</p>
                </div>
            </div>
        </template>
    </div>
</template>
<script>
    import BadgeComponent from "@/components/global/BadgeComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import AuditorsService from "@/services/auditors/AuditorsService";
    import dateHelper from "@/helpers/date.js";

    export default {
        name: "AuditorCardDetail",
        components: {
            BadgeComponent,
            LoadingComponent,
        },
        props: {
            selectedDocument: {
                type: Object,
                default: null,
            },
            selectedDocumentWorkflows: {
                type: Array,
                default: () => [],
            },
        },
        data() {
            return {
                isLoading: false,
                mustSelectWorkflow: false,
                selectedWorkflowId: null,
                auditCardDetails: null,
                displayedLimit: 10,
                stageFilterOptions: [],
                selectedStageId: "0",
                historySearchInput: "",
            };
        },
        computed: {
            hasMultipleWorkflows() {
                return (this.selectedDocumentWorkflows || []).length > 1;
            },
            showReturnInDetail() {
                return this.hasMultipleWorkflows && !!this.selectedWorkflowId;
            },
            displayedDetailsEntries() {
                const list = this.auditCardDetails || [];
                return list.slice(0, this.displayedLimit);
            },
            showHistoryLoadMore() {
                const total = (this.auditCardDetails || []).length;
                return total > 10 && this.displayedLimit < total;
            },
        },
        methods: {
            async refreshWithCurrentDocument() {
                console.log("selectedDocument", this.selectedDocument);
                console.log("selectedDocumentWorkflows", this.selectedDocumentWorkflows);
                if (this.selectedDocument == null) return;

                this.displayedLimit = 10;
                this.selectedWorkflowId = null;
                this.auditCardDetails = null;

                const workflowsCount = (this.selectedDocumentWorkflows || []).length;
                if (workflowsCount === 0) return;

                if (workflowsCount > 1) {
                    this.mustSelectWorkflow = true;
                    return;
                }
                this.mustSelectWorkflow = false;
                await this.getAuditCardDetails();
            },
            async onSelectWorkflow(workflow) {
                this.selectedWorkflowId = workflow?.id ?? workflow;
                this.mustSelectWorkflow = false;
                await this.getAuditCardDetails();
            },
            onReturnToWorkflowList() {
                this.mustSelectWorkflow = true;
                this.selectedWorkflowId = null;
                this.auditCardDetails = null;
            },
            loadMoreHistory() {
                this.displayedLimit += 10;
            },
            formatDate(date) {
                return dateHelper.formatDate(date) || "—";
            },
            async getAuditCardDetails() {
                if (this.selectedDocument?.cardId == null) return;
                const workflowId =
                    this.selectedWorkflowId || this.selectedDocumentWorkflows?.[0]?.id;
                if (workflowId == null) return;

                this.isLoading = true;
                try {
                    const params = { take: this.displayedLimit };
                    const response = await AuditorsService.getCardAuditDetails(
                        this.selectedDocument.cardId,
                        workflowId,
                        params
                    );
                    console.log("response", response);
                    if (response.error) {
                        return this.$notify({
                            title: "audit-cards.title",
                            message:
                                response.error.response?.data?.detail ?? response.error.message,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.auditCardDetails = Array.isArray(response)
                        ? response
                        : Array.isArray(response?.data)
                          ? response.data
                          : [];
                } finally {
                    this.isLoading = false;
                }
            },
        },
        async created() {
            await this.refreshWithCurrentDocument();
        },
    };
</script>
<style scoped>
    .auditor-filter-sm {
        font-size: 0.75rem;
    }
    .auditor-filter-sm .form-control,
    .auditor-filter-sm .input-group-text {
        font-size: 0.75rem;
    }
    .min-vh-50 {
        min-height: 50vh;
    }
    .audit-history-list {
        flex: 1 1 0;
        min-height: 0;
    }
    .audit-history-card {
        background-color: transparent;
    }
    .audit-user-badge {
        background-color: var(--bs-secondary-bg, #ececec);
        color: var(--bs-secondary-color, #6c757d);
        border-radius: 999px;
        width: 24px;
        height: 24px;
        font-size: 0;
    }
    .audit-history-card-content {
        flex: 1 1 100%;
        min-width: 0;
    }
    .workflow-doc-icon,
    .workflow-wf-icon {
        width: 40px;
        height: 40px;
        border-radius: 8px;
        background-color: var(--bs-primary-bg-subtle, rgba(13, 110, 253, 0.15));
        color: var(--bs-primary);
    }
    .workflow-select-heading,
    .workflow-select-card-title {
        color: var(--bs-body-color);
    }
    .workflow-select-card {
        background-color: var(--bs-secondary-bg, transparent);
    }
    .workflow-select-card-clickable:hover {
        background-color: var(--bs-tertiary-bg, rgba(0, 0, 0, 0.04));
    }
    .workflow-empty-icon-wrap {
        background-color: var(--bs-secondary-bg, transparent);
    }
    .cursor-pointer {
        cursor: pointer;
    }
</style>
