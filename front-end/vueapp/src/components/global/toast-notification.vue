<template>
    <div class="position-fixed mt-5 end-0 p-3" style="z-index: 11" v-if="shouldShowNotification">
        <div
            id="liveToast"
            :class="showToast ? 'toast fade show' : 'toast fade hide'"
            role="alert"
            aria-live="assertive"
            aria-atomic="true"
        >
            <div class="toast-header">
                <strong class="me-5">
                    <i class="fas fa-bell bell-animated"></i>
                    {{
                        isFinished
                            ? errorUpload
                                ? $t("documents.errors.uploadError")
                                : $t("documents.uploadComplete")
                            : $t("common.almost")
                    }}
                </strong>
                <button
                    type="button"
                    class="chevron-btn"
                    @click="toggleExtraContent"
                    aria-label="Toggle"
                    style="margin-left: auto"
                >
                    <span :class="['chevron', { collapsed: !isExpanded }]" v-if="!isFinished">
                        <i class="fa fa-chevron-down"></i>
                    </span>
                </button>
                <button
                    type="button"
                    class="fa fa-times btn btn-color"
                    :title="$t('common.close')"
                    data-bs-dismiss="toast"
                    aria-label="Close"
                    @click="close"
                    style="padding: 0"
                ></button>
            </div>
            <div class="toast-body toast-upload" v-if="!isFinished">
                <a>{{ uploadingFiles }} -</a>
                <a>{{ maxSizeUpload }}</a>
                <a class="margin-label">{{ $t("documents.showingFilesUpload") }}</a>
                <div class="scroll-area">
                    <div :class="['extra-content', { show: isExpanded }]" v-for="(item, index) in dataUpload">
                        <div class="row">
                            <div class="col-2 mt-2">
                                <i class="far fa-file-pdf" style="font-size: 20px"></i>
                            </div>
                            <div class="col-7 mt-1">
                                {{ item.nameFile }}&ensp;
                                <span
                                    class="text-danger"
                                    style="font-size: smaller"
                                    v-if="!item.success && !item.isLoading"
                                >
                                    {{ $t("common.failed") }}
                                </span>
                                <div class="progress" style="height: 5px">
                                    <div
                                        class="progress-bar"
                                        role="progressbar"
                                        :aria-valuenow="item.countProgress * 10"
                                        aria-valuemin="0"
                                        :aria-valuemax="item.chunks * 10"
                                        :style="[{ width: item.countProgress * 10 + '%' }]"
                                    ></div>
                                </div>
                            </div>
                            <div class="col-2 mt-3">
                                <i class="far fa-check-circle text-success" v-if="item.success && !item.isLoading"></i>
                                <i
                                    class="fa fa-exclamation-triangle text-danger"
                                    v-if="!item.success && !item.isLoading"
                                ></i>
                                <div
                                    class="spinner-border spinner-border-sm text-primary"
                                    role="status"
                                    v-if="item.isLoading"
                                ></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="toast-body toast-success" v-if="isFinished && !errorUpload">
                <p>{{ $t("documents.uploadedFiles") }}</p>
            </div>
            <div class="toast-body toast-danger" v-if="isFinished && errorUpload">
                <p>{{ $t("documents.uploadedFilesError") }}</p>
            </div>
        </div>
    </div>
</template>
<script>
    import GlobalEventService from "@/services/globalEventService";

    export default {
        name: "ToastNotification",
        props: {
            showToast: {
                required: true,
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                title: "Toast Alert",
                isExpanded: false,
                dataUpload: [],
                isFinished: false,
                errorUpload: false,
                maxSizeUpload: 0,
                uploadingFiles: 0,
            };
        },
        components: {},
        watch: {},
        methods: {
            close: function () {
                this.$emit("close");
            },
            toggleExtraContent() {
                this.isExpanded = !this.isExpanded;
            },
            handleUploadComplete(payload) {
                this.dataUpload.forEach((upload) => {
                    if (payload.nameFile === upload.nameFile) {
                        if (upload.success && (payload.chunks == 1 || payload.chunks === upload.countProgress + 1)) {
                            upload.isLoading = false;
                            upload.chunks = 10;
                            upload.countProgress = 10;
                            upload.success = payload.success;
                        } else {
                            upload.isLoading = false;
                            upload.countProgress = 10;
                            upload.success = payload.success;
                        }
                    }
                });

                if (this.dataUpload.every((item) => item.isLoading === false && item.success)) {
                    this.isFinished = true;
                    this.errorUpload = false;
                    this.uploadingFiles = this.maxSizeUpload;
                    GlobalEventService.emit("all-uploads-complete", false);
                } else if (this.dataUpload.every((item) => item.isLoading === false && !item.success)) {
                    this.errorUpload = true;
                    this.isFinished = true;
                    this.uploadingFiles = this.maxSizeUpload;
                    GlobalEventService.emit("all-uploads-complete", false);
                } else if (this.dataUpload.every((item) => item.isLoading === false)) {
                    GlobalEventService.emit("refresh-once", false);
                }
            },
            handleUploadInProgress(payload) {
                this.dataUpload.forEach((upload) => {
                    if (payload.nameFile === upload.nameFile) {
                        if (upload.countProgress === 0) {
                            upload.chunks = payload.chunks;
                            upload.countProgress++;
                        } else if (upload.countProgress < payload.chunks - 1) {
                            upload.countProgress++;
                        } else if (upload.countProgress === payload.chunks - 1) {
                            upload.isLoading = false;
                            upload.countProgress = 10;
                        }
                    }
                });

                this.uploadingFiles = this.maxSizeUpload - this.dataUpload.filter((item) => item.isLoading).length;
            },
            handleUploadStarted(payload) {
                this.uploadingFiles = 0;
                this.isFinished = false;
                this.errorUpload = false;
                this.dataUpload = [];
                this.maxSizeUpload = payload.namesFiles.length;
                payload.namesFiles.forEach((nameFile) => {
                    this.dataUpload.push({
                        nameFile: nameFile,
                        chunks: 0,
                        success: payload.success,
                        countProgress: 0,
                        isLoading: true,
                    });
                });
            },
        },
        computed: {
            shouldShowNotification() {
                return this.$route.name !== "Login" && this.$route.name !== "Logout";
            },
        },
        created() {
            GlobalEventService.on("uploadInProgress", this.handleUploadInProgress);
            GlobalEventService.on("uploadComplete", this.handleUploadComplete);
            GlobalEventService.on("uploadStarted", this.handleUploadStarted);
        },
        mounted() {},
        unmounted() {},
    };
