<template>
    <div class="modal fade show" id="novoTimeModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="overlay" :class="{ active: showModalUserTeam }"></div>
                <div class="modal-header custom-header">
                    <h6 class="modal-title" id="novoTimeModalLabel">
                        {{ userData.id ? $t("labelEditUser") : $t("labelNewUser") }}
                        <small class="text-muted d-block text-sm">{{ $t("labelNewUserMessage") }}</small>
                    </h6>
                    <button type="button" class="btn-close" @click="close"></button>
                </div>
                <Form ref="formRef" @submit="saveUser">
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-6">
                                <div class="mb-3">
                                    <label for="userName" class="form-label fw-semibold mb-0">
                                        {{ $t("labelName") }}
                                    </label>
                                    <Field
                                        type="text"
                                        class="form-control form-control-sm"
                                        id="userName"
                                        ref="userNameInput"
                                        autocomplete="off"
                                        name="userName"
                                        :rules="'required|min:3|max:150'"
                                        v-model="userData.name"
                                        :placeholder="$t('labelTypeUserName')"
                                    />
                                    <ErrorMessage name="userName" class="invalid-feedback d-block" />
                                </div>
                            </div>
                            <div class="col-6">
                                <div class="mb-3">
                                    <label for="userEmail" class="form-label fw-semibold mb-0">
                                        {{ $t("labelEmail") }}
                                    </label>
                                    <Field
                                        type="text"
                                        class="form-control form-control-sm"
                                        id="userEmail"
                                        ref="userEmailInput"
                                        autocomplete="off"
                                        name="userEmail"
                                        :rules="'required|min:5|max:100|email'"
                                        v-model="userData.email"
                                        :placeholder="$t('labelTypeUserEmail')"
                                        @blur="validateEmailBackend"
                                    />
                                    <ErrorMessage name="userEmail" class="invalid-feedback d-block" />
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-6">
                                <label for="userPassword" class="form-label fw-semibold mb-0">
                                    {{ $t("labelPassword") }}
                                </label>
                                <PasswordInputComponent
                                    :placeholder="$t('labelTypePassword')"
                                    :rules="passwordRules"
                                    name="userPassword"
                                    v-model="userData.password"
                                />
                            </div>
                            <div class="col-6">
                                <label for="userConfirmedPassword" class="form-label fw-semibold mb-0">
                                    {{ $t("labelConfirmedPassword") }}
                                </label>
                                <PasswordInputComponent
                                    :placeholder="$t('labelTypeConfirmedPassword')"
                                    :rules="confirmedPasswordRules"
                                    name="userConfirmedPassword"
                                    v-model="userData.confirmedPassword"
                                />
                            </div>
                        </div>
                        <SelectionListComponent
                            :id="'profiles'"
                            :labelPanel="'labelProfiles'"
                            :labelSelectedQuantity="'labelSelectedProfiles'"
                            :labelSearch="'labelSearchProfiles'"
                            :items="profiles"
                            :loading="loading"
                            v-model:selectedItems="selectedProfiles"
                        />

                        <SelectionListComponent
                            :id="'teams'"
                            :labelPanel="'labelTeams'"
                            :labelSelectedQuantity="'labelSelectedTeams'"
                            :labelSearch="'labelSearchTeams'"
                            :items="teams"
                            :loading="loading"
                            v-model:selectedItems="selectedTeams"
                        >
                            <template #footer>
                                <div class="border-top mt-2 pt-2">
                                    <button
                                        type="button"
                                        class="btn btn-sm btn-outline-secondary fw-semibold"
                                        @click="addNewTeam"
                                    >
                                        + {{ $t("labelNewTeam") }}
                                    </button>
                                </div>
                            </template>
                        </SelectionListComponent>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary btn-sm" @click="close">
                            {{ $t("labelCancel") }}
                        </button>
                        <button v-if="userData.id" type="submit" class="btn btn-primary btn-sm">
                            {{ $t("labelEdit") }}
                        </button>
                        <button v-else type="submit" class="btn btn-primary btn-sm">
                            {{ $t("labelCreate") }}
                        </button>
                    </div>
                </Form>
            </div>
        </div>
    </div>
    <modal-user-team v-if="showModalUserTeam" @close="closeModalUserTeam" @teamCreated="teamCreated"></modal-user-team>
    <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
</template>

