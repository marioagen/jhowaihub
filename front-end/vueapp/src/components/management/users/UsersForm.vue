<template>
    <main>
        <div class="container-fluid scroll-area mx-4 mt-4">
            <div class="row align-items-center">
                <div class="col-6">
                    <div class="row">
                        <div class="col-1">
                            <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="returnToTable">
                                <LucideIcon icon="ArrowLeft" />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">{{ $t(formTitle) }}</h5>
                                <p><small class="text-muted">{{ $t(formSubtitle) }}</small></p>
                            </div>
                        </div>
                    </div>
                </div>            
                <div class="col-auto ms-auto">
                    <button class="btn btn-primary btn-sm" @click="saveUser">
                        <LucideIcon icon="Save" :size="15" />
                        {{ $t("labelSave") }}
                    </button>
                </div>
            </div>
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <Form ref="formRef">
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
                            :items="profilesList"
                            :loading="isLoading"
                            v-model:selectedItems="selectedProfiles"
                        />
                        <SelectionListComponent
                            :id="'teams'"
                            :labelPanel="'labelTeams'"
                            :labelSelectedQuantity="'labelSelectedTeams'"
                            :labelSearch="'labelSearchTeams'"
                            :items="teamsList"
                            :loading="isLoading"
                            v-model:selectedItems="selectedTeams"
                        >
                            <template #footer>
                                <div class="border-top mt-2 pt-2">
                                    <button
                                        type="button"
                                        class="btn btn-sm btn-outline-secondary fw-semibold"
                                        @click="openTeamSection"
                                    >
                                        + {{ $t("management.teams.createBtn") }}
                                    </button>
                                </div>
                            </template>
                        </SelectionListComponent>
                    </Form>
                </div>
                <div v-if="showTeams" class="main-div shadow-sm mt-2">
                    <Form @submit="createTeam" ref="formRefTeam">
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="teamName" class="form-label fw-semibold mb-0">{{ $t("labelTeamName") }}</label>
                                <Field
                                    type="text"
                                    class="form-control form-control-sm"
                                    id="teamName"
                                    ref="teamNameInput"
                                    autocomplete="off"
                                    name="teamName"
                                    v-model="teamData.name"
                                    :placeholder="$t('labelTypeTeamName')"
                                    :rules="'required|min:3|max:100'"
                                />
                                <ErrorMessage name="teamName" class="invalid-feedback d-block" />
                            </div>
                        </div>
                        <div class="col-auto ms-auto">
                            <button class="btn btn-primary btn-sm">
                                <LucideIcon icon="Save" :size="15" />
                                {{ $t("labelSave") }}
                            </button>
                        </div>
                    </Form>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import api from "@/services/api";
    import { Form, Field, ErrorMessage } from "vee-validate";
    import SelectionListComponent from "@/components/global/SelectionListComponent.vue";
    import PasswordInputComponent from "@/components/global/PasswordInputComponent.vue";
    import UserService from "@/services/users/UserService";
    import ErrorCode from "@/constants/Errorcode";

    export default {
        name: "UserForm",
        components: {
            Form,
            Field,
            ErrorMessage,
            SelectionListComponent,
            PasswordInputComponent,
        },
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
            email: {
                type: String,
                required: false,
                default: null,
            },
        },
        data() {
            return {
                isLoading: true,
                userData: {
                    id: null,
                    name: "",
                    email: "",
                    teams: [],
                    profiles: [],
                    password: "",
                },
                teamData: {},
                selectedTeams: [],
                selectedProfiles: [],
                searchTeams: "",
                searchProfiles: "",
                teamsList: [],
                profilesList: [],
                showPassword: false,
                showConfirmedPassword: false,
                showTeams: false,
            };
        },
        computed: {
            formTitle() {
                return this.isEdit ? "management.users.editTitle" : "management.users.createTitle";
            },
            formSubtitle() {
                return this.isEdit ? "management.users.editSubtitle" : "management.users.createSubtitle";
            },
            filteredTeams() {
                if (!this.searchTeams) {
                    return this.teams;
                }
                return this.teams.filter((team) => team.name.toLowerCase().includes(this.searchTeams.toLowerCase()));
            },
            passwordRules() {
                return {
                    required: this.isEdit,
                    custom_password: true,
                    min: 6,
                    max: 50,
                };
            },
            confirmedPasswordRules() {
                return {
                    required: this.isEdit,
                    confirmed: "userPassword",
                    min: 6,
                    max: 50,
                };
            },
        },
        mounted() {
            this.getTeams();
            this.getProfiles();
            this.setupEdit();
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

                this.isLoading = true;
                api.post("User/IsEmailInUse", paramsReq)
                    .then((response) => {
                        if (response && response.data && response.data === true) {
                            this.$refs.formRef.setFieldError("userEmail", this.$t("management.users.emailDuplicated"));

                        } else {
                            this.$refs.formRef.setFieldError("userEmail", "");
                        }
                    })
                    .catch((e) => {
                        this.$notify({
                            title: 'management.users.title',
                            message: "management.users.invalid",
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            getTeams() {
                var paramsReq = {
                    search: "",
                    pageSize: 0,
                    page: 1,
                    isAscending: this.isAscending,
                };

                this.isLoading = true;
                api.get("/Team/Paged", { params: paramsReq })
                    .then((response) => {
                        this.teamsList = response.data.content;
                    })
                    .catch((e) => {
                        console.log(e);
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            getProfiles() {
                var paramsReq = {
                    search: "",
                    pageSize: 0,
                    page: 1,
                    isAscending: this.isAscending,
                };

                this.isLoading = true;
                api.get("/Profile/Paged", { params: paramsReq })
                    .then((response) => {
                        this.profilesList = response.data.content;
                    })
                    .catch((e) => {
                        console.log(e);
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            selectAll() {
                this.selectedTeams = this.filteredTeams.map((user) => user.id);
            },
            clearSelection() {
                this.selectedTeams = [];
            },
            returnToTable() {
                this.$router.push({
                    name: "Management",
                    query: { tab: "users" },
                });
            },
            openTeamSection() {
                this.showTeams = !this.showTeams;
            },
            closeTeamSection() {
                this.showTeams = false;
                this.teamData.name = "";
            },
            saveUser() {
                let response;
                if (!this.isEdit) {
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
                response.then((response) => {
                        this.returnToTable();
                        this.$notify({
                            title: "management.users.title",
                            message: "management.users.saveSuccess",
                            variant: "success",
                            icon: "CircleX",
                        });
                    })
                    .catch((e) => {
                        this.$notify({
                            title: "management.users.title",
                            message: "management.users.saveError",
                            variant: "danger",
                            icon: "CircleX",
                        });
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
            setupEdit() {
                if(!this.isEdit) return;
                UserService.getUserByEmail(this.email)
                    .then((response) => {
                        if(response.error !== undefined) {
                            this.returnToTable();
                            return this.$notify({
                                title: 'management.users.title',
                                message: 'management.users.invalid',
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        this.userData = response;
                        this.selectedProfiles = response.profiles.map(p => p.id);
                        this.selectedTeams = response.teams.map(t => t.id);
                    });
            },
            createTeam() {
                const team = {
                    name: this.teamData.name,
                };
                api.post("Team", team).then(() => {
                        this.$notify({
                            title: 'management.teams.title',
                            message: 'management.teams.saveSuccess',
                            variant: 'success',
                            icon: 'CircleCheckBig',
                        });
                        this.closeTeamSection();
                        this.getTeams();
                    })
                    .catch((err) => {
                        const errorCode = err?.response?.data?.errorCode;
                        let errorMessage = "management.teams.invalid";
                        if (errorCode && errorCode === ErrorCode.Duplicated) {
                            this.$refs.formRef.setFieldError("teamName", this.$t("management.teams.duplicated"));
                            errorMessage = "management.teams.duplicated";
                        } 
                        this.$notify({
                            title: 'management.teams.title',
                            message: errorMessage,
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    });
            },
        },
    };
</script>

<style scoped>
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