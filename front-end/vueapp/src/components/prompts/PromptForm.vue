<template>
    <div class="container-fluid scroll-area">
        <form @submit.prevent="save">
            <div class="row align-items-center mt-3" v-if="!embedded">
                <div class="col-md-8 d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center">
                        <button class="btn btn-sm btn-back p-0 me-3" @click="cancel" type="button">
                            <LucideIcon icon="ArrowLeft" :size="17" class="me-1" />
                            <span class="fw-bold">
                                {{ $t("common.back") }}
                            </span>
                        </button>
                        <div>
                            <div class="fw-semibold">
                                {{
                                    isEditMode ? $t("prompts.editPrompt") : $t("prompts.newPrompt")
                                }}
                            </div>
                            <div class="text-muted small">
                                {{
                                    isEditMode
                                        ? $t("prompts.subtitleEdit")
                                        : $t("prompts.subtitleNew")
                                }}
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row align-items-start g-3">
                <div class="col-lg-7 col-xl-8">
                    <div class="card mt-3">
                        <div class="card-body">
                            <h6 class="card-title mb-3" v-if="!embedded">
                                {{ $t("prompts.information") }}
                            </h6>
                            <h6 v-else class="mb-3">
                                {{ $t("prompts.newPrompt") }}
                            </h6>

                            <div class="mb-3">
                                <label for="inputNamePrompt" class="form-label">
                                    {{ $t("prompts.namePrompt") }}
                                </label>
                                <Field
                                    name="name"
                                    :rules="'required|max:100'"
                                    v-slot="{ field, errorMessage }"
                                >
                                    <input
                                        v-bind="field"
                                        type="text"
                                        class="form-control"
                                        :placeholder="$t('prompts.placeholderNamePrompt')"
                                        id="inputNamePrompt"
                                        aria-describedby=""
                                        name="name"
                                        :class="{
                                            'is-invalid': errorMessage,
                                        }" />
                                    <span class="validation-message text-danger" v-if="errorMessage">
                                        {{ errorMessage }}
                                    </span>
                                </Field>
                            </div>
                            <div class="mb-3">
                                <label for="FormControlTextarea1" class="form-label">
                                    {{ $t("common.description") }}
                                </label>
                                <Field name="description" :rules="'required|max:500'" v-slot="{ field, errorMessage }">
                                    <textarea v-bind="field" type="text" class="form-control" id="inputDescription"
                                        aria-describedby="descriptionCounter" rows="3" name="description"
                                        maxlength="500" :class="{
                                            'is-invalid': errorMessage,
                                        }" @input="field.onInput($event)" />
                                    <div id="descriptionCounter" class="form-text text-end">
                                        {{ (values.description || "").length }}/500
                                    </div>
                                    <span class="validation-message text-danger" v-if="errorMessage">
                                        {{ errorMessage }}
                                    </span>
                                </Field>
                            </div>

                            <div class="mb-3">
                                <label for="FormControlTextarea2" class="form-label">
                                    {{ $t("prompts.promptContent") }}
                                </label>
                                <Field name="text" rules="required" v-slot="{ field, errorMessage }">
                                    <textarea v-bind="field" type="text" class="form-control" id="FormControlTextarea2"
                                        rows="3" name="text" :class="{
                                            'is-invalid': errorMessage,
                                        }" />
                                    <span class="validation-message text-danger" v-if="errorMessage">
                                        {{ errorMessage }}
                                    </span>
                                </Field>
                                <button type="button" class="btn btn-sm btn-outline-primary mt-2" @click="refinePrompt"
                                    :disabled="isRefining">
                                    <LucideIcon icon="Wand2" :size="17" class="me-2" v-if="!isRefining" />
                                    <LucideIcon icon="LoaderCircle" :size="17" class="me-2 animate-spin" v-else />
                                    <span class="fw-bold">
                                        {{ $t("prompts.refinePrompt") }}
                                    </span>
                                </button>
                            </div>
                            <div class="mb-3 team-selector-container rounded p-3">
                                <div class="d-flex justify-content-between align-items-center mb-1">
                                    <div class="d-flex align-items-center mb-1">
                                        <LucideIcon icon="PlugZap" class="icon-blue" />
                                        <label class="form-label mb-0 ms-2">
                                            Consulta externa de IA
                                        </label>
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <Field name="enableAccessToMcp" type="checkbox" :value="true"
                                        v-slot="{ field, errorMessage }">
                                        <div class="form-check">
                                            <input v-bind="field" id="templateActive" type="checkbox"
                                                class="form-check-input" :class="{ 'is-invalid': errorMessage }" />
                                            <label class="form-check-label" for="templateActive">
                                                Habilitar consulta externa de IA
                                            </label>
                                        </div>
                                    </Field>
                                </div>
                                <div v-if="values.enableAccessToMcp">
                                    <div class="text-danger small mb-3 d-flex align-items-center gap-1">
                                        <span class="text-danger">*</span>
                                        <span>
                                            {{ $t("validation.required") }}
                                        </span>
                                    </div>
                                    <div class="mb-3 rounded">
                                        <div class="input-group">
                                            <span class="input-group-text border-end-0">
                                                <LucideIcon icon="Search" size="16" />
                                            </span>
                                            <input type="text" class="form-control form-control-sm" :placeholder="$t(
                                                'documents.workflowListModal.searchPlaceholder'
                                            )
                                                " v-model="searchTerm" />
                                        </div>
                                    </div>
                                    <div class="mb-1 d-flex gap-2 p-2 rounded">
                                        <button type="button" class="btn btn-custom-light btn-sm"
                                            @click="selectAll($event)">
                                            <LucideIcon icon="Check" class="me-1" />
                                            {{ $t("common.selectAll") }}
                                        </button>
                                        <button type="button" class="btn btn-custom-light btn-sm"
                                            @click="clearSelection($event)">
                                            <LucideIcon icon="X" class="me-1" />
                                            {{ $t("common.clearSelection") }}
                                        </button>
                                    </div>
                                    <div class="text-muted small mb-1">
                                        {{ $t("documents.upload.warningWorkflowNotListed") }}
                                    </div>
                                    <div class="border rounded bg-select p-1 user-list scrollable-list">
                                        <div v-if="isLoading" class="text-center">
                                            <div class="spinner-border text-primary" role="status">
                                                <span class="visually-hidden">
                                                    {{ $t("common.loading") }}
                                                </span>
                                            </div>
                                        </div>
                                        <div v-else-if="filtersAPiTemplateList.length === 0" class="text-center text-muted py-3">
                                            {{ $t("documents.upload.noWorkflowFound") }}
                                        </div>
                                        <div v-if="!isLoading" v-for="api in filtersAPiTemplateList" :key="api.id" class="p-1">
                                            <div class="form-check d-flex align-items-center">
                                                <input class="form-check-input me-3" type="checkbox" :id="`user-${api.id}`"
                                                    :value="api.id" v-model="apiTemplatesSelected" />
                                                <label class="form-check-label d-flex align-items-center w-100"
                                                    :for="`user-${api.id}`">                                                                                            
                                                    <div class="d-flex flex-column">
                                                        <span class="fw-semibold">
                                                            {{ api.name }}
                                                        </span>
                                                        <small class="gray-color">{{ api.url }}</small>
                                                    </div>
                                                </label>
                                            </div>
                                        </div>
                                    </div>
                                    <div v-if="apiTemplatesSelected.length > 0" class="mt-3">
                                        <label class="form-label">
                                            {{ $t("documents.upload.selectionList") }}
                                        </label>
                                        <div class="d-flex flex-wrap gap-2">
                                            <div v-for="id in apiTemplatesSelected" :key="id"
                                                class="badge rounded-pill d-flex align-items-center px-2 py-1 selected-team-chip">
                                                <LucideIcon icon="Building" class="me-1" />
                                                <span class="me-1">
                                                    {{ getName(id) }}
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="d-flex justify-content-end gap-2 mt-3">
                                <button class="btn btn-secondary" type="button" @click="cancel">
                                    {{ $t("common.cancel") }}
                                </button>
                                <button class="btn btn-primary" type="submit">
                                    <LucideIcon icon="Save" :size="17" class="me-2" />
                                    {{ $t("common.save") }}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <div
                    v-if="!embedded"
                    class="col-lg-5 col-xl-4"
                >
                    <div class="card mt-3 playground-sticky">
                        <div class="card-body">
                            <div class="d-flex align-items-center gap-2 mb-3">
                                <LucideIcon
                                    icon="Play"
                                    :size="22"
                                    class="playground-title-icon flex-shrink-0"
                                />
                                <h6 class="mb-0 fw-semibold">
                                    {{ $t("prompts.playground.title") }}
                                </h6>
                            </div>

                            <div class="mb-2 d-flex justify-content-between align-items-center">
                                <label class="form-label small mb-0 fw-semibold">
                                    {{ $t("prompts.playground.contextLabel") }}
                                </label>
                                <button
                                    type="button"
                                    class="btn btn-link btn-sm text-danger text-decoration-none p-0 d-inline-flex align-items-center"
                                    @click="clearTestContext"
                                >
                                    <LucideIcon
                                        icon="Trash2"
                                        :size="14"
                                        class="me-1"
                                    />
                                    {{ $t("common.clearSelection") }}
                                </button>
                            </div>
                            <textarea
                                v-model="testContext"
                                class="form-control mb-3 playground-textarea"
                                rows="6"
                                :placeholder="$t('prompts.playground.contextPlaceholder')"
                            />

                            <button
                                type="button"
                                class="btn btn-success w-100 d-inline-flex align-items-center justify-content-center gap-2 mb-3"
                                :disabled="isTesting || !canTestPrompt"
                                @click="testPromptInContext"
                            >
                                <LucideIcon
                                    v-if="!isTesting"
                                    icon="Play"
                                    :size="18"
                                />
                                <LucideIcon
                                    v-else
                                    icon="LoaderCircle"
                                    :size="18"
                                    class="animate-spin"
                                />
                                <span class="fw-semibold">
                                    {{ $t("prompts.playground.testButton") }}
                                </span>
                            </button>

                            <div class="mb-2 d-flex justify-content-between align-items-center">
                                <label class="form-label small mb-0 fw-semibold">
                                    {{ $t("prompts.playground.resultLabel") }}
                                </label>
                                <button
                                    type="button"
                                    class="btn btn-link btn-sm text-muted text-decoration-none p-0"
                                    @click="clearTestResult"
                                >
                                    {{ $t("common.clearSelection") }}
                                </button>
                            </div>
                            <textarea
                                v-model="testResult"
                                class="form-control playground-output"
                                rows="10"
                                readonly
                                placeholder=""
                            />
                        </div>
                    </div>
                </div>
            </div>
        </form>
    </div>
