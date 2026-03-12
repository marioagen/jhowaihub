<template>
    <div>
        <div class="card mb-2">
            <div class="card-body p-3">
                <div class="row align-items-center gap-3">
                    <div class="col-12">
                        <div class="flex flex-col items-start gap-3 flex-1 align-items-center">
                            <div>
                                <LucideIcon
                                    icon="Clock"
                                    :size="14"
                                    class="me-2"
                                />
                                <span>
                                    {{ $t("workflow.boardView") }}
                                </span>
                            </div>
                            <div class="dropdown">
                                <button
                                    class="btn btn-light border text-start d-flex justify-content-between align-items-center w-100 dropdown-toggle pe-1"
                                    type="button"
                                    data-bs-toggle="dropdown"
                                    aria-expanded="false"
                                >
                                    <div>
                                        <div class="fw-bold font-size-sm">
                                            {{ selectedOption.teamName }}
                                        </div>
                                        <div class="text-muted font-size-xs">
                                            {{ selectedOption.name }}
                                        </div>
                                    </div>
                                    <LucideIcon
                                        icon="ChevronDown"
                                        :size="20"
                                        class="ms-2"
                                    />
                                </button>
                                <ul class="dropdown-menu p-2 workflow-list">
                                    <li
                                        v-if="workflowList.length > 5"
                                        class="mb-1"
                                    >
                                        <div class="input-group input-group-sm">
                                            <span class="input-group-text p-1">
                                                <LucideIcon
                                                    icon="Search"
                                                    :size="16"
                                                    class="me-1"
                                                />
                                            </span>
                                            <input
                                                id="filter-workflow"
                                                v-model="workflowSearchText"
                                                type="text"
                                                name="filter"
                                                class="form-control"
                                                @input="searchWorkflow"
                                                @click.stop=""
                                            />
                                        </div>
                                    </li>
                                    <li
                                        v-for="item in filteredWorkflows"
                                        :key="item.id"
                                    >
                                        <a
                                            class="dropdown-item"
                                            @click="selectOption(item)"
                                        >
                                            <div class="fw-bold">
                                                {{ item.teams.name }}
                                            </div>
                                            <div class="text-muted small">
                                                {{ item.name }}
                                            </div>
                                        </a>
                                    </li>
                                </ul>
                            </div>
                            <div class="badge bg-secondary badge-custom">
                                <LucideIcon
                                    icon="Workflow"
                                    :size="14"
                                    class="me-2"
                                    stroke="#0d6efd"
                                />
                                <span>
                                    {{ selectedOption.name || $t("workflow.selectWorkflow") }}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 d-flex align-items-center">
                        <div class="row w-100 m-0">
                            <div class="col-10 p-0">
                                <WorkflowViewFilters @filter="filterData" />
                            </div>
                            <div class="col-2 pe-0 ps-4">
                                <button
                                    class="btn btn-primary new-doc-btn py-1 px-2"
                                    @click="redirectToNewUpload"
                                >
                                    <LucideIcon
                                        icon="Plus"
                                        :size="14"
                                    />
                                    {{ $t("documents.createBtn") }}
                                </button>
                            </div>
                        </div>
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
                            @cardMoved="handleCardMoved"
                            @cardUpdated="updateCard"
                            :isLoading="isLoadingKanban"
                            :cardIdsToUpdate="cardIdsToUpdate"
                        />
                    </div>
                </div>
            </div>
        </div>
        <div
            v-else
            class="text-center"
        >
            <span class="text-primary">
                {{ $t("workflow.notFound") }}
            </span>
        </div>
    </div>
