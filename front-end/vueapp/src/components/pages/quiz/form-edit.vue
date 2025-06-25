<template>
    <main>
        <div class="container-fluid">
            <div class="mb-2 navbar-container">
                <div class="row">
                    <nav-bar :sidebarData="sidebarData" />
                </div>
            </div>
            <div class="custom-padding">
                <div class="row">
                    <breadcrumb :crumbs="crumbsData" />
                </div>
                <div class="row" style="height: 100%; overflow-y: auto;">
                    <div class="col-md-12">
                        <div class="form-save">
                            <form @submit="save">
                                <h5 class="mb-3"> {{ $t('labelGeneralInformation') }} </h5>
                                <div class="row">
                                    <div class="col-md-6">
                                        <div class="mb-3">
                                            <label class="form-label" for="title"> {{ $t('labelName') }} </label>
                                            <input type="text" class="form-control" id="title"
                                                   v-validate
                                                   ref="titleInpt" autocomplete="off" v-model="form.title" required />
                                        </div>
                                        <div class="mb-2" v-if="form.title.length <= 50">
                                            <a style="text-decoration: none; color: #AEB2BA; cursor: default;">
                                                {{ 50 - form.title.length }} {{ (50 - form.title.length) > 1 ? $t('labelCharacters') : $t('labelCharacter') }}
                                            </a>
                                        </div>
                                        <div class="mb-2" v-if="form.title.length > 50">
                                            <a style="text-decoration: none; color: #dc3545; cursor: default;">
                                                {{ 50 - form.title.length }} {{ $t('labelCharacters') }}
                                            </a>
                                            <a class="exceedDesc" style="float:right; text-decoration: none; color: #dc3545; cursor: default;">{{$t('labelExceedTitle')}}</a>
                                        </div>
                                        <div class="mb-3">
                                            <label class="form-label" for="typeDocId"> {{ $t('labelDocumentType') }} </label>
                                            <select class="form-select" id="typeDocId"
                                                    v-validate
                                                    v-model="form.typeDocId" required>
                                                <option value=""> {{ $t('labelSelect') }} </option>
                                                <option :value="item.id" v-for="(item, index) in typeDocs" :key="index"> {{ item.id }} - {{ item.name }} </option>
                                            </select>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="mb-3">
                                            <label class="form-label" for="questionsId">
                                                {{ $t('labelQuestions') }}
                                                <span v-if="form.questions.length == 1">
                                                    (<strong>{{ form.questions.length }}</strong> {{ $t('labelSelected') }})
                                                </span>
                                                <span v-if="form.questions.length > 1">
                                                    (<strong>{{ form.questions.length }}</strong> {{ $t('labelSelecteds') }})
                                                </span>
                                                <span class="badge rounded-pill bg-primary badge-custom" style="cursor: pointer;" :title="$t('labelClearSelection')" @click="form.questions = []" v-if="form.questions.length > 0">
                                                    <i class="fas fa-trash" style="font-size: .8em;float: left;margin: 2px 4px 0 auto;"></i>
                                                    {{ $t('labelClearSelection') }}
                                                </span>
                                            </label>
                                            <div>
                                                <Multiselect v-model="form.questions"
                                                             :options="questionsOptions"
                                                             mode="tags"
                                                             :breakTags="true"
                                                             :object="true"
                                                             :close-on-select="false"
                                                             :searchable="true"
                                                             :create-option="false"
                                                             ref="questionsInpt"
                                                             :placeholder="$t('labelSelectQuestions')"
                                                             :noOptionsText="$t('labelNoQuestionsRegistered')"
                                                             :noResultsText="form.questions.length === questionsOptions.length ? $t('labelNoMoreQuestionsAvailable') : $t('labelQuestionNotFound')" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <button type="submit" class="btn btn-primary m-2" :title="$t('labelSave')" style="float:right"> {{ $t('labelSave') }} </button>
                                <router-link class="btn btn-secondary m-2 btn-custom-cancel" style="float:right" :to="{ name: 'Quiz', query: { page: backPage } }" :title="$t('labelBack')"> {{ $t('labelBack') }} </router-link>
                            </form>
                        </div>
                    </div>
                    <div class="col-md-6"></div>
                </div>
            </div>
        </div>
        <!-- Component ToastAlert -->
        <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
    </main>
</template>

