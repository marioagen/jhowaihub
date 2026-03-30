<template>
    <main
        class="flex-shrink-0"
        v-if="isLoading"
    >
        <div class="container mb-5">
            <div class="row justify-content-md-center full-height">
                <div class="col-md-auto">
                    <div class="div-center">
                        <div>
                            <div class="content-box">
                                <h5 class="h5-custom-modal">
                                    {{ message }}
                                </h5>
                            </div>
                            <div class="text-center">
                                <img
                                    svg-inline
                                    src="@/assets/img/icon-load-circle.svg"
                                    alt="isLoading"
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
    <main
        class="main-scroll"
        v-if="!isLoading"
    >
        <div class="container-fluid mt-4">
            <div clas="align-items-center">
                <div class="row">
                    <div class="col-1">
                        <button
                            class="btn btn-outline-primary btn-table btn-sm table-btn"
                            @click="backToListDocuments"
                        >
                            <LucideIcon icon="ArrowLeft" />
                        </button>
                    </div>
                    <div class="col-10">
                        <div>
                            <h5 class="mb-1">
                                {{ $t("documents.upload.title") }}
                            </h5>
                            <p>
                                <small class="text-muted">
                                    {{ $t("documents.upload.subtitle") }}
                                </small>
                            </p>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12 mb-5">
                        <div class="box-upload-form">
                            <form
                                class="form-upload"
                                @submit.prevent="save"
                            >
                                <div class="d-flex justify-content-between">
                                    <h5 class="mb-4">
                                        {{ $t("documents.upload.cardTitle") }}
                                    </h5>
                                    <div class="form-check d-flex align-items-center mb-3">
                                        <input
                                            class="form-check-input me-2"
                                            type="checkbox"
                                            id="isDocumentBatchChk"
                                            :value="isDocumentsBatch"
                                            v-model="isDocumentsBatch"
                                        />
                                        <label
                                            class="form-check-label d-flex align-items-center w-100"
                                            for="isDocumentBatchChk"
                                        >
                                            <div class="fw-semibold">
                                                {{ $t("documents.documentsBatchCheckbox") }}
                                            </div>
                                        </label>
                                    </div>
                                </div>
                                <div class="col-lg-12 col-md-12 col-sm-12 mb-3">
                                    <label class="label-container mb-2">
                                        {{ $t("documents.upload.dropZone") }}
                                        <span class="clear-button">
                                            <img
                                                src="@/assets/img/icon-dropzone-remove-all.svg"
                                                alt="Remove All"
                                                :title="$t('documents.upload.removeAllDropzone')"
                                                @click="confirmationDialog()"
                                            />
                                        </span>
                                    </label>
                                    <div
                                        action="/file-upload"
                                        class="dropzone"
                                        id="dropzoneUpload"
                                        ref="dropzone"
                                    ></div>
                                </div>
                                <div class="mb-1">
                                    <label
                                        class="form-label"
                                        for="descId"
                                    >
                                        {{ $t("documents.descriptionDocumentNote") }}
                                    </label>
                                    <textarea
                                        rows="5"
                                        id="descId"
                                        v-validate
                                        name="text"
                                        v-model="form.description"
                                        required
                                        :class="{
                                            'form-control': true,
                                            'red-warning': form.description.length > 250,
                                        }"
                                    >
                                ></textarea
                                    >
                                </div>
                                <div class="mb-3">
                                    <div>
                                        <a
                                            :class="[
                                                'char-counter',
                                                form.description.length <= 250
                                                    ? 'char-normal'
                                                    : 'char-error',
                                            ]"
                                        >
                                            {{ 250 - form.description.length }}
                                            {{
                                                250 - form.description.length === 1
                                                    ? $t("common.character")
                                                    : $t("common.characters")
                                            }}
                                        </a>
                                        <a
                                            v-if="form.description.length > 250"
                                            class="char-counter char-error exceedDesc"
                                        >
                                            {{ $t("documents.descriptionExceeded") }}
                                        </a>
                                    </div>
                                </div>
                                <div
                                    class="mb-3 team-selector-container rounded p-3"
                                    :class="{
                                        'is-invalid': hasError,
                                        'is-valid': !hasError,
                                    }"
                                >
                                    <div
                                        class="d-flex justify-content-between align-items-center mb-1"
                                    >
                                        <div class="d-flex align-items-center mb-1">
                                            <LucideIcon
                                                icon="Building"
                                                class="icon-blue"
                                            />
                                            <label class="form-label mb-0 ms-2">
                                                {{ $t("documents.upload.linkWorkflow") }}
                                            </label>
                                        </div>
                                        <span class="selected-count">
                                            {{ selectedWorkflows.length }}
                                            {{ $t("common.selected") }}
                                        </span>
                                    </div>
                                    <div class="text-muted small mb-3">
                                        {{ $t("documents.upload.linkSubtitle") }}
                                    </div>
                                    <div
                                        v-if="hasError"
                                        class="text-danger small mb-3 d-flex align-items-center gap-1"
                                    >
                                        <span class="text-danger">*</span>
                                        <span>
                                            {{ $t("validation.required") }}
                                        </span>
                                    </div>
                                    <div
                                        v-if="selectedWorkflows.length > 0"
                                        class="mb-3"
                                    >
                                        <label class="form-label">
                                            {{ $t("documents.upload.selectionList") }}
                                        </label>
                                        <div class="d-flex flex-wrap gap-2">
                                            <div
                                                v-for="id in selectedWorkflowsOrderedByName"
                                                :key="id"
                                                class="badge rounded-pill d-flex align-items-center px-2 py-1 selected-team-chip"
                                            >
                                                <LucideIcon
                                                    icon="Building"
                                                    class="me-1"
                                                />
                                                <span class="me-1">
                                                    {{ getName(id) }}
                                                </span>
                                                <button
                                                    type="button"
                                                    class="chip-remove-btn"
                                                    :title="$t('documents.upload.removeWorkflowChip')"
                                                    :aria-label="$t('documents.upload.removeWorkflowChip')"
                                                    @click.stop="removeWorkflowFromSelection(id)"
                                                >
                                                    <LucideIcon
                                                        icon="X"
                                                        :size="14"
                                                    />
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="mb-3 rounded">
                                        <div class="input-group">
                                            <span class="input-group-text border-end-0">
                                                <LucideIcon
                                                    icon="Search"
                                                    size="16"
                                                />
                                            </span>
                                            <input
                                                type="text"
                                                class="form-control form-control-sm"
                                                :placeholder="
                                                    $t(
                                                        'documents.workflowListModal.searchPlaceholder'
                                                    )
                                                "
                                                v-model="searchTerm"
                                            />
                                        </div>
                                    </div>
                                    <div class="mb-1 d-flex gap-2 p-2 rounded">
                                        <button
                                            type="button"
                                            class="btn btn-custom-light btn-sm"
                                            @click="selectAll($event)"
                                        >
                                            <LucideIcon
                                                icon="Check"
                                                class="me-1"
                                            />
                                            {{ $t("common.selectAll") }}
                                        </button>
                                        <button
                                            type="button"
                                            class="btn btn-custom-light btn-sm"
                                            @click="clearSelection($event)"
                                        >
                                            <LucideIcon
                                                icon="X"
                                                class="me-1"
                                            />
                                            {{ $t("common.clearSelection") }}
                                        </button>
                                    </div>
                                    <div class="text-muted small mb-1">
                                        {{ $t("documents.upload.warningWorkflowNotListed") }}
                                    </div>
                                    <div
                                        class="border rounded bg-select p-1 user-list scrollable-list"
                                    >
                                        <div
                                            v-if="isLoading"
                                            class="text-center"
                                        >
                                            <div
                                                class="spinner-border text-primary"
                                                role="status"
                                            >
                                                <span class="visually-hidden">
                                                    {{ $t("common.loading") }}
                                                </span>
                                            </div>
                                        </div>
                                        <div
                                            v-else-if="filtersWorkflowList.length === 0"
                                            class="text-center text-muted py-3"
                                        >
                                            {{ $t("documents.upload.noWorkflowFound") }}
                                        </div>
                                        <div
                                            v-if="!isLoading"
                                            v-for="team in filtersWorkflowList"
                                            :key="team.id"
                                            class="p-1"
                                        >
                                            <div class="form-check d-flex align-items-center">
                                                <input
                                                    class="form-check-input me-3"
                                                    type="checkbox"
                                                    :id="`user-${team.id}`"
                                                    :value="team.id"
                                                    v-model="selectedWorkflows"
                                                />
                                                <label
                                                    class="form-check-label d-flex align-items-center w-100"
                                                    :for="`user-${team.id}`"
                                                >
                                                    <div class="fw-semibold">
                                                        {{ team.name }}
                                                    </div>
                                                </label>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <button
                                    type="submit"
                                    class="btn btn-primary m-2 float-right"
                                    :title="$t('common.send')"
                                    :disabled="form.description.length > 250"
                                >
                                    {{ $t("common.send") }}
                                </button>
                                <router-link
                                    class="btn btn-secondary m-2 btn-custom-cancel float-right"
                                    :to="{
                                        name: 'Documents',
                                        query: {
                                            page: '1',
                                        },
                                    }"
                                    :title="$t('common.cancel')"
                                >
                                    {{ $t("common.cancel") }}
                                </router-link>
                            </form>
                        </div>
                    </div>
                    <div class="col-md-6"></div>
                </div>
            </div>
        </div>
        <ConfirmModal
            id="deleteConfirm"
            title="documents.upload.removeAllDropzone"
            message="documents.thisActionRemoveAllFiles"
            cancelText="common.cancel"
            confirmText="common.confirm"
            confirmVariant="primary"
            ref="DeleteDialog"
            :isLoading="isLoading"
            @confirm="removeAllFiles"
        />
    </main>
