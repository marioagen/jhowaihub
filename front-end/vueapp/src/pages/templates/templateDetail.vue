<template>
    <main>
        <div class="container-fluid mx-2">
            <form @submit.prevent="save">
                <div class="d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center mb-1">
                        <button
                            type="button"
                            class="btn btn-sm p-0 me-3"
                            @click="redirectToTemplateList"
                        >
                            <LucideIcon
                                icon="ArrowLeft"
                                :size="17"
                                class="me-1"
                            />
                        </button>
                        <div>
                            <h5 class="mb-0 fw-bold">
                                {{ getTemplateTitle }}
                            </h5>
                            <p class="mb-1">
                                {{ getTemplateSubtitle }}
                            </p>
                        </div>
                    </div>
                    <div class="d-flex gap-2">
                        <button
                            type="button"
                            class="btn btn-sm btn-outline-secondary"
                            @click="redirectToTemplateList"
                        >
                            {{ $t("template.cancelBtn") }}
                        </button>
                        <button
                            type="submit"
                            class="btn btn-sm btn-primary"
                            :disabled="isSaving"
                        >
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
                                <h6 class="card-title mb-3">
                                    {{ $t("template.requestDetails") }}
                                </h6>
                                <div class="mb-3">
                                    <label
                                        for="templateName"
                                        class="form-label"
                                    >
                                        {{ $t("template.templateName") }}
                                    </label>
                                    <Field
                                        name="name"
                                        rules="required|max:100"
                                        v-slot="{ field, errorMessage }"
                                    >
                                        <input
                                            v-bind="field"
                                            type="text"
                                            class="form-control"
                                            :placeholder="$t('template.templateNamePlaceholder')"
                                            id="templateName"
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
                                <div class="row mb-3">
                                    <div class="col-md-3">
                                        <label
                                            for="method"
                                            class="form-label"
                                        >
                                            {{ $t("template.method") }}
                                        </label>
                                        <Field
                                            name="method"
                                            rules="required"
                                            v-slot="{ field, errorMessage }"
                                        >
                                            <select
                                                v-bind="field"
                                                class="form-select"
                                                id="method"
                                                :class="{
                                                    'is-invalid': errorMessage,
                                                }"
                                            >
                                                <option
                                                    v-for="method in methodsList"
                                                    :key="method.id"
                                                    :value="method.value"
                                                >
                                                    {{ method.value }}
                                                </option>
                                            </select>
                                            <span
                                                class="validation-message text-danger"
                                                v-if="errorMessage"
                                            >
                                                {{ errorMessage }}
                                            </span>
                                        </Field>
                                    </div>
                                    <div class="col-md-9">
                                        <label
                                            for="endpointUrl"
                                            class="form-label"
                                        >
                                            {{ $t("template.endpointUrl") }}
                                        </label>
                                        <Field
                                            name="url"
                                            rules="required|max:500"
                                            v-slot="{ field, errorMessage }"
                                        >
                                            <input
                                                v-bind="field"
                                                type="text"
                                                class="form-control"
                                                :placeholder="$t('template.endpointUrlPlaceholder')"
                                                id="endpointUrl"
                                                maxlength="500"
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
                                </div>
                                <ul
                                    class="nav nav-tabs mb-3"
                                    role="tablist"
                                >
                                    <li
                                        class="nav-item"
                                        role="presentation"
                                    >
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
                                    <li
                                        class="nav-item"
                                        role="presentation"
                                    >
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
                                    <div
                                        class="tab-pane fade show active"
                                        id="query-params"
                                        role="tabpanel"
                                    >
                                        <div
                                            class="d-flex justify-content-between align-items-center mb-3"
                                        >
                                            <h6 class="mb-0">
                                                {{ $t("template.queryParameters") }}
                                            </h6>
                                            <button
                                                type="button"
                                                class="btn btn-sm btn-link"
                                                @click="addQueryParam"
                                            >
                                                <LucideIcon
                                                    icon="Plus"
                                                    :size="15"
                                                />
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
                                                        <LucideIcon
                                                            icon="Trash2"
                                                            :size="15"
                                                        />
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                        <div
                                            v-else
                                            class="text-center text-muted py-4"
                                        >
                                            <small>
                                                {{ $t("template.noQueryParameters") }}
                                            </small>
                                        </div>
                                    </div>
                                    <div
                                        class="tab-pane fade"
                                        id="headers"
                                        role="tabpanel"
                                    >
                                        <div
                                            class="d-flex justify-content-between align-items-center mb-3"
                                        >
                                            <h6 class="mb-0">
                                                {{ $t("template.headers") }}
                                            </h6>
                                            <button
                                                type="button"
                                                class="btn btn-sm btn-link"
                                                @click="addHeader"
                                            >
                                                <LucideIcon
                                                    icon="Plus"
                                                    :size="15"
                                                />
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
                                                        <LucideIcon
                                                            icon="Trash2"
                                                            :size="15"
                                                        />
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                        <div
                                            v-else
                                            class="text-center text-muted py-4"
                                        >
                                            <small>
                                                {{ $t("template.noQueryParameters") }}
                                            </small>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-6">
                        <div class="card">
                            <div class="card-body">
                                <TabsComponent
                                    :tabs="bodyAndTestsTabs"
                                    compact
                                >
                                    <template #body>
                                        <div class="mb-2">
                                            <h6 class="card-title">
                                                {{ $t("template.requestBody") }}
                                            </h6>
                                            <small class="text-muted">
                                                {{ $tBracketsToBraces("template.bodySubtitle") }}
                                            </small>
                                        </div>
                                        <Field
                                            name="body"
                                            rules="jsonValidation"
                                            v-slot="{ field, errorMessage }"
                                        >
                                            <div class="position-relative">
                                                <textarea
                                                    v-bind="field"
                                                    ref="bodyTextarea"
                                                    class="form-control font-monospace"
                                                    rows="15"
                                                    :placeholder="bodyPlaceholder"
                                                    :class="{
                                                        'is-invalid': errorMessage,
                                                    }"
                                                    @input="handleBodyInput"
                                                    @keydown="handleKeyDown"
                                                    @blur="hideAutocomplete"
                                                ></textarea>
                                                <div
                                                    v-if="showAutocomplete"
                                                    class="autocomplete-dropdown"
                                                    :style="{
                                                        top: autocompletePosition.top + 'px',
                                                        left: autocompletePosition.left + 'px',
                                                    }"
                                                >
                                                    <div
                                                        v-for="(
                                                            option, index
                                                        ) in filteredAutocompleteOptions"
                                                        :key="index"
                                                        class="autocomplete-item"
                                                        :class="{
                                                            active:
                                                                index === selectedAutocompleteIndex,
                                                        }"
                                                        @mousedown.prevent="
                                                            selectAutocompleteOption(option)
                                                        "
                                                    >
                                                        <strong>
                                                            {{ option.label }}
                                                        </strong>
                                                        <span class="text-muted ms-2">
                                                            {{ option.value }}
                                                        </span>
                                                    </div>
                                                </div>
                                            </div>
                                            <span
                                                class="validation-message text-danger"
                                                v-if="errorMessage"
                                            >
                                                {{ errorMessage }}
                                            </span>
                                        </Field>
                                        <div
                                            class="alert alert-info mt-3 py-2 px-3 d-flex align-items-start"
                                        >
                                            <LucideIcon
                                                icon="Lightbulb"
                                                :size="16"
                                                class="me-2 flex-shrink-0"
                                            />
                                            <small>
                                                {{ $tBracketsToBraces("template.variablesTip") }}
                                            </small>
                                        </div>
                                    </template>
                                    <template #tests>
                                        <div class="template-tests">
                                            <h6
                                                class="template-tests-heading d-flex align-items-center gap-2 mb-2"
                                            >
                                                <LucideIcon
                                                    icon="Globe"
                                                    :size="18"
                                                    class="text-primary flex-shrink-0"
                                                />
                                                <span class="card-title mb-0">
                                                    {{ $t("template.tests.detectedVariables") }}
                                                </span>
                                            </h6>
                                            <div
                                                class="template-tests-variables-box mb-3 d-flex align-items-center justify-content-center"
                                            >
                                                <template
                                                    v-if="detectedTemplateVariables.length === 0"
                                                >
                                                    <p
                                                        class="template-tests-empty text-muted text-center mb-0 small"
                                                    >
                                                        <span>
                                                            {{
                                                                $t("template.tests.noVariablesLead")
                                                            }}
                                                        </span>
                                                        <code
                                                            v-pre
                                                            class="template-tests-brace-badge"
                                                        >
                                                            {{}}
                                                        </code>
                                                        <span>
                                                            {{
                                                                $t("template.tests.noVariablesTail")
                                                            }}
                                                        </span>
                                                    </p>
                                                </template>
                                                <div
                                                    v-else
                                                    class="d-flex flex-wrap gap-2 justify-content-center w-100"
                                                >
                                                    <span
                                                        v-for="v in detectedTemplateVariables"
                                                        :key="v"
                                                        class="badge bg-light text-dark border font-monospace"
                                                    >
                                                        {{ v }}
                                                    </span>
                                                </div>
                                            </div>
                                            <div
                                                v-if="detectedTemplateVariables.length > 0"
                                                class="template-tests-variable-inputs mb-3"
                                            >
                                                <h6 class="small fw-semibold text-secondary mb-2">
                                                    {{ $t("template.tests.variableValues") }}
                                                </h6>
                                                <div
                                                    v-for="v in detectedTemplateVariables"
                                                    :key="'var-input-' + v"
                                                    class="mb-2"
                                                >
                                                    <label
                                                        class="form-label small font-monospace mb-1"
                                                        :for="'test-var-' + v"
                                                    >
                                                        {{ v }}
                                                    </label>
                                                    <input
                                                        :id="'test-var-' + v"
                                                        v-model="variableValues[v]"
                                                        type="text"
                                                        class="form-control form-control-sm"
                                                        autocomplete="off"
                                                    />
                                                </div>
                                            </div>
                                            <button
                                                type="button"
                                                class="btn template-tests-simulate-btn w-100 d-inline-flex align-items-center justify-content-center mb-4"
                                                :disabled="testsSimulateLoading"
                                                @click="simulateRequest"
                                            >
                                                <span
                                                    v-if="testsSimulateLoading"
                                                    class="spinner-border spinner-border-sm me-2"
                                                    role="status"
                                                    aria-hidden="true"
                                                ></span>
                                                <LucideIcon
                                                    v-else
                                                    icon="Play"
                                                    :size="18"
                                                    class="me-2 flex-shrink-0"
                                                />
                                                {{ $t("template.tests.simulateRequest") }}
                                            </button>
                                            <div
                                                v-if="testsSimulateLoading"
                                                class="template-tests-result-placeholder text-center py-4"
                                            >
                                                <span
                                                    class="spinner-border text-primary"
                                                    role="status"
                                                    aria-hidden="true"
                                                ></span>
                                            </div>
                                            <div
                                                v-else-if="testsResult"
                                                class="template-tests-result text-start"
                                            >
                                                <div
                                                    class="mb-2 d-flex align-items-center flex-wrap gap-2"
                                                >
                                                    <span
                                                        class="badge method-badge"
                                                        :class="testsResultStatusBadgeClass"
                                                    >
                                                        {{ testsResult.statusCode }}
                                                    </span>
                                                    <span class="small text-muted">
                                                        {{
                                                            $t("template.tests.responseHttpStatus")
                                                        }}
                                                    </span>
                                                </div>
                                                <pre
                                                    class="template-tests-response-body small mb-0 p-3 rounded border bg-light text-dark"
                                                    >{{ testsResult.content }}</pre
                                                >
                                            </div>
                                            <div
                                                v-else
                                                class="template-tests-result-placeholder text-center py-2"
                                            >
                                                <div
                                                    class="template-tests-result-icon-wrap mx-auto mb-3 d-flex align-items-center justify-content-center"
                                                >
                                                    <LucideIcon
                                                        icon="Play"
                                                        :size="32"
                                                        class="text-secondary"
                                                    />
                                                </div>
                                                <p class="fw-medium text-secondary mb-1">
                                                    {{ $t("template.tests.noRunTitle") }}
                                                </p>
                                                <p class="small text-muted mb-0">
                                                    {{ $t("template.tests.noRunHint") }}
                                                </p>
                                            </div>
                                        </div>
                                    </template>
                                </TabsComponent>
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
    import ToolsService from "@/services/tools/ToolsServices";
    import textHelper from "@/helpers/textHelper";
    import TabsComponent from "@/components/global/TabsComponent.vue";

    export default {
        name: "TemplateDetail",
        components: {
            Field,
            TabsComponent,
        },
        props: {
            methodsList: {
                type: Array,
                required: false,
                default: () => [
                    { id: 1, value: "GET" },
                    { id: 2, value: "POST" },
                    { id: 3, value: "PUT" },
                    { id: 4, value: "PATCH" },
                    { id: 5, value: "DELETE" },
                ],
            },
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
                testsSimulateLoading: false,
                testsResult: null,
                variableValues: {},
                bodyPlaceholder: '{\n  "key": "variable"\n}',
                showAutocomplete: false,
                autocompletePosition: { top: 0, left: 0 },
                selectedAutocompleteIndex: 0,
                autocompleteOptions: [],
                bodyAndTestsTabs: [
                    { name: "body", label: "template.tabBody" },
                    { name: "tests", label: "template.tabTests" },
                ],
            };
        },
        computed: {
            routeId() {
                return this.$route.params.id;
            },
            isEditMode() {
                return this.routeId !== undefined;
            },
            filteredAutocompleteOptions() {
                return this.autocompleteOptions
                    .filter((w) => w.active)
                    .map((opt) => ({
                        ...opt,
                        label: opt.labelKey ? this.$t(opt.labelKey) : opt.label,
                    }));
            },
            getTemplateTitle() {
                return this.isEditMode
                    ? this.$t("template.formEdit.title")
                    : this.$t("template.formCreate.title");
            },
            getTemplateSubtitle() {
                return this.isEditMode
                    ? this.$t("template.formEdit.subtitle")
                    : this.$t("template.formCreate.subtitle");
            },
            detectedTemplateVariables() {
                const body = (this.values && this.values.body) || "";
                const url = (this.values && this.values.url) || "";
                const combined = `${body}\n${url}`;
                const re = /\{\{([^{}]+)\}\}/g;
                const seen = new Set();
                let m;
                while ((m = re.exec(combined)) !== null) {
                    const name = m[1].trim();
                    if (name) {
                        seen.add(name);
                    }
                }
                return Array.from(seen).sort((a, b) => a.localeCompare(b));
            },
            testsResultStatusBadgeClass() {
                if (!this.testsResult) return "";
                const c = this.testsResult.statusCode;
                if (c >= 200 && c < 300) return "method-get";
                if (c >= 400 && c < 500) return "method-patch";
                if (c >= 500) return "method-delete";
                return "method-post";
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
        mounted() {
            this.loadToolsForAutocomplete();
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
            detectedTemplateVariables: {
                handler(vars) {
                    const next = { ...this.variableValues };
                    for (const v of vars) {
                        if (!(v in next)) {
                            next[v] = "";
                        }
                    }
                    this.variableValues = next;
                },
                immediate: true,
            },
        },
        methods: {
            toolTypeDisplayName(apiName) {
                if (!apiName) return "";
                const key = "tools.typeDisplay." + apiName;
                const translated = this.$t(key);
                return translated !== key ? translated : apiName;
            },
            loadToolsForAutocomplete() {
                ToolsService.getToolsList()
                    .then((tools) => {
                        if (tools && Array.isArray(tools)) {
                            this.autocompleteOptions = tools.map((tool) => {
                                const translatedType = this.toolTypeDisplayName(tool.toolType);
                                const sanitizedType = textHelper.sanitizeToolNameForVariable(
                                    tool.toolType
                                );
                                return {
                                    label: `${translatedType} - ${tool.name}`,
                                    value: `{{${sanitizedType}}}`,
                                    active: true,
                                };
                            });
                        }
                    })
                    .catch((e) => {
                        console.error("Failed to load tools for autocomplete", e);
                    });
            },
            handleBodyInput(event) {
                const textarea = event.target;
                const value = textarea.value;
                const cursorPosition = textarea.selectionStart;

                if (value[cursorPosition - 1] === "{") {
                    if (!this.isMainJsonOpeningBrace(value, cursorPosition - 1)) {
                        this.showAutocompleteDropdown(textarea);
                    }
                } else {
                    this.hideAutocomplete();
                }
            },
            isMainJsonOpeningBrace(value, position) {
                const beforeCursor = value.substring(0, position).trim();

                if (beforeCursor === "") {
                    return true;
                }

                let depth = 0;
                for (let i = 0; i < position; i++) {
                    if (value[i] === "{") depth++;
                    if (value[i] === "}") depth--;
                }

                return depth === 0;
            },
            showAutocompleteDropdown(textarea) {
                const coords = this.getCaretCoordinates(textarea);
                this.autocompletePosition = {
                    top: coords.top + 20,
                    left: coords.left,
                };
                this.showAutocomplete = true;
                this.selectedAutocompleteIndex = 0;
            },
            hideAutocomplete() {
                setTimeout(() => {
                    this.showAutocomplete = false;
                }, 200);
            },
            handleKeyDown(event) {
                if (!this.showAutocomplete) return;

                if (event.key === "ArrowDown") {
                    event.preventDefault();
                    this.selectedAutocompleteIndex =
                        (this.selectedAutocompleteIndex + 1) %
                        this.filteredAutocompleteOptions.length;
                } else if (event.key === "ArrowUp") {
                    event.preventDefault();
                    this.selectedAutocompleteIndex =
                        (this.selectedAutocompleteIndex -
                            1 +
                            this.filteredAutocompleteOptions.length) %
                        this.filteredAutocompleteOptions.length;
                } else if (event.key === "Enter" || event.key === "Tab") {
                    event.preventDefault();
                    this.selectAutocompleteOption(
                        this.filteredAutocompleteOptions[this.selectedAutocompleteIndex]
                    );
                } else if (event.key === "Escape") {
                    this.showAutocomplete = false;
                }
            },
            selectAutocompleteOption(option) {
                const textarea = this.$refs.bodyTextarea;
                const cursorPosition = textarea.selectionStart;
                const value = textarea.value;

                const beforeCursor = value.substring(0, cursorPosition - 1);
                const afterCursor = value.substring(cursorPosition);

                const newValue = beforeCursor + option.value + afterCursor;
                const newCursorPosition = beforeCursor.length + option.value.length;

                this.setValues({
                    ...this.values,
                    body: newValue,
                });

                this.form.body = newValue;

                this.$nextTick(() => {
                    textarea.selectionStart = newCursorPosition;
                    textarea.selectionEnd = newCursorPosition;
                    textarea.focus();
                });

                this.showAutocomplete = false;
            },
            getCaretCoordinates(textarea) {
                const rect = textarea.getBoundingClientRect();
                const style = window.getComputedStyle(textarea);

                const mirror = document.createElement("div");
                const styles = [
                    "fontFamily",
                    "fontSize",
                    "fontWeight",
                    "letterSpacing",
                    "lineHeight",
                    "padding",
                    "border",
                    "boxSizing",
                ];

                styles.forEach((prop) => {
                    mirror.style[prop] = style[prop];
                });

                mirror.style.position = "absolute";
                mirror.style.visibility = "hidden";
                mirror.style.whiteSpace = "pre-wrap";
                mirror.style.width = rect.width + "px";

                const textBeforeCursor = textarea.value.substring(0, textarea.selectionStart);
                mirror.textContent = textBeforeCursor;

                document.body.appendChild(mirror);

                const coordinates = {
                    top: mirror.scrollHeight - textarea.scrollTop,
                    left: 10,
                };

                document.body.removeChild(mirror);

                return coordinates;
            },
            redirectToTemplateList() {
                this.$router.push({ name: "Templates" });
            },
            buildApiTemplateDraftPayload() {
                const queryParams = this.form.queryParams
                    .filter((p) => p.key.trim() !== "")
                    .map((p) => ({
                        key: p.key,
                        value: `{{${p.key}}}`,
                    }));
                const headers = this.form.headers
                    .filter((h) => h.key.trim() !== "")
                    .map((h) => ({
                        key: h.key,
                        value: `{{${h.key}}}`,
                    }));
                return {
                    name: this.values.name,
                    method: this.values.method,
                    url: this.values.url,
                    queryTemplate: queryParams.length === 0 ? null : JSON.stringify(queryParams),
                    headerTemplate: headers.length === 0 ? null : JSON.stringify(headers),
                    bodyTemplate:
                        this.values.body === "" || this.values.body == null
                            ? null
                            : this.values.body,
                };
            },
            async simulateRequest() {
                const validation = await this.validate();
                if (!validation.valid) {
                    return;
                }

                const variables = {};
                for (const v of this.detectedTemplateVariables) {
                    variables[v] =
                        this.variableValues[v] != null && this.variableValues[v] !== ""
                            ? String(this.variableValues[v])
                            : "";
                }

                const payload = {
                    templateId: this.isEditMode ? Number(this.routeId) : null,
                    draft: this.buildApiTemplateDraftPayload(),
                    variables,
                    templateName: this.values.name || null,
                    tenant: this.$store.state.userProfile?.tenant ?? null,
                    email: this.$store.state.userProfile?.login ?? null,
                    executionId: null,
                };

                this.testsSimulateLoading = true;
                this.testsResult = null;

                try {
                    this.testsResult = await TemplateService.executeRequestTest(payload);
                    const statusCode = this.testsResult?.statusCode;
                    if (typeof statusCode === "number" && statusCode >= 200 && statusCode < 300) {
                        this.$notify({
                            title: "common.success",
                            message: "template.tests.simulateSuccess",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    }
                } catch (error) {
                    this.$notify({
                        title: "common.error",
                        message: "template.tests.simulateError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.testsSimulateLoading = false;
                }
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
                            this.form.queryParams = [];
                        }

                        try {
                            const parsedHeaders = data.headerTemplate
                                ? typeof data.headerTemplate === "string"
                                    ? JSON.parse(data.headerTemplate)
                                    : data.headerTemplate
                                : [];
                            this.form.headers = parsedHeaders.map((h) => ({
                                key: h.key,
                            }));
                        } catch (e) {
                            this.form.headers = [];
                        }

                        this.setValues({
                            name: this.form.name,
                            method: this.form.method,
                            url: this.form.url,
                            body: this.form.body,
                        });
                    })
                    .catch(() => {
                        this.$notify({
                            title: "common.error",
                            message: "template.editError",
                            variant: "danger",
                            icon: "CircleX",
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
                        .map((p) => ({
                            key: p.key,
                            value: `{{${p.key}}}`,
                        }));

                    const headers = this.form.headers
                        .filter((h) => h.key.trim() !== "")
                        .map((h) => ({
                            key: h.key,
                            value: `{{${h.key}}}`,
                        }));

                    const templateData = {
                        name: this.values.name,
                        method: this.values.method,
                        url: this.values.url,
                        bodyTemplate: this.values.body == "" ? null : this.values.body,
                        queryTemplate:
                            queryParams.length === 0 ? null : JSON.stringify(queryParams),
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
                            this.$notify({
                                title: "common.success",
                                message: successMsg,
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                            this.redirectToTemplateList();
                        })
                        .catch((error) => {
                            const errorMsg = this.isEditMode
                                ? this.$t("template.editError")
                                : this.$t("template.createError");
                            this.$notify({
                                title: "common.error",
                                message: errorMsg,
                                variant: "danger",
                                icon: "CircleX",
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

    .autocomplete-dropdown {
        position: absolute;
        background: white;
        border: 1px solid #dee2e6;
        border-radius: 0.25rem;
        box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
        z-index: 1000;
        max-height: 300px;
        overflow-y: auto;
        min-width: 250px;
    }

    .autocomplete-item {
        padding: 0.5rem 1rem;
        cursor: pointer;
        transition: background-color 0.15s ease-in-out;
    }

    .autocomplete-item:hover,
    .autocomplete-item.active {
        background-color: #f8f9fa;
    }

    .autocomplete-item strong {
        display: block;
        font-size: 0.875rem;
    }

    .autocomplete-item .text-muted {
        font-size: 0.75rem;
        font-family: "Courier New", Courier, monospace;
    }

    .template-tests-heading .card-title {
        font-size: 1rem;
    }

    .template-tests-variables-box {
        min-height: 5.5rem;
        padding: 1.25rem;
        border: 2px dashed var(--color-border-form-control, #d3d3d3);
        border-radius: 0.5rem;
        background-color: var(--color-bg-form-control, #ffffff);
    }

    .template-tests-brace-badge {
        display: inline-block;
        margin: 0 0.2rem;
        padding: 0.1rem 0.35rem;
        font-size: 0.75rem;
        font-family: "Courier New", Courier, monospace;
        color: var(--color-body-content, #212529);
        background-color: var(--color-bg-body-content, #f5f5f5);
        border: 1px solid var(--color-border-form-control, #d3d3d3);
        border-radius: 0.25rem;
        vertical-align: middle;
    }

    .template-tests-simulate-btn {
        color: var(--color-btn-primary, #ffffff) !important;
        background-color: var(--color-bg-btn-primary, #0d6efd) !important;
        border: none;
        border-radius: 0.5rem;
        padding: 0.55rem 1rem;
        font-weight: 500;
    }

    .template-tests-simulate-btn:hover {
        filter: brightness(0.95);
        color: var(--color-btn-primary, #ffffff) !important;
    }

    .template-tests-response-body {
        max-height: min(50vh, 22rem);
        overflow: auto;
        white-space: pre;
    }

    .template-tests-result-icon-wrap {
        width: 5rem;
        height: 5rem;
        border-radius: 50%;
        background-color: var(--color-bg-body-content, #e9ecef);
    }
</style>
