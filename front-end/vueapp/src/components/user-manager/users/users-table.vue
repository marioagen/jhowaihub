<template>
    <div>
        <table-component modalName="labelUsers"
                         emptyMessage="labelNoUsersWasFound"
                         :data="table.data"
                         :columns="table.columns"
                         :isLoading="table.isLoading"
                         :pagination="table.pagination"
                          @change-page="changePage">
            <template #cell-name="{ data }">
                <div v-if="!loading" class="p-1">
                    <div class="d-flex">
                        <label class="form-check-label d-flex align-items-center w-100">
                            <div class="rounded-circle d-flex align-items-center justify-content-center btn-primary fw-bold me-3 initials">
                                {{ getInitials(data.row.name) }}
                            </div>
                            <div>
                                <div class="fw-semibold">{{ data.row.name }}</div>
                                <div class="text-muted small">{{ data.row.email }}</div>
                            </div>
                        </label>
                    </div>
                </div>
            </template>
            <template #cell-teams="{ data }">
                <div v-if="data.row.teams.length > 0">
                    <div v-for="team in data.row.teams">
                        <span class="badge">{{ team.name }}</span>
                    </div>
                </div>
                <div v-else>
                        <span>-</span>
                </div>
            </template>
            <template #cell-actions="{ data }">
                <div class="dropdown column-align"> 
                    <a class="btn p-0 border-0" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                        <i class="fas fa-ellipsis-v"></i>
                    </a>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><a class="dropdown-item" @click="editUser(data.row)">{{$t('labelEdit')}}</a></li>
                        <li><a class="dropdown-item" @click="confirmationDialog(data.row)">{{$t('labelDelete')}}</a></li>
                    </ul>
                </div>
            </template>
        </table-component>
        <modal-user v-if="modalUserShow" @userCreated="handleUserCreated" @close="closeModalUser" :userEditing="selectedUser" />
        <modal-alert v-if="modalAlertShow"
                     :type="'Confirm'"
                     :entity="selectedUser"
                     :alertTitle="$t('labelYouAreAboutToDeleteTeam')"
                     :alertMessage="$t('labelThisActionCannotBeUndone')"
                     :okLabel="$t('labelConfirm')"
                     :cancelLabel="$t('labelCancel')"
                     @open="deleteUser"
                     @close="closeModal" />
    </div>
</template>
<script>
    import api from "@/services/api";
    import dates from "@/helpers/Dates";
    import TableComponent from "@/components/global/table-component.vue";
    import ModalUser from '@/components/user-manager/users/modals/new-user.vue';
    import ModalAlert from '@/components/common/modal-alert';
    import UserService from "@/services/users/UserService";
    export default {
        name: "UsersTable",
        components: {
            TableComponent,
            ModalUser,
            ModalAlert
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "labelUser" },
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
        }),
        methods: {
            getUsers: function (obj) {
                this.table.isLoading = true;
                this.searching = false;
                this.dataDocument = [];
                this.listIds = [];
                var paramsReq = {
                    search: obj.search.trim() ?obj.search.trim() : '',
                    pageSize: this.selectedOption,
                    page: obj.page,
                    isAscending: this.isAscending,
                    colType: this.colType,
                }
                let self = this;
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
            deleteUser() {
                let userId = this.selectedUser.id;
                UserService.deleteUsersById(userId)
                    .then((status) => {
                        if (status) {
                            this.closeModal();
                            this.getUsers({ search: '', page: 1, type: null });
                        }
                    })
            },
            filterList(input) {
                this.searchInput = input;
                this.getUsers({ search: input, page: this.queryPage, type: null });
            },
            getInitials(name) {
                if (!name) return '';
                const parts = name.trim().split(' ');
                if (parts.length === 1) {
                    const n = parts[0];
                    return (n[0] || '').toUpperCase() + (n[n.length - 1] || '').toUpperCase();
                }
                const first = parts[0][0] || '';
                const last = parts[parts.length - 1].slice(-1) || '';
                return (first + last).toUpperCase();
            },
            handleUserCreated: function () {
                this.getUsers({ search: '', page: this.queryPage, type: null });
                this.closeModalUser();
            },
            openModalUser: function () {
                this.modalUserShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModalUser: function () {
                this.modalUserShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            confirmationDialog(user) {
                this.selectedUser = user;
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal() {
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            changePage(page) {
                this.getUsers({ search: '', page: page, type: null });
            }

        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getUsers({ search: '', page: this.queryPage, type: null });
        },
    }
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

    .column-align
    {
        justify-content: center;
        display: flex;
    }
</style>