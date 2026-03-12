<template>
    <div class="card-body d-flex flex-column p-0">
        <!-- Placeholder when no workflow selected -->
        <template v-if="!selectedWorkflow">
            <div
                class="d-flex flex-column align-items-center justify-content-center min-vh-50 py-5"
            >
                <div class="workflow-detail-placeholder-icon text-secondary mb-3">
                    <LucideIcon
                        icon="Workflow"
                        :size="64"
                        stroke-width="1.25"
                    />
                </div>
                <p class="text-muted text-center mb-0">
                    Selecione uma esteira para ver a auditoria processual
                </p>
            </div>
        </template>

        <!-- Detail content when workflow selected -->
        <template v-else>
            <div class="workflow-detail-content p-3 d-flex flex-column flex-grow-1 min-h-0">
                <!-- 1. Summary cards -->
                <div class="row g-2 mb-3">
                    <div class="col-4">
                        <div
                            class="workflow-summary-card rounded-2 p-2 border d-flex flex-column align-items-center"
                        >
                            <LucideIcon
                                icon="FileText"
                                :size="20"
                                class="text-primary mb-1"
                            />
                            <span class="fs-5 fw-bold">{{ summary.totalDocuments }}</span>
                            <span class="small text-muted">Total Documentos</span>
                        </div>
                    </div>
                    <div class="col-4">
                        <div
                            class="workflow-summary-card rounded-2 p-2 border d-flex flex-column align-items-center"
                        >
                            <LucideIcon
                                icon="CheckCircle"
                                :size="20"
                                class="text-success mb-1"
                            />
                            <span class="fs-5 fw-bold">{{ summary.finalizados }}</span>
                            <span class="small text-muted">Finalizados</span>
                        </div>
                    </div>
                    <div class="col-4">
                        <div
                            class="workflow-summary-card rounded-2 p-2 border d-flex flex-column align-items-center"
                        >
                            <LucideIcon
                                icon="XCircle"
                                :size="20"
                                class="text-danger mb-1"
                            />
                            <span class="fs-5 fw-bold">{{ summary.reprovados }}</span>
                            <span class="small text-muted">Reprovados</span>
                        </div>
                    </div>
                </div>

                <!-- 2. Distribuição por Etapa -->
                <div class="mb-3">
                    <h6 class="small fw-semibold text-muted mb-2 d-flex align-items-center gap-1">
                        <LucideIcon
                            icon="BarChart3"
                            :size="14"
                        />
                        Distribuição por Etapa
                    </h6>
                    <div
                        class="workflow-stages-bar d-flex align-items-stretch gap-0 rounded-2 border overflow-hidden"
                    >
                        <template
                            v-for="(stage, index) in stages"
                            :key="stage.id"
                        >
                            <div
                                v-if="index > 0"
                                class="workflow-stage-arrow d-flex align-items-center flex-shrink-0 px-1"
                            >
                                <LucideIcon
                                    icon="ChevronRight"
                                    :size="16"
                                    class="text-muted"
                                />
                            </div>
                            <div
                                class="workflow-stage-block flex-grow-1 text-center py-2 px-2"
                                :class="{ 'workflow-stage-block-terminal': stage.isTerminal }"
                            >
                                <div class="small fw-bold">{{ stage.count }}</div>
                                <div class="small text-muted text-break">{{ stage.name }}</div>
                            </div>
                        </template>
                    </div>
                </div>

                <!-- 3. Timeline Processual -->
                <div class="workflow-timeline-section d-flex flex-column flex-grow-1 min-h-0">
                    <div class="d-flex align-items-center flex-wrap gap-2 mb-2">
                        <h6 class="mb-0 fw-bold d-flex align-items-center gap-1">
                            <LucideIcon
                                icon="History"
                                :size="18"
                            />
                            Timeline Processual
                            <BadgeComponent
                                :text="filteredTimelineEntries.length + ' eventos'"
                                variant="secondary"
                                size="sm"
                                :clickable="false"
                            />
                        </h6>
                    </div>
                    <div class="d-flex align-items-center flex-wrap gap-2 mb-2">
                        <div
                            class="input-group input-group-sm auditor-filter-sm flex-grow-1 flex-md-grow-0 flex-md-grow-1"
                        >
                            <span class="input-group-text border-end-0 py-1">
                                <LucideIcon
                                    icon="Search"
                                    :size="14"
                                />
                            </span>
                            <input
                                v-model="timelineSearch"
                                type="text"
                                class="form-control form-control-sm border-start-0 py-1"
                                placeholder="Buscar por usuário, documento, detalhes, etapa..."
                                aria-label="Buscar na timeline"
                            />
                        </div>
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
                                {{ selectedStageLabel }}
                                <LucideIcon
                                    icon="ChevronDown"
                                    :size="12"
                                />
                            </button>
                            <ul class="dropdown-menu dropdown-menu-start">
                                <li
                                    v-for="opt in stageFilterOptions"
                                    :key="opt.value"
                                >
                                    <a
                                        class="dropdown-item"
                                        href="#"
                                        @click.prevent="selectedStageId = opt.value"
                                    >
                                        {{ opt.label }}
                                    </a>
                                </li>
                            </ul>
                        </div>
                        <div class="dropdown">
                            <button
                                class="btn btn-light btn-sm border py-1 px-2 auditor-filter-sm d-flex align-items-center gap-1 dropdown-toggle"
                                type="button"
                                data-bs-toggle="dropdown"
                                aria-expanded="false"
                            >
                                {{ selectedActionLabel }}
                                <LucideIcon
                                    icon="ChevronDown"
                                    :size="12"
                                />
                            </button>
                            <ul class="dropdown-menu dropdown-menu-start">
                                <li
                                    v-for="opt in actionFilterOptions"
                                    :key="opt.value"
                                >
                                    <a
                                        class="dropdown-item"
                                        href="#"
                                        @click.prevent="selectedActionId = opt.value"
                                    >
                                        {{ opt.label }}
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </div>

                    <div
                        class="workflow-timeline-list overflow-auto flex-grow-1 min-h-0 d-flex flex-column"
                    >
                        <div
                            v-for="entry in displayedTimelineEntries"
                            :key="entry.id"
                            class="workflow-timeline-card rounded-2 p-2 mb-2 border"
                        >
                            <div class="d-flex align-items-start gap-2 flex-wrap">
                                <span
                                    class="workflow-timeline-user-badge d-inline-flex align-items-center justify-content-center flex-shrink-0"
                                >
                                    <LucideIcon
                                        icon="User"
                                        :size="12"
                                    />
                                </span>
                                <span class="small fw-semibold align-self-center">
                                    {{ entry.userName }}
                                </span>
                                <div class="min-w-0 flex-grow-1 workflow-timeline-card-content">
                                    <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                        <BadgeComponent
                                            v-for="tag in entry.actionTags"
                                            :key="tag.label"
                                            :text="tag.label"
                                            :variant="tag.variant"
                                            size="sm"
                                            :clickable="false"
                                        />
                                        <span
                                            v-if="entry.documentName"
                                            class="small text-primary d-inline-flex align-items-center gap-1"
                                        >
                                            <LucideIcon
                                                icon="FileText"
                                                :size="12"
                                            />
                                            {{ entry.documentName }}
                                        </span>
                                    </div>
                                    <p class="small text-muted mb-1">{{ entry.description }}</p>
                                    <div
                                        class="small text-muted d-flex align-items-center flex-wrap gap-2"
                                    >
                                        <span class="d-inline-flex align-items-center gap-1">
                                            <LucideIcon
                                                icon="Clock"
                                                :size="12"
                                            />
                                            {{ entry.timestamp }}
                                        </span>
                                        <span
                                            v-if="entry.stageName"
                                            class="d-inline-flex align-items-center gap-1"
                                        >
                                            <LucideIcon
                                                icon="Workflow"
                                                :size="12"
                                            />
                                            {{ entry.stageName }}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div
                            v-if="showTimelineLoadMore"
                            class="mt-2 mb-3 text-center"
                        >
                            <button
                                type="button"
                                class="btn btn-outline-primary btn-sm"
                                @click="loadMoreTimeline"
                            >
                                Carregar mais
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>
<script>
    import BadgeComponent from "@/components/global/BadgeComponent.vue";

    const MOCK_STAGES = [
        { id: "s1", name: "Recebimento", count: 1, isTerminal: false },
        { id: "s2", name: "Verificação Financeira", count: 1, isTerminal: false },
        { id: "s3", name: "Aprovação de Pagamento", count: 1, isTerminal: false },
        { id: "s4", name: "Pagos e Conciliados", count: 1, isTerminal: true },
    ];

    const MOCK_TIMELINE_ENTRIES = [
        {
            id: "e1",
            userName: "Pedro Oliveira",
            actionTags: [{ label: "Avançar", variant: "success" }],
            documentName: "Nota Fiscal #8021",
            description: "Documento avançado para 'Verificação Financeira'",
            timestamp: "2024-02-11 11:20:05",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "avancar",
        },
        {
            id: "e2",
            userName: "Carlos Silva",
            actionTags: [{ label: "Editar resposta", variant: "warning" }],
            documentName: "Contrato Fornecedor TechCorp",
            description: "Campo 'Valor Total' alterado de 'R$ 4.500,00' para 'R$ 4.967,89'",
            timestamp: "2024-02-11 10:15:22",
            stageName: "Verificação Financeira",
            stageId: "s2",
            actionId: "editar",
        },
        {
            id: "e3",
            userName: "Ana Costa",
            actionTags: [{ label: "Perguntar ao documento", variant: "primary" }],
            documentName: "Nota Fiscal #8021",
            description: "Pergunta: 'Qual o prazo de pagamento acordado?'",
            timestamp: "2024-02-10 16:45:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "perguntar",
        },
        {
            id: "e4",
            userName: "Pedro Oliveira",
            actionTags: [{ label: "Atribuir", variant: "primary" }],
            documentName: "Nota Fiscal #8021",
            description: "Documento atribuído para verificação",
            timestamp: "2024-02-10 14:30:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "atribuir",
        },
        {
            id: "e5",
            userName: "Maria Santos",
            actionTags: [{ label: "Upload", variant: "primary" }],
            documentName: "Nota Fiscal #8021",
            description: "Documento carregado no sistema",
            timestamp: "2024-02-10 09:00:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "upload",
        },
        {
            id: "e6",
            userName: "Carlos Silva",
            actionTags: [{ label: "Avançar", variant: "success" }],
            documentName: "Nota Fiscal #8021",
            description: "Documento avançado para 'Aprovação de Pagamento'",
            timestamp: "2024-02-11 14:00:00",
            stageName: "Verificação Financeira",
            stageId: "s2",
            actionId: "avancar",
        },
        {
            id: "e7",
            userName: "Pedro Oliveira",
            actionTags: [{ label: "Avançar", variant: "success" }],
            documentName: "Nota Fiscal #8021",
            description: "Documento avançado para 'Pagos e Conciliados'",
            timestamp: "2024-02-11 15:30:00",
            stageName: "Aprovação de Pagamento",
            stageId: "s3",
            actionId: "avancar",
        },
        {
            id: "e8",
            userName: "Ana Costa",
            actionTags: [{ label: "Atribuir", variant: "primary" }],
            documentName: "Contrato Fornecedor TechCorp",
            description: "Documento atribuído para análise jurídica",
            timestamp: "2024-02-09 11:00:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "atribuir",
        },
        {
            id: "e9",
            userName: "Maria Santos",
            actionTags: [{ label: "Editar resposta", variant: "warning" }],
            documentName: "Nota Fiscal #8021",
            description: "Campo 'Data de vencimento' atualizado",
            timestamp: "2024-02-10 11:22:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "editar",
        },
        {
            id: "e10",
            userName: "Carlos Silva",
            actionTags: [{ label: "Perguntar ao documento", variant: "primary" }],
            documentName: "Contrato Fornecedor TechCorp",
            description: "Pergunta: 'O valor inclui impostos?'",
            timestamp: "2024-02-10 08:45:00",
            stageName: "Verificação Financeira",
            stageId: "s2",
            actionId: "perguntar",
        },
        {
            id: "e11",
            userName: "Pedro Oliveira",
            actionTags: [{ label: "Upload", variant: "primary" }],
            documentName: "Contrato Fornecedor TechCorp",
            description: "Documento carregado no sistema",
            timestamp: "2024-02-08 16:00:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "upload",
        },
        {
            id: "e12",
            userName: "Ana Costa",
            actionTags: [{ label: "Avançar", variant: "success" }],
            documentName: "Nota Fiscal #8021",
            description: "Documento avançado para 'Recebimento'",
            timestamp: "2024-02-10 09:05:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "avancar",
        },
        {
            id: "e13",
            userName: "Maria Santos",
            actionTags: [{ label: "Atribuir", variant: "primary" }],
            documentName: "Nota Fiscal #8021",
            description: "Documento atribuído para verificação financeira",
            timestamp: "2024-02-10 10:00:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "atribuir",
        },
        {
            id: "e14",
            userName: "Carlos Silva",
            actionTags: [{ label: "Editar resposta", variant: "warning" }],
            documentName: "Nota Fiscal #8021",
            description: "Campo 'Observações' preenchido",
            timestamp: "2024-02-10 15:00:00",
            stageName: "Verificação Financeira",
            stageId: "s2",
            actionId: "editar",
        },
        {
            id: "e15",
            userName: "Pedro Oliveira",
            actionTags: [{ label: "Perguntar ao documento", variant: "primary" }],
            documentName: "Nota Fiscal #8021",
            description: "Pergunta: 'Confirmar valor do frete?'",
            timestamp: "2024-02-11 09:30:00",
            stageName: "Verificação Financeira",
            stageId: "s2",
            actionId: "perguntar",
        },
        {
            id: "e16",
            userName: "Ana Costa",
            actionTags: [{ label: "Avançar", variant: "success" }],
            documentName: "Contrato Fornecedor TechCorp",
            description: "Documento avançado para 'Verificação Financeira'",
            timestamp: "2024-02-09 14:20:00",
            stageName: "Recebimento",
            stageId: "s1",
            actionId: "avancar",
        },
        {
            id: "e17",
            userName: "Maria Santos",
            actionTags: [{ label: "Upload", variant: "primary" }],
            documentName: "Anexo Comprovante",
            description: "Comprovante de pagamento anexado",
            timestamp: "2024-02-11 16:00:00",
            stageName: "Pagos e Conciliados",
            stageId: "s4",
            actionId: "upload",
        },
        {
            id: "e18",
            userName: "Carlos Silva",
            actionTags: [{ label: "Avançar", variant: "success" }],
            documentName: "Nota Fiscal #8021",
            description: "Documento finalizado e conciliado",
            timestamp: "2024-02-11 15:45:00",
            stageName: "Pagos e Conciliados",
            stageId: "s4",
            actionId: "avancar",
        },
    ];

    export default {
        name: "AuditorWorkflowDetail",
        components: { BadgeComponent },
        props: {
            selectedWorkflow: {
                type: Object,
                default: null,
            },
        },
        data() {
            return {
                stages: MOCK_STAGES,
                timelineEntries: [],
                timelineSearch: "",
                selectedStageId: "",
                selectedActionId: "",
                timelineDisplayedLimit: 10,
            };
        },
        computed: {
            summary() {
                const w = this.selectedWorkflow;
                if (!w) return { totalDocuments: 0, finalizados: 1, reprovados: 1 };
                return {
                    totalDocuments: w.docsCount ?? 4,
                    finalizados: w.greenCount ?? 1,
                    reprovados: w.redCount ?? 1,
                };
            },
            stageFilterOptions() {
                const base = [{ value: "", label: "Todas as etapas" }];
                return base.concat(this.stages.map((s) => ({ value: s.id, label: s.name })));
            },
            actionFilterOptions() {
                return [
                    { value: "", label: "Todas as ações" },
                    { value: "avancar", label: "Avançar" },
                    { value: "editar", label: "Editar resposta" },
                    { value: "perguntar", label: "Perguntar ao documento" },
                    { value: "atribuir", label: "Atribuir" },
                    { value: "upload", label: "Upload" },
                ];
            },
            selectedStageLabel() {
                const opt = this.stageFilterOptions.find((o) => o.value === this.selectedStageId);
                return opt ? opt.label : "Todas as etapas";
            },
            selectedActionLabel() {
                const opt = this.actionFilterOptions.find((o) => o.value === this.selectedActionId);
                return opt ? opt.label : "Todas as ações";
            },
            filteredTimelineEntries() {
                let list = this.timelineEntries;
                const q = (this.timelineSearch || "").toLowerCase().trim();
                if (q) {
                    list = list.filter(
                        (e) =>
                            (e.userName && e.userName.toLowerCase().includes(q)) ||
                            (e.documentName && e.documentName.toLowerCase().includes(q)) ||
                            (e.description && e.description.toLowerCase().includes(q)) ||
                            (e.stageName && e.stageName.toLowerCase().includes(q)) ||
                            e.actionTags?.some((t) => t.label.toLowerCase().includes(q))
                    );
                }
                if (this.selectedStageId) {
                    list = list.filter((e) => e.stageId === this.selectedStageId);
                }
                if (this.selectedActionId) {
                    list = list.filter((e) => e.actionId === this.selectedActionId);
                }
                return [...list].sort((a, b) =>
                    (b.timestamp || "").localeCompare(a.timestamp || "")
                );
            },
            displayedTimelineEntries() {
                return this.filteredTimelineEntries.slice(0, this.timelineDisplayedLimit);
            },
            showTimelineLoadMore() {
                const total = this.filteredTimelineEntries.length;
                return total > 10 && this.timelineDisplayedLimit < total;
            },
        },
        methods: {
            loadMoreTimeline() {
                this.timelineDisplayedLimit += 10;
            },
        },
        watch: {
            selectedWorkflow: {
                handler(w) {
                    if (w) {
                        this.timelineEntries = [...MOCK_TIMELINE_ENTRIES];
                    } else {
                        this.timelineEntries = [];
                    }
                    this.timelineDisplayedLimit = 10;
                },
                immediate: true,
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
    .workflow-detail-placeholder-icon {
        opacity: 0.6;
    }
    .workflow-summary-card {
        background-color: var(--bs-secondary-bg, #f8f9fa);
    }
    .workflow-stages-bar {
        background-color: var(--bs-secondary-bg, #f8f9fa);
    }
    .workflow-stage-block {
        background-color: rgba(13, 110, 253, 0.12);
        min-width: 0;
    }
    .workflow-stage-block-terminal {
        background-color: rgba(25, 135, 84, 0.15);
    }
    .workflow-stage-arrow {
        background-color: var(--bs-secondary-bg, #f8f9fa);
    }
    .workflow-detail-content {
        flex: 1 1 0;
        min-height: 0;
        overflow: hidden;
    }
    .workflow-timeline-section {
        flex: 1 1 0;
        min-height: 0;
        overflow: hidden;
    }
    .workflow-timeline-list {
        flex: 1 1 0;
        min-height: 0;
    }
    .workflow-timeline-card {
        background-color: transparent;
    }
    .workflow-timeline-user-badge {
        background-color: rgba(13, 110, 253, 0.15);
        color: var(--bs-primary);
        border-radius: 999px;
        width: 24px;
        height: 24px;
        font-size: 0;
    }
    .workflow-timeline-card-content {
        flex: 1 1 100%;
        min-width: 0;
    }
</style>
