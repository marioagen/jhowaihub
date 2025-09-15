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
                        <input v-model="toolsData.name" class="form-control form-control-sm" />
                    </div>
                </div>
                <div class="row mb-3">
                    <div class="col">
                        <label>{{ $t("tools.form.types") }}</label>
                        <select
                            id="toolTypeId"
                            class="form-select form-select-sm"
                            v-model="toolsData.toolTypeId"
                        >
                            <option value="">{{ $t("tools.form.typesSelect") }}</option>
                            <option 
                                v-for="(item, index) in typesList" 
                                :key="index"
                                :value="item.id" 
                            >
                                {{ item.id }} - {{ item.name }}
                            </option>
                        </select>
                    </div>
                </div>
                <div class="row mb-3">
                    <div class="col-6">
                        <label>{{ $t("tools.form.entries") }}</label>
                        <select
                            id="inputDataId"
                            class="form-select form-select-sm"
                            v-model="toolsData.inputDataId"
                        >
                            <option value="">{{ $t("tools.form.entriesSelect") }}</option>
                            <option 
                                v-for="(item, index) in inputsList" 
                                :key="index"
                                :value="item.id" 
                            >
                                {{ item.id }} - {{ item.name }}
                            </option>
                        </select>
                    </div>
                    <div class="col-6">
                        <label>{{ $t("tools.form.output") }}</label>
                        <select
                            id="outputDataId"
                            class="form-select form-select-sm"
                            v-model="toolsData.outputDataId"
                        >
                            <option value="">{{ $t("tools.form.outputSelect") }}</option>
                            <option 
                                v-for="(item, index) in outputsList" 
                                :key="index"
                                :value="item.id" 
                            >
                                {{ item.id }} - {{ item.name }}
                            </option>
                        </select>
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

    export default {
        components: {
            ModalComponent,
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
                    this.toolsData = tool;
                }
                this.$refs.ToolModal.open();
            },
            close() {
                this.$refs.ToolModal.close();
            },
            resetData() {
                this.toolsData = {
                    id: "",
                    name: "",
                    toolTypeId: "",
                    inputDataId: "",
                    outputDataId: "",
                };
            },
            save() {
                if (this.isEdit) {
                    return this.editTool();
                }
                return this.createTool();
            },
            createTool() {
                this.isLoading = true;
                let params = {
                    name: this.toolsData.name,
                    toolTypeId: this.toolsData.toolTypeId,
                    inputDataId: this.toolsData.inputDataId,
                    outputDataId: this.toolsData.outputDataId,
                };
                ToolsService.createTool(params)
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
                ToolsService.editTool(this.toolsData)
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
