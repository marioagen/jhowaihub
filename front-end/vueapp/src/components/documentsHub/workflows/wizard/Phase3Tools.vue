<template>
    <div class="phase-container">
        <div class="row">
            <div class="col">
                <p class="section-title">
                    {{ $t("workflow.toolFlowsTitle") }}
                </p>
            </div>
        </div>

        <div
            v-if="!workflowSteps || workflowSteps.length === 0"
            class="phase3-empty text-center py-5"
        >
            <LucideIcon
                icon="Wrench"
                :size="40"
                class="phase3-empty__icon mb-3"
            />
            <p class="fw-semibold mb-1">{{ $t("workflow.noStepsAvailable") }}</p>
            <p class="text-muted small">{{ $t("workflow.noStepsHint") }}</p>
        </div>

        <div
            v-else
            class="row"
        >
            <div class="col-12">
                <div
                    v-if="hasAnyOutdatedTool"
                    class="phase3-outdated-banner mb-3 d-flex align-items-center justify-content-between gap-2 flex-wrap"
                >
                    <div class="d-flex align-items-center gap-2">
                        <LucideIcon icon="TriangleAlert" :size="16" class="phase3-outdated-banner__icon flex-shrink-0" />
                        <span class="small">{{ $t("workflow.outdatedToolsHint") }}</span>
                    </div>
                    <button
                        v-if="workflowId"
                        class="btn btn-sm phase3-outdated-action d-inline-flex align-items-center gap-1 flex-shrink-0"
                        @click="acknowledgeToolUpdate"
                        :disabled="isAcknowledging"
                    >
                        <span
                            v-if="isAcknowledging"
                            class="spinner-border spinner-border-sm"
                            role="status"
                        />
                        <LucideIcon v-else icon="CircleCheck" :size="13" />
                        {{ $t("workflow.confirmUpdate") }}
                    </button>
                </div>

                <div
                    class="phase3-hint mb-3 d-flex align-items-start gap-2"
                    v-if="stepsWithoutTools > 0"
                >
                    <LucideIcon
                        icon="Info"
                        :size="16"
                        class="phase3-hint__icon flex-shrink-0 mt-1"
                    />
                    <p class="mb-0 small">
                        {{ $t("workflow.phase3Hint", { count: stepsWithoutTools }) }}
                    </p>
                </div>

                <div
                    v-for="(step, index) in workflowSteps"
                    :key="step.id || index"
                    class="step-tool-card card shadow-sm rounded-3 mb-3"
                    :class="{ 'step-tool-card--configured': step.hasStepTools }"
                >
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <div class="step-badge">
                                {{ step.order || index + 1 }}
                            </div>
                            <div>
                                <h6 class="mb-0">{{ step.name }}</h6>
                                <span class="small text-muted d-flex align-items-center gap-1 mt-1">
                                    <LucideIcon
                                        icon="Users"
                                        :size="12"
                                    />
                                    {{ getProfileName(step.profileId) }}
                                </span>
                            </div>
                        </div>
                        <div class="d-flex align-items-center gap-2 flex-wrap">
                            <span
                                v-if="step.stepTools && step.stepTools.some(st => st.hasUpdate)"
                                class="badge phase3-outdated-badge d-flex align-items-center gap-1"
                            >
                                <LucideIcon icon="TriangleAlert" :size="12" />
                                {{ $t("workflow.outdatedLabel") }}
                            </span>
                            <span
                                v-if="step.hasStepTools"
                                class="badge bg-success-subtle text-success d-flex align-items-center gap-1"
                            >
                                <LucideIcon
                                    icon="CircleCheck"
                                    :size="13"
                                />
                                {{ $t("workflow.configuredLabel") }}
                            </span>
                            <span
                                v-else
                                class="badge bg-warning-subtle text-warning d-flex align-items-center gap-1"
                            >
                                <LucideIcon
                                    icon="CircleDashed"
                                    :size="13"
                                />
                                {{ $t("workflow.pendingLabel") }}
                            </span>
                        </div>
                    </div>
                    <div class="card-body">
                        <div
                            v-if="step.hasStepTools"
                            class="tools-list"
                        >
                            <p class="small text-muted mb-3">
                                <LucideIcon
                                    icon="Wrench"
                                    :size="14"
                                    class="me-1"
                                />
                                {{ $t("workflow.configuredTools") }}: {{ step.stepTools?.length ?? 0 }}
                            </p>
                            <div class="d-flex gap-2">
                                <button
                                    class="btn btn-outline-primary btn-sm flex-grow-1 d-inline-flex align-items-center justify-content-center gap-1"
                                    @click="editToolFlow(step)"
                                >
                                    <LucideIcon
                                        icon="SquarePen"
                                        :size="14"
                                    />
                                    {{ $t("workflow.editToolFlow") }}
                                </button>
                                <button
                                    class="btn btn-outline-danger btn-sm d-inline-flex align-items-center justify-content-center gap-1"
                                    @click="removeToolFlow(step)"
                                    :title="$t('workflow.removeToolFlow')"
                                >
                                    <LucideIcon
                                        icon="Trash2"
                                        :size="14"
                                    />
                                </button>
                            </div>
                        </div>
                        <div v-else>
                            <p class="text-muted small mb-2">
                                {{ $t("workflow.addToolFlowHint") }}
                            </p>
                            <button
                                class="btn btn-primary btn-sm w-100 d-inline-flex align-items-center justify-content-center gap-1"
                                @click="addToolFlow(step)"
                            >
                                <LucideIcon
                                    icon="Plus"
                                    :size="15"
                                />
                                {{ $t("workflow.addToolFlow") }}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "Phase3Tools",
        emits: ["add-tool-flow", "edit-tool-flow", "remove-tool-flow", "acknowledge-tool-update"],
        props: {
            workflowSteps: {
                type: Array,
                required: true,
            },
            profilesList: {
                type: Array,
                default: () => [],
            },
            phase: {
                type: Number,
                required: false,
                default: 0,
            },
            hasStepTools: {
                type: Boolean,
                required: false,
                default: false,
            },
            workflowId: {
                type: Number,
                required: false,
                default: null,
            },
            isAcknowledging: {
                type: Boolean,
                required: false,
                default: false,
            },
        },
        computed: {
            stepsWithoutTools() {
                return (this.workflowSteps || []).filter((s) => !s.hasStepTools).length;
            },
            hasAnyOutdatedTool() {
                return (this.workflowSteps || []).some((s) =>
                    (s.stepTools || []).some((st) => st.hasUpdate)
                );
            },
        },
        methods: {
            getProfileName(profileId) {
                const profile = this.profilesList.find((p) => p.id === parseInt(profileId));
                return profile ? profile.text : "—";
            },
            addToolFlow(step) {
                this.$emit("add-tool-flow", step, this.phase);
            },
            editToolFlow(step) {
                this.$emit("edit-tool-flow", step, this.phase);
            },
            removeToolFlow(step) {
                this.$emit("remove-tool-flow", step);
            },
            acknowledgeToolUpdate() {
                this.$emit("acknowledge-tool-update", this.workflowId);
            },
            getData() {
                return {
                    steps: this.workflowSteps.map((step) => ({
                        id: step.id,
                        order: step.order,
                        stepTools: step.stepTools || [],
                        hasStepTools: step.hasStepTools || false,
                    })),
                };
            },
        },
    };
