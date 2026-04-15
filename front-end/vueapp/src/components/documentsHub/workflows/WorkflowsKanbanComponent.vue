<template>
    <div>
        <div class="card mb-2">
            <div class="card-body p-3">
                <div class="row align-items-center gap-3">
                    <div class="col-12">
                        <div class="flex flex-col items-start gap-3 flex-1 align-items-center">
                            <div class="d-flex align-items-center gap-2 flex-wrap">
                                <div class="d-flex align-items-center">
                                    <LucideIcon
                                        icon="Clock"
                                        :size="14"
                                        class="me-2"
                                    />
                                    <span>
                                        {{ $t("workflow.boardView") }}
                                    </span>
                                </div>
                            </div>
                            <div class="dropdown">
                                <button
                                    class="btn btn-light border text-start d-flex justify-content-between align-items-center w-100 dropdown-toggle pe-1"
                                    type="button"
                                    data-bs-toggle="dropdown"
                                    aria-expanded="false"
                                >
                                    <LucideIcon
                                        icon="Workflow"
                                        :size="14"
                                        class="me-2"
                                        stroke="#0d6efd"
                                    />
                                    <div>
                                        <div class="fw-bold font-size-sm">
                                            {{ selectedOption.teamName }}
                                        </div>
                                        <div class="font-size-xs">
                                            {{ selectedOption.name }}
                                        </div>
                                    </div>
                                    <LucideIcon
                                        icon="ChevronDown"
                                        :size="20"
                                        class="ms-2 text-muted"
                                    />
                                    <LucideIcon
                                        icon="ChevronDown"
                                        :size="20"
                                        class="ms-2 text-muted"
                                    />
                                </button>
                                <ul class="dropdown-menu p-2 workflow-list">
                                    <li class="mb-1">
                                        <div class="input-group input-group-sm">
                                            <span class="input-group-text p-1">
                                                <LucideIcon
                                                    icon="Search"
                                                    :size="16"
                                                    class="me-1"
                                                />
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
                            <button
                                v-if="canManageWorkflow"
                                type="button"
                                class="btn btn-primary borderless p-0 d-inline-flex align-items-center"
                                :disabled="!selectedOption.id || isLoadingKanban"
                                v-tooltip="$t('workflow.editWorkflowBoard')"
                                :aria-label="$t('workflow.editWorkflowBoard')"
                                @click="redirectToWorkflowEditPage"
                            >
                                <LucideIcon
                                    icon="SquarePen"
                                    :size="20"
                                />
                            </button>
                        </div>
                    </div>
                    <div class="col-12">
                        <div class="row w-100 m-0 g-2 align-items-center">
                            <div class="col-9 p-0">
                                <WorkflowViewFilters @filter="filterData" />
                            </div>
                            <div class="col-1 d-flex justify-content-center align-items-center p-0">
                                <div
                                    class="btn-group btn-group-sm"
                                    role="group"
                                    :aria-label="$t('workflow.viewModeGroupLabel')"
                                >
                                    <button
                                        type="button"
                                        class="btn d-inline-flex align-items-center justify-content-center"
                                        :class="
                                            isKanbanView ? 'btn-primary' : 'btn-outline-primary'
                                        "
                                        :aria-pressed="isKanbanView"
                                        v-tooltip="$t('workflow.viewModeBoard')"
                                        :aria-label="$t('workflow.viewModeBoard')"
                                        @click="onSelectKanbanView"
                                    >
                                        <LucideIcon
                                            icon="SquareKanban"
                                            :size="16"
                                        />
                                    </button>
                                    <button
                                        type="button"
                                        class="btn d-inline-flex align-items-center justify-content-center"
                                        :class="
                                            !isKanbanView ? 'btn-primary' : 'btn-outline-primary'
                                        "
                                        :aria-pressed="!isKanbanView"
                                        v-tooltip="$t('workflow.viewModeList')"
                                        :aria-label="$t('workflow.viewModeList')"
                                        @click="onSelectListView"
                                    >
                                        <LucideIcon
                                            icon="Rows4"
                                            :size="16"
                                        />
                                    </button>
                                </div>
                            </div>
                            <div class="col-2 d-flex align-items-center justify-content-end pe-0">
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
                        <div
                            v-if="showMassiveBtns"
                            class="w-100 mt-2 pt-2 border-top"
                        >
                            <div
                                class="d-flex align-items-center justify-content-between flex-wrap gap-2"
                            >
                                <div
                                    class="d-flex align-items-center gap-2 workflow-bulk-selection-meta"
                                >
                                    <span
                                        class="badge rounded-pill bg-primary px-2 py-1 d-inline-flex align-items-center justify-content-center"
                                    >
                                        {{ selectedCardIds.length }}
                                    </span>
                                    <span class="text-secondary small mb-0">
                                        {{ $t("workflow.bulk.selectedDocuments") }}
                                    </span>
                                </div>
                                <div class="d-flex align-items-center gap-2 flex-wrap">
                                    <div
                                        v-if="canBulkAssign"
                                        class="dropdown"
                                    >
                                        <button
                                            type="button"
                                            class="btn btn-primary btn-sm d-inline-flex align-items-center gap-1 dropdown-toggle"
                                            data-bs-toggle="dropdown"
                                            data-bs-auto-close="true"
                                            aria-expanded="false"
                                            :disabled="isBulkAssigning"
                                            :aria-label="$t('workflow.bulk.assign')"
                                        >
                                            <LucideIcon
                                                v-if="!isBulkAssigning"
                                                icon="UserPlus"
                                                :size="16"
                                            />
                                            <LucideIcon
                                                v-else
                                                icon="Loader"
                                                :size="16"
                                                class="animate-spin"
                                            />
                                            {{ $t("workflow.bulk.assign") }}
                                        </button>
                                        <ul
                                            class="dropdown-menu dropdown-menu-end shadow-sm p-2 bulk-assign-users-list"
                                            style="
                                                min-width: 12rem;
                                                max-height: 20rem;
                                                overflow-y: auto;
                                            "
                                        >
                                            <li
                                                v-if="users.length > 5"
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
                                                        v-model="bulkAssignUserSearchText"
                                                        type="search"
                                                        class="form-control"
                                                        autocomplete="off"
                                                        @click.stop=""
                                                    />
                                                </div>
                                            </li>
                                            <li
                                                v-for="user in filteredBulkAssignUsers"
                                                :key="user.id"
                                            >
                                                <button
                                                    type="button"
                                                    class="dropdown-item text-start"
                                                    :disabled="isBulkAssigning"
                                                    @click="assignRangeToUser(user.id)"
                                                >
                                                    {{ user.name }}
                                                </button>
                                            </li>
                                        </ul>
                                    </div>
                                    <button
                                        v-if="canBulkReject"
                                        type="button"
                                        class="btn btn-danger btn-sm d-inline-flex align-items-center gap-1"
                                        @click="rejectRange"
                                    >
                                        <LucideIcon
                                            icon="XCircle"
                                            :size="16"
                                        />
                                        {{ $t("analyze.rejection.reject") }}
                                    </button>
                                </div>
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
                            v-if="isKanbanView"
                            :kanbanData="kanbanCards"
                            :users="users"
                            :isLoading="isLoadingKanban"
                            :cardIdsToUpdate="cardIdsToUpdate"
                            @reload="reloadKanban"
                            @cardMoved="handleCardMoved"
                            @cardUpdated="updateCard"
                            ref="kanbanBoardRef"
                        />
                        <WorkflowAccordionComponent
                            v-else
                            :data="kanbanCards"
                            :users="users"
                            :selected-card-ids="selectedCardIds"
                            @reload="reloadKanban"
                            @cardUpdated="updateCard"
                            @cardMoved="handleCardMoved"
                            @toggle-card-selection="onToggleAccordionCardSelection"
                            @toggle-step-selection="onToggleAccordionStepSelection"
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
        <DocumentRejectionModal
            v-if="canBulkReject"
            ref="documentRejectionModalRef"
            :card-ids="selectedCardIds"
            @success="onBulkRejectSuccess"
        />
    </div>
