<template>
    <main>
        <div class="container-fluid mx-2">
            <div>
                <div class="d-flex justify-content-between align-items-center mb-1">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("workflow.title") }}</h5>
                        <p class="mb-1">
                            <small class="text-muted">{{ $t("workflow.subtitle") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="redirectToNewUpload">
                        <LucideIcon icon="Plus" :size="17" />
                        {{ $t("documents.createBtn") }}
                    </button>
                </div>
                <div class="card mb-2">
                    <div class="card-body p-3">
                        <div class="row">
                            <div class="col-6">
                                <div class="flex flex-col items-start gap-3 flex-1 align-items-center">
                                    <div>
                                        <LucideIcon icon="Clock" :size="14" class="me-2" />
                                        <span>{{ $t("workflow.boardView") }}</span>
                                    </div>
                                    <div class="dropdown">
                                        <button
                                            class="btn btn-light border text-start d-flex justify-content-between align-items-center w-100 dropdown-toggle pe-1"
                                            type="button" data-bs-toggle="dropdown" aria-expanded="false">
                                            <div>
                                                <div class="fw-bold font-size-sm">{{ selectedOption.teamName }}</div>
                                                <div class="text-muted font-size-xs">{{ selectedOption.name }}</div>
                                            </div>
                                            <LucideIcon icon="ChevronDown" :size="20" class="ms-2" />
                                        </button>
                                        <ul class="dropdown-menu p-2 workflow-list">
                                            <li v-if="workflowList.length > 5" class="mb-1">
                                                <div class="input-group input-group-sm">
                                                    <span class="input-group-text p-1">
                                                        <LucideIcon icon="Search" :size="16" class="me-1" />
                                                    </span>
                                                    <input id="filter-workflow" v-model="workflowSearchText" type="text"
                                                        name="filter" class="form-control" @input="searchWorkflow"
                                                        @click.stop="" />
                                                </div>
                                            </li>
                                            <li v-for="item in filteredWorkflows" :key="item.id">
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
                    <div class="card custom-height">
                        <div class="card-body d-flex flex-column p-2 card-container">
                            <div class="kanban-wrapper">
                                <KanbanBoard 
                                    ref="kanbanBoardRef"
                                    :kanbanData="kanbanCards" 
                                    :users="users" 
                                    @reload="reloadKanban" 
                                    :isLoading="isLoadingKanban"
                                    :cardIdsToUpdate="cardIdsToUpdate"
                                />
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
            workflowSearchText: "",
            filteredWorkflows: [],
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
            users: [],
            cardIdsToUpdate: [],
            updateCardsDebounceTimer: null
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
                    this.filteredWorkflows = response;
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
        updateSpecificCards(cardIds) {
            if (!cardIds || cardIds.length === 0) return;
            
            // Clear existing debounce timer
            if (this.updateCardsDebounceTimer) {
                clearTimeout(this.updateCardsDebounceTimer);
            }
            
            // Debounce the update to batch rapid changes
            this.updateCardsDebounceTimer = setTimeout(() => {
                // Update the cardIdsToUpdate prop to trigger KanbanBoard watch
                this.cardIdsToUpdate = [...cardIds];
                
                this.isLoadingKanban = true;
                WorkflowService.getWorkflowStepsById(this.selectedOption.id, this.filters)
                    .then((response) => {
                        // Handle both array and object with steps property
                        const currentSteps = Array.isArray(this.kanbanCards) 
                            ? this.kanbanCards 
                            : (this.kanbanCards?.steps || []);
                        const responseSteps = Array.isArray(response) 
                            ? response 
                            : (response?.steps || []);
                        
                        if (currentSteps.length > 0 && responseSteps.length > 0) {
                            // Update only the specific cards
                            cardIds.forEach(cardId => {
                                for (let i = 0; i < currentSteps.length; i++) {
                                    const step = currentSteps[i];
                                    if (step.cards) {
                                        const cardIndex = step.cards.findIndex(c => c.id === cardId);
                                        if (cardIndex !== -1) {
                                            // Find the updated card in the response
                                            for (let j = 0; j < responseSteps.length; j++) {
                                                const responseStep = responseSteps[j];
                                                if (responseStep.cards) {
                                                    const updatedCard = responseStep.cards.find(c => c.id === cardId);
                                                    if (updatedCard) {
                                                        // Replace the card with updated data using Vue reactivity
                                                        this.$set(step.cards, cardIndex, updatedCard);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                            
                            // Update kanbanCards to trigger reactivity
                            if (Array.isArray(this.kanbanCards)) {
                                this.kanbanCards = [...currentSteps];
                            } else if (this.kanbanCards) {
                                this.kanbanCards = { ...this.kanbanCards, steps: [...currentSteps] };
                            }
                        } else {
                            // If structure is different, update the whole thing
                            this.kanbanCards = response;
                        }
                    })
                    .finally(() => {
                        this.isLoadingKanban = false;
                        // Clear cardIdsToUpdate after a short delay to allow KanbanBoard to process
                        setTimeout(() => {
                            this.cardIdsToUpdate = [];
                        }, 100);
                    });
            }, 300); // 300ms debounce delay
        },
        filterData(filters) {
            this.filters = filters;
            this.reloadKanban();
        },
        redirectToNewUpload() {
            this.$router.push({ name: "DocumentsUpload" });
        },
        searchWorkflow() {
            const searchText = this.workflowSearchText.toLowerCase();
            this.filteredWorkflows = this.workflowList.filter(o =>
                (o.name && o.name.toLowerCase().includes(searchText)) ||
                (o.teams && o.teams.name && o.teams.name.toLowerCase().includes(searchText))
            );
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
            console.log(message);
            // Handle both array and object with steps property
            const steps = Array.isArray(this.kanbanCards) 
                ? this.kanbanCards 
                : (this.kanbanCards?.steps || []);
            
            if (steps.length === 0) return;
            let foundCard = null;

            for (let i = 0; i < steps.length; i++) {
                const step = steps[i];
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
                // Instead of reloading everything, collect card ID and update only that card
                if (!this.cardIdsToUpdate.includes(message.cardId)) {
                    this.cardIdsToUpdate.push(message.cardId);
                }
                this.updateSpecificCards([message.cardId]);
            }
        });
    },
    beforeUnmount() {
        if (this.updateCardsDebounceTimer) {
            clearTimeout(this.updateCardsDebounceTimer);
        }
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
    max-height: 75vh;
    overflow-y: auto;
}

.kanban-wrapper {
    overflow-x: auto;
    white-space: nowrap;
}

.workflow-list {
    min-width: 100%;
    max-height: 300px;
    overflow-y: auto;
}

.dropdown-toggle::after {
    display: none;
}

.custom-height {
    height: calc(100vh - 230px);
}
</style>