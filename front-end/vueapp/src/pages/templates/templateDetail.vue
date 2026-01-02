<template>
    <main>
        <div class="container-fluid mx-2">
            <form @submit.prevent="save">
                <div class="d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center mb-1">
                        <button type="button" class="btn btn-sm p-0 me-3" @click="redirectToTemplateList">
                            <LucideIcon icon="ArrowLeft" :size="17" class="me-1" />
                        </button>
                        <div>
                            <h5 class="mb-0 fw-bold">
                                {{ isEditMode ? $t("template.formEdit.title") : $t("template.formCreate.title") }}
                            </h5>
                            <p class="mb-1">
                                {{ isEditMode ? $t("template.formEdit.subtitle") : $t("template.formCreate.subtitle") }}
                            </p>
                        </div>
                    </div>
                    <div class="d-flex gap-2">
                        <button type="button" class="btn btn-sm btn-outline-secondary" @click="redirectToTemplateList">
                            {{ $t("template.cancelBtn") }}
                        </button>
                        <button type="submit" class="btn btn-sm btn-primary" :disabled="isSaving">
                            <span
                                v-if="isSaving"
                                class="spinner-border spinner-border-sm me-2"
                                role="status"
                                aria-hidden="true"
                            ></span>
                            {{ $t("template.createBtn") }}
                        </button>
                    </div>
                </div>

                <div class="row">
                    <div class="col-lg-6">
                        <div class="card">
                            <div class="card-body">
                                <h6 class="card-title mb-3">{{ $t("template.requestDetails") }}</h6>

                                <div class="mb-3">
                                    <label for="templateName" class="form-label">
                                        {{ $t("template.templateName") }}
                                    </label>
                                    <Field name="name" rules="required|max:100" v-slot="{ field, errorMessage }">
                                        <input
                                            v-bind="field"
                                            type="text"
                                            class="form-control"
                                            :placeholder="$t('template.templateNamePlaceholder')"
                                            id="templateName"
                                            :class="{ 'is-invalid': errorMessage }"
                                        />
                                        <span class="validation-message text-danger" v-if="errorMessage">
                                            {{ errorMessage }}
                                        </span>
                                    </Field>
                                </div>

                                <div class="row mb-3">
                                    <div class="col-md-3">
                                        <label for="method" class="form-label">{{ $t("template.method") }}</label>
                                        <Field name="method" rules="required" v-slot="{ field, errorMessage }">
                                            <select
                                                v-bind="field"
                                                class="form-select"
                                                id="method"
                                                :class="{ 'is-invalid': errorMessage }"
                                            >
                                                <option value="GET">GET</option>
                                                <option value="POST">POST</option>
                                                <option value="PUT">PUT</option>
                                                <option value="PATCH">PATCH</option>
                                                <option value="DELETE">DELETE</option>
                                            </select>
                                            <span class="validation-message text-danger" v-if="errorMessage">
                                                {{ errorMessage }}
                                            </span>
                                        </Field>
                                    </div>
                                    <div class="col-md-9">
                                        <label for="endpointUrl" class="form-label">
                                            {{ $t("template.endpointUrl") }}
                                        </label>
                                        <Field name="url" rules="required|max:500" v-slot="{ field, errorMessage }">
                                            <input
                                                v-bind="field"
                                                type="text"
                                                class="form-control"
                                                :placeholder="$t('template.endpointUrlPlaceholder')"
                                                id="endpointUrl"
                                                :class="{ 'is-invalid': errorMessage }"
                                            />
                                            <span class="validation-message text-danger" v-if="errorMessage">
                                                {{ errorMessage }}
                                            </span>
                                        </Field>
                                    </div>
                                </div>

                                <ul class="nav nav-tabs mb-3" role="tablist">
                                    <li class="nav-item" role="presentation">
                                        <button
                                            class="nav-link active"
                                            id="query-params-tab"
                                            data-bs-toggle="tab"
                                            data-bs-target="#query-params"
                                            type="button"
                                            role="tab"
                                        >
                                            {{ $t("template.queryParams") }}
                                        </button>
                                    </li>
                                    <li class="nav-item" role="presentation">
                                        <button
                                            class="nav-link"
                                            id="headers-tab"
                                            data-bs-toggle="tab"
                                            data-bs-target="#headers"
                                            type="button"
                                            role="tab"
                                        >
                                            {{ $t("template.headers") }}
                                        </button>
                                    </li>
                                </ul>

                                <div class="tab-content">
                                    <div class="tab-pane fade show active" id="query-params" role="tabpanel">
                                        <div class="d-flex justify-content-between align-items-center mb-3">
                                            <h6 class="mb-0">{{ $t("template.queryParameters") }}</h6>
                                            <button type="button" class="btn btn-sm btn-link" @click="addQueryParam">
                                                <LucideIcon icon="Plus" :size="15" />
                                                {{ $t("template.addParam") }}
                                            </button>
                                        </div>

                                        <div v-if="form.queryParams.length > 0">
                                            <div
                                                v-for="(param, index) in form.queryParams"
                                                :key="index"
                                                class="row mb-2 align-items-center"
                                            >
                                                <div class="col-10">
                                                    <input
                                                        v-model="param.key"
                                                        type="text"
                                                        class="form-control form-control-sm"
                                                        :placeholder="$t('template.keyPlaceholder')"
                                                    />
                                                </div>
                                                <div class="col-2">
                                                    <button
                                                        type="button"
                                                        class="btn btn-sm btn-link text-danger"
                                                        @click="removeQueryParam(index)"
                                                    >
                                                        <LucideIcon icon="Trash2" :size="15" />
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                        <div v-else class="text-center text-muted py-4">
                                            <small>{{ $t("template.noQueryParameters") }}</small>
                                        </div>
                                    </div>

                                    <div class="tab-pane fade" id="headers" role="tabpanel">
                                        <div class="d-flex justify-content-between align-items-center mb-3">
                                            <h6 class="mb-0">{{ $t("template.headers") }}</h6>
                                            <button type="button" class="btn btn-sm btn-link" @click="addHeader">
                                                <LucideIcon icon="Plus" :size="15" />
                                                {{ $t("template.addParam") }}
                                            </button>
                                        </div>

                                        <div v-if="form.headers.length > 0">
                                            <div
                                                v-for="(header, index) in form.headers"
                                                :key="index"
                                                class="row mb-2 align-items-center"
                                            >
                                                <div class="col-10">
                                                    <input
                                                        v-model="header.key"
                                                        type="text"
                                                        class="form-control form-control-sm"
                                                        :placeholder="$t('template.keyPlaceholder')"
                                                    />
                                                </div>
                                                <div class="col-2">
                                                    <button
                                                        type="button"
                                                        class="btn btn-sm btn-link text-danger"
                                                        @click="removeHeader(index)"
                                                    >
                                                        <LucideIcon icon="Trash2" :size="15" />
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                        <div v-else class="text-center text-muted py-4">
                                            <small>{{ $t("template.noQueryParameters") }}</small>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-lg-6">
                        <div class="card">
                            <div class="card-body">
                                <div class="mb-2">
                                    <h6 class="card-title">{{ $t("template.requestBody") }}</h6>
                                    <small class="text-muted">{{ $t("template.bodySupportsVariables") }}</small>
                                </div>

                                <Field name="body" v-slot="{ field, errorMessage }">
                                    <textarea
                                        v-bind="field"
                                        class="form-control font-monospace"
                                        rows="15"
                                        :placeholder="bodyPlaceholder"
                                        :class="{ 'is-invalid': errorMessage }"
                                    ></textarea>
                                    <span class="validation-message text-danger" v-if="errorMessage">
                                        {{ errorMessage }}
                                    </span>
                                </Field>

                                <div class="alert alert-info mt-3 py-2 px-3 d-flex align-items-start">
                                    <LucideIcon icon="Lightbulb" :size="16" class="me-2 mt-1 flex-shrink-0" />
                                    <small>{{ $t("template.variablesTip") }}</small>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    </main>
