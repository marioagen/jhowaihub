<template>
    <main>
        <FullscreenLoadingComponent v-if="isLoading" />
        <div
            class="container-fluid scroll-area mx-4 mt-4"
        >
            <div class="row align-items-center">
                <div class="col-auto">                    
                    <div class="row">
                        <div class="col">
                            <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="redirectToIndex" type="button">
                                <LucideIcon icon="ArrowLeft" />
                                {{ $t("labelBack") }}
                            </button>
                        </div>
                        <div class="col-8">
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
                        <LucideIcon icon="Save" size="15" />
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
                            <Field name="name" rules="required" v-slot="{ field, errorMessage }" ref="nameField">
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
                                    <LucideIcon icon="Users" size="16" />
                                </span>

                                <Field name="teamId" rules="required" v-slot="{ field, errors }" ref="teamField">
                                    <select
                                        id="typeDocId"
                                        class="form-select form-select-sm border-start-0"
                                        v-bind="field"
                                    >
                                        <option value="">{{ $t("workflow.responsableTeam") }}</option>
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
                            <LucideIcon icon="Plus" size="15" />
                            {{ $t("workflow.createNewStep") }}
                        </button>
                    </div>
                </div>
                <div class="row">
                    <div class="d-flex gap-3 overflow-auto flex-nowrap pb-2">
                        <WorkflowStepComponent
                            v-for="(step, index) in stepsList"
                            :key="step.id || index"
                            :step="step"
                            :index="index + 1"
                            :is-last="index === stepsList.length - 1" 
                            :profilesList="profilesList"
                            :statusList="statusList"
                            @update-step="updateStep(index, $event)"
                            @remove-step="removeStep(index)"
                            class="workflow-step-card"
                            ref="stepRefs"
                        />
                        
                        <div class="add-step-card text-center p-4 rounded-3 border-dashed flex-shrink-0" @click="addStep">
                            <div class="icon-circle mb-2">
                                <LucideIcon icon="Plus" size="16" />
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
    import { Field, Form, useForm } from "vee-validate";
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
            Form
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
        watch: {
            "$store.state.userProfile.language": function () {
                this.setCrumbsData();
            },
        },
        setup() {
            const { validate, values } = useForm();
            return {
                validate,
                values
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
                if(!this.isEdit) return;
                this.isLoading = true;
                console.log(this.id)
                WorkflowService.getWorkflowById(this.id)
                    .then((response) => {
                        console.log(response)
                        if(response.error !== undefined) {
                            this.$router.push({ name: "Workflow" });
                            return this.$notify({
                                title: 'Workflow',
                                message: response.error,
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        this.workflowData.id = response.id;
                        this.workflowData.name = response.name;
                        this.workflowData.teamId = response.teamId;
                        this.stepsList = response.steps;
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            updateStep(index, updatedStep) {
                this.stepsList[index] = { ...this.stepsList[index], ...updatedStep };
            },
            addStep() {
                this.stepsList.push({
                    id: crypto.randomUUID?.() || Date.now() + Math.random(),
                    name: '',
                    status: '',
                    profile: '',
                });
            },
            removeStep(index) {
                this.stepsList.splice(index, 1);
            },
            async save() {
                if (!this.stepsList || this.stepsList.length === 0) {
                    return this.$notify({
                        title: 'Workflow',
                        message: 'Add at least one step before saving.',
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
                        title: 'Workflow',
                        message: 'Campos inválidos',
                        variant: 'warning',
                        icon: 'CircleAlert',
                    });
                }
                
                this.isLoading = true;
                if(this.isEdit) {
                    return this.editWorkflow();
                }
                return this.createWorkflow();
            },
            createWorkflow() {
                let params = {
                    name: this.values.name,
                    teamId: this.values.teamId,
                    steps: this.stepsList
                };
                
                WorkflowService.createWorkflow(params)
                    .then((response) => {
                        if(response.error === undefined) {
                            this.redirectToIndex();
                            return this.$notify({
                                title: 'Workflow',
                                message: 'workflow.createSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        }
                        this.$notify({
                            title: 'Workflow',
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
                let params = {
                    id: this.workflowData.id,
                    name: this.workflowData.name,
                    teamId: this.workflowData.teamId,
                    steps: this.stepsList
                };

                WorkflowService.editWorkflow(params)
                    .then((response) => {
                        if(response === undefined) {
                            this.redirectToIndex();
                            return this.$notify({
                                title: 'Workflow',
                                message: 'workflow.editSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        }
                        this.$notify({
                            title: 'Workflow',
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
                return this.$router.push({ name: "WorkflowEditor" });
            },
        },
        created() {
            console.log(this.stepsList)
            console.log(this.stepsList.length)
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
