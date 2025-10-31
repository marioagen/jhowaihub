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
                    <button class="btn btn-primary btn-sm" @click="save">
                        <LucideIcon icon="Save" :size="15" />
                        {{ $t("labelSave") }}
                    </button>
                </div>
            </div>
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <Form @submit="save" ref="formRef">
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
                                :id="'users'"
                                :labelPanel="'labelTeamMembers'"
                                :labelSelectedQuantity="'labelSelectedUsers'"
                                :labelSearch="'labelSearchUsers'"
                                :items="filteredUsers"
                                :loading="isLoading"
                                :type="'user-list'"
                                v-model:selectedItems="selectedUsers"
                                ref="SelectionListComponent"
                            >
                                <template #footer>
                                    <div class="border-top mt-2 pt-2">
                                        <button
                                            type="button"
                                            class="btn btn-sm btn-outline-secondary fw-semibold"
                                            @click="showUserSection"
                                        >
                                            <LucideIcon :icon="'UserPlus'" :size="16" />
                                            {{ $t("management.users.createBtn") }}
                                        </button>
                                    </div>
                                </template>
                            </SelectionListComponent>
                        </div>
                    </Form>
                </div>
                <div v-if="showUsers" class="main-div shadow-sm mt-2">
                    <Form @submit="createUser" ref="formRef">
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
                        <div class="col-auto ms-auto">
                            <button class="btn btn-primary btn-sm mt-2">
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
    import { Form, Field, ErrorMessage } from "vee-validate";
    import api from "@/services/api";
    import SelectionListComponent from "@/components/global/SelectionListComponent.vue";
    import PasswordInputComponent from "@/components/global/PasswordInputComponent.vue";
    import ErrorCode from "@/constants/Errorcode";
    import TeamsService from "@/services/teams/TeamsService";

    export default {
        name: "TeamForm",
        components: {
            Form,
            Field,
            ErrorMessage,
            PasswordInputComponent,
            SelectionListComponent,
        },
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
            id: {
                type: Number,
                required: false,
                default: null,
            }
        },
        data() {
            return {
                isLoading: true,
                teamData: {
                    id: 0,
                    name: "",
                    users: [],
                },
                userData: {},
                selectedUsers: [],
                selectedProfiles: [],
                searchTerm: "",
                usersList: [],
                showUsers: false,
            };
        },
        computed: {
            formTitle() {
                return this.isEdit ? "management.teams.editTitle" : "management.teams.createTitle";
            },
            formSubtitle() {
                return this.isEdit ? "management.teams.editSubtitle" : "management.teams.createSubtitle";
            },
            filteredUsers() {
                if (!this.searchTerm) {
                    return this.usersList;
                }
                return this.usersList.filter(
                    (user) =>
                        user.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                        user.email.toLowerCase().includes(this.searchTerm.toLowerCase())
                );
            },
        },
        mounted() {
            this.getUsers();
            this.getProfiles();
            this.setupEdit();
        },
        methods: {
            returnToTable() {
                this.$router.push({
                    name: "Management",
                    query: { tab: "teams" },
                });
            },
            getUsers() {
                var paramsReq = {
                    search: "",
                    pageSize: 0,
                    page: 1,
                    isAscending: this.isAscending,
                };
                this.isLoading = true;
                api.get("/User/Paged", { params: paramsReq })
                    .then((response) => {
                        this.usersList = response.data.content;
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
                    .then(({ data }) => {
                        this.profilesList = data.content;
                    })
                    .catch((e) => {
                        console.log(e);
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            selectAll() {
                this.selectedUsers = this.filteredUsers.map((user) => user.id);
            },
            clearSelection() {
                this.selectedUsers = [];
            },
            openNewUserSection() {
                this.showUsers = !this.showUsers;
            },
            save() {
                const team = {
                    id: this.teamData.id,
                    name: this.teamData.name,
                    userIds: this.selectedUsers,
                    profileIds: this.selectedProfiles,
                };
                
                const request = team.id === 0 ? api.post("Team", team) : api.put("Team", team);
                request.then(() => {
                        this.$notify({
                            title: 'management.teams.title',
                            message: 'management.teams.saveSuccess',
                            variant: 'success',
                            icon: 'CircleCheckBig',
                        });
                        this.returnToTable();
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
            resetForm() {
                this.teamData.id = 0;
                this.teamData.name = "";
                this.selectedUsers = [];
                this.searchTerm = "";
            },
            setupEdit() {
                if(!this.isEdit) return;
                TeamsService.getTeamById(this.id)
                    .then((response) => {
                        this.teamData = response;
                        this.selectedUsers = response.users.map((u) => u.id);
                        this.selectedProfiles = response.profiles.map(p => p.id);
                    });
            },
            showUserSection() {
                this.showUsers = !this.showUsers;
            },
            closeUserSection() {
                this.userData = {};
                this.showUserSection();
            },
            createUser() {
                const user = {
                    name: this.userData.name,
                    email: this.userData.email,
                    password: this.userData.password,
                    teamIds: [],
                };

                api.post("User", user).then(() => {
                        this.$notify({
                            title: 'management.users.title',
                            message: 'management.users.saveSuccess',
                            variant: 'success',
                            icon: 'CircleCheckBig',
                        });
                        this.getUsers();
                        this.closeUserSection();
                    })
                    .catch((err) => {
                        const errorCode = err?.response?.data?.errorCode;
                        let errorMessage = "management.teams.invalid";
                        if (errorCode && errorCode === ErrorCode.Duplicated) {
                            this.$refs.formRef.setFieldError("teamName", this.$t("management.users.duplicated"));
                            errorMessage = "management.users.duplicated";
                        } 
                        this.$notify({
                            title: 'management.users.title',
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