</template>
<script>
    import { hasPermission } from "@/utils/permissions";
    import PermissionGroups from "@/constants/PermissionGroups";
    import PermissionNames from "@/constants/PermissionNames";
    import signalRService from "@/services/signalR/signalRServices.js";
    import GlobalEventService from "@/services/globalEventService.js";
    import WorkflowService from "@/services/workflow/WorkflowService.js";
    import KanbanBoard from "@/components/documentsHub/workflows/kanban/KanbanBoard.vue";
    import WorkflowViewFilters from "@/components/workflow/WorkflowViewFilters.vue";
    import UserService from "@/services/users/UserService";
    import LogService from "@/services/log/logService";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import WorkflowAccordionComponent from "@/components/documentsHub/workflows/accordion/WorkflowAccordionComponent.vue";
    import DocumentRejectionModal from "@/components/analyze/modals/DocumentRejectionModal.vue";
    import CardsServices from "@/services/cards/CardsServices";
    export default {
        name: "WorkflowPage",
        data() {
            return {
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
                isKanbanView: true,
                selectedCardIds: [],
                isBulkAssigning: false,
                bulkAssignUserSearchText: "",
            };
        },
        components: {
            LoadingComponent,
            WorkflowViewFilters,
            KanbanBoard,
            WorkflowAccordionComponent,
            DocumentRejectionModal,
        },
        computed: {
            hasList() {
                return this.workflowList.length > 0;
            },
            canManageWorkflow() {
                return hasPermission("WorkflowManagement", "View");
            },
            showMassiveBtns() {
                return !this.isKanbanView && this.selectedCardIds.length > 0;
            },
            canBulkAssign() {
                return this.users.length > 0;
            },
            canBulkReject() {
                return hasPermission(PermissionGroups.Documents, PermissionNames.Reject);
            },
            filteredBulkAssignUsers() {
                const q = (this.bulkAssignUserSearchText || "").toLowerCase().trim();
                if (!q) {
                    return this.users;
                }
                return this.users.filter((u) => u.name && u.name.toLowerCase().includes(q));
            },
        },
        methods: {
            clearBulkSelection() {
                this.selectedCardIds = [];
                this.bulkAssignUserSearchText = "";
            },
            assignRangeErrorMessage(error) {
                const data = error?.response?.data;
                if (typeof data === "string" && data) {
                    return data;
                }
                if (data?.labelError) {
                    return data.labelError;
                }
                if (data?.message) {
                    return data.message;
                }
                return this.$t("workflow.bulk.assignError");
            },
            async assignRangeToUser(userId) {
                if (!this.canBulkAssign || this.selectedCardIds.length === 0 || !userId) {
                    return;
                }
                const cardIds = [...new Set(this.selectedCardIds)];
                const params = {
                    UserId: userId,
                    CardIds: cardIds,
                };
                this.isBulkAssigning = true;
                try {
                    const response = await CardsServices.assignRange(params);
                    if (response?.error !== undefined) {
                        this.$notify({
                            title: this.$t("common.error"),
                            message: this.assignRangeErrorMessage(response.error),
                            variant: "danger",
                            icon: "CircleX",
                        });
                        return;
                    }
                    this.$notify({
                        title: this.$t("common.success"),
                        message: this.$t("workflow.bulk.assignSuccess"),
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                    this.reloadKanban();
                    this.clearBulkSelection();
                } catch (e) {
                    this.$notify({
                        title: this.$t("common.error"),
                        message: this.$t("workflow.bulk.assignError"),
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isBulkAssigning = false;
                }
            },
            rejectRange() {
                if (!this.canBulkReject || this.selectedCardIds.length === 0) {
                    return;
                }
                this.$refs.documentRejectionModalRef?.open(this.selectedOption?.id);
            },
            onBulkRejectSuccess() {
                this.reloadKanban();
                this.clearBulkSelection();
            },
            onSelectKanbanView() {
                this.isKanbanView = true;
                this.clearBulkSelection();
            },
            onSelectListView() {
                this.isKanbanView = false;
            },
            onToggleAccordionCardSelection({ cardId, selected }) {
                const set = new Set(this.selectedCardIds);
                if (selected) {
                    set.add(cardId);
                } else {
                    set.delete(cardId);
                }
                this.selectedCardIds = Array.from(set);
            },
            onToggleAccordionStepSelection({ cardIds, selectAll }) {
                const set = new Set(this.selectedCardIds);
                if (selectAll) {
                    cardIds.forEach((id) => set.add(id));
                } else {
                    cardIds.forEach((id) => set.delete(id));
                }
                this.selectedCardIds = Array.from(set);
            },
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
                    return;
                }

                const teamIds = teams.map((t) => t.id);

                UserService.getUsersByTeamIds(teamIds)
                    .then((users) => {
                        this.users = users;
                    })
                    .catch((error) => {
                        LogService.showMessage("Error loading users:", error);
                        this.users = [];
                    });
            },
            selectOption(workflow) {
                if (!workflow?.id) return;

                this.$store.commit("setLastSelectedWorkflow", {
                    id: workflow.id,
                    name: workflow.name,
                });

                this.selectedOption = {
                    id: workflow.id,
                    name: workflow.name,
                };

                this.clearBulkSelection();
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
                this.clearBulkSelection();
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
            redirectToWorkflowEditPage() {
                if (!this.canManageWorkflow || !this.selectedOption.id) {
                    return;
                }

                this.$router.push({
                    name: "EditWorkflow",
                    params: {
                        id: this.selectedOption.id,
                    },
                    query: { from: "kanban" },
                });
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

                if (message.failed === true) {
                    foundCard.status.name = "Fail";
                    foundCard.status.color = "#D10000";

                    if (message.labelError) {
                        this.$notify({
                            title: this.$t("common.error"),
                            message: this.$t(message.labelError),
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                }

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
            GlobalEventService.off("all-uploads-complete", this.getWorkflowByUser);
            GlobalEventService.off("refresh-once", this.getWorkflowByUser);
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

    .workflow-bulk-selection-meta {
        min-height: 2rem;
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

    .borderless {
        background-color: transparent !important;
        border: none !important;
        color: var(--bs-primary) !important;
        line-height: 1;
        box-shadow: none !important;
    }

    .borderless:hover:not(:disabled) {
        background-color: transparent !important;
        border: none !important;
        color: var(--bs-primary) !important;
        filter: brightness(0.92);
        cursor: pointer;
    }

    .borderless:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .borderless:focus-visible {
        outline: 2px solid var(--bs-primary);
        outline-offset: 2px;
    }

    .animate-spin {
        animation: bulk-assign-spin 1s linear infinite;
    }

    @keyframes bulk-assign-spin {
        100% {
            transform: rotate(360deg);
        }
    }
</style>
