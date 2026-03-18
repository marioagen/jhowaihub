<template>
    <div class="card-body d-flex flex-column p-0">
        <template v-if="selectedDocument != null">
            <template v-if="hasMultipleWorkflows">
                <template v-if="mustSelectWorkflow && !isLoading">
                    <div class="p-3">
                        <h6
                            class="workflow-select-heading mb-2 fw-bold d-flex align-items-center gap-2"
                        >
                            <LucideIcon
                                icon="Layers"
                                :size="20"
                                class="text-warning"
                            />
                            {{ $t("auditor.documents.detail.selectWorkflow") }}
                        </h6>
                        <p class="small text-muted mb-3">
                            {{
                                $t("auditor.documents.detail.documentInWorkflows", {
                                    count: selectedDocumentWorkflows.length,
                                })
                            }}
                        </p>
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
                                        {{ selectedDocument.documentName }}
                                    </div>
                                    <div class="small text-muted">
                                        {{
                                            selectedDocument.actionsCount != null
                                                ? $t("auditor.documents.detail.actionsCount", {
                                                      count: selectedDocument.actionsCount,
                                                  })
                                                : $t("auditor.documents.detail.workflowsCount", {
                                                      count: selectedDocumentWorkflows.length,
                                                  })
                                        }}
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div
                            v-for="workflow in selectedDocumentWorkflows"
                            :key="workflow.id ?? workflow.Id"
                            class="workflow-select-card workflow-select-card-clickable rounded-2 p-2 mb-2 border cursor-pointer d-flex align-items-center gap-2"
                            @click="onSelectWorkflow(workflow)"
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
                                    {{ workflow.name }}
                                </div>
                                <div class="small text-muted">
                                    <template v-if="workflow.stepName">
                                        {{ $t("auditor.documents.detail.step") }}:
                                        {{ workflow.stepName }}
                                    </template>
                                    <template v-else>
                                        {{ $t("auditor.documents.detail.workflowId") }}:
                                        {{ workflow.id }}
                                    </template>
                                </div>
                                <div
                                    v-if="workflow.lastAction || workflow.lastActionTimestamp"
                                    class="small text-muted"
                                >
                                    {{ $t("auditor.documents.detail.lastAction") }}:
                                    {{ workflow.lastAction ?? "—" }}
                                    <span class="ms-1">
                                        {{ workflow.lastActionTimestamp ?? "" }}
                                    </span>
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
                <div
                    v-else-if="isLoading"
                    class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-5"
                >
                    <LoadingComponent />
                </div>
                <template v-else>
                    <div class="p-3 border-bottom">
                        <div
                            v-if="showReturnInDetail"
                            class="d-flex align-items-center gap-2 mb-2"
                        >
                            <button
                                type="button"
                                class="btn btn-light btn-sm border d-flex align-items-center gap-1 py-1 px-2"
                                :aria-label="$t('auditor.documents.detail.backToWorkflowList')"
                                @click="onReturnToWorkflowList"
                            >
                                <LucideIcon
                                    icon="ArrowLeft"
                                    :size="14"
                                />
                                {{ $t("auditor.documents.detail.back") }}
                            </button>
                        </div>
                        <div
                            class="d-flex align-items-start justify-content-between flex-wrap gap-2 mb-2"
                        >
                            <div class="d-flex align-items-start gap-3 flex-wrap min-w-0">
                                <div class="min-w-0">
                                    <div class="d-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="FileText"
                                            :size="18"
                                            class="flex-shrink-0"
                                        />
                                        <span class="text-break">{{ documentDisplayName }}</span>
                                    </div>
                                </div>
                                <div class="min-w-0">
                                    <div class="d-flex align-items-center gap-1 small text-muted">
                                        <LucideIcon
                                            icon="Workflow"
                                            :size="18"
                                            class="flex-shrink-0"
                                        />
                                        <span class="text-break">{{ workflowDisplayName }}</span>
                                        <BadgeComponent
                                            :text="
                                                $t('auditor.documents.detail.events', {
                                                    count: documentHistory.length,
                                                })
                                            "
                                            variant="secondary"
                                            size="sm"
                                            :clickable="false"
                                        />
                                    </div>
                                </div>
                            </div>
                            <div
                                class="d-flex align-items-center gap-2 flex-wrap justify-content-end"
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
                                        @click="setStageAndRefresh(stage.value)"
                                    >
                                        {{ stage.label }}
                                    </button>
                                </div>
                                <div class="d-flex align-items-center gap-2 flex-wrap">
                                    <button
                                        type="button"
                                        class="btn btn-sm border py-1 px-2 auditor-filter-sm d-flex align-items-center gap-1"
                                        :class="orderDescending ? 'btn-primary' : 'btn-light'"
                                        @click="toggleOrderAndRefresh"
                                    >
                                        <LucideIcon
                                            icon="ArrowUpDown"
                                            :size="12"
                                        />
                                        {{
                                            orderDescending
                                                ? $t("auditor.documents.detail.orderNewest")
                                                : $t("auditor.documents.detail.orderOldest")
                                        }}
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
                                            {{ selectedActionLabel }}
                                            <LucideIcon
                                                icon="ChevronDown"
                                                :size="12"
                                            />
                                        </button>
                                        <ul class="dropdown-menu dropdown-menu-start">
                                            <li
                                                v-for="action in actionFilterOptions"
                                                :key="action.value == null ? 'all' : action.value"
                                            >
                                                <a
                                                    class="dropdown-item"
                                                    href="#"
                                                    @click.prevent="
                                                        setActionAndRefresh(action.value)
                                                    "
                                                >
                                                    {{ action.label }}
                                                </a>
                                            </li>
                                        </ul>
                                    </div>
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
                                :placeholder="$t('auditor.documents.detail.searchPlaceholder')"
                                :aria-label="$t('auditor.documents.detail.searchAria')"
                                v-model="historySearchInput"
                                @input="onHistorySearchInput"
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
                                <BadgeComponent
                                    variant="primary"
                                    size="sm"
                                    :clickable="false"
                                    icon-only
                                >
                                    <LucideIcon
                                        icon="User"
                                        :size="12"
                                    />
                                </BadgeComponent>
                                <div
                                    class="d-flex align-items-center flex-wrap gap-1 gap-xl-2 align-self-center min-w-0 flex-grow-1"
                                >
                                    <span class="small fw-semibold">{{ entry.userName }}</span>
                                    <BadgeComponent
                                        v-if="entry.actionName"
                                        :text="auditActionDisplay(entry).title"
                                        variant="primary"
                                        size="sm"
                                        :clickable="false"
                                    />
                                    <span
                                        v-if="entry.stepName"
                                        class="small text-muted"
                                    >
                                        {{ entry.stepName }}
                                    </span>
                                </div>
                                <div class="w-100 audit-history-card-content">
                                    <div
                                        v-if="entry.actionName"
                                        class="small text-muted"
                                    >
                                        {{ auditActionDisplay(entry).action }}
                                    </div>
                                    <div
                                        class="small text-muted d-flex align-items-center gap-1 mt-1"
                                    >
                                        <LucideIcon
                                            icon="Clock"
                                            :size="12"
                                        />
                                        {{ formatDateWithTime(entry.created) }}
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
                                {{ $t("auditor.documents.detail.loadMore") }}
                            </button>
                        </div>
                    </div>
                </template>
            </template>
            <template v-else>
                <div
                    v-if="isLoading"
                    class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-5"
                >
                    <LoadingComponent />
                </div>
                <template v-else>
                    <div class="p-3 border-bottom">
                        <div
                            class="d-flex align-items-start justify-content-between flex-wrap gap-2 mb-2"
                        >
                            <div class="d-flex align-items-start gap-3 flex-wrap min-w-0">
                                <div class="min-w-0">
                                    <div class="d-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="FileText"
                                            :size="18"
                                            class="flex-shrink-0"
                                        />
                                        <span class="text-break">{{ documentDisplayName }}</span>
                                    </div>
                                </div>
                                <div class="min-w-0">
                                    <div class="d-flex align-items-center gap-1 small text-muted">
                                        <LucideIcon
                                            icon="Workflow"
                                            :size="18"
                                            class="flex-shrink-0"
                                        />
                                        <span class="text-break">{{ workflowDisplayName }}</span>
                                        <BadgeComponent
                                            :text="
                                                $t('auditor.documents.detail.events', {
                                                    count: documentHistory.length,
                                                })
                                            "
                                            variant="secondary"
                                            size="sm"
                                            :clickable="false"
                                        />
                                    </div>
                                </div>
                            </div>
                            <div
                                class="d-flex align-items-center gap-2 flex-wrap justify-content-end"
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
                                        @click="setStageAndRefresh(stage.value)"
                                    >
                                        {{ stage.label }}
                                    </button>
                                </div>
                                <div class="d-flex align-items-center gap-2 flex-wrap">
                                    <button
                                        type="button"
                                        class="btn btn-sm border py-1 px-2 auditor-filter-sm d-flex align-items-center gap-1"
                                        :class="orderDescending ? 'btn-primary' : 'btn-light'"
                                        @click="toggleOrderAndRefresh"
                                    >
                                        <LucideIcon
                                            icon="ArrowUpDown"
                                            :size="12"
                                        />
                                        {{
                                            orderDescending
                                                ? $t("auditor.documents.detail.orderNewest")
                                                : $t("auditor.documents.detail.orderOldest")
                                        }}
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
                                            {{ selectedActionLabel }}
                                            <LucideIcon
                                                icon="ChevronDown"
                                                :size="12"
                                            />
                                        </button>
                                        <ul class="dropdown-menu dropdown-menu-start">
                                            <li
                                                v-for="opt in actionFilterOptions"
                                                :key="opt.value == null ? 'all' : opt.value"
                                            >
                                                <a
                                                    class="dropdown-item"
                                                    href="#"
                                                    @click.prevent="setActionAndRefresh(opt.value)"
                                                >
                                                    {{ opt.label }}
                                                </a>
                                            </li>
                                        </ul>
                                    </div>
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
                                :placeholder="$t('auditor.documents.detail.searchPlaceholder')"
                                :aria-label="$t('auditor.documents.detail.searchAria')"
                                v-model="historySearchInput"
                                @input="onHistorySearchInput"
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
                                <BadgeComponent
                                    variant="primary"
                                    size="sm"
                                    :clickable="false"
                                    icon-only
                                >
                                    <LucideIcon
                                        icon="User"
                                        :size="12"
                                    />
                                </BadgeComponent>
                                <div
                                    class="d-flex align-items-center flex-wrap gap-1 gap-xl-2 align-self-center min-w-0 flex-grow-1"
                                >
                                    <span class="small fw-semibold">{{ entry.userName }}</span>
                                    <BadgeComponent
                                        v-if="entry.actionName"
                                        :text="auditActionDisplay(entry).title"
                                        variant="primary"
                                        size="sm"
                                        :clickable="false"
                                    />
                                    <span
                                        v-if="entry.stepName"
                                        class="small text-muted"
                                    >
                                        {{ entry.stepName }}
                                    </span>
                                </div>
                                <div class="w-100 audit-history-card-content">
                                    <div
                                        v-if="entry.actionName"
                                        class="small text-muted"
                                    >
                                        {{ auditActionDisplay(entry).action }}
                                    </div>
                                    <div
                                        class="small text-muted d-flex align-items-center gap-1 mt-1"
                                    >
                                        <LucideIcon
                                            icon="Clock"
                                            :size="12"
                                        />
                                        {{ formatDateWithTime(entry.created) }}
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
                                {{ $t("auditor.documents.detail.loadMore") }}
                            </button>
                        </div>
                    </div>
                </template>
            </template>
        </template>
        <template v-else>
            <div class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-4">
                <div class="text-center text-muted py-5">
                    <div class="rounded-circle d-inline-flex p-4 mb-3 workflow-empty-icon-wrap">
                        <LucideIcon
                            icon="History"
                            :size="48"
                        />
                    </div>
                    <p class="mb-0">{{ $t("auditor.documents.detail.selectDocument") }}</p>
                </div>
            </div>
        </template>
    </div>
