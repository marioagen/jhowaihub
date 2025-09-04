<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("documents.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("documents.subtitle") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="redirectToNewUpload">
                        <LucideIcon icon="Plus" size="17" />
                        {{ $t("documents.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="d-flex align-items-center gap-2 flex-wrap">
                            <div class="flex-grow-1">
                                <SearchComponent
                                    :entity="entitySearch"
                                    :resetInput="resetInputSearch"
                                    @search="filterList"
                                    @clean="filterList"
                                />
                            </div>

                            <div class="w-auto">
                                <select
                                    v-model="selectedTeamId"
                                    :disabled="noTeams || loadingTeams"
                                    class="form-select form-select-sm w-auto"
                                    @change="onTeamChange"
                                >
                                    <option v-if="noTeams" :value="null" disabled>{{ $t("labelNoTeams") }}</option>
                                    <option v-else :value="0">{{ $t("labelAllTeams") }}</option>
                                    <option v-for="team in teamList" :key="team.id" :value="team.id">
                                        {{ team.name }}
                                    </option>
                                </select>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <DocumentsTable 
                ref="DocumentsTable"
            />
        </div>
    </main>
</template>

<script>
    import api from "@/services/api";
    import paginationDivider from "@/utils/paginationDivider";
    import GlobalEventService from "@/services/globalEventService.js";
    import SearchComponent from "@/components/global/SearchComponent.vue";
    import DocumentsTable from "@/components/documents/DocumentsTable.vue";

    export default {
        name: "DocumentsPage",
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
                sidebarData: "Documents",
                queryPage: this.$route.query.page ? this.$route.query.page : 1,
                searchInput: "",
                searching: false,
                dataDocument: [],
                loading: false,
                pagination: { currentPage: 0, pageCount: 0, rowCount: 0, listPage: 0 },
                modalAlertShow: false,
                modalWarningShow: false,
                modalEntity: {},
                isAscending: false,
                colType: 2,
                selectedOption: 10,
                divider: new paginationDivider(),
                listIds: [],
                showLoading: false,
                docDataEmbedding: {
                    Id: Number,
                    Embeddings_model_name: "",
                },
                isReprocessing: false,
                selectedTeamId: 0,
                teamList: [],
                loadingTeams: false,
                noTeams: false,
            };
        },
        components: {
            SearchComponent,
            DocumentsTable,
        },
        watch: {
            searchInput(val) {
                this.searching = false;
            },
            "$store.state.userProfile.language"() {
                this.setCrumbsData();
                this.setEntitySearch();
            },
            keyMongoAccess: {
                immediate: true,
                handler: async function (newValue) {
                    if (newValue) {
                        await this.loadTeams();
                        this.$refs.DocumentsTable.getDocuments({ search: "", page: this.queryPage, type: null });
                    }
                },
            },
        },
        methods: {
            setCrumbsData() {
                this.crumbsData = [
                    { crumb: this.$t("labelDocuments"), link: { to: "Documents" } },
                    { crumb: this.$t("labelListing"), link: { to: "Documents", queryPage: this.$route.query.page } },
                ];
            },
            setEntitySearch: function () {
                this.entitySearch = {
                    screen: "document",
                    labelInput: this.$t("labelSearchDocument"),
                    placeholderInput: this.$t("labelDocumentNameOrDescription"),
                    labelButton: "",
                };
            },
            async loadTeams() {
                this.loadingTeams = true;
                const paramsReq = {
                    search: "",
                    pageSize: 0,
                    page: 1,
                    isAscending: this.isAscending,
                };

                try {
                    const response = await api.get("/Team/PagedByUser", { params: paramsReq });
                    this.teamList = response.data.content;

                    if (this.teamList.length === 0) {
                        this.noTeams = true;
                        this.selectedTeamId = null;
                    } else {
                        this.noTeams = false;
                        this.selectedTeamId = 0;
                    }
                } catch (err) {
                    this.teamList = [];
                    this.noTeams = true;
                    this.selectedTeamId = null;
                } finally {
                    this.loadingTeams = false;
                }
            },
            onTeamChange() {
                this.getList({ page: 1, search: this.searchInput });
            },
            redirectToNewUpload: function (quiz) {
                this.$router.push({ name: "DocumentsUpload" });
            },
            filterList(obj) {
                this.searchInput = obj.search;
                this.$refs.DocumentsTable.getDocuments({ search: obj.search, page: this.queryPage, type: null });
            },
        },
        computed: {
            keyMongoAccess() {
                return this.$store.state.userProfile.keyMongoAccess;
            },
        },
        async created() {
            this.setCrumbsData();
            this.setEntitySearch();

            await this.loadTeams();
            GlobalEventService.on("all-uploads-complete", this.reloadList);
            GlobalEventService.on("refresh-once", this.reloadList);
            if (this.noTeams) {
                this.selectedTeamId = null;
            }
        },
        beforeUnmount() {
            GlobalEventService.off("all-uploads-complete", this.reloadList);
            GlobalEventService.off("refresh-once", this.reloadList);
        },
    };
</script>

<style scoped>
    .team-list {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
    }

    .team-badge {
        background-color: #f1f1f1;
        border: 1px solid #ccc;
        border-radius: 12px;
        padding: 4px 10px;
        font-size: 0.85rem;
        color: #333;
        white-space: nowrap;
    }

    .content-center {
        align-items: center;
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: center;
    }

    tbody {
        background-color: #fff !important;
    }

    .content-left-middle {
        text-align: left;
        vertical-align: middle;
    }

    .content-center-middle {
        text-align: center;
        vertical-align: middle;
    }

    .bg-success {
        background-color: #edfef2 !important;
        color: #0eaa42 !important;
        font-weight: inherit !important;
        padding: 8px 12px !important;
    }

    .navbar-container {
        padding-top: 0px;
        padding: 0;
    }

    .container-fluid {
        padding: 0 13px;
    }

    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }

    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }
</style>
