<template>
    <main class="workflow-wizard-page">
        <FullscreenLoadingComponent v-if="isLoading" />
        <div class="container-fluid scroll-area">
            <div class="workflow-wizard-page__inner">
            <div class="row align-items-center mb-4 workflow-wizard-page__header">
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
            <div class="row mb-4">
                <div class="col-12">
                    <div class="phase-nav d-flex justify-content-center">
                        <div
                            v-for="(phase, index) in phases"
                            :key="index"
                            class="phase-item"
                            :class="{
                                active: currentPhase === index + 1,
                                completed: index + 1 < currentPhase,
                                'phase-clickable': isEdit,
                            }"
                            @click="goToPhase(index + 1)"
                        >
                            <div class="phase-circle">
                                <LucideIcon
                                    v-if="index + 1 < currentPhase"
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
                                v-if="index < phases.length - 1"
                                class="phase-connector"
                            ></div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-12 min-w-0">
                    <div class="main-div shadow-sm min-w-0">
                        <Phase1NameAndTeams
                            v-if="currentPhase === 1"
                            ref="phase1"
                            :initialData="phase1Data"
                            :key="phase1Data?.name"
                        />

                        <Phase2Steps
                            v-if="currentPhase === 2"
                            ref="phase2"
                            :initialSteps="phase2Data?.steps ?? []"
                            :key="phase2Data?.steps.length"
                        />

                        <Phase3Tools
                            v-if="currentPhase === 3"
                            ref="phase3"
                            :workflowSteps="phase3Data?.steps ?? []"
                            :key="phase3Data?.steps.length"
                            :profilesList="profilesList ?? []"
                            :phase="currentPhase"
                            :workflowId="workflowIdInternal"
                            :isAcknowledging="isAcknowledgingToolUpdate"
                            @add-tool-flow="handleAddToolFlow"
                            @edit-tool-flow="handleEditToolFlow"
                            @remove-tool-flow="handleRemoveToolFlow"
                            @acknowledge-tool-update="handleAcknowledgeToolUpdate"
                            :hasStepsTools="phase3Data?.steps.hasStepTools"
                        />
                    </div>
                </div>
            </div>
            <div class="row mt-4 mb-2 workflow-wizard-page__footer">
                <div class="col-12 d-flex justify-content-between">
                    <button
                        v-if="currentPhase > 1"
                        class="btn btn-outline-secondary"
                        @click="previousPhase"
                        type="button"
                    >
                        <LucideIcon
                            icon="ChevronLeft"
                            :size="16"
                            class="text-muted"
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
                        {{ isEdit ? $t("workflow.finalize") : $t("workflow.createWorkflow") }}
                    </button>
                </div>
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
    <ConfirmModalValidationInput
        id="editValidationConfirm"
        title="documents.editValidationTitle"
        messageKey="documents.editValidationMessage"
        :messageParams="{
            count: documentCountToEdit,
            name: phase1Data?.name || '',
        }"
        cancelText="common.cancel"
        confirmText="documents.confirmEdit"
        :placeholder="$t('documents.editValidationPlaceholder', { name: phase1Data?.name })"
        :validationText="phase1Data?.name || ''"
        confirmVariant="danger"
        iconeName="AlertTriangle"
        iconVariant="warning"
        ref="EditValidationDialog"
        :isLoading="isLoading"
        @confirm="() => executeSavePhase2(null, true)"
    />
    <ConfirmModalValidationInput
        id="removeToolValidationConfirm"
        title="workflow.removeToolValidationTitle"
        messageKey="workflow.removeToolValidationMessage"
        :messageParams="{ name: pendingStepToRemove?.name || '' }"
        cancelText="common.cancel"
        confirmText="workflow.confirmRemoveTool"
        :placeholder="
            $t('workflow.removeToolValidationPlaceholder', { name: pendingStepToRemove?.name })
        "
        :validationText="pendingStepToRemove?.name || ''"
        confirmVariant="danger"
        iconeName="AlertTriangle"
        iconVariant="warning"
        ref="RemoveToolValidationDialog"
        :isLoading="isLoading"
        @confirm="() => executeRemoveToolFlow(null, true)"
    />
