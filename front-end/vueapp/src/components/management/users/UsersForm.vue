<template>
    <main>
        <div class="container-fluid scroll-area mx-4 mt-4">
            <Form
                ref="formRef"
                v-slot="{ meta }"
                autocomplete="off"
            >
                <div class="row align-items-center">
                    <div class="col-6">
                        <div class="row">
                            <div class="col-1">
                                <button
                                    type="button"
                                    class="btn btn-outline-primary btn-table btn-sm table-btn"
                                    @click="returnToTable"
                                >
                                    <LucideIcon icon="ArrowLeft" />
                                </button>
                            </div>
                            <div class="col-10">
                                <div>
                                    <h5 class="mb-0 fw-bold">
                                        {{ $t(formTitle) }}
                                    </h5>
                                    <p>
                                        <small class="text-muted">
                                            {{ $t(formSubtitle) }}
                                        </small>
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-auto ms-auto">
                        <button
                            type="button"
                            class="btn btn-primary btn-sm"
                            @click="saveUser"
                            :disabled="!meta.valid"
                        >
                            <LucideIcon
                                icon="Save"
                                :size="15"
                            />
                            {{ $t("common.save") }}
                        </button>
                    </div>
                </div>
                <div class="row mt-1">
                    <div class="main-div shadow-sm">
                        <div class="row">
                            <div class="col-6">
                                <div class="mb-3">
                                    <label
                                        for="userName"
                                        class="form-label fw-semibold mb-0"
                                    >
                                        {{ $t("common.name") }}
                                    </label>
                                    <Field
                                        type="text"
                                        class="form-control form-control-sm"
                                        id="userName"
                                        ref="userNameInput"
                                        autocomplete="new-username"
                                        name="userName"
                                        :rules="'required|min:3|max:150'"
                                        v-model="userData.name"
                                        :placeholder="$t('management.users.typeUserName')"
                                    />
                                    <ErrorMessage
                                        name="userName"
                                        class="invalid-feedback d-block"
                                    />
                                </div>
                            </div>
                            <div class="col-6">
                                <div class="mb-3">
                                    <label
                                        for="userEmail"
                                        class="form-label fw-semibold mb-0"
                                    >
                                        {{ $t("management.users.email") }}
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
                                        :placeholder="$t('management.users.typeUserEmail')"
                                        @blur="validateEmailBackend"
                                    />
                                    <ErrorMessage
                                        name="userEmail"
                                        class="invalid-feedback d-block"
                                    />
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-6">
                                <label
                                    for="userPassword"
                                    class="form-label fw-semibold mb-0"
                                >
                                    {{ $t("management.users.password") }}
                                </label>
                                <PasswordInputComponent
                                    :placeholder="$t('management.users.typePassword')"
                                    :rules="passwordRules"
                                    name="userPassword"
                                    autocomplete="new-password"
                                    v-model="userData.password"
                                />
                            </div>
                            <div class="col-6">
                                <label
                                    for="userConfirmedPassword"
                                    class="form-label fw-semibold mb-0"
                                >
                                    {{ $t("management.users.confirmedPassword") }}
                                </label>
                                <PasswordInputComponent
                                    :placeholder="$t('management.users.typeConfirmedPassword')"
                                    :rules="confirmedPasswordRules"
                                    name="userConfirmedPassword"
                                    autocomplete="new-password"
                                    v-model="userData.confirmedPassword"
                                />
                            </div>
                        </div>
                        <SelectionListComponent
                            :id="'teams'"
                            :labelPanel="'management.teams.title'"
                            :labelSelectedQuantity="'common.selected'"
                            :labelSearch="'management.teams.searchTeams'"
                            :items="teamsList"
                            :loading="isLoading"
                            :listHeight="'300px'"
                            v-model:selectedItems="selectedTeams"
                        >
                            <template #footer>
                                <div class="border-top mt-2 pt-2">
                                    <button
                                        type="button"
                                        class="btn btn-sm btn-outline-secondary fw-semibold"
                                        @click="openTeamSection"
                                    >
                                        +
                                        {{ $t("management.teams.createBtn") }}
                                    </button>
                                </div>
                            </template>
                        </SelectionListComponent>
                    </div>
                </div>
            </Form>
            <div
                v-if="showTeams"
                ref="teamPanel"
                class="row mt-2"
            >
                <div class="main-div shadow-sm">
                    <Form
                        @submit="createTeam"
                        ref="formRefTeam"
                        v-slot="{ meta }"
                    >
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <h6 class="fw-bold mb-0">{{ $t("management.teams.createTitle") }}</h6>
                            <button
                                type="button"
                                class="btn btn-sm btn-outline-secondary"
                                @click="closeTeamSection"
                            >
                                {{ $t("common.cancel") }}
                            </button>
                        </div>
                        <div class="mb-3">
                            <label
                                for="teamName"
                                class="form-label fw-semibold mb-0"
                            >
                                {{ $t("management.teams.teamName") }}
                                <span class="text-danger ms-1">*</span>
                            </label>
                            <Field
                                type="text"
                                class="form-control form-control-sm"
                                id="teamName"
                                ref="teamNameInput"
                                autocomplete="off"
                                name="teamName"
                                v-model="teamData.name"
                                :placeholder="$t('management.teams.typeTeamName')"
                                :rules="'required|min:3|max:100'"
                            />
                            <ErrorMessage
                                name="teamName"
                                class="invalid-feedback d-block"
                            />
                        </div>
                        <SelectionListComponent
                            :id="'team-profiles'"
                            :labelPanel="'management.profiles.profiles'"
                            :labelSelectedQuantity="'management.profiles.selectedProfiles'"
                            :labelSearch="'management.profiles.searchProfiles'"
                            :items="profilesList"
                            :loading="isLoadingProfiles"
                            chip-icon="ShieldCheck"
                            v-model:selectedItems="selectedProfiles"
                        />
                        <div class="d-flex justify-content-end gap-2 mt-3">
                            <button
                                type="button"
                                class="btn btn-sm btn-outline-secondary"
                                @click="closeTeamSection"
                            >
                                {{ $t("common.cancel") }}
                            </button>
                            <button
                                type="submit"
                                class="btn btn-primary btn-sm"
                                :disabled="!meta.valid"
                            >
                                <LucideIcon
                                    icon="Plus"
                                    :size="15"
                                />
                                {{ $t("management.teams.createBtn") }}
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
                isLoadingProfiles: false,
                userData: {
                    id: null,
                    name: "",
                    email: "",
                    teams: [],
                    profiles: [],
                    password: "",
                    confirmedPassword: "",
                },
                teamData: {},
                selectedTeams: [],
                selectedProfiles: [],
                searchTeams: "",
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
                return this.isEdit
                    ? "management.users.editSubtitle"
                    : "management.users.createSubtitle";
            },
            filteredTeams() {
                if (!this.searchTeams) {
                    return this.teams;
                }
                return this.teams.filter((team) =>
                    team.name.toLowerCase().includes(this.searchTeams.toLowerCase())
                );
            },
            passwordRules() {
                return {
                    required: !this.isEdit,
                    custom_password: true,
                    min: 6,
                    max: 50,
                };
            },
            confirmedPasswordRules() {
                return {
                    required: !this.isEdit,
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
            if (!this.isEdit) {
                this.clearAutofilledCreateFields();
                this.$nextTick(() => this.clearAutofilledCreateFields());
                setTimeout(() => this.clearAutofilledCreateFields(), 150);
                setTimeout(() => this.clearAutofilledCreateFields(), 700);
            }
        },
        methods: {
            clearAutofilledCreateFields() {
                if (this.isEdit) return;

                this.userData.name = "";
                this.userData.email = "";
                this.userData.password = "";
                this.userData.confirmedPassword = "";

                ["userName", "userEmail", "userPassword", "userConfirmedPassword"].forEach((id) => {
                    const input = document.getElementById(id);
                    if (!input) return;

                    input.value = "";
                });

                this.$refs.formRef?.resetForm({
                    values: {
                        userName: "",
                        userEmail: "",
                        userPassword: "",
                        userConfirmedPassword: "",
                    },
                });
            },
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
                            this.$refs.formRef.setFieldError(
                                "userEmail",
                                this.$t("management.users.errors.emailDuplicated")
                            );
                        } else {
                            this.$refs.formRef.setFieldError("userEmail", "");
                        }
                    })
                    .catch((e) => {
                        this.$notify({
                            title: "management.users.title",
                            message: "management.users.errors.invalid",
                            variant: "danger",
                            icon: "CircleX",
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
                api.get("/Team/Paged", {
                    params: paramsReq,
                })
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

                this.isLoadingProfiles = true;
                api.get("/Profile/Paged", { params: paramsReq })
                    .then(({ data }) => {
                        this.profilesList = data.content;
                    })
                    .catch((e) => {
                        console.log(e);
                    })
                    .finally(() => {
                        this.isLoadingProfiles = false;
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
                if (this.showTeams) {
                    this.$nextTick(() => {
                        this.$refs.teamPanel?.scrollIntoView({
                            behavior: "smooth",
                            block: "start",
                        });
                    });
                }
            },
            closeTeamSection() {
                this.showTeams = false;
                this.teamData.name = "";
                this.selectedProfiles = [];
                if (this.$refs.formRefTeam) {
                    this.$refs.formRefTeam.resetForm();
                }
            },
            saveUser() {
                let response;
                if (!this.isEdit) {
                    const user = {
                        name: this.userData.name,
                        email: this.userData.email,
                        password: this.userData.password,
                        teamIds: this.selectedTeams,
                    };
                    response = api.post("User", user);
                } else {
                    const userEdit = {
                        name: this.userData.name,
                        email: this.userData.email,
                        password: this.userData.password,
                        teamIds: this.selectedTeams,
                        id: this.userData.id,
                    };
                    response = api.put("User", userEdit);
                }
                response
                    .then((response) => {
                        this.returnToTable();
                        this.$notify({
                            title: "management.users.title",
                            message: "management.users.saveSuccess",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    })
                    .catch((e) => {
                        const errorCode = e?.response?.data?.errorCode;
                        const detail = (e?.response?.data?.detail || "").toLowerCase();
                        const isNameDuplicated =
                            errorCode === ErrorCode.Duplicated && detail.includes("name");
                        if (errorCode === ErrorCode.Duplicated && this.$refs.formRef) {
                            if (isNameDuplicated) {
                                this.$refs.formRef.setFieldError(
                                    "userName",
                                    this.$t("management.users.errors.duplicated")
                                );
                            } else {
                                this.$refs.formRef.setFieldError(
                                    "userEmail",
                                    this.$t("management.users.errors.emailDuplicated")
                                );
                            }
                        }
                        const notifyMessage = isNameDuplicated
                            ? "management.users.errors.duplicated"
                            : errorCode === ErrorCode.Duplicated
                              ? "management.users.errors.emailDuplicated"
                              : "management.users.errors.saveError";
                        this.$notify({
                            title: "management.users.title",
                            message: notifyMessage,
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
                if (!this.isEdit) return;
                UserService.getUserByEmail(this.email).then((response) => {
                    if (response.error !== undefined) {
                        this.returnToTable();
                        return this.$notify({
                            title: "management.users.title",
                            message: "management.users.invalid",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.userData = response;
                    this.selectedTeams = response.teams.map((t) => t.id);
                });
            },
            createTeam() {
                const team = {
                    name: this.teamData.name,
                    profileIds: this.selectedProfiles,
                };
                api.post("Team", team)
                    .then(() => {
                        this.$notify({
                            title: "management.teams.title",
                            message: "management.teams.saveSuccess",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                        this.closeTeamSection();
                        this.getTeams();
                    })
                    .catch((err) => {
                        const errorCode = err?.response?.data?.errorCode;
                        let notifyMessage = "management.teams.invalid";
                        if (errorCode && errorCode === ErrorCode.Duplicated) {
                            this.$refs.formRefTeam.setFieldError(
                                "teamName",
                                this.$t("management.teams.duplicated")
                            );
                            notifyMessage = "management.teams.duplicated";
                        }
                        this.$notify({
                            title: "management.teams.title",
                            message: notifyMessage,
                            variant: "danger",
                            icon: "CircleX",
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
    .border-top {
        border-top: 1px solid var(--color-border-form-control) !important;
    }
</style>
