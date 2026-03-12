<template>
    <div>
        <div
            v-if="loading"
            class="audit-list-wrapper d-flex flex-column flex-grow-1 min-h-0 align-items-center justify-content-center py-5"
        >
            <LoadingComponent />
        </div>
        <template v-else>
            <div class="audit-list-wrapper d-flex flex-column flex-grow-1 min-h-0">
                <div class="audit-list overflow-auto flex-grow-1 min-h-0">
                    <div
                        v-for="item in displayedWorkflowItems"
                        :key="item.id"
                        class="audit-list-item rounded-2 p-2 mb-2 cursor-pointer"
                        :class="{
                            'audit-list-item-selected border-start border-primary border-3':
                                selectedWorkflow && selectedWorkflow.id === item.id,
                            border: !selectedWorkflow || selectedWorkflow.id !== item.id,
                        }"
                        @click="$emit('select-workflow', item)"
                    >
                        <div class="d-flex align-items-start gap-2">
                            <span
                                class="workflow-card-icon d-inline-flex align-items-center justify-content-center flex-shrink-0 text-muted"
                            >
                                <LucideIcon
                                    icon="Workflow"
                                    :size="18"
                                />
                            </span>
                            <div class="min-w-0 flex-grow-1">
                                <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                    <span class="fw-semibold small text-break">
                                        {{ item.title }}
                                    </span>
                                    <BadgeComponent
                                        :text="item.statusLabel"
                                        :variant="item.statusVariant"
                                        size="sm"
                                        :clickable="false"
                                    />
                                </div>
                                <div class="small text-muted d-flex align-items-center gap-1 mb-2">
                                    <LucideIcon
                                        icon="UsersRound"
                                        :size="12"
                                    />
                                    {{ item.teamName }}
                                </div>
                                <div
                                    class="small text-muted d-flex align-items-center flex-wrap gap-2"
                                >
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="FileText"
                                            :size="12"
                                        />
                                        {{ item.docsCount }} docs
                                    </span>
                                    <span
                                        class="d-inline-flex align-items-center gap-1 text-success"
                                    >
                                        <span
                                            class="workflow-metric-dot bg-success rounded-circle"
                                        ></span>
                                        {{ item.greenCount }}
                                    </span>
                                    <span
                                        class="d-inline-flex align-items-center gap-1 text-danger"
                                    >
                                        <span
                                            class="workflow-metric-dot bg-danger rounded-circle"
                                        ></span>
                                        {{ item.redCount }}
                                    </span>
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Zap"
                                            :size="12"
                                        />
                                        {{ item.eventsCount }} eventos
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div
                    v-if="showLoadMoreButton"
                    class="audit-list-footer flex-shrink-0 pt-2"
                >
                    <button
                        type="button"
                        class="btn btn-outline-primary btn-sm w-100"
                        @click="loadMore"
                    >
                        Load more
                    </button>
                </div>
            </div>
        </template>
    </div>
