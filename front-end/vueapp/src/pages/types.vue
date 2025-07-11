<template>
    <main>
        <div class="container-fluid mt-4">
            <div class="custom-padding">
                <div class="row">
                    <breadcrumb :crumbs="crumbsData" />
                </div>
                <search-bar :entity="entitySearch" :resetInput="resetInputSearch" @search="filterList" @action="addType" />
                <div class="mb-2" style="height: 30px;">
                    <button type="button" class="btn delete-custom d-flex align-items-center" @click="confirmationDialog(item)" v-if="this.listIds.length > 0">
                        <i class="fas fa-trash text-danger" style="font-size: .9em; margin-right: 8px"></i>
                        {{$t('labelDelete')}}
                    </button>
                </div>
                <TypesTable 
                    ref="TypesTable"
                    @toast="handleToast"
                />
                </div>
            </div>
    </main>
        <modal-form 
            v-if="modalAlertShow"
            :dataEditing="dataModal" 
            @openAdd="addType" 
            @close="closeModal" 
        />

        <toast-alert 
            :showToast="toastShow" 
            :colorToast="toastColor" 
            :messageToast="toastMessage" 
            @close="closeToast" 
        />
</template>

<script>
    import Breadcrumb from '@/components/common/breadcrumb';
    import SearchBar from '@/components/common/search-bar';
    import ModalForm from '@/components/pages/type/modal-form';
    import ModalAlert from '@/components/common/modal-alert';
    import ToastAlert from '@/components/common/toast-alert';
    import paginationDivider from "@/utils/paginationDivider";
    import Pagination from '@/components/common/pagination';
    import TypesTable from "@/components/types/types-table.vue";
    import TypesService from "@/services/types/TypesService";

    export default {
        name: "TypeManager",
        emits: ['showAlertToast'],
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
                sidebarData: "Type",
                queryPage: this.$route.query.page ? this.$route.query.page : 1,
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
            }
        },
        components: {
            Breadcrumb,
            SearchBar,
            ModalForm,
            ModalAlert,
            ToastAlert,
            Pagination,
            TypesTable
        },
        watch: {
            searchInput: function (val) {
                this.searching = false;
            },
            '$store.state.userProfile.language': function () {
                this.setCrumbsData();
                this.setEntitySearch();
            },
        },
        methods: {
            setCrumbsData: function () {
                this.crumbsData = [
                    { crumb: this.$t('labelManage'), link: { to: 'Type' } },
                    { crumb: this.$t('labelTypes'), link: { to: 'Type' } },
                ];
            },
            setEntitySearch: function () {
                this.entitySearch = {
                    screen: "type",
                    labelInput: this.$t('labelSearchTypes'),
                    placeholderInput: this.$t('labelTypeNameOrId'),
                    labelButton: this.$t('labelNewType'),
                };
            },
            filterList(obj) {
                this.$refs.TypesTable.filterList(obj.search);
            },
            addType: function (name) {
                const self = this;
                TypesService.addType(name)
                    .then((result) => {
                        if (!result.success) {
                            const messageKey = result.status === 409
                                ? 'labelDocumentTypeAlreadyExists'
                                : 'labelDocumentTypeError'

                            this.alertToast(this.$t(messageKey), 'toast-warning')
                        }
                        else {
                            self.$refs.TypesTable.getTypes({ search: '', page: self.queryPage, type: null })
                            self.resetInputSearch = !self.resetInputSearch;
                            self.alertToast(self.$t('labelDocumentTypeSuccess'), "toast-success");
                        }
                    })
                    .finally(() => {
                        console.log('Finished request.')
                    })                   
            },
            confirmationDialog: function (item) {
                this.modalEntity = item;
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            openModal: function (data = null) {
                this.modalAlertShow = true;
                if (data) this.dataModal = data;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal: function () {
                this.modalAlertShow = false;
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            upperFormat: function (str) {
                return str.toUpperCase();
            },
            alertToast(msg, color) {
                this.clearMyInterval(); 
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;

                this.myInterval = setTimeout(() => {
                    this.toastMessage = '';
                    this.toastColor = '';
                    this.toastShow = false;
                    this.myInterval = null;
                }, 4000);
            },
            closeToast: function () {
                this.toastShow = false;
                this.clearMyInterval();
            },
            clearMyInterval() {
                if (this.myInterval) {
                    clearTimeout(this.myInterval);
                    this.myInterval = null;
                }
            },
            handleToast({ message, color }) {
                this.alertToast(message, color);
            }
        },
        computed: {},
        created() {
            this.setCrumbsData();
            this.setEntitySearch();
        },
        mounted() { },
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
        background-color: #EDFEF2 !important;
        color: #0EAA42 !important;
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
