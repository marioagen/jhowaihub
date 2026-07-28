<template>
    <div class="system-audit-container d-flex flex-column">
        <!-- KPI strip -->
        <div class="row g-2 mb-3">
            <div class="col-6 col-md-3" v-for="kpi in kpiCards" :key="kpi.label">
                <div class="system-kpi-card rounded-3 p-3 border d-flex align-items-center gap-3">
                    <span class="system-kpi-icon d-inline-flex align-items-center justify-content-center flex-shrink-0" :class="kpi.iconBg">
                        <LucideIcon :icon="kpi.icon" :size="20" />
                    </span>
                    <div>
                        <div class="fw-bold fs-5 lh-1">{{ kpi.value }}</div>
                        <div class="small text-muted mt-1">{{ kpi.label }}</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Filters row -->
        <div class="d-flex align-items-center flex-wrap gap-2 mb-3">
            <div class="input-group input-group-sm system-search flex-grow-1">
                <span class="input-group-text border-end-0 py-1">
                    <LucideIcon icon="Search" :size="14" />
                </span>
                <input
                    v-model="search"
                    type="text"
                    class="form-control form-control-sm border-start-0 py-1"
                    :placeholder="$t('auditor.system.filters.searchPlaceholder')"
                    @input="applyFilters"
                />
            </div>
            <select v-model="eventTypeFilter" class="form-select form-select-sm system-type-filter" @change="applyFilters">
                <option value="">{{ $t("auditor.system.filters.allEventTypes") }}</option>
                <option value="access">{{ $t("auditor.system.eventTypes.access") }}</option>
                <option value="apiCall">{{ $t("auditor.system.eventTypes.apiCall") }}</option>
                <option value="userCreated">{{ $t("auditor.system.eventTypes.userCreated") }}</option>
                <option value="profileChanged">{{ $t("auditor.system.eventTypes.profileChanged") }}</option>
                <option value="userManagement">{{ $t("auditor.system.eventTypes.userManagement") }}</option>
                <option value="workflowChanged">{{ $t("auditor.system.eventTypes.workflowChanged") }}</option>
            </select>
            <button type="button" class="btn btn-light btn-sm border py-1 px-2 d-flex align-items-center gap-1" style="font-size:0.72rem" @click="toggleOrder">
                <LucideIcon icon="ArrowUpDown" :size="11" />
                {{ orderDescending ? $t("auditor.tools.detail.orderNewest") : $t("auditor.tools.detail.orderOldest") }}
            </button>
        </div>

        <!-- Timeline -->
        <div v-if="isLoading" class="d-flex justify-content-center py-5">
            <LoadingComponent />
        </div>
        <div v-else-if="displayedEvents.length === 0" class="text-muted small text-center py-5">
            {{ $t("auditor.system.summary.empty") }}
        </div>
        <div v-else class="system-timeline overflow-auto flex-grow-1">
            <div
                v-for="(entry, idx) in displayedEvents"
                :key="idx"
                class="system-event-row rounded-3 mb-2 border d-flex align-items-start gap-3 p-3"
            >
                <!-- Left: type indicator -->
                <span class="system-event-type-icon d-inline-flex align-items-center justify-content-center flex-shrink-0" :class="eventIconBg(entry.eventType)">
                    <LucideIcon :icon="eventIcon(entry.eventType)" :size="16" />
                </span>

                <!-- Center: main info -->
                <div class="flex-grow-1 min-w-0">
                    <div class="d-flex align-items-center flex-wrap gap-2 mb-1">
                        <span class="badge rounded-pill system-event-badge" :class="eventBadgeClass(entry.eventType)" style="font-size:0.62rem">
                            {{ $t(`auditor.system.eventTypes.${entry.eventType}`) }}
                        </span>
                        <span class="small fw-semibold text-truncate">{{ entry.userName }}</span>
                    </div>
                    <div class="small text-body mb-1 system-event-detail">{{ entry.detail }}</div>
                    <div v-if="entry.endpoint" class="system-endpoint-tag d-inline-flex align-items-center gap-1 rounded-1 px-2 py-0" style="font-size:0.65rem">
                        <span class="system-method-badge" :class="methodClass(entry.method)">{{ entry.method }}</span>
                        <code class="text-muted">{{ entry.endpoint }}</code>
                    </div>
                </div>

                <!-- Right: meta -->
                <div class="text-end flex-shrink-0 d-flex flex-column align-items-end gap-1">
                    <span class="small text-muted d-flex align-items-center gap-1">
                        <LucideIcon icon="Clock" :size="11" />
                        {{ formatDate(entry.createdAt) }}
                    </span>
                    <span v-if="entry.statusCode" class="badge rounded-pill" :class="statusBadgeClass(entry.statusCode)" style="font-size:0.6rem">
                        {{ entry.statusCode }}
                    </span>
                    <span v-if="entry.durationMs != null" class="small text-muted" style="font-size:0.65rem">
                        {{ entry.durationMs }}ms
                    </span>
                </div>
            </div>

            <div v-if="hasMore" class="text-center pt-2 pb-3">
                <button type="button" class="btn btn-outline-primary btn-sm" @click="loadMore">
                    {{ $t("auditor.system.summary.loadMore") }}
                </button>
            </div>
        </div>
    </div>
