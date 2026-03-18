<template>
    <div class="card-body d-flex flex-column p-0">
        <template v-if="!selectedUser">
            <div
                class="d-flex flex-column align-items-center justify-content-center min-vh-50 py-5"
            >
                <div class="user-detail-placeholder-icon text-secondary mb-3">
                    <LucideIcon
                        icon="User"
                        :size="64"
                        stroke-width="1.25"
                    />
                </div>
                <p class="text-muted text-center mb-0">
                    {{ $t("auditor.users.detail.selectUser") }}
                </p>
            </div>
        </template>
        <template v-else>
            <div
                v-if="isLoading"
                class="d-flex align-items-center justify-content-center flex-grow-1 min-vh-50 p-5"
            >
                <LoadingComponent />
            </div>
            <template v-else>
                <div class="user-detail-content p-3 d-flex flex-column flex-grow-1 min-h-0">
                    <div
                        class="user-detail-profile-card rounded-2 p-3 mb-3 border d-flex align-items-center"
                    >
                        <div class="d-flex align-items-center flex-wrap gap-2 w-100">
                            <span
                                class="user-detail-profile-icon d-inline-flex align-items-center justify-content-center flex-shrink-0"
                            >
                                <LucideIcon
                                    icon="User"
                                    :size="24"
                                />
                            </span>
                            <span class="user-detail-name fw-bold flex-grow-1">
                                {{ userDetail?.userName ?? selectedUser.userName }}
                            </span>
                            <template
                                v-for="(team, idx) in teamsList"
                                :key="team.teamId ?? idx"
                            >
                                <BadgeComponent
                                    v-if="team.teamName"
                                    :text="team.teamName"
                                    variant="secondary"
                                    size="sm"
                                    :clickable="false"
                                />
                            </template>
                        </div>
                    </div>

                    <div class="row g-2 mb-3">
                        <div class="col-6 col-md-3">
                            <div
                                class="user-detail-stat-card rounded-2 p-2 border d-flex flex-column align-items-center text-center"
                            >
                                <span class="user-detail-stat-value fw-bold">
                                    {{ totalActions3And13 }}
                                </span>
                                <span class="small text-muted">
                                    {{ $t("auditor.users.detail.totalActions") }}
                                </span>
                            </div>
                        </div>
                        <div class="col-6 col-md-3">
                            <div
                                class="user-detail-stat-card rounded-2 p-2 border d-flex flex-column align-items-center text-center"
                            >
                                <span class="user-detail-stat-value fw-bold">
                                    {{ selectedUser.workflowCount ?? 0 }}
                                </span>
                                <span class="small text-muted">
                                    {{ $t("auditor.users.detail.workflows") }}
                                </span>
                            </div>
                        </div>
                        <div
                            v-for="action in logCountOnly3And13"
                            :key="action.actionTypeCode"
                            class="col-6 col-md-3"
                        >
                            <div
                                class="user-detail-stat-card rounded-2 p-2 border d-flex flex-column align-items-center text-center"
                            >
                                <span class="user-detail-stat-value fw-bold">
                                    {{ action.count }}
                                </span>
                                <span class="small text-muted">{{ action.label }}</span>
                            </div>
                        </div>
                    </div>

                    <div
                        class="user-detail-activity-section d-flex flex-column flex-grow-1 min-h-0"
                    >
                        <div
                            class="d-flex align-items-center justify-content-between flex-wrap gap-2 mb-2"
                        >
                            <h6
                                class="mb-0 small fw-normal d-flex align-items-center gap-1 user-detail-heading"
                            >
                                <LucideIcon
                                    icon="History"
                                    :size="14"
                                    class="text-primary"
                                />
                                {{ $t("auditor.users.detail.activityHistory") }}
                                <BadgeComponent
                                    :text="
                                        $t('auditor.users.detail.events', {
                                            count: activityEntries.length,
                                        })
                                    "
                                    variant="secondary"
                                    size="sm"
                                    :clickable="false"
                                />
                            </h6>
                            <div class="d-flex align-items-center gap-2">
                                <button
                                    type="button"
                                    class="btn btn-light btn-sm border py-1 px-2 user-detail-filter d-flex align-items-center gap-1"
                                    :class="filters.orderDescending ? 'btn-primary' : ''"
                                    @click="toggleOrderAndRefresh"
                                >
                                    <LucideIcon
                                        icon="ArrowUpDown"
                                        :size="12"
                                    />
                                    {{
                                        filters.orderDescending
                                            ? $t("auditor.users.detail.orderNewest")
                                            : $t("auditor.users.detail.orderOldest")
                                    }}
                                </button>
                                <div class="dropdown">
                                    <button
                                        class="btn btn-light btn-sm border py-1 px-2 user-detail-filter d-flex align-items-center gap-1 dropdown-toggle"
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
                                            :key="action.value"
                                        >
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                                @click.prevent="setActionFilter(action.value)"
                                            >
                                                {{ action.label }}
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                        <div class="mb-2">
                            <div class="input-group input-group-sm user-detail-filter">
                                <span class="input-group-text border-end-0 py-1">
                                    <LucideIcon
                                        icon="Search"
                                        :size="14"
                                    />
                                </span>
                                <input
                                    v-model="filters.input"
                                    type="text"
                                    class="form-control form-control-sm border-start-0 py-1"
                                    :placeholder="$t('auditor.users.detail.searchPlaceholder')"
                                    :aria-label="$t('auditor.users.detail.searchAria')"
                                    @input="onActivitySearchInput"
                                />
                            </div>
                        </div>

                        <div
                            class="user-detail-activity-list overflow-auto flex-grow-1 min-h-0 d-flex flex-column"
                        >
                            <div
                                v-for="(entry, index) in displayedActivityEntries"
                                :key="entry.cardId + '-' + index"
                                class="user-activity-card rounded-2 p-2 mb-2 border"
                            >
                                <div class="d-flex align-items-start gap-2">
                                    <span
                                        class="user-activity-doc-icon d-inline-flex align-items-center justify-content-center flex-shrink-0"
                                    >
                                        <LucideIcon
                                            icon="FileText"
                                            :size="16"
                                        />
                                    </span>
                                    <div class="min-w-0 flex-grow-1 user-activity-card-content">
                                        <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                            <span class="user-activity-doc-title small fw-bold">
                                                {{ entry.cardName }}
                                            </span>
                                            <BadgeComponent
                                                v-if="entry.actionType"
                                                :text="auditActionDisplay(entry).title"
                                                variant="primary"
                                                size="sm"
                                                :clickable="false"
                                            />
                                        </div>
                                        <div
                                            class="small text-muted mb-1 user-activity-description"
                                        >
                                            {{ auditActionDisplay(entry).action }}
                                        </div>
                                        <div
                                            class="d-flex align-items-center justify-content-between flex-wrap gap-2 small text-muted"
                                        >
                                            <span class="d-inline-flex align-items-center gap-1">
                                                <LucideIcon
                                                    icon="Clock"
                                                    :size="12"
                                                />
                                                {{ formatDateWithTime(entry.created) }}
                                            </span>
                                            <span class="d-inline-flex align-items-center gap-1">
                                                <template v-if="entry.workflowName">
                                                    <LucideIcon
                                                        icon="Workflow"
                                                        :size="12"
                                                    />
                                                    {{ entry.workflowName }}
                                                </template>
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div
                                v-if="showActivityLoadMore"
                                class="mt-2 mb-3 text-center"
                            >
                                <button
                                    type="button"
                                    class="btn btn-outline-primary btn-sm"
                                    @click="loadMoreActivity"
                                >
                                    {{ $t("auditor.users.detail.loadMore") }}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </template>
    </div>
