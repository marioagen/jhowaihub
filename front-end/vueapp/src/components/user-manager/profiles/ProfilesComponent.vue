<template>
    <div class="mt-3 mb-3">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h6 class="mb-0 fw-bold">{{ $t("labelProfilePermissions") }}</h6>
                <p>
                    <small class="text-muted">{{ $t("labelProfilesMessage") }}</small>
                </p>
            </div>
            <button class="btn btn-primary btn-sm" @click="openModalProfile">+ {{ $t("labelNewProfile") }}</button>
        </div>
        <div class="card mb-3">
            <div class="card-body">
                <SearchComponent :entity="entitySearch" :resetInput="resetInputSearch" @search="filterList" />
            </div>
        </div>
        <profiles-table ref="ProfilesTable" />
    </div>
    <ProfilesModal :isEdit="false" :type="modalType" @reload="reloadTable" ref="ProfilesModal" />
</template>

<script>
    import ProfilesModal from "@/components/user-manager/profiles/modals/ProfilesModal.vue";
    import ProfilesTable from "@/components/user-manager/profiles/ProfilesTable.vue";
    import SearchComponent from "@/components/global/SearchComponent.vue";
    import editIcon from "@/assets/img/edit-outlined.svg";
    import deleteIcon from "@/assets/img/delete-outlined.svg";

    export default {
        name: "ProfilesComponent",
        data() {
            return {
                menuActions: {},
                loading: false,
                searching: false,
                modalAlertShow: false,
                modalEntity: {},
                search: "",
                queryPage: this.$route.query.page ? this.$route.query.page : 1,
                pagination: { currentPage: 0, pageCount: 0, rowCount: 0, listPage: 0 },
                listIds: [],
                entitySearch: {},
            };
        },
        watch: {
            searchInput: function (val) {
                this.searching = false;
            },
            "$store.state.userProfile.language": function () {
                this.setMenuActions();
                this.setEntitySearch();
            },
            "$store.state.userProfile.keyMongoAccess"(newValue) {
                if (newValue) {
                    this.$refs.ProfilesTable.getProfiles({ search: "", page: this.queryPage, type: null });
                }
            },
        },
        components: {
            ProfilesTable,
            SearchComponent,
            ProfilesModal,
        },
        methods: {
            setMenuActions: function () {
                this.menuActions = {
                    options: [
                        { label: this.$t("labelEdit"), value: "edit", icon: editIcon },
                        {
                            label: this.$t("labelDelete"),
                            value: "delete",
                            icon: deleteIcon,
                            color: "text-danger",
                        },
                    ],
                };
            },
            openModalProfile() {
                this.$refs.ProfilesModal.open();
            },
            filterList(obj) {
                this.$refs.ProfilesTable.filterList(obj.search);
            },
            setEntitySearch: function () {
                this.entitySearch = {
                    screen: "profile",
                    labelInput: this.$t("labelSearchProfiles"),
                    placeholderInput: this.$t("labelTypeProfileName"),
                };
            },
            reloadTable() {
                this.$refs.ProfilesModal.close();
                this.$refs.ProfilesTable.reload();
            },
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
