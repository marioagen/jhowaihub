<template>
    <button v-if="showMultiDelete" class="btn btn-outline-danger btn-sm mb-2 ms-2" @click="deleteMultipleTypes">
        {{ $t("labelDelete") }}
    </button>
    <div>
        <TableComponent
            modalName="labelTypes"
            emptyMessage="labelNoDocumentTypeWasFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            @selectedRows="selectedRows"
            @change-page="changePage"
        >
            <template #cell-created="{ data }">
                {{ formatDate(data.row.created) }}
            </template>
            <template #cell-actions="{ data }">
                <button class="btn btn-outline-success btn-sm table-btn" @click="editType(data.row)">
                    <LucideIcon icon="SquarePen" />
                </button>
                <button class="btn btn-outline-danger btn-sm ms-2 table-btn" @click="confirmationDialog(data.row)">
                    <LucideIcon icon="Trash2" />
                </button>
            </template>
        </TableComponent>

        <modal-form v-if="modalTypeShow" :dataEditing="selectedType" @openEdit="editTypeRequest" @close="closeModal" />
        <modal-alert
            v-if="modalAlertShow"
            :type="'Confirm'"
            :entity="selectedType"
            :alertTitle="$t('labelYouAreAboutToDeleteDocumentType')"
            :alertMessage="$t('labelThisActionCannotBeUndone')"
            :okLabel="$t('labelConfirm')"
            :cancelLabel="$t('labelCancel')"
            @open="deleteType"
            @close="closeModal"
        />
    </div>
</template>

<script>
    import dates from "@/helpers/Dates";
    import TypesService from "@/services/types/TypesService";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ModalForm from "@/components/pages/type/modal-form";
    import ModalAlert from "@/components/common/modal-alert";
    import ToastAlert from "@/components/common/toast-alert";

    export default {
        name: "TypesTable",
        components: {
            TableComponent,
            ModalForm,
            ModalAlert,
            ToastAlert,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "labelName" },
                    { key: "created", label: "labelInclusionDate" },
                    { key: "emailCreator", label: "labelOwner" },
                    { key: "actions", label: "labelAction" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 100,
                    itemsPerPage: 10,
                    totalItems: 2000,
                },
                selectedRows: [],
            },
            selectedType: {},
            queryPage: 1,
            selectedOption: 10,
            isAscending: false,
            colType: 2,
            modalTypeShow: false,
            modalAlertShow: false,
            toastShow: false,
            toastColor: "",
            toastMessage: "",
            searchInput: "",
        }),
        methods: {
            getTypes(obj) {
                this.table.isLoading = true;
                this.searching = false;
                let params = {
                    search: this.searchInput.trim() ? this.searchInput.trim() : "",
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };

                TypesService.getTypes(params)
                    .then((response) => {
                        const content = response?.content || [];
                        const pagination = response?.pagination || {};

                        this.table.data = content;
                        this.table.pagination = pagination;
                    })
                    .finally(() => {
                        if (obj.type === "search") this.searching = true;
                        this.table.isLoading = false;
                        this.searchInput = "";
                    });
            },
            formatDate(date) {
                return dates.formatDate(date);
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getTypes({ search: "", page: this.queryPage, type: null });
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
            },
            editType(type) {
                this.selectedType = type;
                this.openModalType();
            },
            editTypeRequest(item) {
                const params = {
                    id: item.id,
                    name: item.name,
                };
                TypesService.editType(params)
                    .then((result) => {
                        if (!result.success) {
                            const messageKey =
                                result.status === 409 ? "labelDocumentTypeAlreadyExists" : "labelDocumentTypeError";

                            this.emitToast(this.$t(messageKey), "toast-warning");
                            this.finishEdit();
                            return;
                        }

                        this.emitToast(this.$t("labelDocumentTypeEditSuccess"), "toast-success");
                        this.finishEdit();
                    })
                    .finally(() => {
                        console.log("Finished request.");
                    });
            },
            emitToast(message, color) {
                this.$emit("toast", { message, color });
            },
            finishEdit() {
                this.closeModal();
                this.getTypes({ search: "", page: 1, type: null });
            },
            deleteMultipleTypes() {
                const typeIds = this.table.selectedRows.map((item) => item.id);
                this.deleteType(typeIds);
            },
            deleteType(typeIds) {
                const idsToDelete = typeIds || [this.selectedTeam.id];
                TypesService.deleteTypeById(idsToDelete)
                    .then((success) => {
                        if (success) {
                            this.closeModal();
                            this.getTypes({ search: "", page: 1, type: null });
                            this.emitToast(this.$t("labelDocumentTypeRemoveSuccess"), "toast-success");
                        } else {
                            this.emitToast(this.$t("labelDocumentTypeRemoveError"), "toast-warning");
                        }
                    })
                    .finally(() => {
                        this.listIds = [];
                        this.table.selectedRows = [];
                    });
            },
            filterList(input) {
                this.searchInput = input;
                this.getTypes({ search: input, page: this.queryPage, type: null });
            },
            openModalType: function () {
                this.modalTypeShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModalType: function () {
                this.modalTypeShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            confirmationDialog(team) {
                this.selectedTeam = team;
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal() {
                this.modalAlertShow = false;
                this.modalTypeShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            changePage(page) {
                this.getTypes({ search: "", page: page, type: null });
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
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTypes({ search: "", page: this.queryPage, type: null });
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>
