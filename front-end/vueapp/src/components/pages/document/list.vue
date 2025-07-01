<template>
    <main>
        <!-- Component NavBar -->
        <div class="container-fluid mt-4">
            <div class="custom-padding">
                <div class="row">
                    <!-- Component Breadcrumb -->
                    <breadcrumb :crumbs="crumbsData" />
                </div>
                <!-- Component SearchBar -->
                <search-bar :entity="entitySearch" :resetInput="resetInputSearch" @search="getList" @action="redirectToNewUpload"/>
                <div class="row" v-if="loading">
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <tbody>
                                <tr class="tr-head-1">
                                    <td style="text-align: center;">
                                        <i class="fas fa-sync-alt fa-spin text-secondary"></i>&nbsp;{{ $t('labelLoading') }}..
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="mb-2" style="height: 30px;">
                    <button type="button" class="btn delete-custom d-flex align-items-center" @click="confirmationDialog(item)" v-if="this.listIds.length > 0">
                        <i class="fas fa-trash text-danger" style="font-size: .9em; margin-right: 8px"></i>
                        {{$t('labelDelete')}}
                    </button>
                </div>
                <div class="row" v-if="dataDocument.length === 0 && !loading && (searching || !searching)">
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <tbody>
                                <tr class="tr-head-1">
                                    <td style="text-align: center;">
                                        <i class="fas fa-exclamation-circle text-secondary"></i>&nbsp;{{ $t('labelNoDocumentsWereFound') }}.
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
                                            <input class="form-check-input" type="checkbox" value="" @click="checkAll($event)">
                                        </div>
                                    </td>
                                    <td class="content-left-middle">{{ $t('labelDocumentName') }}</td>
                                    <td class="content-left-middle">{{ $t('labelDescription') }} <i id="1" class="fas fa-sort" @click="orderList(1)" style="cursor: pointer;" :title="$t('labelOrder')"></i></td>
                                    <td class="content-center-middle">{{ $t('labelInclusionDate') }} <i id="2" class="fas fa-sort" @click="orderList(2)" style="cursor: pointer;" :title="$t('labelOrder')"></i></td>
                                    <td class="content-center-middle">{{ $t('labelStatus') }} <i id="3" class="fas fa-sort" @click="orderList(3)" style="cursor: pointer;" :title="$t('labelOrder')"></i></td>
                                    <td class="content-center-middle">{{ $t('labelAction') }}</td>
                                </tr>
                                <tr v-for="(item, index) in dataDocument" :key="index">
                                    <td class="content-center-middle" style="width: 40px;">
                                        <a>
                                            <div class="form-check">
                                                <input class="form-check-input checkbox" type="checkbox" value="" :id="item.id" @click="countChecks(item.id)" />
                                            </div>
                                        </a>
                                    </td>
                                    <td class="content-left-middle" style="max-width: 350px;"> 
                                        <truncate-text :item="item" :text="item.name"/>
                                    </td>
                                    <td class="content-left-middle" style="max-width: 200px;">
                                        <truncate-text :item="resetName(item)" :text="item.description"/>
                                    </td>
                                    <td class="content-center-middle" style="width: 164px;"> {{ dateFormat(item.created) }} </td>
                                    <td class="content-center-middle" style="width: 100px;">
                                        <span class="badge rounded-pill bg-custom-primary" v-if="item.status==0">{{ $t('labelNotAnalyzed') }}</span>
                                        <span class="badge rounded-pill bg-custom-success" v-if="item.status==1">{{ $t('labelAnalyzed') }}</span>
                                    </td>
                                    <td class="content-center-middle" style="width: 100px;">
                                        <button class="btn btn-primary btn-sm" :title="$t('labelAnalyze')"  @click="embeddingData(item.id)" v-if="item.status==0"> {{ upperFormat($t('labelAnalyze')) }} </button>
                                        <router-link class="btn btn-success btn-sm" :to="{ name: 'Analyzer', params: { id: item.id }, query: { page: pagination.currentPage } }" :title="$t('labelConsult')" v-if="item.status==1"> {{ upperFormat($t('labelConsult')) }} </router-link>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="row mt-1" v-if="!loading && dataDocument.length > 0">
                    <div class="col">
                        <div class="d-inline-block lines">
                            <p class="d-inline">{{$t('labelLines')}}</p>
                        </div>
                        <div class="d-inline-block" style="margin-left: 1%">
                            <select class="form-select form-select-sm d-inline" v-model="selectedOption" @change="getList({ search: '', page: 1, type: null })">
                                <option selected>10</option>
                                <option value="25">25</option>
                                <option value="50">50</option>
                                <option value="100">100</option>
                                <option value="0">{{$t('labelAll')}}</option>
                            </select>
                        </div>
                        <Pagination :paginationData="pagination" :dataList="dataDocument"></Pagination>
                    </div>
                    <div class="col-auto">
                        <nav>
                            <ul class="pagination justify-content-center">
                                <!-- Chevrons left -->
                                <li class="page-item" v-if="pagination.currentPage != 1">
                                    <a class="page-link" @click="getList({ search: '', page: pagination.currentPage-1, type: null })">
                                        <i class="fas fa-chevron-left"></i>
                                    </a>
                                </li>
                                <li class="page-item disabled" v-else>
                                    <a class="page-link" tabindex="-1" aria-disabled="true">
                                        <i class="fas fa-chevron-left"></i>
                                    </a>
                                </li>
                                <!-- Pages -->
                                <li :class="pagination.currentPage === i ? `page-item active` : `page-item`" v-for="i in pagination.listPage">
                                    <a class="page-link" @click="getList({ search: '', page: i, type: null })" v-if="pagination.currentPage != i">
                                        {{ i }}
                                    </a>
                                    <a class="page-link" v-else> {{ i }} </a>
                                </li>
                                <!-- Chevrons right -->
                                <li class="page-item" v-if="pagination.currentPage <= pagination.pageCount-1">
                                    <a class="page-link" @click="getList({ search: '', page: pagination.currentPage+1, type: null })">
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
            </div>
        </div>
    </main>
    <!-- Component ModalAlert -->
    <modal-alert v-if="modalAlertShow" :type="'Confirm'" :entity="modalEntity" :alertTitle="$t('labelYouAreAboutToDeleteDocument')" :alertMessage="$t('labelThisActionCannotBeUndone')" :okLabel="$t('labelConfirm')" :cancelLabel="$t('labelCancel')" @open="deleteItem" @close="closeModal" />
    <modal-alert v-if="modalWarningShow" :type="'Warning'" :entity="modalEntity" :alertTitle="$t('labelCaution')" :alertMessage="$t('labelNotReloadThePage')" :okLabel="$t('labelConfirm')" @open="confirmWarning" @close="closeModalWarning" />
    <NormalizeIndex :docData="docDataEmbedding" :isReprocessing="isReprocessing" v-if="showLoading"></NormalizeIndex>
