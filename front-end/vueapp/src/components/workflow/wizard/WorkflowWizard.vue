<template>
    <main>
        <FullscreenLoadingComponent v-if="isLoading" />
        <div class="container-fluid scroll-area mx-4 mt-4">
            <!-- Header -->
            <div class="row align-items-center mb-4">
                <div class="col-auto">
                    <button
                        class="btn btn-outline-primary btn-table btn-sm"
                        @click="redirectToIndex"
                        type="button"
                    >
                        <LucideIcon icon="ArrowLeft" />
                    </button>
                </div>
                <div class="col">
                    <h5 class="mb-0 fw-bold">{{ formTitle }}</h5>
                    <p class="text-muted small mb-0">{{ formSubtitle }}</p>
                </div>
            </div>

            <!-- Phase Navigation -->
            <div class="row mb-4">
                <div class="col-12">
                    <div class="phase-nav d-flex justify-content-center">
                        <div
                            v-for="(phase, index) in phases"
                            :key="index"
                            class="phase-item"
                            :class="{ active: currentPhase === index + 1, completed: index + 1 < currentPhase }"
                        >
                            <div class="phase-circle">
                                <LucideIcon v-if="index + 1 < currentPhase" icon="Check" :size="20" />
                                <span v-else>{{ index + 1 }}</span>
                            </div>
                            <span class="phase-label">{{ phase }}</span>
                            <div v-if="index < phases.length - 1" class="phase-connector"></div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Phase Content -->
            <div class="row">
                <div class="col-12">
                    <div class="main-div shadow-sm">
                        <!-- Phase 1: Name and Teams -->
                        <Phase1NameAndTeams
                            v-if="currentPhase === 1"
                            ref="phase1"
                            :initialData="phase1Data"
                        />

                        <!-- Phase 2: Steps -->
                        <Phase2Steps
                            v-if="currentPhase === 2"
                            ref="phase2"
                            :initialSteps="phase2Data.steps"
                        />

                        <!-- Phase 3: Tools -->
                        <Phase3Tools
                            v-if="currentPhase === 3"
                            ref="phase3"
                            :workflowSteps="phase3Data.steps"
                            :profilesList="profilesList"
                            :phase="currentPhase"
                            @add-tool-flow="handleAddToolFlow"
                            @edit-tool-flow="handleEditToolFlow"
                            @remove-tool-flow="handleRemoveToolFlow"
                        />
                    </div>
                </div>
            </div>

            <!-- Navigation Buttons -->
            <div class="row mt-4">
                <div class="col-12 d-flex justify-content-between">
                    <button
                        v-if="currentPhase > 1"
                        class="btn btn-outline-secondary"
                        @click="previousPhase"
                        type="button"
                    >
                        <LucideIcon icon="ChevronLeft" :size="16" />
                        {{ $t("workflow.previous") }}
                    </button>
                    <div v-else></div>

                    <button
                        v-if="currentPhase < 3"
                        class="btn btn-primary"
                        @click="nextPhase"
                        type="button"
                    >
                        {{ $t("workflow.next") }}
                        <LucideIcon icon="ChevronRight" :size="16" />
                    </button>
                    <button
                        v-else
                        class="btn btn-success"
                        @click="saveWorkflow"
                        type="button"
                    >
                        <LucideIcon icon="Save" :size="16" />
                        {{ isEdit ? $t("workflow.saveChanges") : $t("workflow.createWorkflow") }}
                    </button>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
import { useForm } from "vee-validate";
import Phase1NameAndTeams from "./Phase1NameAndTeams.vue";
import Phase2Steps from "./Phase2Steps.vue";
import Phase3Tools from "./Phase3Tools.vue";
import WorkflowService from "@/services/workflow/WorkflowService";
import ProfilesService from "@/services/profiles/ProfilesService";
import FullscreenLoadingComponent from "@/components/global/FullscreenLoadingComponent.vue";

