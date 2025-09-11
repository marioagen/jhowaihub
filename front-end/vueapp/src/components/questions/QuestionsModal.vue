<template>
    <ModalComponent id="questionModal" :isLoading="isLoading" @save="save" ref="QuestionModal">
        <template #header>
            <div class="modal-header">
                <h5 class="modal-title"> {{ $t(titleText) }} </h5>
                <button class="btn-close" data-bs-dismiss="modal" @click="close" />
            </div>
        </template>

        <template #body>
            <div class="modal-body">
                <label>{{ $t("questions.description") }}</label>
                <textarea rows="7" v-model="questionData.description" class="form-control" />
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
import ModalComponent from '@/components/global/ModalComponent.vue';
import QuestionsService from '@/services/questions/QuestionsService';

export default {
    components: {
        ModalComponent
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
        questionData: {
            id: "",
            description: "",
        },
        isLoading: false,
    }),
    computed: {
        titleText() {
            return this.isEdit ? "questions.modalEdit.title" : "questions.modalCreate.title";
        },
        saveText() {
            return this.isEdit ? "questions.modalEdit.save" : "questions.modalCreate.save";
        },
    },
    methods: {
        open(type = null) {
            if (type === null) {
                this.resetData();
            } else {
                this.questionData = type;
            }
            this.$refs.QuestionModal.open();
        },
        close() {
            this.$refs.QuestionModal.close();
        },
        resetData() {
            this.questionData = { id: "", name: "" };
        },
        save() {
            if (this.isEdit) {
                return this.editQuestion();
            }
            return this.createQuestion();
        },
        createQuestion() {
            this.isLoading = true;
            QuestionsService.createQuestion(this.questionData.description)
                .then((result) => {
                    if (!result.error) {
                        this.resetData();
                        this.$emit("reload");
                        this.close();
                        return this.$notify({
                            title: this.$t("questions.title"),
                            message: this.$t("questions.createSuccess"),
                            variant: 'success',
                            icon: 'CircleCheckBig',
                        });
                    }

                    const messageKey = result.error === "labelQuestionAlreadyExists"
                        ? "questions.errorDuplicated"
                        : "questions.createError";

                    this.$notify({
                        title: this.$t("questions.title"),
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
                            title: this.$t("questions.title"),
                            message: this.$t("questions.editSuccess"),
                            variant: 'success',
                            icon: 'CircleCheckBig',
                        });
                    }

                    const messageKey = result.status === 409 ? "questions.errorDuplicated" : "questions.editError";
                    this.$notify({
                       title: this.$t("questions.title"),
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