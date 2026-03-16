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
            <div
                v-if="isLoading"
                class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-5"
            >
                <LoadingComponent />
            </div>
            <div
                v-else
                class="workflow-detail-content p-3 d-flex flex-column flex-grow-1 min-h-0"
            >
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
                    <div
                        class="d-flex align-items-center flex-wrap justify-content-between gap-2 mb-2"
                    >
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
                        <div class="d-flex align-items-center flex-wrap gap-2">
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
                    </div>
                    <div class="input-group input-group-sm auditor-filter-sm mb-2">
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

                    <div
                        class="workflow-timeline-list overflow-auto flex-grow-1 min-h-0 d-flex flex-column"
                    >
                        <div
                            v-for="entry in displayedTimelineEntries"
                            :key="entry.id"
                            class="workflow-timeline-card rounded-2 p-2 mb-2 border"
                        >
                            <div class="workflow-timeline-card-content">
                                <div
                                    class="d-flex flex-nowrap align-items-center gap-1 gap-sm-2 mb-1 workflow-timeline-card-first-row"
                                >
                                    <span
                                        class="workflow-timeline-user-badge d-inline-flex align-items-center justify-content-center flex-shrink-0"
                                    >
                                        <LucideIcon
                                            icon="User"
                                            :size="12"
                                        />
                                    </span>
                                    <span
                                        class="small fw-semibold text-nowrap text-truncate min-w-0"
                                    >
                                        {{ entry.userName }}
                                    </span>
                                    <template
                                        v-for="tag in entry.actionTags"
                                        :key="tag.label"
                                    >
                                        <BadgeComponent
                                            :text="tag.label"
                                            :variant="tag.variant"
                                            size="sm"
                                            :clickable="false"
                                            class="flex-shrink-0"
                                        />
                                    </template>
                                    <span
                                        v-if="entry.documentName"
                                        class="small text-primary d-inline-flex align-items-center gap-1 text-nowrap text-truncate min-w-0"
                                    >
                                        <LucideIcon
                                            icon="FileText"
                                            :size="12"
                                            class="flex-shrink-0"
                                        />
                                        <span class="text-truncate">{{ entry.documentName }}</span>
                                    </span>
                                </div>
                                <div
                                    v-if="entry.actionTags.length"
                                    class="small text-muted mb-1"
                                >
                                    {{ entry.actionTags[0].label }}
                                </div>
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
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import AuditorsService from "@/services/auditors/AuditorsService";
    import dateHelper from "@/helpers/date.js";

    function mapCardsToTimelineEntries(cards) {
        if (!Array.isArray(cards)) return [];
        return cards.map((c, i) => ({
            id: `e-${c.cardId}-${i}-${c.created}`,
            userName: c.userName ?? "",
            actionTags: [{ label: c.actionType ?? "", variant: "primary" }],
            documentName: c.cardName ?? "",
            description: [c.actionType, c.stepName].filter(Boolean).join(" · "),
            timestamp: dateHelper.formatDateWithTime(c.created) ?? "",
            stageName: c.stepName ?? "",
            stageId: String(c.stepId ?? ""),
            actionId: (c.actionType || "").toLowerCase().replace(/\s+/g, "-"),
        }));
    }

    function mapStepsToStages(stepsCount) {
        if (!Array.isArray(stepsCount) || stepsCount.length === 0) return [];
        return stepsCount.map((s, i) => ({
            id: String(s.stepId ?? i),
            name: s.stepName ?? "",
            count: s.cardCount ?? 0,
            isTerminal: i === stepsCount.length - 1,
        }));
    }

    /** Maps UI action slug to AuditCardActionType enum value for the API. */
    const ACTION_SLUG_TO_TYPE = {
        avancar: 3,
        editar: 4,
        perguntar: 12,
        atribuir: 1,
        upload: 0,
    };

    export default {
        name: "AuditorWorkflowDetail",
        components: { BadgeComponent, LoadingComponent },
        props: {
            selectedWorkflow: {
                type: Object,
                default: null,
            },
        },
        data() {
            return {
                isLoading: false,
                workflowDetail: null,
                timelineSearch: "",
                selectedStageId: "",
                selectedActionId: "",
                timelineDisplayedLimit: 10,
                actionFilterOptions: [
                    { value: "", label: "Todas as ações" },
                    { value: "avancar", label: "Avançar" },
                    { value: "editar", label: "Editar resposta" },
                    { value: "perguntar", label: "Perguntar ao documento" },
                    { value: "atribuir", label: "Atribuir" },
                    { value: "upload", label: "Upload" },
                ],
            };
        },
        computed: {
            summary() {
                const sc = this.workflowDetail?.cardStatusCount;
                if (!sc) return { totalDocuments: 0, finalizados: 0, reprovados: 0 };
                return {
                    totalDocuments: sc.totalCards ?? 0,
                    finalizados: sc.finalized ?? 0,
                    reprovados: sc.rejected ?? 0,
                };
            },
            stages() {
                return mapStepsToStages(this.workflowDetail?.stepsCount);
            },
            timelineEntries() {
                return mapCardsToTimelineEntries(this.workflowDetail?.cards);
            },
            stageFilterOptions() {
                const base = [{ value: "", label: "Todas as etapas" }];
                return base.concat(this.stages.map((s) => ({ value: s.id, label: s.name })));
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
            async refreshWithCurrentDocument() {
                if (this.selectedWorkflow?.workflowId == null) {
                    this.workflowDetail = null;
                    return;
                }
                this.timelineDisplayedLimit = 10;
                this.isLoading = true;
                const search = (this.timelineSearch || "").trim() || undefined;
                const stepId = this.selectedStageId ? Number(this.selectedStageId) : undefined;
                const actionType = this.selectedActionId
                    ? ACTION_SLUG_TO_TYPE[this.selectedActionId]
                    : undefined;
                const params = {};
                if (search !== undefined) params.search = search;
                if (stepId !== undefined && !Number.isNaN(stepId)) params.stepId = stepId;
                if (actionType !== undefined) params.actionType = actionType;
                try {
                    const response = await AuditorsService.getWorkflowAuditDetails(
                        this.selectedWorkflow.workflowId,
                        params
                    );
                    if (response.error) {
                        this.$notify({
                            title: "audit-workflows.title",
                            message:
                                response.error.response?.data?.detail ?? response.error.message,
                            variant: "danger",
                            icon: "CircleX",
                        });
                        this.workflowDetail = null;
                        return;
                    }
                    this.workflowDetail =
                        response && typeof response === "object" && !Array.isArray(response)
                            ? response
                            : (response?.data ?? null);
                } finally {
                    this.isLoading = false;
                }
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
