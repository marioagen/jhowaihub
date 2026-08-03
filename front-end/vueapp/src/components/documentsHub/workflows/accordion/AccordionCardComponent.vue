<template>
    <tr class="accordion-card-row w-100">
        <td class="text-center align-middle py-2">
            <input
                type="checkbox"
                class="form-check-input m-0"
                :checked="isCardSelected"
                @change="onRowCheckboxChange"
            />
        </td>
        <td class="align-middle py-2 small">
            <button
                type="button"
                class="btn btn-link btn-sm p-0 text-decoration-none"
                :disabled="showLoading"
                @click="redirectToAnalyzer"
            >
                #{{ dataCard.id }}
            </button>
        </td>
        <td
            class="align-middle py-2 small min-w-0"
            :title="dataCard.name"
        >
            {{ truncateText(dataCard.name) }}
        </td>
        <td class="align-middle py-2 small min-w-0">
            <span class="d-inline-block text-truncate w-100">
                {{ dataCard.description || "—" }}
            </span>
        </td>
        <td class="align-middle py-2 small text-nowrap">
            {{ formatDateWithTime(dataCard.created) }}
        </td>
        <td class="align-middle py-2">
            <span
                class="badge status-badge"
                :style="statusBadgeStyle"
            >
                {{ statusLabel }}
            </span>
        </td>
        <td
            class="align-middle py-2 small text-truncate min-w-0"
            :title="dataCard.owner"
        >
            {{ dataCard.owner || "—" }}
        </td>
        <td
            class="align-middle py-2 small text-truncate min-w-0"
            :title="assignedName"
        >
            {{ assignedName }}
        </td>
        <td class="align-middle py-2 text-end">
            <div class="d-flex align-items-center justify-content-end gap-1">
                <!-- Inline approve/reject quick-action buttons (mirrors Kanban card) -->
                <template v-if="showQuickActions && !showLoading">
                    <button
                        type="button"
                        class="card-reject-btn"
                        @click.stop="emitReject"
                        :title="$t('common.reject')"
                        :aria-label="$t('common.reject')"
                    >
                        <LucideIcon
                            icon="CircleX"
                            :size="16"
                        />
                    </button>
                    <button
                        type="button"
                        class="card-approve-btn"
                        @click.stop="onAdvanceClick"
                        :title="$t('common.approve')"
                        :aria-label="$t('common.approve')"
                        :disabled="isLoadingAnalysis"
                    >
                        <LucideIcon
                            v-if="!isLoadingAnalysis"
                            icon="CircleCheck"
                            :size="16"
                        />
                        <div
                            v-else
                            class="spinner-grow spinner-grow-sm"
                            role="status"
                        />
                    </button>
                </template>

                <!-- Overflow menu: Analisar + Avançar -->
                <div class="dropdown dropdown-end">
                    <button
                        :id="`accordion-card-actions-${dataCard.id}`"
                        type="button"
                        class="btn btn-light btn-sm border-0 px-2 py-1"
                        data-bs-toggle="dropdown"
                        data-bs-auto-close="true"
                        aria-expanded="false"
                        :aria-label="$t('workflow.actions')"
                    >
                        <LucideIcon
                            icon="Ellipsis"
                            :size="18"
                            class="text-muted"
                        />
                    </button>
                    <ul
                        class="dropdown-menu dropdown-menu-end shadow-sm"
                        :aria-labelledby="`accordion-card-actions-${dataCard.id}`"
                    >
                        <li>
                            <button
                                type="button"
                                class="dropdown-item d-flex align-items-center gap-2"
                                :disabled="showLoading"
                                @click="onAnalyzeClick"
                            >
                                <LucideIcon
                                    icon="FileSearch"
                                    :size="16"
                                    class="text-primary flex-shrink-0"
                                />
                                {{ $t("common.analyze") }}
                            </button>
                        </li>
                    </ul>
                </div>
            </div>
        </td>
    </tr>
