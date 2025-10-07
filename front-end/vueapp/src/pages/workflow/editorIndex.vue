<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("workflow.editTitle") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("workflow.subtitleEditor") }}</small>
                        </p>
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center gap-3">
                            <div class="d-flex align-items-center">
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
                                    <li v-for="item in workflowList" :key="item.id">
                                        <a class="dropdown-item" @click="selectOption(item)">
                                            <div class="fw-bold">{{ item.team.name }}</div>
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
                        <div class="d-flex align-items-center gap-2">
                            <button 
                                class="btn btn-outline-primary btn-sm" 
                                @click="redirectToForm"
                            >
                                <LucideIcon icon="Plus" :size="14" class="me-2" />
                                {{ $t("workflow.createBtn") }}
                            </button>
                            <button 
                                class="btn btn-primary btn-sm" 
                                @click="editWorkflow"
                            >
                                <LucideIcon icon="PenLine" :size="14" class="me-2" />
                                {{ $t("workflow.editBtn") }}
                            </button>
                        </div>
                    </div>
                </div>
                <div v-if="isLoaded" class="card mb-3 h-100">
                    <div class="card-body d-flex flex-column p-2 card-container">
                        <div class="kanban-wrapper">
                            <WorkflowCards 
                                :kanbanData="board"
                                :isEditor="true"
                            />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <FullscreenLoadingComponent 
            v-if="isDeleting"
        />
    </main>    
</template>

<script>
    import FullscreenLoadingComponent from "@/components/global/FullscreenLoadingComponent.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import WorkflowCards from "@/components/workflow/WorkflowCards.vue";

    export default {
        name: "WorkflorEditorIndex",
        components: {
            FullscreenLoadingComponent,
            WorkflowCards,
        },
        data() {
            return {
                isLoaded: false,
                crumbsData: [],
                entitySearch: {},
                isDeleting: false,
                workflowList: [],
                selectedOption: {
                    teamName: "TeamName",
                    name: "Nome"
                },
                board: [],
                changeLanguage: false,
            };
        },
        methods: {
            getWorkflowList() {
                this.workflowList = [];
                var email = this.$store.state.userProfile.login;
                WorkflowService.getWorkflowList(email)
                    .then((response) => {
                        if(response.error !== undefined) {
                            this.$notify({
                                title: 'Error',
                                message: 'Dados salvos com erro com sucesso!',
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        this.workflowList = response;
                        if(this.workflowList.length > 0) {
                            const lastSelected = this.$store.state.lastSelectedWorkflow;
                            let workflowToSelect = this.workflowList[0]; 

                            if (lastSelected) {
                                const foundWorkflow = this.workflowList.find(w => 
                                    w.team.id === lastSelected.teamId && w.id === lastSelected.id
                                );
                                if (foundWorkflow) {
                                    workflowToSelect = foundWorkflow;
                                }
                            }

                            this.selectOption(workflowToSelect);
                            this.filteredworkflows();
                        } else {
                            this.isLoaded = false;
                        }
                    });
            },
            selectOption(workflow) {
                this.selectedOption = {
                    id: workflow.id,
                    name: workflow.name,
                    teamName: workflow.team.name,
                    teamId: workflow.team.id,
                }
                
                this.$store.commit('setLastSelectedWorkflow', {
                    id: workflow.id,
                    name: workflow.name,
                    teamName: workflow.team.name,
                    teamId: workflow.team.id,
                });
                this.$store.commit('cleanTempWorkflow');
                this.getWorkflowbyTeam(workflow.team.id);
            },
            getWorkflowbyTeam(id) {
                this.isLoaded = false;
                WorkflowService.getWorkflowByTeamId(id)
                    .then((response) => {
                        this.board = response;
                    })
                    .finally(() => {
                        this.isLoaded = true;
                    });
            },
            filteredworkflows() {
                return this.workflowList.filter(
                    (workflow) => workflow.id !== this.selectedOption.id
                );
            },
            redirectToForm() {
                this.$router.push({ name: "NewWorkflow" });
            },
            editWorkflow() {
                this.$router.push({ 
                    name: "EditWorkflow", 
                    params: {
                        id: this.selectedOption.id,
                    }, 
                });
            },
        },
        created() {
            this.getWorkflowList();
        },
    };
</script>

<style scoped>
    .content-center {
        align-items: center;
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: center;
    }

    tbody {
        background-color: #fff !important;
    }

    .content-left-middle {
        text-align: left;
        vertical-align: middle;
        max-width: 200px;
    }

    .content-right-middle {
        text-align: right;
        vertical-align: middle;
    }

    .content-center-middle {
        text-align: center;
        vertical-align: middle;
    }

    .bg-success {
        background-color: #edfef2 !important;
        color: #0eaa42 !important;
        font-weight: inherit !important;
        padding: 8px 12px !important;
    }

    .container-fluid {
        padding: 0 13px;
    }

    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }

    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }
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