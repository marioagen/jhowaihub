<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("labelWorkflowBoard") }}</h5>
                        <p>
                            <small class="text-muted">{{$t("labelWorkflowSubTitle")}}</small>
                        </p>
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="flex flex-col items-start gap-4 flex-1 align-items-center">
                            <div>
                                <LucideIcon icon="Clock" size="14" class="me-2" />
                                <span>{{$t("labelWatchingWorkflow")}}</span>
                            </div>
                            <div class="dropdown">
                                <button class="btn btn-light border text-start"
                                        type="button"
                                        data-bs-toggle="dropdown"
                                        aria-expanded="false">
                                    <div class="fw-bold font-size-sm">{{selectedOption.teamName}}</div>
                                    <div class="text-muted font-size-xs">{{selectedOption.name}}</div>
                                </button>

                                <ul class="dropdown-menu">
                                    <li v-for="item in workflowList" :key="item.id">
                                        <a class="dropdown-item" @click="selectOption(item)">
                                            <div class="fw-bold">{{item.team.name}}</div>
                                            <div class="text-muted small">{{item.name}}</div>
                                        </a>
                                    </li>
                                </ul>
                            </div>
                            <div class="badge bg-secondary badge-custom">
                                <LucideIcon icon="Workflow" size="14" class="me-2" stroke="#0d6efd" />
                                <span>{{selectedOption.name}}</span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="card mb-3 h-100">
                    <div class="card-body d-flex flex-column p-2 card-container">
                        <div class="kanban-wrapper">
                            <WorkflowCards :kanbanData="kanbanCards"
                                           @reload="reloadKanban">
                            </WorkflowCards>
                        </div>
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="flex flex-col items-start gap-4 flex-1 align-items-center">
                            <div>
                                <span class="me-1">Workflow:</span>
                                <b>{{selectedOption.name}}</b>
                            </div>
                            <div>
                                <span class="me-1">{{$t("labelTeam")}}:</span>
                                <b>{{selectedOption.teamName}}</b>
                            </div>
                            <div>
                                <span class="me-1">{{$t("labelTotalDocuments")}}</span>
                                <b>{{numDocs}}</b>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
</main>
</template>

<script>
    import WorkflowService from "@/services/workflow/WorkflowService.js";
    import WorkflowCards from "@/components/workflow/WorkFlowCards.vue";
    export default {
        name: "WorkflowDocuments",
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
                modalQuestion: {
                    name: "",
                },
                workflowList: [],
                selectedOption: {
                    name: "",
                    teamName: "",
                    teamId: 0,
                },
                kanbanCards: [],
                numDocs: 0
            };
        },
        components:
        {
            WorkflowCards
        },
        watch: {
            "$store.state.userProfile.language": function () {
                this.setEntitySearch();
            },
        },
        methods: {
            getWorkflows() {
                WorkflowService.getWorkflows()
                    .then((response) => {
                        console.log(response);
                        this.workflowList = response;
                    })
                    .finally(() => {
                        this.selectOption(this.workflowList[0]);
                        this.filteredworkflows();
                    });
            },
            getWorkflowbyTeam(id) {
                WorkflowService.getWorkbyTeamId(id)
                    .then((response) => {
                        console.log(response);
                        this.kanbanCards = response;
                    })
                    .finally(() => {
                    });
            },
            filteredworkflows() {
                return this.workflowList.filter(
                    (workflow) => workflow.id !== this.selectedOption.id
                );
            },
            selectOption(workflow) {
                this.selectedOption = {
                    name: workflow.name,
                    teamName: workflow.team.name,
                    teamId: workflow.team.id
                }
                this.getWorkflowbyTeam(workflow.team.id);
            },
            reloadKanban() {
                this.getWorkflowbyTeam(this.selectedOption.teamId);
            },
        },
        created() {
            this.getWorkflows();
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
        white-space: nowrap; /* impede quebra de linha */
    }

</style>