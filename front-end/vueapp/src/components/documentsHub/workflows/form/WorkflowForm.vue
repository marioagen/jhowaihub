<template>
    <main>
        <FullscreenLoadingComponent v-if="isLoading" />
        <div class="container-fluid scroll-area mx-4 mt-4">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="row">
                        <div class="col-2">
                            <button
                                class="btn btn-outline-primary btn-table btn-sm table-btn"
                                @click="redirectToIndex"
                                type="button"
                            >
                                <LucideIcon icon="ArrowLeft" />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">
                                    {{ $t(formTitle) }}
                                </h5>
                                <p>
                                    <small class="text-muted">
                                        {{ $t(formSubtitle) }}
                                    </small>
                                </p>
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
                        <LucideIcon
                            icon="Save"
                            :size="15"
                        />
                        {{ $t("common.save") }}
                    </button>
                </div>
            </div>
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <div class="row">
                        <div class="col">
                            <p>
                                <small class="text-muted">
                                    {{ $t("workflow.manage") }}
                                </small>
                            </p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-12">
                            <label>
                                {{ $t("workflow.name") }}
                            </label>
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
                                <span
                                    class="validation-message text-danger"
                                    v-if="errorMessage"
                                >
                                    {{ errorMessage }}
                                </span>
                            </Field>
                        </div>
                        <div class="col-12">
                            <div v-if="isLoadingTeams">
                                <div class="d-flex justify-content-center">
                                    <div
                                        class="spinner-border text-primary"
                                        role="status"
                                    ></div>
                                </div>
                            </div>
                            <div
                                v-else
                                class="row mt-3"
                            >
                                <Field
                                    name="selectedTeams"
                                    rules="requiredArray"
                                    v-model="selectedTeams"
                                    v-slot="{ errors }"
                                    ref="teamField"
                                >
                                    <div class="row">
                                        <div
                                            v-for="team in teamsList"
                                            :key="team.id"
                                            class="col-3 p-1"
                                        >
                                            <div class="form-check d-flex align-items-center">
                                                <input
                                                    class="form-check-input me-3"
                                                    type="checkbox"
                                                    :id="`team-${team.id}`"
                                                    :value="team.id"
                                                    v-model="selectedTeams"
                                                />
                                                <label
                                                    class="form-check-label fw-semibold"
                                                    :for="`team-${team.id}`"
                                                >
                                                    {{ team.text }}
                                                </label>
                                            </div>
                                        </div>
                                    </div>
                                    <span
                                        class="validation-message text-danger"
                                        v-if="errors.length"
                                    >
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
                        <h6 class="mb-4">
                            {{ $t("workflow.steps") }}
                        </h6>
                    </div>
                    <div class="col-auto">
                        <button
                            class="btn btn-primary btn-sm"
                            type="button"
                            @click="addStep"
                        >
                            <LucideIcon
                                icon="Plus"
                                :size="15"
                            />
                            {{ $t("workflow.createNewStep") }}
                        </button>
                    </div>
                </div>
                <div v-if="isLoadingSteps">
                    <div class="d-flex justify-content-center">
                        <div
                            class="spinner-border text-primary"
                            role="status"
                        ></div>
                    </div>
                </div>
                <div
                    v-else
                    class="row"
                >
                    <div class="d-flex gap-3 overflow-auto flex-nowrap pb-2">
                        <WorkflowStep
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
                            @remove-flow="removeFlow(index, $event)"
                            @saveWorkflow="saveWorkflowInStore"
                            class="workflow-step-card"
                            ref="stepRefs"
                        />
                        <div
                            class="add-step-card text-center p-4 rounded-3 border-dashed flex-shrink-0"
                            @click="addStep"
                        >
                            <div class="icon-circle mb-2">
                                <LucideIcon
                                    icon="Plus"
                                    :size="16"
                                />
                            </div>
                            <h6 class="fw-semibold mb-1">
                                {{ $t("workflow.addBtn") }}
                            </h6>
                            <p class="text-muted small mb-0">
                                {{ $t("workflow.addBtnDescription") }}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>
