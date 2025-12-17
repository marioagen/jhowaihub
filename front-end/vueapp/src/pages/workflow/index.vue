<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("workflow.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("workflow.subtitle") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="redirectToNewUpload">
                        <LucideIcon icon="Plus" :size="17" />
                        {{ $t("documents.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="row">
                            <div class="col-6">
                                <div class="flex flex-col items-start gap-3 flex-1 align-items-center">
                                    <div>
                                        <LucideIcon icon="Clock" :size="14" class="me-2" />
                                        <span>{{ $t("workflow.boardView") }}</span>
                                    </div>
                                    <div class="dropdown">
                                        <button class="btn btn-light border text-start" type="button"
                                            data-bs-toggle="dropdown" aria-expanded="false">
                                            <div class="fw-bold font-size-sm">{{ selectedOption.teamName }}</div>
                                            <div class="text-muted font-size-xs">{{ selectedOption.name }}</div>
                                        </button>
                                        <ul class="dropdown-menu">
                                            <li v-for="item in workflowList" :key="item.id">
                                                <a class="dropdown-item" @click="selectOption(item)">
                                                    <div class="fw-bold">{{ item.teams.name }}</div>
                                                    <div class="text-muted small">{{ item.name }}</div>
                                                </a>
                                            </li>
                                        </ul>
                                    </div>
                                    <div class="badge bg-secondary badge-custom">
                                        <LucideIcon icon="Workflow" :size="14" class="me-2" stroke="#0d6efd" />
                                        <span>{{ selectedOption.name || $t("workflow.selectWorkflow") }}</span>
                                    </div>

                                </div>
                            </div>
                            <div class="col-6">
                                <WorkflowFilters @filter="filterData" class="ms-auto" />
                            </div>
                        </div>
                    </div>
                </div>
                <div v-if="isLoadingKanban">
                    <LoadingComponent />
                </div>
                <div v-else-if="hasList">
                    <div class="card mb-3 h-100">
                        <div class="card-body d-flex flex-column p-2 card-container">
                            <div class="kanban-wrapper">
                                <KanbanBoard :kanbanData="kanbanCards" :users="users" @reload="reloadKanban" />
                            </div>
                        </div>
                    </div>
                    <div class="card mb-3">
                        <div class="card-body">
                            <div class="flex flex-col items-start gap-4 flex-1 align-items-center">
                                <div>
                                    <span class="me-1">Workflow:</span>
                                    <b>{{ selectedOption.name }}</b>
                                </div>
                                <div>
                                    <span class="me-1">{{ $t("labelTeam") }}:</span>
                                    <b>{{ selectedOption.teamName }}</b>
                                </div>
                                <div>
                                    <span class="me-1">{{ $t("labelTotalDocuments") }}</span>
                                    <b>{{ kanbanCards.numDocuments }}</b>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div v-else class="text-center">
                    <span class="text-primary">{{ $t("workflow.notFound") }}</span>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
import signalRService from "@/services/signalR/signalRServices.js";
import GlobalEventService from "@/services/globalEventService.js";
import WorkflowService from "@/services/workflow/WorkflowService.js";
import KanbanBoard from "@/components/workflow/kanban/KanbanBoard.vue";
import WorkflowFilters from "@/components/workflow/WorkflowFilters.vue";
import UserService from "@/services/users/UserService";
import LoadingComponent from "@/components/global/LoadingComponent.vue";

export default {
    name: "WorkflowPage",
    data() {
        return {
            crumbsData: [],
            entitySearch: {},
            modalQuestion: {
                name: "",
            },
            workflowList: [],
            selectedOption: {
                id: 0,
                name: "Select a workflow",
                teamName: "Select a team",
                teamId: 0,
            },
            kanbanCards: [],
            numDocs: 0,
            isLoaded: false,
            isLoadedUsers: false,
            isLoadingKanban: true,
            signalrEventExecutionChanged: "CardExecutionChanged",
            filters: {
                orderBy: "",
                input: null,
                login: null,
                isAllUsers: true,
            },
            users: []
        };
    },
    components: {
        LoadingComponent,
        WorkflowFilters,
        KanbanBoard,
    },
    computed: {
        hasList() {
            return this.workflowList.length > 0;
        },
    },
    methods: {
        getWorkflowByUser() {
            this.isLoadingKanban = true;
            var email = this.$store.state.userProfile.login;
            WorkflowService.getWorkflowList(email)
                .then((response) => {
                    if (response.error !== undefined) {
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.error",
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    }

                    this.workflowList = response;
                    if (this.workflowList.length > 0) {
                        return this.setSelectedWorkflow();
                    }
                    this.isLoadingKanban = false;
                });
        },
        setSelectedWorkflow() {
            const redicteWorkflowId = this.$route.query.id;
            let workflowToSelect = this.workflowList[0];

            if (redicteWorkflowId !== undefined) {
                const foundWorkflow = this.workflowList.find(w => w.id == redicteWorkflowId);
                if (foundWorkflow) {
                    return this.selectOption(foundWorkflow);
                } else {
                    return this.$notify({
                        title: 'workflow.index',
                        message: 'workflow.error',
                        variant: 'danger',
                        icon: 'CircleX',
                    });
                }
            }

            const lastSelected = this.$store.state.lastSelectedWorkflow;
            if (lastSelected) {
                const foundWorkflow = this.workflowList.find(w => w.id === lastSelected.id);
                if (foundWorkflow) {
                    return this.selectOption(foundWorkflow);
                }
            }

            this.selectOption(workflowToSelect);
        },
        getWorkflowStepsById(workflowId) {
            this.isLoadingKanban = true;
            WorkflowService.getWorkflowStepsById(workflowId, this.filters)
                .then((response) => {
                    this.kanbanCards = response;
                })
                .finally(() => {
                    this.isLoadingKanban = false;
                });
        },
        getUsersByTeams(teams) {
            if (!teams || teams.length === 0) {
                this.users = [];
                this.isLoading = false;
                return;
            }

            this.isLoading = true;
            const teamIds = teams.map(t => t.id);

            UserService.getUsersByTeamIds(teamIds)
                .then(users => {
                    this.users = users;
                })
                .catch(error => {
                    console.error('Error loading users:', error);
                    this.users = [];
                })
                .finally(() => {
                    this.isLoading = false;
                });
        },
        selectOption(workflow) {
            if (!workflow?.id) return;

            this.isLoaded = false;
            this.isLoadedUsers = false;

            this.$store.commit('setLastSelectedWorkflow', {
                id: workflow.id,
                name: workflow.name,
            });

            this.selectedOption = {
                id: workflow.id,
                name: workflow.name,
            };

            this.getWorkflowStepsById(workflow.id);
            this.getUsersByTeams(workflow.teams);
        },
        reloadKanban() {
            this.getWorkflowStepsById(this.selectedOption.id);
        },
        filterData(filters) {
            this.filters = filters;
            this.reloadKanban();
        },
        redirectToNewUpload() {
            this.$router.push({ name: "DocumentsUpload" });
        },
    },
    created() {
        this.getWorkflowByUser();
        GlobalEventService.on("all-uploads-complete", this.getWorkflowByUser);
        GlobalEventService.on("refresh-once", this.getWorkflowByUser);
    },
    async mounted() {
        await signalRService.startConnection();
        signalRService.on(this.signalrEventExecutionChanged, (message) => {
            if (!this.kanbanCards.steps) return;
            let foundCard = null;

            for (let i = 0; i < this.kanbanCards.steps.length; i++) {
                const step = this.kanbanCards.steps[i];
                if (step.cards) {
                    const card = step.cards.find(c => c.id === message.cardId);
                    if (card) {
                        foundCard = card;
                        foundCard.toolName = message.toolName;
                        break;
                    }
                }
            }

            if (!foundCard) return;
            foundCard.percentage = message.percentage;
            if (message.percentage === 100.0 && foundCard.stepId !== message.stepId) {
                this.getWorkflowByUser();
            }
        });
    },
    beforeUnmount() {
        signalRService.off(this.signalrEventExecutionChanged);
        signalRService.stopConnection();
        GlobalEventService.off("all-uploads-complete", this.reloadKanban);
        GlobalEventService.off("refresh-once", this.reloadKanban);
    },
};
</script>

<style scoped>
.flex {
    display: flex;
}

.bg-secondary {
    background-color: #f5f7fa !important;
    color: gray;
    border-color: #f5f7fa !important;
}

.font-size-sm {
    font-size: small;
}

.font-size-xs {
    font-size: x-small;
}

.card-container {
    max-height: 70vh;
}

.kanban-wrapper {
    overflow-x: auto;
    white-space: nowrap;
}
</style>