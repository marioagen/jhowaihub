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
                    v-if="filteredAuditItems.length === 0"
                    class="audit-list-empty text-muted small text-center py-5"
                >
                    No audit cards to show.
                </div>
                <div
                    v-else
                    class="audit-list overflow-auto flex-grow-1 min-h-0"
                >
                    <div
                        v-for="item in displayedAuditItems"
                        :key="item.cardId"
                        class="audit-list-item rounded-2 p-2 mb-2 cursor-pointer"
                        :class="{
                            'audit-list-item-selected border-start border-primary border-3':
                                selectedDocument && selectedDocument.cardId === item.cardId,
                            border: !selectedDocument || selectedDocument.cardId !== item.cardId,
                        }"
                        @click="$emit('select-document', item)"
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
                                        {{ item.cardName }}
                                    </span>
                                    <BadgeComponent
                                        :text="item.statusName"
                                        variant="secondary"
                                        size="sm"
                                        :clickable="false"
                                    />
                                    <BadgeComponent
                                        v-if="workflowsCount(item) > 1"
                                        :text="workflowsCount(item)"
                                        variant="warning"
                                        size="sm"
                                        :clickable="false"
                                    />
                                </div>
                                <div
                                    class="small text-primary d-flex align-items-center gap-1 mb-0"
                                >
                                    <LucideIcon
                                        icon="Workflow"
                                        :size="12"
                                    />
                                    {{ workflowsLabel(item) }}
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
    import AuditorsService from "@/services/auditors/AuditorsService";

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
                isLoading: false,
                auditCardList: [],
                displayedLimit: 10,
            };
        },
        computed: {
            filteredAuditItems() {
                const list = this.auditCardList ?? [];
                const search = (this.filterParams?.search || "").toLowerCase().trim();
                const statusId = this.filterParams?.statusId || "";
                return list.filter((item) => {
                    const cardIdStr = item.cardId != null ? String(item.cardId) : "";
                    const cardName = (item.cardName || "").toLowerCase();
                    const workflowNames = (item.workflows || [])
                        .map((w) => (w.name || "").toLowerCase())
                        .join(" ");
                    const matchesSearch =
                        !search ||
                        cardIdStr.toLowerCase().includes(search) ||
                        cardName.includes(search) ||
                        workflowNames.includes(search);
                    const statusName = (item.statusName || "").toLowerCase();
                    const matchesStatus = !statusId || statusName === statusId.toLowerCase();
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
            workflowsCount(item) {
                return item.workflows.length;
            },
            workflowsLabel(item) {
                const w = item.workflows;
                if (!Array.isArray(w) || w.length === 0) return "—";
                return w
                    .map((x) => x.name)
                    .filter(Boolean)
                    .join(", ");
            },
            async getAuditCardsSummary() {
                this.isLoading = true;
                try {
                    const params = this.filterParams || {};
                    const response = await AuditorsService.getCardsAuditSummary(params);
                    console.log(response);
                    if (response.error) {
                        return this.$notify({
                            title: "audit-cards.title",
                            message: response.error.response.data.detail,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.auditCardList = Array.isArray(response)
                        ? response
                        : Array.isArray(response?.data)
                          ? response.data
                          : [];
                } finally {
                    this.isLoading = false;
                }
            },
            loadMore() {
                this.displayedLimit = Math.min(
                    this.displayedLimit + 10,
                    this.filteredAuditItems.length
                );
            },
        },
        async created() {
            await this.getAuditCardsSummary();
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
