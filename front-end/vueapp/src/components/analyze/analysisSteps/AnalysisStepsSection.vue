<template>
    <div class="step-analysis-container">
        <DocumentChat
            :document-id="documentId"
            v-if="documentData && documentData.canAnswer"
        />

        <div
            v-if="documentData && documentData.steps && documentData.steps.length > 0"
            class="step-analysis-container__stepper"
        >
            <StepsViewer
                :steps="documentData.steps"
                :initial-step-id="documentData.lastProcessedStepId"
                @step-changed="handleStepChange"
            />
        </div>

        <ExtractedDataViewer
            v-if="currentStepData"
            :fields="currentStepData.outputs"
            :title="`${$t('analyze.extractedData')} - ${currentStepData.name}`"
            @field-updated="handleFieldUpdate"
        />
    </div>
</template>
<script>
    import DocumentChat from "@/components/analyze/analysisSteps/DocumentChat.vue";
    import StepsViewer from "@/components/analyze/analysisSteps/StepsViewer.vue";
    import ExtractedDataViewer from "@/components/analyze/analysisSteps/ExtractedDataViewer.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import CardsServices from "@/services/cards/CardsServices";

    export default {
        name: "AnalysisStepsSection",
        components: {
            DocumentChat,
            StepsViewer,
            ExtractedDataViewer,
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

                    if (
                        this.documentData.lastProcessedStepId &&
                        this.documentData.steps.length > 0
                    ) {
                        const lastStep = this.documentData.steps.find(
                            (s) => s.id === this.documentData.lastProcessedStepId
                        );
                        this.currentStepData = lastStep || this.documentData.steps[0];
                    } else if (this.documentData.steps.length > 0) {
                        this.currentStepData = this.documentData.steps[0];
                    }
                } catch (error) {
                    this.$notify({
                        title: "analyze.title",
                        message: "analyze.errorLoadDocumentData",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.loading = false;
                }
            },
            handleStepChange(step) {
                this.currentStepData = step;
            },
            handleFieldUpdate({ id, field, outputsJson }) {
                let params = {};
                if (outputsJson) {
                    params = {
                        id: id,
                        value: outputsJson,
                    };
                } else {
                    params = {
                        id: id,
                        value: field.value,
                    };
                }
                WorkflowService.updateStepToolOutput(params)
                    .then((response) => {
                        if (response == true) {
                            this.$notify({
                                title: "analyze.title",
                                message: "analyze.successEditOutput",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "analyze.title",
                                message: "analyze.failedEditOutput",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.loadDocumentData();
                    });
            },
            async findByIdAnalyzeWithSteps(id) {
                await CardsServices.findByIdAnalyzeWithSteps(id)
                    .then((response) => {
                        this.documentData = response.data;
                    })
                    .catch((err) => {
                        console.log(err);
                    });
            },
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
    }

    .step-analysis-container__stepper {
        position: sticky;
        top: 0;
        z-index: 10;
        flex-shrink: 0;
        background: var(--color-bg-body-content, #fff);
        padding-bottom: 0.25rem;
        box-shadow: 0 4px 6px -4px rgba(0, 0, 0, 0.12);
    }

    @media (max-width: 768px) {
        .step-analysis-container {
            gap: 0.75rem;
        }
    }
</style>
