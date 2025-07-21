<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("labelTypes") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("labelTypesMessage") }}</small>
                        </p>
                    </div>
                    <button 
                        class="btn btn-primary btn-sm" 
                        @click="openModalType"
                    >
                        <LucideIcon icon="Plus" size="17" />
                        {{ $t("labelNewType") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <SearchComponent 
                            :entity="entitySearch" 
                            :resetInput="resetInputSearch" 
                            @search="filterList" 
                        />
                    </div>
                </div>

                <TypesTable ref="TypesTable" @toast="handleToast" />
            </div>

            <modal-form v-if="modalAlertShow" :dataEditing="dataModal" @openAdd="addType" @close="closeModal" />
            <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
        </div>
    </main>    
</template>

<script>
    import ModalForm from "@/components/pages/type/modal-form";
    import ToastAlert from "@/components/common/toast-alert";
    import paginationDivider from "@/utils/paginationDivider";
    import TypesTable from "@/components/types/TypesTable.vue";
    import TypesService from "@/services/types/TypesService";
    import SearchComponent from "@/components/global/SearchComponent.vue";

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
            ModalForm,
            ToastAlert,
            TypesTable,
            SearchComponent,
        },
        watch: {
            searchInput: function (val) {
                this.searching = false;
            },
            "$store.state.userProfile.language": function () {
                this.setEntitySearch();
            },
        },
        methods: {
            setEntitySearch () {
                this.entitySearch = {
                    screen: "type",
                    labelInput: this.$t("labelSearchTypes"),
                    placeholderInput: this.$t("labelTypeNameOrId"),
                    labelButton: this.$t("labelNewType"),
                };
            },
            filterList(obj) {
                this.$refs.TypesTable.filterList(obj.search);
            },
            openModalType() {

            },
            addType: function (name) {
                const self = this;
                TypesService.addType(name)
                    .then((result) => {
                        if (!result.success) {
                            const messageKey =
                                result.status === 409 ? "labelDocumentTypeAlreadyExists" : "labelDocumentTypeError";

                            this.alertToast(this.$t(messageKey), "toast-warning");
                        } else {
                            self.$refs.TypesTable.getTypes({ search: "", page: self.queryPage, type: null });
                            self.resetInputSearch = !self.resetInputSearch;
                            self.alertToast(self.$t("labelDocumentTypeSuccess"), "toast-success");
                        }
                    })
                    .finally(() => {
                        console.log("Finished request.");
                    });
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
                    this.toastMessage = "";
                    this.toastColor = "";
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
            },
        },
        created() {
            this.setEntitySearch();
        },
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
        overflow-y: auto;
    }

    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }
</style>