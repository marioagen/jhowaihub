<template>
    <div class="doc-chat-container">
        <button class="chat-toggle-button" @click="toggleChat" :class="{ expanded: isExpanded }">
            <i class="fas fa-comment-dots"></i>
            {{ $t("analyze.askTheDoc") }}
        </button>

        <div v-if="isExpanded" class="chat-panel">
            <div class="chat-header">
                <i class="fas fa-comment-dots"></i>
                {{ $t("analyze.conversationWithDocument") }}
                <button class="close-button" @click="toggleChat" :title="$t('labelClose')">
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="questionnaire-section">
                <label class="input-label">{{ $t("analyze.questionnaireToApply") }}</label>
                <div class="questionnaire-controls">
                    <select v-model="selectedQuestionnaireId" 
                            class="questionnaire-select">
                        <option :value="null">{{ $t("analyze.selectQuestionnaire") }}</option>
                        <option v-for="questionnaire in questionnaires" 
                                :key="questionnaire.id" 
                                :value="questionnaire.id">
                            {{ questionnaire.name }}
                        </option>
                    </select>
                    <button class="apply-button"
                            @click="applyQuestionnaire"
                            :disabled="!selectedQuestionnaireId || isApplyingQuestionnaire">
                        <div v-if="isApplyingQuestionnaire" class="spinner-border spinner-border-sm text-light" role="status"></div>
                        <i v-else class="fas fa-arrow-up"></i>
                    </button>
                </div>
            </div>

            <div v-if="questionnaireResults.length > 0" class="results-section">
                <label class="input-label">{{ $t("analyze.questionnaireResults") }}</label>
                <div class="results-list">
                    <div v-for="(result, index) in questionnaireResults" 
                         :key="index" 
                         class="result-card">
                        <div class="result-question">
                            <strong>{{ result.question }}</strong>
                        </div>
                        <div class="result-answer">
                            {{ result.answer }}
                            <span v-if="result.confirmed" class="confirmed-badge">
                                <i class="fas fa-check-circle"></i> {{ $t("analyze.confirmed") }}
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="chat-input-section">
                <label class="input-label">{{ $t("analyze.askAI") }}</label>
                <textarea v-model="question"
                          class="chat-textarea"
                          :placeholder="$t('analyze.typeYourQuestion')"
                          rows="4"
                          @input="handleInput"></textarea>

                <button v-if="question.trim()"
                        class="send-button"
                        @click="sendQuestion"
                        :disabled="isSending">
                    <i class="fas fa-paper-plane"></i>
                    {{ $t("analyze.sendQuestion") }}
                </button>
                <div v-if="output != ''">
                    <label class="input-label">{{ $t("analyze.output") }}</label>
                    <textarea v-model="output"
                              class="chat-textarea"
                              rows="4">
                    </textarea>
                    <button type="button" class="btn btn-outline-primary" @click="copy">{{ $t("analyze.copy") }}</button>
                    <button type="button" class="btn btn-outline-primary" @click="clear">{{ $t("analyze.clear") }}</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import DocumentServices from "@/services/documents/DocumentsServices";
    import QuizzesService from "@/services/quizzes/QuizzesService";
    export default {
        name: "DocChat",
        props: {
            documentId: {
                type: Number,
                required: true,
            },
        },
        emits: ["question-sent"],
        data() {
            return {
                isExpanded: false,
                question: "",
                isSending: false,
                output: "",
                questionnaires: [],
                selectedQuestionnaireId: null,
                isApplyingQuestionnaire: false,
                questionnaireResults: [],
            };
        },
        methods: {
            toggleChat() {
                this.isExpanded = !this.isExpanded;
            },
            handleInput() {
            },
            async loadQuestionnaires() {
                try {
                    const result = await QuizzesService.getQuizzes({ pageSize: 100, pageNumber: 1 });
                    if (result.content) {
                        this.questionnaires = result.content;
                    }
                } catch (error) {
                    this.$notify({
                        title: "analyze.title",
                        message: "analyze.errorLoadingQuestionnaires",
                        variant: "danger",
                        icon: "CircleX",
                    });
                }
            },
            async applyQuestionnaire() {
                if (!this.selectedQuestionnaireId) {
                    this.$notify({
                        title: "analyze.title",
                        message: "analyze.pleaseSelectQuestionnaire",
                        variant: "warning",
                        icon: "AlertTriangle",
                    });
                    return;
                }

                this.isApplyingQuestionnaire = true;
                const params = {
                    idDocument: this.documentId,
                    idQuestionnaire: this.selectedQuestionnaireId,
                };

                try {
                    const response = await DocumentServices.applyQuestionnaire(params);
                    if (response && response.data) {
                        this.questionnaireResults = response.data;
                        this.$notify({
                            title: "analyze.title",
                            message: "analyze.successApplyingQuestionnaire",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    }
                } catch (error) {
                    this.$notify({
                        title: "analyze.title",
                        message: "analyze.errorApplyingQuestionnaire",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isApplyingQuestionnaire = false;
                }
            },
            async sendQuestion() {
                if (!this.question.trim()) return;
                this.isSending = true;
                let params = {
                    id: this.documentId,
                    input: this.question,
                }
                try {
                    await DocumentServices.inputDocument(params)
                    .then((response) => {
                        this.$notify({
                            title: "analyze.title",
                            message:"analyze.successEditOutput",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                        this.output = response.data;
                    })
                    
                } catch (error) {
                    this.$notify({
                        title: "analyze.title",
                        message:"analyze.failedEditOutput",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isSending = false;
                }
            },
            copy() {
                navigator.clipboard.writeText(this.output)
            },
            clear() {
                this.output = ''
            },
        },
        mounted() {
            this.loadQuestionnaires();
        },
    };
</script>

<style scoped>
    .doc-chat-container {
        margin-top: 1rem;
    }

    .chat-toggle-button {
        width: 100%;
        padding: 0.75rem 1rem;
        background: #f8f9fa;
        border: 2px dashed #0073e6;
        border-radius: 8px;
        color: #0073e6;
        font-size: 1rem;
        font-weight: 500;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 0.5rem;
        transition: all 0.3s ease;
    }

    .chat-toggle-button:hover {
        background: #e7f3ff;
        border-color: #005bb5;
    }

    .chat-toggle-button.expanded {
        display: none;
    }

    .chat-panel {
        background: white;
        border: 2px solid #0073e6;
        border-radius: 8px;
        padding: 1rem;
        box-shadow: 0 4px 12px rgba(0, 115, 230, 0.15);
        animation: slideDown 0.3s ease;
    }

    @keyframes slideDown {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    .chat-header {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        color: #0073e6;
        font-weight: 600;
        margin-bottom: 1rem;
        padding-bottom: 0.75rem;
        border-bottom: 1px solid #e0e0e0;
        position: relative;
    }

    .close-button {
        position: absolute;
        right: 0;
        background: transparent;
        border: none;
        color: #666;
        font-size: 1.2rem;
        cursor: pointer;
        padding: 0.25rem;
        transition: color 0.3s ease;
    }

    .close-button:hover {
        color: #333;
    }

    .chat-input-section {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
    }

    .input-label {
        font-size: 0.9rem;
        color: #333;
        font-weight: 500;
        margin: 0;
    }

    .chat-textarea {
        width: 100%;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 6px;
        font-size: 0.9rem;
        font-family: inherit;
        resize: vertical;
        transition: border-color 0.3s ease;
    }

    .chat-textarea:focus {
        outline: none;
        border-color: #0073e6;
    }

    .chat-textarea::placeholder {
        color: #999;
    }

    .send-button {
        align-self: flex-start;
        background: #0073e6;
        border: none;
        border-radius: 6px;
        color: white;
        font-size: 0.95rem;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 0.5rem;
        transition: all 0.3s ease;
    }

    .send-button:hover:not(:disabled) {
        background: #005bb5;
        transform: translateY(-1px);
        box-shadow: 0 4px 8px rgba(0, 115, 230, 0.3);
    }

    .send-button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }

    .questionnaire-section {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        margin-bottom: 1.5rem;
        padding-bottom: 1.5rem;
        border-bottom: 1px solid #e0e0e0;
    }

    .questionnaire-controls {
        display: flex;
        gap: 0.5rem;
        align-items: stretch;
    }

    .questionnaire-select {
        flex: 1;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 6px;
        font-size: 0.9rem;
        font-family: inherit;
        background: white;
        cursor: pointer;
        transition: border-color 0.3s ease;
    }

    .questionnaire-select:focus {
        outline: none;
        border-color: #0073e6;
    }

    .apply-button {
        width: 48px;
        height: 48px;
        background: #0073e6;
        border: none;
        border-radius: 6px;
        color: white;
        font-size: 1rem;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.3s ease;
        flex-shrink: 0;
    }

    .apply-button:hover:not(:disabled) {
        background: #005bb5;
        transform: translateY(-1px);
        box-shadow: 0 4px 8px rgba(0, 115, 230, 0.3);
    }

    .apply-button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }

    .results-section {
        margin-bottom: 1.5rem;
        padding-bottom: 1.5rem;
        border-bottom: 1px solid #e0e0e0;
    }

    .results-list {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        margin-top: 0.75rem;
    }

    .result-card {
        background: #f8f9fa;
        border: 1px solid #e0e0e0;
        border-radius: 6px;
        padding: 1rem;
        transition: box-shadow 0.3s ease;
    }

    .result-card:hover {
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .result-question {
        color: #333;
        margin-bottom: 0.5rem;
        font-size: 0.95rem;
    }

    .result-answer {
        color: #666;
        font-size: 0.9rem;
        line-height: 1.5;
        white-space: pre-wrap;
        word-wrap: break-word;
    }

    .confirmed-badge {
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
        margin-left: 0.5rem;
        padding: 0.25rem 0.5rem;
        background: #d4edda;
        color: #155724;
        border-radius: 4px;
        font-size: 0.75rem;
        font-weight: 500;
    }

    @media (max-width: 768px) {
        .chat-panel {
            padding: 0.75rem;
        }

        .chat-textarea {
            padding: 0.6rem;
            font-size: 0.85rem;
        }

        .send-button {
            padding: 0.65rem 1.25rem;
            font-size: 0.9rem;
        }

        .questionnaire-select {
            font-size: 0.85rem;
            padding: 0.6rem;
        }

        .apply-button {
            width: 44px;
            height: 44px;
        }

        .result-card {
            padding: 0.75rem;
        }
    }
</style>
