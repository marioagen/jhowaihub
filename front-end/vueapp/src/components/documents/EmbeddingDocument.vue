<template>
    <main class="flex-shrink-0 overlay">
        <div class="container mb-5">
            <div class="row justify-content-md-center" style="height: 100%">
                <div class="col-md-auto">
                    <div class="div-center">
                        <div v-if="isLoading">
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
</template>
<script>
    import NormalizeServices from "@/services/documents/NormalizeServices";
    export default {
        name: "EmbeddingDocument",
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
                isLoading: true,
                message: "",
                timeoutMessage: ENV_CONFIG.VUE_APP_WAITING_TIME_MSG_UPLD,
            };
        },
        components: {
        },
        methods: {
            verifyNormalizedDoc() {
                NormalizeServices.VerifyNormalize(this.docData.Id)
                    .then((response) => {
                        if (response.status === 0) {
                            this.message = this.$t("labelNormalizingTheDocument");
                            this.normalizeDoc();
                        } else {
                            if (this.isReprocessing) {
                                this.message = this.$t("labelNormalizingTheDocument");
                                this.normalizeDoc();
                            } else {
                                this.message = this.$t("labelDocumentHasAlreadyBeenStandardizedPreviously", [
                                    response.name,
                                ]);
                                this.redirectToDocument();
                            }
                        }
                    });
            },
            normalizeDoc() {
                window.onbeforeunload = function () {
                    return true;
                };

                let paramsReq = {
                    Id: this.docData.Id,
                    Embeddings_model_name: this.docData.Embeddings_model_name,
                };

                this.isLoading = true;
                NormalizeServices.AnalyzeDocument(paramsReq)
                    .then((response) => {
                        window.onbeforeunload = null;
                        if(response.error !== undefined) {
                            return this.$notify({
                                title: this.$t('documents.failedToNormalize'),
                                message: this.$t('documents.theFileMayBeUnreadableOrHaveAnError'),
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        if (this.isReprocessing) {
                            location.reload();
                        } else {
                            this.redirectToAnalyzer();
                        }
                    })
                    .finally(() => {
                        this.isLoading = false;
                    })
            },
            redirectToAnalyzer() {
                setTimeout(() => {
                    this.$router.push({
                        name: "Analyzer",
                        params: { 
                            id: this.docData.Id 
                        },
                        query: { 
                            page: this.backPage 
                        },
                    });
                }, 500);
            },
            redirectToDocument() {
                setTimeout(() => {
                    this.$router.push({ name: "Documents", query: { page: this.backPage } });
                }, 6000);
            },
        },
        created() {
            this.message = this.$t("documents.preparingTheDocument");
            this.verifyNormalizedDoc();
        },
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
