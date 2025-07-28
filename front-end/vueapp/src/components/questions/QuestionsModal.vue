<template>
    <ModalComponent
        id="questionsModal"
        :isLoading="isLoading"
        @save="save"
        ref="questionsModal"
    >
        <template #header>
            <div class="modal-header">
                <h5 class="modal-title"> {{ $t(titleText) }} </h5>
                <button 
                    class="btn-close" 
                    data-bs-dismiss="modal" 
                    @click="close" 
                />
            </div>
        </template>

        <template #body>
            <div class="modal-body">
                <label>Description</label>
                <textarea rows="7" v-model="questionData.description" class="form-control" />
            </div>
        </template>

        <template #footer>
            <div class="modal-footer">
                <button 
                    class="btn btn-secondary btn-sm" 
                    @click="close"
                >
                    {{ $t("labelCancel") }}
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
    import ModalComponent from '@/components/global/ModalComponent.vue';
    import QuestionsService from '@/services/questions/QuestionsService';
    
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
            questionData: {
                id: "",
                description: "",
            },
            isLoading: false,
        }),
        computed: {
            titleText() {
                return this.isEdit ? "labelEditTitleQuestion" : "labelSaveTitleQuestion";
            },
            saveText() {
                return this.isEdit ? "labelEditQuestion" : "labelSaveQuestion";
            },
        },
        methods: {
            open(type = null) {
                if(type === null) {
                    this.resetData();
                } else {
                    this.questionData = type;
                }
                this.$refs.questionsModal.open();
            },
            close() {
                this.$refs.questionsModal.close();
            },
            resetData() {
                this.questionData = { id: "", name: "" };
            },
            save() {
                if(this.isEdit) {
                    return this.editQuestion();
                }
                return this.createQuestion();
            },
            createQuestion() {
                this.isLoading = true;
                QuestionsService.createQuestion(this.questionData.name)
                    .then((result) => {
                        if (result.success) {
                            this.$emit('reload');
                            return this.$notify({
                                title: 'Tipos',
                                message: this.$t("labelQuestionSuccess"),
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        } 
                        const messageKey = result.status === 409 ? "labelQuestionAlreadyExists" : "labelQuestionError";
                        this.$notify({
                            title: 'Tipos',
                            message: this.$t(messageKey),
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });

            },
            editQuestion() {
                this.isLoading = true;
                QuestionsService.editQuestion(this.questionData)
                    .then((result) => {
                        if (result) {
                            this.$emit("reload");
                            return this.$notify({
                                title: 'Tipos',
                                message: this.$t("labelQuestionEditSuccess"),
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        }

                        const messageKey = result.status === 409 ? "labelQuestionAlreadyExists" : "labelQuestionError";
                        this.$notify({
                            title: 'Tipos',
                            message: this.$t(messageKey),
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
        }
    }
</script>