</template>

<script>
    import * as moment from "moment/moment";
    import NavBar from '@/components/common/nav-bar';
    import Breadcrumb from '@/components/common/breadcrumb';
    import SearchBar from '@/components/common/search-bar';
    import ModalAlert from '@/components/common/modal-alert';
    import api from "@/services/api";
    import paginationDivider from "@/utils/paginationDivider";
    import GlobalEventService from '../../../services/globalEventService.js';
    import Pagination from '@/components/common/pagination';
    import NormalizeIndex from '@/components/pages/normalize/loading';
    import TruncateText from "@/components/common/truncate-text.vue";

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
            }
        },
        components: {
            NavBar,
            Breadcrumb,
            SearchBar,
            ModalAlert,
            Pagination,
            NormalizeIndex,
            TruncateText
        },
        watch: {
            searchInput: function (val) {
                this.searching = false;
            },
            '$store.state.userProfile.language': function () {
                this.setCrumbsData();
                this.setEntitySearch();
            },
            '$store.state.userProfile.keyMongoAccess'(newValue) {
                if (newValue) {
                    this.getList({ search: '', page: this.queryPage, type: null });
                }
            }
        },
        methods: {
            embeddingData: function (id) {
                this.docDataEmbedding.Id = id;
                this.showLoading = true;
            },
            checkAll: function (event) {
                const checkboxes = document.querySelectorAll('.checkbox');
                let checkboxIds = [];
                this.listIds = [];
                checkboxes.forEach(checkbox => {
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
                    this.listIds = this.listIds.filter(i => i !== id);
                }
            },
            countMultipleChecks: function (checkboxIds) {
                parseInt(checkboxIds);
                checkboxIds.forEach(id => {
                    let checkBox = document.getElementById(id);
                    if (checkBox.checked) {
                        this.listIds.push(id);
                    } else {
                        this.listIds = this.listIds.filter(i => i !== id);
                    }
                })
            },
            setCrumbsData: function () {
                this.crumbsData = [
                    { crumb: this.$t('labelDocuments'), link: { to: 'DocumentList' } },
                    { crumb: this.$t('labelListing'), link: { to: 'DocumentList', queryPage: this.$route.query.page } },
                ];
            },
            setEntitySearch: function () {
                this.entitySearch = {
                    screen: "document",
                    labelInput: this.$t('labelSearchDocument'),
                    placeholderInput: this.$t('labelDocumentNameOrDescription'),
                    labelButton: "",
                };
            },
            getList: function (obj) { // obj = { search, page, type }
                this.searchInput = obj.search;
                this.loading = true;
                this.searching = false;
                this.dataDocument = [];
                this.listIds = [];
                var paramsReq = {
                    search: this.searchInput.trim() ? this.searchInput.trim() : '',
                    pageSize: this.selectedOption,
                    page: obj.page,
                    isAscending: this.isAscending,
                    colType: this.colType,
                }
                let self = this;
                api.get('/Document', { params: paramsReq })
                    .then(function (response) { // Handle success
                        self.dataDocument = response.data.content;
                        self.pagination = {
                            currentPage: response.data.currentPage,
                            pageCount: response.data.pageCount,
                            rowCount: response.data.rowCount,
                            listPage: self.divider.calculatePageCount(response.data.pageCount, response.data.currentPage)
                        };
                        self.loading = false;
                        if (obj.type === "search") self.searching = true;
                    }).catch(function (e) { // Handle error
                        console.log(e);
                        self.loading = false;
                        if (obj.type === "search") self.searching = true;
                    }).finally(function () { // Always executed
                        console.log("Finished request.");
                    });
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
                    ids: this.listIds
                }
                api.delete('/Document/Delete', { data: this.listIds })
                    .then(function (response) { // Handle success
                        self.closeModal();
                        self.getList({ search: '', page: 1, type: null });
                    }).catch(function (e) { // Handle error
                        console.log(e);
                    }).finally(function () { // Always executed
                        console.log("Finished request.");
                    });
                this.listIds = [];
            },
            dateFormat: function (str) {
                if (this.$store.state.userProfile.language === "en") {
                    return moment(str).format("YYYY/MM/DD");
                } else {
                    return moment(str).format("DD/MM/YYYY");
                }
            },
            upperFormat: function (str) {
                return str.toUpperCase();
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
            reloadList() {
                if (this.$route.name === "DocumentList") {
                    this.getList({ search: '', page: 1, type: null });
                }
            },
            resetName(item){
                var itemClone = {...item};
                itemClone.name = null;
                return itemClone;
            },
             redirectToNewUpload: function (quiz) {
                this.$router.push({ name: 'DocumentUpload' });
            },
            },
        computed: {},
        created() {
            this.setCrumbsData();
            this.setEntitySearch();
            GlobalEventService.on('all-uploads-complete', this.reloadList);
            GlobalEventService.on('refresh-once', this.reloadList);
            if (this.$store.state.userProfile.keyMongoAccess) {
                this.getList({ search: '', page: this.queryPage, type: null });
            };
        },
        mounted() {
            if (localStorage.getItem('showToast') === 'true') {
                this.warningDialog();
                localStorage.removeItem('showToast');
            }
        },
        unmounted() { },
    }
</script>

<style scoped>
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
        background-color: #EDFEF2 !important;
        color: #0EAA42 !important;
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
        max-height: calc(100% - 280px);
        overflow-y: auto;
        min-height: 5%;
    }

    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }
</style>
