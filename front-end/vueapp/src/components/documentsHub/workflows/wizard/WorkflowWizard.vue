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
                    <h5 class="mb-0 fw-bold">
                        {{ formTitle }}
                    </h5>
                    <p class="text-muted small mb-0">
                        {{ formSubtitle }}
                    </p>
                </div>
            </div>

            <!-- Phase Navigation -->
            <div class="row mb-4">
                <div class="col-12">
                    <div
                        class="phase-nav d-flex justify-content-center"
                    >
                        <div
                            v-for="(phase, index) in phases"
                            :key="index"
                            class="phase-item"
                            :class="{
                                active:
                                    currentPhase ===
                                    index + 1,
                                completed:
                                    index + 1 <
                                    currentPhase,
                            }"
                        >
                            <div class="phase-circle">
                                <LucideIcon
                                    v-if="
                                        index + 1 <
                                        currentPhase
                                    "
                                    icon="Check"
                                    :size="20"
                                />
                                <span v-else>
                                    {{ index + 1 }}
                                </span>
                            </div>
                            <span class="phase-label">
                                {{ phase }}
                            </span>
                            <div
                                v-if="
                                    index <
                                    phases.length - 1
                                "
                                class="phase-connector"
                            ></div>
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
                            :initialData="
                                phase1Data ?? null
                            "
                            :key="phase1Data?.name"
                        />

                        <Phase2Steps
                            v-if="currentPhase === 2"
                            ref="phase2"
                            :initialSteps="
                                phase2Data?.steps ?? []
                            "
                            :key="phase2Data?.steps.length"
                        />

                        <Phase3Tools
                            v-if="currentPhase === 3"
                            ref="phase3"
                            :workflowSteps="
                                phase3Data?.steps ?? []
                            "
                            :key="phase3Data?.steps.length"
                            :profilesList="
                                profilesList ?? []
                            "
                            :phase="currentPhase"
                            @add-tool-flow="
                                handleAddToolFlow
                            "
                            @edit-tool-flow="
                                handleEditToolFlow
                            "
                            @remove-tool-flow="
                                handleRemoveToolFlow
                            "
                            :hasStepsTools="
                                phase3Data?.steps
                                    .hasStepTools
                            "
                        />
                    </div>
                </div>
            </div>

            <!-- Navigation Buttons -->
            <div class="row mt-4 mb-2">
                <div
                    class="col-12 d-flex justify-content-between"
                >
                    <button
                        v-if="currentPhase > 1"
                        class="btn btn-outline-secondary"
                        @click="previousPhase"
                        type="button"
                    >
                        <LucideIcon
                            icon="ChevronLeft"
                            :size="16"
                        />
                        {{ $t("workflow.previous") }}
                    </button>
                    <div v-else></div>

                    <button
                        v-if="currentPhase < 3"
                        class="btn btn-primary"
                        @click="nextPhase"
                        type="button"
                    >
                        {{ $t("workflow.saveStep") }}
                        <LucideIcon
                            icon="Save"
                            :size="16"
                        />
                    </button>
                    <button
                        v-else
                        class="btn btn-success"
                        @click="finalize"
                        type="button"
                    >
                        <LucideIcon
                            icon="Check"
                            :size="16"
                        />
                        {{
                            isEdit
                                ? $t("workflow.finalize")
                                : $t(
                                      "workflow.createWorkflow"
                                  )
                        }}
                    </button>
                </div>
            </div>
        </div>
    </main>
    <ConfirmModal
        id="confirm-leave-wizard-modal"
        :isLoading="isLoading"
        title="common.caution"
        message="workflow.leaveMessage"
        confirmText="common.confirm"
        cancelText="common.cancel"
        confirmVariant="primary"
        iconeName="AlertTriangle"
        iconVariant="warning"
        @confirm="confirmNavigation"
        @cancel="cancelNavigation"
        ref="confirmLeaveModal"
    />
