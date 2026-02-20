<template>
    <div class="kanban-board-container">
        <div class="d-flex flex-nowrap">
            <div
                class="col-3 kanban-col me-3"
                v-for="step in stepsList"
                :key="step.id"
            >
                <div class="card flex-grow-1 kanban-column-card">
                    <div
                        class="card-header d-flex justify-content-between align-items-center"
                        :class="findOrder(step.order)"
                    >
                        <span>
                            {{ step.name }}
                            <span class="badge">
                                {{ toFinalizeCardLength(step.cards) }}
                            </span>
                        </span>
                        <div
                            v-if="step.order === maxOrder"
                            class="cursor-pointer"
                            @click="toggleLastColumnVisibility"
                        >
                            <LucideIcon
                                :icon="isLastColumnVisible ? 'Eye' : 'EyeOff'"
                                size="16"
                            />
                        </div>
                    </div>
                    <div v-if="isEditor">
                        <div class="card-body">
                            <div class="d-flex justify-content-center mb-3">
                                <div
                                    class="rounded-circle bg-light d-flex align-items-center justify-content-center"
                                >
                                    <LucideIcon icon="Workflow" />
                                </div>
                            </div>

                            <h6 class="card-title">
                                {{ $t("workflow.stepTitle") }}
                            </h6>
                            <p class="card-text text-muted small xsm-text">
                                {{ $t("workflow.stepSubtitle") }}
                            </p>
                        </div>
                    </div>
                    <div
                        v-else
                        class="kanban-column-body"
                    >
                        <div v-show="step.order !== maxOrder || isLastColumnVisible">
                            <div
                                v-for="card in step.cards"
                                :key="card.id"
                                class="card-body"
                                :id="card.id"
                            >
                                <KanbanCard
                                    v-if="showFinalized(card.status.id, step)"
                                    :dataCard="card"
                                    :dataStep="step"
                                    :isFirstStep="step.order === minOrder"
                                    :isLoading="isLoading"
                                    :isLastStep="isLastStep(step)"
                                    @reload="reloadList"
                                    @cardMoved="handleCardMoved"
                                    @cardUpdated="handleCardUpdated"
                                    label="common.analyze"
                                    :users="users"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    import KanbanCard from "@/components/documentsHub/workflows/kanban/KanbanCard.vue";
    export default {
        name: "KanbanBoard",
        components: {
            KanbanCard,
        },
        props: {
            kanbanData: {
                type: [Array, Object],
                required: true,
            },
            users: {
                type: Array,
                required: false,
                default: () => [],
            },
            isEditor: {
                type: Boolean,
                required: false,
                default: false,
            },
            cardIdsToUpdate: {
                type: Array,
                required: false,
                default: () => [],
            },
        },
        watch: {
            kanbanData() {
                this.setCard();
            },
            cardIdsToUpdate(newCardIds) {
                if (newCardIds && newCardIds.length > 0) {
                    this.updateCards(newCardIds);
                }
            },
        },
        data: () => ({
            firstStep: false,
            lastStep: false,
            customClass: "",
            stepsList: [],
            isLastColumnVisible: true,
        }),
        computed: {
            minOrder() {
                return Math.min(...this.kanbanData.map((s) => s.order));
            },
            maxOrder() {
                return Math.max(...this.kanbanData.map((s) => s.order));
            },
        },
        methods: {
            findOrder(stepOrder) {
                const minOrder = Math.min(...this.kanbanData.map((s) => s.order));
                const maxOrder = Math.max(...this.kanbanData.map((s) => s.order));

                if (stepOrder === minOrder) return "first-steps";
                if (stepOrder === maxOrder) return "last-step";
                return "middle-step";
            },
            reloadList() {
                this.$emit("reload");
            },
            handleCardMoved(cardMoveData) {
                this.$emit("cardMoved", cardMoveData);
            },
            handleCardUpdated(cardUpdateData) {
                this.$emit("cardUpdated", cardUpdateData);
            },
            setCard() {
                this.stepsList = this.kanbanData;
            },
            updateCards(cardIds) {
                if (!cardIds || cardIds.length === 0) return;
                cardIds.forEach((cardId) => {
                    const cardElement = document.getElementById(cardId);
                    if (cardElement) {
                        cardElement.remove();
                    }
                });
            },
            toggleLastColumnVisibility() {
                this.isLastColumnVisible = !this.isLastColumnVisible;
                localStorage.setItem("kanban_last_column_visibility", this.isLastColumnVisible);
            },
            isLastStep(step) {
                return step.order === this.maxOrder;
            },
            showFinalized(id, step) {
                if (id == 6) return false;
                if (this.isLastStep(step)) return this.isLastColumnVisible;
                return true;
            },
            toFinalizeCardLength(cards) {
                return cards.filter((card) => card.status.id !== 6).length;
            },
        },
        mounted() {
            this.setCard();
            const savedVisibility = localStorage.getItem("kanban_last_column_visibility");
            if (savedVisibility !== null) {
                this.isLastColumnVisible = savedVisibility === "true";
            }
        },
    };
</script>
<style scoped>
    .kanban-board-container {
        height: 100%;
        width: 100%;
        padding: 0.5rem 0;
        overflow: visible;
    }

    .kanban-board-container > .d-flex {
        height: 100%;
        align-items: stretch;
    }

    .kanban-col {
        height: 100%;
        display: flex;
        flex-direction: column;
        flex-shrink: 0;
        min-width: 0;
    }

    .kanban-column-card {
        height: 100%;
        display: flex;
        flex-direction: column;
        min-width: 0;
    }

    .kanban-column-body {
        flex: 1;
        overflow-y: auto;
        overflow-x: hidden;
        min-height: 0;
        -webkit-overflow-scrolling: touch;
    }

    .first-steps {
        background-color: #dbe9fc;
    }

    .first-steps > span > span {
        background-color: #a7bad3 !important;
    }

    .last-step {
        background-color: #dcfce7;
    }

    .last-step > span > span {
        background-color: #a1cfb1 !important;
    }
    .bg-primary {
        background-color: #dbeafe !important;
        color: #2b7fff !important;
    }

    .bg-primary {
        background-color: #dbeafe !important;
        color: #2b7fff !important;
    }

    .bg-warning {
        background-color: #fef9c2 !important;
        color: #a65f00 !important;
    }

    .bg-danger {
        background-color: #ffedd4 !important;
        color: #ca3500 !important;
    }

    .bg-success {
        background-color: #d0fae5 !important;
        color: #007a55 !important;
    }

    @media (min-width: 768px) and (max-width: 1024px) {
        .kanban-col {
            width: 100% !important;
            display: block !important;
        }
    }

    .xsm-text {
        font-size: 0.55rem;
    }

    /* Reduce spacing between kanban cards - adjust this value to make cards more/less compact */
    .kanban-col .card-body {
        --kanban-card-gap: 0.25rem; /* Spacing between cards - adjust this value */
        padding-top: var(--kanban-card-gap) !important;
        padding-bottom: var(--kanban-card-gap) !important;
        padding-left: 0.5rem;
        padding-right: 0.5rem;
    }

    /* Ensure step names wrap properly within card-header boundaries */
    .card-header {
        word-wrap: break-word;
        overflow-wrap: break-word;
        white-space: normal;
        hyphens: auto;
        flex-shrink: 0;
    }

    .badge {
        color: unset;
        background-color: #ebebeb;
    }
</style>
