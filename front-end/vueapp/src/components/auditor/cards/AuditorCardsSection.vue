<template>
    <div class="row g-3">
        <div class="col-4">
            <div class="card h-100 rounded-3">
                <div class="card-body d-flex flex-column">
                    <div class="input-group input-group-sm auditor-filter-sm mb-3">
                        <span class="input-group-text bg-white border-end-0 py-1">
                            <LucideIcon
                                icon="Search"
                                :size="14"
                            />
                        </span>
                        <input
                            type="text"
                            class="form-control form-control-sm border-start-0 py-1"
                            placeholder="ID, nome do documento ou esteira..."
                            aria-label="Buscar Documento"
                            v-model="searchInput"
                        />
                    </div>
                    <div class="dropdown mb-3">
                        <button
                            class="btn btn-light btn-sm w-100 text-start d-flex align-items-center justify-content-between border py-1 auditor-filter-sm"
                            type="button"
                            data-bs-toggle="dropdown"
                            aria-expanded="false"
                        >
                            <LucideIcon
                                icon="Filter"
                                :size="12"
                                class="me-2"
                            />
                            {{ selectedStatusLabel }}
                            <LucideIcon
                                icon="ChevronDown"
                                :size="12"
                                class="ms-1"
                            />
                        </button>
                        <ul class="dropdown-menu dropdown-menu-start">
                            <li
                                v-for="opt in statusFilterOptions"
                                :key="opt.value"
                            >
                                <a
                                    class="dropdown-item"
                                    href="#"
                                    @click.prevent="selectedStatusId = opt.value"
                                >
                                    {{ opt.label }}
                                </a>
                            </li>
                        </ul>
                    </div>
                    <div class="audit-list-wrapper d-flex flex-column flex-grow-1 min-h-0">
                        <div class="audit-list overflow-auto flex-grow-1 min-h-0">
                            <div
                                v-for="(item, index) in displayedAuditItems"
                                :key="item.id"
                                class="audit-list-item rounded-2 p-2 mb-2 cursor-pointer"
                                :class="{
                                    'bg-light border-start border-primary border-3':
                                        selectedDocumentId === item.id,
                                    border: selectedDocumentId !== item.id,
                                }"
                                @click="selectedDocumentId = item.id"
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
                                        <div
                                            class="small text-muted d-flex align-items-center gap-1 mb-0"
                                        >
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
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card h-100 rounded-3">
                <div class="card-body d-flex flex-column p-0">
                    <template v-if="selectedDocument">
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
                                            selectedStageId === stage.value
                                                ? 'btn-primary'
                                                : 'btn-light'
                                        "
                                        @click="selectedStageId = stage.value"
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
                                    v-model="historySearchInput"
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
                                        <div
                                            class="small text-muted d-flex align-items-center gap-1"
                                        >
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
                    <template v-else>
                        <div
                            class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-4"
                        >
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
            </div>
        </div>
    </div>
</template>
<script>
    import BadgeComponent from "@/components/global/BadgeComponent.vue";

    export default {
        name: "AuditorCardsSection",
        components: {
            BadgeComponent,
        },
        data() {
            return {
                searchInput: "",
                selectedDocumentId: "boleto-2345",
                selectedStatusId: "",
                selectedStageId: "0",
                historySearchInput: "",
                displayedLimit: 10,
                statusFilterOptions: [
                    { value: "", label: "Todos os status" },
                    { value: "ativo", label: "Ativo" },
                    { value: "finalizado", label: "Finalizado" },
                ],
                auditItems: [
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
                        alterations: "5 alterações + 2 esteiras",
                        workflowsCount: 2,
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
                ],
                auditHistoryByDocument: {
                    "boleto-2345": [
                        {
                            id: "h1",
                            userName: "João Ferreira",
                            actionTags: [
                                { label: "Deletar", variant: "secondary" },
                                { label: "Protocolo", variant: "secondary" },
                            ],
                            description: 'Documento deletado: "Upload duplicado"',
                            timestamp: "2024-02-08 11:25:15",
                        },
                        {
                            id: "h2",
                            userName: "João Ferreira",
                            actionTags: [
                                { label: "Upload", variant: "primary" },
                                { label: "Protocolo", variant: "secondary" },
                            ],
                            description: "Documento carregado no sistema",
                            timestamp: "2024-02-08 11:20:00",
                        },
                    ],
                    "nota-5673": [
                        {
                            id: "h3",
                            userName: "Maria Silva",
                            actionTags: [{ label: "Upload", variant: "primary" }],
                            description: "Nota fiscal registrada",
                            timestamp: "2024-02-07 14:00:00",
                        },
                    ],
                },
            };
        },
        computed: {
            selectedDocument() {
                return this.auditItems.find((item) => item.id === this.selectedDocumentId) || null;
            },
            stageFilterOptions() {
                if (!this.selectedDocument || !this.selectedDocument.step) return [];
                return this.selectedDocument.step.split(" • ").map((label, i) => ({
                    value: String(i),
                    label: label.trim(),
                }));
            },
            auditHistoryEntries() {
                if (!this.selectedDocumentId) return [];
                const list = this.auditHistoryByDocument[this.selectedDocumentId];
                return list || [];
            },
            selectedStatusLabel() {
                const opt = this.statusFilterOptions.find((o) => o.value === this.selectedStatusId);
                return opt ? opt.label : this.statusFilterOptions[0].label;
            },
            displayedAuditItems() {
                return this.auditItems.slice(0, this.displayedLimit);
            },
            showLoadMoreButton() {
                return this.auditItems.length > 10 && this.displayedLimit < this.auditItems.length;
            },
        },
        methods: {
            loadMore() {
                this.displayedLimit = Math.min(this.displayedLimit + 10, this.auditItems.length);
            },
        },
        watch: {
            selectedDocumentId() {
                this.selectedStageId = "0";
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
    .audit-list-item:hover {
        background-color: rgba(0, 0, 0, 0.04);
    }
    .cursor-pointer {
        cursor: pointer;
    }
    .min-vh-50 {
        min-height: 50vh;
    }
    .audit-list-wrapper {
        min-height: 0;
        max-height: calc(100vh - 400px);
    }
    .audit-list-wrapper .audit-list {
        min-height: 0;
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
</style>
