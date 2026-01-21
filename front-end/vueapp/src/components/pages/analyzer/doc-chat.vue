<template>
    <div class="doc-chat-container">
        <button
            class="chat-toggle-button"
            @click="toggleChat"
            :class="{ expanded: isExpanded }"
        >
            <i class="fas fa-comment-dots"></i>
            {{ $t("analyze.askTheDoc") }}
        </button>

        <div
            v-if="isExpanded"
            class="chat-panel"
        >
            <div class="chat-header">
                <i class="fas fa-comment-dots"></i>
                {{ $t("analyze.askTheDoc") }}
                <button
                    class="close-button"
                    @click="toggleChat"
                    :title="$t('common.close')"
                >
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="questionnaire-section">
                <label class="input-label">
                    {{ $t("analyze.questionnaireToApply") }}
                </label>
                <div class="questionnaire-controls">
                    <select
                        v-model="selectedQuestionnaireId"
                        class="questionnaire-select"
                    >
                        <option :value="null">
                            {{
                                $t(
                                    "analyze.selectQuestionnaire"
                                )
                            }}
                        </option>
                        <option
                            v-for="questionnaire in questionnaires"
                            :key="questionnaire.id"
                            :value="questionnaire.id"
                        >
                            {{ questionnaire.title }}
                        </option>
                    </select>
                    <button
                        class="apply-button"
                        @click="applyQuestionnaire"
                        :disabled="
                            !selectedQuestionnaireId ||
                            isApplyingQuestionnaire
                        "
                    >
                        <div
                            v-if="isApplyingQuestionnaire"
                            class="spinner-border spinner-border-sm text-light"
                            role="status"
                        ></div>
                        <i
                            v-else
                            class="fas fa-arrow-up"
                        ></i>
                    </button>
                </div>
            </div>

            <div
                v-if="questionnaireResults.length > 0"
                class="results-section"
            >
                <div class="results-header">
                    <label class="input-label">
                        {{
                            $t(
                                "analyze.questionnaireResults"
                            )
                        }}
                    </label>
                    <button
                        class="close-results-button"
                        @click="clearResults"
                        :title="$t('analyze.closeResults')"
                    >
                        <i class="fas fa-times"></i>
                        {{ $t("analyze.closeResults") }}
                    </button>
                </div>
                <div class="results-list">
                    <div
                        v-for="(
                            result, index
                        ) in questionnaireResults"
                        :key="index"
                        class="result-card"
                    >
                        <div class="result-question">
                            <strong>
                                {{ result.question }}
                            </strong>
                        </div>
                        <div class="result-answer">
                            {{ result.answer }}
                            <span
                                v-if="result.confirmed"
                                class="confirmed-badge"
                            >
                                <i
                                    class="fas fa-check-circle"
                                ></i>
                                {{
                                    $t("analyze.confirmed")
                                }}
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="chat-input-section">
                <textarea
                    v-model="question"
                    class="chat-textarea"
                    :placeholder="
                        $t('analyze.typeYourQuestion')
                    "
                    rows="4"
                    @input="handleInput"
                ></textarea>

                <button
                    v-if="question.trim()"
                    class="send-button"
                    @click="sendQuestion"
                    :disabled="isSending"
                >
                    <LucideIcon
                        icon="RefreshCcw"
                        :size="17"
                        :class="{
                            'animate-spin': isSending,
                        }"
                        v-if="isSending"
                    />
                    <LucideIcon
                        icon="SendHorizontal"
                        :size="17"
                        v-if="!isSending"
                    />
                    {{ $t("analyze.sendQuestion") }}
                </button>
                <div v-if="output != ''">
                    <label class="input-label">
                        {{ $t("common.output") }}
                    </label>
                    <textarea
                        v-model="output"
                        class="chat-textarea"
                        rows="4"
                    ></textarea>
                    <button
                        type="button"
                        class="btn btn-outline-primary"
                        @click="copy"
                    >
                        {{ $t("analyze.copy") }}
                    </button>
                    <button
                        type="button"
                        class="btn btn-outline-primary"
                        @click="clear"
                    >
                        {{ $t("analyze.clear") }}
                    </button>
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
                appliedQuestionnaireId: null,
            };
        },
        methods: {
            toggleChat() {
                this.isExpanded = !this.isExpanded;
                if (!this.isExpanded) {
                    this.clearResults();
                    this.selectedQuestionnaireId = null;
                }
            },
            clearResults() {
                this.questionnaireResults = [];
                this.appliedQuestionnaireId = null;
            },
            handleInput() {},
            async loadQuestionnaires() {
                try {
                    const result =
                        await QuizzesService.getQuizzes({
                            page: 1,
                            pageSize: 100,
                            search: "",
                            isAscending: false,
                            colType: 2,
                        });
                    if (result.content) {
                        this.questionnaires =
                            result.content;
                    }
                } catch (error) {
                    this.$notify({
                        title: "analyze.title",
                        message:
                            "analyze.errorLoadingQuestionnaires",
                        variant: "danger",
                        icon: "CircleX",
                    });
                }
            },
            async applyQuestionnaire() {
                if (!this.selectedQuestionnaireId) {
                    this.$notify({
                        title: "analyze.title",
                        message:
                            "analyze.pleaseSelectQuestionnaire",
                        variant: "warning",
                        icon: "AlertTriangle",
                    });
                    return;
                }

                this.isApplyingQuestionnaire = true;

                try {
                    const questionnaireDetails =
                        await QuizzesService.getQuizzById(
                            this.selectedQuestionnaireId
                        );
                    if (questionnaireDetails.error) {
                        throw new Error(
                            "Failed to load questionnaire details"
                        );
                    }
                    const questions =
                        questionnaireDetails.questions ||
                        [];
                    const questionTexts = questions
                        .map((q) => q.description?.trim())
                        .filter((q) => q);

                    const params = {
                        idDocument: this.documentId,
                        idQuestionnaire:
                            this.selectedQuestionnaireId,
                    };

                    const response =
                        await DocumentServices.applyQuestionnaire(
                            params
                        );
                    if (response.error) {
                        throw new Error(
                            "Failed to apply questionnaire"
                        );
                    }

                    this.appliedQuestionnaireId =
                        this.selectedQuestionnaireId;
                    const historyResponse =
                        await DocumentServices.getDocumentHistory(
                            this.documentId
                        );
                    if (historyResponse.error) {
                        throw new Error(
                            "Failed to load document history"
                        );
                    }

                    let historyData = null;
                    if (
                        historyResponse.data &&
                        historyResponse.data.value &&
                        Array.isArray(
                            historyResponse.data.value
                        )
                    ) {
                        historyData =
                            historyResponse.data.value;
                    } else if (
                        historyResponse.data &&
                        Array.isArray(historyResponse.data)
                    ) {
                        historyData = historyResponse.data;
                    }

                    if (historyData) {
                        const sortedHistory = [
                            ...historyData,
                        ].sort((a, b) => {
                            const dateA = new Date(
                                a.created ||
                                    a.createdAt ||
                                    0
                            );
                            const dateB = new Date(
                                b.created ||
                                    b.createdAt ||
                                    0
                            );
                            return dateB - dateA;
                        });

                        const results = [];
                        for (const questionText of questionTexts) {
                            if (!questionText) continue;
                            const matchingEntry =
                                sortedHistory.find(
                                    (item) => {
                                        const itemInput =
                                            item.input?.trim();
                                        const textMatches =
                                            itemInput &&
                                            itemInput ===
                                                questionText;
                                        const idMatches =
                                            !item.questionnaireId ||
                                            item.questionnaireId ===
                                                this
                                                    .appliedQuestionnaireId;
                                        return (
                                            textMatches &&
                                            idMatches
                                        );
                                    }
                                );
                            if (matchingEntry) {
                                results.push({
                                    question:
                                        matchingEntry.input,
                                    answer: matchingEntry.output,
                                    confirmed:
                                        matchingEntry.confirmed,
                                    questionnaireId:
                                        matchingEntry.questionnaireId,
                                });
                            }
                        }

                        this.questionnaireResults = results;
                    }

                    this.$notify({
                        title: "analyze.title",
                        message:
                            "analyze.successApplyingQuestionnaire",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                } catch (error) {
                    this.$notify({
                        title: "analyze.title",
                        message:
                            "analyze.errorApplyingQuestionnaire",
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
                };
                try {
                    await DocumentServices.inputDocument(
                        params
                    ).then((response) => {
                        this.$notify({
                            title: "analyze.title",
                            message:
                                "analyze.successEditOutput",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                        this.output = response.data;
                    });
                } catch (error) {
                    this.$notify({
                        title: "analyze.title",
                        message: "analyze.failedEditOutput",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isSending = false;
                }
            },
            copy() {
                navigator.clipboard.writeText(this.output);
            },
            clear() {
                this.output = "";
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
        max-height: 70vh;
        overflow-y: auto;
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
        width: 50%;
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
        margin-bottom: 2rem;
        padding-bottom: 2rem;
        border-bottom: 1px solid #e0e0e0;
    }

    .results-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 0.75rem;
    }

    .close-results-button {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.5rem 1rem;
        background: #f8f9fa;
        border: 1px solid #ddd;
        border-radius: 6px;
        color: #666;
        font-size: 0.85rem;
        cursor: pointer;
        transition: all 0.3s ease;
    }

    .close-results-button:hover {
        background: #e9ecef;
        border-color: #ccc;
        color: #333;
    }

    .close-results-button i {
        font-size: 0.75rem;
    }

    .results-list {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        margin-top: 0.75rem;
        margin-bottom: 1.5rem;
        max-height: 60vh;
        overflow-y: auto;
        padding-right: 0.5rem;
    }

    /* Scrollbar styling */
    .results-list::-webkit-scrollbar {
        width: 8px;
    }

    .results-list::-webkit-scrollbar-track {
        background: #f1f1f1;
        border-radius: 4px;
    }

    .results-list::-webkit-scrollbar-thumb {
        background: #ccc;
        border-radius: 4px;
    }

    .results-list::-webkit-scrollbar-thumb:hover {
        background: #999;
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
            margin-bottom: 2rem;
            padding-bottom: 2rem;
            border-bottom: 1px solid #e0e0e0;
        }

        .results-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 0.75rem;
        }

        .close-results-button {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            background: #f8f9fa;
            border: 1px solid #ddd;
            border-radius: 6px;
            color: #666;
            font-size: 0.85rem;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .close-results-button:hover {
            background: #e9ecef;
            border-color: #ccc;
            color: #333;
        }

        .close-results-button i {
            font-size: 0.75rem;
        }

        .results-list {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
            margin-top: 0.75rem;
            margin-bottom: 1.5rem;
            max-height: 60vh;
            overflow-y: auto;
            padding-right: 0.5rem;
        }

        /* Scrollbar styling */
        .results-list::-webkit-scrollbar {
            width: 8px;
        }

        .results-list::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 4px;
        }

        .results-list::-webkit-scrollbar-thumb {
            background: #ccc;
            border-radius: 4px;
        }

        .results-list::-webkit-scrollbar-thumb:hover {
            background: #999;
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

        .results-list {
            max-height: 50vh;
        }

        .results-header {
            flex-direction: column;
            align-items: flex-start;
            gap: 0.5rem;
        }

        .close-results-button {
            align-self: stretch;
            justify-content: center;
        }

        .result-card {
            padding: 0.75rem;
        }
    }

    .animate-spin {
        animation: spin 1s linear infinite;
        color: white;
    }

    @keyframes spin {
        from {
            transform: rotate(0deg);
        }

        to {
            transform: rotate(360deg);
        }
    }
</style>
