<template>
    <div class="step-analysis-container">
        <step-stepper
            v-if="documentData && documentData.steps && documentData.steps.length > 0"
            :steps="documentData.steps"
            :initial-step-id="documentData.lastProcessedStepId"
            @step-changed="handleStepChange"
        />

        <extracted-fields
            v-if="currentStepData"
            :fields="currentStepData.outputs"
            :title="`${$t('labelExtractedData')} - ${currentStepData.name}`"
            @field-updated="handleFieldUpdate"
        />

        <doc-chat
            :document-id="documentId"
            @question-sent="handleQuestionSent"
        />
    </div>
</template>

<script>
    import StepStepper from "@/components/pages/analyzer/step-stepper";
    import ExtractedFields from "@/components/pages/analyzer/extracted-fields";
    import DocChat from "@/components/pages/analyzer/doc-chat";
    import api from "@/services/api";

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
            };
        },
        methods: {
            async loadDocumentData() {
                this.loading = true;
                try {
                    const response = await api.get(`/Document/AnalyzeSteps/${this.documentId}`);
                    this.documentData = response.data;
                    
                    // Set the initial step to the last processed one
                    if (this.documentData.lastProcessedStepId && this.documentData.steps.length > 0) {
                        const lastStep = this.documentData.steps.find(
                            s => s.id === this.documentData.lastProcessedStepId
                        );
                        this.currentStepData = lastStep || this.documentData.steps[0];
                    } else if (this.documentData.steps.length > 0) {
                        this.currentStepData = this.documentData.steps[0];
                    }
                } catch (error) {
                    console.error("Error loading document data:", error);
                    this.$emit("show-alert-toast", {
                        msg: "Erro ao carregar dados do documento",
                        color: "toast-danger",
                    });
                } finally {
                    this.loading = false;
                }
            },
            handleStepChange(step) {
                this.currentStepData = step;
            },
            handleFieldUpdate({ index, field }) {
                // Here you could send the update to the backend if needed
                console.log("Field updated:", field);
                this.$emit("show-alert-toast", {
                    msg: "Campo atualizado com sucesso",
                    color: "toast-success",
                });
            },
            async handleQuestionSent(question) {
                try {
                    const response = await api.post("/Document/Input", {
                        id: this.documentId,
                        input: question,
                    });
                    
                    this.$emit("show-alert-toast", {
                        msg: "Pergunta enviada com sucesso",
                        color: "toast-success",
                    });

                    // You might want to display the response somehow
                    console.log("AI Response:", response.data);
                } catch (error) {
                    console.error("Error sending question:", error);
                    this.$emit("show-alert-toast", {
                        msg: "Erro ao enviar pergunta",
                        color: "toast-danger",
                    });
                }
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
        height: 100%;
    }

    @media (max-width: 768px) {
        .step-analysis-container {
            gap: 0.75rem;
        }
    }
</style>