<script>
    import NavBar from '@/components/common/nav-bar';
    import Breadcrumb from '@/components/common/breadcrumb';
    import Multiselect from '@vueform/multiselect';
    import ToastAlert from '@/components/common/toast-alert';
    import api from "@/services/api";

    export default {
        name: "QuizFormEdit",
        directives: {
            validate: {
                inserted: function (el, binding) {
                    el.addEventListener('input', function () {
                        el.setCustomValidity('');
                        if (!el.checkValidity()) {
                            el.reportValidity();
                        }
                    });

                    el.addEventListener('invalid', function (event) {
                        event.preventDefault();
                        if (el.validity.valueMissing) {
                            el.setCustomValidity(this.$t('labelFillInThisField'));
                        }
                        el.reportValidity();
                    });
                }
            }
        },
        data() {
            return {
                crumbsData: [],
                sidebarData: "QuizEdit",
                idEditing: this.$route.params.id,
                backPage: this.$route.query.page,
                typeDocs: [],
                questionsOptions: [],
                form: {
                    title: '',
                    typeDocId: '',
                    questions: [],
                },
                myInterval: null,
                toastShow: false,
                toastColor: "",
                toastMessage: "",
            }
        },
        components: {
            NavBar,
            Breadcrumb,
            Multiselect,
            ToastAlert,
        },
        watch: {
            '$store.state.userProfile.language': function () {
                this.setCrumbsData();
            },
        },
        methods: {
            setCrumbsData: function () {
                this.crumbsData = [
                    { crumb: this.$t('labelQuestionnaires'), link: { to: 'Quiz' } },
                    { crumb: this.$t('labelEdit'), link: { to: 'QuizEdit', queryPage: this.$route.query.page } },
                ];
            },
            getQuiz: function () {
                let self = this;
                api.get('/Questionnaire/' + this.idEditing)
                    .then(function (response) { // Handle success
                        self.form.title = response.data.title;
                        self.form.typeDocId = response.data.typeDoc.id;
                        var questions = response.data.questions;
                        for (let i = 0; i < questions.length; i++) {
                            var item = {
                                value: questions[i].id,
                                label: questions[i].id + " - " + questions[i].description
                            };
                            self.form.questions.push(item);
                        }
                    }).catch(function (e) { // Handle error
                        console.log(e);
                    }).finally(function () { // Always executed
                        console.log("Finished request.");
                    });
            },
            getTypeDocs: function () {
                let self = this;
                api.get('/TypeDoc/FindAll')
                    .then(function (response) { // Handle success
                        self.typeDocs = response.data;
                    }).catch(function (e) { // Handle error
                        console.log(e);
                    }).finally(function () { // Always executed
                        console.log("Finished request.");
                    });
            },
            getQuestions: function () {
                let self = this;
                api.get('/Question/FindAll')
                    .then(function (response) { // Handle success
                        for (let i = 0; i < response.data.length; i++) {
                            var item = {
                                value: response.data[i].id,
                                label: response.data[i].id + " - " + response.data[i].description
                            };
                            self.questionsOptions.push(item);
                        }
                    }).catch(function (e) { // Handle error
                        console.log(e);
                    }).finally(function () { // Always executed
                        console.log("Finished request.");
                    });
            },
            save: function (e) {
                e.preventDefault();
                var paramsData = {
                    title: this.form.title,
                    typeDocId: this.form.typeDocId,
                    questionsId: this.form.questions.map(obj => obj.value),
                    id: parseInt(this.idEditing),
                };
                if (paramsData.questionsId.length == 0) { // If no question is selected
                    this.$refs.questionsInpt.focus();
                    this.clearMyInterval();
                    this.alertToast(this.$t('labelNoQuestionsWereSelected'), "toast-danger");
                } else {
                    let self = this;
                    api.put('/Questionnaire', paramsData)
                        .then(function (response) { // Handle success
                            self.$router.push({ name: 'Quiz', query: { page: self.backPage } });
                        }).catch(function (e) { // Handle error
                            if (e.response.status == 409) {
                                self.alertToast(self.$t('labelDuplicatedQuestionnaire'), "toast-warning");
                            }
                            else {
                                self.alertToast(self.$t('labelQuestionnaireError'), "toast-warning");
                            }
                        }).finally(function () { // Always executed
                            console.log("Finished request.");
                        });
                }
            },
            alertToast: function (msg, color) {
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;
                let self = this;
                this.myInterval = setInterval(function () {
                    self.toastMessage = "";
                    self.toastColor = "";
                    self.toastShow = false;
                    clearInterval(self.myInterval);
                }, 4000);
            },
            closeToast: function () {
                this.toastShow = false;
                this.clearMyInterval();
            },
            clearMyInterval: function () {
                clearInterval(this.myInterval);
                this.myInterval = null;
            },
            cleanForm: function () {
                this.form = {
                    title: '',
                    typeDocId: '',
                    questions: [],
                };
            },
        },
        computed: {},
        created() {
            this.setCrumbsData();
            this.getQuiz();
            this.getTypeDocs();
            this.getQuestions();
        },
        mounted() {
            this.$refs.titleInpt.focus();
        },
        unmounted() { },
    }
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
</style>
