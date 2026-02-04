<template>
    <main>
        <div class="container-fluid mx-2">
            <div class="row align-items-center mb-3">
                <div class="col-auto">
                    <button
                        class="btn btn-outline-primary btn-table btn-sm"
                        @click="handleNavigateBack"
                        type="button"
                    >
                        <LucideIcon icon="ArrowLeft" />
                    </button>
                </div>
                <div class="col">
                    <div>
                        <h5 class="mb-0 fw-bold">
                            {{
                                $t(
                                    "template.configuration.title"
                                )
                            }}
                        </h5>
                        <p class="mb-0">
                            <small class="text-muted">
                                {{
                                    $t(
                                        "template.configuration.subtitle"
                                    )
                                }}
                            </small>
                        </p>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <div class="card">
                        <div class="card-body">
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
                                    v-model="
                                        selectedTemplateId
                                    "
                                    @change="
                                        onTemplateSelect
                                    "
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
                                    :template-data="
                                        templateData
                                    "
                                    :read-only="false"
                                    :editable="true"
                                    @update:queryParam="
                                        handleQueryParamUpdate
                                    "
                                    @update:header="
                                        handleHeaderUpdate
                                    "
                                    @update:body="
                                        handleBodyUpdate
                                    "
                                />
                            </div>
                        </div>

                        <div
                            class="card-footer d-flex justify-content-end"
                        >
                            <button
                                type="button"
                                :class="`btn btn-${confirmVariant}`"
                                :disabled="
                                    isLoading ||
                                    !selectedTemplate
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
                                        {{
                                            $t(
                                                "common.confirm"
                                            )
                                        }}
                                    </span>
                                </div>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import TemplateService from "@/services/template/TemplateService";
    import ToolsServices from "@/services/tools/ToolsServices";
    import TemplateFormDisplay from "@/components/templates/TemplateFormDisplay.vue";

    export default {
        name: "TemplateConfiguration",
        components: {
            TemplateFormDisplay,
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
            this.loadTemplates();
            this.loadExistingStepToolParameter();
        },
        methods: {
            resetForm() {
                this.selectedTemplateId = "";
                this.selectedTemplate = null;
                this.editableQueryParams = [];
                this.editableHeaders = [];
                this.editableBody = "";
            },
            loadExistingStepToolParameter() {
                const stepToolId =
                    this.$route.params.stepToolId;
                if (!stepToolId) return;

                this.isLoading = true;
                ToolsServices.getStepToolById(stepToolId)
                    .then((data) => {
                        if (data && data.templateToolId) {
                            this.selectedTemplateId =
                                data.templateToolId;
                            this.onTemplateSelect();

                            // TODO: Criar lógica para pegar params e preencher campos
                        }
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
                this.isLoading = true;
                const dto = {
                    stepToolId:
                        this.$route.params.stepToolId,
                    method: this.selectedTemplate.method,
                    url: this.computedUrl,
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

                ToolsServices.createTemplateStepTool(dto)
                    .then((result) => {
                        if (!result) {
                            this.$notify({
                                title: "common.error",
                                message:
                                    "template.configuration.saveError",
                                variant: "danger",
                                icon: "CircleX",
                            });
                            return;
                        }

                        this.$notify({
                            title: "common.success",
                            message:
                                "template.configuration.savedSuccessfully",
                            variant: "success",
                            icon: "CheckCircle",
                        });
                        this.handleNavigateBack();
                    })
                    .catch((e) => {
                        this.$notify({
                            title: "common.error",
                            message:
                                e.response?.data?.message ||
                                "template.configuration.saveError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            handleCancel() {
                this.handleNavigateBack();
            },
            handleNavigateBack() {
                this.$router.back();
            },
        },
    };
</script>