</template>

<script>
    import { Field, useForm } from "vee-validate";
    import TemplateService from "@/services/template/TemplateService";
    import { notify } from "@/utils/notification";

    export default {
        name: "TemplateDetail",
        components: {
            Field,
        },
        data() {
            return {
                form: {
                    name: "",
                    method: "GET",
                    url: "",
                    queryParams: [],
                    headers: [],
                    body: "",
                },
                isSaving: false,
                isLoading: false,
                bodyPlaceholder: '{\n  "key": "{{variable}}"\n}',
            };
        },
        computed: {
            routeId() {
                return this.$route.params.id;
            },
            isEditMode() {
                return this.routeId !== undefined;
            },
        },
        setup() {
            const { validate, setValues, values, resetForm } = useForm();
            return { validate, setValues, values, resetForm };
        },
        mounted() {
            if (this.isEditMode) {
                this.loadTemplate();
            } else {
                this.setValues({
                    name: "",
                    method: "GET",
                    url: "",
                    body: "",
                });
            }
        },
        watch: {
            "form.queryParams": {
                handler() {
                    this.updateUrlWithQueryParams();
                },
                deep: true,
            },
        },
        methods: {
            redirectToTemplateList() {
                this.$router.push({ name: "Template" });
            },

            addQueryParam() {
                this.form.queryParams.push({ key: "" });
            },

            removeQueryParam(index) {
                this.form.queryParams.splice(index, 1);
            },

            addHeader() {
                this.form.headers.push({ key: "" });
            },

            removeHeader(index) {
                this.form.headers.splice(index, 1);
            },

            updateUrlWithQueryParams() {
                const validQueryParams = this.form.queryParams.filter((p) => p.key.trim() !== "");

                if (!this.values.url) {
                    return;
                }

                const baseUrl = this.values.url.split("?")[0];

                if (validQueryParams.length > 0) {
                    const queryString = validQueryParams
                        .map((p) => `${encodeURIComponent(p.key)}={{${p.key}}}`)
                        .join("&");
                    this.setValues({
                        ...this.values,
                        url: `${baseUrl}?${queryString}`,
                    });
                } else {
                    this.setValues({
                        ...this.values,
                        url: baseUrl,
                    });
                }
            },

            loadTemplate() {
                this.isLoading = true;
                TemplateService.getTemplateById(this.routeId)
                    .then((data) => {
                        this.form.name = data.name || "";
                        this.form.method = data.method || "GET";
                        this.form.url = data.url || "";
                        this.form.body = data.bodyTemplate || "";

                        try {
                            const parsedQueryParams = data.queryTemplate
                                ? typeof data.queryTemplate === "string"
                                    ? JSON.parse(data.queryTemplate)
                                    : data.queryTemplate
                                : [];
                            this.form.queryParams = parsedQueryParams.map((p) => ({ key: p.key }));
                        } catch (e) {
                            console.error("Error parsing queryParams:", e);
                            this.form.queryParams = [];
                        }

                        try {
                            const parsedHeaders = data.headerTemplate
                                ? typeof data.headerTemplate === "string"
                                    ? JSON.parse(data.headerTemplate)
                                    : data.headerTemplate
                                : [];
                            this.form.headers = parsedHeaders.map((h) => ({ key: h.key }));
                        } catch (e) {
                            console.error("Error parsing headers:", e);
                            this.form.headers = [];
                        }

                        this.setValues({
                            name: this.form.name,
                            method: this.form.method,
                            url: this.form.url,
                            body: this.form.body,
                        });
                    })
                    .catch((error) => {
                        console.error("Error loading template:", error);
                        notify({
                            title: this.$t("common.error"),
                            message: this.$t("template.editError"),
                            variant: "danger",
                        });
                        this.redirectToTemplateList();
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },

            save() {
                this.validate().then((result) => {
                    if (!result.valid) {
                        return;
                    }

                    this.isSaving = true;

                    const queryParams = this.form.queryParams
                        .filter((p) => p.key.trim() !== "")
                        .map((p) => ({ key: p.key, value: `{{${p.key}}}` }));

                    const headers = this.form.headers
                        .filter((h) => h.key.trim() !== "")
                        .map((h) => ({ key: h.key, value: `{{${h.key}}}` }));

                    const templateData = {
                        name: this.values.name,
                        method: this.values.method,
                        url: this.values.url,
                        bodyTemplate: this.values.body == "" ? null : this.values.body,
                        queryTemplate: queryParams.length === 0 ? null : JSON.stringify(queryParams),
                        headerTemplate: headers.length === 0 ? null : JSON.stringify(headers),
                    };

                    if (this.isEditMode) {
                        templateData.id = this.routeId;
                    }

                    const savePromise = this.isEditMode
                        ? TemplateService.updateTemplate(templateData)
                        : TemplateService.createTemplate(templateData);

                    savePromise
                        .then(() => {
                            const successMsg = this.isEditMode
                                ? this.$t("template.editSuccess")
                                : this.$t("template.createSuccess");
                            notify({
                                title: this.$t("common.success"),
                                message: successMsg,
                                variant: "success",
                            });
                            this.redirectToTemplateList();
                        })
                        .catch((error) => {
                            console.error("Error saving template:", error);
                            const errorMsg = this.isEditMode
                                ? this.$t("template.editError")
                                : this.$t("template.createError");
                            notify({
                                title: this.$t("common.error"),
                                message: errorMsg,
                                variant: "danger",
                            });
                        })
                        .finally(() => {
                            this.isSaving = false;
                        });
                });
            },
        },
    };
</script>

<style scoped>
    .method-badge {
        padding: 0.25rem 0.5rem;
        border-radius: 0.25rem;
        font-size: 0.75rem;
        font-weight: 600;
        text-transform: uppercase;
    }

    .method-get {
        background-color: #e3f2fd;
        color: #1976d2;
    }

    .method-post {
        background-color: #e8f5e9;
        color: #388e3c;
    }

    .method-put {
        background-color: #fff3e0;
        color: #f57c00;
    }

    .method-patch {
        background-color: #fce4ec;
        color: #c2185b;
    }

    .method-delete {
        background-color: #ffebee;
        color: #d32f2f;
    }

    .font-monospace {
        font-family: "Courier New", Courier, monospace;
        font-size: 0.875rem;
    }
</style>
