<template>
    <div class="card-body d-flex flex-column p-0">
        <template v-if="selectedDocument">
            <!-- Multiple workflows: show workflow selection or loading or detail with return -->
            <template v-if="hasMultipleWorkflows">
                <!-- Workflow selection screen -->
                <template v-if="!selectedWorkflowId && !detailLoading">
                    <div class="p-3">
                        <h6 class="mb-2 fw-bold d-flex align-items-center gap-2 text-body">
                            <LucideIcon
                                icon="GitBranch"
                                :size="20"
                                class="text-warning"
                            />
                            Selecionar Esteira
                        </h6>
                        <p class="small text-muted mb-3">
                            Este documento participa de
                            <strong>{{ documentWorkflows.length }} esteiras</strong>
                            diferentes. Selecione qual deseja visualizar.
                        </p>
                        <!-- Document info card (not clickable) -->
                        <div class="workflow-select-card rounded-2 p-2 mb-2 border bg-light">
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
                                    <div class="fw-semibold text-body">
                                        {{ selectedDocument.title }}
                                    </div>
                                    <div class="small text-muted">
                                        {{
                                            selectedDocument.alterations ||
                                            documentWorkflows.length + " esteiras"
                                        }}
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- Workflow cards (clickable) -->
                        <div
                            v-for="wf in documentWorkflows"
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
                                <div class="fw-semibold text-body">{{ wf.name }}</div>
                                <div class="small text-muted">
                                    Etapa: {{ wf.stage }} · {{ wf.eventsCount }} eventos
                                </div>
                                <div class="small text-muted">
                                    Última ação: {{ wf.lastAction }}
                                    <span class="ms-1">{{ wf.lastActionTimestamp }}</span>
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
                    v-else-if="detailLoading"
                    class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-5"
                >
                    <LoadingComponent />
                </div>
                <!-- Detail view with return button -->
                <template v-else>
                    <div class="p-3 border-bottom">
                        <div class="d-flex align-items-center gap-2 mb-2">
                            <button
                                type="button"
                                class="btn btn-light btn-sm border d-flex align-items-center gap-1 py-1 px-2"
                                aria-label="Voltar para lista de esteiras"
                                @click="$emit('return-to-workflow-list')"
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
                                Histórico - {{ selectedDocument.title }}
                                <BadgeComponent
                                    :text="auditHistoryEntries.length"
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
                            <span class="input-group-text bg-white border-end-0 py-1">
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
                    <div class="audit-history-list overflow-auto flex-grow-1 px-3 pb-3">
                        <div
                            v-for="entry in auditHistoryEntries"
                            :key="entry.id"
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
                                            v-for="tag in entry.actionTags"
                                            :key="tag.label"
                                            :text="tag.label"
                                            :variant="tag.variant"
                                            size="sm"
                                            :clickable="false"
                                        />
                                    </div>
                                    <p class="small text-muted mb-1">{{ entry.description }}</p>
                                    <div class="small text-muted d-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Clock"
                                            :size="12"
                                        />
                                        {{ entry.timestamp }}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </template>
            </template>

            <!-- Single workflow: current detail (no return btn) -->
            <template v-else>
                <div class="p-3 border-bottom">
                    <div class="mb-2">
                        <h6 class="mb-0 fw-bold d-flex align-items-center gap-1">
                            <LucideIcon
                                icon="History"
                                :size="18"
                            />
                            Histórico - {{ selectedDocument.title }}
                            <BadgeComponent
                                :text="auditHistoryEntries.length"
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
                                    selectedStageId === stage.value ? 'btn-primary' : 'btn-light'
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
                        <span class="input-group-text bg-white border-end-0 py-1">
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
                <div class="audit-history-list overflow-auto flex-grow-1 px-3 pb-3">
                    <div
                        v-for="entry in auditHistoryEntries"
                        :key="entry.id"
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
                                        v-for="tag in entry.actionTags"
                                        :key="tag.label"
                                        :text="tag.label"
                                        :variant="tag.variant"
                                        size="sm"
                                        :clickable="false"
                                    />
                                </div>
                                <p class="small text-muted mb-1">{{ entry.description }}</p>
                                <div class="small text-muted d-flex align-items-center gap-1">
                                    <LucideIcon
                                        icon="Clock"
                                        :size="12"
                                    />
                                    {{ entry.timestamp }}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </template>
        <template v-else>
            <div class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-4">
                <div class="text-center text-muted py-5">
                    <div class="rounded-circle bg-light d-inline-flex p-4 mb-3">
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
            /** When document has multiple workflows, the selected workflow id (from parent) */
            selectedWorkflowId: {
                type: String,
                default: "",
            },
            auditHistoryEntries: {
                type: Array,
                default: () => [],
            },
            stageFilterOptions: {
                type: Array,
                default: () => [],
            },
            selectedStageId: {
                type: String,
                default: "0",
            },
            historySearchInput: {
                type: String,
                default: "",
            },
        },
        emits: [
            "update:selectedStageId",
            "update:historySearchInput",
            "select-workflow",
            "return-to-workflow-list",
        ],
        data() {
            return {
                detailLoading: false,
            };
        },
        computed: {
            documentWorkflows() {
                const wf = this.selectedDocument?.workflows;
                return Array.isArray(wf) ? wf : [];
            },
            hasMultipleWorkflows() {
                return this.documentWorkflows.length > 1;
            },
        },
        methods: {
            onSelectWorkflow(workflow) {
                const cardId = this.selectedDocument?.id;
                if (!cardId || !workflow?.id) return;
                this.$emit("select-workflow", { cardId, workflowId: workflow.id });
                this.detailLoading = true;
                // Simulate API call; replace with actual call in parent if needed
                setTimeout(() => {
                    this.detailLoading = false;
                }, 600);
            },
        },
        watch: {
            selectedDocument() {
                this.detailLoading = false;
            },
            selectedWorkflowId() {
                if (!this.selectedWorkflowId) this.detailLoading = false;
            },
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
        min-height: 0;
        max-height: calc(100vh - 320px);
    }
    .audit-history-card {
        background-color: #fff;
    }
    .audit-user-badge {
        background-color: #ececec;
        color: #6c757d;
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
        background-color: #e7f1ff;
        color: #0d6efd;
    }
    .workflow-select-card-clickable:hover {
        background-color: rgba(0, 0, 0, 0.03);
    }
    .cursor-pointer {
        cursor: pointer;
    }
</style>
