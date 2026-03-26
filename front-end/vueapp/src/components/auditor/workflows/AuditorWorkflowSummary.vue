<template>
    <div>
        <div
            v-if="isLoading"
            class="audit-list-wrapper d-flex flex-column flex-grow-1 min-h-0 align-items-center justify-content-center py-5"
        >
            <LoadingComponent />
        </div>
        <template v-else>
            <div class="audit-list-wrapper d-flex flex-column flex-grow-1 min-h-0">
                <div
                    v-if="auditWorkflowList.length === 0"
                    class="audit-list-empty text-muted small text-center py-5"
                >
                    {{ $t("auditor.workflows.summary.empty") }}
                </div>
                <div
                    v-else
                    class="audit-list overflow-auto flex-grow-1 min-h-0"
                >
                    <div
                        v-for="item in auditWorkflowList"
                        :key="item.workflowId"
                        class="audit-list-item rounded-2 p-2 mb-2 cursor-pointer"
                        :class="{
                            'audit-list-item-selected border-start border-primary border-3':
                                selectedWorkflow && selectedWorkflow.workflowId === item.workflowId,
                            border:
                                !selectedWorkflow ||
                                selectedWorkflow.workflowId !== item.workflowId,
                        }"
                        @click="selectWorkflow(item)"
                    >
                        <div class="d-flex align-items-start gap-2">
                            <span
                                class="workflow-card-icon d-inline-flex align-items-center justify-content-center flex-shrink-0 text-primary"
                            >
                                <LucideIcon
                                    icon="Workflow"
                                    :size="18"
                                />
                            </span>
                            <div class="min-w-0 flex-grow-1">
                                <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                    <span class="fw-semibold small text-break">
                                        {{ item.workflowName }}
                                    </span>
                                </div>
                                <div class="small text-muted d-flex align-items-center gap-1 mb-2">
                                    <LucideIcon
                                        icon="UsersRound"
                                        :size="12"
                                    />
                                    {{ item.teamName || "—" }}
                                </div>
                                <div
                                    class="small text-muted d-flex align-items-center flex-wrap gap-2"
                                >
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="FileText"
                                            :size="12"
                                        />
                                        {{ item.documentCount }}
                                        {{ $t("auditor.workflows.summary.docs") }}
                                    </span>
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Zap"
                                            :size="12"
                                        />
                                        {{ item.logsCount }}
                                        {{ $t("auditor.workflows.summary.events") }}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div
                    v-if="hasMore"
                    class="audit-list-footer flex-shrink-0 pt-2"
                >
                    <button
                        type="button"
                        class="btn btn-outline-primary btn-sm w-100"
                        @click="loadMore"
                    >
                        {{ $t("auditor.workflows.summary.loadMore") }}
                    </button>
                </div>
            </div>
        </template>
    </div>
</template>
<script>
    import BadgeComponent from "@/components/global/BadgeComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import AuditorsService from "@/services/auditors/AuditorsService";

    export default {
        name: "AuditorWorkflowSummary",
        components: {
            BadgeComponent,
            LoadingComponent,
        },
        props: {
            filters: {
                type: Object,
                default: () => ({ search: "" }),
            },
        },
        emits: ["select-workflow"],
        data() {
            return {
                isLoading: false,
                selectedWorkflow: null,
                auditWorkflowList: [],
                skip: 0,
                hasMore: false,
                take: 10,
            };
        },
        methods: {
            selectWorkflow(item) {
                this.selectedWorkflow = item;
                this.$emit("select-workflow", item);
            },
            async getWorkflowAuditSummary(append = false) {
                this.isLoading = true;
                try {
                    const search = (this.filters.search || "").trim() || undefined;
                    const response = await AuditorsService.getWorkflowAuditSummary({
                        take: this.take,
                        skip: this.skip,
                        ...(search && { search }),
                    });
                    if (response.error) {
                        return this.$notify({
                            title: "auditor.workflows.title",
                            message:
                                response.error.response?.data?.detail ?? response.error.message,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }

                    this.auditWorkflowList = append
                        ? [...this.auditWorkflowList, ...response.items]
                        : response.items;
                    this.hasMore = response?.hasMore === true;
                } finally {
                    this.isLoading = false;
                }
            },
            loadMore() {
                this.skip += this.take;
                this.getWorkflowAuditSummary(true);
            },
            refreshWithCurrentFilters() {
                this.skip = 0;
                this.getWorkflowAuditSummary(false);
            },
        },
        async created() {
            await this.getWorkflowAuditSummary();
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
        max-height: calc(100vh - 430px);
    }
    .audit-list-wrapper .audit-list {
        min-height: 0;
    }
    .border {
        border: 1px solid var(--color-border-form-control) !important;
    }
</style>