<script>
    import api from "@/services/api";
    import ModalUserTeam from "@/components/management/users/modals/TeamModal.vue";
    import ToastAlert from "@/components/common/toast-alert";
    import ErrorCode from "@/constants/Errorcode";
    import { Form, Field, ErrorMessage } from "vee-validate";
    import SelectionListComponent from "@/components/global/SelectionListComponent.vue";
    import PasswordInputComponent from "@/components/global/PasswordInputComponent.vue";

    export default {
        name: "ModalUser",
        components: {
            ModalUserTeam,
            ToastAlert,
            ErrorCode,
            Form,
            Field,
            ErrorMessage,
            SelectionListComponent,
            PasswordInputComponent,
        },
        props: {
            userEditing: {
                required: true,
                type: Object,
                default: {},
            },
        },
        data() {
            return {
                userData: {
                    id: this.userEditing.id ? this.userEditing.id : null,
                    name: this.userEditing.name ? this.userEditing.name : "",
                    email: this.userEditing.email ? this.userEditing.email : "",
                    teams: this.userEditing.teams ? this.userEditing.teams : [],
                    profiles: this.userEditing.profiles ? this.userEditing.profiles : [],
                    password: "",
                },
                selectedTeams: this.userEditing.teams ? this.userEditing.teams.map((u) => u.id) : [],
                selectedProfiles: this.userEditing.profiles ? this.userEditing.profiles.map((u) => u.id) : [],
                searchTeams: "",
                searchProfiles: "",
                teams: [],
                profiles: [],
                loading: false,
                showModalUserTeam: false,
                toastShow: false,
                toastColor: "",
                toastMessage: "",
                myInterval: null,
                showPassword: false,
                showConfirmedPassword: false,
            };
        },
        emits: ["close", "userCreated"],
        computed: {
            filteredTeams() {
                if (!this.searchTeams) {
                    return this.teams;
                }
                return this.teams.filter((team) => team.name.toLowerCase().includes(this.searchTeams.toLowerCase()));
            },
            passwordRules() {
                return {
                    required: this.userEditing.id ? false : true,
                    custom_password: true,
                    min: 6,
                    max: 50,
                };
            },
            confirmedPasswordRules() {
                return {
                    required: this.userEditing.id ? false : true,
                    confirmed: "userPassword",
                    min: 6,
                    max: 50,
                };
            },
        },
        mounted() {
            this.loadTeams();
            this.loadProfiles();
        },
        methods: {
            async validateEmailBackend() {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(this.userData.email.trim())) {
                    return;
                }
                var paramsReq = {
                    email: this.userData.email.trim(),
                    userId: this.userData.id,
                };
                let self = this;
                api.post("User/IsEmailInUse", paramsReq)
                    .then(function (response) {
                        if (response && response.data && response.data === true) {
                            self.$refs.formRef.setFieldError("userEmail", self.$t("labelErrorEmailAlreadyExists"));
                        } else {
                            self.$refs.formRef.setFieldError("userEmail", "");
                        }
                        self.loading = false;
                    })
                    .catch(function (e) {
                        self.alertToast(self.$t("labelUserError"), "toast-warning");
                        self.loading = false;
                    })
                    .finally(function () {
                        console.log("Finished request.");
                    });
            },
            loadTeams() {
                var paramsReq = {
                    search: "",
                    pageSize: 0,
                    page: 1,
                    isAscending: this.isAscending,
                };

                api.get("/Team/Paged", { params: paramsReq })
                    .then((response) => {
                        this.teams = response.data.content;
                        this.loading = false;
                    })
                    .catch((e) => {
                        console.log(e);
                        this.loading = false;
                    })
                    .finally(() => {
                        console.log("Finished request.");
                        this.loading = false;
                    });
            },
            loadProfiles() {
                var paramsReq = {
                    search: "",
                    pageSize: 0,
                    page: 1,
                    isAscending: this.isAscending,
                };

                api.get("/Profile/Paged", { params: paramsReq })
                    .then((response) => {
                        this.profiles = response.data.content;
                    })
                    .catch((e) => {
                        console.log(e);
                    })
                    .finally(() => {
                        this.loading = false;
                    });
            },
            selectAll() {
                this.selectedTeams = this.filteredTeams.map((user) => user.id);
            },
            clearSelection() {
                this.selectedTeams = [];
            },
            addNewTeam() {
                this.showModalUserTeam = true;
            },
            saveUser: function (e) {
                let response;
                let self = this;

                if (this.userData.id == null) {
                    const user = {
                        name: this.userData.name,
                        email: this.userData.email,
                        password: this.userData.password,
                        teamIds: this.selectedTeams,
                        profileIds: this.selectedProfiles,
                    };
                    response = api.post("User", user);
                } else {
                    const userEdit = {
                        name: this.userData.name,
                        email: this.userData.email,
                        password: this.userData.password,
                        teamIds: this.selectedTeams,
                        profileIds: this.selectedProfiles,
                        id: this.userData.id,
                    };
                    response = api.put("User", userEdit);
                }
                response
                    .then((response) => {
                        this.$emit("userCreated");
                        this.close();
                    })
                    .catch((e) => {
                        self.alertToast(self.$t("labelUserError"), "toast-warning");
                    });
            },
            resetForm() {
                this.userData.id = 0;
                this.userData.name = "";
                this.selectedTeams = [];
                this.searchTeams = "";

                if (this.$refs.formRef) {
                    this.$refs.formRef.resetForm();
                }
            },
            close: function () {
                this.$emit("close");
            },
            closeModalUserTeam() {
                this.showModalUserTeam = false;
                this.loadTeams();
            },
            teamCreated() {
                this.loadTeams();
                this.closeModalUserTeam();
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
        },
    };
</script>

<style scoped>
    .custom-header {
        padding: 15px 15px 0;
        border-bottom-width: 0px !important;
    }

    .initials {
        width: 30px;
        height: 30px;
    }

    .show {
        display: block;
    }

    .overlay.active {
        display: block;
        z-index: 1060;
    }

    .overlay {
        display: none;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.85);
        position: absolute;
        left: 0;
        top: 0;
        z-index: -1;
    }
</style>