</template>
<script>
    import BadgeComponent from "@/components/global/BadgeComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import auditActionHelper, { AuditActionTypeOptions } from "@/helpers/auditActionHelper";
    import { buildSelectOptionsWithAll } from "@/utils/selectOptions";
    import AuditorsService from "@/services/auditors/AuditorsService";
    import dateHelper from "@/helpers/date.js";

    export default {
        name: "AuditorDocumentDetail",
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
                documentAuditDetail: null,
                displayedLimit: 10,
                stageFilterOptions: [],
                selectedStageId: "0",
                historySearchInput: "",
                searchDebounceTimer: null,
                selectedActionCode: null,
                orderDescending: true,
            };
        },
        computed: {
            actionFilterOptions() {
                return buildSelectOptionsWithAll(AuditActionTypeOptions, this.$t, {
                    allLabelKey: "auditor.documents.detail.allActions",
                    labelKeyPrefix: "auditor.documents.detail.actionTypes.",
                });
            },
            selectedActionLabel() {
                const opt = this.actionFilterOptions.find(
                    (o) => o.value === this.selectedActionCode
                );
                return opt ? opt.label : this.$t("auditor.documents.detail.allActions");
            },
            hasMultipleWorkflows() {
                return (this.selectedDocumentWorkflows || []).length > 1;
            },
            showReturnInDetail() {
                return this.hasMultipleWorkflows && !!this.selectedWorkflowId;
            },
            documentHistory() {
                const d = this.documentAuditDetail;
                const list = d?.documentHistory ?? d?.DocumentHistory ?? [];
                return Array.isArray(list) ? list : [];
            },
            displayedDetailsEntries() {
                return this.documentHistory.slice(0, this.displayedLimit);
            },
            showHistoryLoadMore() {
                const total = this.documentHistory.length;
                return total >= 10 && total === this.displayedLimit;
            },
            documentDisplayName() {
                const d = this.documentAuditDetail;
                return (
                    d?.documentName ??
                    d?.DocumentName ??
                    this.selectedDocument?.documentName ??
                    this.$t("auditor.documents.detail.document")
                );
            },
            workflowDisplayName() {
                const d = this.documentAuditDetail;
                return d?.workflowName ?? d?.WorkflowName ?? "";
            },
        },
        methods: {
            async refreshWithCurrentDocument() {
                if (this.selectedDocument == null) return;

                this.displayedLimit = 10;
                this.selectedWorkflowId = null;
                this.documentAuditDetail = null;
                this.selectedStageId = "0";
                this.historySearchInput = "";
                this.selectedActionCode = null;
                this.orderDescending = true;

                const workflowsCount = (this.selectedDocumentWorkflows || []).length;
                if (workflowsCount === 0) return;

                if (workflowsCount > 1) {
                    this.mustSelectWorkflow = true;
                    return;
                }
                this.mustSelectWorkflow = false;
                await this.getDocumentAuditDetail();
            },
            async onSelectWorkflow(workflow) {
                this.selectedWorkflowId = workflow?.id ?? workflow;
                this.mustSelectWorkflow = false;
                await this.getDocumentAuditDetail();
            },
            onReturnToWorkflowList() {
                this.mustSelectWorkflow = true;
                this.selectedWorkflowId = null;
                this.documentAuditDetail = null;
            },
            setStageAndRefresh(stageValue) {
                this.selectedStageId = stageValue;
                this.getDocumentAuditDetail();
            },
            setActionAndRefresh(value) {
                this.selectedActionCode = value;
                this.getDocumentAuditDetail();
            },
            toggleOrderAndRefresh() {
                this.orderDescending = !this.orderDescending;
                this.getDocumentAuditDetail();
            },
            onHistorySearchInput() {
                if (this.searchDebounceTimer) clearTimeout(this.searchDebounceTimer);
                this.searchDebounceTimer = setTimeout(() => {
                    this.searchDebounceTimer = null;
                    if (
                        this.selectedDocument != null &&
                        !this.mustSelectWorkflow &&
                        !this.isLoading
                    ) {
                        this.getDocumentAuditDetail();
                    }
                }, 300);
            },
            loadMoreHistory() {
                this.displayedLimit += 10;
                this.getDocumentAuditDetail();
            },
            formatDate(date) {
                return dateHelper.formatDate(date) || "—";
            },
            formatDateWithTime(date) {
                return dateHelper.formatDateWithTime(date) || "—";
            },
            auditActionDisplay(entry) {
                return auditActionHelper.getAuditActionDisplay(entry?.actionName, {
                    t: this.$t,
                    stepName: entry?.stepName || this.$t("auditor.users.detail.nextStep"),
                });
            },
            async getDocumentAuditDetail() {
                const workflowId =
                    this.selectedWorkflowId ??
                    this.selectedDocumentWorkflows?.[0]?.id ??
                    this.selectedDocumentWorkflows?.[0]?.Id;
                if (this.selectedDocument == null || workflowId == null) return;

                this.isLoading = true;
                const documentId =
                    this.selectedDocument.documentId ?? this.selectedDocument.DocumentId;
                const search = (this.historySearchInput || "").trim() || undefined;
                const stepRaw = this.selectedStageId
                    ? parseInt(this.selectedStageId, 10)
                    : undefined;
                const step =
                    stepRaw !== undefined && !Number.isNaN(stepRaw) && stepRaw !== 0
                        ? stepRaw
                        : undefined;
                const params = {
                    take: this.displayedLimit,
                    ...(search && { search }),
                    ...(step !== undefined && { step }),
                    ...(this.selectedActionCode != null && { action: this.selectedActionCode }),
                    orderDescending: this.orderDescending,
                };
                try {
                    const response = await AuditorsService.getDocumentAuditDetails(
                        documentId,
                        workflowId,
                        params
                    );
                    if (response.error) {
                        if (response.error.response?.status === 404) {
                            const doc = this.selectedDocument;
                            const wfId = workflowId;
                            this.documentAuditDetail = {
                                documentId: doc?.documentId ?? doc?.DocumentId,
                                documentName: doc?.documentName ?? doc?.DocumentName ?? "",
                                workflowId: wfId,
                                workflowName: "",
                                documentHistory: [],
                            };
                            return;
                        }
                        return this.$notify({
                            title: "auditor.documents.title",
                            message:
                                response.error.response?.data?.detail ?? response.error.message,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.documentAuditDetail = response ?? null;
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
    .workflow-select-card:not(.workflow-select-card-clickable) {
        background-color: rgba(13, 110, 253, 0.08);
    }
    :global(.css-theme-dark) .workflow-select-card:not(.workflow-select-card-clickable) {
        background-color: rgba(66, 133, 244, 0.18);
    }
    .workflow-select-card-clickable {
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
