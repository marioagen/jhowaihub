<template>
    <div
        class="doc-view-scroll"
        :class="fillContainer ? 'w-100' : documentView === 'both' ? 'col-md-6' : 'col-12'"
    >
        <div
            class="mb-2"
            style="margin-top: 12px !important"
        >
            <div v-if="viewMode === $options.VIEW_MODE_PDF">
                <strong class="form-label mb-1">PDF ORIGINAL&nbsp;&nbsp;</strong>
                <a
                    @click="openTab"
                    v-if="srcPdf"
                >
                    <i
                        class="fas fa-expand text-primary"
                        style="cursor: pointer"
                        :title="$t('common.expand')"
                    ></i>
                </a>
                <img
                    src="@/assets/img/go-to-text.png"
                    @click="toggleToText"
                    style="cursor: pointer; float: right"
                    :title="$t('documents.ocrText')"
                    v-if="srcPdf && hasOcrText"
                />
                <div
                    class="view-pdf"
                    v-if="srcPdf"
                >
                    <object
                        :data="srcPdf + `#zoom=80`"
                        type="application/pdf"
                        width="100%"
                        height="100%"
                    >
                        <embed
                            :src="srcPdf"
                            type="application/pdf"
                        />
                    </object>
                </div>
                <div
                    class="mt-1 p-2"
                    v-if="errorPdf"
                    style="border: 1px solid #dc3545; text-align: center; cursor: pointer"
                    @click="reloadPage"
                >
                    <span
                        class="text-danger"
                        style="text-decoration: none"
                    >
                        <i class="fas fa-exclamation-circle"></i>
                        {{ $t("documents.attentionPDFDisplayFailed") }}.
                    </span>
                </div>
                <div
                    class="mt-1 p-2 loading-div"
                    v-if="loading"
                    @click="reloadPage"
                >
                    <div
                        class="spinner-border spinner-border-sm text-primary"
                        style="margin-right: 1%"
                        role="status"
                        v-if="loading"
                    ></div>
                    <span
                        class="text-primary"
                        style="text-decoration: none"
                    >
                        {{ $t("documents.loadingFilePleaseWait") }}.
                    </span>
                </div>
            </div>
            <div
                v-else-if="viewMode === $options.VIEW_MODE_TEXT"
                class="scroll-text"
            >
                <div>
                    <strong class="form-label mb-3">
                        {{ upperFormat($t("documents.ocrText")) }}&nbsp;&nbsp;
                    </strong>
                    <i
                        class="fas fa-spinner fa-pulse text-primary"
                        v-if="loadingText"
                    ></i>
                    <img
                        src="@/assets/img/go-to-pdf.png"
                        @click="viewMode = $options.VIEW_MODE_PDF"
                        style="cursor: pointer; float: right"
                        :title="$t('documents.pdfBack')"
                    />
                </div>
                <textarea
                    type="text"
                    class="form-control custom-textarea textarea-norm-full"
                    v-model="textContent"
                    readonly
                ></textarea>
            </div>
        </div>
    </div>
</template>
<script>
    import DocumentsServices from "@/services/documents/DocumentsServices.js";
    import LogService from "@/services/log/logService";

    const VIEW_MODE_PDF = "pdf";
    const VIEW_MODE_TEXT = "text";

    export default {
        name: "DocumentViewer",
        VIEW_MODE_PDF,
        VIEW_MODE_TEXT,
        props: {
            documentView: {
                type: String,
                required: true,
            },
            /** When true, component fills its container (e.g. inside resizable split). */
            fillContainer: {
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                documentId: this.$route.params.documentId,
                viewMode: VIEW_MODE_PDF,
                srcPdf: null,
                errorPdf: false,
                loading: true,
                loadingText: false,
                textContent: "",
                hasOcrText: false,
                showLoading: false,
                message: "",
                loadingNormalize: false,
                modalAlertShow: false,
                dataView: {
                    Id: parseInt(this.documentId),
                    Embeddings_model_name: "",
                },
                isReprocessing: true,
            };
        },
        methods: {
            getDocument() {
                this.srcPdf = null;
                this.errorPdf = false;
                DocumentsServices.findDocument(this.documentId).then((response) => {
                    if (response.error !== undefined) {
                        return this.$notify({
                            title: "analyze.title",
                            message: "analyze.failedLoadDocument",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }

                    this.srcPdf = window.URL.createObjectURL(
                        new Blob([response], {
                            type: "application/pdf",
                        })
                    );
                    this.loading = false;
                });
            },
            updateModel(model) {
                this.dataView.Embeddings_model_name = model;
                this.$emit("showNormalize", this.dataView, this.isReprocessing);
            },
            toggleToText() {
                this.viewMode = VIEW_MODE_TEXT;
                if (this.textContent == "") {
                    this.loadingText = true;
                    DocumentsServices.getOcrText(this.documentId)
                        .then((response) => {
                            if (response.error !== undefined) {
                                this.loadingText = false;
                                return;
                            }
                            if (response.hasOcr) {
                                this.textContent = response.content;
                            } else {
                                this.textContent = this.$t("documents.ocrNotAvailable");
                            }
                            this.loadingText = false;
                        })
                        .catch((error) => {
                            this.textContent = this.$t("documents.ocrLoadError");
                            this.loadingText = false;
                        });
                }
            },
            checkOcrAvailability() {
                DocumentsServices.getOcrText(this.documentId)
                    .then((response) => {
                        if (response && response.hasOcr) {
                            this.hasOcrText = true;
                        }
                    })
                    .catch((error) => {
                        LogService.showMessage(this.$t("documents.ocrFetchError"));
                    });
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
            normalizeDoc() {
                window.onbeforeunload = function () {
                    return true;
                };
                let paramsReq = {
                    Id: parseInt(this.documentId),
                    Embeddings_model_name: "",
                };
                this.loadingNormalize = true;
                DocumentsServices.normalizeDocument(paramsReq)
                    .then((response) => {
                        if (response.error !== undefined) {
                            window.onbeforeunload = null;
                        }
                        window.onbeforeunload = null;
                        this.message = this.$t("documents.normalizingTheDocument");
                    })
                    .finally(() => {
                        this.loadingNormalize = false;
                    });
            },
        },
        created() {
            this.getDocument();
            this.checkOcrAvailability();
        },
    };
</script>
<style scoped>
    .fas,
    .far {
        font-weight: 900 !important;
    }

    .scroll-text {
        overflow-y: auto;
        max-height: 65vh;
    }

    .text-primary {
        color: #47aaff !important;
    }

    .custom-textarea {
        border-color: #0073e6 !important;
    }

    .view-pdf {
        width: 100% !important;
        height: auto !important;
        max-height: 65vh;
        min-height: 300px;
        aspect-ratio: 1/1.414;
        display: flex;
        justify-content: center;
        align-items: center;
    }
    @media (max-width: 768px) {
        .view-pdf {
            width: 90% !important;
        }
    }

    .view-pdf object,
    .view-pdf embed {
        width: 100% !important;
        height: 100% !important;
        max-width: 100%;
        max-height: 70vh;
        display: block;
    }

    .textarea-norm-full {
        height: calc(100vh - 138px) !important;
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
