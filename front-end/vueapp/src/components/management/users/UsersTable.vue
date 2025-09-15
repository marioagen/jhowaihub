<template>
    <div>
        <TableComponent
            modalName="labelUsers"
            emptyMessage="labelNoUsersWasFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            :hasSelection="false"
            @change-page="changePage"
        >
            <template #cell-name="{ data }">
                <div class="p-1">
                     <div class="d-flex">
                        <label class="form-check-label d-flex align-items-center w-100">
                            <AvatarComponent :name="data.row.name" />
                            <div>
                                <div class="fw-semibold">{{ data.row.name }}</div>
                                <div class="text-muted small">{{ data.row.email }}</div>
                            </div>
                        </label>
                    </div>
                </div>
            </template>
            <template #cell-profiles="{ data }">
                <div v-if="data.row.profiles.length > 0">
                    <BadgeComponent
                        v-for="profile in data.row.profiles"
                        :key="profile.id"
                        :text="profile.name"
                        class="ms-2"
                        variant="primary"
                        @setClick="filterByProfile(profile)"
                    />
                </div>
                <span v-else>-</span>
            </template>
            <template #cell-teams="{ data }">
                <div v-if="data.row.teams.length > 0">
                    <BadgeOutlinedComponent
                        v-for="team in data.row.teams"
                        :key="team.id"
                        :text="team.name"
                        class="ms-2"
                        variant="primary"
                        @setClick="filterByTeam(team)"
                    />
                </div>
                <span v-else>-</span>
            </template>
            <template #cell-actions="{ data }">
                <div class="dropdown position-static">
                    <a class="btn p-0 border-0" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                        <LucideIcon icon="Ellipsis" />
                    </a>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li>
                            <a class="dropdown-item d-flex align-items-center gap-2" @click="editUser(data.row)">
                                <LucideIcon icon="SquarePen" />
                                {{ $t("labelEdit") }}
                            </a>
                        </li>
                        <li>
                            <a
                                class="dropdown-item d-flex align-items-center gap-2"
                                @click="openConfirmation(data.row)"
                            >
                                <LucideIcon icon="Trash2" />
                                {{ $t("labelDelete") }}
                            </a>
                        </li>
                    </ul>
                </div>
            </template>
        </TableComponent>
    </div>
    <modal-user
        v-if="modalUserShow"
        :userEditing="selectedUser"
        @userCreated="handleUserCreated"
        @close="closeModalUser"
    />
    <ConfirmModal
        id="deleteConfirm"
        title="labelYouAreAboutToDeleteUser"
        message="labelThisActionCannotBeUndone"
        cancelText="labelCancel"
        confirmText="labelConfirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteUser"
    />
</template>

<script>
    import AvatarComponent from "@/components/global/AvatarComponent.vue";
    import UserService from "@/services/users/UserService";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ModalUser from "@/components/management/users/modals/UserModal.vue";
    import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent.vue";
    import BadgeComponent from "@/components/global/BadgeComponent.vue";

    export default {
        name: "UsersTable",
        emits: ["setFilter"],
        components: {
            BadgeOutlinedComponent,
            AvatarComponent,
            BadgeComponent,
            TableComponent,
            ConfirmModal,
            ModalUser,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "name", label: "labelUser" },
                    { key: "profiles", label: "labelProfiles" },
                    { key: "teams", label: "labelTeams" },
                    { key: "actions", label: "labelAction" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 100,
                    itemsPerPage: 10,
                    totalItems: 2000,
                },
            },
            queryPage: 1,
            searchInput: "",
            selectedOption: 10,
            isAscending: false,
            colType: 2,
            modalUserShow: false,
            modalAlertShow: false,
            selectedUser: {},
            isDeleting: false,
        }),
        methods: {
            getUsers(obj) {
                this.table.isLoading = true;
                this.searching = false;
                this.dataDocument = [];
                this.listIds = [];
                var paramsReq = {
                    search: obj.search.trim() ? obj.search.trim() : "",
                    pageSize: this.selectedOption,
                    page: obj.page,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };

                UserService.getUsers(paramsReq)
                    .then((response) => {
                        const content = response?.content || [];
                        const pagination = response?.pagination || {};

                        this.table.data = content;
                        this.table.pagination = pagination;
                    })
                    .finally(() => {
                        if (obj.type === "search") this.searching = true;
                        this.table.isLoading = false;
                    });
            },
            editUser(user) {
                this.selectedUser = user;
                this.openModalUser();
            },
            openConfirmation(user) {
                this.selectedUser = user;
                this.$refs.DeleteDialog.open();
            },
            deleteUser() {
                this.isDeleting = true;
                let userId = this.selectedUser.id;
                UserService.deleteUsersById(userId)
                    .then((status) => {
                        if (status) {
                            this.$refs.DeleteDialog.close();
                            this.getUsers({ search: "", page: 1, type: null });
                        }
                    })
                    .finally(() => {
                        this.isDeleting = false;
                    });
            },
            filterList(input) {
                this.searchInput = input;
                this.getUsers({ search: input, page: this.queryPage, type: null });
            },
            handleUserCreated: function () {
                this.getUsers({ search: "", page: this.queryPage, type: null });
                this.closeModalUser();
            },
            openModalUser: function () {
                this.modalUserShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal() {
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            closeModalUser: function () {
                this.modalUserShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            changePage(page) {
                this.getUsers({ search: "", page: page, type: null });
            },
            filterByTeam(team) {
                this.$emit("setFilter", team.name);
            },
            filterByProfile(profile) {
                this.$emit("setFilter", profile.name);
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getUsers({ search: "", page: this.queryPage, type: null });
        },
    };
</script>

<style scoped>
    .badge {
        display: inline-block;
        background-color: #e0ecff;
        color: #0057d8;
        padding: 4px 10px;
        border-radius: 6px;
        font-size: 12px;
        margin-right: 4px;
    }

    .initials {
        width: 30px;
        height: 30px;
    }
</style>
