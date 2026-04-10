<template>
    <div class="workflow-accordion w-100">
        <div
            v-if="steps.length === 0"
            class="text-muted small py-2"
        >
            {{ $t("workflow.noStepsAvailable") }}
        </div>
        <AccordionComponent
            v-else
            class="workflow-accordion-inner w-100"
            accordion-id="workflow-kanban-accordion"
        >
            <AccordionItem
                v-for="step in steps"
                :key="step.id"
                :item-id="`step-${step.id}`"
                :item-class="headerClassForStep(step)"
            >
                <template #header>
                    <span class="workflow-accordion-step-title">
                        {{ step.name }}
                        <span class="badge">{{ cardCount(step) }}</span>
                    </span>
                </template>
                <p
                    v-if="cardCount(step) === 0"
                    class="text-muted small mb-0"
                >
                    {{ $t("workflow.noCardsForStep") }}
                </p>
                <ul
                    v-else
                    class="list-unstyled mb-0"
                >
                    <li
                        v-for="card in cardsForStep(step)"
                        :key="card.id"
                    >
                        {{ displayCardLabel(card) }}
                    </li>
                </ul>
            </AccordionItem>
        </AccordionComponent>
    </div>
</template>
<script>
    import AccordionComponent, { AccordionItem } from "@/components/global/AccordionComponent.vue";

    export default {
        name: "WorkflowAccordionComponent",
        components: {
            AccordionComponent,
            AccordionItem,
        },
        props: {
            data: {
                type: [Array, Object],
                required: true,
            },
        },
        computed: {
            steps() {
                const raw = this.data;
                if (!raw) return [];
                if (Array.isArray(raw)) return raw;
                return raw.steps ?? [];
            },
            minOrder() {
                if (this.steps.length === 0) return 0;
                return Math.min(...this.steps.map((s) => s.order));
            },
            maxOrder() {
                if (this.steps.length === 0) return 0;
                return Math.max(...this.steps.map((s) => s.order));
            },
        },
        methods: {
            headerClassForStep(step) {
                return this.findOrder(step.order);
            },
            findOrder(stepOrder) {
                if (this.steps.length === 0) return "middle-step";
                if (stepOrder === this.minOrder) return "first-steps";
                if (stepOrder === this.maxOrder) return "last-step";
                return "middle-step";
            },
            cardsForStep(step) {
                return step.cards ?? [];
            },
            cardCount(step) {
                return this.cardsForStep(step).length;
            },
            displayCardLabel(card) {
                if (card.name && String(card.name).trim()) {
                    return card.name;
                }
                if (card.id != null) {
                    return String(card.id);
                }
                return "—";
            },
        },
    };
</script>
<style scoped>
    .workflow-accordion {
        width: 100%;
        min-width: 0;
        align-self: stretch;
    }

    .workflow-accordion-inner {
        width: 100%;
        min-width: 0;
    }

    .workflow-accordion :deep(.accordion-item) {
        margin-bottom: 0.375rem;
    }

    .workflow-accordion :deep(.accordion-item:last-of-type) {
        margin-bottom: 0;
    }

    /* Match Kanban column header typography (KanbanBoard .card-header: body-sized title text) */
    .workflow-accordion :deep(.accordion-header .accordion-button) {
        word-wrap: break-word;
        overflow-wrap: break-word;
        white-space: normal;
        hyphens: auto;
        flex-shrink: 0;
        font-family: inherit;
        font-size: 1rem;
        font-weight: 400;
        line-height: 1.5;
    }

    .workflow-accordion :deep(.workflow-accordion-step-title) {
        display: inline;
        text-align: start;
    }

    /* Kanban column header backgrounds on accordion trigger (KanbanBoard .first-steps / .last-step) */
    .workflow-accordion :deep(.accordion-item.first-steps .accordion-button) {
        background-color: var(--color-bg-kanban-primary) !important;
        color: #212529 !important;
        box-shadow: none !important;
    }

    .workflow-accordion :deep(.accordion-item.first-steps .workflow-accordion-step-title > .badge) {
        background-color: var(--color-bg-kanban-primary-accent) !important;
        color: unset;
    }

    .workflow-accordion :deep(.accordion-item.last-step .accordion-button) {
        background-color: var(--color-bg-kanban-success) !important;
        color: #212529 !important;
        box-shadow: none !important;
    }

    .workflow-accordion :deep(.accordion-item.last-step .workflow-accordion-step-title > .badge) {
        background-color: var(--color-bg-kanban-success-accent) !important;
        color: unset;
    }

    /* Middle columns: neutral header like default .card-header */
    .workflow-accordion :deep(.accordion-item.middle-step .accordion-button) {
        background-color: rgba(0, 0, 0, 0.03) !important;
        color: #212529 !important;
        box-shadow: none !important;
    }

    .workflow-accordion :deep(.accordion-item.middle-step .workflow-accordion-step-title > .badge) {
        background-color: var(--color-hover-transfer) !important;
        color: unset;
    }

    .workflow-accordion :deep(.accordion-item .badge) {
        color: unset;
    }
</style>
