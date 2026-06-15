<template>
    <ModalComponent
        id="toolModal"
        :isLoading="isLoading"
        @save="save"
        ref="ToolModal"
    >
        <template #header>
            <div class="modal-header">
                <h5 class="modal-title">
                    {{ $t(titleText) }}
                </h5>
                <button
                    class="btn-close"
                    data-bs-dismiss="modal"
                    @click="close"
                />
            </div>
        </template>
        <template #body>
            <div class="modal-body">
                <div class="row mb-3">
                    <div class="col">
                        <label>
                            {{ $t("connectors.form.name") }}
                        </label>
                        <Field
                            name="name"
                            rules="required"
                            v-slot="{ field, errorMessage }"
                        >
                            <input
                                v-bind="field"
                                class="form-control form-control-sm"
                                :class="{
                                    'is-invalid': errorMessage,
                                }"
                            />
                            <span
                                v-if="errorMessage"
                                class="validation-message text-danger"
                            >
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                </div>
                <div class="row mb-3">
                    <div class="col">
                        <label>
                            {{ $t("connectors.form.types") }}
                        </label>
                        <Field
                            name="toolTypeId"
                            rules="required"
                            v-slot="{ field, errorMessage }"
                        >
                            <select
                                v-bind="field"
                                class="form-select form-select-sm"
                                :class="{
                                    'is-invalid': errorMessage,
                                }"
                                @change="changeToolType"
                            >
                                <option value="">
                                    {{ $t("connectors.form.typesSelect") }}
                                </option>
                                <option
                                    v-for="(item, index) in typesList"
                                    :key="index"
                                    :value="item.id"
                                >
                                    {{ toolTypeLabel(item.description) }}
                                </option>
                            </select>
                            <span
                                v-if="errorMessage"
                                class="validation-message text-danger"
                            >
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                </div>
                <div
                    v-if="isN8NConnectorToolType"
                    class="row mb-3"
                >
                    <div class="col-6">
                        <label>
                            {{ $t("connectors.form.connectorUrl") }}
                        </label>
                        <Field
                            name="connectorUrl"
                            :rules="isN8NConnectorToolType ? 'required' : ''"
                            v-slot="{ field, errorMessage }"
                        >
                            <input
                                v-bind="field"
                                class="form-control form-control-sm"
                                autocomplete="off"
                                :class="{
                                    'is-invalid': errorMessage,
                                }"
                                :placeholder="$t('tools.form.connectorUrlPlaceholder')"
                                @blur="validateConnector"
                            />
                            <span
                                v-if="errorMessage"
                                class="validation-message text-danger"
                            >
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                    <div class="col-6">
                        <label>
                            {{ $t("connectors.form.connectorApiKey") }}
                        </label>
                        <Field
                            name="connectorApiKey"
                            :rules="isN8NConnectorToolType && apiKeyRequired ? 'required' : ''"
                            v-slot="{ field, errorMessage }"
                        >
                            <input
                                v-bind="field"
                                type="password"
                                class="form-control form-control-sm"
                                autocomplete="new-password"
                                @blur="validateConnector"
                                :class="{
                                    'is-invalid': errorMessage,
                                }"
                            />
                            <span
                                v-if="errorMessage"
                                class="validation-message text-danger"
                            >
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                </div>
                <div class="row mb-3">
                    <div class="col-6">
                        <label>
                            {{ $t("connectors.form.entries") }}
                        </label>
                        <Field
                            name="inputDataId"
                            rules="required"
                            v-slot="{ field, errorMessage }"
                        >
                            <select
                                v-bind="field"
                                class="form-select form-select-sm"
                                :class="{
                                    'is-invalid': errorMessage,
                                }"
                            >
                                <option value="">
                                    {{ $t("connectors.form.entriesSelect") }}
                                </option>
                                <option
                                    v-for="(item, index) in inputsList"
                                    :key="index"
                                    :value="item.id"
                                >
                                    {{ item.id }} -
                                    {{ item.name }}
                                </option>
                            </select>
                            <span
                                v-if="errorMessage"
                                class="validation-message text-danger"
                            >
                                {{ errorMessage }}
                            </span>
                        </Field>
                        <Field
                            v-slot="{ field }"
                            name="isEditableInput"
                            type="checkbox"
                            :value="true"
                            :unchecked-value="false"
                        >
                            <div class="form-check mt-1 p-0">
                                <input
                                    type="checkbox"
                                    name="isEditableInput"
                                    v-bind="field"
                                    :value="true"
                                    id="isEditableInput"
                                />
                                <label class="form-check-label ps-1"
                                       for="isEditableInput">
                                    {{ $t("connectors.form.entriesEditable") }}
                                </label>
                            </div>
                        </Field>
                    </div>
                    <div class="col-6">
                        <label>
                            {{ $t("common.output") }}
                        </label>
                        <Field
                            name="outputDataId"
                            rules="required"
                            v-slot="{ field, errorMessage }"
                        >
                            <select
                                v-bind="field"
                                class="form-select form-select-sm"
                                :class="{
                                    'is-invalid': errorMessage,
                                }"
                            >
                                <option value="">
                                    {{ $t("connectors.form.outputSelect") }}
                                </option>
                                <option
                                    v-for="(item, index) in outputsList"
                                    :key="index"
                                    :value="item.id"
                                >
                                    {{ item.id }} -
                                    {{ item.name }}
                                </option>
                            </select>
                            <span
                                v-if="errorMessage"
                                class="validation-message text-danger"
                            >
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                </div>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer">
                <button
                    class="btn btn-outline-primary btn-table btn-sm table-btn"
                    @click="close"
                >
                    {{ $t("common.cancel") }}
                </button>
                <button
                    class="btn btn-primary btn-sm"
                    @click="save"
                >
                    {{ $t(saveText) }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>
<script>
    import { Field, useForm } from "vee-validate";
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import ToolsService from "@/services/tools/ToolsServices";
    import ToolsTypesService from "@/services/tools/ToolsTypesService";
    import ToolsDataService from "@/services/tools/ToolsDataService";
    import ToolType from "@/constants/ToolType";
    import { translateIfExists } from "@/utils/i18nHelpers";

    export default {
        components: {
            ModalComponent,
            Field,
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
        emits: ["reload"],
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
        },
        data: () => ({
            typesList: [],
            inputsList: [],
            outputsList: [],
            isLoading: false,
            isN8NConnectorToolType: false,
            toolsData: {
                id: "",
                name: "",
                toolTypeId: "",
                inputDataId: "",
                outputDataId: "",
                isEditableInput: false,
                connectorUrl: "",
                connectorApiKey: "",
            },
        }),
        computed: {
            titleText() {
                return this.isEdit ? "connectors.formEdit.title" : "connectors.formCreate.title";
            },
            saveText() {
                return this.isEdit ? "connectors.editBtn" : "connectors.createBtn";
            },
            apiKeyRequired() {
                return this.isEdit && this.isN8NConnectorToolType ? false : true;
            },
        },
        methods: {
            toolTypeLabel(description) {
                return translateIfExists(this.$te, this.$t, description);
            },
            async validateConnector() {
                if (this.values.connectorUrl && this.values.connectorApiKey) {
                    this.$notify({
                        title: "connectors.index",
                        message: "connectors.form.validatingConnector",
                        variant: "warning",
                        icon: "CircleAlert",
                    });
                    let params = {
                        connectorUrl: this.values.connectorUrl,
                        connectorApiKey: this.values.connectorApiKey,
                    };
                    ToolsService.validateConnector(params).then((result) => {
                        if (result) {
                            return this.$notify({
                                title: "connectors.index",
                                message: "connectors.form.validConnector",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "connectors.index",
                                message: "connectors.form.invalidConnector",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    });
                }
            },
            changeToolType() {
                this.isN8NConnectorToolType =
                    (this.values.toolTypeId &&
                        this.typesList
                            .find((t) => t.id === this.values.toolTypeId)
                            ?.name?.toLowerCase()
                            ?.includes(ToolType.N8N.toLowerCase())) ||
                    false;
            },
            getToolTypes() {
                ToolsTypesService.getToolTypes().then((response) => {
                    this.typesList = response;
                });
            },
            getToolDatas() {
                ToolsDataService.getToollData().then((response) => {
                    this.inputsList = response;
                    this.outputsList = response;
                });
            },
            open(tool = null) {
                this.resetData();
                if (tool !== null) {
                    this.setValues({
                        id: tool.id,
                        name: tool.name,
                        toolTypeId: tool.toolTypeId,
                        inputDataId: tool.inputDataId,
                        outputDataId: tool.outputDataId,
                        isEditableInput: tool.isEditableInput,
                        connectorUrl: tool.connectorUrl,
                    });
                }
                this.changeToolType();
                this.$refs.ToolModal.open();
            },
            close() {
                this.$refs.ToolModal.close();
            },
            resetData() {
                this.resetForm({
                    values: {
                        id: "",
                        name: "",
                        toolTypeId: "",
                        inputDataId: "",
                        outputDataId: "",
                        isEditableInput: false,
                        connectorUrl: "",
                    },
                });
            },
            async save() {
                const result = await this.validate();
                if (!result.valid) {
                    return this.$notify({
                        title: "connectors.index",
                        message: "connectors.validationError",
                        variant: "warning",
                        icon: "CircleAlert",
                    });
                }

                if (this.isEdit) {
                    return this.editTool();
                }
                return this.createTool();
            },
            createTool() {
                this.isLoading = true;
                ToolsService.createTool(this.values)
                    .then((result) => {
                        if (result.error === undefined) {
                            this.$emit("reload");
                            this.close();
                            return this.$notify({
                                title: "connectors.index",
                                message: "connectors.createSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            if (result.error == 1) {
                                this.$notify({
                                    title: "connectors.index",
                                    message: "connectors.duplicated",
                                    variant: "danger",
                                    icon: "CircleX",
                                });
                            } else {
                                this.$notify({
                                    title: "connectors.index",
                                    message: "connectors.createError",
                                    variant: "danger",
                                    icon: "CircleX",
                                });
                            }
                        }
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            editTool() {
                this.isLoading = true;
                ToolsService.editTool(this.values)
                    .then((result) => {
                        if (result.error === undefined) {
                            this.$emit("reload");
                            this.close();
                            return this.$notify({
                                title: "connectors.index",
                                message: "connectors.editSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "connectors.index",
                                message: "connectors.editError",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
        },
        created() {
            this.resetData();
            this.getToolTypes();
            this.getToolDatas();
        },
    };
</script>