</template>
<script>
    import signalRService from "@/services/signalR/signalRServices.js";
    import GlobalEventService from "@/services/globalEventService.js";
    import WorkflowService from "@/services/workflow/WorkflowService.js";
    import KanbanBoard from "@/components/documentsHub/workflows/kanban/KanbanBoard.vue";
    import WorkflowViewFilters from "@/components/workflow/WorkflowViewFilters.vue";
    import UserService from "@/services/users/UserService";
    import LogService from "@/services/log/logService";
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
                    orderBy: "created desc",
                    input: null,
                    login: null,
                    isAllUsers: true,
                },
                users: [],
                cardIdsToUpdate: [],
                updateCardsDebounceTimer: null,
            };
        },
        components: {
            LoadingComponent,
            WorkflowViewFilters,
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
                WorkflowService.getWorkflowList(email).then((response) => {
                    if (response.error !== undefined) {
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.error",
                            variant: "danger",
                            icon: "CircleX",
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
                    const foundWorkflow = this.workflowList.find((w) => w.id == redicteWorkflowId);
                    if (foundWorkflow) {
                        return this.selectOption(foundWorkflow);
                    } else {
                        return this.$notify({
                            title: "workflow.index",
                            message: "workflow.error",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                }

                const lastSelected = this.$store.state.lastSelectedWorkflow;
                if (lastSelected) {
                    const foundWorkflow = this.workflowList.find((w) => w.id === lastSelected.id);
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
                const teamIds = teams.map((t) => t.id);

                UserService.getUsersByTeamIds(teamIds)
                    .then((users) => {
                        this.users = users;
                    })
                    .catch((error) => {
                        LogService.showMessage("Error loading users:", error);
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

                this.$store.commit("setLastSelectedWorkflow", {
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
            handleCardMoved(cardMoveData) {
                this.updateCard({
                    card: cardMoveData.card,
                    currentStepOrder: cardMoveData.currentStepOrder,
                    newStepOrder: cardMoveData.nextStepOrder,
                });
            },
            updateCard(cardUpdateData) {
                const steps = Array.isArray(this.kanbanCards)
                    ? this.kanbanCards
                    : this.kanbanCards?.steps || [];

                if (steps.length === 0) return;

                const currentStep = steps.find((s) => s.order === cardUpdateData.currentStepOrder);
                if (!currentStep) {
                    LogService.showMessage("Could not find current step for card update");
                    return;
                }

                if (!currentStep.cards) {
                    currentStep.cards = [];
                }

                const cardIndex = currentStep.cards.findIndex(
                    (c) => c.id === cardUpdateData.card.id
                );
                if (cardIndex === -1) {
                    LogService.showMessage("Card not found in current step");
                    return;
                }

                const existingCard = currentStep.cards[cardIndex];
                const shouldMove =
                    cardUpdateData.newStepOrder !== undefined &&
                    cardUpdateData.newStepOrder !== cardUpdateData.currentStepOrder;
                if (shouldMove) {
                    const targetStep = steps.find((s) => s.order === cardUpdateData.newStepOrder);
                    if (!targetStep) {
                        LogService.showMessage("Could not find target step for card movement");
                        return;
                    }

                    const updatedCard = {
                        ...existingCard,
                        ...cardUpdateData.card,
                        stepId: targetStep.id,
                        status: targetStep.status
                            ? { ...targetStep.status }
                            : cardUpdateData.card.status || existingCard.status,
                    };

                    currentStep.cards.splice(cardIndex, 1);
                    if (!targetStep.cards) {
                        targetStep.cards = [];
                    }
                    targetStep.cards.push(updatedCard);
                } else {
                    const updatedCard = {
                        ...existingCard,
                        ...cardUpdateData.card,
                    };

                    currentStep.cards[cardIndex] = updatedCard;
                }

                if (Array.isArray(this.kanbanCards)) {
                    this.kanbanCards = [...steps];
                } else if (this.kanbanCards) {
                    this.kanbanCards = {
                        ...this.kanbanCards,
                        steps: [...steps],
                    };
                }
            },
            updateSpecificCards(cardIds, signalRMessage = null) {
                if (!cardIds || cardIds.length === 0) return;

                if (this.updateCardsDebounceTimer) {
                    clearTimeout(this.updateCardsDebounceTimer);
                }

                this.updateCardsDebounceTimer = setTimeout(() => {
                    const steps = Array.isArray(this.kanbanCards)
                        ? this.kanbanCards
                        : this.kanbanCards?.steps || [];

                    if (steps.length === 0) return;

                    cardIds.forEach((cardId) => {
                        let currentCard = null;
                        let currentStepOrder = null;

                        for (const step of steps) {
                            if (step.cards) {
                                const card = step.cards.find((c) => c.id === cardId);
                                if (card) {
                                    currentCard = card;
                                    currentStepOrder = step.order;
                                    break;
                                }
                            }
                        }

                        if (!currentCard) {
                            LogService.showMessage(`Card ${cardId} not found in current state`);
                            return;
                        }

                        let newStepOrder = currentStepOrder;
                        if (signalRMessage && signalRMessage.stepId) {
                            const targetStep = steps.find((s) => s.id === signalRMessage.stepId);
                            if (targetStep) {
                                newStepOrder = targetStep.order;
                            }
                        }

                        const updatedCardData = {
                            ...currentCard,
                        };

                        if (signalRMessage) {
                            if (signalRMessage.percentage !== undefined) {
                                updatedCardData.percentage = signalRMessage.percentage;
                            }
                            if (signalRMessage.toolName !== undefined) {
                                updatedCardData.toolName = signalRMessage.toolName;
                            }
                        }

                        this.updateCard({
                            card: updatedCardData,
                            currentStepOrder: currentStepOrder,
                            newStepOrder: newStepOrder,
                        });
                    });

                    setTimeout(() => {
                        this.cardIdsToUpdate = [];
                    }, 100);
                }, 300);
            },
            filterData(filters) {
                this.filters = filters;
                this.reloadKanban();
            },
            redirectToNewUpload() {
                this.$router.push({
                    name: "DocumentsUpload",
                });
            },
            searchWorkflow() {
                const searchText = this.workflowSearchText.toLowerCase();
                this.filteredWorkflows = this.workflowList.filter(
                    (o) =>
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
                const steps = Array.isArray(this.kanbanCards)
                    ? this.kanbanCards
                    : this.kanbanCards?.steps || [];

                if (steps.length === 0) return;

                let foundCard = null;
                let currentStepOrder = null;

                for (const step of steps) {
                    if (step.cards) {
                        const card = step.cards.find((c) => c.id === message.cardId);
                        if (card) {
                            foundCard = card;
                            currentStepOrder = step.order;
                            break;
                        }
                    }
                }

                if (!foundCard) {
                    LogService.showMessage(`Card ${message.cardId} not found in SignalR update`);
                    return;
                }

                const currentStep = steps.find((s) => s.order === currentStepOrder);
                const cardNeedsToMove = currentStep && currentStep.id !== message.stepId;

                foundCard.percentage = message.percentage;
                foundCard.toolName = message.toolName;

                if (cardNeedsToMove) {
                    if (!this.cardIdsToUpdate.includes(message.cardId)) {
                        this.cardIdsToUpdate.push(message.cardId);
                    }
                    this.updateSpecificCards([message.cardId], message);
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
        height: 100%;
        overflow: hidden;
        display: flex;
        flex-direction: column;
    }

    .kanban-wrapper {
        flex: 1;
        overflow-x: auto;
        overflow-y: hidden;
        min-height: 0;
        display: flex;
        align-items: stretch;
        -webkit-overflow-scrolling: touch;
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

    .new-doc-btn {
        font-size: 0.75rem;
        line-height: 1.2;
        width: 100%;
        height: 100%;
    }
    .border {
        border: 1px solid var(--color-border-form-control) !important;
    }
    .bg-secondary {
        background-color: var(--color-hover-transfer) !important;
        border-color: var(--color-hover-transfer) !important;
    }
</style>
