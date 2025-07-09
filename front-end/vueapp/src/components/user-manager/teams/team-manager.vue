<template>
    <div class="scroll-area mt-3 mb-3">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h6 class="mb-0"> {{ $t('labelTeams') }}</h6>
                <p><small class="text-muted">{{ $t('labelTeamsMessage') }}</small></p>
            </div>
            <button 
                class="btn btn-primary btn-sm" 
                @click="openModalTeam"
            >
                + {{ $t('labelNewTeam')}}
            </button>
        </div>

        <div class="card mb-3">
            <div class="card-body">
                <input
                    v-model="search"
                    type="text"
                    class="form-control form-control-sm"
                    placeholder="Buscar times..."
                    @keydown.enter="filterList"
                />
            </div>
        </div>

        <teams-table 
            ref="TeamsTable"
        />
    </div>
    <modal-team v-if="modalTeamShow" @teamCreated="handleTeamCreated" @close="closeModalTeam" :teamEditing="teamEditing"/>
</template>

<script>
import ModalTeam from '@/components/user-manager/teams/modals/new-team.vue';
import paginationDivider from "@/utils/paginationDivider";
import TeamsTable from "@/components/user-manager/teams/teams-table.vue";

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
                this.$refs.TeamsTable.getTeams({ search: '', page: this.queryPage, type: null })
            }
        },
    },
    components: {
        ModalTeam,
        TeamsTable
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
        handleTeamCreated() {
            this.$refs.TeamsTable.getTeams({ search: '', page: this.queryPage, type: null })
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
        filterList() {
            this.$refs.TeamTable.filterList(this.search);
        },
    },
    created() {
        this.setMenuActions();
        // console.log(this.$store.state.userProfile.keyMongoAccess)
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
