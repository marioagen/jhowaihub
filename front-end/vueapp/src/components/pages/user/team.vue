<template>
    <div class="scroll-area mt-3 mb-3">
        <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <h6 class="mb-0"> {{ $t('labelTeams') }}</h6>
            <p><small class="text-muted">{{ $t('labelTeamsMessage') }}</small></p>
        </div>
        <button class="btn btn-primary btn-sm" @click="openModalTeam">+ {{ $t('labelNewTeam')}}</button>
        </div>

        <div class="card mb-3">
            <div class="card-body">
                <input
                    v-model="search"
                    type="text"
                    class="form-control form-control-sm"
                    placeholder="Buscar times..."
                />
            </div>
        </div>

        <div class="card">
        <div class="card-body">
                <table class="table table-hover caption-top">
                    <caption>{{ $t('labelTeams') }} ({{ pagination.rowCount }})</caption>
                    <thead>
                        <tr>
                            <th class="content-left-middle">{{ $t('labelId') }} <img class="icon-order" src="@/assets/img/order-item.svg" @click="orderList(1)"/></th>
                            <th class="content-left-middle">{{ $t('labelTeamName') }} <img class="icon-order" src="@/assets/img/order-item.svg" @click="orderList(2)"/></th>
                            <th class="content-center-middle">{{ $t('labelMembers') }} <img class="icon-order" src="@/assets/img/order-item.svg" @click="orderList(3)"/></th>
                            <th class="content-right-middle">{{ $t('labelAction') }}</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="team in teams" :key="team.id">
                            <td class="content-left-middle">#{{ team.id }}</td>
                            <td class="content-left-middle">{{ team.name }}</td>
                            <td class="content-center-middle">
                                <img class="icon-pill" src="@/assets/img/users-tab.svg" width="16"/>
                                {{ team.users.length }}
                            </td>
                            <td class="content-right-middle">
                                <dropdown-menu :menuOptions="menuActions" @action="handleMenuAction($event, team)" />
                            </td>
                        </tr>
                    </tbody>
                </table>
                <div>
                    <pagination-container
                        :pagination="{currentPage: 1, pageCount: 2, rowCount: 20, listPage: 1}"
                        :dataList="teams"
                        :loading="false"
                    ></pagination-container>
                </div>
            </div>
        </div>
    </div>
    <modal-alert v-if="modalAlertShow" :type="'Confirm'" :entity="modalEntity" :alertTitle="$t('labelYouAreAboutToDeleteTeam')" :alertMessage="$t('labelThisActionCannotBeUndone')" :okLabel="$t('labelConfirm')" :cancelLabel="$t('labelCancel')" @open="deleteItem" @close="closeModal" />
    <modal-team v-if="modalTeamShow" @teamCreated="handleTeamCreated" @close="closeModalTeam" :teamEditing="teamEditing"/>
</template>

<script>
import api from "@/services/api";
import PaginationContainer from '@/components/common/pagination-container.vue';
import DropdownMenu from '@/components/common/dropdown-menu.vue';
import ModalAlert from '@/components/common/modal-alert';
import paginationDivider from "@/utils/paginationDivider";
import ModalTeam from '@/components/pages/user/modal-team.vue';

export default {
    name: 'TeamsManager',
    data() {
        return {
            menuActions: {},
            loading: false,
            searching: false,
            modalAlertShow: false,
            modalTeamShow: false,
            modalEntity: {},
            search: '',
            queryPage: this.$route.query.page ? this.$route.query.page : 1,
            pagination: { currentPage: 0, pageCount: 0, rowCount: 0, listPage: 0 }, 
            teams: [],
            divider: new paginationDivider(),
            listIds: [],
            teamEditing: {},
        };
    },
    watch: {
        searchInput: function (val) {
            this.searching = false;
        },
        '$store.state.userProfile.language': function () {
            this.setMenuActions();
        },
        '$store.state.userProfile.keyMongoAccess'(newValue) {
            if (newValue) {
                this.getList({ search: '', page: this.queryPage, type: null });
            }
        },
    },
    components: {
        PaginationContainer,
        DropdownMenu,
        ModalAlert,
        ModalTeam
    },
    methods: {
        getList: function (obj) {
            this.loading = true;
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
            api.get('/Team/Paged', { params: paramsReq })
                .then(function (response) {
                    self.teams = response.data.content;
                    self.pagination = {
                        currentPage: response.data.currentPage,
                        pageCount: response.data.pageCount,
                        rowCount: response.data.rowCount,
                        listPage: self.divider.calculatePageCount(response.data.pageCount, response.data.currentPage)
                    };
                    self.loading = false;
                    if (obj.type === "search") self.searching = true;
                }).catch(function (e) {
                    console.log(e);
                    self.loading = false;
                    if (obj.type === "search") self.searching = true;
                }).finally(function () {
                    console.log("Finished request.");
                });                    
        },
        orderList: function (col) {
            if (this.isAscending) {
                this.isAscending = false;
            }
            else {
                this.isAscending = true;
            }
            this.colType = col;
            this.getList({ search: '', page: this.queryPage, type: null })
        },
        setMenuActions: function () {
            this.menuActions = {
                options: [
                    { label: this.$t('labelEdit'), value: "edit", icon: require("@/assets/img/edit-outlined.svg") },
                    { label: this.$t('labelDelete'), value: "delete", icon: require("@/assets/img/delete-outlined.svg"), color: "text-danger" },
                ],
            };
        },
        handleMenuAction: function(option, item) {
            if (option.value === "edit") {
                this.teamEditing = {
                    id: item.id,
                    name: item.name,
                    users: item.users
                };
                this.openModalTeam();
                
            } else if (option.value === "delete") {
                this.listIds = [item.id];
                this.confirmationDialog(item);
            }
        },
        confirmationDialog: function (item) {
            this.modalEntity = item;
            this.modalAlertShow = true;
            document.getElementsByTagName("BODY")[0].children[1].className += " active";
        },
        closeModal: function () {
            this.modalAlertShow = false;
            document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
        },
        handleTeamCreated: function() {
            this.getList({ search: '', page: this.queryPage, type: null });
            this.closeModalTeam();
        },
        openModalTeam: function() {
            this.modalTeamShow = true;
            document.getElementsByTagName("BODY")[0].children[1].className += " active";
        },
        closeModalTeam: function() {
            this.modalTeamShow = false;
            document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
        },
        deleteItem: function () {
            self = this;
            api.delete('/Team/DeleteByIds', { data: this.listIds })
                .then(function (response) { 
                    self.closeModal();
                    self.getList({ search: '', page: 1, type: null });
                }).catch(function (e) { 
                    console.log(e);
                }).finally(function () { 
                    console.log("Finished request.");
                });
            this.listIds = [];
        },
    },
    created() {
        this.setMenuActions();
        if (this.$store.state.userProfile.keyMongoAccess) {
            this.getList({ search: '', page: this.queryPage, type: null });
        };
    },
};
</script>
<style scoped>
    .show {
        display: block;
    }
    .table td,
    .table th {
        vertical-align: middle;
    }
</style>
