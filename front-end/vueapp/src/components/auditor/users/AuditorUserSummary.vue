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
                    v-if="auditUserList.length === 0"
                    class="audit-list-empty text-muted small text-center py-5"
                >
                    {{ $t("auditor.users.summary.empty") }}
                </div>
                <div
                    v-else
                    class="audit-list overflow-auto flex-grow-1 min-h-0"
                >
                    <div
                        v-for="item in auditUserList"
                        :key="item.userId"
                        class="audit-list-item user-card-item rounded-2 p-2 mb-2 cursor-pointer"
                        :class="{
                            'audit-list-item-selected border-start border-primary border-3':
                                selectedUser && selectedUser.userId === item.userId,
                            border: !selectedUser || selectedUser.userId !== item.userId,
                        }"
                        @click="selectUser(item)"
                    >
                        <div class="d-flex align-items-start gap-2">
                            <span
                                class="user-card-icon d-inline-flex align-items-center justify-content-center flex-shrink-0"
                            >
                                <LucideIcon
                                    icon="User"
                                    :size="18"
                                />
                            </span>
                            <div class="min-w-0 flex-grow-1">
                                <div class="user-card-name fw-semibold small text-break mb-1">
                                    {{ item.userName }}
                                </div>
                                <div
                                    v-if="getItemTeams(item).length > 0"
                                    class="d-flex flex-wrap gap-1 mb-1"
                                >
                                    <BadgeComponent
                                        v-for="(team, idx) in getItemTeams(item)"
                                        :key="team.teamId ?? `${item.userId}-${idx}`"
                                        :text="team.teamName"
                                        variant="secondary"
                                        size="sm"
                                        :clickable="false"
                                    />
                                </div>
                                <div
                                    class="small text-muted d-flex align-items-center flex-wrap gap-2"
                                >
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Zap"
                                            :size="12"
                                        />
                                        {{
                                            $t("auditor.users.summary.actionsCount", {
                                                count: item.logCount,
                                            })
                                        }}
                                    </span>
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Workflow"
                                            :size="12"
                                        />
                                        {{
                                            $t("auditor.users.summary.workflowsCount", {
                                                count: item.workflowCount,
                                            })
                                        }}
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
                        {{ $t("auditor.users.summary.loadMore") }}
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
        name: "AuditorUserSummary",
        components: {
            BadgeComponent,
            LoadingComponent,
        },
        props: {
            filters: {
                type: Object,
                default: () => ({ search: "", teamId: "" }),
            },
        },
        emits: ["select-user"],
        data() {
            return {
                isLoading: false,
                selectedUser: null,
                auditUserList: [],
                take: 10,
                skip: 0,
                hasMore: false,
            };
        },
        methods: {
            getItemTeams(item) {
                const teams = item?.teams;
                if (!Array.isArray(teams)) return [];
                return teams.filter((t) => t && t.teamName);
            },
            selectUser(item) {
                this.selectedUser = item;
                this.$emit("select-user", item);
            },
            async getAuditUsersSummary(append = false) {
                this.isLoading = true;
                try {
                    const search = (this.filters.search || "").trim() || undefined;
                    const teamIdRaw = this.filters.teamId
                        ? parseInt(this.filters.teamId, 10)
                        : undefined;
                    const teamId =
                        teamIdRaw !== undefined && !Number.isNaN(teamIdRaw) ? teamIdRaw : undefined;
                    const params = {
                        take: this.take,
                        skip: this.skip,
                        ...(search && { userName: search }),
                        ...(teamId !== undefined && { teamId }),
                    };

                    const response = await AuditorsService.getUserAuditSummary(params);
                    if (response.error) {
                        return this.$notify({
                            title: "auditor.users.title",
                            message:
                                response.error.response?.data?.detail ?? response.error.message,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }

                    this.auditUserList = append
                        ? [...this.auditUserList, ...response.items]
                        : response.items;
                    this.hasMore = response?.hasMore === true;
                } finally {
                    this.isLoading = false;
                }
            },
            loadMore() {
                this.skip += this.take;
                this.getAuditUsersSummary(true);
            },
            refreshWithCurrentFilters() {
                this.skip = 0;
                this.getAuditUsersSummary(false);
            },
        },
        async created() {
            await this.getAuditUsersSummary();
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
    .user-card-icon {
        width: 36px;
        height: 36px;
        border-radius: 8px;
        background-color: var(--bs-primary-bg-subtle, rgba(13, 110, 253, 0.15));
        color: var(--bs-primary);
    }
    .user-card-name {
        color: var(--bs-body-color);
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