</template>
<script>
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import AuditorsService from "@/services/auditors/AuditorsService";
    import dateHelper from "@/helpers/date.js";

    const EVENT_ICONS = {
        access: "LogIn",
        apiCall: "Zap",
        userCreated: "UserPlus",
        profileChanged: "UserCog",
        userManagement: "Users",
        workflowChanged: "Workflow",
    };
    const EVENT_BG = {
        access: "ev-bg-access",
        apiCall: "ev-bg-api",
        userCreated: "ev-bg-user",
        profileChanged: "ev-bg-profile",
        userManagement: "ev-bg-mgmt",
        workflowChanged: "ev-bg-workflow",
    };
    const EVENT_BADGE = {
        access: "ev-badge-access",
        apiCall: "ev-badge-api",
        userCreated: "ev-badge-user",
        profileChanged: "ev-badge-profile",
        userManagement: "ev-badge-mgmt",
        workflowChanged: "ev-badge-workflow",
    };

    export default {
        name: "AuditorSystemSection",
        components: { LoadingComponent },
        data() {
            return {
                isLoading: false,
                allEvents: [],
                search: "",
                eventTypeFilter: "",
                orderDescending: true,
                take: 20,
                skip: 0,
                hasMore: false,
            };
        },
        computed: {
            displayedEvents() {
                let list = [...this.allEvents];
                if (this.search) {
                    const q = this.search.toLowerCase();
                    list = list.filter(
                        (e) =>
                            e.userName?.toLowerCase().includes(q) ||
                            e.detail?.toLowerCase().includes(q) ||
                            e.endpoint?.toLowerCase().includes(q)
                    );
                }
                if (this.eventTypeFilter) {
                    list = list.filter((e) => e.eventType === this.eventTypeFilter);
                }
                return this.orderDescending ? list : [...list].reverse();
            },
            kpiCards() {
                const t = this.$t;
                const accesses = this.allEvents.filter((e) => e.eventType === "access").length;
                const apiCalls = this.allEvents.filter((e) => e.eventType === "apiCall").length;
                const userEvents = this.allEvents.filter((e) =>
                    ["userCreated", "profileChanged", "userManagement"].includes(e.eventType)
                ).length;
                const workflowEvents = this.allEvents.filter(
                    (e) => e.eventType === "workflowChanged"
                ).length;
                return [
                    { icon: "LogIn", iconBg: "ev-bg-access", label: t("auditor.system.summary.totalAccesses"), value: accesses },
                    { icon: "Zap", iconBg: "ev-bg-api", label: t("auditor.system.summary.totalApiCalls"), value: apiCalls },
                    { icon: "Users", iconBg: "ev-bg-user", label: t("auditor.system.eventTypes.userManagement"), value: userEvents },
                    { icon: "Workflow", iconBg: "ev-bg-workflow", label: t("auditor.system.eventTypes.workflowChanged"), value: workflowEvents },
                ];
            },
        },
        methods: {
            eventIcon(type) { return EVENT_ICONS[type] ?? "Activity"; },
            eventIconBg(type) { return EVENT_BG[type] ?? ""; },
            eventBadgeClass(type) { return EVENT_BADGE[type] ?? ""; },
            formatDate(d) { return dateHelper.formatDateWithTime(d) || "—"; },
            toggleOrder() { this.orderDescending = !this.orderDescending; },
            applyFilters() { /* client-side, displayedEvents handles it */ },
            methodClass(method) {
                const m = (method || "").toUpperCase();
                if (m === "GET") return "method-get";
                if (m === "POST") return "method-post";
                if (m === "PUT") return "method-put";
                if (m === "DELETE") return "method-delete";
                return "method-other";
            },
            statusBadgeClass(code) {
                if (code >= 200 && code < 300) return "bg-success-subtle text-success";
                if (code >= 400 && code < 500) return "bg-warning-subtle text-warning";
                return "bg-danger-subtle text-danger";
            },
            async loadEvents(append = false) {
                this.isLoading = true;
                try {
                    const params = { take: this.take, skip: this.skip };
                    const response = await AuditorsService.getSystemAuditEvents(params);
                    const items = Array.isArray(response) ? response : (response?.items ?? []);
                    this.allEvents = append ? [...this.allEvents, ...items] : items;
                    this.hasMore = response?.hasMore === true;
                } finally {
                    this.isLoading = false;
                }
            },
            loadMore() {
                this.skip += this.take;
                this.loadEvents(true);
            },
        },
        async created() {
            await this.loadEvents();
        },
    };
