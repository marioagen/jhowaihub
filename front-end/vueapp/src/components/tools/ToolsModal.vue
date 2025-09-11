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
                <div class="row">
                    <div class="col">
                        <label>{{ $t("tools.form.name") }}</label>
                        <input v-model="toolsData.name" class="form-control form-control-sm" />
                    </div>
                </div>
                <div class="row">
                    <div class="col">
                        <label>{{ $t("tools.form.types") }}</label>
                        <select
                            id="toolTypeId"
                            class="form-select form-select-sm"
                            v-model="toolsData.toolTypeId"
                        >
                            <option value="">{{ $t("tools.form.typesSelect") }}</option>
                            <option 
                                v-for="(item, index) in entriesList" 
                                :key="index"
                                :value="item.id" 
                            >
                                {{ item.id }} - {{ item.name }}
                            </option>
                        </select>
                    </div>
                </div>
                <div class="row">
                    <div class="col-6">
                        <label>{{ $t("tools.form.entries") }}</label>
                        <select
                            id="inputDataId"
                            class="form-select form-select-sm"
                            v-model="toolsData.inputDataId"
                        >
                            <option value="">{{ $t("tools.form.entriesSelect") }}</option>
                            <option 
                                v-for="(item, index) in entriesList" 
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
            entriesList: [],
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
                ToolsService.createTool(this.toolsData.name)
                    .then((result) => {
                        if (result.success) {
                            this.$emit("reload");
                            return this.$notify({
                                title: "tools.index",
                                message: "tools.createSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        }
                        
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
                        if (result.success) {
                            this.$emit("reload");
                            return this.$notify({
                                title: "tools.index",
                                message: "tools.editSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        }

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
    };
</script>
