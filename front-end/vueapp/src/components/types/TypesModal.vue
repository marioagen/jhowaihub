<template>
    <ModalComponent
        id="typeModal"
        :isLoading="isLoading"
        @save="save"
        ref="TypeModal"
    >
        <template #header>
            <div class="modal-header">
                <h5 class="modal-title"> {{ titleText }} </h5>
                <button 
                    class="btn-close" 
                    data-bs-dismiss="modal" 
                    @click="close" 
                />
            </div>
        </template>

        <template #body>
            <div class="modal-body">
                <label>Name</label>
                <input v-model="typeData.name" class="form-control" />
            </div>
        </template>

        <template #footer>
            <div class="modal-footer">
                <button 
                    class="btn btn-secondary btn-sm" 
                    @click="close"
                >
                    Cancel
                </button>
                <button 
                    class="btn btn-primary btn-sm" 
                    @click="save"
                >
                    {{ saveText }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>

<script>
    import ModalComponent from '@/components/global/ModalComponent.vue';
    import TypesService from '@/services/types/TypesService';
    
    export default {
        components: {
            ModalComponent
        },
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
        },
        data: () => ({
            typeData: {
                id: "",
                name: "",
            },
            isLoading: false,
        }),
        computed: {
            titleText() {
                return this.isEdit ? "labelEditTitleType" : "labelSaveTitleType";
            },
            saveText() {
                return this.isEdit ? "labelEditType" : "labelSaveType";
            },
        },
        methods: {
            open(type = null) {
                if(type === null) {
                    this.resetData();
                } else {
                    this.typeData = type;
                }
                this.$refs.TypeModal.open();
            },
            close() {
                this.$refs.TypeModal.close();
            },
            resetData() {
                this.typeData = { id: "", name: "" };
            },
            save() {
                if(this.isEdit) {
                    return this.editType();
                }
                return this.createType();
            },
            createType() {
                this.isLoading = true;
                TypesService.addType(this.typeData.name)
                    .then((result) => {
                        if (result.success) {
                            this.alertToast(this.$t("labelDocumentTypeSuccess"), "toast-success");
                            this.close();
                            this.resetData();
                            this.$emit("reload");
                            return;
                        } 
                        const messageKey = result.status === 409 ? "labelDocumentTypeAlreadyExists" : "labelDocumentTypeError";
                        this.alertToast(this.$t(messageKey), "toast-warning");
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });

            },
            editType() {
                this.isLoading = true;
                TypesService.editType(this.typeData)
                    .then((result) => {
                        if (result.success) {
                            this.alertToast(this.$t("labelDocumentTypeEditSuccess"), "toast-success");
                            this.close();
                            this.$emit("reload");
                            return;
                        }

                        const messageKey = result.status === 409 ? "labelDocumentTypeAlreadyExists" : "labelDocumentTypeError";
                        this.alertToast(this.$t(messageKey), "toast-warning");
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            alertToast(msg, color) {
                this.clearMyInterval();
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;

                this.myInterval = setTimeout(() => {
                    this.toastMessage = "";
                    this.toastColor = "";
                    this.toastShow = false;
                    this.myInterval = null;
                }, 4000);
            },
            closeToast: function () {
                this.toastShow = false;
                this.clearMyInterval();
            },
            clearMyInterval() {
                if (this.myInterval) {
                    clearTimeout(this.myInterval);
                    this.myInterval = null;
                }
            },
        }
    }
</script>