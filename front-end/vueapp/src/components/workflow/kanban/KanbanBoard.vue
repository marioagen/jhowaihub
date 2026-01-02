<template>
    <div class="container mt-2">
        <div class="d-flex flex-nowrap">
            <div class="col-3 kanban-col me-3" v-for="step in stepsList" :key="step.id">
                <div class="card flex-grow-1">
                    <div class="card-header" :class="findOrder(step.order)">
                        {{ step.name }}
                    </div>
                    <div v-if="isEditor">
                        <div class="card-body">
                            <div class="d-flex justify-content-center mb-3">
                                <div class="rounded-circle bg-light d-flex align-items-center justify-content-center">
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
                    <div v-else>
                        <div v-for="card in step.cards" :key="card.id" class="card-body">
                            <KanbanCard :dataCard="card" :dataStep="step" :isFirstStep="step.order === minOrder"
                                :isLastStep="step.order === maxOrder" @reload="reloadList" label="labelAnalyze"
                                :users="users" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import KanbanCard from "@/components/workflow/kanban/KanbanCard.vue";
export default {
    name: "KanbanBoard",
    components: {
        KanbanCard
    },
    props: {
        kanbanData: {
            type: [Array, Object],
            required: true,
        },
        users: {
            type: Array,
            required: false,
            default: () => []
        },
        isEditor: {
            type: Boolean,
            required: false,
            default: false,
        },
    },
    watch: {
        kanbanData() {
            this.setCard();
        },
    },
    data: () => ({
        firstStep: false,
        lastStep: false,
        customClass: "",
        stepsList: [],
    }),
    computed: {
        minOrder() {
            return Math.min(...this.kanbanData.map(s => s.order));
        },
        maxOrder() {
            return Math.max(...this.kanbanData.map(s => s.order));
        }
    },
    methods: {
        findOrder(stepOrder) {
            const minOrder = Math.min(...this.kanbanData.map(s => s.order));
            const maxOrder = Math.max(...this.kanbanData.map(s => s.order));

            if (stepOrder === minOrder) return "first-steps";
            if (stepOrder === maxOrder) return "last-step";
            return "middle-step";
        },
        reloadList() {
            this.$emit('reload');
        },
        setCard() {
            this.stepsList = this.kanbanData;
        },
    },
    mounted() {
        this.setCard();
    }
};
</script>

<style scoped>
.first-steps {
    background-color: #dbe9fc;
}

.last-step {
    background-color: #dcfce7;
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

@media (min-width: 768px) and (max-width: 991.98px) {
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
    --kanban-card-gap: 0.25rem;  /* Spacing between cards - adjust this value */
    padding-top: var(--kanban-card-gap) !important;
    padding-bottom: var(--kanban-card-gap) !important;
    padding-left: 0.5rem;
    padding-right: 0.5rem;
}
</style>