</template>
<script>
    import { useForm } from "vee-validate";
    import Phase1NameAndTeams from "./Phase1NameAndTeams.vue";
    import Phase2Steps from "./Phase2Steps.vue";
    import Phase3Tools from "./Phase3Tools.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import ToolsService from "@/services/tools/ToolsServices";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import FullscreenLoadingComponent from "@/components/global/FullscreenLoadingComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import ConfirmModalValidationInput from "@/components/global/ConfirmModalValidationInput.vue";

    export default {
        name: "WorkflowWizard",
        components: {
            Phase1NameAndTeams,
            Phase2Steps,
            Phase3Tools,
            FullscreenLoadingComponent,
            ConfirmModal,
            ConfirmModalValidationInput,
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
            const form = useForm();
            return {
                validate: form.validate,
                meta: form.meta,
            };
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
                phase1Data: null,
                phase2Data: null,
                phase3Data: null,
                profilesList: [],
                canLeave: false,
                pendingNavegation: null,
                documentCountToEdit: 0,
                pendingStepToRemove: null,
                fromKanban: false,
                isAcknowledgingToolUpdate: false,
            };
        },
        computed: {
            formTitle() {
                return this.$t(
                    this.isEdit ? "workflow.formEdit.title" : "workflow.formCreate.title"
                );
            },
            formSubtitle() {
                return this.$t(
                    this.isEdit ? "workflow.formEdit.subtitle" : "workflow.formCreate.subtitle"
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
                        icon: "AlertTriangle",
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
                        if (this.currentPhase === 1 && (this.workflowIdInternal || this.isEdit)) {
                            this.loadPhase1Data();
                        } else if (this.currentPhase === 2 && this.workflowIdInternal) {
                            this.loadPhase2Data();
                        }
                    });
                }
            },
            async goToPhase(newPhase) {
                if (!this.isEdit || newPhase === this.currentPhase) {
                    return;
                }

                if (newPhase < this.currentPhase) {
                    await this.goBackwardToPhase(newPhase);
                } else if (newPhase === this.currentPhase + 1) {
                    await this.nextPhase();
                } else if (newPhase > this.currentPhase) {
                    await this.goForwardToPhase(newPhase);
                }
            },
            async goBackwardToPhase(newPhase) {
                const isValid = await this.validate();
                if (!isValid.valid) {
                    this.showValidationError();
                    return;
                }

                const navigationCallback = () => this.executeNavigation(newPhase);

                if (this.meta.dirty) {
                    this.checkNavigation(navigationCallback);
                } else {
                    navigationCallback();
                }
            },
            async goForwardToPhase(newPhase) {
                const isValid = await this.validate();
                if (!isValid.valid) {
                    this.showValidationError();
                    return;
                }

                await this.saveRequiredPhases(newPhase);

                if (this.currentPhase <= newPhase) {
                    this.executeNavigation(newPhase);
                }
            },
            async saveRequiredPhases(newPhase) {
                if (this.currentPhase === 1) {
                    await this.savePhase1();
                    if (newPhase > 2) {
                        await this.savePhase2();
                    }
                } else if (this.currentPhase === 2) {
                    await this.savePhase2();
                }
            },
            executeNavigation(newPhase) {
                this.currentPhase = newPhase;
                this.loadPhaseData(newPhase);
            },
            loadPhaseData(phase) {
                if (phase === 1) {
                    this.loadPhase1Data();
                } else if (phase === 2) {
                    this.loadPhase2Data();
                } else if (phase === 3) {
                    this.loadPhase3Data();
                }
            },
            showValidationError() {
                this.$notify({
                    title: "workflow.index",
                    message: "validation.hasInvalid",
                    variant: "warning",
                    icon: "AlertTriangle",
                });
            },
            async savePhase1() {
                this.isLoading = true;
                const phase1Component = this.$refs.phase1;
                const data = phase1Component.getData();
                let workflowIdInternal = this.workflowIdInternal;
                try {
                    if (workflowIdInternal != null || this.isEdit) {
                        this.phase1Data = data;
                        this.currentPhase = 2;
                        const params = {
                            id: this.workflowIdInternal,
                            name: data.name,
                            description: data.description ?? "",
                            teams: data.teams,
                        };

                        const result = await WorkflowService.updatePhase1(params);
                        await this.loadPhase2Data();
                    } else {
                        const workflowId = await WorkflowService.createPhase1(data);
                        if (workflowId.error) {
                            throw new Error(workflowId.error);
                        }
                        this.workflowIdInternal = workflowId;
                        this.phase1Data = data;
                        this.currentPhase = 2;
                        await this.loadPhase2Data();
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.phase1Success",
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
                const hasRemovedOriginalSteps = phase2Component.hasRemovedOriginalSteps();

                if (data.steps.length === 0) {
                    this.$notify({
                        title: "workflow.index",
                        message: "validation.oneStep",
                        variant: "warning",
                        icon: "AlertTriangle",
                    });
                    this.isLoading = false;
                    return;
                }

                if (this.workflowIdInternal && hasRemovedOriginalSteps) {
                    WorkflowService.countDocuments(this.workflowIdInternal)
                        .then((count) => {
                            if (count > 0) {
                                this.documentCountToEdit = count;
                                this.isLoading = false;
                                this.$refs.EditValidationDialog.open();
                            } else {
                                this.executeSavePhase2(data);
                            }
                        })
                        .catch(() => {
                            this.isLoading = false;
                            this.$notify({
                                title: "workflow.index",
                                message: "documents.errors.removeError",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        });
                } else {
                    this.executeSavePhase2(data);
                }
            },
            async executeSavePhase2(dataParam, resetDocuments = false) {
                this.isLoading = true;
                this.$refs.EditValidationDialog?.close();
                const data = dataParam?.steps ? dataParam : this.$refs.phase2.getData();

                try {
                    const params = {
                        workflowId: this.workflowIdInternal,
                        steps: data.steps,
                        resetDocuments: resetDocuments,
                    };

                    const result = await WorkflowService.updatePhase2(params);
                    if (result.error) {
                        throw new Error(result.error);
                    }

                    this.phase2Data = data;
                    this.currentPhase = 3;
                    await this.loadPhase3Data();

                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.phase2Success",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                } catch {
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
                        this.$router.push(this.resolveRedirectToIndexRoute());
                    });
                } else {
                    this.canLeave = true;
                    this.$router.push(this.resolveRedirectToIndexRoute());
                }
            },
            resolveRedirectToIndexRoute() {
                if (this.fromKanban && this.workflowIdInternal) {
                    return {
                        name: "Workflow",
                        query: {
                            id: this.workflowIdInternal,
                            tab: "workflows",
                        },
                    };
                }
                return { name: "WorkflowPage" };
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
                localStorage.setItem("wizardPhase1Data", JSON.stringify(this.phase1Data));
                localStorage.setItem("wizardPhase2Data", JSON.stringify(this.phase2Data));
                localStorage.setItem("wizardPhase3Data", JSON.stringify(this.phase3Data));
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
                localStorage.setItem("wizardPhase1Data", JSON.stringify(this.phase1Data));
                localStorage.setItem("wizardPhase2Data", JSON.stringify(this.phase2Data));
                localStorage.setItem("wizardPhase3Data", JSON.stringify(this.phase3Data));
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
                this.isLoading = true;
                try {
                    const documentsCount = this.workflowIdInternal
                        ? await WorkflowService.countDocuments(this.workflowIdInternal)
                        : 0;
                    if (documentsCount > 0) {
                        this.pendingStepToRemove = step;
                        this.$refs.RemoveToolValidationDialog.open();
                    } else {
                        await this.executeRemoveToolFlow(step, false);
                    }
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.removeError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
            async handleAcknowledgeToolUpdate(workflowId) {
                if (!workflowId) return;
                this.isAcknowledgingToolUpdate = true;
                try {
                    const success = await ToolsService.acknowledgeToolUpdate(workflowId);
                    if (success) {
                        await this.loadPhase3Data();
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.toolUpdateAcknowledged",
                            variant: "success",
                            icon: "CircleCheck",
                        });
                    } else {
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.toolUpdateAcknowledgeError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                } catch {
                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.toolUpdateAcknowledgeError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isAcknowledgingToolUpdate = false;
                }
            },
            async executeRemoveToolFlow(stepParam = null, resetDocuments = false) {
                this.$refs.RemoveToolValidationDialog?.close();
                const step = stepParam || this.pendingStepToRemove;
                if (!step) return;

                this.isLoading = true;
                try {
                    let phase3DataResult = await this.getPhase3Data();

                    const stepIndex = phase3DataResult.findIndex((s) => s.id === step.id);
                    if (stepIndex !== -1) {
                        phase3DataResult[stepIndex].stepTools = [];
                    }

                    const params = {
                        workflowId: this.workflowId ?? this.$route.params.workflowId,
                        steps: phase3DataResult,
                        resetDocuments: resetDocuments,
                    };

                    const result = await WorkflowService.updatePhase3(params);
                    if (result && result.error !== undefined) {
                        return this.$notify({
                            title: "flow.title",
                            message: "flow.formFlow.progressFlowUpdateFail",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    } else {
                        this.loadPhase3Data();
                        this.$notify({
                            title: "flow.title",
                            message: "flow.formFlow.progressFlowSuccess",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    }
                } catch (error) {
                    this.$notify({
                        title: "flow.title",
                        message: error.message || "flow.formFlow.progressFlowFail",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.pendingStepToRemove = null;
                    this.isLoading = false;
                }
            },
            async loadWorkflowData() {
                this.workflowIdInternal = this.workflowIdInternal ?? this.$route.params.workflowId;
                this.currentPhase = this.currentPhase ?? this.$route.params.phase;

                if (!this.workflowIdInternal && !this.isEdit) {
                    return;
                }

                this.isLoading = true;
                try {
                    if (this.currentPhase == 1) {
                        if (this.workflowIdInternal || this.isEdit) {
                            let result = await this.getPhase1Data();
                            this.phase1Data = {
                                name: result.name,
                                description: result.description ?? "",
                                teams: result.teams.map((t) => t.id),
                            };
                        }
                    } else if (this.currentPhase == 2) {
                        let result = await this.getPhase2Data();
                        this.phase2Data = {
                            steps: result.map((step) => ({
                                id: step.id,
                                name: step.name,
                                order: step.order,
                                profileId: String(step.profile?.id || ""),
                                statusId: String(step.status?.id || ""),
                                hasStepTools: step.hasStepTools,
                                isActive: true,
                            })),
                        };
                    } else if (this.currentPhase == 3) {
                        await this.loadPhase3Data();
                    }
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message: error.message || "workflow.loadError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                    this.redirectToIndex();
                } finally {
                    this.isLoading = false;
                }
            },
            async loadProfiles() {
                const response = await ProfilesService.getProfilesList();
                if (response.error === undefined) {
                    this.profilesList = response.map((r) => ({
                        id: r.id,
                        text: r.name,
                    }));
                }
            },
            async getPhase1Data() {
                const phase1DataReturn = await WorkflowService.getPhase1ById(
                    this.workflowIdInternal
                );
                if (phase1DataReturn.error) {
                    throw new Error(phase1DataReturn.error);
                }
                return phase1DataReturn;
            },
            async getPhase2Data() {
                const phase2DataReturn = await WorkflowService.getPhase2ById(
                    this.workflowIdInternal
                );
                if (phase2DataReturn.error) {
                    throw new Error(phase2DataReturn.error);
                }
                return phase2DataReturn;
            },
            async getPhase3Data() {
                const phase3DataReturn = await WorkflowService.getPhase3ById(
                    this.workflowIdInternal
                );
                if (phase3DataReturn.error) {
                    throw new Error(phase3DataReturn.error);
                }
                return phase3DataReturn;
            },
            async loadPhase1Data() {
                if (!this.workflowIdInternal && !this.isEdit) return;
                this.isLoading = true;
                try {
                    let result = await this.getPhase1Data();
                    this.phase1Data = {
                        name: result.name,
                        description: result.description ?? "",
                        teams: result.teams.map((t) => t.id),
                    };
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message: error.message || "workflow.loadError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
            async loadPhase2Data() {
                if (!this.workflowIdInternal) return;
                this.isLoading = true;
                try {
                    let result = await this.getPhase2Data();
                    this.phase2Data = {
                        steps: result.map((step) => ({
                            id: step.id,
                            name: step.name,
                            order: step.order,
                            profileId: String(step.profile?.id || ""),
                            statusId: String(step.status?.id || ""),
                            hasStepTools: step.hasStepTools,
                            isActive: true,
                        })),
                    };
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message: error.message || "workflow.loadError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
            mapPhase3Steps(steps) {
                return (steps || []).map((step) => ({
                    id: step.id,
                    name: step.name,
                    order: step.order,
                    profileId: String(step.profile?.id || step.profileId || ""),
                    statusId: String(step.status?.id || step.statusId || ""),
                    hasStepTools: step.hasStepTools || (step.stepTools?.length ?? 0) > 0,
                    stepTools: step.stepTools || [],
                    isActive: step.isActive !== false,
                }));
            },
            async loadPhase3Data() {
                if (!this.workflowIdInternal) return;
                this.isLoading = true;
                try {
                    await this.loadProfiles();

                    const result = await this.getPhase3Data();
                    const steps = Array.isArray(result) ? result : result?.steps || [];
                    this.phase3Data = { steps: this.mapPhase3Steps(steps) };
                } catch (error) {
                    this.$notify({
                        title: "workflow.index",
                        message: error.message || "workflow.loadError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
        },
        created() {
            this.loadWorkflowData();
        },
        async mounted() {
            if (this.$route.query.from === "kanban") {
                this.fromKanban = true;
            }
            const phase1 = localStorage.getItem("wizardPhase1Data");
            const phase2 = localStorage.getItem("wizardPhase2Data");
            const phase3 = localStorage.getItem("wizardPhase3Data");
            if (phase1 && phase2 && phase3) {
                this.phase1Data = JSON.parse(phase1);
                this.phase2Data = JSON.parse(phase2);
                this.phase3Data = JSON.parse(phase3);
                localStorage.removeItem("wizardPhase1Data");
                localStorage.removeItem("wizardPhase2Data");
                localStorage.removeItem("wizardPhase3Data");
            }
        },
        watch: {
            "$route.params.phase": {
                handler(newPhase) {
                    if (newPhase) {
                        this.currentPhase = Number(newPhase);
                        if (this.workflowIdInternal) {
                            if (this.currentPhase === 1) {
                                this.loadPhase1Data();
                            } else if (this.currentPhase === 2) {
                                this.loadPhase2Data();
                            } else if (this.currentPhase === 3) {
                                this.loadPhase3Data();
                            }
                        }
                    }
                },
                immediate: false,
            },
        },
    };
</script>
<style scoped>
    .workflow-wizard-page {
        width: 100%;
        min-width: 0;
        flex: 1;
        display: flex;
        flex-direction: column;
        overflow: hidden;
        height: calc(100vh - 58px);
        max-height: calc(100vh - 58px);
    }

    .scroll-area {
        flex: 1;
        width: 100%;
        max-width: 100%;
        height: 100%;
        overflow-y: auto;
        overflow-x: hidden;
        padding: 1.25rem 0 2rem;
        box-sizing: border-box;
    }

    .workflow-wizard-page__inner {
        width: 100%;
        max-width: 1200px;
        margin: 0 auto;
        padding-left: 2rem;
        padding-right: 2rem;
        box-sizing: border-box;
    }

    .workflow-wizard-page__inner :deep(.row) {
        --bs-gutter-x: 1.5rem;
        margin-left: 0;
        margin-right: 0;
    }

    .container-fluid {
        width: 100%;
        max-width: 100%;
        padding: 0;
        box-sizing: border-box;
    }

    .main-div {
        min-height: 400px;
        min-width: 0;
        background: var(--bs-body-bg, #fff);
        border: 1px solid var(--bs-border-color, #dee2e6);
        border-radius: 0.5rem;
    }

    .phase-nav {
        position: relative;
        width: 100%;
        max-width: 760px;
        margin: 0 auto;
        padding: 0 0.5rem;
        box-sizing: border-box;
    }

    .phase-item {
        display: flex;
        flex-direction: column;
        align-items: center;
        position: relative;
        flex: 1 1 0;
        min-width: 0;
        max-width: none;
        padding: 0 0.25rem;
    }

    .phase-item.phase-clickable {
        cursor: pointer;
    }

    .phase-item.phase-clickable:hover:not(.active) .phase-circle {
        background-color: var(--color-bg-phase-circle-hover);
        color: var(--color-bg-phase-circle-active);
        transform: scale(1.05);
    }

    .phase-circle {
        width: 48px;
        height: 48px;
        border-radius: 50%;
        background-color: var(--color-bg-phase-circle);
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
        background-color: var(--color-bg-phase-circle-success) !important;
        color: white;
    }

    .phase-label {
        margin-top: 8px;
        font-size: 13px;
        font-weight: 500;
        color: #6b7280;
        text-align: center;
        line-height: 1.35;
        max-width: 100%;
        padding: 0 2px;
        word-break: break-word;
    }

    .phase-item.active .phase-label {
        color: var(--color-bg-phase-circle-active);
        font-weight: 600;
    }

    .phase-item.completed .phase-label {
        color: var(--color-bg-phase-circle-success) !important;
    }

    .phase-connector {
        position: absolute;
        top: 24px;
        left: 50%;
        width: 100%;
        height: 2px;
        background-color: var(--color-bg-phase-circle) !important;
        z-index: 1;
    }

    .phase-item.completed .phase-connector {
        background-color: var(--color-bg-phase-circle-success) !important;
    }
    @media (max-width: 768px) {
        .workflow-wizard-page__inner {
            padding-left: 1.25rem;
            padding-right: 1.25rem;
        }
    }
</style>
