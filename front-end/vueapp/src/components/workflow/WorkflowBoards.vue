<template>
    <main>
        <FullscreenLoadingComponent v-if="isLoading" />
        <div
            class="container-fluid scroll-area mx-4 mt-4"
        >
            <div class="row align-items-center">
                <div class="col-auto">                    
                    <div class="row">
                        <div class="col-2">
                            <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="redirectToIndex" type="button">
                                <LucideIcon icon="ArrowLeft" />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">{{ $t(formTitle) }}</h5>
                                <p><small class="text-muted">{{ $t(formSubtitle) }}</small></p>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-auto ms-auto">
                    <button 
                        class="btn btn-primary btn-sm" 
                        :disabled="cantSave"
                        type="button"
                        @click="save"
                    >
                        <LucideIcon icon="Save" :size="15" />
                        {{ $t("quizzes.formSave") }}
                    </button>
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
                            <label>{{ $t("workflow.name") }}</label>
                            <Field 
                                name="name" 
                                rules="required" 
                                v-slot="{ field, errorMessage }"
                                v-model="workflowData.name"
                                ref="nameField"
                            >
                                <input 
                                    class="form-control form-control-sm"
                                    :placeholder="$t('workflow.name')"
                                    v-bind="field"
                                />
                                <span class="validation-message text-danger" v-if="errorMessage">{{ errorMessage }}</span>
                            </Field>
                        </div>
                        <div class="col">
                            <label>{{ $t("workflow.responsableTeam") }}</label>
                            <div class="input-group">
                                <span class="input-group-text border-end-0 bg-white">
                                    <LucideIcon icon="Users" :size="16" />
                                </span>

                                <Field 
                                    name="teamId" 
                                    rules="required" 
                                    v-slot="{ field, errors }" 
                                    v-model="workflowData.teamId"
                                    ref="teamField"
                                >
                                    <select
                                        id="typeDocId"
                                        class="form-select form-select-sm border-start-0"
                                        v-bind="field"
                                    >
                                        <option value="" disabled>{{ $t("workflow.responsableTeam") }}</option>
                                        <option 
                                            v-for="(item, index) in teamsList"
                                            :key="index"
                                            :value="item.id" 
                                        >
                                            {{ item.id }} - {{ item.text }}
                                        </option>
                                    </select>

                                    <span class="validation-message text-danger" v-if="errors?.length">
                                        {{ errors[0] }}
                                    </span>
                                </Field>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row mt-4">
                <div class="row d-flex justify-content-between align-items-center">
                    <div class="col-auto">
                        <h6 class="mb-4">{{ $t("workflow.steps") }}</h6>
                    </div>
                    <div class="col-auto">
                        <button class="btn btn-primary btn-sm" type="button" @click="addStep">
                            <LucideIcon icon="Plus" :size="15" />
                            {{ $t("workflow.createNewStep") }}
                        </button>
                    </div>
                </div>
                <div v-if="isLoadingSteps">
                    <div class="d-flex justify-content-center">
                        <div class="spinner-border text-primary" role="status"></div>
                    </div>
                </div>
                <div v-else class="row">
                    <div class="d-flex gap-3 overflow-auto flex-nowrap pb-2">
                        <WorkflowStepComponent
                            v-for="(step, index) in activeStepsList"
                            :key="step.id || index"
                            :step="step"
                            :index="index + 1"
                            :is-last="index === activeStepsList.length - 1" 
                            :profilesList="profilesList"
                            :statusList="statusList"
                            :isEdit="isEdit"
                            :workflowId="id"
                            @update-step="updateStep(index, $event)"
                            @remove-step="removeStep(index, $event)"
                            @saveWorkflow="saveWorkflowInStore"
                            class="workflow-step-card"
                            ref="stepRefs"
                        />                        
                        <div class="add-step-card text-center p-4 rounded-3 border-dashed flex-shrink-0" @click="addStep">
                            <div class="icon-circle mb-2">
                                <LucideIcon icon="Plus" :size="16" />
                            </div>
                            <h6 class="fw-semibold mb-1">{{ $t("workflow.addBtn") }}</h6>
                            <p class="text-muted small mb-0">{{ $t("workflow.addBtnDescription") }}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import { Field, useForm } from "vee-validate";
    import WorkflowStepComponent from "@/components/workflow/WorkflowStepComponent.vue";
    import TeamsService from "@/services/teams/TeamsService";
    import StatusService from "@/services/status/StatusService";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import FullscreenLoadingComponent from "@/components/global/FullscreenLoadingComponent.vue";
    
    export default {
        name: "WorkflowBoards",
        components: {
            FullscreenLoadingComponent,
            WorkflowStepComponent,
            Field,
        },
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
            id: {
                type: Number,
                required: false,
                default: null,
            },
        },
        setup() {
            const { validate, values, setValues } = useForm();
            return {
                validate,
                values,
                setValues
            }
        },
        data() {
            return {
                profilesList: [],
                statusList: [],
                teamsList: [],
                stepsList: [],
                workflowData: {
                    name: "",
                    teamId: "",
                },
                isLoading: false,
                isLoadingSteps: false,
                workflowStepRefs: [],
            };
        },
        computed: {
            cantSave() {
                return this.stepsList.length === 0;
            },
            formTitle() {
                return this.isEdit ? "workflow.formEdit.title" : "workflow.formCreate.title";
            },
            formSubtitle() {
                return this.isEdit ? "workflow.formEdit.subtitle" : "workflow.formCreate.subtitle";
            },
            activeStepsList() {
                return this.stepsList.filter(s => s.isActive !== false);
            },
        },
        methods: {
            getTeams() {
                TeamsService.getTeamList()
                    .then((response) => {
                        if(response.error !== undefined) return;
                        this.teamsList = response.map(r => ({ id: r.id, text: r.name }));
                    });
            },
            getStatus() {
                StatusService.getStatus()
                    .then((response) => {
                        if(response.error !== undefined) return;
                        this.statusList = response;
                    });
            },
            getProfiles() {
                ProfilesService.getProfilesList()
                    .then((response) => {
                        if(response.error !== undefined) return;
                        this.profilesList = response.map(r => ({ id: r.id, text: r.name }));
                    });
            },
            setEdit() {
                let hasInStore = this.$store.state.tempWorkflow.status;
                if(hasInStore && !this.isEdit) {
                    this.setWorkflowFromStore();
                }

                if(this.isEdit) {
                    this.isLoading = true;
                    WorkflowService.getWorkflowById(this.id)
                        .then((response) => {
                            if(response.error !== undefined) {
                                this.$router.push({ name: "Workflow" });
                                return this.$notify({
                                    title: 'workflow.index',
                                    message: response.error,
                                    variant: 'danger',
                                    icon: 'CircleX',
                                });
                            }
                            this.workflowData.id = response.id;
                            this.workflowData.name = response.name;
                            this.workflowData.teamId = response.teamId;
                            this.stepsList = response.steps.map(step => ({
                                ...step,
                                profileId: step.profile?.id || "",
                                statusId: step.status?.id || "",
                                isActive: true,
                            }));
                        })
                        .finally(() => {
                            this.saveWorkflowInStore();
                            this.setWorkflowFromStore();
                            this.isLoading = false;
                        });
                }
            },
            setWorkflowFromStore() {
                let workflowData = this.$store.state.tempWorkflow.data;
                this.workflowData.name = workflowData.name;
                this.workflowData.teamId = workflowData.teamId;
                console.log(this.$store.state.tempWorkflow)
                this.stepsList = this.$store.state.tempWorkflow.list;
            },
            updateStep(index, updatedStep) {
                this.stepsList[index] = { ...this.stepsList[index], ...updatedStep };
            },
            addStep() {
                this.stepsList.push({
                    id: 0,
                    name: '',
                    status: '',
                    profile: '',
                    isActive: true,
                    stepTools: []
                });
            },
            removeStep(index, deactivatedStep) {
                this.isLoadingSteps = true;
                if (this.isEdit) {
                    const i = this.stepsList.findIndex(s => s.id === deactivatedStep.id);
                    if (i !== -1) {
                        this.stepsList.splice(i, 1, { ...this.stepsList[i], ...deactivatedStep });
                    }
                } else {
                    const active = this.activeStepsList[index];
                    const i = this.stepsList.findIndex(s => s === active);
                    if (i !== -1) {
                        this.stepsList.splice(i, 1, { ...this.stepsList[i], ...deactivatedStep });
                    }
                }
                setTimeout(() => {
                   this.isLoadingSteps = false;
                }, 3000);
            },
            async save() {
                if (!this.stepsList || this.stepsList.length === 0) {
                    return this.$notify({
                        title: 'workflow.index',
                        message: 'validation.oneStep',
                        variant: 'warning',
                        icon: 'CircleAlert',
                    });
                }

                const nameValid = await this.$refs.nameField?.validate?.();
                const teamValid = await this.$refs.teamField?.validate?.();

                let stepsValid = true;
                const stepRefs = this.$refs.stepRefs || [];
                for (const stepRef of stepRefs) {
                    if (stepRef?.validateStep) {
                        const valid = await stepRef.validateStep();
                        if (!valid) stepsValid = false;
                    }
                }

                if (!nameValid?.valid || !teamValid?.valid || !stepsValid) {
                    return this.$notify({
                        title: 'workflow.index',
                        message: 'validation.hasInvalid',
                        variant: 'warning',
                        icon: 'CircleAlert',
                    });
                }
                
                this.reorderList();

                this.isLoading = true;
                if(this.isEdit) {
                    return this.editWorkflow();
                }
                return this.createWorkflow();
            },
            reorderList() {
                this.activeStepsList.forEach((step, index) => {
                    step.order = index + 1;
                });
            },
            createWorkflow() {
                console.log(this.$store.state.tempWorkflow.list);
                let params = {
                    name: this.workflowData.name,
                    teamId: this.workflowData.teamId,
                    steps: this.$store.state.tempWorkflow.list,
                };

                WorkflowService.createWorkflow(params)
                    .then((response) => {
                        if(response.error === undefined) {
                            this.redirectToIndex();
                            return this.$notify({
                                title: 'workflow.index',
                                message: 'workflow.createSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        }
                        this.$notify({
                            title: 'workflow.index',
                            message: 'workflow.createError',
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            editWorkflow() {
                console.log( this.$store.state.tempWorkflow.list);
                let params = {
                    id: this.workflowData.id,
                    name: this.workflowData.name,
                    teamId: this.workflowData.teamId,
                    steps: this.$store.state.tempWorkflow.list,
                };
                console.log(params);
                WorkflowService.editWorkflow(params)
                    .then((response) => {
                        if(response.error === undefined) {
                            this.redirectToIndex();
                            return this.$notify({
                                title: 'workflow.index',
                                message: 'workflow.editSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        }
                        this.$notify({
                            title: 'workflow.index',
                            message: 'workflow.editError',
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            redirectToIndex() {
                this.$store.commit('cleanTempWorkflow');
                return this.$router.push({ name: "WorkflowEditor" });
            },
            saveWorkflowInStore() {
                this.reorderList();
                this.$store.commit('setTempWorkflow', {
                    list: this.activeStepsList, 
                    data: this.workflowData
                });
            },
        },
        created() {
            this.getTeams();
            this.getStatus();
            this.getProfiles();
            this.setEdit();
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
