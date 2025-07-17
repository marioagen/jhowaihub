<template>
    <div class="scroll-area mt-3 mb-3">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h6 class="mb-0 fw-bold"> {{ $t('labelUsers') }}</h6>
                <p><small class="text-muted">{{ $t('labelUsersMessage') }}</small></p>
            </div>
            <button 
                class="btn btn-primary btn-sm" 
                @click="openModalUser"
            >
                + {{ $t('labelNewUser')}}
            </button>
        </div>
        <div class="card mb-3">
            <div class="card-body">
                <SearchComponent 
                    :entity="entitySearch" 
                    :resetInput="resetInputSearch" 
                    @search="filterList"
                    @clean="filterList"
                    ref="SearchComponent"
                />
            </div>
        </div>
        <UsersTable
            @setFilter="setFilter"
            ref="UserTable"
        />
    </div>
    <modal-alert v-if="modalAlertShow" :type="'Confirm'" :entity="modalEntity" :alertTitle="$t('labelYouAreAboutToDeleteTeam')" :alertMessage="$t('labelThisActionCannotBeUndone')" :okLabel="$t('labelConfirm')" :cancelLabel="$t('labelCancel')" @open="deleteItem" @close="closeModal" />
    <modal-user v-if="modalUserShow" @userCreated="handleUserCreated" @close="closeModalUser" :userEditing="userEditing"/>
</template>

<script>
import ModalAlert from '@/components/common/modal-alert';
import ModalUser from '@/components/user-manager/users/modals/UserModal.vue';
import paginationDivider from "@/utils/paginationDivider";
import UsersTable from "@/components/user-manager/users/UsersTable.vue";
import SearchBar from '@/components/common/search-bar';
import SearchComponent from '@/components/global/SearchComponent.vue';

export default {
    name: 'UsersManager',
    data() {
        return {
            menuActions: {},
            loading: false,
            searching: false,
            modalAlertShow: false,
            modalUserShow: false,
            modalEntity: {},
            search: '',
            entitySearch: {},
            queryPage: this.$route.query.page ? this.$route.query.page : 1,
            pagination: { currentPage: 0, pageCount: 0, rowCount: 0, listPage: 0 }, 
            teams: [],
            divider: new paginationDivider(),
            listIds: [],
            userEditing: {},
        };
    },
    watch: {
        searchInput: function (val) {
            this.searching = false;
        },
        '$store.state.userProfile.language': function () {
            this.setMenuActions();
            this.setEntitySearch();
        },
        '$store.state.userProfile.keyMongoAccess'(newValue) {
            if (newValue) {
                this.$refs.UserTable.getUsers({ search: '', page: this.queryPage, type: null })
            }
        },
    },
    components: {
        ModalAlert,
        ModalUser,
        UsersTable,
        SearchBar,
        SearchComponent
    },
    methods: {
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
                this.userEditing = {
                    id: item.id,
                    name: item.name,
                    email: item.email,
                    teams: item.teams
                };
                this.openModalUser();
                
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
        handleUserCreated: function () {
            this.$refs.UserTable.getUsers({ search: '', page: this.queryPage, type: null })
            this.closeModalUser();
        },
        openModalUser: function() {
            this.modalUserShow = true;
            document.getElementsByTagName("BODY")[0].children[1].className += " active";
        },
        closeModalUser: function() {
            console.log("oi");
            this.modalUserShow = false;
            document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
        },
        filterList(obj) {
            this.$refs.UserTable.filterList(obj.search);
        },
        setEntitySearch: function () {
            this.entitySearch = {
                screen: "user",
                labelInput: this.$t('labelSearchUsers'),
                placeholderInput: this.$t('labelTypeUserName'),
            };
        },
        setFilter(team) {
            this.$refs.SearchComponent.searchInput = team.name;
            this.$refs.UserTable.getUsers({ search: team.name, page: this.queryPage, type: null })
        }
    },
    created() {
        this.setMenuActions();
        this.setEntitySearch();
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
