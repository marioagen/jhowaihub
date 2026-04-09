<template>
    <div class="container-fluid scroll-area">
        <form @submit.prevent="save">
            <div
                class="row align-items-center mt-3"
                v-if="!embedded"
            >
                <div class="col-md-8 d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center">
                        <button
                            class="btn btn-sm btn-back p-0 me-3"
                            @click="cancel"
                            type="button"
                        >
                            <LucideIcon
                                icon="ArrowLeft"
                                :size="17"
                                class="me-1"
                            />
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
                            <h6
                                class="card-title mb-3"
                                v-if="!embedded"
                            >
                                {{ $t("prompts.information") }}
                            </h6>
                            <h6
                                v-else
                                class="mb-3"
                            >
                                {{ $t("prompts.newPrompt") }}
                            </h6>

                            <div class="mb-3">
                                <label
                                    for="inputNamePrompt"
                                    class="form-label"
                                >
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
                                        }"
                                    />
                                    <span
                                        class="validation-message text-danger"
                                        v-if="errorMessage"
                                    >
                                        {{ errorMessage }}
                                    </span>
                                </Field>
                            </div>
                            <div class="mb-3">
                                <label
                                    for="FormControlTextarea1"
                                    class="form-label"
                                >
                                    {{ $t("common.description") }}
                                </label>
                                <Field
                                    name="description"
                                    :rules="'required|max:500'"
                                    v-slot="{ field, errorMessage }"
                                >
                                    <textarea
                                        v-bind="field"
                                        type="text"
                                        class="form-control"
                                        id="inputDescription"
                                        aria-describedby="descriptionCounter"
                                        rows="3"
                                        name="description"
                                        maxlength="500"
                                        :class="{
                                            'is-invalid': errorMessage,
                                        }"
                                        @input="field.onInput($event)"
                                    />
                                    <div
                                        id="descriptionCounter"
                                        class="form-text text-end"
                                    >
                                        {{ (values.description || "").length }}/500
                                    </div>
                                    <span
                                        class="validation-message text-danger"
                                        v-if="errorMessage"
                                    >
                                        {{ errorMessage }}
                                    </span>
                                </Field>
                            </div>

                            <div class="mb-3">
                                <label
                                    for="FormControlTextarea2"
                                    class="form-label"
                                >
                                    {{ $t("prompts.promptContent") }}
                                </label>
                                <Field
                                    name="text"
                                    rules="required"
                                    v-slot="{ field, errorMessage }"
                                >
                                    <textarea
                                        v-bind="field"
                                        type="text"
                                        class="form-control"
                                        id="FormControlTextarea2"
                                        rows="3"
                                        name="text"
                                        :class="{
                                            'is-invalid': errorMessage,
                                        }"
                                    />
                                    <span
                                        class="validation-message text-danger"
                                        v-if="errorMessage"
                                    >
                                        {{ errorMessage }}
                                    </span>
                                </Field>
                                <button
                                    type="button"
                                    class="btn btn-sm btn-outline-primary mt-2"
                                    @click="refinePrompt"
                                    :disabled="isRefining"
                                >
                                    <LucideIcon
                                        icon="Wand2"
                                        :size="17"
                                        class="me-2"
                                        v-if="!isRefining"
                                    />
                                    <LucideIcon
                                        icon="LoaderCircle"
                                        :size="17"
                                        class="me-2 animate-spin"
                                        v-else
                                    />
                                    <span class="fw-bold">
                                        {{ $t("prompts.refinePrompt") }}
                                    </span>
                                </button>
                            </div>

                            <div class="d-flex justify-content-end gap-2 mt-3">
                                <button
                                    class="btn btn-secondary"
                                    type="button"
                                    @click="cancel"
                                >
                                    {{ $t("common.cancel") }}
                                </button>
                                <button
                                    class="btn btn-primary"
                                    type="submit"
                                >
                                    <LucideIcon
                                        icon="Save"
                                        :size="17"
                                        class="me-2"
                                    />
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
                                    {{ $t("prompts.playgroundTitle") }}
                                </h6>
                            </div>

                            <div class="mb-2 d-flex justify-content-between align-items-center">
                                <label class="form-label small mb-0 fw-semibold">
                                    {{ $t("prompts.playgroundContextLabel") }}
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
                                :placeholder="$t('prompts.playgroundContextPlaceholder')"
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
                                    {{ $t("prompts.playgroundTestButton") }}
                                </span>
                            </button>

                            <div class="mb-2 d-flex justify-content-between align-items-center">
                                <label class="form-label small mb-0 fw-semibold">
                                    {{ $t("prompts.playgroundResultLabel") }}
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
                testContext: "",
                testResult: "",
                isTesting: false,
            };
        },
        computed: {
            isEditMode() {
                return this.idEdit !== undefined && this.idEdit !== null && this.idEdit !== 0;
            },
            canTestPrompt() {
                const t = this.values?.text;
                return typeof t === "string" && t.trim().length > 0;
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
                if (result.valid) {
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
                    };
                    this.setValues(this.form);
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
                    };
                    this.setValues(this.form);
                });
            },
            updatePrompt: function () {
                var paramsData = {
                    id: this.idEdit,
                    name: this.values.name,
                    description: this.values.description,
                    text: this.values.text,
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
                    },
                });
            },
            clearTestContext() {
                this.testContext = "";
            },
            clearTestResult() {
                this.testResult = "";
            },
            testPromptInContext() {
                if (!this.values || !this.values.text || this.values.text.trim() === "") {
                    return this.$notify({
                        title: "prompts.title",
                        message: "prompts.emptyPromptError",
                        variant: "warning",
                        icon: "AlertCircle",
                    });
                }
                this.isTesting = true;
                PromptService.testPrompt({
                    promptText: this.values.text,
                    contextText: this.testContext,
                })
                    .then((response) => {
                        if (response && response.error) {
                            throw new Error("Test failed");
                        }
                        this.testResult =
                            typeof response === "string" ? response : String(response ?? "");
                    })
                    .catch(() => {
                        this.$notify({
                            title: "prompts.title",
                            message: "prompts.playgroundTestError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isTesting = false;
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
        },
        mounted() {
            if (this.id) {
                this.findById(this.id);
            } else if (this.cloneId) {
                this.loadCloneData(this.cloneId);
            }
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
        color: var(--color-body-content) !important;
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
