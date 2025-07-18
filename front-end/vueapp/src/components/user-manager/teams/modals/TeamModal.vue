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
                <div class="modal-body">
                    <form ref="formRef">
                        <div class="mb-3">
                            <label for="teamName" class="form-label">{{ $t("labelTeamName") }}</label>
                            <input
                                type="text"
                                class="form-control form-control-sm"
                                id="teamName"
                                ref="teamNameInput"
                                autocomplete="off"
                                name="teamName"
                                :rules="'required|min:2|max:50'"
                                v-model="teamData.name"
                                :placeholder="$t('labelTypeTeamName')"
                                @blur="nameError = teamData.name ? '' : $t('labelRequiredField')"
                                @input="nameError = ''"
                            />
                            <div v-if="nameError" class="invalid-feedback d-block">{{ nameError }}</div>
                        </div>
                        <div class="mb-3">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <label class="form-label mb-0">{{ $t("labelTeamMembers") }}</label>
                                <span class="text-muted">
                                    {{ selectedUsers.length }} {{ $t("labelSelectedWithO") }}
                                </span>
                            </div>

                            <div class="mb-3">
                                <div class="input-group">
                                    <span class="input-group-text"><i class="fas fa-search text-secondary"></i></span>
                                    <input
                                        type="text"
                                        class="form-control form-control-sm"
                                        :placeholder="$t('labelSearchUsers')"
                                        v-model="searchTerm"
                                    />
                                </div>
                            </div>
                            <div class="mb-3">
                                <button type="button" class="btn btn-outline-primary btn-sm me-2" @click="selectAll">
                                    <i class="bi bi-check-all"></i>
                                    {{ $t("labelSelectAll") }}
                                </button>
                                <button type="button" class="btn btn-outline-secondary btn-sm" @click="clearSelection">
                                    <i class="bi bi-x-circle"></i>
                                    {{ $t("labelClearSelection") }}
                                </button>
                            </div>

                            <div class="border rounded p-1 user-list">
                                <div v-if="loading" class="text-center">
                                    <div class="spinner-border text-primary" role="status">
                                        <span class="visually-hidden">{{ $t("labelLoading") }}</span>
                                    </div>
                                </div>
                                <div v-if="!loading" v-for="user in filteredUsers" :key="user.id" class="p-1">
                                    <div class="form-check d-flex align-items-center">
                                        <input
                                            class="form-check-input me-3"
                                            type="checkbox"
                                            :id="`user-${user.id}`"
                                            :value="user.id"
                                            v-model="selectedUsers"
                                        />
                                        <label
                                            class="form-check-label d-flex align-items-center w-100"
                                            :for="`user-${user.id}`"
                                        >
                                            <div
                                                class="rounded-circle d-flex align-items-center justify-content-center btn-primary fw-bold me-3 initials"
                                            >
                                                {{ getInitials(user.name) }}
                                            </div>
                                            <div>
                                                <div class="fw-semibold">{{ user.name }}</div>
                                                <div class="text-muted small">{{ user.email }}</div>
                                            </div>
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="mb-3">
                            <button type="button" class="btn btn-sm btn-outline-primary w-100" @click="addNewUser">
                                + {{ $t("labelNewUser") }}
                            </button>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" @click="close">
                        {{ $t("labelCancel") }}
                    </button>
                    <button type="button" class="btn btn-primary btn-sm" @click="saveTeam">
                        {{ $t("labelCreate") }}
                    </button>
                </div>
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
    import ModalTeamUser from "@/components/user-manager/teams/modals/UserModal.vue";
    import ToastAlert from "@/components/common/toast-alert";
    import ErrorCode from "@/constants/Errorcode";

    export default {
        name: "ModalTeam",
        components: {
            ModalTeamUser,
            ToastAlert,
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
                nameError: "",
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
            validateForm() {
                let valid = true;
                if (!this.teamData.name || this.teamData.name.length < 2) {
                    this.nameError = this.$t("labelRequiredField");
                    valid = false;
                }
                return valid;
            },
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
                e.preventDefault();
                if (!this.validateForm()) return;

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
                    })
                    .catch((err) => {
                        const errorCode = err?.response?.data?.errorCode;

                        if (errorCode && errorCode !== ErrorCode.DefaultError) {
                            if (errorCode === ErrorCode.Duplicated) {
                                this.nameError = this.$t("labelErrorTeamAlreadyExists");
                            } else {
                                this.alertToast(this.$t("labelUserError"), "toast-warning");
                            }
                        } else {
                            this.alertToast(this.$t("labelUserError"), "toast-warning");
                        }
                    })
                    .finally(() => {
                        this.close();
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

    .initials {
        width: 30px;
        height: 30px;
    }

    .user-list {
        max-height: 200px;
        min-height: 200px;
        overflow-y: auto;
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
