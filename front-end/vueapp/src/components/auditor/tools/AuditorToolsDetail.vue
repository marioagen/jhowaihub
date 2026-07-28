<template>
    <div class="card-body d-flex flex-column p-0">
        <template v-if="!selectedTool">
            <div class="d-flex flex-column align-items-center justify-content-center min-vh-50 py-5">
                <div class="text-secondary mb-3" style="opacity:0.5">
                    <LucideIcon icon="Wrench" :size="56" stroke-width="1.25" />
                </div>
                <p class="text-muted text-center mb-0 small">{{ $t("auditor.tools.detail.selectTool") }}</p>
            </div>
        </template>
        <template v-else>
            <div v-if="isLoading" class="d-flex align-items-center justify-content-center flex-grow-1 p-5">
                <LoadingComponent />
            </div>
            <template v-else>
                <div class="tool-detail-content p-3 d-flex flex-column flex-grow-1 min-h-0">
                    <!-- Header card -->
                    <div class="tool-detail-header-card rounded-2 p-3 mb-3 border d-flex align-items-center gap-3">
                        <span class="tool-detail-icon d-inline-flex align-items-center justify-content-center flex-shrink-0">
                            <LucideIcon :icon="categoryIcon" :size="22" />
                        </span>
                        <div class="flex-grow-1 min-w-0">
                            <div class="fw-bold">{{ selectedTool.toolName }}</div>
                            <div class="small text-muted">
                                <span class="badge rounded-pill me-1" :class="categoryBadgeClass">
                                    {{ $t(`auditor.tools.categories.${selectedTool.category}`) }}
                                </span>
                            </div>
                        </div>
                        <div class="text-end flex-shrink-0">
                            <div class="fw-bold fs-5">{{ events.length }}</div>
                            <div class="small text-muted">{{ $t("auditor.tools.detail.events", { count: events.length }) }}</div>
                        </div>
                    </div>

                    <!-- Stats row -->
                    <div class="row g-2 mb-3">
                        <div class="col-4" v-for="stat in statCards" :key="stat.label">
                            <div class="rounded-2 p-2 border d-flex flex-column align-items-center text-center tool-stat-card">
                                <LucideIcon :icon="stat.icon" :size="18" class="mb-1" :class="stat.iconClass" />
                                <span class="fw-bold">{{ stat.value }}</span>
                                <span class="small text-muted">{{ stat.label }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- Timeline -->
                    <div class="d-flex align-items-center justify-content-between mb-2 flex-wrap gap-2">
                        <h6 class="mb-0 small fw-normal d-flex align-items-center gap-1">
                            <LucideIcon icon="History" :size="14" class="text-primary" />
                            {{ $t("auditor.tools.detail.activityHistory") }}
                        </h6>
                        <div class="d-flex gap-2">
                            <button
                                type="button"
                                class="btn btn-light btn-sm border py-1 px-2 d-flex align-items-center gap-1"
                                style="font-size:0.72rem"
                                @click="toggleOrder"
                            >
                                <LucideIcon icon="ArrowUpDown" :size="11" />
                                {{ orderDescending ? $t("auditor.tools.detail.orderNewest") : $t("auditor.tools.detail.orderOldest") }}
                            </button>
                        </div>
                    </div>

                    <div class="tool-activity-list overflow-auto flex-grow-1 min-h-0">
                        <div
                            v-for="(entry, idx) in displayedEvents"
                            :key="idx"
                            class="tool-event-card rounded-2 p-2 mb-2 border d-flex align-items-start gap-2"
                        >
                            <span class="tool-event-icon d-inline-flex align-items-center justify-content-center flex-shrink-0" :class="actionIconBg(entry.action)">
                                <LucideIcon :icon="actionIcon(entry.action)" :size="14" />
                            </span>
                            <div class="flex-grow-1 min-w-0">
                                <div class="d-flex align-items-center gap-1 flex-wrap mb-1">
                                    <span class="small fw-semibold">{{ entry.userName }}</span>
                                    <span class="badge rounded-pill" :class="actionBadgeClass(entry.action)" style="font-size:0.62rem">
                                        {{ $t(`auditor.tools.actions.${entry.action}`) }}
                                    </span>
                                </div>
                                <div class="small text-muted mb-1">{{ entry.detail }}</div>
                                <div class="small text-muted d-flex align-items-center gap-1">
                                    <LucideIcon icon="Clock" :size="11" />
                                    {{ formatDate(entry.createdAt) }}
                                </div>
                            </div>
                        </div>
                        <div v-if="events.length === 0" class="text-muted small text-center py-4">
                            {{ $t("auditor.tools.summary.empty") }}
                        </div>
                    </div>
                </div>
            </template>
        </template>
    </div>
</template>
<script>
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import AuditorsService from "@/services/auditors/AuditorsService";
    import dateHelper from "@/helpers/date.js";

    const CAT_ICONS = { agent: "Bot", connector: "Plug", apiTemplate: "FileCode", questionnaire: "ClipboardList" };
    const CAT_BADGE = { agent: "badge-agent", connector: "badge-connector", apiTemplate: "badge-api", questionnaire: "badge-quiz" };
    const ACTION_ICONS = { created: "Plus", updated: "Pencil", deleted: "Trash2" };
    const ACTION_BG = { created: "icon-bg-created", updated: "icon-bg-updated", deleted: "icon-bg-deleted" };
    const ACTION_BADGE = { created: "badge-created", updated: "badge-updated", deleted: "badge-deleted" };

    export default {
        name: "AuditorToolsDetail",
        components: { LoadingComponent },
        props: {
            selectedTool: { type: Object, default: null },
        },
        data() {
            return {
                isLoading: false,
                events: [],
                orderDescending: true,
            };
        },
        computed: {
            categoryIcon() { return CAT_ICONS[this.selectedTool?.category] ?? "Wrench"; },
            categoryBadgeClass() { return CAT_BADGE[this.selectedTool?.category] ?? ""; },
            displayedEvents() {
                const list = [...this.events];
                return this.orderDescending ? list : list.reverse();
            },
            statCards() {
                const t = this.$t;
                const counts = this.events.reduce((acc, e) => { acc[e.action] = (acc[e.action] || 0) + 1; return acc; }, {});
                return [
                    { icon: "Plus", iconClass: "text-success", label: t("auditor.tools.actions.created"), value: counts.created ?? 0 },
                    { icon: "Pencil", iconClass: "text-warning", label: t("auditor.tools.actions.updated"), value: counts.updated ?? 0 },
                    { icon: "Trash2", iconClass: "text-danger", label: t("auditor.tools.actions.deleted"), value: counts.deleted ?? 0 },
                ];
            },
        },
        watch: {
            selectedTool(val) { if (val) this.refresh(); },
        },
        methods: {
            actionIcon(action) { return ACTION_ICONS[action] ?? "Activity"; },
            actionIconBg(action) { return ACTION_BG[action] ?? ""; },
            actionBadgeClass(action) { return ACTION_BADGE[action] ?? ""; },
            formatDate(d) { return dateHelper.formatDateWithTime(d) || "—"; },
            toggleOrder() { this.orderDescending = !this.orderDescending; },
            async refresh() {
                if (!this.selectedTool?.toolId) return;
                this.isLoading = true;
                try {
                    const response = await AuditorsService.getToolsAuditDetail(this.selectedTool.toolId);
                    this.events = Array.isArray(response) ? response : (response?.events ?? []);
                } finally {
                    this.isLoading = false;
                }
            },
        },
    };
</script>
<style scoped>
    .min-vh-50 { min-height: 50vh; }
    .tool-detail-content { flex: 1 1 0; min-height: 0; overflow: hidden; }
    .tool-activity-list { flex: 1 1 0; min-height: 0; }
    .tool-detail-header-card { background-color: var(--bs-secondary-bg, transparent); }
    .tool-detail-icon {
        width: 48px; height: 48px; border-radius: 10px;
        background-color: var(--bs-primary-bg-subtle, rgba(13,110,253,0.12));
        color: var(--bs-primary);
    }
    .tool-stat-card { background-color: var(--bs-secondary-bg, transparent); }
    .tool-event-icon {
        width: 28px; height: 28px; border-radius: 6px; flex-shrink: 0;
    }
    .icon-bg-created { background-color: rgba(16,185,129,0.12); color: #059669; }
    .icon-bg-updated { background-color: rgba(245,158,11,0.12); color: #d97706; }
    .icon-bg-deleted { background-color: rgba(239,68,68,0.12); color: #dc2626; }
    .badge-agent    { background-color: rgba(99,102,241,0.12); color: #6366f1; }
    .badge-connector{ background-color: rgba(16,185,129,0.12); color: #059669; }
    .badge-api      { background-color: rgba(245,158,11,0.12); color: #d97706; }
    .badge-quiz     { background-color: rgba(236,72,153,0.12); color: #db2777; }
    .badge-created  { background-color: rgba(16,185,129,0.12); color: #059669; }
    .badge-updated  { background-color: rgba(245,158,11,0.12); color: #d97706; }
    .badge-deleted  { background-color: rgba(239,68,68,0.12); color: #dc2626; }
    .border { border: 1px solid var(--color-border-form-control) !important; }
</style>
