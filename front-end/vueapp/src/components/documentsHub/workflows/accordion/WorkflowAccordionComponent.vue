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
                        <span class="badge">{{ visibleCardCount(step) }}</span>
                    </span>
                </template>
                <p
                    v-if="cardsForStep(step).length === 0"
                    class="text-muted small mb-0"
                >
                    {{ $t("workflow.noCardsForStep") }}
                </p>
                <p
                    v-else-if="visibleCardsForStep(step).length === 0"
                    class="text-muted small mb-0"
                >
                    {{ $t("workflow.noCardsForStep") }}
                </p>
                <div
                    v-else
                    class="table-responsive border rounded workflow-accordion-table-wrap"
                >
                    <table class="table table-sm align-middle mb-0 small">
                        <thead>
                            <tr class="text-secondary">
                                <th
                                    scope="col"
                                    class="text-center border-bottom"
                                    style="width: 2.5rem"
                                >
                                    <input
                                        type="checkbox"
                                        class="form-check-input m-0"
                                        disabled
                                        :tabindex="-1"
                                        :aria-hidden="true"
                                    />
                                </th>
                                <th
                                    scope="col"
                                    class="border-bottom"
                                >
                                    {{ $t("workflow.listView.colId") }}
                                </th>
                                <th
                                    scope="col"
                                    class="border-bottom"
                                >
                                    {{ $t("workflow.listView.colDocument") }}
                                </th>
                                <th
                                    scope="col"
                                    class="border-bottom"
                                >
                                    {{ $t("workflow.listView.colDescription") }}
                                </th>
                                <th
                                    scope="col"
                                    class="border-bottom text-nowrap"
                                >
                                    {{ $t("workflow.listView.colDate") }}
                                </th>
                                <th
                                    scope="col"
                                    class="border-bottom"
                                >
                                    {{ $t("workflow.listView.colStatus") }}
                                </th>
                                <th
                                    scope="col"
                                    class="border-bottom"
                                >
                                    {{ $t("workflow.listView.colApplicant") }}
                                </th>
                                <th
                                    scope="col"
                                    class="border-bottom"
                                >
                                    {{ $t("workflow.listView.colResponsible") }}
                                </th>
                                <th
                                    scope="col"
                                    class="border-bottom text-end"
                                    style="width: 3.5rem"
                                >
                                    {{ $t("workflow.listView.colActions") }}
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <AccordionCardComponent
                                v-for="card in visibleCardsForStep(step)"
                                :key="card.id"
                                :data-card="card"
                                :data-step="step"
                                :is-first-step="step.order === minOrder"
                                :is-last-step="isLastStep(step)"
                                :users="users"
                                @reload="onReload"
                                @card-updated="onCardUpdated"
                                @card-moved="onCardMoved"
                            />
                        </tbody>
                    </table>
                </div>
            </AccordionItem>
        </AccordionComponent>
    </div>
</template>
<script>
    import AccordionComponent, { AccordionItem } from "@/components/global/AccordionComponent.vue";
    import AccordionCardComponent from "@/components/documentsHub/workflows/accordion/AccordionCardComponent.vue";

    const LAST_COLUMN_VISIBILITY_KEY = "kanban_last_column_visibility";

    export default {
        name: "WorkflowAccordionComponent",
        components: {
            AccordionComponent,
            AccordionItem,
            AccordionCardComponent,
        },
        props: {
            data: {
                type: [Array, Object],
                required: true,
            },
            users: {
                type: Array,
                required: false,
                default: () => [],
            },
        },
        emits: ["reload", "cardUpdated", "cardMoved"],
        data: () => ({
            isLastColumnVisible: true,
        }),
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
        mounted() {
            const saved = localStorage.getItem(LAST_COLUMN_VISIBILITY_KEY);
            if (saved !== null) {
                this.isLastColumnVisible = saved === "true";
            }
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
            isLastStep(step) {
                return step.order === this.maxOrder;
            },
            showFinalized(statusId, step) {
                if (statusId === 6) return false;
                if (this.isLastStep(step)) return this.isLastColumnVisible;
                return true;
            },
            visibleCardsForStep(step) {
                return this.cardsForStep(step).filter((card) =>
                    this.showFinalized(card.status?.id, step)
                );
            },
            visibleCardCount(step) {
                return this.visibleCardsForStep(step).length;
            },
            onReload() {
                this.$emit("reload");
            },
            onCardUpdated(payload) {
                this.$emit("cardUpdated", payload);
            },
            onCardMoved(payload) {
                this.$emit("cardMoved", payload);
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

    .workflow-accordion-table-wrap {
        font-size: 0.8125rem;
    }

    .workflow-accordion :deep(.accordion-item) {
        margin-bottom: 0.375rem;
    }

    .workflow-accordion :deep(.accordion-item:last-of-type) {
        margin-bottom: 0;
    }
    .workflow-accordion :deep(.accordion-header) {
        font-size: inherit;
        font-weight: inherit;
        line-height: inherit;
    }

    .workflow-accordion :deep(.accordion-header .accordion-button) {
        word-wrap: break-word;
        overflow-wrap: break-word;
        white-space: normal;
        hyphens: auto;
        flex-shrink: 0;
        font-family: inherit;
        font-size: 0.875rem;
        font-weight: 400;
        line-height: 1.5;
        padding: 0.25rem 0.5rem;
        min-height: 0;
    }

    .workflow-accordion :deep(.accordion-header .accordion-button::after) {
        width: 0.875rem;
        height: 0.875rem;
        background-size: 0.875rem;
    }

    .workflow-accordion :deep(.workflow-accordion-step-title) {
        display: inline;
        text-align: start;
    }

    .workflow-accordion :deep(.workflow-accordion-step-title > .badge) {
        font-size: 0.65rem;
        padding: 0.2em 0.45em;
        font-weight: 500;
        vertical-align: middle;
    }

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