export default {
    name: "WorkflowWizard",
    components: {
        Phase1NameAndTeams,
        Phase2Steps,
        Phase3Tools,
        FullscreenLoadingComponent,
    },
    props: {
        isEdit: {
            type: Boolean,
            default: false,
        },
        workflowId: {
            type: Number,
            default: null,
        },
    },
    setup() {
        const { validate } = useForm();
        return { validate };
    },
        data() {
            return {
                currentPhase: Number(this.$route.params.phase ?? 1),
                phases: [
                    this.$t("workflow.nameAndAssociations"),
                    this.$t("workflow.steps"),
                    this.$t("workflow.tools"),
                ],
                isLoading: false,
                workflowIdInternal: this.workflowId,
                phase1Data: {
                    name: "",
                    teams: [],
                },
                phase2Data: {
                    steps: [],
                },
                phase3Data: {
                    steps: [],
                },
                profilesList: [],
            };
        },
        //watch: {
        //    currentPhase(currentPhase) {
        //        if (currentPhase === 3) {
        //            loadWorkflowData();
        //        }
        //    }
        //},
    computed: {
        formTitle() {
            return this.isEdit ? "workflow.formEdit.title" : "workflow.formCreate.title";
        },
        formSubtitle() {
            return this.isEdit ? "workflow.formEdit.subtitle" : "workflow.formCreate.subtitle";
        },
    },
    methods: {
        async nextPhase() {
            const isValid = await this.validate();
            if (!isValid.valid) {
                return this.$notify({
                    title: 'workflow.index',
                    message: 'validation.hasInvalid',
                    variant: 'warning',
                    icon: 'CircleAlert',
                });
            }

            if (this.currentPhase === 1) {
                await this.savePhase1();
            } else if (this.currentPhase === 2) {
                await this.savePhase2();
            }
        },
        async previousPhase() {
            if (this.currentPhase > 1) {
                this.currentPhase--;
                // Reload data from backend when navigating back
                await this.reloadCurrentPhaseData();
            }
        },
        async reloadCurrentPhaseData() {
            // Only reload if we have a workflow ID (data exists in database)
            if (!this.workflowIdInternal) return;

            this.isLoading = true;
            try {
                const workflow = await WorkflowService.getWorkflowById(this.workflowIdInternal);
                if (workflow.error) {
                    throw new Error(workflow.error);
                }

                // Update all phase data from database
                this.phase1Data = {
                    name: workflow.name,
                    teams: workflow.teams.map(t => t.id),
                };
                this.phase2Data = {
                    steps: workflow.steps.map(step => ({
                        id: step.id,
                        name: step.name,
                        order: step.order,
                        profileId: String(step.profile?.id || ''),
                        statusId: String(step.status?.id || ''),
                        stepTools: step.stepTools || []
                    }))
                };
                this.phase3Data = {
                    steps: this.phase2Data.steps.map(step => ({
                        ...step,
                        stepTools: step.stepTools || []
                    }))
                };
            } catch (error) {
                this.$notify({
                    title: 'workflow.index',
                    message: 'workflow.loadError',
                    variant: 'danger',
                    icon: 'CircleX',
                });
            } finally {
                this.isLoading = false;
            }
        },
        async savePhase1() {
            this.isLoading = true;
            const phase1Component = this.$refs.phase1;
            const data = phase1Component.getData();

            try {
                if (this.isEdit) {
                    // For edit mode, just store data locally and move to next phase
                    this.phase1Data = data;
                    this.currentPhase = 2;
                } else {
                    // Create workflow with Phase 1 data
                    const workflowId = await WorkflowService.createPhase1(data);
                    if (workflowId.error) {
                        throw new Error(workflowId.error);
                    }
                    this.workflowIdInternal = workflowId;
                    this.phase1Data = data;
                    this.currentPhase = 2;

                    this.$notify({
                        title: 'workflow.index',
                        message: 'workflow.phase1Success',
                        variant: 'success',
                        icon: 'CircleCheckBig',
                    });
                }
            } catch (error) {
                this.$notify({
                    title: 'workflow.index',
                    message: 'workflow.phase1Error',
                    variant: 'danger',
                    icon: 'CircleX',
                });
            } finally {
                this.isLoading = false;
            }
        },
        async savePhase2() {
            this.isLoading = true;
            const phase2Component = this.$refs.phase2;
            const data = phase2Component.getData();

            if (data.steps.length === 0) {
                this.$notify({
                    title: 'workflow.index',
                    message: 'validation.oneStep',
                    variant: 'warning',
                    icon: 'CircleAlert',
                });
                this.isLoading = false;
                return;
            }

            try {
                const params = {
                    workflowId: this.workflowIdInternal,
                    steps: data.steps,
                };

                const result = await WorkflowService.updatePhase2(params);
                if (result.error) {
                    throw new Error(result.error);
                }

                this.phase2Data = data;
                this.currentPhase = 3;
                // Reload data from database to get fresh step data with IDs
                await this.reloadCurrentPhaseData();
                
                this.$notify({
                    title: 'workflow.index',
                    message: 'workflow.phase2Success',
                    variant: 'success',
                    icon: 'CircleCheckBig',
                });
            } catch (error) {
                this.$notify({
                    title: 'workflow.index',
                    message: 'workflow.phase2Error',
                    variant: 'danger',
                    icon: 'CircleX',
                });
            } finally {
                this.isLoading = false;
            }
        },
        async saveWorkflow() {
            this.isLoading = true;
            const phase3Component = this.$refs.phase3;
            const data = phase3Component.getData();

            try {
                const params = {
                    workflowId: this.workflowIdInternal,
                    steps: data.steps,
                };

                const result = await WorkflowService.updatePhase3(params);
                if (result.error) {
                    throw new Error(result.error);
                }

                this.$notify({
                    title: 'workflow.index',
                    message: this.isEdit ? 'workflow.editSuccess' : 'workflow.createSuccess',
                    variant: 'success',
                    icon: 'CircleCheckBig',
                });

                this.redirectToIndex();
            } catch (error) {
                this.$notify({
                    title: 'workflow.index',
                    message: this.isEdit ? 'workflow.editError' : 'workflow.createError',
                    variant: 'danger',
                    icon: 'CircleX',
                });
            } finally {
                this.isLoading = false;
            }
        },
        redirectToIndex() {
            this.$router.push({ name: "WorkflowManagement" });
        },
        handleAddToolFlow(step,phase) {
            // Navigate to flow editor for this step, passing workflow ID
            this.$router.push({
                name: "NewFlow",
                params: { 
                    stepOrder: step.order, 
                    phase: this.currentPhase,
                    workflowId: this.workflowIdInternal
                }
            });
        },
        handleEditToolFlow(step,phase) {
            // Navigate to flow editor for this step, passing workflow ID
            this.$router.push({
                name: "EditFlow",
                params: { 
                    stepOrder: step.order, 
                    phase: this.currentPhase,
                    workflowId: this.workflowIdInternal
                }
            });
        },
        handleRemoveToolFlow(step) {
            const stepIndex = this.phase3Data.steps.findIndex(s => s.id === step.id);
            if (stepIndex !== -1) {
                this.phase3Data.steps[stepIndex].stepTools = [];
            }
        },
        async loadWorkflowData() {
            if (!this.isEdit || !this.workflowId) return;

            this.isLoading = true;
            try {
                const workflow = await WorkflowService.getWorkflowById(this.workflowId);
                if (workflow.error) {
                    throw new Error(workflow.error);
                }

                this.workflowIdInternal = workflow.id;
                this.phase1Data = {
                    name: workflow.name,
                    teams: workflow.teams.map(t => t.id),
                };
                this.phase2Data = {
                    steps: workflow.steps.map(step => ({
                        id: step.id,
                        name: step.name,
                        order: step.order,
                        profileId: String(step.profile?.id || ''),
                        statusId: String(step.status?.id || ''),
                        stepTools: step.stepTools || []
                    }))
                };
                this.phase3Data = {
                    steps: this.phase2Data.steps
                };
            } catch (error) {
                this.$notify({
                    title: 'workflow.index',
                    message: error.message || 'workflow.loadError',
                    variant: 'danger',
                    icon: 'CircleX',
                });
                this.redirectToIndex();
            } finally {
                this.isLoading = false;
            }
        },
        async loadProfiles() {
            try {
                const response = await ProfilesService.getProfilesList();
                if (response.error === undefined) {
                    this.profilesList = response.map(r => ({ id: r.id, text: r.name }));
                }
            } catch (error) {
                console.error("Error loading profiles:", error);
            }
        },
    },
    async created() {
        this.loadProfiles();
        await this.loadWorkflowData();
    },
    async mounted() {
        // Reload data when returning from flow editor or when component is mounted
        if (this.workflowIdInternal || this.isEdit) {
            await this.reloadCurrentPhaseData();
        }
    },
};
</script>

