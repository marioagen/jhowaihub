<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mb-3">            
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("workflow.title") }}</h5>
                        <p>
                            <small class="text-muted">{{$t("workflow.subtitle")}}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="redirectToNewUpload">
                        <LucideIcon icon="Plus" :size="17" />
                        {{ $t("documents.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="flex flex-col items-start gap-3 flex-1 align-items-center">
                            <div>
                                <LucideIcon icon="Clock" :size="14" class="me-2" />
                                <span>{{$t("workflow.boardView")}}</span>
                            </div>
                            <div class="dropdown">
                                <button 
                                    class="btn btn-light border text-start"
                                    type="button"
                                    data-bs-toggle="dropdown"
                                    aria-expanded="false"
                                >
                                    <div class="fw-bold font-size-sm">{{ selectedOption.teamName }}</div>
                                    <div class="text-muted font-size-xs">{{ selectedOption.name }}</div>
                                </button>
                                <ul class="dropdown-menu">
                                    <li 
                                        v-for="item in workflowList" 
                                        :key="item.id"
                                    >
                                        <a 
                                            class="dropdown-item" 
                                            @click="selectOption(item)"
                                        >
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
                            <WorkflowFilters @filter="filterData" class="ms-auto" />
                        </div>
                    </div>
                </div>
                <div v-if="isLoadingKanban">
                    <div class="d-flex justify-content-center">
                        <div class="spinner-border text-primary" role="status"></div>
                    </div>
                </div>
                <div v-else>                    
                    <div class="card mb-3 h-100">
                        <div class="card-body d-flex flex-column p-2 card-container">
                            <div class="kanban-wrapper">
                                <KanbanBoard
                                    :kanbanData="kanbanCards"
                                    :users="users"
                                    @reload="reloadKanban"
                                />
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
                                    <span class="me-1">{{$t("labelTeam")}}:</span>
                                    <b>{{ selectedOption.teamName }}</b>
                                </div>
                                <div>
                                    <span class="me-1">{{$t("labelTotalDocuments")}}</span>
                                    <b>{{ kanbanCards.numDocuments }}</b>
                                </div>
                            </div>
                        </div>
                    </div>
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
                    input: null,
                    isAllUsers: true,
                },
                users: []
            };
        },
        components: {
            WorkflowFilters,
            KanbanBoard,
        },
        methods: {
            getWorkflowByUser() {
                this.isLoadingKanban = true;
                var email = this.$store.state.userProfile.login;
                WorkflowService.getWorkflowList(email)
                    .then((response) => {
                        if(response.error !== undefined) {
                            this.$notify({
                                title: "workflow.index",
                                message: "workflow.error",
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        this.workflowList = response;
                        if(this.workflowList.length > 0) {
                            this.setSelectedWorkflow();
                        }
                    });
            },
            setSelectedWorkflow() {
                let workflowToSelect = this.workflowList[0];
                const redicteWorkflowId = this.$route.query.id;
                if(redicteWorkflowId !== undefined) {
                    const foundWorkflow = this.workflowList.find(w =>
                        w.id == redicteWorkflowId
                    );
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
                if (lastSelected && redicteWorkflowId === undefined) {
                    const foundWorkflow = this.workflowList.find(w =>
                        w.teams.id === lastSelected.teamId && w.id === lastSelected.id
                    );
                    if (foundWorkflow) {
                        return this.selectOption(lastSelected);
                    }
                }

                this.selectOption(workflowToSelect);
            },
            getWorkflowById(workflowId) {
                this.isLoadingKanban = true;
                WorkflowService.getWorkflowById(workflowId, this.filters)
                    .then((response) => {
                        this.kanbanCards = response;
                        console.log(response)
                    })
                    .finally(() => {
                        this.isLoadingKanban = false;
                    });
            },
            selectOption(workflow) {
                if(workflow.teams.length < 1) return;
                this.isLoaded = false;
                this.isLoadedUsers = false;
                this.selectedOption = {
                    id: workflow.id,
                    name: workflow.name,
                    teamName: workflow.teams[0].name,
                    teamId: workflow.teams[0].id,
                }
                this.$store.commit('setLastSelectedWorkflow', {
                    id: workflow.id,
                    name: workflow.name,
                    teamName: workflow.teams[0].name,
                    teamId: workflow.teams[0].id,
                });

                this.getWorkflowById(workflow.id);
            },
            reloadKanban() {                
                this.getWorkflowById(this.selectedOption.id);
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
                const step = this.kanbanCards.steps.find(s => s.id === message.stepId);
                if (!step?.cards) return;

                const item = step.cards.find(card => card.id === message.cardId);
                if (item) {
                    item.percentage = message.percentage;
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