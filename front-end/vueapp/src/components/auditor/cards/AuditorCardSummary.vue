<template>
    <div>
        <!-- List (filters are in Section) -->
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
                        v-for="item in displayedAuditItems"
                        :key="item.id"
                        class="audit-list-item rounded-2 p-2 mb-2 cursor-pointer"
                        :class="{
                            'audit-list-item-selected border-start border-primary border-3':
                                selectedDocument && selectedDocument.id === item.id,
                            border: !selectedDocument || selectedDocument.id !== item.id,
                        }"
                        @click="$emit('select-document', item)"
                    >
                        <div class="d-flex align-items-start gap-2">
                            <LucideIcon
                                :icon="item.icon"
                                :size="16"
                                class="text-muted mt-1 flex-shrink-0"
                            />
                            <div class="min-w-0 flex-grow-1">
                                <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                    <span class="fw-semibold small text-break">
                                        {{ item.title }}
                                    </span>
                                    <BadgeComponent
                                        :text="item.status"
                                        :variant="item.statusVariant"
                                        size="sm"
                                        :clickable="false"
                                    />
                                    <BadgeComponent
                                        v-if="item.workflowsCount"
                                        :text="item.workflowsCount"
                                        variant="warning"
                                        size="sm"
                                        :clickable="false"
                                    />
                                </div>
                                <div class="small text-muted d-flex align-items-center gap-1 mb-0">
                                    <LucideIcon
                                        icon="Workflow"
                                        :size="12"
                                    />
                                    {{ item.step }}
                                </div>
                                <div class="small text-muted">
                                    {{ item.alterations }}
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

    const MOCK_AUDIT_ITEMS = [
        {
            id: "boleto-2345",
            title: "Boleto #2345",
            icon: "FileText",
            status: "Ativo",
            statusVariant: "primary",
            step: "Análise Jurídica de Contratos • Protocolo",
            alterations: "7 alterações",
            workflowsCount: null,
        },
        {
            id: "nota-5673",
            title: "Nota Fiscal #5673",
            icon: "FileText",
            status: "Finalizado",
            statusVariant: "success",
            step: "Processamento de Notas Fiscais • Pagos e Conciliados",
            alterations: "12 alterações",
            workflowsCount: null,
        },
        {
            id: "contrato-joao",
            title: "Contrato João Silva",
            icon: "FileText",
            status: "Ativo",
            statusVariant: "primary",
            step: "Gestão de Documentos RH • Validação Documentos",
            alterations: "7 alterações no total",
            workflowsCount: 2,
            workflows: [
                {
                    id: "wf-rh-1",
                    name: "Gestão de Documentos RH",
                    stage: "Validação Documentos",
                    eventsCount: 4,
                    lastAction: "Perguntar ao documento por Luciana Melo",
                    lastActionTimestamp: "2024-02-12 10:45:00",
                },
                {
                    id: "wf-juridico-1",
                    name: "Análise Jurídica de Contratos",
                    stage: "Análise Jurídica",
                    eventsCount: 3,
                    lastAction: "Perguntar ao documento por Dra. Mariana Costa",
                    lastActionTimestamp: "2024-02-12 15:00:00",
                },
            ],
        },
        {
            id: "atestado-102",
            title: "Atestado Médico #102",
            icon: "FileText",
            status: "Finalizado",
            statusVariant: "success",
            step: "Gestão de Documentos RH • Validação Documentos",
            alterations: "3 alterações",
            workflowsCount: null,
        },
        {
            id: "contrato-techcorp",
            title: "Contrato Fornecedor TechCorp",
            icon: "FileText",
            status: "Ativo",
            statusVariant: "primary",
            step: "Análise Jurídica de Contratos • Protocolo",
            alterations: "9 alterações + 2 esteiras",
            workflowsCount: 2,
        },
        {
            id: "aditivo-45",
            title: "Aditivo Contratual #45",
            icon: "FileText",
            status: "Ativo",
            statusVariant: "primary",
            step: "Análise Jurídica de Contratos • Protocolo",
            alterations: "4 alterações + 2 esteiras",
            workflowsCount: 2,
        },
        {
            id: "doc-extra-1",
            title: "Documento #1001",
            icon: "FileText",
            status: "Ativo",
            statusVariant: "primary",
            step: "Análise Jurídica • Protocolo",
            alterations: "2 alterações",
            workflowsCount: null,
        },
        {
            id: "doc-extra-2",
            title: "Documento #1002",
            icon: "FileText",
            status: "Finalizado",
            statusVariant: "success",
            step: "Processamento • Concluído",
            alterations: "8 alterações",
            workflowsCount: null,
        },
        {
            id: "doc-extra-3",
            title: "Documento #1003",
            icon: "FileText",
            status: "Ativo",
            statusVariant: "primary",
            step: "Gestão RH • Validação",
            alterations: "1 alteração",
            workflowsCount: null,
        },
        {
            id: "doc-extra-4",
            title: "Documento #1004",
            icon: "FileText",
            status: "Finalizado",
            statusVariant: "success",
            step: "Análise Jurídica • Protocolo",
            alterations: "6 alterações",
            workflowsCount: null,
        },
        {
            id: "doc-extra-5",
            title: "Documento #1005",
            icon: "FileText",
            status: "Ativo",
            statusVariant: "primary",
            step: "Processamento • Em análise",
            alterations: "3 alterações",
            workflowsCount: 1,
        },
    ];

    export default {
        name: "AuditorCardSummary",
        components: {
            BadgeComponent,
            LoadingComponent,
        },
        props: {
            /** Current selection from parent (Section); used to highlight the active row */
            selectedDocument: {
                type: Object,
                default: null,
            },
            /** Filter params from Section (set by Filters); when Section updates this, it calls getData again */
            filterParams: {
                type: Object,
                default: () => ({ search: "", statusId: "" }),
            },
        },
        emits: ["select-document"],
        data() {
            return {
                loading: false,
                auditItems: [],
                displayedLimit: 10,
            };
        },
        computed: {
            filteredAuditItems() {
                const search = (this.filterParams?.search || "").toLowerCase().trim();
                const statusId = this.filterParams?.statusId || "";
                return this.auditItems.filter((item) => {
                    const matchesSearch =
                        !search ||
                        (item.id && item.id.toLowerCase().includes(search)) ||
                        (item.title && item.title.toLowerCase().includes(search)) ||
                        (item.step && item.step.toLowerCase().includes(search));
                    const matchesStatus =
                        !statusId || (item.status && item.status.toLowerCase() === statusId);
                    return matchesSearch && matchesStatus;
                });
            },
            displayedAuditItems() {
                return this.filteredAuditItems.slice(0, this.displayedLimit);
            },
            showLoadMoreButton() {
                return (
                    this.filteredAuditItems.length > 10 &&
                    this.displayedLimit < this.filteredAuditItems.length
                );
            },
        },
        methods: {
            async getData() {
                this.loading = true;
                try {
                    const params = this.filterParams || {};
                    // TODO: replace with real API call (e.g. audit/documents list), pass params
                    await new Promise((r) => setTimeout(r, 400));
                    this.auditItems = [...MOCK_AUDIT_ITEMS];
                    if (this.auditItems.length) {
                        this.$emit("select-document", this.auditItems[0]);
                    }
                } finally {
                    this.loading = false;
                }
            },
            loadMore() {
                this.displayedLimit = Math.min(
                    this.displayedLimit + 10,
                    this.filteredAuditItems.length
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
    .audit-list-wrapper {
        min-height: 0;
        max-height: calc(100vh - 430px);
    }
    .audit-list-wrapper .audit-list {
        min-height: 0;
    }
</style>
