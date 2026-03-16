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
                    v-if="workflowItems.length === 0"
                    class="audit-list-empty text-muted small text-center py-5"
                >
                    No audit workflows to show.
                </div>
                <div
                    v-else
                    class="audit-list overflow-auto flex-grow-1 min-h-0"
                >
                    <div
                        v-for="item in displayedWorkflowItems"
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
                                        {{ item.workflowName }}
                                    </span>
                                </div>
                                <div class="small text-muted d-flex align-items-center gap-1 mb-2">
                                    <LucideIcon
                                        icon="UsersRound"
                                        :size="12"
                                    />
                                    {{ item.teamName || "—" }}
                                    <BadgeComponent
                                        text="Ativa"
                                        variant="success"
                                        size="sm"
                                        :clickable="false"
                                    />
                                </div>
                                <div
                                    class="small text-muted d-flex align-items-center flex-wrap gap-2"
                                >
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="FileText"
                                            :size="12"
                                        />
                                        {{ item.cardCount }} docs
                                    </span>
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Zap"
                                            :size="12"
                                        />
                                        {{ item.logsCount }} eventos
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
                workflowItems: [],
                displayedLimit: 10,
            };
        },
        computed: {
            filteredWorkflowItems() {
                const q = (this.filters.search || "").toLowerCase().trim();
                if (!q) return this.workflowItems;
                return this.workflowItems.filter(
                    (item) =>
                        (item.workflowName && item.workflowName.toLowerCase().includes(q)) ||
                        (item.teamName && item.teamName.toLowerCase().includes(q))
                );
            },
            displayedWorkflowItems() {
                return this.filteredWorkflowItems;
            },
            showLoadMoreButton() {
                return (
                    this.workflowItems.length > 0 &&
                    this.workflowItems.length >= this.displayedLimit
                );
            },
        },
        methods: {
            selectWorkflow(item) {
                this.selectedWorkflow = item;
                this.$emit("select-workflow", item);
            },
            async getWorkflowAuditSummary() {
                this.isLoading = true;
                try {
                    const search = (this.filters.search || "").trim() || undefined;
                    const response = await AuditorsService.getWorkflowAuditSummary({
                        take: this.displayedLimit,
                        ...(search && { search }),
                    });
                    if (response.error) {
                        return this.$notify({
                            title: "audit-workflows.title",
                            message:
                                response.error.response?.data?.detail ?? response.error.message,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.workflowItems = Array.isArray(response)
                        ? response
                        : Array.isArray(response?.data)
                          ? response.data
                          : [];
                } finally {
                    this.isLoading = false;
                }
            },
            loadMore() {
                this.displayedLimit += 10;
                this.getWorkflowAuditSummary();
            },
            refreshWithCurrentFilters() {
                this.displayedLimit = 10;
                this.getWorkflowAuditSummary();
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
</style>