</template>
<script>
    import CardsServices from "@/services/cards/CardsServices";
    import dates from "@/helpers/date";

    export default {
        name: "AccordionCardComponent",
        emits: ["reload", "cardUpdated", "cardMoved", "toggle-card-selection", "cardReject"],
        props: {
            isCardSelected: {
                type: Boolean,
                default: false,
            },
            dataCard: {
                type: Object,
                required: true,
            },
            dataStep: {
                type: Object,
                required: true,
            },
            isFirstStep: {
                type: Boolean,
                required: true,
            },
            isLastStep: {
                type: Boolean,
                required: true,
            },
            users: {
                type: Array,
                required: false,
                default: () => [],
            },
        },
        data: () => ({
            isLoadingAnalysis: false,
        }),
        computed: {
            showLoading() {
                return !this.isCardFailed && this.dataCard.percentage < 100;
            },
            isCardRejected() {
                return this.dataCard.status?.name?.toLowerCase() === "rejected";
            },
            isCardFailed() {
                return this.dataCard.status?.name?.toLowerCase() === "fail";
            },
            statusBadgeStyle() {
                const color =
                    this.isCardRejected || this.isCardFailed
                        ? this.dataCard.status?.color
                        : this.dataStep.status?.color;
                if (!color) return {};
                return {
                    "--cor-base": color,
                    color: "var(--cor-base)",
                    backgroundColor: "color-mix(in srgb, var(--cor-base) 30%, white)",
                };
            },
            statusLabel() {
                const status =
                    this.isCardRejected || this.isCardFailed
                        ? this.dataCard.status
                        : this.dataStep.status;
                const name = status?.name;
                if (!name) return "—";
                return this.$t("workflow.statusList." + name.toLowerCase());
            },
            assignedName() {
                return this.dataCard.assignedUser?.name || "—";
            },
            showAdvance() {
                if (this.isLastStep) return false;
                return !this.isFirstStep || !!this.dataCard.assignedUser;
            },
            showQuickActions() {
                if (this.isLastStep) return false;
                return !this.isFirstStep || !!this.dataCard.assignedUser;
            },
            backPage() {
                return this.$route.query.page;
            },
        },
        methods: {
            formatDateWithTime(date) {
                return dates.formatDateWithTime(date);
            },
            truncateText(text) {
                if (!text) return "";
                return text.length > 40 ? text.substring(0, 40) + "…" : text;
            },
            redirectToAnalyzer() {
                if (this.showLoading) return;
                this.$router.push({
                    name: "Analyzer",
                    params: {
                        documentId: this.dataCard.documentId,
                        cardId: this.dataCard.id,
                    },
                    query: this.backPage ? { page: this.backPage } : {},
                });
            },
            onAnalyzeClick() {
                this.redirectToAnalyzer();
            },
            onRowCheckboxChange(e) {
                this.$emit("toggle-card-selection", {
                    cardId: this.dataCard.id,
                    selected: e.target.checked,
                });
            },
            async updateStatus(nextStepOrder = null) {
                const targetOrder = nextStepOrder ?? this.dataStep.order + 1;
                if (this.isLastStep && nextStepOrder === null) {
                    return;
                }
                const params = {
                    CardId: this.dataCard.id,
                    NextStepOrder: targetOrder,
                    WorkflowId: this.dataStep.workflowId,
                };
                const response = await CardsServices.updateStepAndStatus(params);
                if (response?.error !== undefined) {
                    throw new Error(response.error.response?.data?.labelError);
                }
            },
            emitReject() {
                this.$emit("cardReject", {
                    cardId: this.dataCard.id,
                    workflowId: this.dataStep.workflowId,
                });
            },
            async onAdvanceClick() {
                if (!this.showAdvance || this.isLoadingAnalysis) return;
                this.isLoadingAnalysis = true;
                const nextStepOrder = this.dataStep.order + 1;
                try {
                    await this.updateStatus();
                    this.$emit("cardMoved", {
                        card: { ...this.dataCard },
                        currentStepOrder: this.dataStep.order,
                        nextStepOrder,
                    });
                } catch (e) {
                    this.$notify({
                        title: "common.error",
                        message: "card.errorAdvancingCard",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoadingAnalysis = false;
                }
            },
        },
    };
</script>
<style scoped>
    .accordion-card-row {
        width: 100%;
    }

    .accordion-card-row .status-badge {
        font-size: 0.65rem;
        font-weight: 500;
        max-width: 100%;
        white-space: normal;
    }

    .animate-spin {
        animation: spin 1s linear infinite;
    }

    @keyframes spin {
        100% {
            transform: rotate(360deg);
        }
    }

    /* ── Approve / Reject quick-action buttons (mirrors KanbanCard) ── */
    .card-approve-btn,
    .card-reject-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 32px;
        height: 32px;
        min-width: 32px;
        padding: 0;
        border: none;
        background: transparent;
        border-radius: 50%;
        cursor: pointer;
        flex-shrink: 0;
        transition: background-color 0.15s ease, transform 0.1s ease;
    }

    .card-approve-btn {
        color: #0eaa42;
    }

    .card-approve-btn:hover:not(:disabled) {
        background-color: rgba(14, 170, 66, 0.12);
        color: #089436;
        transform: scale(1.12);
    }

    .card-approve-btn:active:not(:disabled) {
        transform: scale(0.95);
    }

    .card-approve-btn:disabled {
        opacity: 0.55;
        cursor: default;
    }

    .card-reject-btn {
        color: #dc3545;
    }

    .card-reject-btn:hover {
        background-color: rgba(220, 53, 69, 0.12);
        color: #b02a37;
        transform: scale(1.12);
    }

    .card-reject-btn:active {
        transform: scale(0.95);
    }

    .spinner-grow {
        width: 1rem;
        height: 1rem;
    }
</style>