</template>
<script>
    import "dropzone/dist/dropzone.css";
    import api from "@/services/api";
    import Dropzone from "dropzone";
    import uploadFileWorker from "@/workers";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";

    export default {
        name: "DocumentUpload",
        directives: {
            validate: {
                inserted: function (el, binding) {
                    el.addEventListener("input", function () {
                        el.setCustomValidity("");
                        if (!el.checkValidity()) {
                            el.reportValidity();
                        }
                    });

                    el.addEventListener("invalid", function (event) {
                        event.preventDefault();
                        if (el.validity.valueMissing) {
                            el.setCustomValidity(this.$t("validation.fillInThisField"));
                        }
                        el.reportValidity();
                    });
                },
            },
        },
        data() {
            return {
                timeoutMessage: ENV_CONFIG.VUE_APP_WAITING_TIME_MSG_UPLD,
                timerReq: ENV_CONFIG.VUE_APP_TIMER_REQ,
                maxFiles: 10000000000,
                filesList: [],
                url: null,
                form: {
                    IDEA: "",
                    description: "",
                    emailCreator: "",
                },
                searchTerm: "",
                isLoading: false,
                message: "",
                myInterval: null,
                fileUpload: null,
                chunks: [],
                teams: [],
                workflowsList: [],
                dropzoneInstance: null,
                selectedWorkflows: [],
                hasError: true,
                isDocumentsBatch: false,
            };
        },
        components: {
            ConfirmModal,
        },
        watch: {
            selectedWorkflows() {
                this.validateSelection();
            },
        },
        methods: {
            initializeDropzone() {
                this.dropzoneInstance = new Dropzone("#dropzoneUpload", {
                    paramName: "file",
                    maxFilesize: 10000000000,
                    acceptedFiles: ".pdf",
                    maxFiles: this.maxFiles,
                    autoProcessQueue: false,
                    addRemoveLinks: true,
                });
                this.dropzoneInstance.on("addedfile", this.onFileAdded);
                this.dropzoneInstance.on("removedfile", this.onFileRemove);
            },
            getName(id) {
                const workflow = this.workflowsList.find((t) => t.id === id);
                return workflow ? workflow.name : "Desconhecido";
            },
            selectAll(event) {
                event.target.blur();
                this.selectedWorkflows = this.filtersWorkflowList.map((user) => user.id);
            },
            clearSelection(event) {
                event.target.blur();
                this.selectedWorkflows = [];
            },
            removeWorkflowFromSelection(id) {
                this.selectedWorkflows = this.selectedWorkflows.filter((wId) => wId !== id);
            },
            validateSelection() {
                this.hasError = this.selectedWorkflows.length === 0;
            },
            clickUplodFile() {
                document.getElementById("inputFileId").click();
            },
            onFileAdded(message) {
                if (this.filesList.some((f) => f.name == message.name)) {
                    this.dropzoneInstance.removeFile(message);
                } else {
                    this.filesList.push(message);
                }
                this.updateDropzoneState();
            },
            removeFile(uuid) {
                this.filesList = this.filesList.filter((fileObj) => fileObj.upload.uuid != uuid);
                this.updateDropzoneState();
            },
            removeAllFiles() {
                this.dropzoneInstance.removeAllFiles();
                this.filesList = [];
                this.updateDropzoneState();
                this.$refs.DeleteDialog?.close();
            },
            updateDropzoneState() {
                const dropzone = this.$refs.dropzone;
                if (this.filesList.length > 0) {
                    dropzone.classList.add("files-added");
                } else {
                    dropzone.classList.remove("files-added");
                }
            },
            onFileRemove(message) {
                const fileId = message.upload.uuid;
                this.removeFile(fileId);
            },
            checkExceededPages() {
                api.get("/Document/CheckExceededPages")
                    .then((response) => {
                        if (response.data === true) {
                            this.$notify({
                                title: "documents.numberOfPagesHasBeenExceeded",
                                message: "documents.numberOfPagesHasBeenExceeded",
                                variant: "warning",
                                icon: "CircleX",
                            });
                        }
                    })
                    .catch((e) => {
                        console.log(e);
                    });
            },
            validateForm() {
                let valid = true;
                if (this.filesList.length == 0) {
                    this.$notify({
                        title: "documents.upload.noFileChosen",
                        message: "documents.upload.noFileChosen",
                        variant: "warning",
                        icon: "CircleX",
                    });
                    valid = false;
                } else if (this.selectedWorkflows.length == 0) {
                    this.$notify({
                        title: "documents.upload.noTeamChosen",
                        message: "documents.upload.noTeamChosen",
                        variant: "warning",
                        icon: "CircleX",
                    });
                    valid = false;
                }
                return valid;
            },
            save(e) {
                e.preventDefault();
                if (!this.validateForm()) return;
                window.onbeforeunload = function () {
                    return true;
                };
                this.message = this.$t("documents.sendingTheDocument");
                this.isLoading = true;

                const apiHeaders = {
                    "X-Email": this.$store.state.userProfile.login,
                    "X-Tenant": this.$store.state.userProfile.tenant,
                    "X-Key-Mongo-Access": this.$store.state.userProfile.keyMongoAccess,
                    "X-Language": this.$store.state.userProfile.language,
                    Authorization: `Bearer ${this.$store.state.userProfile.tokenApi}`,
                };
                const chunkSize = 19 * 1024 * 1024;

                const filesNames = this.filesList.map((u) => u.name);
                const promises = this.filesList.map((fileObj, index) => {
                    const isLastFile = index === this.filesList.length - 1;

                    const file = fileObj;
                    let additionalData = {
                        name: file.name.replace(".pdf", ""),
                        description: this.form.description,
                        emailCreator: this.$store.state.userProfile.login,
                        filesNames: filesNames,
                        workflows: this.selectedWorkflows.slice(),
                    };

                    return this.readFileAsArrayBuffer(file).then((arrayBuffer) => {
                        const totalChunks = Math.ceil(arrayBuffer.byteLength / chunkSize);
                        const chunks = [];
                        for (let i = 0; i < totalChunks; i++) {
                            const start = i * chunkSize;
                            const end = Math.min(start + chunkSize, arrayBuffer.byteLength);
                            const chunk = arrayBuffer.slice(start, end);
                            chunks.push({
                                fileChunk: chunk,
                                fileType: file.type,
                                additionalData: additionalData,
                                headers: {
                                    ...apiHeaders,
                                },
                                fileName: file.name,
                                userEmail: this.$store.state.userProfile.login,
                                tokenAzure: this.$store.state.userProfile.tokenAzure,
                                url: ENV_CONFIG.VUE_APP_BASE_URL_API,
                                chunkIndex: i,
                                totalChunks: totalChunks,
                                isLastFile: isLastFile,
                                isDocumentBatch: this.validateDocumentsBatch,
                            });
                        }
                        return chunks;
                    });
                });

                Promise.all(promises)
                    .then((fileDataChunksArray) => {
                        fileDataChunksArray.forEach((chunks) => {
                            chunks.forEach((chunkData) => {
                                uploadFileWorker.send({
                                    message: chunkData,
                                });
                            });
                        });
                        localStorage.setItem("showToast", "true");
                    })
                    .finally(() => {
                        this.$router.push({
                            name: "Workflow",
                        });
                    });
            },
            backToListDocuments() {
                this.$router.push({
                    name: "Documents",
                    query: { page: "1", showToast: "true" },
                });
            },
            readFileAsArrayBuffer(file) {
                return new Promise((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onload = () => resolve(reader.result);
                    reader.onerror = (error) => reject(error);
                    reader.readAsArrayBuffer(file);
                });
            },
            createChunks(file) {
                let size = 19922944,
                    chunks = Math.ceil(file.size / size);
                for (let i = 0; i < chunks; i++) {
                    this.chunks.push(
                        file.slice(i * size, Math.min(i * size + size, file.size), file.type)
                    );
                }
            },
            confirmationDialog() {
                if (this.filesList.length > 0) {
                    this.$refs.DeleteDialog?.open();
                } else {
                    this.$notify({
                        title: "documents.upload.noFileChosen",
                        message: "documents.upload.noFileChosen",
                        variant: "warning",
                        icon: "CircleX",
                    });
                }
            },
            getWorkflows() {
                var email = this.$store.state.userProfile.login;
                WorkflowService.getWorkflowList(email)
                    .then((response) => {
                        if (response.error !== undefined) {
                            return this.$notify({
                                title: "workflows.title",
                                message: "workflows.error",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                        this.workflowsList = response.filter((t) => t.name);
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
        },
        computed: {
            filtersWorkflowList() {
                if (!this.searchTerm) {
                    return this.workflowsList;
                }
                return this.workflowsList.filter((team) =>
                    team.name.toLowerCase().includes(this.searchTerm.toLowerCase())
                );
            },
            selectedWorkflowsOrderedByName() {
                return [...this.selectedWorkflows].sort((a, b) => {
                    const nameA = this.getName(a).toLowerCase();
                    const nameB = this.getName(b).toLowerCase();
                    return nameA.localeCompare(nameB);
                });
            },
            validateDocumentsBatch() {
                if (this.isDocumentsBatch && this.filesList.length > 1) {
                    return true;
                }
                return false;
            },
        },
        mounted() {
            this.initializeDropzone();
            this.getWorkflows();
            this.validateSelection();
        },
    };
</script>
<style scoped>
    .icon-blue {
        color: #155dfc;
        width: 20px;
        height: 20px;
    }

    .float-right {
        float: right;
    }

    .char-counter {
        text-decoration: none;
        cursor: default;
    }

    .char-normal {
        color: #aeb2ba;
    }

    .char-error {
        color: #dc3545;
        float: right;
    }

    .full-height {
        height: 100%;
    }

    .content-box {
        width: 100%;
        float: left;
        text-align: center;
    }

    .team-selector-container {
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
        border: 1.5px solid var(--color-border-form-control);
        border-radius: 0.375rem;
        transition: border-color 0.3s ease;
        min-height: 150px;
    }

    .team-selector-container.is-invalid {
        border-color: #dc3545 !important;
    }

        .team-selector-container.is-valid {
            border-color: var(--color-bg-primary-badge) !important;
        }

    .selected-count {
        background-color: var(--color-bg-primary-badge) !important;
        color: var(--color-text-primary-badge) !important;
        padding: 2px 8px;
        border-radius: 12px;
        font-weight: 600;
        font-size: 0.875rem;
        user-select: none;
    }

    .scrollable-list {
        max-height: 200px;
        overflow-y: auto;
    }

    .main-scroll {
        height: 100vh;
        overflow-y: auto;
    }

    .box-upload-form {
        background-color: var(--color-card-content);
        border-radius: 12px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
        padding: 24px;
        overflow: hidden;
    }

    .btn-custom-light {
        background-color: var(--color-bg-body-content) !important;
        border-color: var(--color-border-form-control) !important;
        color: var(--color-body-content) !important;
        transition: background-color 0.2s ease;
    }

    .btn-custom-light:hover,
    .btn-custom-light:focus {
        background-color: #e2e6ea !important;
        /* tom levemente mais escuro ao hover/focus */
        color: #212529 !important;
    }

    /* Chips azul escuro com texto branco */
    .selected-team-chip {
        background-color: #155dfc !important;
        color: white !important;
    }

    .chip-remove-btn {
        border: none;
        background: transparent;
        color: inherit;
        padding: 0 0 0 4px;
        margin: 0;
        line-height: 1;
        display: inline-flex;
        align-items: center;
        cursor: pointer;
        opacity: 0.85;
    }

    .chip-remove-btn:hover,
    .chip-remove-btn:focus-visible {
        opacity: 1;
    }

    .chip-remove-btn:focus-visible {
        outline: 2px solid rgba(255, 255, 255, 0.8);
        outline-offset: 1px;
        border-radius: 2px;
    }

    .team-chip-icon {
        font-size: 1rem;
        color: white !important;
    }

    .custom-dropzone {
        background-image: url("@/assets/img/icon-dropzone.svg");
        background-repeat: no-repeat;
        background-position: center;
        color: var(--color-body-content) !important;
        border: 2px dashed #0073e6 !important;
        background-size: 35px;
    }

    .files-added {
        background-image: none !important;
    }

    .label-container {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .clear-button {
        cursor: pointer;
        color: #b1bbcb;
        margin-left: 10px;
        font-weight: bold;
    }

    .input-group-text {
        padding: 0.6rem 0.75rem !important;
    }

    h3 {
        color: black;
        margin-top: 2%;
        text-align: left;
    }

    .form-upload {
        padding-top: 20px !important;
    }

    .custom-file-button input[type="text"],
    .custom-file-button input[type="file"] {
        margin-left: -2px !important;
    }

    .custom-file-button input[type="file"]::-webkit-file-upload-button {
        display: none;
    }

    .custom-file-button input[type="file"]::file-selector-button {
        display: none;
    }

    .custom-file-button:hover label {
        cursor: pointer;
    }

    .fas {
        font-weight: 900 !important;
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }

    .div-center {
        position: relative;
        top: 50%;
        left: 50%;
        -webkit-transform: translate(-50%, -50%);
        transform: translate(-50%, -50%);
        /*width: 500px;*/
    }

    .h5-custom-modal {
        font-weight: initial;
        color: #0073e6;
        text-align: center;
    }

    .border-right {
        border-top-right-radius: 0.25rem !important;
        border-bottom-right-radius: 0.25rem !important;
    }

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

    .container-fluid {
        padding: 0 13px;
    }

    #descId {
        height: 100px;
    }

    .red-warning {
        border-color: #dc3545 !important;
    }

    @media (max-width: 767px) {
        .exceedDesc {
            display: none;
        }
    }

    .bg-select {
        background-color: var(--color-card-content) !important;
        border-color: var(--color-border-form-control) !important;
    }

</style>
