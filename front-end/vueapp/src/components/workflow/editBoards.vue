<template>
    <main>
        <div class="container-fluid scroll-area mx-4 mt-4">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("workflow.title") }}</h5>
                        <p><small class="text-muted">{{ $t("workflow.subtitle") }}</small></p>
                    </div>
                </div>
            </div>
            
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <div class="row">
                        <div class="col">
                            <p><small class="text-muted">{{ $t("workflow.manage") }}</small></p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col">
                            <label>{{ $t("quizzes.formName") }}</label>
                            <input 
                                class="form-control form-control-sm"
                                :placeholder="$t('quizzes.formNamePlaceholder')"
                                v-model="workflowData.name"
                            />
                        </div>
                        <div class="col">
                            <label>{{ $t("quizzes.type") }}</label>
                            <select
                                id="typeDocId"
                                class="form-select form-select-sm"
                                v-model="workflowData.profile"
                            >
                                <option value="">{{ $t("quizzes.formSelect") }}</option>
                                <!-- <option 
                                    v-for="(item, index) in docTypesList" 
                                    :key="index"
                                    :value="item.id" 
                                >
                                    {{ item.id }} - {{ item.name }}
                                </option> -->
                            </select>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row mt-4">
                <div>
                    <h6 class="mb-4">{{ $t("workflow.steps") }}</h6>
                </div>
                <div class="row">
                    <div class="d-flex gap-3 overflow-auto flex-nowrap pb-2">
                        <WorkflowStepComponent
                            v-for="(step, index) in stepsList"
                            :key="index"
                            :step="step"
                            :index="index + 1"
                            @update-step="updateStep(index, $event)"
                            @remove-step="removeStep(index)"
                            class="workflow-step-card"
                        />
                        
                        <div class="add-step-card text-center p-4 rounded-3 border-dashed flex-shrink-0" @click="addStep">
                            <div class="icon-circle mb-2">
                                <LucideIcon icon="Plus" size="16" />
                            </div>
                            <h6 class="fw-semibold mb-1">Adicionar Etapa</h6>
                            <p class="text-muted small mb-0">Clique para criar uma nova etapa</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import WorkflowStepComponent from "@/components/workflow/WorkflowStepComponent.vue";

    export default {
        name: "QuizFormNew",
        props: {
        },
        components: {
            WorkflowStepComponent
        },
        data() {
            return {
                stepsList: [],
                steps: {
                    status: "",
                    profile: "",
                },
                workflowData: {
                    name: "",
                    profile: "",
                },
            };
        },
        watch: {
            "$store.state.userProfile.language": function () {
                this.setCrumbsData();
            },
        },
        methods: {
            updateStep(index, updatedStep) {
                this.stepsList[index] = { ...this.stepsList[index], ...updatedStep };
            },
            addStep() {
                this.stepsList.push({
                    status: '',
                    profile: '',
                });
            },
            removeStep(index) {
                this.stepsList.splice(index, 1);
            },
        },
        created() {
        },
    };
</script>

<style scoped>
    @import "@vueform/multiselect/themes/default.css";

    .multiselect-dropdown {
        max-height: var(--ms-max-height) !important;
    }

    .form-save {
        padding-top: 20px !important;
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }

    .container-fluid {
        padding: 0 13px;
    }

    .main-div {
        border: 1px solid #d3d3d3;
        border-radius: 8px;
        background: white;
        padding: 20px 24px;
    }

    .add-step-card {
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
    .workflow-step-card {
        min-width: 280px;
        flex-shrink: 0;
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
</style>
