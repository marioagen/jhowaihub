<template>
    <ModalComponent id="toolModal" :isLoading="isLoading" @save="save" ref="ToolModal">
        <template #header>
            <div class="modal-header">
                <h5 class="modal-title">{{ $t(titleText) }}</h5>
                <button class="btn-close" data-bs-dismiss="modal" @click="close" />
            </div>
        </template>
        <template #body>
            <div class="modal-body">
                <div class="row mb-3">
                    <div class="col">
                        <label>{{ $t("tools.form.name") }}</label>
                        <Field name="name" rules="required" v-slot="{ field, errorMessage }">
                            <input v-bind="field" class="form-control form-control-sm"
                                :class="{ 'is-invalid': errorMessage }" />
                            <span v-if="errorMessage" class="validation-message text-danger">
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                </div>
                <div class="row mb-3">
                    <div class="col">
                        <label>{{ $t("tools.form.types") }}</label>
                        <Field name="toolTypeId" rules="required" v-slot="{ field, errorMessage }">
                            <select v-bind="field" class="form-select form-select-sm"
                                :class="{ 'is-invalid': errorMessage }">
                                <option value="">{{ $t("tools.form.typesSelect") }}</option>
                                <option v-for="(item, index) in typesList" :key="index" :value="item.id">
                                    {{ item.id }} - {{ item.name }}
                                </option>
                            </select>
                            <span v-if="errorMessage" class="validation-message text-danger">
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                </div>
                <div class="row mb-3">
                    <div class="col-6">
                        <label>{{ $t("tools.form.entries") }}</label>
                        <Field name="inputDataId" rules="required" v-slot="{ field, errorMessage }">
                            <select v-bind="field" class="form-select form-select-sm"
                                :class="{ 'is-invalid': errorMessage }">
                                <option value="">{{ $t("tools.form.entriesSelect") }}</option>
                                <option v-for="(item, index) in inputsList" :key="index" :value="item.id">
                                    {{ item.id }} - {{ item.name }}
                                </option>
                            </select>
                            <span v-if="errorMessage" class="validation-message text-danger">
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                    <div class="col-6">
                        <label>{{ $t("tools.form.output") }}</label>
                        <Field name="outputDataId" rules="required" v-slot="{ field, errorMessage }">
                            <select v-bind="field" class="form-select form-select-sm"
                                :class="{ 'is-invalid': errorMessage }">
                                <option value="">{{ $t("tools.form.outputSelect") }}</option>
                                <option v-for="(item, index) in outputsList" :key="index" :value="item.id">
                                    {{ item.id }} - {{ item.name }}
                                </option>
                            </select>
                            <span v-if="errorMessage" class="validation-message text-danger">
                                {{ errorMessage }}
                            </span>
                        </Field>
                    </div>
                </div>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer">
                <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="close">
                    {{ $t("labelCancel") }}
                </button>
                <button class="btn btn-primary btn-sm" @click="save">
                    {{ $t(saveText) }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>

<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import ToolsService from "@/services/tools/ToolsServices";
    import ToolsTypesService from '@/services/tools/ToolsTypesService';
    import ToolsDataService from '@/services/tools/ToolsDataService';
    import { Field, useForm } from "vee-validate";

    export default {
        components: {
            ModalComponent,
            Field,
        },
        setup() {
            const { validate, setValues, values } = useForm();
            return { validate, setValues, values };
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
            toolsData: {
                id: "",
                name: "",
                toolTypeId: "",
                inputDataId: "",
                outputDataId: "",
            },
        }),
        computed: {
            titleText() {
                return this.isEdit ? "tools.formEdit.title" : "tools.formCreate.title";
            },
            saveText() {
                return this.isEdit ? "tools.editBtn" : "tools.createBtn";
            },
        },
        methods: {
            getToolTypes() {
                ToolsTypesService.getToolTypes()
                    .then((response) => {
                        this.typesList = response;
                    });
            },
            getToolDatas() {
                ToolsDataService.getToollData()
                    .then((response) => {
                        this.inputsList = response;
                        this.outputsList = response;
                    });
            },
            open(tool = null) {
                if (tool === null) {
                    this.resetData();
                } else {
                    this.setValues({
                        id: tool.id,
                        name: tool.name,
                        toolTypeId: tool.toolTypeId,
                        inputDataId: tool.inputDataId,
                        outputDataId: tool.outputDataId,
                    });
                }
                this.$refs.ToolModal.open();
            },
            close() {
                this.$refs.ToolModal.close();
            },
            resetData() {
                this.values.name = "";
                this.values.toolTypeId = "";
                this.values.inputDataId = "";
                this.values.outputDataId = "";
            },
            async save() {
                const result = await this.validate();
                if (!result.valid) {
                    return this.$notify({
                        title: "tools.index",
                        message: "tools.validationError",
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
                        if (result) {
                            this.$emit("reload");
                            this.close();
                            return this.$notify({
                                title: "tools.index",
                                message: "tools.createSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        }
                    })
                    .catch(() => {
                        this.$notify({
                            title: "tools.index",
                            message: "tools.createError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },            
            editTool() {
                this.isLoading = true;
                ToolsService.editTool(this.values)
                    .then((result) => {                        
                        if (result) {
                            this.$emit("reload");
                            this.close();
                            return this.$notify({
                                title: "tools.index",
                                message: "tools.editSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        }
                    })
                    .catch(() => {
                        this.$notify({
                            title: "tools.index",
                            message: "tools.editError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
        },
        created() {
            this.getToolTypes();
            this.getToolDatas();
        }
    };
</script>
