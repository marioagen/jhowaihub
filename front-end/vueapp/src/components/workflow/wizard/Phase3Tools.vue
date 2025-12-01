<template>
    <div class="phase-container">
        <div class="row">
            <div class="col">
                <p class="section-title">{{ $t("workflow.toolFlowsTitle") }}</p>
            </div>
        </div>
        <div v-if="!workflowSteps || workflowSteps.length === 0" class="text-center text-muted py-5">
            <p>{{ $t("workflow.noStepsAvailable") }}</p>
        </div>
        <div v-else class="row">
            <div class="col-12">
                <div
                    v-for="(step, index) in workflowSteps"
                    :key="step.id || index"
                    class="step-tool-card card shadow-sm rounded-3 mb-3"
                >
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <div class="step-badge">
                                <LucideIcon icon="Info" :size="16" />
                            </div>
                            <h6 class="mb-0">{{ step.name }}</h6>
                        </div>
                        <div class="text-muted small">
                            <LucideIcon icon="Users" :size="14" class="me-1" />
                            {{ getProfileName(step.profileId) }}
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label small text-muted">{{ $t("workflow.responsible") }}</label>
                            <div class="d-flex align-items-center">
                                <LucideIcon icon="Users" :size="16" class="me-2 text-primary" />
                                <span>{{ getProfileName(step.profileId) }}</span>
                            </div>
                        </div>
                        <div v-if="step.hasStepTools" class="tools-list">
                            <p class="small text-muted mb-2">{{ $t("workflow.configuredTools") }}: {{ step.length }}</p>
                            <button
                                class="btn btn-outline-primary btn-sm w-100 mb-2"
                                @click="editToolFlow(step)"
                            >
                                <LucideIcon icon="SquarePen" :size="15" class="me-1" />
                                {{ $t("workflow.editToolFlow") }}
                            </button>
                            <button
                                class="btn btn-outline-danger btn-sm w-100"
                                @click="removeToolFlow(step)"
                            >
                                <LucideIcon icon="Trash" :size="15" class="me-1" />
                                {{ $t("workflow.removeToolFlow") }}
                            </button>
                        </div>
                        <div v-else>
                            <button
                                class="btn btn-outline-primary btn-sm w-100"
                                @click="addToolFlow(step)"
                            >
                                <LucideIcon icon="Plus" :size="15" class="me-1" />
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
    props: {
        workflowSteps: {
            type: Array,
            required: true
        },
        profilesList: {
            type: Array,
            default: () => []
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
        }

    },
    methods: {
        getProfileName(profileId) {
            const profile = this.profilesList.find(p => p.id === parseInt(profileId));
            return profile ? profile.text : 'N/A';
        },
        addToolFlow(step) {
            console.log(step);
            this.$emit('add-tool-flow', step, this.phase);
        },
        editToolFlow(step) {
            this.$emit('edit-tool-flow', step, this.phase);
        },
        removeToolFlow(step) {
            this.$emit('remove-tool-flow', step);
        },
        getData() {
            return {
                steps: this.workflowSteps.map(step => ({
                    id: step.id,
                    order: step.order,
                    stepTools: step.stepTools || [],
                    hasStepTools: step.hasStepTools || false
                }))
            };
        }
    }
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
    border: 1px solid #e5e7eb;
}

.card-header {
    background-color: #f9fafb;
    border-bottom: 1px solid #e5e7eb;
    padding: 12px 16px;
}

.step-badge {
    display: flex;
    justify-content: center;
    align-items: center;
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background-color: #2F80ED;
    color: white;
    margin-right: 12px;
}

.tools-list {
    border-top: 1px solid #e5e7eb;
    padding-top: 12px;
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