</template>
<script>
    import { useForm } from "vee-validate";
    import Phase1NameAndTeams from "./Phase1NameAndTeams.vue";
    import Phase2Steps from "./Phase2Steps.vue";
    import Phase3Tools from "./Phase3Tools.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import FullscreenLoadingComponent from "@/components/global/FullscreenLoadingComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";

    export default {
        name: "WorkflowWizard",
        components: {
            Phase1NameAndTeams,
            Phase2Steps,
            Phase3Tools,
            FullscreenLoadingComponent,
            ConfirmModal,
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
                currentPhase: Number(
                    this.$route.params.phase ?? 1
                ),
                phases: [
                    this.$t("workflow.nameAndAssociations"),
                    this.$t("workflow.steps"),
                    this.$t("workflow.tools"),
                ],
                isLoading: false,
                workflowIdInternal: this.workflowId,
                phase1Data: null,
                phase2Data: null,
                phase3Data: null,
                profilesList: [],
                canLeave: false,
                pendingNavegation: null,
            };
        },
        computed: {
            formTitle() {
                return this.$t(
                    this.isEdit
                        ? "workflow.formEdit.title"
                        : "workflow.formCreate.title"
                );
            },
            formSubtitle() {
                return this.$t(
                    this.isEdit
                        ? "workflow.formEdit.subtitle"
                        : "workflow.formCreate.subtitle"
                );
            },
        },
        methods: {
            async nextPhase() {
                const isValid = await this.validate();
                if (!isValid.valid) {
                    return this.$notify({
                        title: "workflow.index",
                        message: "validation.hasInvalid",
                        variant: "warning",
                        icon: "CircleAlert",
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
                    this.checkNavigation(() => {
                        this.currentPhase--;
                        this.reloadCurrentPhaseData();
                    });
                }
            },
            async reloadCurrentPhaseData() {
                this.isLoading = true;
                try {
                    await this.loadWorkflowData();
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.loadError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
            async savePhase1() {
                this.isLoading = true;
                const phase1Component = this.$refs.phase1;
                const data = phase1Component.getData();
                let workflowIdInternal =
                    this.workflowIdInternal;
                try {
                    if (
                        workflowIdInternal != null ||
                        this.isEdit
                    ) {
                        this.phase1Data = data;
                        this.currentPhase = 2;
                        const params = {
                            id: this.workflowIdInternal,
                            name: data.name,
                            teams: data.teams,
                        };

                        const result =
                            await WorkflowService.updatePhase1(
                                params
                            );
                        await this.reloadCurrentPhaseData();
                    } else {
                        const workflowId =
                            await WorkflowService.createPhase1(
                                data
                            );
                        if (workflowId.error) {
                            throw new Error(
                                workflowId.error
                            );
                        }
                        this.workflowIdInternal =
                            workflowId;
                        this.phase1Data = data;
                        this.currentPhase = 2;
                        await this.reloadCurrentPhaseData();
                        this.$notify({
                            title: "workflow.index",
                            message:
                                "workflow.phase1Success",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    }
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.phase1Error",
                        variant: "danger",
                        icon: "CircleX",
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
                        title: "workflow.index",
                        message: "validation.oneStep",
                        variant: "warning",
                        icon: "CircleAlert",
                    });
                    this.isLoading = false;
                    return;
                }

                try {
                    const params = {
                        workflowId: this.workflowIdInternal,
                        steps: data.steps,
                    };

                    const result =
                        await WorkflowService.updatePhase2(
                            params
                        );
                    if (result.error) {
                        throw new Error(result.error);
                    }

                    this.phase2Data = data;
                    this.currentPhase = 3;
                    await this.reloadCurrentPhaseData();

                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.phase2Success",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.phase2Error",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
            finalize() {
                this.canLeave = true;
                this.redirectToIndex();
            },
            redirectToIndex() {
                if (!this.canLeave) {
                    this.checkNavigation(() => {
                        this.canLeave = true;
                        this.$router.push({
                            name: "WorkflowManagement",
                        });
                    });
                } else {
                    this.$router.push({
                        name: "WorkflowManagement",
                    });
                }
            },
            checkNavigation(next) {
                this.pendingNavegation = next;
                this.$refs.confirmLeaveModal.open();
            },
            confirmNavigation() {
                this.$refs.confirmLeaveModal.close();
                if (this.pendingNavegation) {
                    this.pendingNavegation();
                    this.pendingNavegation = null;
                }
            },
            cancelNavigation() {
                this.$refs.confirmLeaveModal.close();
                this.pendingNavegation = null;
            },
            handleAddToolFlow(step, phase) {
                this.canLeave = true;
                // Salva dados locais antes de sair
                localStorage.setItem(
                    "wizardPhase1Data",
                    JSON.stringify(this.phase1Data)
                );
                localStorage.setItem(
                    "wizardPhase2Data",
                    JSON.stringify(this.phase2Data)
                );
                localStorage.setItem(
                    "wizardPhase3Data",
                    JSON.stringify(this.phase3Data)
                );
                // Redireciona normalmente
                this.$router.push({
                    name: "NewFlow",
                    params: {
                        stepOrder: step.order,
                        phase: this.currentPhase,
                        workflowId: this.workflowIdInternal,
                        stepId: step.id,
                        hasStepTools: step.hasStepTools,
                    },
                });
            },
            handleEditToolFlow(step, phase) {
                this.canLeave = true;
                localStorage.setItem(
                    "wizardPhase1Data",
                    JSON.stringify(this.phase1Data)
                );
                localStorage.setItem(
                    "wizardPhase2Data",
                    JSON.stringify(this.phase2Data)
                );
                localStorage.setItem(
                    "wizardPhase3Data",
                    JSON.stringify(this.phase3Data)
                );
                this.$router.push({
                    name: "EditFlow",
                    params: {
                        stepOrder: step.order,
                        phase: this.currentPhase,
                        workflowId: this.workflowIdInternal,
                        stepId: step.id,
                        hasStepTools: step.hasStepTools,
                    },
                });
            },
            async handleRemoveToolFlow(step) {
                const phase3Component = this.$refs.phase3;
                let phase3DataResult =
                    await this.getPhase3Data();

                const stepIndex =
                    phase3DataResult.findIndex(
                        (s) => s.id === step.id
                    );
                if (stepIndex !== -1) {
                    phase3DataResult[stepIndex].stepTools =
                        [];
                }

                const params = {
                    workflowId:
                        this.workflowId ??
                        this.$route.params.workflowId,
                    steps: phase3DataResult,
                };

                await WorkflowService.updatePhase3(params)
                    .then((result) => {
                        if (result.error !== undefined) {
                            return this.$notify({
                                title: "flow.title",
                                message:
                                    "flow.formFlow.progressFlowUpdateFail",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        } else {
                            this.reloadCurrentPhaseData();
                            this.$notify({
                                title: "flow.title",
                                message:
                                    "flow.formFlow.progressFlowSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        }
                    })
                    .catch((error) => {
                        this.$notify({
                            title: "flow.title",
                            message:
                                error.message ||
                                "flow.formFlow.progressFlowFail",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    });
            },
            async loadWorkflowData() {
                this.workflowIdInternal =
                    this.workflowIdInternal ??
                    this.$route.params.workflowId;
                this.currentPhase =
                    this.currentPhase ??
                    this.$route.params.phase;
                if (!this.workflowIdInternal) return;
                this.isLoading = true;
                try {
                    if (this.currentPhase == 1) {
                        let result =
                            await this.getPhase1Data();
                        this.phase1Data = {
                            name: result.name,
                            teams: result.teams.map(
                                (t) => t.id
                            ),
                        };
                    } else if (this.currentPhase == 2) {
                        let result =
                            await this.getPhase2Data();
                        this.phase2Data = {
                            steps: result.map((step) => ({
                                id: step.id,
                                name: step.name,
                                order: step.order,
                                profileId: String(
                                    step.profile?.id || ""
                                ),
                                statusId: String(
                                    step.status?.id || ""
                                ),
                                hasStepTools:
                                    step.hasStepTools,
                                isActive: true,
                            })),
                        };
                    } else if (this.currentPhase == 3) {
                        let result =
                            await this.getPhase2Data();
                        this.phase2Data = {
                            steps: result.map((step) => ({
                                id: step.id,
                                name: step.name,
                                order: step.order,
                                profileId: String(
                                    step.profile?.id || ""
                                ),
                                statusId: String(
                                    step.status?.id || ""
                                ),
                                hasStepTools:
                                    step.hasStepTools,
                            })),
                        };
                        this.phase3Data = this.phase2Data;
                    }
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message:
                            error.message ||
                            "workflow.loadError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                    this.redirectToIndex();
                } finally {
                    this.isLoading = false;
                }
            },
            async loadProfiles() {
                try {
                    const response =
                        await ProfilesService.getProfilesList();
                    if (response.error === undefined) {
                        this.profilesList = response.map(
                            (r) => ({
                                id: r.id,
                                text: r.name,
                            })
                        );
                    }
                } catch (error) {
                    console.error(
                        "Error loading profiles:",
                        error
                    );
                }
            },
            async getPhase1Data() {
                const phase1DataReturn =
                    await WorkflowService.getPhase1ById(
                        this.workflowIdInternal
                    );
                if (phase1DataReturn.error) {
                    throw new Error(phase1DataReturn.error);
                }
                return phase1DataReturn;
            },
            async getPhase2Data() {
                const phase2DataReturn =
                    await WorkflowService.getPhase2ById(
                        this.workflowIdInternal
                    );
                if (phase2DataReturn.error) {
                    throw new Error(phase2DataReturn.error);
                }
                return phase2DataReturn;
            },
            async getPhase3Data() {
                const phase3DataReturn =
                    await WorkflowService.getPhase3ById(
                        this.workflowIdInternal
                    );
                if (phase3DataReturn.error) {
                    throw new Error(phase3DataReturn.error);
                }
                return phase3DataReturn;
            },
        },

        created() {
            this.loadProfiles();
            this.loadWorkflowData();
        },
        async mounted() {
            const phase1 = localStorage.getItem(
                "wizardPhase1Data"
            );
            const phase2 = localStorage.getItem(
                "wizardPhase2Data"
            );
            const phase3 = localStorage.getItem(
                "wizardPhase3Data"
            );
            if (phase1 && phase2 && phase3) {
                this.phase1Data = JSON.parse(phase1);
                this.phase2Data = JSON.parse(phase2);
                this.phase3Data = JSON.parse(phase3);
                localStorage.removeItem("wizardPhase1Data");
                localStorage.removeItem("wizardPhase2Data");
                localStorage.removeItem("wizardPhase3Data");
            } else if (
                this.workflowIdInternal ||
                this.isEdit
            ) {
                await this.reloadCurrentPhaseData();
            }
        },
        watch: {
            "$route.params.phase": {
                handler(newPhase) {
                    if (newPhase) {
                        this.currentPhase =
                            Number(newPhase);
                        if (this.workflowIdInternal) {
                            this.reloadCurrentPhaseData();
                        }
                    }
                },
                immediate: false,
            },
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
        background-color: #2f80ed;
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
        color: #2f80ed;
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
