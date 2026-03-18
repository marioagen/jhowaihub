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
                    Selecione um usuário para ver o histórico de ações
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
                    <!-- 1. User profile card -->
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

                    <!-- 2. Summary stat cards -->
                    <div class="row g-2 mb-3">
                        <div class="col-6 col-md-3">
                            <div
                                class="user-detail-stat-card rounded-2 p-2 border d-flex flex-column align-items-center text-center"
                            >
                                <span class="user-detail-stat-value fw-bold">
                                    {{ totalActions3And13 }}
                                </span>
                                <span class="small text-muted">Total de Ações</span>
                            </div>
                        </div>
                        <div class="col-6 col-md-3">
                            <div
                                class="user-detail-stat-card rounded-2 p-2 border d-flex flex-column align-items-center text-center"
                            >
                                <span class="user-detail-stat-value fw-bold">
                                    {{ selectedUser.workflowCount ?? 0 }}
                                </span>
                                <span class="small text-muted">Esteiras</span>
                            </div>
                        </div>
                        <div
                            v-for="act in logCountOnly3And13"
                            :key="act.actionTypeCode"
                            class="col-6 col-md-3"
                        >
                            <div
                                class="user-detail-stat-card rounded-2 p-2 border d-flex flex-column align-items-center text-center"
                            >
                                <span class="user-detail-stat-value fw-bold">{{ act.count }}</span>
                                <span class="small text-muted">{{ act.label }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- 3. Activity history -->
                    <div
                        class="user-detail-activity-section d-flex flex-column flex-grow-1 min-h-0"
                    >
                        <!-- Header row: title + count on left, filters on right -->
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
                                Histórico de Atividade
                                <BadgeComponent
                                    :text="activityEntries.length + ' eventos'"
                                    variant="secondary"
                                    size="sm"
                                    :clickable="false"
                                />
                            </h6>
                            <div class="d-flex align-items-center gap-2">
                                <button
                                    type="button"
                                    class="btn btn-light btn-sm border py-1 px-2 user-detail-filter d-flex align-items-center gap-1"
                                    :class="orderDescending ? 'btn-primary' : ''"
                                    @click="toggleOrderAndRefresh"
                                >
                                    <LucideIcon
                                        icon="ArrowUpDown"
                                        :size="12"
                                    />
                                    {{ orderDescending ? "Mais recentes" : "Mais antigos" }}
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
                                            v-for="opt in actionFilterOptions"
                                            :key="opt.value"
                                        >
                                            <a
                                                class="dropdown-item"
                                                href="#"
                                                @click.prevent="setActionFilter(opt.value)"
                                            >
                                                {{ opt.label }}
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                        <!-- Search row -->
                        <div class="mb-2">
                            <div class="input-group input-group-sm user-detail-filter">
                                <span class="input-group-text border-end-0 py-1">
                                    <LucideIcon
                                        icon="Search"
                                        :size="14"
                                    />
                                </span>
                                <input
                                    v-model="activitySearch"
                                    type="text"
                                    class="form-control form-control-sm border-start-0 py-1"
                                    placeholder="Buscar por documento, detalhes, esteira, etapa..."
                                    aria-label="Buscar no histórico"
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
                                        <!-- Line 1: document name + action tag -->
                                        <div class="d-flex align-items-center flex-wrap gap-1 mb-1">
                                            <span class="user-activity-doc-title small fw-bold">
                                                {{ entry.cardName }}
                                            </span>
                                            <BadgeComponent
                                                :text="entry.actionType"
                                                :variant="actionBadgeVariant(entry.actionType)"
                                                size="sm"
                                                :clickable="false"
                                            />
                                        </div>
                                        <!-- Line 2: description -->
                                        <div
                                            class="small text-muted mb-1 user-activity-description"
                                        >
                                            {{ activityDescription(entry) }}
                                        </div>
                                        <!-- Line 3: timestamp (left) | workflow + optional context (right) -->
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
                                    @click="activityDisplayedLimit += 10"
                                >
                                    Carregar mais
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
    import AuditorsService from "@/services/auditors/AuditorsService";
    import dateHelper from "@/helpers/date.js";

    export default {
        name: "AuditorUserDetail",
        components: { BadgeComponent, LoadingComponent },
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
                activitySearch: "",
                selectedActionCode: null,
                orderDescending: true,
                activityDisplayedLimit: 10,
                actionFilterOptions: [
                    { value: null, label: "Todas as ações" },
                    { value: 3, label: "Avançar" },
                    { value: 13, label: "Documento de entrada" },
                ],
            };
        },
        computed: {
            /** Action type codes we show in user details: 3 = Advancement, 13 = InputDocument */
            ACTION_CODES_USER_DETAIL: () => [3, 13],
            ACTION_TYPE_NAMES_USER_DETAIL: () => ["Advancement", "InputDocument"],
            logCountOnly3And13() {
                const list = this.userDetail?.logCountByActionType ?? [];
                const labels = { 3: "Avançar", 13: "Documento de entrada" };
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
                const actions = this.userDetail?.actions ?? [];
                if (this.selectedActionCode != null) return actions;
                return actions.filter((a) =>
                    this.ACTION_TYPE_NAMES_USER_DETAIL.includes(a.actionType)
                );
            },
            filteredActivityEntries() {
                let list = this.activityEntries;
                const q = (this.activitySearch || "").toLowerCase().trim();
                if (q) {
                    list = list.filter((e) => {
                        const desc = this.activityDescription(e).toLowerCase();
                        return (
                            (e.cardName && e.cardName.toLowerCase().includes(q)) ||
                            (e.workflowName && e.workflowName.toLowerCase().includes(q)) ||
                            desc.includes(q)
                        );
                    });
                }
                return [...list];
            },
            displayedActivityEntries() {
                return this.filteredActivityEntries.slice(0, this.activityDisplayedLimit);
            },
            showActivityLoadMore() {
                const total = this.filteredActivityEntries.length;
                return total > 10 && this.activityDisplayedLimit < total;
            },
            selectedActionLabel() {
                const opt = this.actionFilterOptions.find(
                    (o) => o.value === this.selectedActionCode
                );
                return opt ? opt.label : "Todas as ações";
            },
        },
        methods: {
            formatDateWithTime(date) {
                return dateHelper.formatDateWithTime(date) || "—";
            },
            actionBadgeVariant(actionType) {
                if (!actionType) return "secondary";
                const t = actionType.toLowerCase();
                if (t.includes("delet") || t.includes("removed") || t.includes("rejection"))
                    return "danger";
                if (t.includes("upload") || t.includes("documentcreated") || t.includes("input"))
                    return "info";
                if (
                    t.includes("assign") ||
                    t.includes("atribuir") ||
                    t.includes("advancement") ||
                    t.includes("finalize") ||
                    t.includes("approval")
                )
                    return "primary";
                return "secondary";
            },
            activityDescription(entry) {
                const name = entry.cardName || "Documento";
                const workflow = entry.workflowName;
                const action = (entry.actionType || "").toLowerCase();
                if (
                    action.includes("upload") ||
                    action.includes("documentcreated") ||
                    action.includes("inputdocument")
                )
                    return "Documento carregado no sistema";
                if (action.includes("assign") || action.includes("atribuir"))
                    return workflow
                        ? `Documento atribuído para ${workflow}`
                        : "Documento atribuído";
                if (action.includes("delet") || action.includes("removed"))
                    return "Documento deletado";
                if (action.includes("advancement"))
                    return workflow
                        ? `Documento encaminhado para ${workflow}`
                        : "Documento encaminhado";
                if (action.includes("finalize")) return "Documento finalizado";
                if (action.includes("approval")) return "Documento aprovado";
                if (action.includes("rejection")) return "Documento rejeitado";
                if (action.includes("unassign")) return "Atribuição removida";
                if (action.includes("editanswer")) return "Resposta editada";
                if (action.includes("inputquestionnaire")) return "Questionário preenchido";
                return `${name} — ${entry.actionType || "Ação"}`;
            },
            toggleOrderAndRefresh() {
                this.orderDescending = !this.orderDescending;
                this.refreshWithCurrentDocument();
            },
            setActionFilter(value) {
                this.selectedActionCode = value;
                this.refreshWithCurrentDocument();
            },
            async refreshWithCurrentDocument() {
                if (this.selectedUser?.userId == null) {
                    this.userDetail = null;
                    return;
                }
                this.activityDisplayedLimit = 10;
                this.isLoading = true;
                const search = (this.activitySearch || "").trim() || undefined;
                const params = {
                    ...(search && { search }),
                    ...(this.selectedActionCode != null && {
                        actionTypeCode: this.selectedActionCode,
                    }),
                    orderDescending: this.orderDescending,
                };
                try {
                    const response = await AuditorsService.getUserAuditDetails(
                        this.selectedUser.userId,
                        params
                    );
                    if (response.error) {
                        this.$notify({
                            title: "audit-users.title",
                            message:
                                response.error.response?.data?.detail ?? response.error.message,
                            variant: "danger",
                            icon: "CircleX",
                        });
                        this.userDetail = null;
                        return;
                    }
                    this.userDetail =
                        response && typeof response === "object" && !Array.isArray(response)
                            ? response
                            : (response?.data ?? null);
                } finally {
                    this.isLoading = false;
                }
            },
        },
        watch: {
            selectedUser: {
                handler() {
                    this.activitySearch = "";
                    this.selectedActionCode = null;
                    this.activityDisplayedLimit = 10;
                    this.userDetail = null;
                    if (this.selectedUser?.userId != null) {
                        this.refreshWithCurrentDocument();
                    }
                },
                immediate: true,
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
