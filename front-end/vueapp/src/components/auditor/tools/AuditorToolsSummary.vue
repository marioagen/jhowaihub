<template>
    <div class="d-flex flex-column flex-grow-1 min-h-0 overflow-hidden">
        <div
            v-if="isLoading"
            class="d-flex flex-column flex-grow-1 min-h-0 align-items-center justify-content-center py-5"
        >
            <LoadingComponent />
        </div>
        <template v-else>
            <div class="d-flex flex-column flex-grow-1 min-h-0">
                <div
                    v-if="items.length === 0"
                    class="text-muted small text-center py-5"
                >
                    {{ $t("auditor.tools.summary.empty") }}
                </div>
                <div
                    v-else
                    class="audit-list overflow-auto flex-grow-1 min-h-0"
                >
                    <div
                        v-for="item in items"
                        :key="item.toolId"
                        class="audit-list-item rounded-2 p-2 mb-2 cursor-pointer"
                        :class="{
                            'audit-list-item-selected border-start border-primary border-3':
                                selectedTool && selectedTool.toolId === item.toolId,
                            border: !selectedTool || selectedTool.toolId !== item.toolId,
                        }"
                        @click="selectItem(item)"
                    >
                        <div class="d-flex align-items-start gap-2">
                            <span class="tool-icon d-inline-flex align-items-center justify-content-center flex-shrink-0">
                                <LucideIcon :icon="categoryIcon(item.category)" :size="16" />
                            </span>
                            <div class="min-w-0 flex-grow-1">
                                <div class="fw-semibold small text-break mb-1">{{ item.toolName }}</div>
                                <div class="d-flex align-items-center gap-2 flex-wrap">
                                    <span
                                        class="badge rounded-pill category-badge"
                                        :class="categoryBadgeClass(item.category)"
                                    >
                                        {{ $t(`auditor.tools.categories.${item.category}`) }}
                                    </span>
                                    <span class="small text-muted d-inline-flex align-items-center gap-1">
                                        <LucideIcon icon="Zap" :size="11" />
                                        {{ $t("auditor.tools.summary.eventsCount", { count: item.eventCount }) }}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div v-if="hasMore" class="pt-2 flex-shrink-0">
                    <button
                        type="button"
                        class="btn btn-outline-primary btn-sm w-100"
                        @click="loadMore"
                    >
                        {{ $t("auditor.tools.summary.loadMore") }}
                    </button>
                </div>
            </div>
        </template>
    </div>
</template>
<script>
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import AuditorsService from "@/services/auditors/AuditorsService";

    const CATEGORY_ICONS = {
        agent: "Bot",
        connector: "Plug",
        apiTemplate: "FileCode",
        questionnaire: "ClipboardList",
    };

    const CATEGORY_BADGE = {
        agent: "badge-agent",
        connector: "badge-connector",
        apiTemplate: "badge-api",
        questionnaire: "badge-quiz",
    };

    export default {
        name: "AuditorToolsSummary",
        components: { LoadingComponent },
        props: {
            filters: { type: Object, default: () => ({ search: "", category: "" }) },
        },
        emits: ["select-tool"],
        data() {
            return {
                isLoading: false,
                selectedTool: null,
                items: [],
                take: 10,
                skip: 0,
                hasMore: false,
            };
        },
        methods: {
            categoryIcon(cat) { return CATEGORY_ICONS[cat] ?? "Wrench"; },
            categoryBadgeClass(cat) { return CATEGORY_BADGE[cat] ?? ""; },
            selectItem(item) {
                this.selectedTool = item;
                this.$emit("select-tool", item);
            },
            async loadItems(append = false) {
                this.isLoading = true;
                try {
                    const params = {
                        take: this.take,
                        skip: this.skip,
                        ...(this.filters.search && { search: this.filters.search }),
                        ...(this.filters.category && { category: this.filters.category }),
                    };
                    const response = await AuditorsService.getToolsAuditSummary(params);
                    if (response?.error) return;
                    this.items = append
                        ? [...this.items, ...(response.items ?? response)]
                        : (response.items ?? response);
                    this.hasMore = response?.hasMore === true;
                } finally {
                    this.isLoading = false;
                }
            },
            loadMore() {
                this.skip += this.take;
                this.loadItems(true);
            },
            refreshWithCurrentFilters() {
                this.skip = 0;
                this.loadItems(false);
            },
        },
        async created() {
            await this.loadItems();
        },
    };
</script>
<style scoped>
    .audit-list-item:hover { background-color: var(--bs-tertiary-bg, rgba(0,0,0,0.04)); }
    .audit-list-item-selected { background-color: var(--bs-primary-bg-subtle, transparent); }
    .cursor-pointer { cursor: pointer; }
    .tool-icon {
        width: 36px; height: 36px; border-radius: 8px;
        background-color: var(--bs-primary-bg-subtle, rgba(13,110,253,0.12));
        color: var(--bs-primary);
    }
    .border { border: 1px solid var(--color-border-form-control) !important; }

    .badge-agent    { background-color: rgba(99,102,241,0.12); color: #6366f1; }
    .badge-connector{ background-color: rgba(16,185,129,0.12); color: #059669; }
    .badge-api      { background-color: rgba(245,158,11,0.12); color: #d97706; }
    .badge-quiz     { background-color: rgba(236,72,153,0.12); color: #db2777; }
</style>
