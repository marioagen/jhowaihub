<template>
    <div class="modal fade show" id="novoTimeModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="overlay" :class="{ active: showModalTeamUser }"></div>
                <div class="modal-header custom-header">
                    <h6 class="modal-title" id="novoTimeModalLabel">
                        {{ $t("labelNewTeam") }}
                        <small class="text-muted d-block text-sm">{{ $t("labelNewTeamMessage") }}</small>
                    </h6>
                    <button type="button" class="btn-close" @click="close"></button>
                </div>
                <Form @submit="saveTeam" ref="formRef">
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
                            :loading="loading"
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
                    <div class="modal-footer">
                        <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="close">
                            {{ $t("labelCancel") }}
                        </button>
                        <button type="submit" class="btn btn-primary btn-sm">
                            {{ $t("labelCreate") }}
                        </button>
                    </div>
                </Form>
            </div>
        </div>
    </div>

    <modal-team-user
        v-if="showModalTeamUser"
        :teamId="teamEditing.id"
        @close="closeModalTeamUser"
        @userCreated="userCreated"
    ></modal-team-user>

    <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
</template>

<script>
    import api from "@/services/api";
    import ModalTeamUser from "@/components/management/teams/modals/UserModal.vue";
    import ToastAlert from "@/components/common/toast-alert";
    import ErrorCode from "@/constants/Errorcode";
    import { Form, Field, ErrorMessage } from "vee-validate";
    import SelectionListComponent from "@/components/global/SelectionListComponent.vue";

    export default {
        name: "ModalTeam",
        components: {
            ModalTeamUser,
            ToastAlert,
            Form,
            Field,
            ErrorMessage,
            SelectionListComponent,
        },
        props: {
            teamEditing: {
                required: true,
                type: Object,
                default: {},
            },
        },
        data() {
            return {
                teamData: {
                    id: this.teamEditing.id ? this.teamEditing.id : 0,
                    name: this.teamEditing.name ? this.teamEditing.name : "",
                    users: this.teamEditing.users ? this.teamEditing.users : [],
                },
                selectedUsers: this.teamEditing.users ? this.teamEditing.users.map((u) => u.id) : [],
                searchTerm: "",
                users: [],
                loading: false,
                showModalTeamUser: false,
                toastShow: false,
                toastColor: "",
                toastMessage: "",
                myInterval: null,
            };
        },
        emits: ["close", "teamCreated"],
        computed: {
            filteredUsers() {
                if (!this.searchTerm) {
                    return this.users;
                }
                return this.users.filter(
                    (user) =>
                        user.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                        user.email.toLowerCase().includes(this.searchTerm.toLowerCase())
                );
            },
        },
        mounted() {
            this.loadUsers();
        },
        methods: {
            loadUsers() {
                var paramsReq = {
                    search: "",
                    pageSize: 0,
                    page: 1,
                    isAscending: this.isAscending,
                };
                api.get("/User/Paged", { params: paramsReq })
                    .then((response) => {
                        this.users = response.data.content;
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
            selectAll() {
                this.selectedUsers = this.filteredUsers.map((user) => user.id);
            },
            clearSelection() {
                this.selectedUsers = [];
            },
            addNewUser() {
                this.showModalTeamUser = true;
            },
            saveTeam(e) {
                const team = {
                    id: this.teamData.id,
                    name: this.teamData.name,
                    userIds: this.selectedUsers,
                };
                const request = team.id === 0 ? api.post("Team", team) : api.put("Team", team);
                request
                    .then(() => {
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
            close: function () {
                this.$emit("close");
            },
            getInitials(name) {
                if (!name) return "";
                const parts = name.trim().split(" ");
                if (parts.length === 1) {
                    const n = parts[0];
                    return (n[0] || "").toUpperCase() + (n[n.length - 1] || "").toUpperCase();
                }
                const first = parts[0][0] || "";
                const last = parts[parts.length - 1].slice(-1) || "";
                return (first + last).toUpperCase();
            },
            closeModalTeamUser() {
                this.showModalTeamUser = false;
                this.loadUsers();
            },
            userCreated() {
                this.loadUsers();
                this.closeModalTeamUser();
            },
            alertToast(msg, color) {
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;
                this.myInterval = setInterval(function () {
                    this.toastMessage = "";
                    this.toastColor = "";
                    this.toastShow = false;
                    clearInterval(this.myInterval);
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
