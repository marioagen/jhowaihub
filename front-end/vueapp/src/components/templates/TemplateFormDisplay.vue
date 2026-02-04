<template>
    <div class="row">
        <div class="col-lg-6">
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
                                :disabled="
                                    readOnly || editable
                                "
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
                                {{
                                    $t(
                                        "template.endpointUrl"
                                    )
                                }}
                            </label>
                            <input
                                :value="templateData.url"
                                type="text"
                                class="form-control"
                                id="endpointUrl"
                                :disabled="readOnly"
                            />
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
                                :id="`query-params-tab-${_uid}`"
                                data-bs-toggle="tab"
                                :data-bs-target="`#query-params-${_uid}`"
                                type="button"
                                role="tab"
                            >
                                {{
                                    $t(
                                        "template.queryParams"
                                    )
                                }}
                            </button>
                        </li>
                        <li
                            class="nav-item"
                            role="presentation"
                        >
                            <button
                                class="nav-link"
                                :id="`headers-tab-${_uid}`"
                                data-bs-toggle="tab"
                                :data-bs-target="`#headers-${_uid}`"
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
                            :id="`query-params-${_uid}`"
                            role="tabpanel"
                        >
                            <div
                                class="d-flex justify-content-between align-items-center mb-3"
                            >
                                <h6 class="mb-0">
                                    {{
                                        $t(
                                            "template.queryParameters"
                                        )
                                    }}
                                </h6>
                            </div>
                            <div
                                v-if="
                                    templateData.queryParams
                                        .length > 0
                                "
                            >
                                <div
                                    v-for="(
                                        param, index
                                    ) in templateData.queryParams"
                                    :key="index"
                                    class="row mb-2 align-items-center"
                                >
                                    <div
                                        :class="
                                            editable
                                                ? 'col-6'
                                                : 'col-12'
                                        "
                                    >
                                        <input
                                            :value="
                                                param.key
                                            "
                                            type="text"
                                            class="form-control form-control-sm"
                                            disabled
                                            :placeholder="
                                                $t(
                                                    'template.keyPlaceholder'
                                                )
                                            "
                                        />
                                    </div>
                                    <div
                                        v-if="editable"
                                        class="col-6"
                                    >
                                        <input
                                            :value="
                                                param.value
                                            "
                                            @input="
                                                updateQueryParam(
                                                    index,
                                                    $event
                                                        .target
                                                        .value
                                                )
                                            "
                                            type="text"
                                            class="form-control form-control-sm"
                                            :placeholder="
                                                $t(
                                                    'template.valuePlaceholder'
                                                )
                                            "
                                        />
                                    </div>
                                </div>
                            </div>
                            <div
                                v-else
                                class="text-center text-muted py-4"
                            >
                                <small>
                                    {{
                                        $t(
                                            "template.noQueryParameters"
                                        )
                                    }}
                                </small>
                            </div>
                        </div>
                        <div
                            class="tab-pane fade"
                            :id="`headers-${_uid}`"
                            role="tabpanel"
                        >
                            <div
                                class="d-flex justify-content-between align-items-center mb-3"
                            >
                                <h6 class="mb-0">
                                    {{
                                        $t(
                                            "template.headers"
                                        )
                                    }}
                                </h6>
                            </div>
                            <div
                                v-if="
                                    templateData.headers
                                        .length > 0
                                "
                            >
                                <div
                                    v-for="(
                                        header, index
                                    ) in templateData.headers"
                                    :key="index"
                                    class="row mb-2 align-items-center"
                                >
                                    <div
                                        :class="
                                            editable
                                                ? 'col-6'
                                                : 'col-12'
                                        "
                                    >
                                        <input
                                            :value="
                                                header.key
                                            "
                                            type="text"
                                            class="form-control form-control-sm"
                                            disabled
                                            :placeholder="
                                                $t(
                                                    'template.keyPlaceholder'
                                                )
                                            "
                                        />
                                    </div>
                                    <div
                                        v-if="editable"
                                        class="col-6"
                                    >
                                        <input
                                            :value="
                                                header.value
                                            "
                                            @input="
                                                updateHeader(
                                                    index,
                                                    $event
                                                        .target
                                                        .value
                                                )
                                            "
                                            type="text"
                                            class="form-control form-control-sm"
                                            :placeholder="
                                                $t(
                                                    'template.valuePlaceholder'
                                                )
                                            "
                                        />
                                    </div>
                                </div>
                            </div>
                            <div
                                v-else
                                class="text-center text-muted py-4"
                            >
                                <small>
                                    {{
                                        $t(
                                            "template.noQueryParameters"
                                        )
                                    }}
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
                    <div class="mb-2">
                        <h6 class="card-title">
                            {{ $t("template.requestBody") }}
                        </h6>
                        <small class="text-muted">
                            {{
                                $t("template.bodySubtitle")
                            }}
                        </small>
                    </div>
                    <div class="position-relative">
                        <textarea
                            :value="templateData.body"
                            @input="
                                updateBody(
                                    $event.target.value
                                )
                            "
                            class="form-control font-monospace"
                            rows="15"
                            :disabled="
                                readOnly && !editable
                            "
                        ></textarea>
                    </div>
                    <div
                        class="alert alert-info mt-3 py-2 px-3 d-flex align-items-start"
                    >
                        <LucideIcon
                            icon="Lightbulb"
                            :size="16"
                            class="me-2 flex-shrink-0"
                        />
                        <small>
                            {{
                                $t("template.variablesTip")
                            }}
                        </small>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "TemplateFormDisplay",
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
        },
        methods: {
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
            updateBody(value) {
                this.$emit("update:body", value);
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
