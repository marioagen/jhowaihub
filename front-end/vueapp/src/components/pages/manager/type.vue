<template>
    <main>
        <div class="container-fluid mt-4">
            <div class="custom-padding">
                <div class="row">
                    <!-- Component Breadcrumb -->
                    <breadcrumb :crumbs="crumbsData" />
                </div>
                <!-- Component SearchBar -->
                <search-bar :entity="entitySearch" :resetInput="resetInputSearch" @search="getList" @action="addType" />
                <div class="row" v-if="loading">
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <tbody>
                                <tr class="tr-head-1">
                                    <td class="text-secondary" style="text-align: center">
                                        <i class="fas fa-sync-alt fa-spin text-secondary"></i>
                                        &nbsp;{{ $t("labelLoading") }}..
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="mb-2" style="height: 30px">
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
                <div class="row" v-if="dataType.length === 0 && !loading">
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <tbody>
                                <tr class="tr-head-1">
                                    <td style="text-align: center">
                                        <i class="fas fa-exclamation-circle text-secondary"></i>
                                        &nbsp;{{ $t("labelNoDocumentTypeWasFound") }}.
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="row scroll-area" v-if="!loading && dataType.length > 0">
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
                                    <td class="content-center-middle">
                                        ID
                                        <i
                                            id="2"
                                            class="fas fa-sort"
                                            @click="orderList(2)"
                                            style="cursor: pointer"
                                            :title="$t('labelOrder')"
                                        ></i>
                                    </td>
                                    <td class="content-left-middle">
                                        {{ $t("labelName") }}
                                        <i
                                            id="1"
                                            class="fas fa-sort"
                                            @click="orderList(1)"
                                            style="cursor: pointer"
                                            :title="$t('labelOrder')"
                                        ></i>
                                    </td>
                                    <td class="content-center-middle">
                                        {{ $t("labelInclusionDate") }}
                                        <i
                                            id="2"
                                            class="fas fa-sort"
                                            @click="orderList(2)"
                                            style="cursor: pointer"
                                            :title="$t('labelOrder')"
                                        ></i>
                                    </td>
                                    <td class="content-center-middle">
                                        {{ $t("labelOwner") }}
                                        <i
                                            id="3"
                                            class="fas fa-sort"
                                            @click="orderList(3)"
                                            style="cursor: pointer"
                                            :title="$t('labelOrder')"
                                        ></i>
                                    </td>
                                    <td class="content-center-middle">{{ $t("labelAction") }}</td>
                                </tr>
                                <tr v-for="(item, index) in dataType" :key="index">
                                    <td class="content-center-middle" style="width: 10px">
                                        <a>
                                            <div class="form-check">
                                                <input
                                                    class="form-check-input checkbox"
                                                    type="checkbox"
                                                    value=""
                                                    :id="item.id"
                                                    @click="countChecks(item.id)"
                                                    v-if="item.emailCreator === $store.state.userProfile.login"
                                                />
                                            </div>
                                        </a>
                                    </td>
                                    <td class="content-center-middle" style="width: 85px">{{ item.id }}</td>
                                    <td class="content-left-middle">
                                        <truncate-text :item="item" :text="item.name" />
                                    </td>
                                    <td class="content-center-middle" style="width: 164px">
                                        {{ dateFormat(item.created) }}
                                    </td>
                                    <td class="content-center-middle" style="width: 164px; font-size: 14px">
                                        {{ item.emailCreator }}
                                    </td>
                                    <td
                                        class="content-right-middle"
                                        style="width: 95px"
                                        v-if="item.emailCreator === $store.state.userProfile.login"
                                    >
                                        <a
                                            class="btn btn-success btn-sm"
                                            :title="$t('labelEdit')"
                                            @click="openModal(item)"
                                        >
                                            <i class="fas fa-pen" style="font-size: 0.75em"></i>
                                        </a>
                                    </td>
                                    <td class="content-right-middle" style="width: 51px" v-else>
                                        <a class="btn btn-secondary btn-sm" :title="$t('labelNotAllowed')">
                                            <i class="fas fa-pen" style="font-size: 0.75em"></i>
                                        </a>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="row mt-1" v-if="!loading && dataType.length > 0">
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
                        <Pagination :paginationData="pagination" :dataList="dataType"></Pagination>
                    </div>
                    <div class="col-auto">
                        <nav aria-label="Page navigation example">
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
            </div>
        </div>
        <!-- Component ToastAlert -->
        <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
    </main>
    <!-- Component ModalForm -->
    <modal-form
        v-if="showModalForm"
        :dataEditing="dataModal"
        @openAdd="addType"
        @openEdit="editType"
        @close="closeModal"
    />
    <!-- Component ModalAlert -->
    <modal-alert
        v-if="modalAlertShow"
        :type="'Confirm'"
        :entity="modalEntity"
        :alertTitle="$t('labelYouAreAboutToDeleteDocumentType')"
        :alertMessage="$t('labelThisActionCannotBeUndone')"
        :okLabel="$t('labelConfirm')"
        :cancelLabel="$t('labelCancel')"
        @open="deleteItem"
        @close="closeModal"
    />
</template>

