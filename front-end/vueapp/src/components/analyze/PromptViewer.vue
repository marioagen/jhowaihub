<template>
    <modal-question
        v-if="showModalQuestion"
        :dataQuiz="quizSelected"
    />
</template>
<script>
    import ModalQuestion from "@/components/pages/analyzer/modal-question";
    import AnalyzerService from "@/services/documents/AnalyzerService";
    import QuizzesService from "@/services/quizzes/QuizzesService";

    export default {
        name: "PromptView",
        emits: ["showHistory", "unshiftHistoryList", "pushHistoryList"],
        props: {
            hashDocument: {
                required: true,
                type: String,
                default: "",
            },
            historyListOrder: {
                required: true,
                type: String,
                default: "",
            },
        },
        data() {
            return {
                documentId: this.$route.params.id,
                input: "",
                output: "",
                quizzes: [],
                quizSelected: "",
                loadingApplying: false,
                loadingOutput: false,
            };
        },
        components: {
            ModalQuestion,
        },
        methods: {
            getQuizzes() {
                QuizzesService.getQuizzes()
                    .then((data) => {
                        this.quizzes = data;
                    })
                    .catch((e) => {
                        console.log(e.error);
                    });
            },
            sendQuiz() {
                this.loadingApplying = true;
                this.$notify({
                    title: this.$t("common.info"),
                    message: this.$t("quizzes.applyingQuestionnaireWait"),
                    variant: "info",
                    icon: "MessageCircle",
                });

                let paramsReq = {
                    idDocument: parseInt(this.documentId),
                    idQuestionnaire: this.quizSelected.id,
                };

                AnalyzerService.sendQuizz(paramsReq).then(() => {
                    if (response.error !== undefined) {
                        console.log(response.error);
                        this.loadingApplying = false;
                        if (e.response && e.response.data === "No Credits to send a Question") {
                            return this.$notify({
                                title: this.$t("quizzes.title"),
                                message: this.$t("questions.numberOfQuestionsHasBeenExceeded"),
                                variant: "danger",
                                icon: "CircleX",
                            });
                        } else if (e.response.status === 404) {
                            return this.$notify({
                                title: this.$t("documents.title"),
                                message: this.$t(
                                    "documents.anInconsistencyWasIdentifiedInTheDocument"
                                ),
                                variant: "danger",
                                icon: "CircleX",
                            });
                        } else if (e.response.status === 402) {
                            return this.$notify({
                                title: this.$t("quizzes.title"),
                                message: this.$t("quizzes.thereIsNotEnoughCredit"),
                                variant: "warning",
                                icon: "CircleAlert",
                            });
                        } else {
                            return this.$notify({
                                title: this.$t("quizzes.title"),
                                message: this.$t("quizzes.failedToApplyQuestionnaire"),
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    }

                    this.loadingApplying = false;
                    return this.$notify({
                        title: this.$t("quizzes.title"),
                        message: this.$t("quizzes.questionnaireAppliedSuccessfully"),
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                });
            },
            submitInput() {
                this.loadingOutput = true;
                this.output = "";
                let paramsReq = {
                    id: this.documentId,
                    input: this.input,
                };
                this.input = "";

                AnalyzerService.submitInput(paramsReq).then((response) => {
                    if (response.error !== undefined) {
                        console.log(response.error);
                        this.loadingOutput = false;
                        if (e.response.data === "No Credits to send a Question") {
                            this.output = "";
                            return this.$notify({
                                title: this.$t("questions.title"),
                                message: this.$t("questions.numberOfQuestionsHasBeenExceeded"),
                                variant: "danger",
                                icon: "CircleX",
                            });
                        } else if (e.response.status === 404) {
                            return (this.output = this.$t(
                                "documents.anInconsistencyWasIdentifiedInTheDocument"
                            ));
                        } else {
                            return (this.output = this.$t("analyze.failedNoResponse"));
                        }
                    }

                    if (typeof response.data === "object") {
                        this.output = JSON.stringify(response.data, undefined, 2);
                    } else {
                        this.output = response.data;
                    }
                    if (this.historyListOrder == "desc") {
                        this.$emit("unshiftHistoryList", {
                            input: paramsReq.input,
                            output: this.output,
                        });
                    } else {
                        this.$emit("pushHistoryList", {
                            input: paramsReq.input,
                            output: this.output,
                        });
                    }
                    this.loadingOutput = false;
                });
            },
            copyToClipboard(content) {
                navigator.clipboard.writeText(content);
                this.$notify({
                    title: this.$t("common.info"),
                    message: this.$t("common.textCopiedToClipboard"),
                    variant: "info",
                    icon: "MessageCircle",
                });
            },
            upperFormat(str) {
                return str.toUpperCase();
            },
        },
        created() {
            this.getQuizzes();
        },
    };
</script>
<style scoped>
    .fas,
    .far {
        font-weight: 900 !important;
    }

    .text-primary {
        color: #47aaff !important;
    }

    .div-quiz {
        display: flex;
    }

    .custom-input {
        border-color: #0073e6 !important;
    }

    .custom-input-button .input-group-text {
        padding: 0.6rem 0.75rem !important;
        background-color: var(--color-bg-form-control) !important;
        color: var(--color-bg-icon-active) !important;
        border-left: none !important;
        cursor: pointer !important;
    }

    @media (min-width: 768px) and (max-width: 1439px) {
        #hashIcon {
            display: none;
        }
        #hashId {
            width: 250px;
            white-space: normal;
            word-wrap: break-word;
            overflow-wrap: break-word;
        }
    }
    @media (min-width: 320px) and (max-width: 767px) {
        .custom-textarea {
            height: calc(100vh - 270px);
            resize: none;
        }
    }
    .custom-textarea {
        max-height: 50vh;
    }
</style>
