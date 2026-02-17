<template>
    <ModalComponent id="modalReject" :isLoading="loading" @save="confirm" ref="ModalReject">
        <template #header>
            <div class="modal-header border-0 pb-0">
                <div>
                    <h5 class="modal-title fw-bold">
                        <i class="fas fa-times-circle text-danger me-2"></i>
                        {{ $t('analyze.reject.title') }}
                    </h5>
                    <small class="text-muted d-block">{{ $t('analyze.reject.justificationInstructions') }}</small>
                </div>
                <button class="btn-close" data-bs-dismiss="modal" @click="close" />
            </div>
        </template>
        <template #body>
            <div class="modal-body">
                <div class="mb-3">
                    <label for="justification" class="form-label">
                        {{ $t('analyze.reject.justification') }} <span class="text-danger">*</span>
                    </label>
                    <Field name="justification" rules="required" v-slot="{ field, errorMessage }">
                        <textarea v-bind="field" class="form-control" id="justification" rows="3"
                            :class="{ 'is-invalid': errorMessage }"
                            :placeholder="$t('analyze.reject.justificationPlaceholder')"></textarea>
                        <span v-if="errorMessage" class="validation-message text-danger">
                            {{ errorMessage }}
                        </span>
                    </Field>
                </div>
                <div class="mb-3">
                    <label for="returnStep" class="form-label">
                        {{ $t('analyze.reject.returnToStep') }} <span class="text-danger">*</span>
                    </label>
                    <Field name="selectedStepId" rules="required" v-slot="{ field, errorMessage }">
                        <select v-bind="field" class="form-select" id="returnStep"
                            :class="{ 'is-invalid': errorMessage }">
                            <option value="">{{ $t('analyze.reject.selectStep') }}</option>
                            <option v-for="step in steps" :key="step.id" :value="step.id">
                                {{ step.name }}
                            </option>
                        </select>
                        <span v-if="errorMessage" class="validation-message text-danger">
                            {{ errorMessage }}
                        </span>
                    </Field>
                </div>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer justify-content-between">
                <button type="button" class="btn btn-light" @click="close">
                    {{ $t('analyze.reject.cancel') }}
                </button>
                <button type="button" class="btn btn-danger" @click="confirm" :disabled="loading">
                    {{ $t('analyze.reject.confirm') }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>

<script>
import { Field, useForm } from "vee-validate";
import ModalComponent from "@/components/global/ModalComponent.vue";
import AnalysisRejectionServices from "@/services/documents/AnalysisRejectionServices";
import LogService from '@/services/log/logService';

export default {
    name: "ModalReject",
    components: {
        ModalComponent,
        Field,
    },
    setup() {
        const { validate, setValues, values, resetForm } = useForm();
        return { validate, setValues, values, resetForm };
    },
    props: {
        cardId: {
            type: [Number, String],
            required: true,
        },
        documentId: {
            type: [Number, String],
            required: true
        }
    },
    data() {
        return {
            steps: [],
            loading: false,
        };
    },
    methods: {
        async fetchSteps(workflowId) {
            try {
                this.loading = true;
                const response = await AnalysisRejectionServices.findWorkflowPreviousSteps(workflowId, this.cardId);
                if (response && Array.isArray(response)) {
                    this.steps = response;
                }
            } catch (error) {
                LogService.showMessage("Error fetching steps: " + error);
            } finally {
                this.loading = false;
            }
        },
        open(workflowId = null) {
            this.resetForm({
                values: {
                    justification: "",
                    selectedStepId: ""
                }
            });
            this.fetchSteps(workflowId);
            this.$refs.ModalReject.open();
        },
        close() {
            this.$refs.ModalReject.close();
            this.$emit("close");
        },
        async confirm() {
            const result = await this.validate();
            if (!result.valid) {
                return LogService.showMessage(this.$t('analyze.reject.validationError'));
            }

            const params = {
                cardId: this.cardId,
                stepId: this.values.selectedStepId,
                justification: this.values.justification,
            };
            console.log(params);
            try {
                this.loading = true;
                const response = await AnalysisRejectionServices.rejectAnalysis(params);
                if (response && !response.error) {
                    this.$emit("success");
                    this.close();
                } else {
                    LogService.showMessage("Error rejecting document: " + (response.error || "Unknown error"));
                }
            } catch (error) {
                LogService.showMessage("Error rejecting document: " + error);
            } finally {
                this.loading = false;
            }
        },
    },
};
</script>
