<template>
    <main class="flex-shrink-0 overlay">
        <div class="container mb-5">
            <div class="row justify-content-md-center" style="height: 100%">
                <div class="col-md-auto">
                    <div class="div-center">
                        <div v-if="loading">
                            <div class="mb-3" style="width: 100%; float: left">
                                <h5 class="h5-custom-modal" v-html="message"></h5>
                            </div>
                            <div style="text-align: center">
                                <img
                                    svg-inline
                                    src="@/assets/img/icon-load-circle.svg"
                                    alt="Loading"
                                    width="60"
                                    class="refresh-animated"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
    <!-- Component ModalAlert -->
    <modal-alert
        v-if="modalAlertShow"
        :type="'Error'"
        :alertTitle="$t('documents.failedToNormalize')"
        :alertMessage="$t('documents.theFileMayBeUnreadableOrHaveAnError')"
        :okLabel="$t('common.close')"
        @close="closeModal"
    />
</template>
<script>
    import ModalAlert from "@/components/pages/analyzer/modal-alert";
    import api from "@/services/api";

    export default {
        name: "NormalizeIndex",
        props: {
            docData: {
                required: true,
                type: Object,
                default: {},
            },
            isReprocessing: {
                required: true,
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                backPage: this.$route.query.page,
                title: "Normalize Index",
                loading: true,
                message: "",
                modalAlertShow: false,
                myInterval: null,
                timeoutMessage: ENV_CONFIG.VUE_APP_WAITING_TIME_MSG_UPLD,
            };
        },
        components: {
            ModalAlert,
        },
        watch: {},
        methods: {
            verifyNormalizedDoc: function () {
                let self = this;
                api.get("/Document/Status/" + this.docData.Id)
                    .then(function (response) {
                        // Handle success
                        if (response.data.status === 0) {
                            // Status not analyzed
                            self.message = self.$t("documents.normalizingTheDocument");
                            self.normalizeDoc();
                        } else {
                            if (self.isReprocessing) {
                                self.message = self.$t("documents.normalizingTheDocument");
                                self.normalizeDoc();
                            } else {
                                self.message = self.$t("documents.documentHasAlreadyBeenStandardizedPreviously", [
                                    response.data.name,
                                ]);
                                self.redirectToDocument();
                            }
                        }
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
            normalizeDoc() {
                window.onbeforeunload = function () {
                    return true;
                };
                let self = this;
                let paramsReq = {
                    Id: this.docData.Id,
                    Embeddings_model_name: this.docData.Embeddings_model_name,
                };
                this.loading = true;
                api.post("/Document/Analyze/", paramsReq)
                    .then(function (response) {
                        // Handle success
                        window.onbeforeunload = null;
                        if (self.isReprocessing) {
                            location.reload();
                        } else {
                            self.redirectToAnalyzer();
                        }
                    })
                    .catch(function (e) {
                        // Handle error
                        window.onbeforeunload = null;
                        console.log(e);
                        self.loading = false;
                        self.showModal();
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                        self.loading = false;
                    });
            },
            redirectToAnalyzer() {
                let self = this;
                setTimeout(function () {
                    self.$router.push({
                        name: "Analyzer",
                        params: { id: self.docData.Id },
                        query: { page: self.backPage },
                    });
                }, 500);
            },
            redirectToDocument() {
                let self = this;
                setTimeout(function () {
                    self.$router.push({ name: "Documents", query: { page: self.backPage } });
                }, 6000);
            },
            showModal() {
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal() {
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
                location.reload();
            },
        },
        computed: {},
        created() {
            this.message = this.$t("documents.preparingTheDocument");
            this.verifyNormalizedDoc();
        },
        mounted() {},
        unmounted() {},
    };
</script>

<style scoped>
    .div-center {
        position: relative;
        top: 50%;
        left: 50%;
        -webkit-transform: translate(-50%, -50%);
        transform: translate(-50%, -50%);
        /*width: 700px;*/
    }

    .h5-custom-modal {
        font-weight: initial;
        color: #0073e6;
        text-align: center;
    }

    /* Refresh animated  */
    .refresh-animated {
        -webkit-animation: spin 2s linear infinite;
        -moz-animation: spin 2s linear infinite;
        animation: spin 2s linear infinite;
    }

    @-moz-keyframes spin {
        100% {
            -moz-transform: rotate(360deg);
        }
    }

    @-webkit-keyframes spin {
        100% {
            -webkit-transform: rotate(360deg);
        }
    }

    @keyframes spin {
        100% {
            -webkit-transform: rotate(360deg);
            transform: rotate(360deg);
        }
    }
    .overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        background-color: var(--color-bg-body-content) !important;
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 9998;
    }
</style>