</script>
<style scoped>
    .phase-container {
        padding: 20px 24px;
    }

    .section-title {
        font-size: 14px;
        color: #6c757d;
        margin-bottom: 16px;
    }

    .step-tool-card {
        border: 1px solid var(--color-border-form-control);
    }

    .card-header {
        background-color: var(--color-header-card-step) !important;
        border-bottom: 1px solid var(--color-border-form-control);
        padding: 12px 16px;
    }

    .step-badge {
        display: flex;
        justify-content: center;
        align-items: center;
        width: 32px;
        height: 32px;
        border-radius: 50%;
        background-color: #2f80ed;
        color: white;
        font-weight: 600;
        font-size: 0.85rem;
        margin-right: 12px;
        flex-shrink: 0;
    }

    .step-tool-card--configured {
        border-color: var(--bs-success-border-subtle, #a3cfbb);
    }

    .step-tool-card--configured .card-header {
        background-color: rgba(25, 135, 84, 0.04) !important;
    }

    .tools-list {
        border-top: 1px solid var(--color-border-form-control);
        padding-top: 12px;
    }

    .phase3-hint {
        background: rgba(13, 110, 253, 0.06);
        border: 1px solid rgba(13, 110, 253, 0.15);
        border-radius: 0.5rem;
        padding: 0.65rem 0.85rem;
        color: var(--bs-body-color);
    }

    .phase3-hint__icon {
        color: var(--color-bg-btn-primary, #0d6efd);
    }

    .phase3-outdated-banner {
        background: #fff8e1;
        border: 1px solid #ffe082;
        border-radius: 0.5rem;
        padding: 0.65rem 0.85rem;
        color: #6d4c06;
    }

    .phase3-outdated-banner__icon {
        color: #d97706;
    }

    .phase3-outdated-action {
        color: #ffffff;
        background-color: #ff6900;
        border-color: #ff6900;
    }

    .phase3-outdated-action:hover,
    .phase3-outdated-action:focus {
        color: #ffffff;
        background-color: #e65f00;
        border-color: #e65f00;
    }

    .phase3-outdated-badge {
        color: #212529;
        background-color: #ff6900;
    }

    .phase3-empty__icon {
        color: var(--bs-secondary-color, #6c757d);
        opacity: 0.5;
    }

    @media (max-width: 768px) {
        .phase-container {
            padding: 15px;
        }

        .card-header {
            flex-direction: column;
            align-items: flex-start !important;
            gap: 8px;
        }

        .card-header .text-muted {
            width: 100%;
        }

        .step-badge {
            width: 28px;
            height: 28px;
            margin-right: 8px;
        }

        .btn-sm {
            font-size: 0.875rem;
            padding: 0.25rem 0.5rem;
        }
    }

    @media (max-width: 576px) {
        .phase-container {
            padding: 10px;
        }

        .section-title {
            font-size: 12px;
        }

        h6 {
            font-size: 0.9rem;
        }
    }
</style>