<script>
    import * as moment from "moment/moment";
    import NavBar from "@/components/common/nav-bar";
    import Breadcrumb from "@/components/common/breadcrumb";
    import SearchBar from "@/components/common/search-bar";
    import ModalForm from "@/components/pages/type/modal-form";
    import ModalAlert from "@/components/common/modal-alert";
    import ToastAlert from "@/components/common/toast-alert";
    import api from "@/services/api";
    import store from "../../../store";
    import paginationDivider from "@/utils/paginationDivider";
    import Pagination from "@/components/common/pagination";
    import TruncateText from "@/components/common/truncate-text.vue";

    export default {
        name: "TypeManager",
        emits: ["showAlertToast"],
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
                sidebarData: "Type",
                queryPage: this.$route.query.page ? this.$route.query.page : 1,
                showModalForm: false,
                searchInput: "",
                searching: false,
                dataType: [],
                loading: false,
                pagination: { currentPage: 0, pageCount: 0, rowCount: 0, listPage: 0 },
                modalAlertShow: false,
                modalEntity: {},
                isAscending: false,
                dataModal: {},
                colType: 2,
                selectedOption: 10,
                toastShow: false,
                toastColor: "",
                toastMessage: "",
                divider: new paginationDivider(),
                listIds: [],
            };
        },
        components: {
            NavBar,
            Breadcrumb,
            SearchBar,
            ModalForm,
            ModalAlert,
            ToastAlert,
            Pagination,
            TruncateText,
        },
        watch: {
            searchInput: function (val) {
                this.searching = false;
            },
            "$store.state.userProfile.language": function () {
                this.setCrumbsData();
                this.setEntitySearch();
            },
        },
        methods: {
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
                let checkBox = document.querySelector(`input[type="checkbox"][id="${id}"]`);
                if (checkBox && checkBox.checked) {
                    this.listIds.push(id);
                } else {
                    this.listIds = this.listIds.filter((i) => i !== id);
                }
            },
            countMultipleChecks: function (checkboxIds) {
                parseInt(checkboxIds);
                checkboxIds.forEach((id) => {
                    let checkBox = document.querySelector(`input[type="checkbox"][id="${id}"]`);
                    if (checkBox && checkBox.checked) {
                        this.listIds.push(id);
                    } else {
                        this.listIds = this.listIds.filter((i) => i !== id);
                    }
                });
            },
            setCrumbsData: function () {
                this.crumbsData = [
                    { crumb: this.$t("labelManage"), link: { to: "Type" } },
                    { crumb: this.$t("labelTypes"), link: { to: "Type", queryPage: this.$route.query.page } },
                ];
            },
            setEntitySearch: function () {
                this.entitySearch = {
                    screen: "type",
                    labelInput: this.$t("labelSearchTypes"),
                    placeholderInput: this.$t("labelTypeNameOrId"),
                    labelButton: this.$t("labelNewType"),
                };
            },
            getList: function (obj) {
                // obj = { search, page, type }
                this.listIds = [];
                this.searchInput = obj.search;
                this.loading = true;
                this.searching = false;
                this.dataType = [];
                var paramsReq = {
                    search: this.searchInput.trim() ? this.searchInput.trim() : "",
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };
                let self = this;
                api.get("/TypeDoc/Paged/", { params: paramsReq })
                    .then(function (response) {
                        // Handle success
                        self.dataType = response.data.content;
                        self.pagination = {
                            currentPage: response.data.currentPage,
                            pageCount: response.data.pageCount,
                            rowCount: response.data.rowCount,
                            listPage: self.divider.calculatePageCount(
                                response.data.pageCount,
                                response.data.currentPage
                            ),
                        };
                        self.loading = false;
                        if (obj.type === "search") self.searching = true;
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                        self.loading = false;
                        if (obj.type === "search") self.searching = true;
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            addType: function (name) {
                let self = this;
                api.post("/TypeDoc?name=" + name)
                    .then(function (response) {
                        // Handle success
                        self.closeModal();
                        self.resetInputSearch = !self.resetInputSearch;
                        self.getList({ search: "", page: 1, type: null });
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                        self.closeModal();
                        if (e.response.status == 409) {
                            self.alertToast(self.$t("labelTypeDocAlreadyExists"), "toast-warning");
                        } else {
                            self.alertToast(self.$t("labelTypeDocError"), "toast-warning");
                        }
                        console.log(e);
                        self.closeModal();
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            editType: function (item) {
                let self = this;
                var paramsReq = {
                    id: item.id,
                    name: item.name,
                };
                api.put("/TypeDoc", paramsReq)
                    .then(function (response) {
                        // Handle success
                        self.closeModal();
                        self.getList({ search: "", page: 1, type: null });
                    })
                    .catch(function (e) {
                        // Handle error
                        if (e.response.status == 409) {
                            self.alertToast(self.$t("labelDocumentTypeAlreadyExists"), "toast-warning");
                        } else {
                            console.log(e);
                            self.alertToast(self.$t("labelDocumentTypeError"), "toast-warning");
                            self.closeModal();
                        }
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            confirmationDialog: function (item) {
                this.modalEntity = item;
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            deleteItem: function () {
                let self = this;
                api.delete("/TypeDoc/DeleteByIds", { data: this.listIds })
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
            },
            openModal: function (data = null) {
                this.showModalForm = true;
                if (data) this.dataModal = data;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal: function () {
                this.showModalForm = false;
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
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
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getList({ search: "", page: this.queryPage, type: null });
            },
            alertToast: function (msg, color) {
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;
                let self = this;
                this.myInterval = setInterval(function () {
                    self.toastMessage = "";
                    self.toastColor = "";
                    self.toastShow = false;
                    clearInterval(self.myInterval);
                }, 4000);
            },
            closeToast: function () {
                this.toastShow = false;
                this.clearMyInterval();
            },
            clearMyInterval: function () {
                clearInterval(this.myInterval);
                this.myInterval = null;
            },
        },
        computed: {},
        created() {
            this.setCrumbsData();
            this.setEntitySearch();
            this.getList({ search: "", page: this.queryPage, type: null });
        },
        mounted() {},
        unmounted() {},
    };
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
        max-width: 200px;
    }

    .content-right-middle {
        text-align: right;
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
