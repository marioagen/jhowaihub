<template>
    <div class="phase-container">
        <div class="row">
            <div class="col">
                <p class="section-title">{{ $t("workflow.stepsTitle") }}</p>
            </div>
            <div class="col-auto">
                <button class="btn btn-primary btn-sm" type="button" @click="addStep">
                    <LucideIcon icon="Plus" :size="15" />
                    {{ $t("workflow.createNewStep") }}
                </button>
            </div>
        </div>
        <div v-if="isLoadingProfiles || isLoadingStatus">
            <div class="d-flex justify-content-center">
                <div class="spinner-border text-primary" role="status"></div>
            </div>
        </div>
        <div v-else class="row">
            <div class="d-flex gap-3 overflow-auto flex-nowrap pb-2">
                <div
                    v-for="(step, index) in steps"
                    :key="step.id || step.tempId || index"
                    class="step-card card shadow-sm rounded-3"
                >
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <div
                                class="step-number"
                            >
                                {{ index + 1 }}
                            </div>
                            <Field
                                :name="`steps[${index}].name`"
                                rules="required"
                                v-model="step.name"
                                v-slot="{ field, errors }"
                            >
                                <div class="d-flex flex-column">
                                    <input
                                        type="text"
                                        class="input-title"
                                        v-bind="field"
                                        :placeholder="$t('workflow.stepNamePlaceholder')"
                                    />
                                    <span v-if="errors[0]" class="validation-message text-danger mt-1">
                                        {{ errors[0] }}
                                    </span>
                                </div>
                            </Field>
                        </div>
                        <button
                            type="button"
                            class="btn btn-link btn-sm"
                            @click="removeStep(index)"
                        >
                            <LucideIcon icon="X" />
                        </button>
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label text-muted small">{{ $t("workflow.status") }}</label>
                            <Field
                                :name="`steps[${index}].statusId`"
                                rules="required"
                                v-model="step.statusId"
                                v-slot="{ field, errors }"
                            >
                                <div class="d-flex flex-column">
                                    <select class="form-select form-select-sm" v-bind="field">
                                        <option value="">{{ $t("workflow.selectStatus") }}</option>
                                        <option v-for="s in statusList" :key="s.id" :value="String(s.id)">
                                            {{ s.name }}
                                        </option>
                                    </select>
                                    <span v-if="errors[0]" class="text-danger small mt-1">{{ errors[0] }}</span>
                                </div>
                            </Field>
                        </div>
                        <div class="mb-2">
                            <label class="form-label text-muted small">{{ $t("workflow.profiles") }}</label>
                            <Field
                                :name="`steps[${index}].profileId`"
                                rules="required"
                                v-model="step.profileId"
                                v-slot="{ field, errors }"
                            >
                                <div class="d-flex flex-column">
                                    <div class="input-group">
                                        <span class="input-group-text border-end-0 bg-white">
                                            <LucideIcon icon="Users" :size="16" />
                                        </span>
                                        <select class="form-select form-select-sm border-start-0" v-bind="field">
                                            <option value="">{{ $t("workflow.selectProfile") }}</option>
                                            <option v-for="p in profilesList" :key="p.id" :value="String(p.id)">
                                                {{ p.text }}
                                            </option>
                                        </select>
                                    </div>
                                    <span v-if="errors[0]" class="text-danger small mt-1">{{ errors[0] }}</span>
                                </div>
                            </Field>
                        </div>
                    </div>
                </div>
                <div
                    class="add-step-card text-center p-4 rounded-3 border-dashed flex-shrink-0"
                    @click="addStep"
                >
                    <div class="icon-circle mb-2">
                        <LucideIcon icon="Plus" :size="16" />
                    </div>
                    <h6 class="fw-semibold mb-1">{{ $t("workflow.addStep") }}</h6>
                    <p class="text-muted small mb-0">{{ $t("workflow.addStepDescription") }}</p>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { Field } from "vee-validate";
import ProfilesService from "@/services/profiles/ProfilesService";
import StatusService from "@/services/status/StatusService";

export default {
    name: "Phase2Steps",
    components: {
        Field,
    },
    props: {
        initialSteps: {
            type: Array,
            default: () => []
        }
    },
    data() {
        return {
            steps: this.initialSteps.length > 0 ? [...this.initialSteps] : [],
            profilesList: [],
            statusList: [],
            isLoadingProfiles: true,
            isLoadingStatus: true,
            tempStepCounter: 1,
        };
    },
    methods: {
        getProfiles() {
            this.isLoadingProfiles = true;
            ProfilesService.getProfilesList()
                .then((response) => {
                    if (response.error !== undefined) return;
                    this.profilesList = response.map(r => ({ id: r.id, text: r.name }));
                })
                .finally(() => {
                    this.isLoadingProfiles = false;
                });
        },
        getStatus() {
            this.isLoadingStatus = true;
            StatusService.getStatus()
                .then((response) => {
                    if (response.error !== undefined) return;
                    this.statusList = response;
                })
                .finally(() => {
                    this.isLoadingStatus = false;
                });
        },
        addStep() {
            this.steps.push({
                id: 0,
                tempId: this.tempStepCounter++,
                name: '',
                order: this.steps.length + 1,
                profileId: '',
                statusId: '',
            });
        },
        removeStep(index) {
            this.steps.splice(index, 1);
            // Reorder remaining steps
            this.steps.forEach((step, idx) => {
                step.order = idx + 1;
            });
        },
        getData() {
            return {
                steps: this.steps.map((step, index) => ({
                    id: step.id || 0,
                    name: step.name,
                    order: index + 1,
                    profileId: parseInt(step.profileId),
                    statusId: parseInt(step.statusId),
                }))
            };
        }
    },
    created() {
        this.getProfiles();
        this.getStatus();
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

.step-card {
    min-width: 280px;
    flex-shrink: 0;
}

.card-header {
    background-color: #e8f1ff;
    padding: 12px 16px;
}

.step-number {
    display: flex;
    justify-content: center;
    align-items: center;
    width: 28px;
    height: 28px;
    border-radius: 50%;
    background-color: #2F80ED;
    color: white;
    font-weight: bold;
    margin-right: 8px;
}

.input-title {
    border: none;
    background: transparent;
    font-weight: 600;
    padding: 4px;
}

.input-title:focus {
    outline: none;
    border-bottom: 1px solid #2F80ED;
}

.add-step-card {
    min-width: 240px;
    flex-shrink: 0;
    border: 2px dashed #d1d5db;
    cursor: pointer;
    min-height: 240px;
    transition: background-color 0.2s;
}

.add-step-card:hover {
    background-color: #f9fafb;
}

.icon-circle {
    width: 32px;
    height: 32px;
    border: 1.5px dashed #9ca3af;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto;
    color: #6b7280;
}

.border-dashed {
    border-style: dashed !important;
}
</style>
