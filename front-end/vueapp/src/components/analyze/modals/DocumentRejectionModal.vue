<template>
    <ModalComponent
        id="modalReject"
        :isLoading="loading"
        @save="confirm"
        ref="ModalReject"
    >
        <template #header>
            <div class="modal-header border-0 pb-0">
                <div>
                    <h5 class="modal-title fw-bold">
                        <i class="fas fa-times-circle text-danger me-2"></i>
                        {{ $t("analyze.rejection.title") }}
                    </h5>
                    <small class="text-muted d-block">
                        {{ $t("analyze.rejection.justificationInstructions") }}
                    </small>
                </div>
                <button
                    class="btn-close"
                    data-bs-dismiss="modal"
                    @click="close"
                />
            </div>
        </template>
        <template #body>
            <div class="modal-body">
                <div
                    v-if="initialLoading"
                    class="py-5"
                >
                    <LoadingComponent />
                </div>
                <template v-else>
                    <div class="mb-3">
                        <label
                            for="justification"
                            class="form-label"
                        >
                            {{ $t("analyze.rejection.justification") }}
                            <span class="text-danger">*</span>
                        </label>
                        <Field
                            name="justification"
                            rules="required"
                            v-slot="{ field, errorMessage }"
                        >
                            <textarea
                                v-bind="field"
                                class="form-control"
                                id="justification"
                                rows="3"
                                :class="{ 'is-invalid': errorMessage }"
                                :placeholder="$t('analyze.rejection.justificationPlaceholder')"
                            ></textarea>
                            <span
                                v-if="errorMessage"
                                class="validation-message text-danger"
                            >
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                    <div class="mb-3">
                        <label
                            for="returnStep"
                            class="form-label"
                        >
                            {{ $t("analyze.rejection.returnToStep") }}
                            <span class="text-danger">*</span>
                        </label>
                        <Field
                            name="selectedStepId"
                            rules="required"
                            v-slot="{ field, errorMessage }"
                        >
                            <select
                                v-bind="field"
                                class="form-select"
                                id="returnStep"
                                :class="{ 'is-invalid': errorMessage }"
                            >
                                <option value="">{{ $t("analyze.rejection.selectStep") }}</option>
                                <option
                                    v-for="step in steps"
                                    :key="step.id"
                                    :value="step.id"
                                >
                                    {{ step.name }}
                                </option>
                            </select>
                            <span
                                v-if="errorMessage"
                                class="validation-message text-danger"
                            >
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                    <div class="mb-2 document-rejection-user-picker">
                        <label
                            class="form-label fw-semibold mb-1 d-flex align-items-center flex-wrap gap-2"
                        >
                            <span>{{ $t("analyze.rejection.assignTo") }}</span>
                            <span class="text-muted small fw-normal">
                                {{ $t("analyze.rejection.assignOptional") }}
                            </span>
                        </label>
                        <SelectionListComponent
                            id="rejection-assign-user"
                            type="user-list"
                            :items="fetchedUsers"
                            :loading="loadingUsers"
                            :list-height="'200px'"
                            :compact="true"
                            :max-selections="1"
                            :hide-bulk-toolbar="true"
                            :show-label="false"
                            :selection-chip-label-key="'analyze.rejection.assignedTo'"
                            label-panel="management.users.title"
                            label-selected-quantity="management.users.selectedUsers"
                            :label-search="'management.users.searchUsers'"
                            v-model:selected-items="selectedUserIds"
                        />
                    </div>
                </template>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer justify-content-between">
                <button
                    type="button"
                    class="btn btn-light"
                    @click="close"
                >
                    {{ $t("analyze.rejection.cancel") }}
                </button>
                <button
                    type="button"
                    class="btn btn-danger"
                    @click="confirm"
                    :disabled="initialLoading || loading"
                >
                    {{ $t("analyze.rejection.confirm") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>
<script>
    import { Field, useForm } from "vee-validate";
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import SelectionListComponent from "@/components/global/SelectionListComponent.vue";
    import AnalysisRejectionServices from "@/services/documents/AnalysisRejectionServices";
    import LogService from "@/services/log/logService";
    import UserService from "@/services/users/UserService";
    import WorkflowService from "@/services/workflow/WorkflowService";

    export default {
        name: "DocumentRejectionModal",
        components: {
            ModalComponent,
            LoadingComponent,
            Field,
            SelectionListComponent,
        },
        setup() {
            const { validate, setValues, values, resetForm } = useForm();
            return { validate, setValues, values, resetForm };
        },
        props: {
            cardIds: {
                type: Array,
                default: null,
            },
            cardId: {
                type: [Number, String],
                default: null,
            },
            documentId: {
                type: [Number, String],
                default: null,
            },
        },
        data() {
            return {
                steps: [],
                loading: false,
                loadingSteps: false,
                loadingUsers: false,
                fetchedUsers: [],
                selectedUserIds: [],
            };
        },
        computed: {
            initialLoading() {
                return this.loadingSteps || this.loadingUsers;
            },
            effectiveCardIds() {
                if (this.cardIds && this.cardIds.length > 0) {
                    return this.cardIds.map((id) => Number(id));
                }
                if (this.cardId == null || this.cardId === "") {
                    return [];
                }
                return [Number(this.cardId) || parseInt(this.cardId, 10)];
            },
        },
        methods: {
            async fetchSteps(workflowId) {
                const firstCardId = this.effectiveCardIds[0];
                if (workflowId == null || firstCardId == null) {
                    this.steps = [];
                    return;
                }
                try {
                    this.loadingSteps = true;
                    const response = await AnalysisRejectionServices.findWorkflowPreviousSteps(
                        workflowId,
                        firstCardId
                    );
                    if (response && Array.isArray(response)) {
                        this.steps = response;
                    }
                } catch (error) {
                    LogService.showMessage("Error fetching steps: " + error);
                } finally {
                    this.loadingSteps = false;
                }
            },
            open(workflowId = null) {
                this.selectedUserIds = [];
                this.fetchedUsers = [];
                this.steps = [];
                this.resetForm({
                    values: {
                        justification: "",
                        selectedStepId: "",
                    },
                });
                this.$refs.ModalReject.open();
                void Promise.all([
                    this.fetchSteps(workflowId),
                    this.ensureUsersForPicker(workflowId),
                ]);
            },
            async ensureUsersForPicker(workflowId) {
                if (workflowId == null || workflowId === 0) {
                    this.fetchedUsers = [];
                    return;
                }
                this.loadingUsers = true;
                try {
                    // Use Phase1 endpoint because we only need team ids for the assignee list.
                    // getWorkflowById returns full workflow data and is significantly heavier.
                    const workflow = await WorkflowService.getPhase1ById(workflowId);
                    if (workflow?.error || !workflow?.teams?.length) {
                        this.fetchedUsers = [];
                        return;
                    }
                    const teamIds = workflow.teams.map((t) => t.id);
                    const list = await UserService.getUsersByTeamIds(teamIds);
                    this.fetchedUsers = Array.isArray(list) ? list : [];
                } catch (e) {
                    LogService.showMessage("Error loading users for rejection modal: " + e);
                    this.fetchedUsers = [];
                } finally {
                    this.loadingUsers = false;
                }
            },
            close() {
                this.$refs.ModalReject.close();
                this.$emit("close");
            },
            selectedAssigneeUserId() {
                if (!this.selectedUserIds.length) return null;
                const raw = this.selectedUserIds[0];
                const user = this.fetchedUsers.find((u) => u.id === raw);
                return user?.id ?? raw;
            },
            async confirm() {
                const result = await this.validate();
                if (!result.valid) {
                    return LogService.showMessage(this.$t("analyze.reject.validationError"));
                }

                const stepId =
                    Number(this.values.selectedStepId) || parseInt(this.values.selectedStepId, 10);
                const justification = this.values.justification;
                const ids = this.effectiveCardIds;
                if (ids.length === 0) {
                    return LogService.showMessage(this.$t("analyze.reject.validationError"));
                }

                const assigneeId = this.selectedAssigneeUserId();

                try {
                    this.loading = true;
                    const payload = {
                        justification,
                        stepId,
                        cardIds: ids,
                        userId: assigneeId ?? null,
                    };
                    const response = await AnalysisRejectionServices.createRejectionRange(payload);
                    if (response && !response.error && response !== false) {
                        this.$emit("success");
                        this.close();
                        this.$notify({
                            title: "analyze.rejection.title",
                            message: "analyze.rejection.success",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    } else {
                        this.$notify({
                            title: "analyze.rejection.title",
                            message: "analyze.rejection.error",
                            variant: "danger",
                            icon: "CircleXBig",
                        });
                    }
                } catch (error) {
                    this.$notify({
                        title: "analyze.rejection.title",
                        message: "analyze.rejection.error",
                        variant: "danger",
                        icon: "CircleXBig",
                    });
                } finally {
                    this.loading = false;
                }
            },
        },
    };
</script>
<style scoped>
    .document-rejection-user-picker :deep(.mt-3) {
        margin-top: 0 !important;
    }
</style>