<script>
    import { Field, useForm } from "vee-validate";
    import WorkflowStep from "@/components/documentsHub/workflows/form/WorkflowStep.vue";
    import TeamsService from "@/services/teams/TeamsService";
    import StatusService from "@/services/status/StatusService";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import FullscreenLoadingComponent from "@/components/global/FullscreenLoadingComponent.vue";

    export default {
        name: "WorkflowBoards",
        components: {
            FullscreenLoadingComponent,
            WorkflowStep,
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
                setValues,
            };
        },
        data() {
            return {
                profilesList: [],
                statusList: [],
                stepsList: [],
                teamsList: [],
                selectedTeams: [],
                workflowData: {
                    name: "",
                },
                isLoading: false,
                isLoadingSteps: false,
                isLoadingTeams: true,
                workflowStepRefs: [],
                tempStepCounter: 1,
            };
        },
        computed: {
            cantSave() {
                return this.stepsList.length === 0;
            },
            formTitle() {
                return this.isEdit
                    ? this.$t("workflow.formEdit.title")
                    : this.$t("workflow.formCreate.title");
            },
            formSubtitle() {
                return this.isEdit
                    ? this.$t("workflow.formEdit.subtitle")
                    : this.$t("workflow.formCreate.subtitle");
            },
            activeStepsList() {
                return this.stepsList.filter((s) => s.isActive !== false);
            },
        },
        methods: {
            getTeams() {
                this.isLoadingTeams = true;
                TeamsService.getTeamList()
                    .then((response) => {
                        if (response.error !== undefined) return;
                        this.teamsList = response.map((r) => ({
                            id: r.id,
                            text: r.name,
                        }));
                    })
                    .finally(() => {
                        this.isLoadingTeams = false;
                    });
            },
            getStatus() {
                StatusService.getStatus().then((response) => {
                    if (response.error !== undefined) return;
                    this.statusList = response;
                });
            },
            getProfiles() {
                ProfilesService.getProfilesList().then((response) => {
                    if (response.error !== undefined) return;
                    this.profilesList = response.map((r) => ({
                        id: r.id,
                        text: r.name,
                    }));
                });
            },
            setEdit() {
                let hasInStore = this.$store.state.tempWorkflow.status;
                if (hasInStore && !this.isEdit) {
                    this.setWorkflowFromStore();
                }

                if (this.isEdit) {
                    this.isLoading = true;
                    WorkflowService.getWorkflowById(this.id)
                        .then((response) => {
                            if (response.error !== undefined) {
                                this.$router.push({
                                    name: "Workflow",
                                });
                                return this.$notify({
                                    title: "workflow.index",
                                    message: response.error,
                                    variant: "danger",
                                    icon: "CircleX",
                                });
                            }
                            this.workflowData.id = response.id;
                            this.workflowData.name = response.name;
                            this.selectedTeams = response.teams.map((team) => team.id);
                            this.stepsList = response.steps.map((step) => ({
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
                this.workflowData.teams = workflowData.teams;
                this.stepsList = this.$store.state.tempWorkflow.list;
            },
            updateStep(index, updatedStep) {
                this.stepsList[index] = {
                    ...this.stepsList[index],
                    ...updatedStep,
                };
            },
            addStep() {
                this.stepsList.push({
                    id: 0,
                    tempId: this.tempStepCounter++,
                    name: "",
                    status: "",
                    order: this.stepsList.length + 1,
                    profile: "",
                    isActive: true,
                    stepTools: [],
                });
            },
            removeStep(order2, step) {
                const i = this.stepsList.findIndex(
                    (s) =>
                        (s.id !== 0 && s.id === step.id) || (s.id === 0 && s.tempId === step.tempId)
                );
                if (i !== -1) {
                    if (step.id === 0) this.stepsList.splice(i, 1);
                    else this.stepsList[i].isActive = false;

                    let order = 1;
                    this.stepsList.forEach((s) => {
                        if (s.isActive) s.order = order++;
                    });
                }
                this.saveTempWorkflow();
            },
            removeFlow(index, stepId) {
                let step = this.stepsList.find((s) => s.id === stepId);
                step.stepTools = [];
                this.updateStep(index, step);
                this.saveTempWorkflow();
            },
            async save() {
                if (!this.stepsList || this.stepsList.length === 0) {
                    return this.$notify({
                        title: "workflow.index",
                        message: "validation.oneStep",
                        variant: "warning",
                        icon: "CircleAlert",
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
                        title: "workflow.index",
                        message: "validation.hasInvalid",
                        variant: "warning",
                        icon: "CircleAlert",
                    });
                }

                this.saveWorkflowInStore();

                this.isLoading = true;
                if (this.isEdit) {
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
                let params = {
                    name: this.workflowData.name,
                    steps: this.$store.state.tempWorkflow.list,
                    teams: this.selectedTeams,
                };

                WorkflowService.createWorkflow(params)
                    .then((response) => {
                        if (response.error === undefined) {
                            this.redirectToIndex();
                            return this.$notify({
                                title: "workflow.index",
                                message: "workflow.createSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        }
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.createError",
                            variant: "danger",
                            icon: "CircleX",
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
                    steps: this.$store.state.tempWorkflow.list,
                    teams: this.selectedTeams,
                };
                WorkflowService.editWorkflow(params)
                    .then((response) => {
                        if (response.error === undefined) {
                            this.redirectToIndex();
                            return this.$notify({
                                title: "workflow.index",
                                message: "workflow.editSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        }
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.editError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            redirectToIndex() {
                this.$store.commit("cleanTempWorkflow");
                return this.$router.push({
                    name: "WorkflowManagement",
                });
            },
            saveWorkflowInStore() {
                this.reorderList();
                const storeList = this.$store.state.tempWorkflow.list;
                if (!storeList || storeList.length === 0) {
                    this.saveTempWorkflow();
                }
            },
            saveTempWorkflow() {
                this.$store.commit("setTempWorkflow", {
                    list: this.stepsList,
                    data: this.workflowData,
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
