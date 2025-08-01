<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("labelDocuments") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("labelDocumentsMessage") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="redirectToNewUpload">
                        <LucideIcon icon="Plus" size="17" />
                        {{ $t("labelNewDocument") }}
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

                <div class="row" v-if="loading">
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <tbody>
                                <tr class="tr-head-1">
                                    <td style="text-align: center">
                                        <i class="fas fa-sync-alt fa-spin text-secondary"></i>
                                        &nbsp;{{ $t("labelLoading") }}..
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="mb-3">
                    <button
                        type="button"
                        class="btn delete-custom d-flex align-items-center"
                        @click="confirmationDialog(item)"
                        v-if="this.listIds.length > 0"
                    >
                        <i class="fas fa-trash text-danger" style="font-size: 0.9em; margin-right: 8px"></i>
                        {{ $t("labelDelete") }}
                    </button>
                </div>
                <div class="row" v-if="dataDocument.length === 0 && !loading && (searching || !searching)">
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <tbody>
                                <tr class="tr-head-1">
                                    <td style="text-align: center">
                                        <i class="fas fa-exclamation-circle text-secondary"></i>
                                        &nbsp;{{ $t("labelNoDocumentsWereFound") }}.
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="row scroll-area" v-if="!loading && dataDocument.length > 0">
                    <div class="table-responsive">
                        <table class="table table-bordered-vertical table-striped">
                            <tbody>
                                <tr class="tr-head-1">
                                    <td class="content-left-middle">
                                        <div class="form-check">
                                            <input
                                                class="form-check-input"
                                                type="checkbox"
                                                value=""
                                                @click="checkAll($event)"
                                            />
                                        </div>
                                    </td>
                                    <td class="content-left-middle">{{ $t("labelDocumentName") }}</td>
                                    <td class="content-left-middle">
                                        {{ $t("labelDescription") }}
                                        <i
                                            id="1"
                                            class="fas fa-sort"
                                            @click="orderList(1)"
                                            style="cursor: pointer"
                                            :title="$t('labelOrder')"
                                        ></i>
                                    </td>
                                    <td class="content-left-middle">
                                        {{ $t("labelInclusionDate") }}
                                        <i
                                            id="2"
                                            class="fas fa-sort"
                                            @click="orderList(2)"
                                            style="cursor: pointer"
                                            :title="$t('labelOrder')"
                                        ></i>
                                    </td>
                                    <td class="content-left-middle">
                                        {{ $t("labelStatus") }}
                                        <i
                                            id="3"
                                            class="fas fa-sort"
                                            @click="orderList(3)"
                                            style="cursor: pointer"
                                            :title="$t('labelOrder')"
                                        ></i>
                                    </td>
                                    <td class="content-left-middle">
                                        {{ $t("labelTeams") }}
                                    </td>
                                    <td class="content-right-middle">{{ $t("labelAction") }}</td>
                                </tr>
                                <tr v-for="(item, index) in dataDocument" :key="index">
                                    <td class="content-right-middle" style="width: 40px">
                                        <a>
                                            <div class="form-check">
                                                <input
                                                    class="form-check-input checkbox"
                                                    type="checkbox"
                                                    value=""
                                                    :id="item.id"
                                                    @click="countChecks(item.id)"
                                                />
                                            </div>
                                        </a>
                                    </td>
                                    <td class="content-left-middle" style="max-width: 350px">
                                        <truncate-text :item="item" :text="item.name" />
                                    </td>
                                    <td class="content-left-middle" style="max-width: 200px">
                                        <truncate-text :item="resetName(item)" :text="item.description" />
                                    </td>
                                    <td class="content-left-middle" style="width: 160px">
                                        {{ dateFormat(item.created) }}
                                    </td>
                                    <td class="content-left-middle" style="width: 150px">
                                        <span class="badge rounded-pill bg-custom-primary" v-if="item.status == 0">
                                            {{ $t("labelNotAnalyzed") }}
                                        </span>
                                        <span class="badge rounded-pill bg-custom-success" v-if="item.status == 1">
                                            {{ $t("labelAnalyzed") }}
                                        </span>
                                    </td>
                                    <td class="content-left-middle">
                                        <div v-if="item.teams && item.teams.length" class="team-list">
                                            <span v-for="(team, i) in item.teams" :key="i" class="team-badge">
                                                {{ team.name }}
                                            </span>
                                        </div>
                                    </td>
                                    <td class="content-right-middle" style="width: 100px">
                                        <button
                                            class="btn btn-primary btn-sm"
                                            :title="$t('labelAnalyze')"
                                            @click="embeddingData(item.id)"
                                            v-if="item.status == 0"
                                        >
                                            {{ upperFormat($t("labelAnalyze")) }}
                                        </button>
                                        <router-link
                                            class="btn btn-success btn-sm"
                                            :to="{
                                                name: 'Analyzer',
                                                params: { id: item.id },
                                                query: { page: pagination.currentPage },
                                            }"
                                            :title="$t('labelConsult')"
                                            v-if="item.status == 1"
                                        >
                                            {{ upperFormat($t("labelConsult")) }}
                                        </router-link>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
            <div class="row mt-1" v-if="!loading && dataDocument.length > 0">
                <div class="col">
                    <div class="d-inline-block lines">
                        <p class="d-inline">{{ $t("labelLines") }}</p>
                    </div>
                    <div class="d-inline-block" style="margin-left: 1%">
                        <select
                            class="form-select form-select-sm d-inline"
                            v-model="selectedOption"
                            @change="getList({ search: '', page: 1, type: null })"
                        >
                            <option selected>10</option>
                            <option value="25">25</option>
                            <option value="50">50</option>
                            <option value="100">100</option>
                            <option value="0">{{ $t("labelAll") }}</option>
                        </select>
                    </div>
                    <Pagination :paginationData="pagination" :dataList="dataDocument"></Pagination>
                </div>
                <div class="col-auto">
                    <nav>
                        <ul class="pagination justify-content-center">
                            <!-- Chevrons left -->
                            <li class="page-item" v-if="pagination.currentPage != 1">
                                <a
                                    class="page-link"
                                    @click="getList({ search: '', page: pagination.currentPage - 1, type: null })"
                                >
                                    <i class="fas fa-chevron-left"></i>
                                </a>
                            </li>
                            <li class="page-item disabled" v-else>
                                <a class="page-link" tabindex="-1" aria-disabled="true">
                                    <i class="fas fa-chevron-left"></i>
                                </a>
                            </li>
                            <!-- Pages -->
                            <li
                                :class="pagination.currentPage === i ? `page-item active` : `page-item`"
                                v-for="i in pagination.listPage"
                            >
                                <a
                                    class="page-link"
                                    @click="getList({ search: '', page: i, type: null })"
                                    v-if="pagination.currentPage != i"
                                >
                                    {{ i }}
                                </a>
                                <a class="page-link" v-else>{{ i }}</a>
                            </li>
                            <!-- Chevrons right -->
                            <li class="page-item" v-if="pagination.currentPage <= pagination.pageCount - 1">
                                <a
                                    class="page-link"
                                    @click="getList({ search: '', page: pagination.currentPage + 1, type: null })"
                                >
                                    <i class="fas fa-chevron-right"></i>
                                </a>
                            </li>
                            <li class="page-item disabled" v-else>
                                <a class="page-link">
                                    <i class="fas fa-chevron-right"></i>
                                </a>
                            </li>
                        </ul>
                    </nav>
                </div>
            </div>
            <!-- Component ModalAlert -->
            <modal-alert
                v-if="modalAlertShow"
                :type="'Confirm'"
                :entity="modalEntity"
                :alertTitle="$t('labelYouAreAboutToDeleteDocument')"
                :alertMessage="$t('labelThisActionCannotBeUndone')"
                :okLabel="$t('labelConfirm')"
                :cancelLabel="$t('labelCancel')"
                @open="deleteItem"
                @close="closeModal"
            />
            <modal-alert
                v-if="modalWarningShow"
                :type="'Warning'"
                :entity="modalEntity"
                :alertTitle="$t('labelCaution')"
                :alertMessage="$t('labelNotReloadThePage')"
                :okLabel="$t('labelConfirm')"
                @open="confirmWarning"
                @close="closeModalWarning"
            />
            <NormalizeIndex
                :docData="docDataEmbedding"
                :isReprocessing="isReprocessing"
                v-if="showLoading"
            ></NormalizeIndex>
        </div>
    </main>
