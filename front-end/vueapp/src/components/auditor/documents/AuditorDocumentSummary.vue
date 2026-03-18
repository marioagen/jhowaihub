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
                    v-if="auditDocumentList.length === 0"
                    class="audit-list-empty text-muted small text-center py-5"
                >
                    No documents to show.
                </div>
                <div
                    v-else
                    class="audit-list overflow-auto flex-grow-1 min-h-0"
                >
                    <div
                        v-for="item in auditDocumentList"
                        :key="item.documentId"
                        class="audit-list-item rounded-2 p-2 mb-2 cursor-pointer"
                        :class="{
                            'audit-list-item-selected border-start border-primary border-3':
                                selectedDocument && selectedDocument.documentId === item.documentId,
                            border:
                                !selectedDocument ||
                                selectedDocument.documentId !== item.documentId,
                        }"
                        @click="selectDocument(item)"
                    >
                        <div class="d-flex align-items-start gap-2">
                            <LucideIcon
                                icon="FileText"
                                :size="16"
                                class="text-muted mt-1 flex-shrink-0"
                            />
                            <div class="min-w-0 flex-grow-1">
                                <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                    <span class="fw-semibold small text-break">
                                        {{ item.documentName }}
                                    </span>
                                    <BadgeComponent
                                        :text="item.isFinalized ? 'Finalized' : 'Active'"
                                        :variant="item.isFinalized ? 'success' : 'primary'"
                                        size="sm"
                                        :clickable="false"
                                    />
                                    <BadgeComponent
                                        v-if="workflowsCount(item) > 1"
                                        variant="warning"
                                        size="sm"
                                        :clickable="false"
                                    >
                                        <span
                                            class="d-inline-flex align-items-center gap-1 audit-badge-workflows-content"
                                        >
                                            <LucideIcon
                                                icon="Layers"
                                                :size="12"
                                                class="flex-shrink-0"
                                            />
                                            {{ workflowsCount(item) }}
                                        </span>
                                    </BadgeComponent>
                                </div>
                                <div class="small text-primary mb-0">
                                    <template v-if="topWorkflows(item).length > 0">
                                        <div
                                            v-for="wf in topWorkflows(item)"
                                            :key="wf.id ?? wf.name"
                                            class="d-flex align-items-center gap-1"
                                        >
                                            <LucideIcon
                                                icon="Workflow"
                                                :size="12"
                                                class="flex-shrink-0"
                                            />
                                            <span class="text-break">{{ wf.name || "—" }}</span>
                                        </div>
                                    </template>
                                    <div
                                        v-else
                                        class="d-flex align-items-center gap-1"
                                    >
                                        <LucideIcon
                                            icon="Workflow"
                                            :size="12"
                                            class="flex-shrink-0"
                                        />
                                        <span>—</span>
                                    </div>
                                </div>
                                <div class="small text-muted">
                                    {{ item.actionsCount }} action(s)
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
    import LucideIcon from "@/components/global/LucideIcon.vue";
    import AuditorsService from "@/services/auditors/AuditorsService";

    export default {
        name: "AuditorDocumentSummary",
        components: {
            BadgeComponent,
            LoadingComponent,
            LucideIcon,
        },
        props: {
            filters: {
                type: Object,
                default: () => ({ search: "", statusId: "" }),
            },
        },
        emits: ["select-document"],
        data() {
            return {
                isLoading: false,
                selectedDocument: null,
                auditDocumentList: [],
                displayedLimit: 10,
            };
        },
        methods: {
            workflowsCount(item) {
                return item.workflows.length;
            },
            topWorkflows(item) {
                const workflows = item.workflows;
                if (!Array.isArray(workflows)) return [];
                return workflows.slice(0, 3);
            },
            selectDocument(item) {
                this.selectedDocument = item;
                this.$emit("select-document", item);
            },
            async getAuditDocumentsSummary() {
                this.isLoading = true;
                try {
                    const search = (this.filters.search || "").trim() || undefined;
                    const isFinalized =
                        this.filters.statusId === "finalizado"
                            ? true
                            : this.filters.statusId === "ativo"
                              ? false
                              : undefined;
                    const params = {
                        take: this.displayedLimit,
                        ...(search && { search }),
                        ...(isFinalized !== undefined && { isFinalized }),
                    };
                    const response = await AuditorsService.getDocumentsAuditSummary(params);
                    if (response.error) {
                        return this.$notify({
                            title: "audit-cards.title",
                            message: response.error.response.data.detail,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.auditDocumentList = Array.isArray(response)
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
                this.getAuditDocumentsSummary();
            },
            refreshWithCurrentFilters() {
                this.displayedLimit = 10;
                this.getAuditDocumentsSummary();
            },
        },
        computed: {
            showLoadMoreButton() {
                return this.auditDocumentList.length === this.displayedLimit;
            },
        },
        async created() {
            await this.getAuditDocumentsSummary();
        },
    };
</script>
<style scoped>
    .audit-badge-workflows-content {
        color: #8b6914;
    }
    .audit-badge-workflows-content :deep(svg) {
        color: inherit;
    }
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