</template>
<script>
import PromptService from "@/services/prompts/PromptsService";
import TemplateService from "@/services/template/TemplateService";
import { Field, useForm } from "vee-validate";

export default {
    name: "PromptForm",
    components: {
        Field,
    },
    props: {
        id: {
            type: Number,
            required: false,
            default: null,
        },
        cloneId: {
            type: Number,
            required: false,
            default: null,
        },
        embedded: {
            type: Boolean,
            default: false,
        },
    },
    emits: ["saved", "cancelled"],
    data() {
        return {
            form: {
                name: "",
                description: "",
                text: "",
            },
            idEdit: 0,
            isRefining: false,
            searchTerm: "",
            apiTemplates: [],
            apiTemplatesSelected: [],
            isLoading: false,
        };
    },
    computed: {
        isEditMode() {
            return this.idEdit !== undefined && this.idEdit !== null && this.idEdit !== 0;
        },
        filtersAPiTemplateList() {
            if (!this.searchTerm) {
                return this.apiTemplates;
            }
            return this.apiTemplates.filter((team) =>
                team.name.toLowerCase().includes(this.searchTerm.toLowerCase())
            );
        },
        canTestPrompt() {
            const textInput = this.values?.text;
            return typeof textInput === "string" && textInput.trim().length > 0;
        },
    },
    setup() {
        const { validate, setValues, values, resetForm } = useForm();
        return {
            validate,
            setValues,
            values,
            resetForm,
        };
    },
    methods: {
        cancel() {
            this.$emit("cancelled");
        },
        async save(e) {
            const result = await this.validate();
            if (result.valid && (
                    (this.values.enableAccessToMcp && this.apiTemplatesSelected.length > 0) ||
                    !this.values.enableAccessToMcp
                )
            ) {
                if (this.isEditMode) {
                    this.updatePrompt();
                } else {
                    this.createPrompt();
                }
            }
        },
        findById(id) {
            this.resetData();
            PromptService.getPromptById(id).then((response) => {
                this.form = {
                    name: response.name,
                    description: response.description,
                    text: response.text,
                    enableAccessToMcp: response.enableAccessToMcp
                };
                this.setValues(this.form);
                this.apiTemplatesSelected = response.promptApiTemplates.map(x => x.apiTemplateId);
                this.idEdit = id;
            });
        },
        loadCloneData(id) {
            this.resetData();
            PromptService.getPromptById(id).then((response) => {
                this.form = {
                    name: response.name + " " + this.$t("prompts.cloneSuffix"),
                    description: response.description,
                    text: response.text,
                    enableAccessToMcp: response.enableAccessToMcp
                };
                this.apiTemplatesSelected = response.promptApiTemplates.map(x => x.apiTemplateId);
                this.setValues(this.form);
            });
        },
        updatePrompt: function () {
            var paramsData = {
                id: this.idEdit,
                name: this.values.name,
                description: this.values.description,
                text: this.values.text,
                enableAccessToMcp: this.values.enableAccessToMcp,
                apiTemplatesSelected: this.apiTemplatesSelected.map(x => x)
            };
            PromptService.updatePrompt(paramsData)
                .then((response) => {
                    if (!response) throw new Error("Update failed");

                    this.$notify({
                        title: "prompts.title",
                        message: "prompts.updateSuccess",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                    this.$emit("saved", response);
                })
                .catch((e) => {
                    this.$notify({
                        title: "prompts.title",
                        message: "prompts.updateError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                });
        },
        createPrompt: function () {
            var paramsData = {
                name: this.values.name,
                description: this.values.description,
                text: this.values.text,
                enableAccessToMcp: this.values.enableAccessToMcp,
                apiTemplatesSelected: this.apiTemplatesSelected.map(x => x)
            };
            PromptService.createPrompt(paramsData)
                .then((response) => {
                    if (response.error) {
                        let errorMessage = response.error.response.data.detail;
                        return this.$notify({
                            title: "prompts.title",
                            message: this.$t(errorMessage),
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }

                    if (!response) {
                        throw new Error("Create failed");
                    }

                    this.$notify({
                        title: "prompts.title",
                        message: "prompts.updateSuccess",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                    this.$emit("saved", response);
                })
                .catch((e) => {
                    this.$notify({
                        title: "prompts.title",
                        message: "prompts.createError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                });
        },
        resetData() {
            this.idEdit = 0;
            this.resetForm({
                values: {
                    name: "",
                    description: "",
                    text: "",
                    enableAccessToMcp: false
                },
            });
        },
         refinePrompt: function () {
            if (!this.values || !this.values.text || this.values.text.trim() === "") {
                return this.$notify({
                    title: "prompts.title",
                    message: "prompts.emptyPromptError",
                    variant: "warning",
                    icon: "AlertCircle",
                });
            }
            this.isRefining = true;
            PromptService.refinePrompt(this.values.text)
                .then((response) => {
                    if (!response || response.error) throw new Error("Refine failed");

                    let refinedText = response;
                    if (typeof response === "object") {
                        refinedText = Object.entries(response)
                            .map(([key, value]) => {
                                if (Array.isArray(value)) {
                                    return `${key}\n${value.map((item) => `${item}`).join("\n")}`;
                                }
                                return `${key}\n${value}`;
                            })
                            .join("\n\n");
                    }
                    this.setValues({
                        ...this.values,
                        text: refinedText,
                    });
                    this.$notify({
                        title: "prompts.title",
                        message: "prompts.refineSuccess",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                })
                .catch((error) => {
                    this.$notify({
                        title: "prompts.title",
                        message: "prompts.refineError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                })
                .finally(() => {
                    this.isRefining = false;
                });
        },
        clearTestContext() {
            this.testContext = "";
        },
        clearTestResult() {
            this.testResult = "";
        },
        testPromptInContext() {
            if (!this.canTestPrompt) {
                return;
            }
            this.isTesting = true;
            PromptService.testPrompt({
                promptText: this.values.text,
                contextText: this.testContext,
            })
                .then((response) => {
                    if (!response || response.error) throw new Error("Test failed");

                    this.testResult =
                        typeof response === "string" ? response : String(response ?? "");
                })
                .catch(() => {
                    this.$notify({
                        title: "prompts.title",
                        message: "prompts.playground.testError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                })
                .finally(() => {
                    this.isTesting = false;
                });
        },
        getTemplatesEnableAccessToMcp() {
            this.isLoading = true;

            TemplateService.getAllTemplates(true)
                .then((response) => {
                    if (response.error !== undefined) {
                        this.$notify({
                            title: "prompts.title",
                            message: this.$t(errorMessage),
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }

                    this.apiTemplates = response;
                })
                .finally(() => {
                    this.isLoading = false;
                });
        },        
        getName(id) {
            const api = this.apiTemplates.find((t) => t.id === id);
            return api ? api.name : "Desconhecido";
        },
        selectAll(event) {
            event.target.blur();
            this.apiTemplatesSelected = this.filtersAPiTemplateList.map((user) => user.id);
        },
        clearSelection(event) {
            event.target.blur();
            this.apiTemplatesSelected = [];
        },
    },
    mounted() {
        if (this.id) {
            this.findById(this.id);
        } else if (this.cloneId) {
            this.loadCloneData(this.cloneId);
        }
        this.getTemplatesEnableAccessToMcp();
    },
    watch: {
        id(newId) {
            if (newId) {
                this.findById(newId);
            } else {
                this.resetData();
            }
        },
        cloneId(newId) {
            if (newId) {
                this.loadCloneData(newId);
            }
        },
    },
};
</script>
<style scoped>
.card {
    border-radius: 10px;
}

.animate-spin {
    animation: spin 1s linear infinite;
}

@keyframes spin {
    from {
        transform: rotate(0deg);
    }

    to {
        transform: rotate(360deg);
    }
}

.btn-back {
    color: var(--color-body-content) !important
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
    /* min-height: 150px; */
}

.team-selector-container.is-invalid {
    border-color: #dc3545 !important;
}

.team-selector-container.is-valid {
    border-color: var(--color-bg-primary-badge) !important;
}


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

    .btn-back {
        color: var(--color-body-content) !important;
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

    .gray-color {
        color: #777;
    }

    .playground-sticky {
        position: sticky;
        top: 1rem;
        align-self: flex-start;
    }

    .playground-title-icon {
        color: var(--bs-success, #198754);
    }

    .playground-textarea {
        border-radius: 10px;
    }

    .playground-output {
        border-radius: 10px;
        background-color: var(--bs-light, #f8f9fa);
        resize: vertical;
        min-height: 12rem;
    }
</style>
