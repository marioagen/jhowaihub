<template>
    <div
        class="modal fade"
        tabindex="-1"
        aria-hidden="true"
        ref="TemplateDialog"
        :aria-labelledby="`${id}-label`"
        :id="id"
    >
        <div
            class="modal-dialog modal-dialog-centered modal-xl modal-dialog-scrollable"
        >
            <div class="modal-content">
                <div class="modal-header">
                    <button
                        class="btn btn-sm btn-outline-primary table-btn"
                        disabled
                    >
                        <LucideIcon icon="Zap" />
                    </button>
                    <h5
                        class="modal-title ms-2"
                        :id="`${id}-label`"
                    >
                        {{ $t("template.modal.title") }}
                    </h5>
                    <button
                        type="button"
                        class="btn-close"
                        data-bs-dismiss="modal"
                        aria-label="Close"
                        :disabled="isLoading"
                    />
                </div>

                <div class="modal-body">
                    <div class="mb-4">
                        <label
                            for="templateSelect"
                            class="form-label fw-bold"
                        >
                            {{
                                $t(
                                    "template.selectTemplate"
                                )
                            }}
                        </label>
                        <select
                            id="templateSelect"
                            class="form-select"
                            v-model="selectedTemplateId"
                            @change="onTemplateSelect"
                            :disabled="isLoading"
                        >
                            <option value="">
                                {{
                                    $t(
                                        "template.selectTemplatePlaceholder"
                                    )
                                }}
                            </option>
                            <option
                                v-for="template in templates"
                                :key="template.id"
                                :value="template.id"
                            >
                                {{ template.name }}
                            </option>
                        </select>
                    </div>

                    <div v-if="selectedTemplate">
                        <TemplateFormDisplay
                            :template-data="templateData"
                            :read-only="false"
                            :editable="true"
                            @update:queryParam="
                                handleQueryParamUpdate
                            "
                            @update:header="
                                handleHeaderUpdate
                            "
                            @update:body="handleBodyUpdate"
                        />
                    </div>
                </div>

                <div
                    class="d-flex justify-content-end mx-4 my-4"
                >
                    <button
                        type="button"
                        class="btn btn-outline-primary btn-table table-btn mx-4"
                        data-bs-dismiss="modal"
                        :disabled="isLoading"
                        @click="handleCancel"
                    >
                        {{ $t("common.cancel") }}
                    </button>
                    <button
                        type="button"
                        class="mx-4"
                        :class="`btn btn-${confirmVariant}`"
                        :disabled="
                            isLoading || !selectedTemplate
                        "
                        @click="handleConfirm"
                    >
                        <div
                            style="min-width: 80px"
                            class="text-center"
                        >
                            <span
                                v-if="isLoading"
                                class="spinner-grow spinner-grow-sm"
                                role="status"
                            ></span>
                            <span v-else>
                                {{ $t("common.confirm") }}
                            </span>
                        </div>
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import TemplateService from "@/services/template/TemplateService";
    import TemplateFormDisplay from "./TemplateFormDisplay.vue";

    export default {
        name: "TemplateModal",
        components: {
            TemplateFormDisplay,
        },
        props: {
            id: {
                type: String,
                required: true,
            },
        },
        data() {
            return {
                isLoading: false,
                templates: [],
                selectedTemplateId: "",
                selectedTemplate: null,
                editableQueryParams: [],
                editableHeaders: [],
                editableBody: "",
            };
        },
        computed: {
            confirmVariant() {
                return "primary";
            },
            computedUrl() {
                if (
                    !this.selectedTemplate ||
                    !this.selectedTemplate.url
                ) {
                    return "";
                }

                const baseUrl =
                    this.selectedTemplate.url.split("?")[0];

                const validParams =
                    this.editableQueryParams.filter(
                        (p) =>
                            p.value && p.value.trim() !== ""
                    );

                if (validParams.length === 0) {
                    return baseUrl;
                }

                const queryString = validParams
                    .map((p) => `${p.key}=${p.value}`)
                    .join("&");

                return `${baseUrl}?${queryString}`;
            },
            templateData() {
                if (!this.selectedTemplate) {
                    return {
                        name: "",
                        method: "GET",
                        url: "",
                        queryParams: [],
                        headers: [],
                        body: "",
                    };
                }

                return {
                    name: this.selectedTemplate.name || "",
                    method:
                        this.selectedTemplate.method ||
                        "GET",
                    url: this.computedUrl,
                    queryParams: this.editableQueryParams,
                    headers: this.editableHeaders,
                    body: this.editableBody,
                };
            },
        },
        mounted() {
            this.modalInstance = new window.bootstrap.Modal(
                this.$refs.TemplateDialog,
                {
                    backdrop: "static",
                    keyboard: false,
                }
            );
            this.loadTemplates();
        },
        methods: {
            open() {
                this.modalInstance?.show();
            },
            close() {
                this.modalInstance?.hide();
                this.resetForm();
            },
            resetForm() {
                this.selectedTemplateId = "";
                this.selectedTemplate = null;
                this.editableQueryParams = [];
                this.editableHeaders = [];
                this.editableBody = "";
            },
            loadTemplates() {
                this.isLoading = true;
                TemplateService.getAllTemplates()
                    .then((data) => {
                        this.templates = data;
                    })
                    .catch(() => {
                        this.$notify({
                            title: "common.error",
                            message: "template.loadError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            onTemplateSelect() {
                if (!this.selectedTemplateId) {
                    this.selectedTemplate = null;
                    this.editableQueryParams = [];
                    this.editableHeaders = [];
                    this.editableBody = "";
                    return;
                }

                this.isLoading = true;
                TemplateService.getTemplateById(
                    this.selectedTemplateId
                )
                    .then((data) => {
                        this.selectedTemplate = data;
                        this.initializeEditableData();
                    })
                    .catch(() => {
                        this.$notify({
                            title: "common.error",
                            message: "template.loadError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            initializeEditableData() {
                this.editableQueryParams =
                    this.parseQueryParams(
                        this.selectedTemplate.queryTemplate
                    );

                this.editableHeaders = this.parseHeaders(
                    this.selectedTemplate.headerTemplate
                );

                this.editableBody =
                    this.selectedTemplate.bodyTemplate ||
                    "";
            },
            handleQueryParamUpdate({ index, value }) {
                if (this.editableQueryParams[index]) {
                    this.editableQueryParams[index].value =
                        value;
                }
            },
            handleHeaderUpdate({ index, value }) {
                if (this.editableHeaders[index]) {
                    this.editableHeaders[index].value =
                        value;
                }
            },
            handleBodyUpdate(value) {
                this.editableBody = value;
            },
            parseQueryParams(queryTemplate) {
                if (!queryTemplate) return [];
                try {
                    const parsed =
                        typeof queryTemplate === "string"
                            ? JSON.parse(queryTemplate)
                            : queryTemplate;
                    return parsed.map((p) => ({
                        key: p.key,
                        value: p.value || "",
                    }));
                } catch (e) {
                    return [];
                }
            },
            parseHeaders(headerTemplate) {
                if (!headerTemplate) return [];
                try {
                    const parsed =
                        typeof headerTemplate === "string"
                            ? JSON.parse(headerTemplate)
                            : headerTemplate;
                    return parsed.map((h) => ({
                        key: h.key,
                        value: h.value || "",
                    }));
                } catch (e) {
                    return [];
                }
            },
            handleConfirm() {
                const filledTemplate = {
                    ...this.selectedTemplate,
                    bodyTemplate: this.editableBody,
                    queryTemplate:
                        this.editableQueryParams.length > 0
                            ? JSON.stringify(
                                  this.editableQueryParams
                              )
                            : null,
                    headerTemplate:
                        this.editableHeaders.length > 0
                            ? JSON.stringify(
                                  this.editableHeaders
                              )
                            : null,
                };

                this.$emit("confirm", filledTemplate);
                this.close();
            },
            handleCancel() {
                this.$emit("cancel");
                this.close();
            },
        },
    };
</script>
