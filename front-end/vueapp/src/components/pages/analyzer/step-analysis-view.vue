<template>
    <div class="step-analysis-container">
        <doc-chat :document-id="documentId"
                  @question-sent="handleQuestionSent" />
        <step-stepper v-if="documentData && documentData.steps && documentData.steps.length > 0"
                      :steps="documentData.steps"
                      :initial-step-id="documentData.lastProcessedStepId"
                      @step-changed="handleStepChange" />

        <extracted-fields v-if="currentStepData"
                          :fields="currentStepData.outputs"
                          :title="`${$t('labelExtractedData')} - ${currentStepData.name}`"
                          @field-updated="handleFieldUpdate" />
    </div>
</template>

<script>
    import StepStepper from "@/components/pages/analyzer/step-stepper";
    import ExtractedFields from "@/components/pages/analyzer/extracted-fields";
    import DocChat from "@/components/pages/analyzer/doc-chat";
    import api from "@/services/api";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import DocumentService from "@/services/documents/DocumentsServices";
    export default {
        name: "StepAnalysisView",
        components: {
            StepStepper,
            ExtractedFields,
            DocChat,
        },
        props: {
            documentId: {
                type: Number,
                required: true,
            },
        },
        emits: ["show-alert-toast"],
        data() {
            return {
                documentData: null,
                currentStepData: null,
                loading: false,
                cardId: null,
            };
        },
        methods: {
            async loadDocumentData() {
                this.loading = true;
                try {

                    await this.findByIdAnalyze(this.documentId);

                    await this.findByIdAnalyzeWithSteps(this.cardId);

                    if (this.documentData.lastProcessedStepId && this.documentData.steps.length > 0) {
                        const lastStep = this.documentData.steps.find(
                            s => s.id === this.documentData.lastProcessedStepId
                        );
                        this.currentStepData = lastStep || this.documentData.steps[0];
                    } else if (this.documentData.steps.length > 0) {
                        this.currentStepData = this.documentData.steps[0];
                    }
                } catch (error) {
                    this.$emit("show-alert-toast", {
                        msg: $t('labelErrorLoadDocumentData'),
                        color: "toast-danger"
                    });
                } finally {
                    this.loading = false;
                }
            },
            handleStepChange(step) {
                this.currentStepData = step;
            },
            handleFieldUpdate({ id, field }) {
                let params = {
                    id: id,
                    value: field.value,
                };
                WorkflowService.updateStepToolOutput(params)
                    .then((response) => {
                        if (response == true) {
                            this.$emit("show-alert-toast", {
                                msg: this.$t('labelSuccessEditOutput'),
                                color: "toast-success"
                            });
                        } else {
                            this.$emit("show-alert-toast", {
                                msg: this.$t('labelFailedEditOutput'),
                                color: "toast-danger"
                            });
                        }
                    })
                    .finally(() => {
                        this.loadDocumentData();
                    });
            },
            async findByIdAnalyze(id) {
               await DocumentService.findByIdAnalyze(id)
                    .then((response) => {
                        this.cardId = response.data.cardId;
                    })
                    .catch((err) => {
                        console.log(err)
                    });
            },
            async findByIdAnalyzeWithSteps(id){
               await DocumentService.findByIdAnalyzeWithSteps(id)
                    .then((response) => {
                        this.documentData = response.data;
                    })
                    .catch((err) => {
                        console.log(err)
                    });
            }
        },
        mounted() {
            this.loadDocumentData();
        },
    };
</script>

<style scoped>
    .step-analysis-container {
        display: flex;
        flex-direction: column;
        gap: 1rem;
        height: 600px;
        overflow-y: auto;
    }

    @media (max-width: 768px) {
        .step-analysis-container {
            gap: 0.75rem;
        }
    }
</style>
