<template>
    <div class="row">
        <div class="col-lg-4">
            <div class="mb-4">
                <DependencySelector
                    :previousStepTools="previousStepTools"
                    :selectedDependencies="selectedDependencies"
                    @update:selectedDependencies="updateDependencies"
                />
            </div>
            <div class="card">
                <div class="card-body">
                    <h6 class="card-title mb-3">
                        {{ $t("template.requestDetails") }}
                    </h6>
                    <div class="row mb-3">
                        <div class="col-md-3">
                            <label
                                for="method"
                                class="form-label"
                            >
                                {{ $t("template.method") }}
                            </label>
                            <select
                                :value="templateData.method"
                                class="form-select"
                                id="method"
                                :disabled="readOnly || editable"
                            >
                                <option
                                    v-for="method in methodsList"
                                    :key="method.id"
                                    :value="method.value"
                                >
                                    {{ method.value }}
                                </option>
                            </select>
                        </div>
                        <div class="col-md-9">
                            <label
                                for="endpointUrl"
                                class="form-label"
                            >
                                {{ $t("template.endpointUrl") }}
                            </label>
                            <input
                                ref="endpointUrlInput"
                                :value="templateData.url"
                                @input="updateUrl($event.target.value)"
                                type="text"
                                class="form-control"
                                id="endpointUrl"
                                maxlength="500"
                                :disabled="readOnly"
                            />
                            <GlobalVariablePicker
                                context="url"
                                :disabled="readOnly"
                                @insert="insertUrlVariable"
                            />
                        </div>
                    </div>
                    <div class="row">
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
                                <div class="d-flex justify-content-between align-items-center mb-3">
                                    <h6 class="mb-0">
                                        {{ $t("template.queryParameters") }}
                                    </h6>
                                </div>
                                <div v-if="templateData.queryParams.length > 0">
                                    <div
                                        v-for="(param, index) in templateData.queryParams"
                                        :key="index"
                                        class="row mb-2 align-items-center"
                                    >
                                        <div :class="editable ? 'col-6' : 'col-12'">
                                            <input
                                                :value="param.key"
                                                type="text"
                                                class="form-control form-control-sm"
                                                disabled
                                                :placeholder="$t('template.keyPlaceholder')"
                                            />
                                        </div>
                                        <div
                                            v-if="editable"
                                            class="col-6"
                                        >
                                            <input
                                                :ref="(element) => setFieldRef('query', index, element)"
                                                :value="param.value"
                                                @input="
                                                    updateQueryParam(index, $event.target.value)
                                                "
                                                type="text"
                                                class="form-control form-control-sm"
                                                :placeholder="$t('template.valuePlaceholder')"
                                            />
                                            <GlobalVariablePicker
                                                context="query"
                                                @insert="insertQueryVariable(index, param.value, $event)"
                                            />
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
                                <div class="d-flex justify-content-between align-items-center mb-3">
                                    <h6 class="mb-0">
                                        {{ $t("template.headers") }}
                                    </h6>
                                </div>
                                <div v-if="templateData.headers.length > 0">
                                    <div
                                        v-for="(header, index) in templateData.headers"
                                        :key="index"
                                        class="row mb-2 align-items-center"
                                    >
                                        <div :class="editable ? 'col-6' : 'col-12'">
                                            <input
                                                :value="header.key"
                                                type="text"
                                                class="form-control form-control-sm"
                                                disabled
                                                :placeholder="$t('template.keyPlaceholder')"
                                            />
                                        </div>
                                        <div
                                            v-if="editable"
                                            class="col-6"
                                        >
                                            <input
                                                :ref="(element) => setFieldRef('header', index, element)"
                                                :value="header.value"
                                                @input="updateHeader(index, $event.target.value)"
                                                type="text"
                                                class="form-control form-control-sm"
                                                :placeholder="$t('template.valuePlaceholder')"
                                            />
                                            <GlobalVariablePicker
                                                context="header"
                                                @insert="insertHeaderVariable(index, header.value, $event)"
                                            />
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
        </div>
        <div class="col-lg-8">
            <div class="card">
                <div class="card-body">
                    <div class="mb-2">
                        <h6 class="card-title">
                            {{ $t("template.requestBody") }}
                        </h6>
                    </div>
                    <Field
                        name="body"
                        rules="jsonValidation"
                        v-slot="{ field, errorMessage }"
                    >
                        <div class="position-relative">
                            <textarea
                                ref="bodyInput"
                                :name="field.name"
                                :value="templateData.body"
                                @input="
                                    handleBodyInput($event);
                                    field.onInput($event);
                                "
                                @blur="field.onBlur($event)"
                                class="form-control font-monospace"
                                rows="17"
                                :disabled="readOnly && !editable"
                                :class="{ 'is-invalid': errorMessage }"
                            ></textarea>
                            <GlobalVariablePicker
                                context="body"
                                :disabled="readOnly && !editable"
                                @insert="insertBodyVariable"
                            />
                        </div>
                        <span
                            class="validation-message text-danger"
                            v-if="errorMessage"
                        >
                            {{ errorMessage }}
                        </span>
                    </Field>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    import DependencySelector from "@/components/flow/DependencySelector.vue";
    import GlobalVariablePicker from "@/components/settings/GlobalVariablePicker.vue";
    import { Field } from "vee-validate";

    export default {
        name: "TemplateFormDisplay",
        components: {
            DependencySelector,
            GlobalVariablePicker,
            Field,
        },
        data() {
            return {
                fieldRefs: {
                    query: {},
                    header: {},
                },
            };
        },
        props: {
            templateData: {
                type: Object,
                required: true,
                default: () => ({
                    method: "GET",
                    url: "",
                    queryParams: [],
                    headers: [],
                    body: "",
                }),
            },
            readOnly: {
                type: Boolean,
                default: true,
            },
            editable: {
                type: Boolean,
                default: false,
            },
            methodsList: {
                type: Array,
                default: () => [
                    { id: 1, value: "GET" },
                    { id: 2, value: "POST" },
                    { id: 3, value: "PUT" },
                    { id: 4, value: "PATCH" },
                    { id: 5, value: "DELETE" },
                ],
            },
            selectedDependencies: {
                type: Array,
                default: () => [],
            },
            previousStepTools: {
                type: Array,
                default: () => [],
            },
        },
        methods: {
            setFieldRef(group, index, element) {
                if (element) this.fieldRefs[group][index] = element;
            },
            insertAtCursor(element, currentValue, placeholder) {
                const value = currentValue || "";
                const start = element?.selectionStart ?? value.length;
                const end = element?.selectionEnd ?? start;
                const updatedValue = `${value.slice(0, start)}${placeholder}${value.slice(end)}`;
                this.$nextTick(() => {
                    element?.focus();
                    element?.setSelectionRange(start + placeholder.length, start + placeholder.length);
                });
                return updatedValue;
            },
            insertUrlVariable(placeholder) {
                this.updateUrl(this.insertAtCursor(this.$refs.endpointUrlInput, this.templateData.url, placeholder));
            },
            insertQueryVariable(index, value, placeholder) {
                this.updateQueryParam(index, this.insertAtCursor(this.fieldRefs.query[index], value, placeholder));
            },
            insertHeaderVariable(index, value, placeholder) {
                this.updateHeader(index, this.insertAtCursor(this.fieldRefs.header[index], value, placeholder));
            },
            insertBodyVariable(placeholder) {
                this.updateBody(this.insertAtCursor(this.$refs.bodyInput, this.templateData.body, placeholder));
            },
            updateUrl(value) {
                this.$emit("update:url", value);
            },
            updateQueryParam(index, value) {
                this.$emit("update:queryParam", {
                    index,
                    value,
                });
            },
            updateHeader(index, value) {
                this.$emit("update:header", {
                    index,
                    value,
                });
            },
            handleBodyInput(event) {
                const value = event.target.value;
                this.updateBody(value);
            },
            updateBody(value) {
                this.$emit("update:body", value);
            },
            updateDependencies(dependencies) {
                this.$emit("update:dependencies", dependencies);
            },
        },
    };
</script>
<style scoped>
    .font-monospace {
        font-family: "Courier New", Courier, monospace;
        font-size: 0.875rem;
    }
</style>
