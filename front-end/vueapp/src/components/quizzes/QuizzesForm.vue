<template>
    <main>
        <div class="container-fluid scroll-area mx-4 mt-4">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="row">
                        <div class="col">
                            <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="returnToTable">
                                <LucideIcon icon="ArrowLeft" />
                                {{ $t("labelBack") }}
                            </button>
                        </div>
                        <div class="col-8">
                            <div>
                                <h5 class="mb-0 fw-bold">{{ $t(formTitle) }}</h5>
                                <p><small class="text-muted">{{ $t(formSubtitle) }}</small></p>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="col-auto ms-auto">
                    <button class="btn btn-primary btn-sm" @click="save">
                        <LucideIcon icon="Save" size="15" />
                        {{ $t("quizzes.formSave") }}
                    </button>
                </div>
            </div>

            <div class="row mt-1">
                <div class="main-div shadow-sm">
                     <div>
                        <h6 class="mb-0">{{ $t("quizzes.basicInfo") }}</h6>
                        <p>
                            <small class="text-muted">{{ $t("quizzes.basicInfoSubtitle") }}</small>
                        </p>
                    </div>
                    <div class="row">
                        <div class="col">
                            <label>{{ $t("quizzes.formName") }}</label>
                            <input 
                                class="form-control form-control-sm"
                                :placeholder="$t('quizzes.formNamePlaceholder')"
                                v-model="form.title"
                            />
                        </div>
                        <div class="col">
                            <label>{{ $t("quizzes.type") }}</label>
                            <select
                                id="typeDocId"
                                class="form-select form-select-sm"
                                v-model="form.typeDocId"
                            >
                                <option value="">{{ $t("quizzes.formSelect") }}</option>
                                <option 
                                    v-for="(item, index) in docTypesList" 
                                    :key="index"
                                    :value="item.id" 
                                >
                                    {{ item.id }} - {{ item.name }}
                                </option>
                            </select>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row mt-4">
                <div class="main-div shadow-sm">
                    <div>
                        <h6 class="mb-0">{{ $t("quizzes.questionsSection.title") }}</h6>
                        <p>
                            <small class="text-muted">{{ $t("quizzes.questionsSection.subtitle") }}</small>
                        </p>
                    </div>
                    <div class="row">
                        <TransferListComponent 
                            v-model="form.questions"
                            :available="questionsList"
                            :key="questionsList"
                            transferListTitle="questions.availableList"
                            transferListPlaceholder="questions.filters.input"
                        />    
                    </div>
                    <button 
                        class="btn btn-outline-primary btn-sm table-btn mt-4"
                        @click="openModalQuestion"
                    >
                        <LucideIcon icon="Plus" size="17" />
                        {{ $t("questions.createBtn") }}
                    </button>
                </div>
            </div>
        
        <QuestionsModal
            :isEdit="false"
            @reload="getQuestions()"
            ref="QuestionsModal"
        />
        </div>
    </main>
</template>

<script>
    import TransferListComponent from "@/components/global/TransferListComponent.vue";
    import QuestionsModal from "@/components/questions/QuestionsModal.vue";
    import TypesService from "@/services/types/TypesService";
    import QuestionsService from "@/services/questions/QuestionsService";
    import QuizzesService from "@/services/quizzes/QuizzesService";

    export default {
        name: "QuizFormNew",
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
            id: {
                type: Number,
                required: false,
            }
        },
        components: {
            TransferListComponent,
            QuestionsModal,
        },
        data() {
            return {
                docTypesList: [],
                questionsList: [],
                form: {
                    title: "",
                    typeDocId: "",
                    questions: [],
                },
                myInterval: null,
            };
        },
        watch: {
            "$store.state.userProfile.language": function () {
                this.setCrumbsData();
            },
        },
        methods: {
            getDocTypes() {
                TypesService.getTypesList()
                    .then((response) => {
                        if(response.error === undefined) {
                            return this.docTypesList = response;
                        }
                    });
            },
            getQuestions() {
                QuestionsService.getQuestionsList()
                    .then((response) => {
                        for (let i = 0; i < response.length; i++) {
                            var item = {
                                id: response[i].id,
                                text: response[i].id + " - " + response[i].description,
                            };
                            this.questionsList.push(item);
                        }
                    });
            },
            setForm() {
                if(!this.isEdit) return;
                QuizzesService.getQuizzById(this.id)
                    .then((response) => {
                        this.form = {
                            title: response.title,
                            typeDocId: response.typeDoc.id,
                            questions: response.questions,
                        }
                    });
            },
            save() {
                if(this.isEdit) {
                    return this.editQuizz();
                }
                return this.createQuizz();
            },
            createQuizz() {
                var paramsData = {
                    title: this.form.title,
                    typeDocId: this.form.typeDocId,
                    questionsId: this.form.questions.map((obj) => obj.id),
                };

                QuizzesService.createQuizz(paramsData)
                    .then((response) => {
                        if(response.error === undefined) {
                            this.$notify({
                                title: 'quizzes.title',
                                message: 'quizzes.createSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                            return this.returnToTable();
                        }
                        this.$notify({
                            title: 'quizzes.title',
                            message: response.error,
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    });
            },
            editQuizz() {
                var paramsData = {
                    id: parseInt(this.id),
                    title: this.form.title,
                    typeDocId: this.form.typeDocId,
                    questionsId: this.form.questions.map((obj) => obj.id),
                };

                QuizzesService.editQuizz(paramsData)
                    .then((response) => {
                        if(response.error === undefined) {
                            this.$notify({
                                title: 'quizzes.title',
                                message: 'quizzes.editSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                            return this.returnToTable();
                        }
                        this.$notify({
                            title: 'quizzes.title',
                            message: response.error,
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    });
            },
            resetForm() {
                this.form = {
                    title: "",
                    typeDocId: "",
                    questions: [],
                };
            },
            openModalQuestion() {
                this.$refs.QuestionsModal.open();
            },
            returnToTable() {
                return this.$router.push({ name: "Quiz" });
            },
        },
        computed: {
            formTitle() {
                return this.isEdit ? "quizzes.formEdit.title" : "quizzes.formCreate.title";
            },
            formSubtitle() {
                return this.isEdit ? "quizzes.formEdit.subtitle" : "quizzes.formCreate.subtitle";
            },
        },
        created() {
            this.getDocTypes();
            this.getQuestions();
            this.setForm();
        },
    };
</script>

<style scoped>
    @import "@vueform/multiselect/themes/default.css";

    .multiselect-dropdown {
        max-height: var(--ms-max-height) !important;
    }

    .form-save {
        padding-top: 20px !important;
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }

    .container-fluid {
        padding: 0 13px;
    }

    .main-div {
        border: 1px solid #d3d3d3;
        border-radius: 8px;
        background: white;
        padding: 20px 24px;
    }
</style>