</template>

<script>
    import date from "@/helpers/date";
    import NavBar from "@/components/common/nav-bar";
    import Breadcrumb from "@/components/common/breadcrumb";
    import SearchBar from "@/components/common/search-bar";
    import ModalAlert from "@/components/common/modal-alert";
    import api from "@/services/api";
    import paginationDivider from "@/utils/paginationDivider";
    import GlobalEventService from "../../../services/globalEventService.js";
    import Pagination from "@/components/common/pagination";
    import NormalizeIndex from "@/components/pages/normalize/loading";
    import TruncateText from "@/components/common/truncate-text.vue";
    import SearchComponent from "@/components/global/SearchComponent.vue";

    export default {
        name: "DocumentList",
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
                sidebarData: "DocumentList",
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
            NavBar,
            Breadcrumb,
            SearchBar,
            ModalAlert,
            Pagination,
            NormalizeIndex,
            TruncateText,
            SearchComponent,
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
                        this.getList({ search: "", page: this.queryPage, type: null });
                    }
                },
            },
        },
        methods: {
            embeddingData: function (id) {
                this.docDataEmbedding.Id = id;
                this.showLoading = true;
            },
            checkAll: function (event) {
                const checkboxes = document.querySelectorAll(".checkbox");
                let checkboxIds = [];
                this.listIds = [];
                checkboxes.forEach((checkbox) => {
                    checkbox.checked = event.target.checked;
                    checkboxIds.push(parseInt(checkbox.id));
                });
                this.countMultipleChecks(checkboxIds);
            },
            countChecks: function (id) {
                let checkBox = document.getElementById(id);
                if (checkBox.checked) {
                    this.listIds.push(id);
                } else {
                    this.listIds = this.listIds.filter((i) => i !== id);
                }
            },
            countMultipleChecks: function (checkboxIds) {
                parseInt(checkboxIds);
                checkboxIds.forEach((id) => {
                    let checkBox = document.getElementById(id);
                    if (checkBox.checked) {
                        this.listIds.push(id);
                    } else {
                        this.listIds = this.listIds.filter((i) => i !== id);
                    }
                });
            },
            setCrumbsData: function () {
                this.crumbsData = [
                    { crumb: this.$t("labelDocuments"), link: { to: "DocumentList" } },
                    { crumb: this.$t("labelListing"), link: { to: "DocumentList", queryPage: this.$route.query.page } },
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
            getList(obj) {
                this.prepareState(obj);

                const teamIds = this.resolveTeamIds();
                if (teamIds.length === 0) {
                    this.handleEmptyTeams();
                    return;
                }

                const queryParams = this.buildQueryParams(obj, teamIds);

                api.get("/Document", { params: queryParams })
                    .then((response) => {
                        this.dataDocument = response.data.content;
                        this.pagination = {
                            currentPage: response.data.currentPage,
                            pageCount: response.data.pageCount,
                            rowCount: response.data.rowCount,
                            listPage: this.divider.calculatePageCount(
                                response.data.pageCount,
                                response.data.currentPage
                            ),
                        };
                        this.loading = false;
                        if (obj.type === "search") this.searching = true;
                    })
                    .catch((e) => {
                        console.log(e);
                        this.loading = false;
                        if (obj.type === "search") this.searching = true;
                    })
                    .finally(() => {
                        console.log("Finished request.");
                    });
            },
            prepareState(obj) {
                this.searchInput = obj.search;
                this.loading = true;
                this.searching = false;
                this.dataDocument = [];
                this.listIds = [];
            },
            resolveTeamIds() {
                if (this.selectedTeamId === 0) {
                    return this.teamList.length > 0 ? this.teamList.map((team) => team.id) : [];
                }
                return [this.selectedTeamId];
            },
            handleEmptyTeams() {
                this.loading = false;
                this.dataDocument = [];
                this.pagination = {
                    currentPage: 1,
                    pageCount: 0,
                    rowCount: 0,
                    listPage: [],
                };
            },
            buildQueryParams(obj, teamIds) {
                return {
                    search: this.searchInput.trim() || "",
                    pageSize: this.selectedOption,
                    page: obj.page,
                    isAscending: this.isAscending,
                    colType: this.colType,
                    teamIds,
                };
            },
            confirmationDialog: function (item) {
                this.modalEntity = item;
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            warningDialog: function () {
                this.modalWarningShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal: function () {
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            closeModalWarning: function () {
                this.modalWarningShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            confirmWarning: function () {
                this.closeModalWarning();
            },
            deleteItem: function () {
                let self = this;
                var paramsReq = {
                    ids: this.listIds,
                };
                api.delete("/Document/Delete", { data: this.listIds })
                    .then(function (response) {
                        // Handle success
                        self.closeModal();
                        self.getList({ search: "", page: 1, type: null });
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
                this.listIds = [];
            },
            dateFormat: function (str) {
                return date.formatDate(str);
            },
            upperFormat: function (str) {
                return str.toUpperCase();
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getList({ search: "", page: this.queryPage, type: null });
            },
            reloadList() {
                if (this.$route.name === "DocumentList") {
                    this.getList({ search: "", page: 1, type: null });
                }
            },
            resetName(item) {
                var itemClone = { ...item };
                itemClone.name = null;
                return itemClone;
            },
            redirectToNewUpload: function (quiz) {
                this.$router.push({ name: "DocumentUpload" });
            },
            filterList(obj) {
                this.searchInput = obj.search;
                this.getList({ search: obj.search, page: this.queryPage, type: null });
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
            if (localStorage.getItem("showToast") === "true") {
                this.warningDialog();
                localStorage.removeItem("showToast");
            }

            await this.loadTeams();
            GlobalEventService.on("all-uploads-complete", this.reloadList);
            GlobalEventService.on("refresh-once", this.reloadList);
            if (this.$store.state.userProfile.keyMongoAccess) {
                this.getList({ search: "", page: this.queryPage, type: null });
            }

            if (this.noTeams) {
                this.selectedTeamId = null;
            }
        },
        mounted() {},
        beforeUnmount() {
            GlobalEventService.off("all-uploads-complete", this.reloadList);
            GlobalEventService.off("refresh-once", this.reloadList);
        },
        unmounted() {},
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
