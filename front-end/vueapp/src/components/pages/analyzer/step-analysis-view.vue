<template>
    <div class="step-analysis-container">
        <doc-chat :document-id="documentId"
                  @question-sent="handleQuestionSent"
                  v-if="documentData && documentData.canAnswer"/>
        <step-stepper v-if="documentData && documentData.steps && documentData.steps.length > 0"
                      :steps="documentData.steps"
                      :initial-step-id="documentData.lastProcessedStepId"
                      @step-changed="handleStepChange" />

        <extracted-fields v-if="currentStepData"
                          :fields="currentStepData.outputs"
                          :title="`${$t('analyze.extractedData')} - ${currentStepData.name}`"
                          @field-updated="handleFieldUpdate" />
    </div>
</template>

<script>
    import StepStepper from "@/components/pages/analyzer/step-stepper";
    import ExtractedFields from "@/components/pages/analyzer/extracted-fields";
    import DocChat from "@/components/pages/analyzer/doc-chat";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import CardsServices from "@/services/cards/CardsServices";
    
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
            cardId: {
                type: Number,
                required: true,
            },
        },
        data() {
            return {
                documentData: null,
                currentStepData: null,
                loading: false,
            };
        },
        methods: {
            async loadDocumentData() {
                this.loading = true;
                try {
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
                        this.$notify({
                            title: "analyze.title",
                            message:"analyze.errorLoadDocumentData",
                            variant: "danger",
                            icon: "CircleX",
                        });
                } finally {
                    this.loading = false;
                    console.log(this.documentData);
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
                            this.$notify({
                                title: "analyze.title",
                                message:"analyze.successEditOutput",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "analyze.title",
                                message:"analyze.failedEditOutput",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.loadDocumentData();
                    });
            },
            async findByIdAnalyzeWithSteps(id){
               await CardsServices.findByIdAnalyzeWithSteps(id)
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
        overflow-y: auto;
    }

    @media (max-width: 768px) {
        .step-analysis-container {
            gap: 0.75rem;
        }
    }
</style>
