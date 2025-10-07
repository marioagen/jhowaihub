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
                                :id="'users'"
                                :labelPanel="'labelTeamMembers'"
                                :labelSelectedQuantity="'labelSelectedUsers'"
                                :labelSearch="'labelSearchUsers'"
                                :items="filteredUsers"
                                :loading="isLoading"
                                :type="'user-list'"
                                v-model:selectedItems="selectedUsers"
                            >
                                <template #footer>
                                    <div class="border-top mt-2 pt-2">
                                        <button
                                            type="button"
                                            class="btn btn-sm btn-outline-secondary fw-semibold"
                                            @click="addNewUser"
                                        >
                                            <LucideIcon :icon="'UserPlus'" :size="16" />
                                            {{ $t("labelNewUser") }}
                                        </button>
                                    </div>
                                </template>
                            </SelectionListComponent>
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

    export default {
        name: "UserForm",
        components: {
            Form,
            Field,
            ErrorMessage,
            SelectionListComponent,
            PasswordInputComponent,
            ModalTeamUser,
            ToastAlert,
            Form,
            Field,
            ErrorMessage,
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
                selectedUsers: this.teamEditing.users ? this.teamEditing.users.map((u) => u.id) : [],
                searchTerm: "",
                usersList: [],
                showTeams: false,
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
        },
        methods: {
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
            selectAll() {
                this.selectedUsers = this.filteredUsers.map((user) => user.id);
            },
            clearSelection() {
                this.selectedUsers = [];
            },
            openNewUserSection() {
            
            },
            save() {
                const team = {
                    id: this.teamData.id,
                    name: this.teamData.name,
                    userIds: this.selectedUsers,
                };
                const request = team.id === 0 ? api.post("Team", team) : api.put("Team", team);
                request.then(() => {
                        this.$emit("teamCreated", team);
                        this.resetForm();
                        this.close();
                    })
                    .catch((err) => {
                        const errorCode = err?.response?.data?.errorCode;

                        if (errorCode && errorCode !== ErrorCode.DefaultError) {
                            if (errorCode === ErrorCode.Duplicated) {
                                this.$refs.formRef.setFieldError("teamName", this.$t("labelErrorTeamAlreadyExists"));
                                this.alertToast(this.$t("labelTeamError"), "toast-warning");
                            } else {
                                this.alertToast(this.$t("labelTeamError"), "toast-warning");
                            }
                        } else {
                            this.alertToast(this.$t("labelTeamError"), "toast-warning");
                        }
                    });
            },
            resetForm() {
                this.teamData.id = 0;
                this.teamData.name = "";
                this.selectedUsers = [];
                this.searchTerm = "";
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