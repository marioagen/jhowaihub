<template>
    <div>
        <TableComponent
            modalName="management.profiles.index"
            emptyMessage="management.profiles.noProfilesWereFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            :hasSelection="false"
            @change-page="changePage"
        >
            <template #cell-users="{ data }">
                <LucideIcon icon="UsersRound" :size="15" />
                {{ data.row.users.length }}
            </template>
            <template #cell-permissions="{ data }">
                {{ data.row.permissions.length }} {{ $t("common.showingToTotal") }} {{ this.permissionsCount }}
            </template>
            <template #cell-actions="{ data }">
                <DropdownComponent>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="redirectToForm(data.row)">
                            <LucideIcon icon="SquarePen" />
                            {{ $t("common.edit") }}
                        </a>
                    </li>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="openConfirmation(data.row)">
                            <LucideIcon icon="Trash2" />
                            {{ $t("common.delete") }}
                        </a>
                    </li>
                </DropdownComponent>
            </template>
        </TableComponent>
    </div>
    <ConfirmModal
        id="deleteConfirm"
        title="management.profiles.youAreAboutToDeleteProfile"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteProfile"
    />
</template>

<script>
    import date from "@/helpers/date";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import PermissionsService from "@/services/permissions/PermissionsService";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import DropdownComponent from "@/components/global/DropdownComponent.vue";

    export default {
        name: "ProfilesTable",
        components: {
            DropdownComponent,
            TableComponent,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "common.id" },
                    { key: "name", label: "management.profiles.profile" },
                    { key: "users", label: "management.users.title" },
                    { key: "permissions", label: "management.profiles.permissions" },
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
            isDeleting: false,
            selectedProfile: {},
            queryPage: 1,
            searchInput: "",
            selectedOption: 10,
            isAscending: false,
            colType: 2,
            modalProfileShow: false,
            modalAlertShow: false,
            permissionsCount: 0,
        }),
        methods: {
            getProfiles(obj) {
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

                ProfilesService.getProfiles(paramsReq)
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
            getPermissions(obj) {
                PermissionsService.getPermissions()
                    .then((response) => {
                        const permissions = response.permissions;
                        this.permissionsCount = permissions.length;
                    })
                    .finally(() => {});
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getProfiles({ search: "", page: this.queryPage, type: null });
            },
            formatDate(str) {
                return date.formatDate(str);
            },
            filterList(input) {
                this.searchInput = input;
                this.getProfiles({ search: input, page: this.queryPage, type: null });
            },
            handleTeamCreated: function () {
                this.getProfiles({ search: "", page: this.queryPage, type: null });
                this.closeModalTeam();
            },
            redirectToForm(profile) {
                this.$router.push({
                    name: "EditProfile",
                    params: {
                        id: profile.id,
                    },
                });
            },
            openConfirmation(profile) {
                this.selectedProfile = [profile.id];
                this.$refs.DeleteDialog.open();
            },
            changePage(page) {
                this.getProfiles({ search: "", page: page, type: null });
            },
            deleteProfile() {
                this.isDeleting = true;
                ProfilesService.deleteProfileById(this.selectedProfile)
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getProfiles({ search: "", page: 1, type: null });
                            this.$notify({
                                title: "Profiles",
                                message: "management.profiles.deleteSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "Profiles",
                                message: "management.profiles.errors.deleteError",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.listIds = [];
                        this.table.selectedRows = [];
                        this.isDeleting = false;
                    });
            },
            reload() {
                this.getProfiles({ search: "", page: this.queryPage, type: null });
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getProfiles({ search: "", page: this.queryPage, type: null });
            this.getPermissions();
        },
    };
</script>

<style>
    .dropdown-toggle::after {
        display: none;
    }
</style>