</script>
<style scoped>
    .system-audit-container { min-height: 0; height: calc(100vh - 230px); }
    .system-timeline { min-height: 0; }
    .system-search { max-width: 320px; min-width: 180px; }
    .system-type-filter { max-width: 200px; }

    .system-kpi-card { background-color: var(--bs-secondary-bg, transparent); }
    .system-kpi-icon { width: 44px; height: 44px; border-radius: 10px; }

    .system-event-row { background-color: transparent; transition: background-color 0.12s; }
    .system-event-row:hover { background-color: var(--bs-tertiary-bg, rgba(0,0,0,0.03)); }

    .system-event-type-icon { width: 36px; height: 36px; border-radius: 8px; }
    .system-event-detail { word-break: break-word; }

    .system-endpoint-tag {
        background-color: var(--bs-tertiary-bg, rgba(0,0,0,0.04));
        border: 1px solid var(--color-border-form-control, #dee2e6);
    }
    .system-method-badge {
        font-size: 0.6rem; font-weight: 700; padding: 0.1rem 0.3rem;
        border-radius: 3px; text-transform: uppercase; letter-spacing: 0.04em;
    }
    .method-get    { background-color: rgba(16,185,129,0.15); color: #059669; }
    .method-post   { background-color: rgba(59,130,246,0.15); color: #2563eb; }
    .method-put    { background-color: rgba(245,158,11,0.15); color: #d97706; }
    .method-delete { background-color: rgba(239,68,68,0.15); color: #dc2626; }
    .method-other  { background-color: rgba(100,116,139,0.15); color: #64748b; }

    /* Event type colours */
    .ev-bg-access   { background-color: rgba(59,130,246,0.12); color: #2563eb; }
    .ev-bg-api      { background-color: rgba(99,102,241,0.12); color: #6366f1; }
    .ev-bg-user     { background-color: rgba(16,185,129,0.12); color: #059669; }
    .ev-bg-profile  { background-color: rgba(245,158,11,0.12); color: #d97706; }
    .ev-bg-mgmt     { background-color: rgba(236,72,153,0.12); color: #db2777; }
    .ev-bg-workflow { background-color: rgba(139,92,246,0.12); color: #7c3aed; }

    .ev-badge-access   { background-color: rgba(59,130,246,0.12); color: #2563eb; }
    .ev-badge-api      { background-color: rgba(99,102,241,0.12); color: #6366f1; }
    .ev-badge-user     { background-color: rgba(16,185,129,0.12); color: #059669; }
    .ev-badge-profile  { background-color: rgba(245,158,11,0.12); color: #d97706; }
    .ev-badge-mgmt     { background-color: rgba(236,72,153,0.12); color: #db2777; }
    .ev-badge-workflow { background-color: rgba(139,92,246,0.12); color: #7c3aed; }

    .border { border: 1px solid var(--color-border-form-control) !important; }
</style>
