<template>
    <div class="system-audit-container d-flex flex-column">
        <!-- KPI strip -->
        <div class="row g-2 mb-3">
            <div class="col-6 col-md-2" v-for="kpi in kpiCards" :key="kpi.label">
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
            <select v-model="domainFilter" class="form-select form-select-sm system-type-filter" @change="applyFilters">
                <option value="">{{ $t("auditor.system.filters.allDomains") }}</option>
                <option value="access">{{ $t("auditor.system.filters.domains.access") }}</option>
                <option value="users">{{ $t("auditor.system.filters.domains.users") }}</option>
                <option value="teams">{{ $t("auditor.system.filters.domains.teams") }}</option>
                <option value="permissions">{{ $t("auditor.system.filters.domains.permissions") }}</option>
                <option value="keys">{{ $t("auditor.system.filters.domains.keys") }}</option>
                <option value="variables">{{ $t("auditor.system.filters.domains.variables") }}</option>
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
                        <span class="badge rounded-pill system-event-badge system-domain-badge" style="font-size:0.62rem">
                            {{ eventDomainLabel(entry.eventType) }}
                        </span>
                        <span class="badge rounded-pill system-event-badge" :class="eventBadgeClass(entry.eventType)" style="font-size:0.62rem">
                            {{ eventActionLabel(entry.eventType) }}
                        </span>
                        <span class="small fw-semibold text-truncate">{{ formatUserName(entry.userName) }}</span>
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
                    <span v-if="entry.ipAddress" class="small text-muted" style="font-size:0.65rem">
                        IP {{ entry.ipAddress }}
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
    import { loadApiKeyAuditLog } from "@/services/settings/apiKeysSettings";
    import { loadGlobalVariableAuditLog } from "@/services/settings/globalVariablesSettings";

    const LEGACY_EVENT_TYPES = new Set([
        "access",
        "apiCall",
        "profileChanged",
        "userManagement",
        "workflowChanged",
    ]);

    const EVENT_ICONS = {
        accessLogin: "LogIn",
        accessLogout: "LogOut",
        userCreated: "UserPlus",
        userUpdated: "UserCog",
        userDeleted: "UserMinus",
        teamCreated: "UsersRound",
        teamUpdated: "Users",
        teamDeleted: "UserX",
        permissionCreated: "ShieldCheck",
        permissionUpdated: "Shield",
        permissionDeleted: "ShieldX",
        apiKeyCreated: "KeyRound",
        apiKeyDeleted: "KeyRound",
        globalVariableCreated: "Braces",
        globalVariableUpdated: "Braces",
        globalVariableDeleted: "Braces",
    };
    const EVENT_BG = {
        accessLogin: "ev-bg-access",
        accessLogout: "ev-bg-access-out",
        userCreated: "ev-bg-user",
        userUpdated: "ev-bg-user",
        userDeleted: "ev-bg-user-del",
        teamCreated: "ev-bg-team",
        teamUpdated: "ev-bg-team",
        teamDeleted: "ev-bg-team-del",
        permissionCreated: "ev-bg-perm",
        permissionUpdated: "ev-bg-perm",
        permissionDeleted: "ev-bg-perm-del",
        apiKeyCreated: "ev-bg-key",
        apiKeyDeleted: "ev-bg-key-del",
        globalVariableCreated: "ev-bg-variable",
        globalVariableUpdated: "ev-bg-variable",
        globalVariableDeleted: "ev-bg-variable-del",
    };
    const EVENT_BADGE = {
        accessLogin: "ev-badge-access",
        accessLogout: "ev-badge-access-out",
        userCreated: "ev-badge-user",
        userUpdated: "ev-badge-user",
        userDeleted: "ev-badge-user-del",
        teamCreated: "ev-badge-team",
        teamUpdated: "ev-badge-team",
        teamDeleted: "ev-badge-team-del",
        permissionCreated: "ev-badge-perm",
        permissionUpdated: "ev-badge-perm",
        permissionDeleted: "ev-badge-perm-del",
        apiKeyCreated: "ev-badge-key",
        apiKeyDeleted: "ev-badge-key-del",
        globalVariableCreated: "ev-badge-variable",
        globalVariableUpdated: "ev-badge-variable",
        globalVariableDeleted: "ev-badge-variable-del",
    };

    function systemEventDomain(eventType) {
        if (!eventType) return "";
        if (eventType.startsWith("access")) return "access";
        if (eventType.startsWith("user")) return "users";
        if (eventType.startsWith("team")) return "teams";
        if (eventType.startsWith("permission")) return "permissions";
        if (eventType.startsWith("apiKey")) return "keys";
        if (eventType.startsWith("globalVariable")) return "variables";
        return "";
    }

    export default {
        name: "AuditorSystemSection",
        components: { LoadingComponent },
        data() {
            return {
                isLoading: false,
                allEvents: [],
                localKeyEvents: [],
                localVariableEvents: [],
                search: "",
                domainFilter: "",
                orderDescending: true,
                take: 20,
                skip: 0,
                hasMore: false,
            };
        },
        computed: {
            mergedEvents() {
                return [...this.allEvents, ...this.localKeyEvents, ...this.localVariableEvents].sort(
                    (a, b) => new Date(b.createdAt) - new Date(a.createdAt),
                );
            },
            displayedEvents() {
                let list = this.mergedEvents.filter((e) => !LEGACY_EVENT_TYPES.has(e.eventType));
                if (this.search) {
                    const q = this.search.toLowerCase();
                    list = list.filter(
                        (e) =>
                            e.userName?.toLowerCase().includes(q) ||
                            e.detail?.toLowerCase().includes(q) ||
                            e.keyName?.toLowerCase().includes(q) ||
                            e.variableName?.toLowerCase().includes(q)
                    );
                }
                if (this.domainFilter) {
                    list = list.filter((e) => systemEventDomain(e.eventType) === this.domainFilter);
                }
                return this.orderDescending ? list : [...list].reverse();
            },
            kpiCards() {
                const t = this.$t;
                const relevant = this.mergedEvents.filter((e) => !LEGACY_EVENT_TYPES.has(e.eventType));
                const countDomain = (domain) =>
                    relevant.filter((e) => systemEventDomain(e.eventType) === domain).length;
                return [
                    {
                        icon: "LogIn",
                        iconBg: "ev-bg-access",
                        label: t("auditor.system.summary.totalAccesses"),
                        value: countDomain("access"),
                    },
                    {
                        icon: "Users",
                        iconBg: "ev-bg-user",
                        label: t("auditor.system.filters.domains.users"),
                        value: countDomain("users"),
                    },
                    {
                        icon: "UsersRound",
                        iconBg: "ev-bg-team",
                        label: t("auditor.system.filters.domains.teams"),
                        value: countDomain("teams"),
                    },
                    {
                        icon: "Shield",
                        iconBg: "ev-bg-perm",
                        label: t("auditor.system.filters.domains.permissions"),
                        value: countDomain("permissions"),
                    },
                    {
                        icon: "KeyRound",
                        iconBg: "ev-bg-key",
                        label: t("auditor.system.filters.domains.keys"),
                        value: countDomain("keys"),
                    },
                    {
                        icon: "Braces",
                        iconBg: "ev-bg-variable",
                        label: t("auditor.system.filters.domains.variables"),
                        value: countDomain("variables"),
                    },
                ];
            },
        },
        methods: {
            eventIcon(type) { return EVENT_ICONS[type] ?? "Activity"; },
            eventIconBg(type) { return EVENT_BG[type] ?? ""; },
            eventBadgeClass(type) { return EVENT_BADGE[type] ?? ""; },
            eventDomainLabel(eventType) {
                const domain = systemEventDomain(eventType);
                if (!domain) return eventType;
                const key = `auditor.system.filters.domains.${domain}`;
                const translated = this.$t(key);
                return translated !== key ? translated : domain;
            },
            eventActionLabel(eventType) {
                if (eventType === "accessLogin" || eventType === "accessLogout") {
                    const key = `auditor.system.eventTypes.${eventType}`;
                    const translated = this.$t(key);
                    return translated !== key ? translated : eventType;
                }
                if (eventType === "apiKeyCreated") return this.$t("auditor.system.actions.create");
                if (eventType === "apiKeyDeleted") return this.$t("auditor.system.actions.delete");
                if (eventType.endsWith("Created")) return this.$t("auditor.system.actions.create");
                if (eventType.endsWith("Updated")) return this.$t("auditor.system.actions.update");
                if (eventType.endsWith("Deleted")) return this.$t("auditor.system.actions.delete");
                const key = `auditor.system.eventTypes.${eventType}`;
                const translated = this.$t(key);
                return translated !== key ? translated : eventType;
            },
            formatDate(d) { return dateHelper.formatDateWithTime(d) || "—"; },
            formatUserName(value) {
                if (!value) return "—";
                if (typeof value === "string" && value.includes("@")) {
                    const local = value.split("@")[0].replace(/\./g, " ");
                    return local.replace(/\b\w/g, (char) => char.toUpperCase());
                }
                return value;
            },
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
            this.localKeyEvents = loadApiKeyAuditLog();
            this.localVariableEvents = loadGlobalVariableAuditLog();
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
    .ev-bg-access-out { background-color: rgba(100,116,139,0.12); color: #64748b; }
    .ev-bg-user-del { background-color: rgba(239,68,68,0.12); color: #dc2626; }
    .ev-bg-team     { background-color: rgba(139,92,246,0.12); color: #7c3aed; }
    .ev-bg-team-del { background-color: rgba(239,68,68,0.1); color: #b91c1c; }
    .ev-bg-perm     { background-color: rgba(14,165,233,0.12); color: #0284c7; }
    .ev-bg-perm-del { background-color: rgba(239,68,68,0.1); color: #b91c1c; }

    .ev-badge-access-out { background-color: rgba(100,116,139,0.12); color: #64748b; }
    .ev-badge-user-del { background-color: rgba(239,68,68,0.12); color: #dc2626; }
    .ev-badge-team     { background-color: rgba(139,92,246,0.12); color: #7c3aed; }
    .ev-badge-team-del { background-color: rgba(239,68,68,0.1); color: #b91c1c; }
    .ev-badge-perm     { background-color: rgba(14,165,233,0.12); color: #0284c7; }
    .ev-badge-perm-del { background-color: rgba(239,68,68,0.1); color: #b91c1c; }
    .ev-bg-key         { background-color: rgba(234,179,8,0.14); color: #b45309; }
    .ev-bg-key-del     { background-color: rgba(239,68,68,0.1); color: #b91c1c; }
    .ev-badge-key      { background-color: rgba(234,179,8,0.14); color: #b45309; }
    .ev-badge-key-del  { background-color: rgba(239,68,68,0.1); color: #b91c1c; }
    .ev-bg-variable        { background-color: rgba(8,145,178,0.12); color: #0891b2; }
    .ev-bg-variable-del    { background-color: rgba(239,68,68,0.1); color: #b91c1c; }
    .ev-badge-variable     { background-color: rgba(8,145,178,0.12); color: #0e7490; }
    .ev-badge-variable-del { background-color: rgba(239,68,68,0.1); color: #b91c1c; }

    .system-domain-badge {
        background-color: var(--bs-tertiary-bg, rgba(0, 0, 0, 0.06));
        color: var(--bs-secondary-color, #64748b);
        font-weight: 600;
    }

    .border { border: 1px solid var(--color-border-form-control) !important; }
</style>