<style scoped>
.container-fluid {
    padding: 0 13px;
}

.main-div {
    border: 1px solid #d3d3d3;
    border-radius: 8px;
    background: white;
    padding: 20px 24px;
    min-height: 400px;
}

/* Phase Navigation Styles */
.phase-nav {
    position: relative;
}

.phase-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    position: relative;
    flex: 1;
    max-width: 200px;
}

.phase-circle {
    width: 48px;
    height: 48px;
    border-radius: 50%;
    background-color: #e5e7eb;
    color: #6b7280;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
    font-size: 18px;
    transition: all 0.3s ease;
    z-index: 2;
}

.phase-item.active .phase-circle {
    background-color: #2F80ED;
    color: white;
}

.phase-item.completed .phase-circle {
    background-color: #10b981;
    color: white;
}

.phase-label {
    margin-top: 8px;
    font-size: 14px;
    font-weight: 500;
    color: #6b7280;
    text-align: center;
}

.phase-item.active .phase-label {
    color: #2F80ED;
    font-weight: 600;
}

.phase-item.completed .phase-label {
    color: #10b981;
}

.phase-connector {
    position: absolute;
    top: 24px;
    left: 50%;
    width: 100%;
    height: 2px;
    background-color: #e5e7eb;
    z-index: 1;
}

.phase-item.completed .phase-connector {
    background-color: #10b981;
}
</style>
