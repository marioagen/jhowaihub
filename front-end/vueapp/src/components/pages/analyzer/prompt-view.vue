<template>
    <!-- Component ModalQuestion -->
    <modal-question v-if="showModalQuestion" :dataQuiz="quizSelected" @close="closeModal" />
</template>

<script>
    import ModalQuestion from "@/components/pages/analyzer/modal-question";
    import api from "@/services/api";

    export default {
        name: "PromptView",
        emits: ["showHistory", "unshiftHistoryList", "pushHistoryList", "showAlertToast", "clearMyInterval"],
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
                idAnalyzer: this.$route.params.id,
                input: "",
                output: "",
                quizzes: [],
                quizSelected: "",
                loadingApplying: false,
                loadingOutput: false,
                showModalQuestion: false,
            };
        },
        components: {
            ModalQuestion,
        },
        watch: {},
        methods: {
            getQuizzes: function () {
                let self = this;
                api.get("/Questionnaire/FindAll")
                    .then(function (response) {
                        // Handle success
                        self.quizzes = response.data;
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            sendQuiz: function () {
                this.loadingApplying = true;
                this.clearMyInterval();
                this.alertToast(this.$t("labelApplyingQuestionnaireWait"), "toast-primary");
                let paramsReq = {
                    idDocument: parseInt(this.idAnalyzer),
                    idQuestionnaire: this.quizSelected.id,
                };
                let self = this;
                api.post("/Document/inputQuestionnaire", paramsReq)
                    .then(function (response) {
                        // Handle success
                        self.loadingApplying = false;
                        self.clearMyInterval();
                        self.alertToast(self.$t("labelQuestionnaireAppliedSuccessfully"), "toast-success");
                        setTimeout(() => self.$emit("showHistory"), 3000);
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                        self.loadingApplying = false;
                        self.clearMyInterval();
                        if (e.response && e.response.data === "No Credits to send a Question") {
                            self.alertToast(self.$t("labelNumberOfQuestionsHasBeenExceeded"), "toast-danger");
                        } else if (e.response.status === 404) {
                            self.alertToast(self.$t("labelAnInconsistencyWasIdentifiedInTheDocument"), "toast-danger");
                        } else if (e.response.status === 402) {
                            self.alertToast(self.$t("labelThereIsNotEnoughCredit"), "toast-warning");
                            setTimeout(() => self.$emit("showHistory"), 3000);
                        } else {
                            self.alertToast(self.$t("labelFailedToApplyQuestionnaire"), "toast-danger");
                        }
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            submitInput: function () {
                this.loadingOutput = true;
                this.output = "";
                let paramsReq = {
                    id: this.idAnalyzer,
                    input: this.input,
                };
                this.input = "";
                let self = this;
                api.post("/Document/input/", paramsReq)
                    .then(function (response) {
                        // Handle success
                        if (typeof response.data === "object") {
                            self.output = JSON.stringify(response.data, undefined, 2);
                        } else {
                            self.output = response.data;
                        }
                        if (self.historyListOrder == "desc") {
                            self.$emit("unshiftHistoryList", { input: paramsReq.input, output: self.output });
                        } else {
                            self.$emit("pushHistoryList", { input: paramsReq.input, output: self.output });
                        }
                        self.loadingOutput = false;
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                        self.loadingOutput = false;
                        if (e.response.data === "No Credits to send a Question") {
                            self.output = "";
                            self.clearMyInterval();
                            self.alertToast(self.$t("labelNumberOfQuestionsHasBeenExceeded"), "toast-danger");
                        } else if (e.response.status === 404) {
                            self.output = self.$t("labelAnInconsistencyWasIdentifiedInTheDocument");
                        } else {
                            self.output = self.$t("labelFailedNoResponse");
                        }
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            openModal: function () {
                this.showModalQuestion = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal: function () {
                this.showModalQuestion = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            copyToClipboard: function (content) {
                navigator.clipboard.writeText(content);
                this.clearMyInterval();
                this.alertToast(this.$t("labelTextCopiedToClipboard"), "toast-primary");
            },
            alertToast: function (msg, color) {
                this.$emit("showAlertToast", { msg: msg, color: color });
            },
            clearMyInterval: function () {
                this.$emit("clearMyInterval");
            },
            upperFormat: function (str) {
                return str.toUpperCase();
            },
        },
        computed: {},
        created() {
            this.getQuizzes();
        },
        mounted() {},
        unmounted() {},
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
