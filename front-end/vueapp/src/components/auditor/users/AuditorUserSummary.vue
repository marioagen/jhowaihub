<template>
    <div>
        <div
            v-if="loading"
            class="audit-list-wrapper d-flex flex-column flex-grow-1 min-h-0 align-items-center justify-content-center py-5"
        >
            <LoadingComponent />
        </div>
        <template v-else>
            <div class="audit-list-wrapper d-flex flex-column flex-grow-1 min-h-0">
                <div class="audit-list overflow-auto flex-grow-1 min-h-0">
                    <div
                        v-for="user in displayedUserItems"
                        :key="user.id"
                        class="audit-list-item user-card-item rounded-2 p-2 mb-2 cursor-pointer"
                        :class="{
                            'audit-list-item-selected border-start border-primary border-3':
                                selectedUser && selectedUser.id === user.id,
                            border: !selectedUser || selectedUser.id !== user.id,
                        }"
                        @click="$emit('select-user', user)"
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
                                    {{ user.name }}
                                </div>
                                <div class="d-flex flex-wrap gap-1 mb-1">
                                    <BadgeComponent
                                        :text="user.teamName"
                                        :variant="user.teamVariant"
                                        size="sm"
                                        :clickable="false"
                                    />
                                </div>
                                <div class="d-flex flex-wrap gap-1 mb-1">
                                    <BadgeComponent
                                        :text="user.primaryWorkflowLabel"
                                        variant="info"
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
                                        {{ user.actionsCount }} ações
                                    </span>
                                    <span class="d-inline-flex align-items-center gap-1">
                                        <LucideIcon
                                            icon="Workflow"
                                            :size="12"
                                        />
                                        {{ user.workflowsCount }} esteira{{
                                            user.workflowsCount !== 1 ? "s" : ""
                                        }}
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

    const MOCK_USER_ITEMS = [
        {
            id: "user-1",
            name: "Dra. Mariana Costa",
            teamId: "juridico",
            teamName: "Time Juridico",
            teamVariant: "secondary",
            primaryWorkflowLabel: "Análise Juridica de...",
            actionsCount: 7,
            workflowsCount: 1,
        },
        {
            id: "user-2",
            name: "João Ferreira",
            teamId: "juridico",
            teamName: "Time Juridico",
            teamVariant: "secondary",
            primaryWorkflowLabel: "Análise Juridica de...",
            actionsCount: 5,
            workflowsCount: 2,
        },
        {
            id: "user-3",
            name: "Ana Costa",
            teamId: "financeiro",
            teamName: "Time Financeiro",
            teamVariant: "danger",
            primaryWorkflowLabel: "Processamento de Not...",
            actionsCount: 12,
            workflowsCount: 1,
        },
        {
            id: "user-4",
            name: "Carlos Silva",
            teamId: "financeiro",
            teamName: "Time Financeiro",
            teamVariant: "danger",
            primaryWorkflowLabel: "Processamento de Not...",
            actionsCount: 9,
            workflowsCount: 2,
        },
        {
            id: "user-5",
            name: "Roberto Lima",
            teamId: "financeiro",
            teamName: "Time Financeiro",
            teamVariant: "danger",
            primaryWorkflowLabel: "Conciliação Bancária",
            actionsCount: 15,
            workflowsCount: 1,
        },
        {
            id: "user-6",
            name: "Fernanda Alves",
            teamId: "rh",
            teamName: "Time RH",
            teamVariant: "info",
            primaryWorkflowLabel: "Gestão de Documentos..",
            actionsCount: 8,
            workflowsCount: 2,
        },
        {
            id: "user-7",
            name: "Luciana Melo",
            teamId: "rh",
            teamName: "Time RH",
            teamVariant: "info",
            primaryWorkflowLabel: "Gestão de Documentos..",
            actionsCount: 6,
            workflowsCount: 1,
        },
        {
            id: "user-8",
            name: "Paula Santos",
            teamId: "juridico",
            teamName: "Time Juridico",
            teamVariant: "secondary",
            primaryWorkflowLabel: "Due Diligence Contratual",
            actionsCount: 4,
            workflowsCount: 1,
        },
        {
            id: "user-9",
            name: "Ricardo Oliveira",
            teamId: "financeiro",
            teamName: "Time Financeiro",
            teamVariant: "danger",
            primaryWorkflowLabel: "Fechamento Mensal",
            actionsCount: 11,
            workflowsCount: 2,
        },
        {
            id: "user-10",
            name: "Carla Mendes",
            teamId: "rh",
            teamName: "Time RH",
            teamVariant: "info",
            primaryWorkflowLabel: "Onboarding de Colaboradores",
            actionsCount: 10,
            workflowsCount: 1,
        },
        {
            id: "user-11",
            name: "Eduardo Souza",
            teamId: "juridico",
            teamName: "Time Juridico",
            teamVariant: "secondary",
            primaryWorkflowLabel: "Renovação de Contratos",
            actionsCount: 3,
            workflowsCount: 1,
        },
    ];

    export default {
        name: "AuditorUserSummary",
        components: {
            BadgeComponent,
            LoadingComponent,
        },
        props: {
            selectedUser: {
                type: Object,
                default: null,
            },
            search: {
                type: String,
                default: "",
            },
            teamId: {
                type: String,
                default: "",
            },
        },
        emits: ["select-user"],
        data() {
            return {
                loading: false,
                userItems: [],
                displayedLimit: 10,
            };
        },
        computed: {
            filteredUserItems() {
                const q = (this.search || "").toLowerCase().trim();
                const tid = this.teamId;
                return this.userItems.filter((item) => {
                    const matchesSearch = !q || (item.name && item.name.toLowerCase().includes(q));
                    const matchesTeam = !tid || (item.teamId && item.teamId === tid);
                    return matchesSearch && matchesTeam;
                });
            },
            displayedUserItems() {
                return this.filteredUserItems.slice(0, this.displayedLimit);
            },
            showLoadMoreButton() {
                return (
                    this.filteredUserItems.length > 10 &&
                    this.displayedLimit < this.filteredUserItems.length
                );
            },
        },
        methods: {
            async getData() {
                this.loading = true;
                try {
                    await new Promise((r) => setTimeout(r, 400));
                    this.userItems = [...MOCK_USER_ITEMS];
                } finally {
                    this.loading = false;
                }
            },
            loadMore() {
                this.displayedLimit = Math.min(
                    this.displayedLimit + 10,
                    this.filteredUserItems.length
                );
            },
        },
        mounted() {
            this.getData();
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
        max-height: calc(100vh - 400px);
    }
    .audit-list-wrapper .audit-list {
        min-height: 0;
    }
</style>
