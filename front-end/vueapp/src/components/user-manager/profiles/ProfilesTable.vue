<template>
    <div>
        <TableComponent modalName="labelProfile"
                        emptyMessage="labelNoProfilesWereFound"
                        :data="table.data"
                        :columns="table.columns"
                        :isLoading="table.isLoading"
                        :pagination="table.pagination"
                        :hasSelection="false"
                        @change-page="changePage">
            <template #cell-users="{ data }">
                <LucideIcon icon="UsersRound" size="15" />
                {{data.row.users.length}}
            </template>
            <template #cell-permissions="{ data }">
                {{data.row.permissions.length}} {{$t("labelShowingToTotal")}} {{this.permissionsCount}}
            </template>
            <template #cell-actions="{ data }">
                <div class="dropdown column-align">
                    <a class="btn p-0 border-0" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                        <LucideIcon icon="Ellipsis" />
                    </a>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li>
                            <a class="dropdown-item d-flex align-items-center gap-2"  @click="openEditModal(data.row)">
                                <LucideIcon icon="SquarePen" />
                                {{ $t("labelEdit") }}
                            </a>
                        </li>
                        <li>
                            <a class="dropdown-item d-flex align-items-center gap-2" @click="openConfirmation(data.row)">
                                <LucideIcon icon="Trash2" />
                                {{ $t("labelDelete") }}
                            </a>
                        </li>
                    </ul>
                </div>
            </template>
        </TableComponent>
    </div>
    <ProfilesModal :isEdit="true"
                   @reload="getProfiles({ search: '', page: this.queryPage, type: null })"
                   ref="ProfilesModal" />
    <ConfirmModal id="deleteConfirm"
                  title="labelYouAreAboutToDeleteProfile"
                  message="labelThisActionCannotBeUndone"
                  cancelText="labelCancel"
                  confirmText="labelConfirm"
                  confirmVariant="primary"
                  ref="DeleteDialog"
                  :isLoading="isDeleting"
                  @confirm="deleteProfile" />
</template>

<script>
    import dates from "@/helpers/Dates";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ProfilesModal from "@/components/user-manager/profiles/modals/ProfilesModal.vue";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import PermissionsService from "@/services/permissions/PermissionsService";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";

    export default {
        name: "RolesTable",
        components: {
            TableComponent,
            ProfilesModal,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "labelProfile" },
                    { key: "users", label: "labelUsers" },
                    { key: "permissions", label: "labelPermissions" },
                    { key: "actions", label: "labelAction" }
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 100,
                    itemsPerPage: 10,
                    totalItems: 2000,
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
                    .finally(() => {
                    });
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
            formatDate(date) {
                return dates.formatDate(date);
            },
            filterList(input) {
                this.searchInput = input;
                this.getProfiles({ search: input, page: this.queryPage, type: null });
            },
            handleTeamCreated: function () {
                this.getProfiles({ search: "", page: this.queryPage, type: null });
                this.closeModalTeam();
            },
            openEditModal(profile) {
                this.$refs.ProfilesModal.open(profile);
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
                            this.emitToast(
                                this.$t("labelDocumentTypeRemoveSuccess"),
                                "toast-success"
                            );
                        } else {
                            this.emitToast(
                                this.$t("labelDocumentTypeRemoveError"),
                                "toast-warning"
                            );
                        }
                    })
                    .finally(() => {
                        this.listIds = [];
                        this.table.selectedRows = [];
                        this.isDeleting = false;
                    });
            },
            emitToast(message, color) {
                this.$emit("toast", { message, color });
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