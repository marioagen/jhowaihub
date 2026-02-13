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
                            {{ $t("template.configuration.title") }}
                        </h5>
                        <p class="mb-0">
                            <small class="text-muted">
                                {{ $t("template.configuration.subtitle") }}
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
                                    {{ $t("template.selectTemplate") }}
                                </label>
                                <select
                                    id="templateSelect"
                                    class="form-select"
                                    v-model="selectedTemplateId"
                                    @change="onTemplateSelect"
                                    :disabled="isLoading"
                                >
                                    <option value="">
                                        {{ $t("template.selectTemplatePlaceholder") }}
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
                                    @update:url="handleUrlUpdate"
                                    @update:queryParam="handleQueryParamUpdate"
                                    @update:header="handleHeaderUpdate"
                                    @update:body="handleBodyUpdate"
                                />
                            </div>
                        </div>

                        <div class="card-footer d-flex justify-content-end">
                            <button
                                type="button"
                                class="btn btn-primary"
                                :disabled="isLoading || !selectedTemplate"
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
        </div>
    </main>
</template>

<script>
    import TemplateService from "@/services/template/TemplateService";
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
                editableUrl: "",
            };
        },
        computed: {
            computedUrl() {
                if (!this.selectedTemplate || !this.selectedTemplate.url) {
                    return "";
                }

                const baseUrl = this.editableUrl
                    ? this.editableUrl.split("?")[0]
                    : this.selectedTemplate.url.split("?")[0];

                const validParams = this.editableQueryParams.filter(
                    (p) => p.value && p.value.trim() !== ""
                );

                if (validParams.length === 0) {
                    return baseUrl;
                }

                const queryString = validParams.map((p) => `${p.key}=${p.value}`).join("&");

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
                    method: this.selectedTemplate.method || "GET",
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
                this.editableUrl = "";
            },
            loadExistingStepToolParameter() {
                const flowStateJson = localStorage.getItem("flow_state_params");

                if (!flowStateJson) {
                    return;
                }

                try {
                    const flowState = JSON.parse(flowStateJson);
                    const selectedNode = flowState.selectedNode;

                    if (
                        !selectedNode ||
                        !selectedNode.data.parameters ||
                        selectedNode.data.parameters.length === 0
                    ) {
                        return;
                    }

                    const parameter = selectedNode.data.parameters[0];
                    if (!parameter.value) {
                        return;
                    }

                    this.isLoading = true;

                    const savedConfig = JSON.parse(parameter.value);

                    if (savedConfig.url) {
                        this.editableUrl = savedConfig.url;
                    }

                    if (savedConfig.query) {
                        this.editableQueryParams = Object.entries(savedConfig.query).map(
                            ([key, value]) => ({
                                key,
                                value,
                            })
                        );
                    }

                    if (savedConfig.headers) {
                        this.editableHeaders = Object.entries(savedConfig.headers).map(
                            ([key, value]) => ({
                                key,
                                value,
                            })
                        );
                    }

                    if (savedConfig.body) {
                        this.editableBody =
                            typeof savedConfig.body === "string"
                                ? savedConfig.body
                                : JSON.stringify(savedConfig.body, null, 2);
                    }

                    if (savedConfig.templateId) {
                        const checkTemplates = setInterval(() => {
                            if (this.templates.length > 0) {
                                clearInterval(checkTemplates);

                                const matchingTemplate = this.templates.find(
                                    (t) => t.id === savedConfig.templateId
                                );

                                if (matchingTemplate) {
                                    this.selectedTemplateId = matchingTemplate.id;
                                    this.selectedTemplate = matchingTemplate;
                                }
                                this.isLoading = false;
                            }
                        }, 100);

                        setTimeout(() => {
                            clearInterval(checkTemplates);
                            this.isLoading = false;
                        }, 5000);
                    } else {
                        this.isLoading = false;
                    }
                } catch {
                    this.isLoading = false;
                }
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
                    this.resetForm();
                    return;
                }

                this.isLoading = true;
                TemplateService.getTemplateById(this.selectedTemplateId)
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
                this.editableQueryParams = this.parseQueryParams(
                    this.selectedTemplate.queryTemplate
                );

                this.editableHeaders = this.parseHeaders(this.selectedTemplate.headerTemplate);

                this.editableBody = this.selectedTemplate.bodyTemplate || "";

                this.editableUrl = "";
            },
            handleUrlUpdate(value) {
                this.editableUrl = value;
            },
            handleQueryParamUpdate({ index, value }) {
                if (this.editableQueryParams[index]) {
                    this.editableQueryParams[index].value = value;
                }
            },
            handleHeaderUpdate({ index, value }) {
                if (this.editableHeaders[index]) {
                    this.editableHeaders[index].value = value;
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
            formatTemplateJson(json, templateId) {
                const formatted = {
                    templateId: templateId,
                    method: json.method,
                    url: json.url,
                };

                if (json.queryTemplate) {
                    try {
                        const queryArray =
                            typeof json.queryTemplate === "string"
                                ? JSON.parse(json.queryTemplate)
                                : json.queryTemplate;

                        const queryObj = {};
                        queryArray.forEach((param) => {
                            if (param.key && param.value !== undefined) {
                                queryObj[param.key] = param.value;
                            }
                        });

                        if (Object.keys(queryObj).length > 0) {
                            formatted.query = queryObj;
                        }
                    } catch {
                        this.$notify({
                            title: "common.error",
                            message: "template.loadError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                }

                if (json.headerTemplate) {
                    try {
                        const headerArray =
                            typeof json.headerTemplate === "string"
                                ? JSON.parse(json.headerTemplate)
                                : json.headerTemplate;

                        const headerObj = {};
                        headerArray.forEach((header) => {
                            if (header.key && header.value !== undefined) {
                                headerObj[header.key] = header.value;
                            }
                        });

                        if (Object.keys(headerObj).length > 0) {
                            formatted.headers = headerObj;
                        }
                    } catch (e) {
                        console.error("Error parsing headerTemplate:", e);
                    }
                }

                if (json.bodyTemplate) {
                    try {
                        formatted.body =
                            typeof json.bodyTemplate === "string"
                                ? JSON.parse(json.bodyTemplate)
                                : json.bodyTemplate;
                    } catch {
                        formatted.body = json.bodyTemplate;
                    }
                }

                return JSON.stringify(formatted);
            },
            handleConfirm() {
                this.isLoading = true;
                const json = {
                    method: this.selectedTemplate.method,
                    url: this.computedUrl,
                    bodyTemplate: this.editableBody,
                    queryTemplate:
                        this.editableQueryParams.length > 0
                            ? JSON.stringify(this.editableQueryParams)
                            : null,
                    headerTemplate:
                        this.editableHeaders.length > 0
                            ? JSON.stringify(this.editableHeaders)
                            : null,
                };

                const formattedJson = this.formatTemplateJson(json, this.selectedTemplateId);

                const newParam = {
                    stepToolId: this.$route.params.stepToolId,
                    value: formattedJson,
                };

                const flowStateJson = localStorage.getItem("flow_state_params");

                if (!flowStateJson) {
                    this.$notify({
                        title: "common.error",
                        message: "template.configuration.saveError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                    this.isLoading = false;
                    this.handleNavigateBack();
                    return;
                }

                const flowState = JSON.parse(flowStateJson);

                let node = flowState.nodes.find((n) => n.id === flowState.selectedNode.id);

                if (!node) {
                    this.$notify({
                        title: "common.error",
                        message: "template.configuration.saveError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                    this.isLoading = false;
                    this.handleNavigateBack();
                    return;
                }

                node.data.parameters = [newParam];
                flowState.selectedNode = undefined;

                localStorage.setItem("flow_state_params", JSON.stringify(flowState));

                this.isLoading = false;
                this.handleNavigateBack();
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
