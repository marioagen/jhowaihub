<template>
    <div class="scroll-area mt-3 mb-3">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h6 class="mb-0 fw-bold">{{ $t("management.users.title") }}</h6>
                <p>
                    <small class="text-muted">{{ $t("management.users.subtitle") }}</small>
                </p>
            </div>
            <button 
                class="btn btn-primary btn-sm" 
                @click="redirectToForm"
            >
                <LucideIcon icon="Plus" />
                {{ $t("management.users.createBtn") }}
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
</template>

<script>
    import UsersTable from "@/components/management/users/UsersTable.vue";
    import SearchComponent from "@/components/global/SearchComponent.vue";

    export default {
        name: "UsersManager",
        data() {
            return {
                searching: false,
                search: "",
                entitySearch: {},
                queryPage: this.$route.query.page ? this.$route.query.page : 1,
                resetInputSearch: "",
            };
        },
        watch: {
            searchInput: function (val) {
                this.searching = false;
            },
            "$store.state.userProfile.keyMongoAccess"(newValue) {
                if (newValue) {
                    this.reloadTable({ search: "", page: this.queryPage, type: null });
                }
            },
        },
        components: {
            UsersTable,
            SearchComponent,
        },
        methods: {
            redirectToForm() {
                this.$router.push({
                    name: 'NewUser',
                });
            },
            reloadTable(params) {
                this.$refs.UserTable.getUsers(params);
            },
            filterList(obj) {
                this.$refs.UserTable.filterList(obj.search);
            },
            setFilter(searchValue) {
                this.$refs.SearchComponent.searchInput = searchValue;
                this.reloadTable({ search: searchValue, page: this.queryPage, type: null });
            },
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