</template>
<script>
    import BadgeComponent from "@/components/global/BadgeComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";

    const MOCK_WORKFLOW_ITEMS = [
        {
            id: "wf-nf-1",
            title: "Processamento de Notas Fiscais",
            teamName: "Time Financeiro",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 4,
            greenCount: 1,
            redCount: 1,
            eventsCount: 18,
        },
        {
            id: "wf-rh-1",
            title: "Gestão de Documentos RH",
            teamName: "Time RH",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 3,
            greenCount: 1,
            redCount: 1,
            eventsCount: 12,
        },
        {
            id: "wf-juridico-1",
            title: "Análise Jurídica de Contratos",
            teamName: "Time Jurídico",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 5,
            greenCount: 2,
            redCount: 0,
            eventsCount: 15,
        },
        {
            id: "wf-nf-2",
            title: "Conciliação Bancária",
            teamName: "Time Financeiro",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 8,
            greenCount: 2,
            redCount: 1,
            eventsCount: 22,
        },
        {
            id: "wf-rh-2",
            title: "Onboarding de Colaboradores",
            teamName: "Time RH",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 6,
            greenCount: 1,
            redCount: 0,
            eventsCount: 14,
        },
        {
            id: "wf-juridico-2",
            title: "Due Diligence Contratual",
            teamName: "Time Jurídico",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 2,
            greenCount: 0,
            redCount: 1,
            eventsCount: 9,
        },
        {
            id: "wf-nf-3",
            title: "Aprovação de Despesas",
            teamName: "Time Financeiro",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 12,
            greenCount: 3,
            redCount: 2,
            eventsCount: 31,
        },
        {
            id: "wf-rh-3",
            title: "Férias e Afastamentos",
            teamName: "Time RH",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 5,
            greenCount: 1,
            redCount: 1,
            eventsCount: 11,
        },
        {
            id: "wf-juridico-3",
            title: "Compliance e Regulatório",
            teamName: "Time Jurídico",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 7,
            greenCount: 2,
            redCount: 0,
            eventsCount: 19,
        },
        {
            id: "wf-nf-4",
            title: "Fechamento Mensal",
            teamName: "Time Financeiro",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 15,
            greenCount: 4,
            redCount: 1,
            eventsCount: 28,
        },
        {
            id: "wf-rh-4",
            title: "Avaliação de Desempenho",
            teamName: "Time RH",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 4,
            greenCount: 1,
            redCount: 0,
            eventsCount: 16,
        },
        {
            id: "wf-juridico-4",
            title: "Renovação de Contratos",
            teamName: "Time Jurídico",
            statusLabel: "Ativa",
            statusVariant: "success",
            docsCount: 3,
            greenCount: 0,
            redCount: 1,
            eventsCount: 7,
        },
    ];

    export default {
        name: "AuditorWorkflowSummary",
        components: {
            BadgeComponent,
            LoadingComponent,
        },
        props: {
            selectedWorkflow: {
                type: Object,
                default: null,
            },
            search: {
                type: String,
                default: "",
            },
        },
        emits: ["select-workflow"],
        data() {
            return {
                loading: false,
                workflowItems: [],
                displayedLimit: 10,
            };
        },
        computed: {
            filteredWorkflowItems() {
                const q = (this.search || "").toLowerCase().trim();
                if (!q) return this.workflowItems;
                return this.workflowItems.filter(
                    (item) =>
                        (item.title && item.title.toLowerCase().includes(q)) ||
                        (item.teamName && item.teamName.toLowerCase().includes(q))
                );
            },
            displayedWorkflowItems() {
                return this.filteredWorkflowItems.slice(0, this.displayedLimit);
            },
            showLoadMoreButton() {
                return (
                    this.filteredWorkflowItems.length > 10 &&
                    this.displayedLimit < this.filteredWorkflowItems.length
                );
            },
        },
        methods: {
            async getData() {
                this.loading = true;
                try {
                    // TODO: replace with real API call (e.g. audit/workflows list)
                    await new Promise((r) => setTimeout(r, 400));
                    this.workflowItems = [...MOCK_WORKFLOW_ITEMS];
                } finally {
                    this.loading = false;
                }
            },
            loadMore() {
                this.displayedLimit = Math.min(
                    this.displayedLimit + 10,
                    this.filteredWorkflowItems.length
                );
            },
        },
        mounted() {
            this.getData();
        },
    };
</script>
<style scoped>
    .audit-list-item-selected {
        background-color: var(--bs-primary-bg-subtle, var(--bs-tertiary-bg, transparent));
    }
    .audit-list-item:hover {
        background-color: var(--bs-tertiary-bg, rgba(0, 0, 0, 0.04));
    }
    .cursor-pointer {
        cursor: pointer;
    }
    .workflow-metric-dot {
        width: 6px;
        height: 6px;
    }
    .audit-list-wrapper {
        min-height: 0;
        max-height: calc(100vh - 400px);
    }
    .audit-list-wrapper .audit-list {
        min-height: 0;
    }
</style>