</script>

<style scoped>
    .toast {
        width: initial;
    }

    /* Bell animated  */
    .bell-animated {
        width: 15px;
        -webkit-animation-name: pendulum;
        animation-delay: 0s;
        -webkit-animation-duration: 1s;
        -webkit-animation-iteration-count: infinite;
        -webkit-animation-direction: linear;
        -webkit-transform-origin: 50% 0%;
        -webkit-animation-timing-function: ease-in-out;
    }

    @-moz-keyframes pendulum {
        0% {
            -webkit-transform: rotate(0deg);
        }

        10% {
            -webkit-transform: rotate(5deg);
        }

        20% {
            -webkit-transform: rotate(15deg);
        }

        30% {
            -webkit-transform: rotate(10deg);
        }

        40% {
            -webkit-transform: rotate(5deg);
        }

        50% {
            -webkit-transform: rotate(0deg);
        }

        60% {
            -webkit-transform: rotate(-5deg);
        }

        70% {
            -webkit-transform: rotate(-10deg);
        }

        80% {
            -webkit-transform: rotate(-15deg);
        }

        90% {
            -webkit-transform: rotate(-5deg);
        }

        100% {
            -webkit-transform: rotate(0deg);
        }
    }

    @-webkit-keyframes pendulum {
        0% {
            -webkit-transform: rotate(0deg);
        }

        10% {
            -webkit-transform: rotate(5deg);
        }

        20% {
            -webkit-transform: rotate(15deg);
        }

        30% {
            -webkit-transform: rotate(10deg);
        }

        40% {
            -webkit-transform: rotate(5deg);
        }

        50% {
            -webkit-transform: rotate(0deg);
        }

        60% {
            -webkit-transform: rotate(-5deg);
        }

        70% {
            -webkit-transform: rotate(-10deg);
        }

        80% {
            -webkit-transform: rotate(-15deg);
        }

        90% {
            -webkit-transform: rotate(-5deg);
        }

        100% {
            -webkit-transform: rotate(0deg);
        }
    }

    @keyframes pendulum {
        0% {
            -webkit-transform: rotate(0deg);
        }

        10% {
            -webkit-transform: rotate(5deg);
        }

        20% {
            -webkit-transform: rotate(15deg);
        }

        30% {
            -webkit-transform: rotate(10deg);
        }

        40% {
            -webkit-transform: rotate(5deg);
        }

        50% {
            -webkit-transform: rotate(0deg);
        }

        60% {
            -webkit-transform: rotate(-5deg);
        }

        70% {
            -webkit-transform: rotate(-10deg);
        }

        80% {
            -webkit-transform: rotate(-15deg);
        }

        90% {
            -webkit-transform: rotate(-5deg);
        }

        100% {
            -webkit-transform: rotate(0deg);
        }
    }

    .chevron-btn {
        border: none;
        background: none;
        cursor: pointer;
    }

    .chevron {
        display: inline-block;
        width: 1em;
        height: 1em;
        transform: rotate(180deg);
        transition: transform 0.3s ease;
        margin-top: 60%;
        color: var(--color-toast-content) !important;
    }

    .chevron.collapsed {
        transform: rotate(0deg);
        color: var(--color-toast-content) !important;
    }

    .extra-content {
        display: none;
    }

    .extra-content.show {
        display: block;
    }

    .btn-color {
        color: var(--color-toast-content) !important;
    }

    .scroll-area {
        overflow-y: auto;
        overflow-x: hidden;
        max-height: 120px;
    }
</style>
