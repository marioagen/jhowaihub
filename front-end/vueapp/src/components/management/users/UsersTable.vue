<template>
    <div>
        <TableComponent
            modalName="management.users.title"
            emptyMessage="management.users.noUsersWasFound"
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
                        class="ms-2 mt-1"
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
                        class="ms-2 mt-1"
                        variant="primary"
                        @setClick="filterByTeam(team)"
                    />
                </div>
                <span v-else>-</span>
            </template>
            <template #cell-actions="{ data }">
                <ActionTableListComponent v-slot="{ actionClass }">
                    <a
                        :class="actionClass"
                        @click="redirectToForm(data.row)"
                        v-tooltip="$t('common.edit')"
                    >
                        <LucideIcon icon="SquarePen" />
                    </a>
                    <a
                        :class="actionClass"
                        class="text-danger"
                        @click="openConfirmation(data.row)"
                        v-tooltip="$t('common.delete')"
                    >
                        <LucideIcon icon="Trash2" />
                    </a>
                </ActionTableListComponent>
            </template>
        </TableComponent>
    </div>
    <ConfirmModal
        id="deleteConfirm"
        title="management.users.youAreAboutToDeleteUser"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
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
    import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent.vue";
    import BadgeComponent from "@/components/global/BadgeComponent.vue";
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";

    export default {
        name: "UsersTable",
        emits: ["setFilter"],
        components: {
            BadgeOutlinedComponent,
            AvatarComponent,
            BadgeComponent,
            TableComponent,
            ConfirmModal,
            ActionTableListComponent,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "name", label: "management.users.user" },
                    { key: "profiles", label: "management.profiles.profiles" },
                    { key: "teams", label: "management.teams.title" },
                    { key: "actions", label: "common.actions" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 0,
                    itemsPerPage: 10,
                    totalItems: 0,
                },
            },
            queryPage: 1,
            searchInput: "",
            selectedOption: 10,
            isAscending: true,
            colType: 2,
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
                            this.$notify({
                                title: "users.title",
                                message: "users.removeSuccess",
                                variant: "success",
                                icon: "CircleX",
                            });
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
            changePage(page) {
                this.getUsers({ search: "", page: page, type: null });
            },
            filterByTeam(team) {
                this.$emit("setFilter", team.name);
            },
            filterByProfile(profile) {
                this.$emit("setFilter", profile.name);
            },
            redirectToForm(user) {
                this.$router.push({
                    name: "EditUser",
                    params: {
                        email: user.email,
                    },
                });
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

    .link-action:hover {
        cursor: pointer;
    }
</style>