</template>
<script>
    import BadgeComponent from "@/components/global/BadgeComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import auditActionHelper from "@/helpers/auditActionHelper";
    import AuditorsService from "@/services/auditors/AuditorsService";
    import dateHelper from "@/helpers/date.js";

    export default {
        name: "AuditorUserDetail",
        components: {
            BadgeComponent,
            LoadingComponent,
        },
        props: {
            selectedUser: {
                type: Object,
                default: null,
            },
        },
        data() {
            return {
                isLoading: false,
                userDetail: null,
                userActions: [],
                filters: {
                    input: "",
                    orderDescending: true,
                    actionType: null,
                    take: 10,
                },
                activitySearchDebounceTimer: null,
            };
        },
        computed: {
            actionFilterOptions() {
                const t = this.$t;
                return [
                    { value: null, label: t("auditor.users.detail.allActions") },
                    { value: 3, label: t("auditor.users.detail.actionAdvance") },
                    { value: 13, label: t("auditor.users.detail.actionInputDocument") },
                ];
            },
            ACTION_CODES_USER_DETAIL: () => [3, 13],
            ACTION_TYPE_NAMES_USER_DETAIL: () => ["Advancement", "InputDocument"],
            logCountOnly3And13() {
                const list = this.userDetail?.logCountByActionType ?? [];
                const t = this.$t;
                const labels = {
                    3: t("auditor.users.detail.actionAdvance"),
                    13: t("auditor.users.detail.actionInputDocument"),
                };
                return this.ACTION_CODES_USER_DETAIL.map((code) => {
                    const item = list.find((a) => a.actionTypeCode === code);
                    return {
                        actionTypeCode: code,
                        count: item?.count ?? 0,
                        label: labels[code] ?? `Tipo ${code}`,
                    };
                });
            },
            totalActions3And13() {
                return this.logCountOnly3And13.reduce((sum, a) => sum + a.count, 0);
            },
            teamsList() {
                const teams = this.userDetail?.teams ?? this.selectedUser?.teams;
                if (!Array.isArray(teams)) return [];
                return teams.filter((t) => t && t.teamName);
            },
            activityEntries() {
                return this.userDetail?.actions ?? [];
            },
            displayedActivityEntries() {
                return this.activityEntries;
            },
            showActivityLoadMore() {
                const total = this.activityEntries.length;
                return total >= 10 && total === this.filters.take;
            },
            selectedActionLabel() {
                const opt = this.actionFilterOptions.find(
                    (o) => o.value === this.filters.actionType
                );
                return opt ? opt.label : this.$t("auditor.users.detail.allActions");
            },
        },
        methods: {
            formatDateWithTime(date) {
                return dateHelper.formatDateWithTime(date) || "—";
            },
            auditActionDisplay(entry) {
                return auditActionHelper.getAuditActionDisplay(entry?.actionType, {
                    t: this.$t,
                    stepName: entry?.stepName || this.$t("auditor.users.detail.nextStep"),
                });
            },
            onActivitySearchInput() {
                if (this.activitySearchDebounceTimer)
                    clearTimeout(this.activitySearchDebounceTimer);
                this.activitySearchDebounceTimer = setTimeout(() => {
                    this.activitySearchDebounceTimer = null;
                    if (this.selectedUser?.userId != null) {
                        this.getUserDetails();
                    }
                }, 300);
            },
            toggleOrderAndRefresh() {
                this.filters.orderDescending = !this.filters.orderDescending;
                this.getUserDetails();
            },
            setActionFilter(value) {
                this.filters.actionType = value;
                this.getUserDetails();
            },
            loadMoreActivity() {
                this.filters.take += 10;
                this.getUserDetails(false);
            },
            refreshWithCurrentDocument(resetActivityLimit = true) {
                return this.getUserDetails(resetActivityLimit);
            },
            async getUserDetails(resetActivityLimit = true) {
                if (this.selectedUser?.userId == null) {
                    this.userDetail = null;
                    return;
                }
                if (resetActivityLimit) {
                    this.filters.input = "";
                    this.filters.actionType = null;
                    this.filters.take = 10;
                    this.filters.orderDescending = true;
                    this.userDetail = null;
                }
                this.isLoading = true;
                const search = (this.filters.input || "").trim() || undefined;
                const params = {
                    take: this.filters.take,
                    ...(search && { search }),
                    ...(this.filters.actionType != null && {
                        actionTypeCode: this.filters.actionType,
                    }),
                    orderDescending: this.filters.orderDescending,
                };

                try {
                    const response = await AuditorsService.getUserAuditDetails(
                        this.selectedUser.userId,
                        params
                    );
                    if (response.error) {
                        this.userDetail = null;
                        return this.$notify({
                            title: "auditor.users.title",
                            message:
                                response.error.response?.data?.detail ?? response.error.message,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    const data = response?.data ?? response;
                    this.userDetail =
                        data && typeof data === "object" && !Array.isArray(data) ? data : null;
                    this.userActions = this.userDetail?.actions ?? [];
                } finally {
                    this.isLoading = false;
                }
            },
        },
    };
</script>
<style scoped>
    .min-vh-50 {
        min-height: 50vh;
    }
    .user-detail-placeholder-icon {
        opacity: 0.6;
    }
    .user-detail-profile-card {
        background-color: var(--bs-secondary-bg, transparent);
    }
    .user-detail-profile-icon {
        width: 48px;
        height: 48px;
        border-radius: 10px;
        background-color: var(--bs-primary-bg-subtle, rgba(13, 110, 253, 0.15));
        color: var(--bs-primary);
    }
    .user-detail-name {
        color: var(--bs-body-color);
    }
    .user-detail-stat-card {
        background-color: var(--bs-secondary-bg, transparent);
    }
    .user-detail-stat-value {
        color: var(--bs-body-color);
        font-size: 1.25rem;
    }
    .user-detail-heading {
        color: var(--bs-body-color);
    }
    .user-detail-filter {
        font-size: 0.75rem;
    }
    .user-detail-filter .form-control,
    .user-detail-filter .input-group-text {
        font-size: 0.75rem;
    }
    .user-detail-content {
        flex: 1 1 0;
        min-height: 0;
        overflow: hidden;
    }
    .user-detail-activity-section {
        flex: 1 1 0;
        min-height: 0;
        overflow: hidden;
    }
    .user-detail-activity-list {
        flex: 1 1 0;
        min-height: 0;
    }
    .user-activity-card {
        background-color: transparent;
    }
    .user-activity-doc-icon {
        width: 32px;
        height: 32px;
        border-radius: 8px;
        background-color: var(--bs-primary-bg-subtle, rgba(13, 110, 253, 0.15));
        color: var(--bs-primary);
    }
    .user-activity-doc-title {
        color: var(--bs-body-color);
    }
    .user-activity-card-content {
        flex: 1 1 100%;
        min-width: 0;
    }
    .user-activity-doc-title {
        color: var(--bs-primary);
    }
    .user-activity-description {
        color: var(--bs-secondary-color);
    }
    .user-activity-protocol {
        font-size: 0.7rem;
        color: var(--bs-secondary);
    }
</style>
