<template>
    <div class="col-md-6">
        <div class="mb-2" style="margin-top: 12px !important">
            <div v-if="isPdf">
                <strong class="form-label mb-1">PDF ORIGINAL&nbsp;&nbsp;</strong>
                <a @click="openTab" v-if="srcPdf">
                    <i class="fas fa-expand text-primary" style="cursor: pointer" :title="$t('labelExpand')"></i>
                </a>
                <img
                    src="../../../assets/img/go-to-text.png"
                    @click="getDocumentNormalized"
                    style="cursor: pointer; float: right"
                    :title="$t('labelDocumentTranscript')"
                    v-if="srcPdf"
                />
                <button type="button" class="btn btn-primary btn-sm mb-1 reindex-button" @click="openModal()">
                    <i class="fas fa-sync-alt"></i>
                    {{ $t("labelReprocess") }}
                </button>
                <div class="view-pdf" v-if="srcPdf">
                    <object :data="srcPdf + `#zoom=80`" type="application/pdf" width="100%" height="100%">
                        <embed :src="srcPdf" type="application/pdf" />
                    </object>
                </div>
                <div
                    class="mt-1 p-2"
                    v-if="errorPdf"
                    style="border: 1px solid #dc3545; text-align: center; cursor: pointer"
                    @click="reloadPage"
                >
                    <span class="text-danger" style="text-decoration: none">
                        <i class="fas fa-exclamation-circle"></i>
                        {{ $t("labelAttentionPDFDisplayFailed") }}.
                    </span>
                </div>
                <div class="mt-1 p-2 loading-div" v-if="loading" @click="reloadPage">
                    <div
                        class="spinner-border spinner-border-sm text-primary"
                        style="margin-right: 1%"
                        role="status"
                        v-if="loading"
                    ></div>
                    <span class="text-primary" style="text-decoration: none">
                        {{ $t("labelLoadingFilePleaseWait") }}.
                    </span>
                </div>
            </div>
            <div v-else>
                <div>
                    <strong class="form-label mb-3">
                        {{ upperFormat($t("labelStandardizedFullText")) }}&nbsp;&nbsp;
                    </strong>
                    <i class="fas fa-spinner fa-pulse text-primary" v-if="loadingDocumentNormalized"></i>
                    <img
                        src="../../../assets/img/go-to-pdf.png"
                        @click="isPdf = true"
                        style="cursor: pointer; float: right"
                        :title="$t('labelPdfBack')"
                    />
                </div>
                <textarea
                    type="text"
                    class="form-control custom-textarea textarea-norm-full"
                    v-model="contentDocumentNormalized"
                    readonly
                ></textarea>
            </div>
        </div>
        <modal-reprocess v-if="showModalForm" @close="closeModal" @formatData="updateModel" />
    </div>
</template>

<script>
    import DocumentsServices from "@/services/documents/DocumentsServices.js";
    import ModalReprocess from "@/components/pages/analyzer/modal-reprocess";
    import ModalAlert from "@/components/common/modal-alert";

    export default {
        name: "DocView",
        data() {
            return {
                idAnalyzer: this.$route.params.id,
                isPdf: true,
                srcPdf: null,
                errorPdf: false,
                loading: true,
                loadingDocumentNormalized: false,
                contentDocumentNormalized: "",
                controllAttempt: 0,
                showModalForm: false,
                showLoading: false,
                message: "",
                loadingNormalize: false,
                modalAlertShow: false,
                dataView: {
                    Id: parseInt(this.idAnalyzer),
                    Embeddings_model_name: "",
                },
                isReprocessing: true,
            };
        },
        components: {
            ModalReprocess,
            ModalAlert,
        },
        methods: {
            getDocument() {
                this.controllAttempt++;
                this.srcPdf = null;
                this.errorPdf = false;
                DocumentsServices.findDocument(this.idAnalyzer)
                    .then((response) => {
                        if (response.error !== undefined) {
                            if (this.controllAttempt < attempt) {
                                this.getPdf(fileGuidId, attempt);
                            } else {
                                this.loading = false;
                                this.errorPdf = true;
                            }
                            return console.log(response.error);
                        }

                        this.srcPdf = window.URL.createObjectURL(new Blob([response], { type: "application/pdf" }));
                        this.loading = false;
                    })
                    .finally(() => {
                        console.log("Finished request.");
                    });
            },
            updateModel(model) {
                this.dataView.Embeddings_model_name = model;
                this.$emit("showNormalize", this.dataView, this.isReprocessing);
            },
            getDocumentNormalized() {
                this.loadingDocumentNormalized = false;
                this.isPdf = false;
                if (this.contentDocumentNormalized == "") {
                    this.loadingDocumentNormalized = true;
                    DocumentsServices.getNormalizedDocument(this.idAnalyzer)
                        .then((response) => {
                            if (response.error !== undefined) {
                                this.modalAlertShow = true;
                                this.loadingDocumentNormalized = false;
                            }
                            this.contentDocumentNormalized = response.content;
                            this.loadingDocumentNormalized = false;
                        })
                        .finally(() => {
                            console.log("Finished request.");
                        });
                }
            },
            openTab() {
                window.open(this.srcPdf, "_blank");
            },
            upperFormat(str) {
                return str.toUpperCase();
            },
            reloadPage() {
                location.reload();
            },
            openModal() {
                this.showModalForm = true;
                this.dataView.Id = parseInt(this.idAnalyzer);
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal() {
                this.showModalForm = false;
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            normalizeDoc() {
                window.onbeforeunload = function () {
                    return true;
                };
                let paramsReq = {
                    Id: parseInt(this.idAnalyzer),
                    Embeddings_model_name: "",
                };
                this.loadingNormalize = true;
                DocumentsServices.normalizeDocument(paramsReq)
                    .then((response) => {
                        if (response.error !== undefined) {
                            window.onbeforeunload = null;
                            return console.log(response.error);
                        }
                        window.onbeforeunload = null;
                        this.message = this.$t("labelNormalizingTheDocument");
                    })
                    .finally(() => {
                        console.log("Finished request.");
                        this.loadingNormalize = false;
                    });
            },
        },
        created() {
            this.getDocument();
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

    .custom-textarea {
        border-color: #0073e6 !important;
    }

    .view-pdf {
        width: 100% !important;
        height: calc(100vh - 138px) !important;
    }

    .textarea-norm-full {
        height: calc(100vh - 138px) !important;
    }

    @media (min-width: 768px) and (max-width: 1024px) {
        .view-pdf {
            height: calc(100vh - 300px) !important;
        }
    }

    .overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        background-color: rgba(0, 0, 0, 0.7);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 9999;
    }

    .overlay-content {
        background-color: #fff;
        padding: 20px;
        border-radius: 5px;
        text-align: center;
    }

    .loading-div {
        border: 1px solid #0d6efd;
        text-align: center;
        cursor: pointer;
    }

    .reindex-button {
        margin-left: 5%;
    }
</style>
